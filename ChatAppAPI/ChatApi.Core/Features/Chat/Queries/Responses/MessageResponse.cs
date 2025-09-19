using ChatApi.Core.Enums.ChatEnums;

namespace ChatApi.Core.Features.Chat.Queries.Responses {
    public class MessageResponse {
        public int Id { get; set; }
        public int ConversationId { get; set; }
        public int SenderId { get; set; }
        public string SenderName { get; set; }
        public string SenderFullName { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; }
        public MessageType MessageType { get; set; }

        //public DateTime? EditedAt { get; set; }
        //public bool IsDeleted { get; set; }
        //public int? ReplyToMessageId { get; set; }
        //public MessageResponse? ReplyToMessage { get; set; }
        //public List<MessageResponse> Replies { get; set; } = new();
        //public bool IsRead { get; set; }
    }
}