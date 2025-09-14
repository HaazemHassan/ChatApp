using ChatApi.Core.Entities.IdentityEntities;
using ChatApi.Core.Enums.ChatEnums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatApi.Core.Entities.ChatEntities {
    public class MessageDelivery {
        [Key]
        public int Id { get; set; }

        [Required]
        public int MessageId { get; set; }

        [Required]
        public int UserId { get; set; }

        public DateTime? DeliveredAt { get; set; }
        public DateTime? ReadAt { get; set; }

        [Required]
        public DeliveryStatus Status { get; set; } = DeliveryStatus.Sent;

        // Navigation Properties
        [ForeignKey(nameof(MessageId))]
        public virtual Message Message { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser User { get; set; }
    }
}
