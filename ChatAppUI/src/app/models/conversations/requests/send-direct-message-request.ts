import { MessageType } from '../../../enums/message-type';

export interface SendDirectMessageRequest {
  senderId: number;
  recipientUserId: number;
  content: string;
  messageType?: MessageType;
}
