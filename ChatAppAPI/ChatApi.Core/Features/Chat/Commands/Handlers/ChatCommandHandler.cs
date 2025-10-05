using AutoMapper;
using ChatApi.Core.Abstracts.InfrastructureAbstracts;
using ChatApi.Core.Abstracts.ServicesContracts;
using ChatApi.Core.Bases;
using ChatApi.Core.Entities.ChatEntities;
using ChatApi.Core.Enums;
using ChatApi.Core.Features.Chat.Commands.RequestsModels;
using ChatApi.Core.Features.Chat.Commands.Responses;
using ChatApi.Core.Features.Chat.Queries.Responses;
using MediatR;

namespace ChatApi.Core.Features.Chat.Commands.Handlers {
    public class ChatCommandHandler : ResponseHandler,
        IRequestHandler<CreateConversationCommand, Response<CreateConversationResponse>>,
        IRequestHandler<SendMessageCommand, Response<SendMessageResponse>>,
        IRequestHandler<EditMessageCommand, Response<string>>,
        IRequestHandler<DeleteMessageCommand, Response<string>>,
        IRequestHandler<AddParticipantCommand, Response<string>>,
        IRequestHandler<RemoveParticipantCommand, Response<string>>,
        IRequestHandler<UpdateTypingStatusCommand, Response<string>>,
        IRequestHandler<MarkMultipleMessagesAsReadCommand, Response<string>>,
        IRequestHandler<MarkMessagesAsDeliveredCommand, Response<string>> {

        private readonly IMessagesService _messagesService;
        private readonly IConversationsService _conversationsService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;
        private readonly IGenericRepository<Conversation> _conversationRepository;

        public ChatCommandHandler(
            IMessagesService messagesService,
            IConversationsService conversationsService,
            IConnectionService connectionService,
            ICurrentUserService currentUserService,
            IMapper mapper,
            IApplicationUserService applicationUserService,
            IGenericRepository<Conversation> conversationRepository) {
            _messagesService = messagesService;
            _conversationsService = conversationsService;
            _currentUserService = currentUserService;
            _mapper = mapper;
            _conversationRepository = conversationRepository;
        }


        public async Task<Response<CreateConversationResponse>> Handle(CreateConversationCommand request, CancellationToken cancellationToken) {

            await using var transaction = await _conversationRepository.BeginTransactionAsync(cancellationToken);
            try {
                var result = await _conversationsService.CreateConversationAsync(request);

                if (result.Status != ServiceOperationStatus.Succeeded) {
                    await transaction.RollbackAsync(cancellationToken);

                    return result.Status switch {
                        ServiceOperationStatus.Unauthorized =>
                            Unauthorized<CreateConversationResponse>(result.ErrorMessage),

                        ServiceOperationStatus.Forbidden =>
                            Forbid<CreateConversationResponse>(result.ErrorMessage),

                        ServiceOperationStatus.InvalidParameters or ServiceOperationStatus.AlreadyExists =>
                            BadRequest<CreateConversationResponse>(result.ErrorMessage),

                        _ => BadRequest<CreateConversationResponse>(result.ErrorMessage ?? "Failed to create conversation")
                    };
                }

                var conversation = result.Data;

                await transaction.CommitAsync(cancellationToken);

                var responseModel = _mapper.Map<CreateConversationResponse>(conversation);
                responseModel.Title = await _conversationsService.GetConversationTitle(conversation.Id);
                var lastMessage = await _messagesService.GetLastMessageInConversationAsync(conversation.Id);
                if (lastMessage != null) {
                    var lastMessageResponse = _mapper.Map<MessageResponse>(lastMessage);
                    lastMessageResponse.DeliveryStatus = _messagesService.GetDeliveryStatus(lastMessage);
                    responseModel.LastMessage = lastMessageResponse;
                }

                return Success(responseModel);
            }
            catch (Exception ex) {
                await transaction.RollbackAsync(cancellationToken);
                return BadRequest<CreateConversationResponse>($"Unexpected error: {ex.Message}");
            }
        }

        public async Task<Response<SendMessageResponse>> Handle(SendMessageCommand request, CancellationToken cancellationToken) {
            if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
                return Unauthorized<SendMessageResponse>("User not authenticated");

            var senderId = _currentUserService.UserId.Value;

            var isParticipant = await _conversationsService.IsUserInConversationAsync(senderId, request.ConversationId);
            if (!isParticipant)
                return Forbid<SendMessageResponse>("User not authorized to send messages in this conversation");

            var message = new Message {
                ConversationId = request.ConversationId,
                SenderId = senderId,
                Content = request.Content,
                MessageType = request.MessageType,
                ReplyToMessageId = request.ReplyToMessageId
            };

            var result = await _messagesService.SendMessageAsync(message);
            if (result.Status != ServiceOperationStatus.Succeeded) {
                return result.Status switch {
                    ServiceOperationStatus.NotFound => NotFound<SendMessageResponse>(result.ErrorMessage ?? "Conversation not found"),
                    _ => BadRequest<SendMessageResponse>(result.ErrorMessage ?? "Failed to send message")
                };
            }

            var messageResponseData = await _messagesService.GetMessageWithDeliveryAsync(message.Id, senderId);
            var responseModel = _mapper.Map<SendMessageResponse>(messageResponseData);
            responseModel.DeliveryStatus = _messagesService.GetDeliveryStatus(messageResponseData);
            return Success(responseModel);

        }



        public async Task<Response<string>> Handle(EditMessageCommand request, CancellationToken cancellationToken) {
            if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
                return Unauthorized<string>("User not authenticated");

            var result = await _messagesService.EditMessageAsync(request.MessageId, request.NewContent);
            return result.Status switch {
                ServiceOperationStatus.Succeeded => Success(result.Data ?? "Message edited successfully"),
                ServiceOperationStatus.NotFound => NotFound<string>(result.ErrorMessage ?? "Message not found"),
                _ => BadRequest<string>(result.ErrorMessage ?? "Failed to edit message")
            };
        }

        public async Task<Response<string>> Handle(DeleteMessageCommand request, CancellationToken cancellationToken) {
            var result = await _messagesService.DeleteMessageAsync(request.MessageId);
            return result.Status switch {
                ServiceOperationStatus.Succeeded => Success(result.Data ?? "Message deleted successfully"),
                ServiceOperationStatus.NotFound => NotFound<string>(result.ErrorMessage ?? "Message not found"),
                _ => BadRequest<string>(result.ErrorMessage ?? "Failed to delete message")
            };
        }

        public async Task<Response<string>> Handle(AddParticipantCommand request, CancellationToken cancellationToken) {
            if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
                return Unauthorized<string>("User not authenticated");

            var result = await _conversationsService.AddParticipantAsync(request.ConversationId, request.UserId, request.Role);
            return result.Status switch {
                ServiceOperationStatus.Succeeded => Success(result.Data ?? "Participant added successfully"),
                ServiceOperationStatus.AlreadyExists => BadRequest<string>(result.ErrorMessage ?? "User is already a participant"),
                ServiceOperationStatus.NotFound => NotFound<string>(result.ErrorMessage ?? "Conversation not found"),
                _ => BadRequest<string>(result.ErrorMessage ?? "Failed to add participant")
            };
        }

        public async Task<Response<string>> Handle(RemoveParticipantCommand request, CancellationToken cancellationToken) {
            var result = await _conversationsService.RemoveParticipantAsync(request.ConversationId, request.UserId);
            return result.Status switch {
                ServiceOperationStatus.Succeeded => Success(result.Data ?? "Participant removed successfully"),
                ServiceOperationStatus.NotFound => NotFound<string>(result.ErrorMessage ?? "Participant not found"),
                _ => BadRequest<string>(result.ErrorMessage ?? "Failed to remove participant")
            };
        }

        public async Task<Response<string>> Handle(UpdateTypingStatusCommand request, CancellationToken cancellationToken) {
            if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
                return Unauthorized<string>("User not authenticated");

            var userId = _currentUserService.UserId.Value;
            var result = await _conversationsService.UpdateTypingStatusAsync(request.ConversationId, userId, request.IsTyping);
            return result.Status switch {
                ServiceOperationStatus.Succeeded => Success(result.Data ?? "Typing status updated successfully"),
                _ => BadRequest<string>(result.ErrorMessage ?? "Failed to update typing status")
            };
        }



        public async Task<Response<string>> Handle(MarkMultipleMessagesAsReadCommand request, CancellationToken cancellationToken) {
            if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
                return Unauthorized<string>("User not authenticated");

            await using var transaction = await _conversationRepository.BeginTransactionAsync(cancellationToken);
            try {
                var userId = _currentUserService.UserId.Value;
                var result = await _messagesService.MarkMessagesAsReadAsync(request.MessageIds, userId);

                if (result.Status != ServiceOperationStatus.Succeeded) {
                    await transaction.RollbackAsync(cancellationToken);
                    return result.Status switch {
                        ServiceOperationStatus.NotFound => NotFound<string>(result.ErrorMessage ?? "Messages not found"),
                        ServiceOperationStatus.InvalidParameters => BadRequest<string>(result.ErrorMessage ?? "Invalid parameters"),
                        _ => BadRequest<string>(result.ErrorMessage ?? "Failed to mark messages as read")
                    };
                }

                await transaction.CommitAsync(cancellationToken);
                return Success(result.Data ?? "Messages marked as read successfully");
            }
            catch (Exception ex) {
                await transaction.RollbackAsync(cancellationToken);
                return BadRequest<string>($"Unexpected error: {ex.Message}");
            }
        }

        public async Task<Response<string>> Handle(MarkMessagesAsDeliveredCommand request, CancellationToken cancellationToken) {
            if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
                return Unauthorized<string>("User not authenticated");

            await using var transaction = await _conversationRepository.BeginTransactionAsync(cancellationToken);
            try {
                var userId = _currentUserService.UserId.Value;
                var result = await _messagesService.MarkMessagesAsDeliveredAsync(request.MessageIds, userId);

                if (result.Status != ServiceOperationStatus.Succeeded) {
                    await transaction.RollbackAsync(cancellationToken);
                    return result.Status switch {
                        ServiceOperationStatus.NotFound => NotFound<string>(result.ErrorMessage ?? "Messages not found"),
                        ServiceOperationStatus.InvalidParameters => BadRequest<string>(result.ErrorMessage ?? "Invalid parameters"),
                        _ => BadRequest<string>(result.ErrorMessage ?? "Failed to mark messages as delivered")
                    };
                }

                await transaction.CommitAsync(cancellationToken);
                return Success(result.Data ?? "Messages marked as delivered successfully");
            }
            catch (Exception ex) {
                await transaction.RollbackAsync(cancellationToken);
                return BadRequest<string>($"Unexpected error: {ex.Message}");
            }
        }
    }
}