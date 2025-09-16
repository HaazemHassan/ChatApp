export interface ConversationMessagesResponse {
  conversationId: number;
  conversationTitle?: string;
  messages: MessageResponse[];
}

export interface MessageResponse {
  id: number;
  conversationId: number;
  senderId: number;
  senderName: string;
  senderFullName: string;
  content: string;
  sentAt: string;
}
