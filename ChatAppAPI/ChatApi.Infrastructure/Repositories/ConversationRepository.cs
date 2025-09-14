using ChatApi.Core.Abstracts.InfrastructureAbstracts;
using ChatApi.Core.Entities.ChatEntities;
using ChatApi.Core.Enums.ChatEnums;
using ChatApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChatApi.Infrastructure.Repositories {
    public class ConversationRepository : GenericRepository<Conversation>, IConversationRepository {
        public ConversationRepository(AppDbContext dbContext) : base(dbContext) { }

        public async Task<IEnumerable<Conversation>> GetUserConversationsAsync(int userId) {
            return await _dbContext.Conversations.Where(c => c.IsActive && c.Messages.Any())
                .Where(c => c.Participants.Any(p => p.IsActive && p.UserId == userId))
                .Include(c => c.Participants)
                    .ThenInclude(p => p.User)
                .OrderByDescending(c => c.LastMessageAt)
                .ToListAsync();
        }

        public async Task<Conversation?> GetConversationWithParticipantsAsync(int conversationId) {
            return await _dbContext.Conversations
                .Include(c => c.Participants)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(c => c.Id == conversationId);
        }

        public async Task<Conversation?> GetDirectConversationBetweenUsersAsync(int userId1, int userId2) {
            return await _dbContext.Conversations
                .Where(c => c.Type == ConversationType.Direct && c.IsActive)
                .Where(c =>
                       c.Participants.Count(p => p.IsActive && (p.UserId == userId1 || p.UserId == userId2)) == 2
                )
                .Include(c => c.Participants)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync();
        }

    }
}