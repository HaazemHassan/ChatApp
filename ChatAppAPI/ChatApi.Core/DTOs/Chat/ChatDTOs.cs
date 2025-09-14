using ChatApi.Core.Enums.ChatEnums;

namespace ChatApi.Core.DTOs.Chat {
    public class ConversationDto {
        public int Id { get; set; }
        public string? Title { get; set; }
        public ConversationType Type { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastMessageAt { get; set; }
        public int CreatedByUserId { get; set; }
        public string CreatedByUserName { get; set; }
        public List<ConversationParticipantDto> Participants { get; set; } = new();
        public MessageDto? LastMessage { get; set; }
        public int UnreadCount { get; set; }
    }

    public class ConversationParticipantDto {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public ConversationParticipantRole Role { get; set; }
        public DateTime JoinedAt { get; set; }
        public DateTime? LastReadAt { get; set; }
        public bool IsOnline { get; set; }
    }

    public class MessageDto {
        public int Id { get; set; }
        public int ConversationId { get; set; }
        public int SenderId { get; set; }
        public string SenderName { get; set; }
        public string Content { get; set; }
        public MessageType MessageType { get; set; }
        public DateTime SentAt { get; set; }
        public DateTime? EditedAt { get; set; }
        public bool IsDeleted { get; set; }
        public int? ReplyToMessageId { get; set; }
        public MessageDto? ReplyToMessage { get; set; }
    }

    public class TypingIndicatorDto {
        public int ConversationId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public bool IsTyping { get; set; }
        public DateTime LastTypingAt { get; set; }
    }
}