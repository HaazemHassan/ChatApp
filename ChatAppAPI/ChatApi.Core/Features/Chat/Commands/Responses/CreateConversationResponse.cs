using ChatApi.Core.Enums.ChatEnums;
using ChatApi.Core.Features.Chat.Queries.Responses;

namespace ChatApi.Core.Features.Chat.Commands.Responses {
    public class CreateConversationResponse {
        public int Id { get; set; }
        public string? Title { get; set; }
        public ConversationType Type { get; set; }
        public DateTime LastMessageAt { get; set; }
        public List<ConversationParticipantResponse> Participants { get; set; } = new();
        public MessageResponse? LastMessage { get; set; }

    }
}