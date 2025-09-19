import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { AuthenticationService } from './authentication.service';
import { MessageResponse } from '../models/conversations/responses/conversation-messages-response';
import { UserConversation } from '../models/conversations/responses/user-conversations-response';
import { MessageType } from '../enums/message-type';
import { ConversationType } from '../enums/conversation-type';
import { SendMessageRequest } from '../models/conversations/requests/send-message-request';

@Injectable({
  providedIn: 'root',
})
export class ChatHubService {
  private hubConnection!: signalR.HubConnection;
  private isStarted = false;

  startConnection(accessToken: string | null) {
    if (this.isStarted) return;
    const token = accessToken;
    if (token === null) {
      console.error('No access token found. User might not be authenticated.');
      return;
    }
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('https://localhost:44318/chatHub', {
        accessTokenFactory: () => token!
      })
      .withAutomaticReconnect()
      .build();

    return this.hubConnection.start().then(() => {
      this.isStarted = true;
      console.log('SignalR connection started.');
    });
  }



  stopConnection() {
    return this.hubConnection?.stop();
  }

  onReceiveMessage(callback: (message: MessageResponse) => void) {
    this.hubConnection.on('ReceiveMessage', callback);
  }

  onNewConversation(callback: (conversation: UserConversation) => void) {
    this.hubConnection.on('NewGroupCreated', callback);
  }

  onNewDirectConversationInfo(callback: (conversation: UserConversation) => void) {
    this.hubConnection.on('NewDirectConversationInfo', callback);
  }

  onError(callback: (error: any) => void) {
    this.hubConnection.on('Error', callback);
  }

  sendMessage(request: SendMessageRequest) {
    return this.hubConnection.invoke(
      'SendMessage',
      request
    );
  }

  createConversation(participantIds: number[], title: string | null, type: ConversationType) {
    return this.hubConnection.invoke(
      'CreateConversation',
      participantIds,
      title,
      type
    );
  }
}
