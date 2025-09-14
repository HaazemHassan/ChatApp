using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatApi.Core.Entities.ChatEntities {
    public class ConnectionGroup {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserConnectionId { get; set; }

        [Required]
        public int ConversationId { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        [ForeignKey(nameof(UserConnectionId))]
        public virtual UserConnection UserConnection { get; set; }

        [ForeignKey(nameof(ConversationId))]
        public virtual Conversation Conversation { get; set; }
    }
}
