using ChatApi.Core.Entities.IdentityEntities;
using ChatApi.Core.Enums.ChatEnums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatApi.Core.Entities.ChatEntities {
    public class Message {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ConversationId { get; set; }

        [Required]
        public int SenderId { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public MessageType MessageType { get; set; } = MessageType.Text;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public DateTime? EditedAt { get; set; }
        public bool IsDeleted { get; set; } = false;

        public int? ReplyToMessageId { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(ConversationId))]
        public virtual Conversation Conversation { get; set; }

        [ForeignKey(nameof(SenderId))]
        public virtual ApplicationUser Sender { get; set; }

        [ForeignKey(nameof(ReplyToMessageId))]
        public virtual Message? ReplyToMessage { get; set; }

        public virtual ICollection<Message> Replies { get; set; } = new HashSet<Message>();
        public virtual ICollection<MessageDelivery> MessageDeliveries { get; set; } = new HashSet<MessageDelivery>();
    }
}
