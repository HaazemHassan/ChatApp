import { DeliveryStatus } from './../../../enums/delivery-status';
import { SendMessageRequest } from './../../../models/conversations/requests/send-message-request';
import {
  Participant,
  UserConversation,
} from './../../../models/conversations/responses/user-conversations-response';
import {
  Component,
  OnInit,
  ViewChild,
  ElementRef,
  AfterViewChecked,
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
export class ConversationWindowComponent implements OnInit, AfterViewChecked {
  @ViewChild('messagesList', { static: false }) messagesList!: ElementRef;
  loading = false;
  error: string | null = null;
  private shouldScrollToBottom = false;
  currentUserId: number | null = null;
  prevConversationId: number | null = null;

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
    private authService: AuthenticationService
  ) {

    effect(() => {
      const id = this.conversation().id;
      if (id && id !== this.prevConversationId) {
        this.prevConversationId = id;
        this.loadMessages();
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


  private loadMessages(): void {
    console.log('Loading messages for conversation:', this.conversation());

    if (!this.conversation() || !this.conversation().id) return;

    this.loading = true;
    this.error = null;

    this.conversationsService
      .getConversationMessages(this.conversation().id!)
      .subscribe({
        next: (response: ConversationMessagesResponse) => {
          this.messages = response.messages || [];
          this.loading = false;
          this.shouldScrollToBottom = true;
          this.groupMessagesByDate();

          const othersMessages = this.messages.filter(m => m.senderId !== this.currentUserId && m.deliveryStatus !== DeliveryStatus.Read);
          console.log('Others messages needing read update:', othersMessages);
          if (othersMessages.length !== 0) {
            this.chatHubService.NotifyMessagesRead(othersMessages.map(m => m.id)).catch(err => {
              console.error('Error notifying messages read:', err);
            });
          }

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
      console.error('Error sending message via SignalR:', err);
      this.error = 'Failed to send message. Please try again.';
      this.messageText = originalText;
    });
  }

  isOnline = computed(() => {
    console.log('Checking online status for conversation:', this.conversation());
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
      const messageDate = new Date(message.sentAt);
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
    const messageDate = new Date(message.sentAt);
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
    const messageDate = new Date(updatedMessage.sentAt);
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

}
