using ChatApi.Core.Enums.ChatEnums;

namespace ChatApi.Core.Features.Chat.Queries.Responses {
    public class GetNewConversationResponse {
        public int? Id { get; set; }
        public string Title { get; set; }
        public ConversationType Type { get; set; } = ConversationType.Direct;
        public List<ConversationParticipantResponse> Participants { get; set; } = new();
        public MessageResponse? LastMessage { get; set; }
        public int UnreadCount { get; set; }
    }
}