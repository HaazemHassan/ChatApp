import { MessageType } from "@microsoft/signalr";

export interface SendMessageRequest {
  conversationId: number;
  content: string;
}
