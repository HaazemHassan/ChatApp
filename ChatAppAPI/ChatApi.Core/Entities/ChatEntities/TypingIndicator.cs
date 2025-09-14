using ChatApi.Core.Entities.IdentityEntities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatApi.Core.Entities.ChatEntities {
    public class TypingIndicator {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ConversationId { get; set; }

        [Required]
        public int UserId { get; set; }

        public bool IsTyping { get; set; } = true;
        public DateTime LastTypingAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey(nameof(ConversationId))]
        public virtual Conversation Conversation { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser User { get; set; }
    }
}
