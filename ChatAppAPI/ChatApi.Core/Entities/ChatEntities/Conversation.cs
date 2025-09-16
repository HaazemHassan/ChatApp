using ChatApi.Core.Entities.IdentityEntities;
using ChatApi.Core.Enums.ChatEnums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatApi.Core.Entities.ChatEntities {
    public class Conversation {
        [Key]
        public int Id { get; set; }

        [MaxLength(100)]
        public string? Title { get; set; } = null;

        [Required]
        public ConversationType Type { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int? CreatedByUserId { get; set; }

        public DateTime? LastMessageAt { get; set; }
        public bool IsActive { get; set; } = true;        //if not deleted  (soft delete)

        [ForeignKey(nameof(CreatedByUserId))]
        public virtual ApplicationUser? CreatedBy { get; set; }
        public virtual ICollection<ConversationParticipant> Participants { get; set; } = new HashSet<ConversationParticipant>();
        public virtual ICollection<Message> Messages { get; set; } = new HashSet<Message>();
    }
}
