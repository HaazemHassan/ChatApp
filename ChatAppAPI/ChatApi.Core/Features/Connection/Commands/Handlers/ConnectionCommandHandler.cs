using ChatApi.Core.Abstracts.ServicesContracts;
using ChatApi.Core.Bases;
using ChatApi.Core.Enums;
using ChatApi.Core.Features.Connection.Commands.RequestsModels;
using MediatR;

namespace ChatApi.Core.Features.Connection.Commands.Handlers {
    public class ConnectionCommandHandler : ResponseHandler,
        IRequestHandler<AddUserConnectionCommand, Response<string>>,
        IRequestHandler<RemoveUserConnectionCommand, Response<string>>,
        IRequestHandler<JoinGroupCommand, Response<string>> {

        private readonly IConnectionService _connectionService;

        public ConnectionCommandHandler(IConnectionService connectionService) {
            _connectionService = connectionService;
        }

        public async Task<Response<string>> Handle(AddUserConnectionCommand request, CancellationToken cancellationToken) {
            var result = await _connectionService.AddUserConnectionAsync(request.UserId, request.ConnectionId);
            return result.Status switch {
                ServiceOperationStatus.Succeeded => Success("Connection added successfully"),
                ServiceOperationStatus.AlreadyExists => BadRequest<string>("Connection already exists"),
                _ => BadRequest<string>(result.ErrorMessage ?? "Failed to add connection")
            };
        }

        public async Task<Response<string>> Handle(RemoveUserConnectionCommand request, CancellationToken cancellationToken) {
            var result = await _connectionService.RemoveUserConnectionAsync(request.ConnectionId);
            return result.Status switch {
                ServiceOperationStatus.Succeeded => Success("Connection removed successfully"),
                ServiceOperationStatus.NotFound => NotFound<string>("Connection not found"),
                _ => BadRequest<string>(result.ErrorMessage ?? "Failed to remove connection")
            };
        }

        public async Task<Response<string>> Handle(JoinGroupCommand request, CancellationToken cancellationToken) {
            // Extract conversation ID from group name if it's in the format "Conversation_{id}"
            if (request.GroupName.StartsWith("Conversation_")) {
                var conversationIdStr = request.GroupName.Replace("Conversation_", "");
                if (int.TryParse(conversationIdStr, out var conversationId)) {
                    var result = await _connectionService.AddToGroupAsync(request.ConnectionId, conversationId);
                    return result.Status switch {
                        ServiceOperationStatus.Succeeded => Success("Joined group successfully"),
                        ServiceOperationStatus.AlreadyExists => BadRequest<string>("Already in group"),
                        _ => BadRequest<string>(result.ErrorMessage ?? "Failed to join group")
                    };
                }
            }

            return BadRequest<string>("Invalid group name format");
        }
    }
}