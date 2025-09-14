using ChatApi.Core.Entities.ChatEntities;

namespace ChatApi.Core.Abstracts.InfrastructureAbstracts {
    public interface IMessageRepository : IGenericRepository<Message> {
        Task<IEnumerable<Message>> GetConversationMessagesAsync(int conversationId, int skip = 0, int take = 50);
        Task<Message?> GetMessageWithRepliesAsync(int messageId);
        Task<Message?> GetMessageWithSenderAsync(int messageId);
    }
}