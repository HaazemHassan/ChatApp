using ChatApi.Core.Bases;
using ChatApi.Core.Entities.ChatEntities;
using ChatApi.Core.Enums;
using ChatApi.Core.Enums.ChatEnums;

namespace ChatApi.Core.Abstracts.ServicesContracts {
    public interface IMessagesService {
        Task<ServiceOperationResult<string?>> SendMessageAsync(Message message);
        Task<ServiceOperationResult<string?>> EditMessageAsync(int messageId, string newContent);
        Task<ServiceOperationResult<string?>> DeleteMessageAsync(int messageId);
        Task<IEnumerable<Message>> GetConversationMessagesAsync(int conversationId, int skip = 0, int take = 50);
        Task<IEnumerable<Message>> GetConversationMessagesWithDeliveryAsync(int conversationId, int userId, int skip = 0, int take = 50);
        Task<ServiceOperationResult<string?>> MarkMessagesAsReadAsync(List<int> messageIds, int userId);
        Task<ServiceOperationResult<string?>> MarkMessagesAsDeliveredAsync(List<int> messageIds, int userId);
        Task<Message?> GetMessageWithSenderAsync(int messageId);
        Task<Message?> GetMessageWithDeliveryAsync(int messageId, int userId);
        Task<Message?> GetLastMessageInConversationAsync(int conversationId);
        Task<IEnumerable<int>> GetUndeliveredMessageIdsForUserAsync(int userId);
        Task<bool> AreAllParticipantsDeliveredAsync(Message message, DeliveryStatus requiredStatus);
        DeliveryStatus GetDeliveryStatus(Message message);
    }
}
