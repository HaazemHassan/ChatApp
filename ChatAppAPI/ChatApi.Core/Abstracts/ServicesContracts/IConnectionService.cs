using ChatApi.Core.Bases;
using ChatApi.Core.Enums;

namespace ChatApi.Core.Abstracts.ServicesContracts {
    public interface IConnectionService {
        Task<ServiceOperationResult<string?>> AddUserConnectionAsync(int userId, string connectionId);
        Task<ServiceOperationResult<string?>> RemoveUserConnectionAsync(string connectionId);
        Task<ServiceOperationResult<string?>> AddToGroupAsync(string connectionId, int conversationId);
        Task<ServiceOperationResult<string?>> RemoveFromGroupAsync(string connectionId, string groupName);
        Task<IEnumerable<string>> GetUserConnectionsAsync(int userId);
        Task<ServiceOperationResult<string?>> UpdateUserOnlineStatusAsync(int userId, bool isOnline);
        Task<IEnumerable<int>> GetOnlineUsersAsync();
    }
}