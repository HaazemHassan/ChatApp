using System.ComponentModel.DataAnnotations.Schema;

namespace ChatApi.Core.Entities.IdentityEntities {
    public class RefreshToken {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? AccessTokenJTI { get; set; }          //Access token jti
        public string Token { get; set; }
        public DateTime Expires { get; set; }
        public bool IsExpired => DateTime.UtcNow >= Expires;
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime? RevokationDate { get; set; }
        public string? ReplacedByToken { get; set; }
        public bool IsActive => RevokationDate == null && !IsExpired;

        [ForeignKey(nameof(UserId))]
        public ApplicationUser? User { get; set; }
    }
}
