using ChatApi.Core.Entities.ChatEntities;
using ChatApi.Core.Entities.IdentityEntities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace ChatApi.Infrastructure.Data {
    public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int, IdentityUserClaim<int>, IdentityUserRole<int>, IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>> {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }


        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<ConversationParticipant> ConversationParticipants { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<MessageDelivery> MessageDeliveries { get; set; }
        public DbSet<UserConnection> UserConnections { get; set; }
        public DbSet<ConnectionGroup> ConnectionGroups { get; set; }
        public DbSet<TypingIndicator> TypingIndicators { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {

            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());


            // Message -> MessageDelivery (NO CASCADE)
            modelBuilder.Entity<MessageDelivery>()
                .HasOne(md => md.Message)
                .WithMany(m => m.MessageDeliveries)
                .HasForeignKey(md => md.MessageId)
                .OnDelete(DeleteBehavior.NoAction);

            // User -> MessageDelivery (NO CASCADE)  
            modelBuilder.Entity<MessageDelivery>()
                .HasOne(md => md.User)
                .WithMany()
                .HasForeignKey(md => md.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Message -> Message (Reply relationship - NO CASCADE)
            modelBuilder.Entity<Message>()
                .HasOne(m => m.ReplyToMessage)
                .WithMany(m => m.Replies)
                .HasForeignKey(m => m.ReplyToMessageId)
                .OnDelete(DeleteBehavior.NoAction);

            // ConversationParticipant -> Message (LastReadMessage - NO CASCADE)
            modelBuilder.Entity<ConversationParticipant>()
                .HasOne(cp => cp.LastReadMessage)
                .WithMany()
                .HasForeignKey(cp => cp.LastReadMessageId)
                .OnDelete(DeleteBehavior.NoAction);

            // User -> Conversation (CreatedBy - NO CASCADE for groups)
            modelBuilder.Entity<Conversation>()
                .HasOne(c => c.CreatedBy)
                .WithMany(u => u.CreatedConversations)
                .HasForeignKey(c => c.CreatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);


            modelBuilder.Entity<Conversation>()
              .Property(c => c.Type)
              .HasConversion<int>();

            modelBuilder.Entity<ConversationParticipant>()
                .Property(cp => cp.Role)
                .HasConversion<int>();

            modelBuilder.Entity<Message>()
                .Property(m => m.MessageType)
                .HasConversion<int>();

            modelBuilder.Entity<MessageDelivery>()
                .Property(md => md.Status)
                .HasConversion<int>();
        }
    }
}
