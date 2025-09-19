using ChatApi.Core.Entities.ChatEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatApi.Infrastructure.Data.EntitiesConfigurations {
    public class MessageConfiguration : IEntityTypeConfiguration<Message> {
        public void Configure(EntityTypeBuilder<Message> builder) {
            builder.HasKey(m => m.Id);
            
            builder.Property(m => m.Content)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(m => m.MessageType)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(m => m.SentAt)
                .IsRequired();

            builder.Property(m => m.IsDeleted)
                .HasDefaultValue(false);

            // Relationships
            builder.HasOne(m => m.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            builder.HasOne(m => m.ReplyToMessage)
                .WithMany(m => m.Replies)
                .HasForeignKey(m => m.ReplyToMessageId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(m => m.MessageDeliveries)
                .WithOne(md => md.Message)
                .HasForeignKey(md => md.MessageId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}