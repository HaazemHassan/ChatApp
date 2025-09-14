using ChatApi.Core.Enums;

namespace ChatApi.Core.Abstracts.ServicesContracts {
    public interface IConnectionService {
        Task<ServiceOperationStatus> AddUserConnectionAsync(int userId, string connectionId);
        Task<ServiceOperationStatus> RemoveUserConnectionAsync(string connectionId);
        Task<ServiceOperationStatus> AddToGroupAsync(string connectionId, int conversationId);
        Task<ServiceOperationStatus> RemoveFromGroupAsync(string connectionId, string groupName);
        Task<IEnumerable<string>> GetUserConnectionsAsync(int userId);
        Task<ServiceOperationStatus> UpdateUserOnlineStatusAsync(int userId, bool isOnline);
        Task<IEnumerable<int>> GetOnlineUsersAsync();
    }
}