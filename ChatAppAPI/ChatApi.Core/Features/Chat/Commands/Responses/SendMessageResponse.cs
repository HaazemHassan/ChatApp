namespace ChatApi.Core.Features.Chat.Commands.Responses {
    public class SendMessageResponse {
        public int Id { get; set; }
        public int ConversationId { get; set; }
        public int SenderId { get; set; }
        public string SenderName { get; set; }
        public string SenderFullName { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; }

        //public MessageType MessageType { get; set; }
        //public int? ReplyToMessageId { get; set; }
    }
}