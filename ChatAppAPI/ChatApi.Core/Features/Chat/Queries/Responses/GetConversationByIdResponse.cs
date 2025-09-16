using ChatApi.Core.Enums.ChatEnums;

namespace ChatApi.Core.Features.Chat.Queries.Responses {
    public class GetConversationByIdResponse {
        public int Id { get; set; }
        public string? Title { get; set; }
        public ConversationType Type { get; set; }
        public List<ConversationParticipantResponse> Participants { get; set; } = new();
        public MessageResponse? LastMessage { get; set; }

        //public bool IsActive { get; set; }
        //public DateTime CreatedAt { get; set; }
        //public int? CreatedByUserId { get; set; }
        //public string? CreatedByUserName { get; set; }
        //public int UnreadCount { get; set; }
    }
}