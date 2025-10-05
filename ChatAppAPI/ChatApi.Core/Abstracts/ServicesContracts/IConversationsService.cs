using ChatApi.Core.Bases;
using ChatApi.Core.Entities.ChatEntities;
using ChatApi.Core.Enums;
using ChatApi.Core.Enums.ChatEnums;
using ChatApi.Core.Features.Chat.Commands.RequestsModels;

namespace ChatApi.Core.Abstracts.ServicesContracts {
    public interface IConversationsService {
        Task<ServiceOperationResult<Conversation?>> CreateConversationAsync(CreateConversationCommand request);
        Task<ServiceOperationResult<string?>> AddParticipantAsync(int conversationId, int userId, ConversationParticipantRole role);
        Task<ServiceOperationResult<string?>> RemoveParticipantAsync(int conversationId, int userId);
        Task<Conversation?> GetConversationByIdAsync(int conversationId);
        Task<IEnumerable<Conversation>> GetUserConversationsAsync(int userId);
        Task<ServiceOperationResult<string?>> UpdateTypingStatusAsync(int conversationId, int userId, bool isTyping);
        Task<IEnumerable<TypingIndicator>> GetActiveTypingIndicatorsAsync(int conversationId);
        Task<bool> IsUserInConversationAsync(int userId, int conversationId);
        Task<Conversation?> GetDirectConversationBetweenUsersAsync(int userId1, int userId2);
        Task<bool> HasDirectConversationWith(int user1Id, int user2Id);
        Task<string> GetConversationTitle(int conversationId);
    }
}
