import { ConversationType } from '../../../enums/conversation-type';
import { MessageResponse } from './conversation-messages-response';

export interface Participant {
  id?: number;
  userId: number;
  userName: string;
  fullName: string;
  isOnline: boolean;
}

export interface UserConversation {
  id?: number;
  title: string;
  type: ConversationType;
  participants: Participant[];
  lastMessage: MessageResponse | null;
  unreadCount: number;
}
