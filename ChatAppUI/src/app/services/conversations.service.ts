import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, Observable, catchError } from 'rxjs';
import { ApiResponse } from '../models/api-response';
import { Participant, UserConversation } from '../models/conversations/responses/user-conversations-response';
import { environment } from '../../environments/environment';
import { ConversationMessagesResponse, MessageResponse } from '../models/conversations/responses/conversation-messages-response';
import { AuthenticationService } from './authentication.service';
import { ConversationType } from '../enums/conversation-type';


@Injectable({
  providedIn: 'root',
})
export class ConversationsService {
  apiUrl: string = environment.apiUrl;
  constructor(private http: HttpClient, private authService: AuthenticationService) { }

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
    conversationId: number,
    pageNumber: number = 1,
    pageSize: number = 20
  ): Observable<ConversationMessagesResponse> {
    return this.http
      .get<ApiResponse<ConversationMessagesResponse>>(
        `${this.apiUrl}/chat/conversations/${conversationId}/messages`,
        {
          params: {
            pageNumber: pageNumber.toString(),
            pageSize: pageSize.toString()
          }
        }
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


  // For direct conversations
  getOtherParticipantInDirectConversation(participants: Participant[]): Participant | null {
    const curUserId = this.authService.getCurrentUserId();
    return participants.find(p => p.userId !== curUserId) || null;

  }

  IsOtherParticipantOnline(participants: Participant[]): boolean {
    const otherParticipant = this.getOtherParticipantInDirectConversation(participants);
    return otherParticipant ? otherParticipant.isOnline : false;
  }

  getDirectConversationWithUser(conversations: UserConversation[], userId: number): UserConversation | null {
    return conversations.find(conv => conv.type === ConversationType.Direct &&
      conv.participants.some(p => p.userId === userId)
    ) || null;
  }
}
