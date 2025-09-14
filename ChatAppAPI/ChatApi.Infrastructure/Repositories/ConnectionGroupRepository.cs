using ChatApi.Core.Abstracts.InfrastructureAbstracts;
using ChatApi.Core.Entities.ChatEntities;
using ChatApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChatApi.Infrastructure.Repositories {
    public class ConnectionGroupRepository : GenericRepository<ConnectionGroup>, IConnectionGroupRepository {
        public ConnectionGroupRepository(AppDbContext dbContext) : base(dbContext) { }

        public async Task<IEnumerable<ConnectionGroup>> GetGroupConnectionsAsync(string groupName) {
            // For SignalR groups, we'll use ConversationId from group name
            if (!groupName.StartsWith("Conversation_"))
                return new List<ConnectionGroup>();

            var conversationIdStr = groupName.Replace("Conversation_", "");
            if (!int.TryParse(conversationIdStr, out var conversationId))
                return new List<ConnectionGroup>();

            return await _dbContext.ConnectionGroups
                .Where(cg => cg.ConversationId == conversationId && cg.IsActive)
                .Include(cg => cg.UserConnection)
                .ToListAsync();
        }
    }
}