using ChatApi.Core.Abstracts.InfrastructureAbstracts;
using ChatApi.Core.Abstracts.ServicesContracts;
using ChatApi.Core.Bases;
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

        public async Task<ServiceOperationResult<string?>> AddUserConnectionAsync(int userId, string connectionId) {
            try {
                var connection = new UserConnection {
                    UserId = userId,
                    ConnectionId = connectionId
                };

                await _connectionRepository.AddAsync(connection);
                await UpdateUserOnlineStatusAsync(userId, true);
                return ServiceOperationResult<string>.Success("Connection added successfully");
            }
            catch (Exception ex) {
                return ServiceOperationResult<string>.Failure(ServiceOperationStatus.Failed, $"Failed to add connection: {ex.Message}");
            }
        }

        public async Task<ServiceOperationResult<string?>> RemoveUserConnectionAsync(string connectionId) {
            try {
                var connection = await _connectionRepository.GetConnectionByIdAsync(connectionId);
                if (connection == null)
                    return ServiceOperationResult<string>.Failure(ServiceOperationStatus.NotFound, "Connection not found");

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

                return ServiceOperationResult<string>.Success("Connection removed successfully");
            }
            catch (Exception ex) {
                return ServiceOperationResult<string>.Failure(ServiceOperationStatus.Failed, $"Failed to remove connection: {ex.Message}");
            }
        }

        public async Task<ServiceOperationResult<string?>> AddToGroupAsync(string connectionId, int conversationId) {
            try {
                var connection = await _connectionRepository.GetConnectionByIdAsync(connectionId);
                if (connection == null)
                    return ServiceOperationResult<string>.Failure(ServiceOperationStatus.NotFound, "Connection not found");

                var existingGroup = await _groupRepository.GetTableNoTracking(g => g.UserConnectionId == connection.Id && g.ConversationId == conversationId).FirstOrDefaultAsync();
                if (existingGroup != null)
                    return ServiceOperationResult<string>.Failure(ServiceOperationStatus.AlreadyExists, "Connection already in group");

                var connectionGroup = new ConnectionGroup {
                    UserConnectionId = connection.Id,
                    ConversationId = conversationId,
                    JoinedAt = DateTime.UtcNow,
                    IsActive = true
                };

                await _groupRepository.AddAsync(connectionGroup);
                return ServiceOperationResult<string>.Success("Added to group successfully");
            }
            catch (Exception ex) {
                return ServiceOperationResult<string>.Failure(ServiceOperationStatus.Failed, $"Failed to add to group: {ex.Message}");
            }
        }

        public async Task<ServiceOperationResult<string?>> RemoveFromGroupAsync(string connectionId, string groupName) {
            try {
                var connection = await _connectionRepository.GetConnectionByIdAsync(connectionId);
                if (connection == null)
                    return ServiceOperationResult<string>.Failure(ServiceOperationStatus.NotFound, "Connection not found");

                // For SignalR groups, we'll use ConversationId from group name
                if (!groupName.StartsWith("Conversation_"))
                    return ServiceOperationResult<string>.Failure(ServiceOperationStatus.InvalidParameters, "Invalid group name format");

                var conversationIdStr = groupName.Replace("Conversation_", "");
                if (!int.TryParse(conversationIdStr, out var conversationId))
                    return ServiceOperationResult<string>.Failure(ServiceOperationStatus.InvalidParameters, "Invalid conversation ID");

                var connectionGroup = await _groupRepository.GetTableNoTracking(g => g.UserConnectionId == connection.Id && g.ConversationId == conversationId).FirstOrDefaultAsync();
                if (connectionGroup == null)
                    return ServiceOperationResult<string>.Failure(ServiceOperationStatus.NotFound, "Group connection not found");

                await _groupRepository.DeleteAsync(connectionGroup);
                return ServiceOperationResult<string>.Success("Removed from group successfully");
            }
            catch (Exception ex) {
                return ServiceOperationResult<string>.Failure(ServiceOperationStatus.Failed, $"Failed to remove from group: {ex.Message}");
            }
        }

        public async Task<IEnumerable<string>> GetUserConnectionsAsync(int userId) {
            var connections = await _connectionRepository.GetUserConnectionsAsync(userId);
            return connections.Select(c => c.ConnectionId);
        }

        public async Task<ServiceOperationResult<string?>> UpdateUserOnlineStatusAsync(int userId, bool isOnline) {
            try {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                    return ServiceOperationResult<string>.Failure(ServiceOperationStatus.NotFound, "User not found");

                user.IsOnline = isOnline;
                if (!isOnline)
                    user.LastSeen = DateTime.UtcNow;

                await _userManager.UpdateAsync(user);
                return ServiceOperationResult<string>.Success("User online status updated successfully");
            }
            catch (Exception ex) {
                return ServiceOperationResult<string>.Failure(ServiceOperationStatus.Failed, $"Failed to update online status: {ex.Message}");
            }
        }

        public async Task<IEnumerable<int>> GetOnlineUsersAsync() {
            var onlineUsers = await _userManager.Users.Where(u => u.IsOnline).Select(u => u.Id).ToListAsync();
            return onlineUsers;
        }
    }
}