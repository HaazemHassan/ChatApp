namespace ChatApi.Core.Features.Chat.Queries.Responses {
    public class GetConversationMessagesResponse {
        public int ConversationId { get; set; }
        public string? ConversationTitle { get; set; }
        public List<MessageResponse> Messages { get; set; } = new();
        //public bool HasMore { get; set; }
        //public int TotalCount { get; set; }
        //public int CurrentPage { get; set; }
    }
}