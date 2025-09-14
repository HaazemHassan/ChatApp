using ChatApi.Core.Entities.IdentityEntities;
using ChatApi.Core.Enums.ChatEnums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatApi.Core.Entities.ChatEntities {
    public class ConversationParticipant {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ConversationId { get; set; }

        [Required]
        public int UserId { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LeftAt { get; set; }

        [Required]
        public ConversationParticipantRole Role { get; set; } = ConversationParticipantRole.Member;

        public bool IsActive { get; set; } = true;        // soft leave (if the user left the conversation but record is kept for history)

        // Tracking last read message
        public int? LastReadMessageId { get; set; }
        public DateTime? LastReadAt { get; set; }

        [ForeignKey(nameof(ConversationId))]
        public virtual Conversation Conversation { get; set; }


        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser User { get; set; }


        [ForeignKey(nameof(LastReadMessageId))]
        public virtual Message? LastReadMessage { get; set; }
    }
}
