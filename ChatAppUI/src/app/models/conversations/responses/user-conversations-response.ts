import { ConversationType } from '../../../enums/conversation-type';

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
  lastMessageAt: string | null;
  participants: Participant[];
}
