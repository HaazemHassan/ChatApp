using ChatApi.Core.Entities.ChatEntities;

namespace ChatApi.Core.Abstracts.InfrastructureAbstracts {
    public interface IMessageDeliveryRepository : IGenericRepository<MessageDelivery> {
        Task<IEnumerable<MessageDelivery>> GetMessageDeliveriesAsync(int messageId);
        Task<MessageDelivery?> GetUserMessageDeliveryAsync(int messageId, int userId);
    }
}