using ChatApi.Core.Abstracts.InfrastructureAbstracts;
using ChatApi.Core.Entities.ChatEntities;
using ChatApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChatApi.Infrastructure.Repositories {
    public class TypingIndicatorRepository : GenericRepository<TypingIndicator>, ITypingIndicatorRepository {
        public TypingIndicatorRepository(AppDbContext dbContext) : base(dbContext) { }

        public async Task<IEnumerable<TypingIndicator>> GetActiveTypingIndicatorsAsync(int conversationId) {
            var cutoffTime = DateTime.UtcNow.AddSeconds(-10); // Consider typing inactive after 10 seconds
            return await _dbContext.TypingIndicators
                .Where(t => t.ConversationId == conversationId && 
                           t.IsTyping && 
                           t.LastTypingAt > cutoffTime)
                .Include(t => t.User)
                .ToListAsync();
        }

        public async Task<TypingIndicator?> GetUserTypingIndicatorAsync(int conversationId, int userId) {
            return await _dbContext.TypingIndicators
                .FirstOrDefaultAsync(t => t.ConversationId == conversationId && t.UserId == userId);
        }
    }
}