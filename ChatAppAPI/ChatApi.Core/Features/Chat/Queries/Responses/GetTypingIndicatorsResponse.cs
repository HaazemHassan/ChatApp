namespace ChatApi.Core.Features.Chat.Queries.Responses {
    public class GetTypingIndicatorsResponse {
        public int ConversationId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public bool IsTyping { get; set; }
        public DateTime LastTypingAt { get; set; }
    }
}