import { catchError } from 'rxjs';
import { ApiResponse } from './../../models/api-response';
import { Participant, UserConversation } from './../../models/conversations/responses/user-conversations-response';
import { MessageResponse } from './../../models/conversations/responses/conversation-messages-response';
import { ConversationsService } from './../../services/conversations.service';
import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ConversationListComponent } from './conversation-list/conversation-list.component';
import { ConversationWindowComponent } from './conversation-window/conversation-window.component';
import { AuthenticationService } from '../../services/authentication.service';
import { ChatHubService } from '../../services/chat-hub.service';
import { ConversationType } from '../../enums/conversation-type';
import { DeliveryStatus } from '../../enums/delivery-status';
import { User } from '../../models/interfaces/userInterface';
@Component({
  selector: 'app-conversations',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    ConversationListComponent,
    ConversationWindowComponent,
  ],
  templateUrl: './conversations.component.html',
  styleUrl: './conversations.component.css',
})
export class ConversationsComponent implements OnInit {

  selectedConversation: UserConversation | null = null;
  conversations: UserConversation[] = [];
  others: UserConversation[] = [];
  newMessage: MessageResponse | null = null;

  constructor(
    private authService: AuthenticationService,
    private conversationsService: ConversationsService,
    private chatHubService: ChatHubService
  ) { }

  ngOnInit(): void {
    this.conversationsService.getUserConversations().subscribe({
      next: (list) => {
        this.conversations = list ?? [];
      },
      error: (err) => {
        console.error(err);
      },
    });


    this.chatHubService.onNewConversation((newConversation: UserConversation) => {
      this.handleNewConversationReceived(newConversation);
    });

    this.chatHubService.onNewDirectConversationInfo((newConversation: UserConversation) => {
      this.handleNewDirectConversationInfoReceived(newConversation);
    });

    this.chatHubService.onReceiveMessage(async (message: MessageResponse) => {
      console.log('Received message via SignalR:', message);
      this.handleMessageReceived(message);
      try {
        const currentUserId = this.authService.getCurrentUser()?.id;

        if (message.senderId !== currentUserId) {
          await this.chatHubService.NotifyMessagesDelivered([message.id]);

          if (this.selectedConversation?.id === message.conversationId) {
            try {
              await this.chatHubService.NotifyMessagesRead([message.id]);
            } catch (err) {
              console.error('Error notifying messages read:', err);
            }
          }
          else
            this.updateUnreadCountForConversation(message.conversationId);
        }
      } catch (err) {
        console.error('Error notifying messages delivered:', err);
      }

      this.others = this.others.filter(otherConv => otherConv.id !== message.conversationId);

    });


    this.chatHubService.onUserOnlineStatusChanged((userId: number, isOnline: boolean) => {
      this.handleUserOnlineStatusChange(userId, isOnline);
    });

    this.chatHubService.onMessagesDelivered((messageIds: number[]) => {
      this.handleMessagesDelivered(messageIds);
    });
  }

  private handleNewConversationReceived(newConversation: UserConversation): void {
    if (!this.existInConversations(newConversation)) {
      this.addConversationToTop(newConversation);
    }

    this.others = this.others.filter(otherConv => this.checkIfTheSameDirectConversation(newConversation, otherConv) ? false : true);
  }

  private handleNewDirectConversationInfoReceived(newConversation: UserConversation): void {
    this.selectedConversation = newConversation;
  }


  private handleMessageReceived(message: MessageResponse): void {
    const conversation = this.findConversationById(message.conversationId);

    if (conversation) {
      const updatedConversation = { ...conversation, lastMessage: message };
      this.removeConversationById(message.conversationId);
      this.addConversationToTop(updatedConversation);

      if (this.selectedConversation && this.selectedConversation.id === message.conversationId) {
        this.newMessage = message;
      }
    } else {
      this.conversationsService.getConversationById(message.conversationId).subscribe({
        next: (conv) => {
          if (conv) {
            if (this.selectedConversation?.id == conv.id) {
              this.newMessage = message;
            }
            this.addConversationToTop(conv);

            // Remove from others list to prevent race condition with search
            this.others = this.others.filter(otherConv =>
              !this.checkIfTheSameDirectConversation(conv, otherConv)
            );
          }
        },
        error: (error) => {
          console.error('Error fetching conversation for new message:', error);
        }
      });
    }
  }

  handleUserOnlineStatusChange(userId: number, isOnline: boolean) {
    const conversation = this.conversationsService.getDirectConversationWithUser(this.conversations, userId);
    if (conversation) {
      const otherParticipant = this.conversationsService.getOtherParticipantInDirectConversation(conversation.participants);
      if (otherParticipant) {
        // Create new objects to trigger change detection
        const updatedParticipant = { ...otherParticipant, isOnline: isOnline };
        const updatedParticipants = conversation.participants.map(p =>
          p.userId === userId ? updatedParticipant : p
        );
        const updatedConversation = { ...conversation, participants: updatedParticipants };

        this.updateConversationInList(updatedConversation);

        if (this.selectedConversation && this.selectedConversation.id === updatedConversation.id) {
          this.selectedConversation = updatedConversation;
        }
      }
    }
  }


  private checkIfTheSameDirectConversation(firstConversation: UserConversation | null, secondConversation: UserConversation | null): boolean {
    if (firstConversation?.type !== ConversationType.Direct || secondConversation?.type !== ConversationType.Direct)
      return false;

    const secondConversationUserIds = secondConversation.participants.map(p => p.userId);
    return firstConversation.participants.every(fp =>
      secondConversationUserIds.includes(fp.userId)
    );
  }

  private handleMessagesDelivered(messageIds: number[]): void {
    this.updateConversationsMessagesDeliveryStatus(messageIds);
    this.updateNewMessageDeliveryStatus(messageIds);
  }

  onConversationSelected(conversation: UserConversation) {
    console.log('Conversation selected:', conversation);
    if (!conversation.id) {
      this.chatHubService.createConversation(
        conversation.participants.map(p => p.userId),
        null,
        conversation.type
      ).catch(err => {
        console.error('Error creating new conversation:', err);
      });
    }
    else {
      this.selectedConversation = conversation;
    }
  }

  onSearchChanged(searchValue: string) {
    this.others = [];
    this.conversationsService.getOtherConversations(searchValue).subscribe({
      next: (conversation) => {
        if (!this.existInConversations(conversation)) {
          this.others = [conversation];
        }
      },
      error: (error) => {
        console.log('User not found or error occurred:', error);
      }
    });
  }

  updateUnreadCountForConversation(conversationId: number) {
    const conversation = this.findConversationById(conversationId);
    if (conversation) {
      const updatedConversation = { ...conversation, unreadCount: conversation.unreadCount + 1 };
      this.updateConversationInList(updatedConversation);
    }
  }

  // Helper Functions
  private findConversationById(conversationId: number): UserConversation | undefined {
    return this.conversations.find(conv => conv.id === conversationId);
  }

  private existInConversations(conversation: UserConversation): boolean {
    return this.conversations.some(conv => conv.id === conversation.id);
  }

  private updateConversationInList(updatedConversation: UserConversation): void {
    this.conversations = this.conversations.map(conv =>
      conv.id === updatedConversation.id ? updatedConversation : conv
    );
  }

  private addConversationToTop(conversation: UserConversation): void {
    this.conversations = [conversation, ...this.conversations];
  }

  private removeConversationById(conversationId: number): void {
    this.conversations = this.conversations.filter(conv => conv.id !== conversationId);
  }

  private updateConversationsMessagesDeliveryStatus(messageIds: number[]): void {
    this.conversations = this.conversations.map(conversation => {
      if (conversation.lastMessage &&
        messageIds.includes(conversation.lastMessage.id) &&
        conversation.lastMessage.deliveryStatus === DeliveryStatus.Sent) {

        const updatedLastMessage = {
          ...conversation.lastMessage,
          deliveryStatus: DeliveryStatus.Delivered
        };
        return { ...conversation, lastMessage: updatedLastMessage };
      }
      return conversation;
    });
  }

  private updateNewMessageDeliveryStatus(messageIds: number[]): void {
    if (this.newMessage &&
      messageIds.includes(this.newMessage.id) &&
      this.newMessage.deliveryStatus === DeliveryStatus.Sent) {
      this.newMessage = {
        ...this.newMessage,
        deliveryStatus: DeliveryStatus.Delivered
      };
    }
  }



}

