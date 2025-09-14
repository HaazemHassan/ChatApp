using ChatApi.Core.Entities.ChatEntities;
using Microsoft.AspNetCore.Identity;

namespace ChatApi.Core.Entities.IdentityEntities {
    public class ApplicationUser : IdentityUser<int> {
        public ApplicationUser() {
            RefreshTokens = new HashSet<RefreshToken>();
        }

        public string FullName { get; set; }
        public string? Address { get; set; }
        public string? Country { get; set; }

        public DateTime? LastSeen { get; set; }
        public bool IsOnline { get; set; }


        public virtual ICollection<RefreshToken> RefreshTokens { get; set; }

        public virtual ICollection<ConversationParticipant> ConversationParticipants { get; set; } = new HashSet<ConversationParticipant>();
        public virtual ICollection<Message> SentMessages { get; set; } = new HashSet<Message>();
        public virtual ICollection<Conversation> CreatedConversations { get; set; } = new HashSet<Conversation>();
        public virtual ICollection<UserConnection> Connections { get; set; } = new HashSet<UserConnection>();

    }
}
