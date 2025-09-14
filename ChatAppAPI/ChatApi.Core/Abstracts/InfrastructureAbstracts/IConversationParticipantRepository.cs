using ChatApi.Core.Entities.ChatEntities;

namespace ChatApi.Core.Abstracts.InfrastructureAbstracts {
    public interface IConversationParticipantRepository : IGenericRepository<ConversationParticipant> {
        Task<IEnumerable<ConversationParticipant>> GetConversationParticipantsAsync(int conversationId);
        Task<ConversationParticipant?> GetParticipantAsync(int conversationId, int userId);
    }
}