using AutoMapper;
using ChatApi.Core.Abstracts.InfrastructureAbstracts;
using ChatApi.Core.Abstracts.ServicesContracts;
using ChatApi.Core.Bases;
using ChatApi.Core.Entities.ChatEntities;
using ChatApi.Core.Enums;
using ChatApi.Core.Features.Chat.Commands.RequestsModels;
using ChatApi.Core.Features.Chat.Commands.Responses;
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
        IRequestHandler<MarkMessageAsReadCommand, Response<string>> {

        private readonly IChatService _chatService;
        private readonly IConnectionService _connectionService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IApplicationUserService _applicationUserService;
        private readonly IMapper _mapper;
        private readonly IGenericRepository<Conversation> _conversationRepository;   //used only to create transactions

        public ChatCommandHandler(
            IChatService chatService,
            IConnectionService connectionService,
            ICurrentUserService currentUserService,
            IMapper mapper,
            IApplicationUserService applicationUserService,
            IGenericRepository<Conversation> conversationRepository) {
            _chatService = chatService;
            _connectionService = connectionService;
            _currentUserService = currentUserService;
            _mapper = mapper;
            _applicationUserService = applicationUserService;
            _conversationRepository = conversationRepository;
        }


        public async Task<Response<CreateConversationResponse>> Handle(CreateConversationCommand request, CancellationToken cancellationToken) {

            await using var transaction = await _conversationRepository.BeginTransactionAsync(cancellationToken);
            try {
                var result = await _chatService.CreateConversationAsync(request);

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
                responseModel.Title = await _chatService.GetConversationTitle(conversation.Id);

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

            var isParticipant = await _chatService.IsUserInConversationAsync(senderId, request.ConversationId);
            if (!isParticipant)
                return Forbid<SendMessageResponse>("User not authorized to send messages in this conversation");

            var message = new Message {
                ConversationId = request.ConversationId,
                SenderId = senderId,
                Content = request.Content,
                MessageType = request.MessageType,
                ReplyToMessageId = request.ReplyToMessageId
            };

            var result = await _chatService.SendMessageAsync(message);
            if (result != ServiceOperationStatus.Succeeded) {
                return result switch {
                    ServiceOperationStatus.NotFound => NotFound<SendMessageResponse>("Conversation not found"),
                    _ => BadRequest<SendMessageResponse>("Failed to send message")
                };
            }

            // Get the message with sender information for the response meta
            var messageWithSender = await _chatService.GetMessageWithSenderAsync(message.Id);
            var responseModel = _mapper.Map<SendMessageResponse>(messageWithSender);
            return Success(responseModel);


        }



        public async Task<Response<string>> Handle(EditMessageCommand request, CancellationToken cancellationToken) {
            if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
                return Unauthorized<string>("User not authenticated");

            var result = await _chatService.EditMessageAsync(request.MessageId, request.NewContent);
            return result switch {
                ServiceOperationStatus.Succeeded => Success("Message edited successfully"),
                ServiceOperationStatus.NotFound => NotFound<string>("Message not found"),
                _ => BadRequest<string>("Failed to edit message")
            };
        }

        public async Task<Response<string>> Handle(DeleteMessageCommand request, CancellationToken cancellationToken) {
            var result = await _chatService.DeleteMessageAsync(request.MessageId);
            return result switch {
                ServiceOperationStatus.Succeeded => Success("Message deleted successfully"),
                ServiceOperationStatus.NotFound => NotFound<string>("Message not found"),
                _ => BadRequest<string>("Failed to delete message")
            };
        }

        public async Task<Response<string>> Handle(AddParticipantCommand request, CancellationToken cancellationToken) {
            if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
                return Unauthorized<string>("User not authenticated");

            var result = await _chatService.AddParticipantAsync(request.ConversationId, request.UserId, request.Role);
            return result switch {
                ServiceOperationStatus.Succeeded => Success("Participant added successfully"),
                ServiceOperationStatus.AlreadyExists => BadRequest<string>("User is already a participant"),
                ServiceOperationStatus.NotFound => NotFound<string>("Conversation not found"),
                _ => BadRequest<string>("Failed to add participant")
            };
        }

        public async Task<Response<string>> Handle(RemoveParticipantCommand request, CancellationToken cancellationToken) {
            var result = await _chatService.RemoveParticipantAsync(request.ConversationId, request.UserId);
            return result switch {
                ServiceOperationStatus.Succeeded => Success("Participant removed successfully"),
                ServiceOperationStatus.NotFound => NotFound<string>("Participant not found"),
                _ => BadRequest<string>("Failed to remove participant")
            };
        }

        public async Task<Response<string>> Handle(UpdateTypingStatusCommand request, CancellationToken cancellationToken) {
            if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
                return Unauthorized<string>("User not authenticated");

            var userId = _currentUserService.UserId.Value;
            var result = await _chatService.UpdateTypingStatusAsync(request.ConversationId, userId, request.IsTyping);
            return result switch {
                ServiceOperationStatus.Succeeded => Success("Typing status updated successfully"),
                _ => BadRequest<string>("Failed to update typing status")
            };
        }

        public async Task<Response<string>> Handle(MarkMessageAsReadCommand request, CancellationToken cancellationToken) {
            if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
                return Unauthorized<string>("User not authenticated");

            var userId = _currentUserService.UserId.Value;
            var result = await _chatService.MarkMessageAsReadAsync(request.MessageId, userId);
            return result switch {
                ServiceOperationStatus.Succeeded => Success("Message marked as read"),
                ServiceOperationStatus.NotFound => NotFound<string>("Message not found"),
                _ => BadRequest<string>("Failed to mark message as read")
            };
        }
    }
}