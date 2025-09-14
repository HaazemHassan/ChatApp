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



@Component({
  selector: 'app-conversation-window',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './conversation-window.component.html',
  styleUrl: './conversation-window.component.css',
})
export class ConversationWindowComponent implements OnInit, AfterViewChecked {
  @ViewChild('messagesList', { static: false }) messagesList!: ElementRef;
  loading = false;
  error: string | null = null;
  private shouldScrollToBottom = false;
  currentUserId: number | null = null;

  conversation = input.required<UserConversation>();
  newMessage = input<MessageResponse | null>(null);
  messages: MessageResponse[] = [];
  messageText: string = '';


  constructor(
    private conversationsService: ConversationsService,
    private chatHubService: ChatHubService,
    private authService: AuthenticationService
  ) {
    effect(() => {
      if (this.conversation().id)
        this.loadMessages();
      else
        this.messages = [];
    });

    effect(() => {
      if (this.newMessage() !== null) {
        this.messages = [...this.messages, this.newMessage()!];
        this.scrollToBottom(true);
      }
    });
  }

  ngOnInit(): void {
    this.currentUserId = this.authService.getCurrentUserId();

  }



  isCurrentUserMessage(message: MessageResponse): boolean {
    return message.senderId === this.currentUserId;
  }

  formatMessageTime(sentAt: string): string {
    const date = new Date(sentAt);
    return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
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
      console.log('Message sent successfully via SignalR');

    }).catch((err) => {
      console.error('Error sending message via SignalR:', err);
      this.error = 'Failed to send message. Please try again.';
      this.messageText = originalText;
    });
  }

  //helpers
  private loadMessages(): void {

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
        },
        error: (err) => {
          this.error = 'Failed to load messages';
          this.loading = false;
          console.error('Error loading messages:', err);
        },
      });
  }

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


}
