using ChatApi.Core.Abstracts.InfrastructureAbstracts;
using ChatApi.Core.Entities.ChatEntities;
using ChatApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChatApi.Infrastructure.Repositories {
    public class MessageDeliveryRepository : GenericRepository<MessageDelivery>, IMessageDeliveryRepository {
        public MessageDeliveryRepository(AppDbContext dbContext) : base(dbContext) { }

        public async Task<IEnumerable<MessageDelivery>> GetMessageDeliveriesAsync(int messageId) {
            return await _dbContext.MessageDeliveries
                .Where(md => md.MessageId == messageId)
                .Include(md => md.User)
                .ToListAsync();
        }

        public async Task<MessageDelivery?> GetUserMessageDeliveryAsync(int messageId, int userId) {
            return await _dbContext.MessageDeliveries
                .FirstOrDefaultAsync(md => md.MessageId == messageId && md.UserId == userId);
        }
    }
}