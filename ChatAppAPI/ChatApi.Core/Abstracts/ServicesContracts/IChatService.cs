using ChatApi.Core.Bases;
using ChatApi.Core.Entities.ChatEntities;
using ChatApi.Core.Enums;
using ChatApi.Core.Enums.ChatEnums;
using ChatApi.Core.Features.Chat.Commands.RequestsModels;

namespace ChatApi.Core.Abstracts.ServicesContracts {
    public interface IChatService {
        Task<ServiceOperationResult<Conversation?>> CreateConversationAsync(CreateConversationCommand request);
        Task<ServiceOperationStatus> AddParticipantAsync(int conversationId, int userId, ConversationParticipantRole role);
        Task<ServiceOperationStatus> RemoveParticipantAsync(int conversationId, int userId);
        Task<ServiceOperationStatus> SendMessageAsync(Message message);
        Task<ServiceOperationStatus> EditMessageAsync(int messageId, string newContent);
        Task<ServiceOperationStatus> DeleteMessageAsync(int messageId);
        Task<Conversation?> GetConversationByIdAsync(int conversationId);
        Task<IEnumerable<Conversation>> GetUserConversationsAsync(int userId);
        Task<IEnumerable<Message>> GetConversationMessagesAsync(int conversationId, int skip = 0, int take = 50);
        Task<IEnumerable<Message>> GetConversationMessagesWithDeliveryAsync(int conversationId, int userId, int skip = 0, int take = 50);
        Task<ServiceOperationStatus> MarkMessageAsReadAsync(int messageId, int userId);
        Task<ServiceOperationResult<string>> MarkMessagesAsReadAsync(List<int> messageIds, int userId);
        Task<ServiceOperationStatus> MarkMessageAsDeliveredAsync(int messageId, int userId);
        Task<ServiceOperationResult<string>> MarkMessagesAsDeliveredAsync(List<int> messageIds, int userId);
        Task<ServiceOperationStatus> UpdateTypingStatusAsync(int conversationId, int userId, bool isTyping);
        Task<IEnumerable<TypingIndicator>> GetActiveTypingIndicatorsAsync(int conversationId);
        Task<bool> IsUserInConversationAsync(int userId, int conversationId);
        Task<Message?> GetMessageWithSenderAsync(int messageId);
        Task<Message?> GetMessageWithDeliveryAsync(int messageId, int userId);
        Task<Conversation?> GetDirectConversationBetweenUsersAsync(int userId1, int userId2);
        Task<bool> HasDirectConversationWith(int user1Id, int user2Id);
        Task<string> GetConversationTitle(int convesationId);
        Task<Message?> GetLastMessageInConversationAsync(int conversationId);
        Task<IEnumerable<int>> GetUndeliveredMessageIdsForUserAsync(int userId);
    }
}