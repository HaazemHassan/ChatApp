namespace ChatApi.Core.Features.Chat.Queries.Responses {
    public class ConversationParticipantResponse {
        public int? Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        //public ConversationParticipantRole Role { get; set; }
        //public DateTime JoinedAt { get; set; }
        //public DateTime? LastReadAt { get; set; }
        //public bool IsOnline { get; set; }
        //public int? LastReadMessageId { get; set; }
    }
}