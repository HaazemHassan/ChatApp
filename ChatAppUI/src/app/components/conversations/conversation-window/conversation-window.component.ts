import { DeliveryStatus } from './../../../enums/delivery-status';
import { SendMessageRequest } from './../../../models/conversations/requests/send-message-request';
import {
  Participant,
  UserConversation,
} from './../../../models/conversations/responses/user-conversations-response';
import {
  Component,
  OnInit,
  OnDestroy,
  AfterViewInit,
  ViewChild,
  ElementRef,
  AfterViewChecked,
  Renderer2,
  input,
  effect,
  output,
  computed,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ConversationsService } from '../../../services/conversations.service';
import { ChatHubService } from '../../../services/chat-hub.service';
import { AuthenticationService } from '../../../services/authentication.service';
import {
  MessageResponse,
  ConversationMessagesResponse,
} from '../../../models/conversations/responses/conversation-messages-response';
import { FormatMessageTimePipe } from "../../../pipes/format-message-time.pipe";
import { StringInitialsPipe } from "../../../pipes/string-initials.pipe";
import { MessageType } from "../../../enums/message-type";
import { ConversationType } from '../../../enums/conversation-type';



@Component({
  selector: 'app-conversation-window',
  standalone: true,
  imports: [CommonModule, FormsModule, FormatMessageTimePipe, StringInitialsPipe],
  templateUrl: './conversation-window.component.html',
  styleUrl: './conversation-window.component.css',
})
export class ConversationWindowComponent implements OnInit, OnDestroy, AfterViewInit, AfterViewChecked {
  @ViewChild('messagesList', { static: false }) messagesList!: ElementRef;
  loading = false;
  loadingMore = false;
  error: string | null = null;
  private shouldScrollToBottom = false;
  currentUserId: number | null = null;
  prevConversationId: number | null = null;
  private scrollUnlistener?: () => void;
  private isScrollListenerAttached = false;

  // Pagination
  currentPage = 1;
  pageSize = 20;
  hasMore = false;
  totalCount = 0;

  MessageType = MessageType;
  ConversationType = ConversationType;
  DeliveryStatus = DeliveryStatus;

  conversation = input.required<UserConversation>();
  newMessage = input<MessageResponse | null>(null);
  messages: MessageResponse[] = [];
  messageText: string = '';
  groupedMessages: { date: string; messages: MessageResponse[] }[] = [];


  constructor(
    private conversationsService: ConversationsService,
    private chatHubService: ChatHubService,
    private authService: AuthenticationService,
    private renderer: Renderer2
  ) {

    effect(() => {
      const id = this.conversation().id;
      if (id && id !== this.prevConversationId) {
        this.prevConversationId = id;
        this.resetPaginationState();
        this.detachScrollListener();
        this.attachScrollListener();
        this.loadInitialMessages();
      }
    });

    effect(() => {
      const newMsg = this.newMessage();
      if (newMsg !== null) {
        const existingMessageIndex = this.messages.findIndex(m => m.id === newMsg.id);

        if (existingMessageIndex !== -1) {
          this.messages = this.messages.map((message, index) =>
            index === existingMessageIndex ? newMsg : message
          );
          // Update the message in the grouped messages as well
          this.updateMessageInGroup(newMsg);
        } else {
          this.messages = [...this.messages, newMsg];
          // Add the new message to the appropriate date group
          this.addMessageToGroup(newMsg);
        }
        this.scrollToBottom(true);
      }
    });

  }



  ngOnInit(): void {
    this.currentUserId = this.authService.getCurrentUserId();

    this.chatHubService.onMessagesDelivered((messageIds: number[]) => {
      this.updateMessagesDeliveryStatus(messageIds, DeliveryStatus.Delivered);
    });

    this.chatHubService.onMessagesRead((messageIds: number[]) => {
      this.updateMessagesDeliveryStatus(messageIds, DeliveryStatus.Read);
    });
  }

  ngAfterViewInit(): void {
    this.attachScrollListener();
  }

  ngOnDestroy(): void {
    this.detachScrollListener();
  }

  private attachScrollListener(): void {
    setTimeout(() => {
      if (this.messagesList?.nativeElement && !this.isScrollListenerAttached) {
        this.scrollUnlistener = this.renderer.listen(
          this.messagesList.nativeElement,
          'scroll',
          this.onScroll.bind(this)
        );
        this.isScrollListenerAttached = true;
      }
    }, 200);
  }

  private detachScrollListener(): void {
    if (this.scrollUnlistener) {
      this.scrollUnlistener();
      this.scrollUnlistener = undefined;
    }
    this.isScrollListenerAttached = false;
  }

  private onScroll(): void {
    const scrollContainer = this.messagesList?.nativeElement;
    if (!scrollContainer) {
      return;
    }

    const scrollTop = scrollContainer.scrollTop;
    const threshold = 50;

    if (scrollTop <= threshold && this.hasMore && !this.loadingMore && !this.loading)
      this.loadMoreMessages();

  }


  private loadInitialMessages(): void {
    if (!this.conversation() || !this.conversation().id) return;

    this.loading = true;
    this.error = null;

    this.conversationsService
      .getConversationMessages(this.conversation().id!, this.currentPage, this.pageSize)
      .subscribe({
        next: (response: ConversationMessagesResponse) => {
          this.messages = (response.messages || []).reverse();
          this.hasMore = response.hasMore;
          this.totalCount = response.totalCount;
          this.currentPage = response.pageNumber;
          this.loading = false;
          this.shouldScrollToBottom = true;
          this.groupMessagesByDate();

          this.chatHubService.MarkConversationAsRead(this.conversation().id!).catch(err => {
            console.error('Error marking conversation as read:', err);
          });

        },
        error: (err) => {
          this.error = 'Failed to load messages';
          this.loading = false;
          console.error('Error loading messages:', err);
        },
      });
  }


  ngAfterViewChecked(): void {
    if (this.shouldScrollToBottom && this.messagesList) {
      this.scrollToBottom();
      this.shouldScrollToBottom = false;
    }
  }

  sendMessage(): void {
    if (!this.messageText.trim() || !this.conversation()
      || this.currentUserId === null) return;

    const originalText = this.messageText;
    this.messageText = '';
    this.error = null;

    const request: SendMessageRequest = {
      conversationId: this.conversation().id!,
      content: originalText.trim()
    };

    this.chatHubService.sendMessage(request).then(() => {
    }).catch((err) => {
      this.error = 'Failed to send message. Please try again.';
      this.messageText = originalText;
    });
  }

  isOnline = computed(() => {
    if (this.conversation().type === ConversationType.Direct)
      return this.conversationsService.IsOtherParticipantOnline(this.conversation().participants);
    return null;   // For group conversations, online status is not applicable
  });


  //helpers
  private scrollToBottom(smooth: boolean = false): void {
    try {
      if (this.messagesList?.nativeElement) {
        const element = this.messagesList.nativeElement;

        requestAnimationFrame(() => {
          element.scrollTo({
            top: element.scrollHeight,
            behavior: smooth ? 'smooth' : 'auto'
          });
        });
      }
    } catch (err) {
      console.error('Error scrolling to bottom:', err);
    }
  }

  private updateMessagesDeliveryStatus(messageIds: number[], deliveryStatus: DeliveryStatus): void {
    this.messages = this.messages.map(message => {
      if (messageIds.includes(message.id) && message.senderId === this.currentUserId) {
        const updatedMessage = {
          ...message,
          deliveryStatus: deliveryStatus
        };
        // Update the message in grouped messages as well
        this.updateMessageInGroup(updatedMessage);
        return updatedMessage;
      }
      return message;
    });
  }

  //Functions to group messages by date
  private groupMessagesByDate(): void {
    const grouped = new Map<string, MessageResponse[]>();

    this.messages.forEach(message => {
      // Convert UTC time to local time
      const dateString = message.sentAt.endsWith('Z') ? message.sentAt : message.sentAt + 'Z';
      const messageDate = new Date(dateString);
      const dateKey = this.getDateKey(messageDate);

      if (!grouped.has(dateKey)) {
        grouped.set(dateKey, []);
      }
      grouped.get(dateKey)!.push(message);
    });

    this.groupedMessages = Array.from(grouped.entries()).map(([date, messages]) => ({
      date,
      messages
    }));
  }

  private addMessageToGroup(message: MessageResponse): void {
    // Convert UTC time to local time
    const dateString = message.sentAt.endsWith('Z') ? message.sentAt : message.sentAt + 'Z';
    const messageDate = new Date(dateString);
    const dateKey = this.getDateKey(messageDate);
    const existingGroup = this.groupedMessages.find(g => g.date === dateKey);

    if (existingGroup) {
      existingGroup.messages = [...existingGroup.messages, message];
    } else {
      this.groupedMessages = [...this.groupedMessages, {
        date: dateKey,
        messages: [message]
      }];
    }
  }

  private updateMessageInGroup(updatedMessage: MessageResponse): void {
    // Convert UTC time to local time
    const dateString = updatedMessage.sentAt.endsWith('Z') ? updatedMessage.sentAt : updatedMessage.sentAt + 'Z';
    const messageDate = new Date(dateString);
    const dateKey = this.getDateKey(messageDate);

    const group = this.groupedMessages.find(g => g.date === dateKey);
    if (group) {
      group.messages = group.messages.map(msg =>
        msg.id === updatedMessage.id ? updatedMessage : msg
      );
    }
  }

  private getDateKey(date: Date): string {
    const today = new Date();
    const yesterday = new Date(today);
    yesterday.setDate(yesterday.getDate() - 1);

    // Reset time for comparison
    const dateOnly = new Date(date.getFullYear(), date.getMonth(), date.getDate());
    const todayOnly = new Date(today.getFullYear(), today.getMonth(), today.getDate());
    const yesterdayOnly = new Date(yesterday.getFullYear(), yesterday.getMonth(), yesterday.getDate());

    if (dateOnly.getTime() === todayOnly.getTime()) {
      return 'Today';
    } else if (dateOnly.getTime() === yesterdayOnly.getTime()) {
      return 'Yesterday';
    } else {
      // Format as "DD/MM/YYYY"
      const day = date.getDate().toString().padStart(2, '0');
      const month = (date.getMonth() + 1).toString().padStart(2, '0');
      const year = date.getFullYear();
      return `${day}/${month}/${year}`;
    }
  }

  loadMoreMessages(): void {
    if (!this.hasMore || this.loadingMore || !this.conversation().id) return;

    this.loadingMore = true;
    const nextPage = this.currentPage + 1;

    const scrollContainer = this.messagesList?.nativeElement;
    if (!scrollContainer) {
      this.loadingMore = false;
      return;
    }

    const oldScrollHeight = scrollContainer.scrollHeight;
    const oldScrollTop = scrollContainer.scrollTop;

    this.conversationsService
      .getConversationMessages(this.conversation().id!, nextPage, this.pageSize)
      .subscribe({
        next: (response: ConversationMessagesResponse) => {
          // Reverse older messages and prepend them to the beginning
          const olderMessages = (response.messages || []).reverse();
          this.messages = [...olderMessages, ...this.messages];
          this.hasMore = response.hasMore;
          this.totalCount = response.totalCount;
          this.currentPage = response.pageNumber;
          this.loadingMore = false;

          // Re-group all messages
          this.groupMessagesByDate();


          requestAnimationFrame(() => {
            requestAnimationFrame(() => {
              const newScrollHeight = scrollContainer.scrollHeight;
              const heightDifference = newScrollHeight - oldScrollHeight;
              scrollContainer.scrollTop = oldScrollTop + heightDifference;
            });
          });
        },
        error: (err) => {
          this.loadingMore = false;
          console.error('Error loading more messages:', err);
        },
      });
  }

  private resetPaginationState(): void {
    this.currentPage = 1;
    this.hasMore = false;
    this.totalCount = 0;
    this.messages = [];
    this.groupedMessages = [];
  }

}
