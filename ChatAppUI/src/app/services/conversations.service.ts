import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, Observable, catchError } from 'rxjs';
import { ApiResponse } from '../models/api-response';
import { UserConversation } from '../models/conversations/responses/user-conversations-response';
import { environment } from '../../environments/environment';
import { ConversationMessagesResponse, MessageResponse } from '../models/conversations/responses/conversation-messages-response';
import { SendDirectMessageRequest } from '../models/conversations/requests/send-direct-message-request';
import { User } from '../models/interfaces/userInterface';


@Injectable({
  providedIn: 'root',
})
export class ConversationsService {
  apiUrl: string = environment.apiUrl;
  constructor(private http: HttpClient) { }

  getUserConversations(): Observable<UserConversation[]> {
    return this.http
      .get<ApiResponse<UserConversation[]>>(`${this.apiUrl}/chat/conversations`)
      .pipe(
        map((res) => res.data ?? []),
        catchError((error) => {
          console.error('Error fetching user conversations:', error);
          throw error;
        })
      );
  }

  getConversationById(conversationId: number): Observable<UserConversation | null> {
    return this.http
      .get<ApiResponse<UserConversation>>(`${this.apiUrl}/chat/conversations/${conversationId}`)
      .pipe(
        map((res) => res.data ?? null),
        catchError((error) => {
          console.error('Error fetching conversation:', error);
          throw error;
        })
      );
  }


  getConversationMessages(
    conversationId: number
  ): Observable<ConversationMessagesResponse> {
    return this.http
      .get<ApiResponse<ConversationMessagesResponse>>(
        `${this.apiUrl}/chat/conversations/${conversationId}/messages`
      )
      .pipe(
        map((res) => res.data!),
        catchError((error) => {
          console.error('Error fetching messages:', error);
          throw error;
        })
      );
  }


  getOtherConversations(username: string): Observable<UserConversation> {
    return this.http
      .get<ApiResponse<UserConversation>>(
        `${this.apiUrl}/chat/conversations/new/${username}`
      )
      .pipe(
        map((res) => res.data!),
        catchError((error) => {
          console.error('Error fetching other conversations:', error);
          throw error;
        })
      );
  }
}
