import { DeliveryStatus } from "../../../enums/delivery-status";
import { MessageType } from "../../../enums/message-type";

export interface ConversationMessagesResponse {
  conversationId: number;
  conversationTitle?: string;
  messages: MessageResponse[];
  hasMore: boolean;
  totalCount: number;
  pageNumber: number;
  pageSize: number;
}

export interface MessageResponse {
  id: number;
  conversationId: number;
  senderId: number;
  senderName: string;
  senderFullName: string;
  content: string;
  sentAt: string;
  messageType: MessageType;
  deliveryStatus: DeliveryStatus;

}
