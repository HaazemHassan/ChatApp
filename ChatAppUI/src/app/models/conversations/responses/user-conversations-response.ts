import { ConversationType } from '../../../enums/conversation-type';
import { MessageResponse } from './conversation-messages-response';

export interface Participant {
  id?: number;
  userId: number;
  userName: string;
  fullName: string;
}

export interface UserConversation {
  id?: number;
  title: string;
  type: ConversationType;
  // lastMessageAt: string | null;
  participants: Participant[];
  lastMessage: MessageResponse | null;
}
