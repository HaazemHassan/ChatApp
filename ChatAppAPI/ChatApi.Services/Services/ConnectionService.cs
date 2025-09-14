using ChatApi.Core.Abstracts.InfrastructureAbstracts;
using ChatApi.Core.Abstracts.ServicesContracts;
using ChatApi.Core.Entities.ChatEntities;
using ChatApi.Core.Entities.IdentityEntities;
using ChatApi.Core.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ChatApi.Services.Services {
    public class ConnectionService : IConnectionService {
        private readonly IUserConnectionRepository _connectionRepository;
        private readonly IConnectionGroupRepository _groupRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public ConnectionService(
            IUserConnectionRepository connectionRepository,
            IConnectionGroupRepository groupRepository,
            UserManager<ApplicationUser> userManager) {
            _connectionRepository = connectionRepository;
            _groupRepository = groupRepository;
            _userManager = userManager;
        }

        public async Task<ServiceOperationStatus> AddUserConnectionAsync(int userId, string connectionId) {
            try {
                var connection = new UserConnection {
                    UserId = userId,
                    ConnectionId = connectionId
                };

                await _connectionRepository.AddAsync(connection);
                await UpdateUserOnlineStatusAsync(userId, true);
                return ServiceOperationStatus.Succeeded;
            }
            catch {
                return ServiceOperationStatus.Failed;
            }
        }

        public async Task<ServiceOperationStatus> RemoveUserConnectionAsync(string connectionId) {
            try {
                var connection = await _connectionRepository.GetConnectionByIdAsync(connectionId);
                if (connection == null)
                    return ServiceOperationStatus.NotFound;

                await _connectionRepository.DeleteAsync(connection);

                // Check if user has other connections
                var userConnections = await _connectionRepository.GetUserConnectionsAsync(connection.UserId);
                if (!userConnections.Any()) {
                    await UpdateUserOnlineStatusAsync(connection.UserId, false);
                }

                // Remove from all groups
                var groupConnections = await _groupRepository.GetTableNoTracking(g => g.UserConnectionId == connection.Id).ToListAsync();
                if (groupConnections.Any()) {
                    await _groupRepository.DeleteRangeAsync(groupConnections.ToList());
                }

                return ServiceOperationStatus.Succeeded;
            }
            catch {
                return ServiceOperationStatus.Failed;
            }
        }

        public async Task<ServiceOperationStatus> AddToGroupAsync(string connectionId, int conversationId) {
            try {
                var connection = await _connectionRepository.GetConnectionByIdAsync(connectionId);
                if (connection == null)
                    return ServiceOperationStatus.NotFound;

                var existingGroup = await _groupRepository.GetTableNoTracking(g => g.UserConnectionId == connection.Id && g.ConversationId == conversationId).FirstOrDefaultAsync();
                if (existingGroup != null)
                    return ServiceOperationStatus.AlreadyExists;

                var connectionGroup = new ConnectionGroup {
                    UserConnectionId = connection.Id,
                    ConversationId = conversationId,
                    JoinedAt = DateTime.UtcNow,
                    IsActive = true
                };

                await _groupRepository.AddAsync(connectionGroup);
                return ServiceOperationStatus.Succeeded;
            }
            catch {
                return ServiceOperationStatus.Failed;
            }
        }

        public async Task<ServiceOperationStatus> RemoveFromGroupAsync(string connectionId, string groupName) {
            try {
                var connection = await _connectionRepository.GetConnectionByIdAsync(connectionId);
                if (connection == null)
                    return ServiceOperationStatus.NotFound;

                // For SignalR groups, we'll use ConversationId from group name
                if (!groupName.StartsWith("Conversation_"))
                    return ServiceOperationStatus.InvalidParameters;

                var conversationIdStr = groupName.Replace("Conversation_", "");
                if (!int.TryParse(conversationIdStr, out var conversationId))
                    return ServiceOperationStatus.InvalidParameters;

                var connectionGroup = await _groupRepository.GetTableNoTracking(g => g.UserConnectionId == connection.Id && g.ConversationId == conversationId).FirstOrDefaultAsync();
                if (connectionGroup == null)
                    return ServiceOperationStatus.NotFound;

                await _groupRepository.DeleteAsync(connectionGroup);
                return ServiceOperationStatus.Succeeded;
            }
            catch {
                return ServiceOperationStatus.Failed;
            }
        }

        public async Task<IEnumerable<string>> GetUserConnectionsAsync(int userId) {
            var connections = await _connectionRepository.GetUserConnectionsAsync(userId);
            return connections.Select(c => c.ConnectionId);
        }

        public async Task<ServiceOperationStatus> UpdateUserOnlineStatusAsync(int userId, bool isOnline) {
            try {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                    return ServiceOperationStatus.NotFound;

                user.IsOnline = isOnline;
                if (!isOnline)
                    user.LastSeen = DateTime.UtcNow;

                await _userManager.UpdateAsync(user);
                return ServiceOperationStatus.Succeeded;
            }
            catch {
                return ServiceOperationStatus.Failed;
            }
        }

        public async Task<IEnumerable<int>> GetOnlineUsersAsync() {
            var onlineUsers = await _userManager.Users.Where(u => u.IsOnline).Select(u => u.Id).ToListAsync();
            return onlineUsers;
        }
    }
}