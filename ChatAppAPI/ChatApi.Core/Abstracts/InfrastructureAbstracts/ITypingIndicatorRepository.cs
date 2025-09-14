using ChatApi.Core.Entities.ChatEntities;

namespace ChatApi.Core.Abstracts.InfrastructureAbstracts {
    public interface ITypingIndicatorRepository : IGenericRepository<TypingIndicator> {
        Task<IEnumerable<TypingIndicator>> GetActiveTypingIndicatorsAsync(int conversationId);
        Task<TypingIndicator?> GetUserTypingIndicatorAsync(int conversationId, int userId);
    }
}