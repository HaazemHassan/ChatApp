using ChatApi.Core.Entities.ChatEntities;

namespace ChatApi.Core.Abstracts.InfrastructureAbstracts {
    public interface IConversationRepository : IGenericRepository<Conversation> {
        Task<IEnumerable<Conversation>> GetUserConversationsAsync(int userId);
        Task<Conversation?> GetConversationWithParticipantsAsync(int conversationId);
        Task<Conversation?> GetDirectConversationBetweenUsersAsync(int userId1, int userId2);
    }
}