using ChatApi.Core.Abstracts.InfrastructureAbstracts;
using ChatApi.Core.Entities.ChatEntities;
using ChatApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChatApi.Infrastructure.Repositories {
    public class ConversationParticipantRepository : GenericRepository<ConversationParticipant>, IConversationParticipantRepository {
        public ConversationParticipantRepository(AppDbContext dbContext) : base(dbContext) { }

        public async Task<IEnumerable<ConversationParticipant>> GetConversationParticipantsAsync(int conversationId) {
            return await _dbContext.ConversationParticipants
                .Where(p => p.ConversationId == conversationId && p.IsActive)
                .Include(p => p.User)
                .ToListAsync();
        }

        public async Task<ConversationParticipant?> GetParticipantAsync(int conversationId, int userId) {
            return await _dbContext.ConversationParticipants
                .FirstOrDefaultAsync(p => p.ConversationId == conversationId && p.UserId == userId);
        }
    }
}