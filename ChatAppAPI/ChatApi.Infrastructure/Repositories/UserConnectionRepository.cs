using ChatApi.Core.Abstracts.InfrastructureAbstracts;
using ChatApi.Core.Entities.ChatEntities;
using ChatApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChatApi.Infrastructure.Repositories {
    public class UserConnectionRepository : GenericRepository<UserConnection>, IUserConnectionRepository {
        public UserConnectionRepository(AppDbContext dbContext) : base(dbContext) { }

        public async Task<IEnumerable<UserConnection>> GetUserConnectionsAsync(int userId) {
            return await _dbContext.UserConnections
                .Where(uc => uc.UserId == userId)
                .ToListAsync();
        }

        public async Task<UserConnection?> GetConnectionByIdAsync(string connectionId) {
            return await _dbContext.UserConnections
                .Include(uc => uc.User)
                .FirstOrDefaultAsync(uc => uc.ConnectionId == connectionId);
        }
    }
}