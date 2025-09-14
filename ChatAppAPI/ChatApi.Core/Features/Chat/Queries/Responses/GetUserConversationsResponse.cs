using ChatApi.Core.Enums.ChatEnums;

namespace ChatApi.Core.Features.Chat.Queries.Responses {
    public class GetUserConversationsResponse {
        public int Id { get; set; }
        public string? Title { get; set; }
        public ConversationType Type { get; set; }
        public DateTime LastMessageAt { get; set; }
        public List<ConversationParticipantResponse> Participants { get; set; } = new();
        //public MessageResponse? LastMessage { get; set; }
        //public int UnreadCount { get; set; }
    }
}