using ChatApi.Core.Entities.ChatEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatApi.Infrastructure.Data.EntitiesConfigurations {
    public class ConversationConfiguration : IEntityTypeConfiguration<Conversation> {
        public void Configure(EntityTypeBuilder<Conversation> builder) {
            builder.HasKey(c => c.Id);
            
            builder.Property(c => c.Title)
                .HasMaxLength(100);

            builder.Property(c => c.Type)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(c => c.CreatedAt)
                .IsRequired();

            builder.Property(c => c.LastMessageAt)
                .IsRequired();

            builder.Property(c => c.IsActive)
                .HasDefaultValue(true);

            // Relationships
            builder.HasOne(c => c.CreatedBy)
                .WithMany(u => u.CreatedConversations)
                .HasForeignKey(c => c.CreatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(c => c.Participants)
                .WithOne(cp => cp.Conversation)
                .HasForeignKey(cp => cp.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(c => c.Messages)
                .WithOne(m => m.Conversation)
                .HasForeignKey(m => m.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}