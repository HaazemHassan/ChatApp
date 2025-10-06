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
      console.log('Received new direct conversation info via SignalR:', newConversation);
      this.handleNewDirectConversationInfoReceived(newConversation);
    });

    this.chatHubService.onReceiveMessage(async (message: MessageResponse) => {
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
    });


    this.chatHubService.onUserOnlineStatusChanged((userId: number, isOnline: boolean) => {
      this.handleUserOnlineStatusChange(userId, isOnline);
    });

    this.chatHubService.onMessagesDelivered((messageIds: number[]) => {
      this.handleMessagesDelivered(messageIds);
    });
  }

  private handleNewConversationReceived(newConversation: UserConversation): void {
    const existingConversation = this.conversations.find(conv =>
      conv.id === newConversation.id
    );
    if (!existingConversation)
      this.conversations = [newConversation, ...this.conversations];
  }

  private handleNewDirectConversationInfoReceived(newConversation: UserConversation): void {
    this.others = [newConversation];
    if (this.checkIfTheSameDirectConversation(this.selectedConversation, newConversation))
      Object.assign(this.selectedConversation!, newConversation);
    else {
      this.selectedConversation = newConversation;
    }
  }


  private handleMessageReceived(message: MessageResponse): void {
    const conversation = this.conversations.find(conv => conv.id === message.conversationId);

    if (conversation) {
      // create new object instead of modifying existing one to ensure change detection
      const updatedConversation = { ...conversation, lastMessage: message };
      this.conversations = this.conversations.filter(conv => conv.id !== message.conversationId);
      this.conversations = [updatedConversation, ...this.conversations];

      // If this is the currently selected conversation, we want to add this message in the conversation window
      if (this.selectedConversation && this.selectedConversation.id === message.conversationId) {
        this.newMessage = message;
      }
    } else {
      this.conversationsService.getConversationById(message.conversationId).subscribe({
        next: (conv) => {
          if (conv) {
            console.log('Fetched conversation for new message:', conv);
            if (this.selectedConversation?.id == conv.id) {
              this.newMessage = message;
              this.others = this.others.filter(otherConv => otherConv.id !== conv.id);
            }
            this.conversations = [conv, ...this.conversations];
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

        this.conversations = this.conversations.map(conv =>
          conv.id === updatedConversation.id ? updatedConversation : conv
        );

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
    this.conversations = this.conversations.map(conversation => {
      if (conversation.lastMessage &&
        messageIds.includes(conversation.lastMessage.id) &&
        conversation.lastMessage.deliveryStatus === DeliveryStatus.Sent) {

        const updatedLastMessage = {
          ...conversation.lastMessage,
          deliveryStatus: DeliveryStatus.Delivered
        };
        return {
          ...conversation,
          lastMessage: updatedLastMessage
        };
      }
      return conversation;
    });

    // Also update the newMessage if it matches any of the messageIds
    if (this.newMessage &&
      messageIds.includes(this.newMessage.id) &&
      this.newMessage.deliveryStatus === DeliveryStatus.Sent) {
      this.newMessage = {
        ...this.newMessage,
        deliveryStatus: DeliveryStatus.Delivered
      };
    }
  }




  onConversationSelected(conversation: UserConversation) {
    console.log('Conversation selected:', conversation);
    if (!conversation.id) {
      this.chatHubService.createConversation(
        conversation.participants.map(p => p.userId),
        null,
        conversation.type
      ).then(() => {
      }).catch(err => {
        console.error('Error creating new conversation:', err);
      });
    }
    else
      this.selectedConversation = conversation;
  }

  onSearchChanged(searchValue: string) {
    this.others = [];
    this.conversationsService.getOtherConversations(searchValue).subscribe({
      next: (conversation) => {
        this.others = [conversation];
        console.log('Fetched other conversation in conversations component:', this.others);
      },
      error: (error) => {
        console.log('User not found or error occurred:', error);
      }
    });
  }


  updateUnreadCountForConversation(conversationId: number) {
    const conversation = this.conversations.find(conv => conv.id === conversationId);
    if (conversation) {
      const updatedConversation = { ...conversation, unreadCount: conversation.unreadCount + 1 };
      this.conversations = this.conversations.map(conv =>
        conv.id === updatedConversation.id ? updatedConversation : conv
      );
    }

  }

}

