using AutoMapper;
using ChatApi.Core.Abstracts.ServicesContracts;
using ChatApi.Core.Bases;
using ChatApi.Core.Entities.ChatEntities;
using ChatApi.Core.Enums.ChatEnums;
using ChatApi.Core.Features.Chat.Queries.RequestsModels;
using ChatApi.Core.Features.Chat.Queries.Responses;
using MediatR;

namespace ChatApi.Core.Features.Chat.Queries.Handlers {
    public class ChatQueryHandler : ResponseHandler,
        IRequestHandler<GetUserConversationsQuery, Response<IEnumerable<GetUserConversationsResponse>>>,
        IRequestHandler<GetConversationMessagesQuery, Response<GetConversationMessagesResponse>>,
        IRequestHandler<GetConversationByIdQuery, Response<GetConversationByIdResponse>>,
        IRequestHandler<GetNewConversationQuery, Response<GetNewConversationResponse>>,
        IRequestHandler<GetTypingIndicatorsQuery, Response<IEnumerable<GetTypingIndicatorsResponse>>> {

        private readonly IChatService _chatService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;
        private IApplicationUserService _applicationUserService;

        public ChatQueryHandler(IChatService chatService, ICurrentUserService currentUserService, IMapper mapper, IApplicationUserService applicationUserService) {
            _chatService = chatService;
            _currentUserService = currentUserService;
            _mapper = mapper;
            _applicationUserService = applicationUserService;
        }

        public async Task<Response<IEnumerable<GetUserConversationsResponse>>> Handle(GetUserConversationsQuery request, CancellationToken cancellationToken) {
            if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
                return Unauthorized<IEnumerable<GetUserConversationsResponse>>("User not authenticated");

            var currentUserId = _currentUserService.UserId.Value;

            var conversations = await _chatService.GetUserConversationsAsync(currentUserId);
            var conversationResponses = _mapper.Map<IEnumerable<GetUserConversationsResponse>>(conversations);

            foreach (var conversationResponse in conversationResponses) {
                conversationResponse.Title = await _chatService.GetConversationTitle(conversationResponse.Id);
                var lastMessage = await _chatService.GetLastMessageInConversationAsync(conversationResponse.Id);
                if (lastMessage != null) {
                    var lastMessageResponse = _mapper.Map<MessageResponse>(lastMessage);
                    lastMessageResponse.DeliveryStatus = CalculateDeliveryStatus(lastMessage);
                    conversationResponse.LastMessage = lastMessageResponse;
                }
            }

            return Success(conversationResponses);
        }

        public async Task<Response<GetConversationMessagesResponse>> Handle(GetConversationMessagesQuery request, CancellationToken cancellationToken) {
            if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
                return Unauthorized<GetConversationMessagesResponse>("User not authenticated");

            var currentUserId = _currentUserService.UserId.Value;

            var conversation = await _chatService.GetConversationByIdAsync(request.ConversationId);
            if (conversation == null)
                return NotFound<GetConversationMessagesResponse>("Conversation not found");

            var messages = await _chatService.GetConversationMessagesWithDeliveryAsync(request.ConversationId, currentUserId, request.Skip, request.Take);
            var messageResponses = _mapper.Map<IEnumerable<MessageResponse>>(messages).ToList();

            // Calculate delivery status for each message
            foreach (var messageResponse in messageResponses) {
                var message = messages.FirstOrDefault(m => m.Id == messageResponse.Id);
                if (message != null) {
                    messageResponse.DeliveryStatus = CalculateDeliveryStatus(message);
                }
            }

            var response = new GetConversationMessagesResponse {
                ConversationId = request.ConversationId,
                ConversationTitle = await _chatService.GetConversationTitle(conversation.Id),
                Messages = messageResponses,
            };

            return Success(response);
        }

        public async Task<Response<GetConversationByIdResponse>> Handle(GetConversationByIdQuery request, CancellationToken cancellationToken) {
            if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
                return Unauthorized<GetConversationByIdResponse>("User not authenticated");

            var currentUserId = _currentUserService.UserId.Value;

            var conversation = await _chatService.GetConversationByIdAsync(request.ConversationId);
            if (conversation == null)
                return NotFound<GetConversationByIdResponse>("Conversation not found");

            var conversationResponse = _mapper.Map<GetConversationByIdResponse>(conversation);

            conversationResponse.Title = await _chatService.GetConversationTitle(conversationResponse.Id);
            var lastMessage = await _chatService.GetLastMessageInConversationAsync(conversation.Id);
            if (lastMessage != null) {
                var lastMessageResponse = _mapper.Map<MessageResponse>(lastMessage);
                lastMessageResponse.DeliveryStatus = CalculateDeliveryStatus(lastMessage);
                conversationResponse.LastMessage = lastMessageResponse;
            }
            return Success(conversationResponse);
        }

        public async Task<Response<IEnumerable<GetTypingIndicatorsResponse>>> Handle(GetTypingIndicatorsQuery request, CancellationToken cancellationToken) {
            if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
                return Unauthorized<IEnumerable<GetTypingIndicatorsResponse>>("User not authenticated");

            var indicators = await _chatService.GetActiveTypingIndicatorsAsync(request.ConversationId);
            var indicatorResponses = _mapper.Map<IEnumerable<GetTypingIndicatorsResponse>>(indicators);

            return Success(indicatorResponses);
        }

        public async Task<Response<GetNewConversationResponse>> Handle(GetNewConversationQuery request, CancellationToken cancellationToken) {
            if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
                return Unauthorized<GetNewConversationResponse>("User not authenticated");

            if (request.Username == _currentUserService.UserName)
                return NotFound<GetNewConversationResponse>("Other user not found");

            var currentUser = await _currentUserService.GetCurrentUserAsync();
            var otherUser = await _applicationUserService.GetByUsernameAsync(request.Username);

            if (otherUser is null || currentUser is null)
                return NotFound<GetNewConversationResponse>("Other user not found");

            Conversation? conversation = await _chatService.GetDirectConversationBetweenUsersAsync(currentUser.Id, otherUser.Id);

            GetNewConversationResponse newConversationResponse;
            if (conversation is not null) {
                newConversationResponse = _mapper.Map<GetNewConversationResponse>(conversation);
                newConversationResponse.Title = await _chatService.GetConversationTitle(conversation.Id);
            }
            else {
                newConversationResponse = new GetNewConversationResponse {
                    Id = null,
                    Title = otherUser.FullName,
                    Type = ConversationType.Direct,
                    LastMessage = null,
                    Participants = new List<ConversationParticipantResponse> {
                        new ConversationParticipantResponse {
                            Id = null,
                            UserId = currentUser.Id,
                            UserName = currentUser.UserName,
                            FullName = currentUser.FullName,
                            IsOnline = currentUser.IsOnline
                        },
                        new ConversationParticipantResponse {
                            Id = null,
                            UserId = otherUser.Id,
                            UserName = otherUser.UserName,
                            FullName = otherUser.FullName,
                            IsOnline = otherUser.IsOnline
                        }
                    }
                };
            }
            return Success(newConversationResponse);
        }

        #region Helpers
        private static DeliveryStatus CalculateDeliveryStatus(Message message) {
            // If no deliveries exist, message is just sent
            if (message.MessageDeliveries == null || !message.MessageDeliveries.Any())
                return DeliveryStatus.Sent;

            // Get the conversation to know how many participants (excluding sender)
            var conversation = message.Conversation;
            if (conversation == null)
                return DeliveryStatus.Sent;

            // Count active participants excluding the sender
            var expectedRecipients = conversation.Participants
                .Count(p => p.IsActive && p.UserId != message.SenderId);

            // If there are no other participants, mark as delivered
            if (expectedRecipients == 0)
                return DeliveryStatus.Delivered;

            var deliveries = message.MessageDeliveries.ToList();
            var deliveryCount = deliveries.Count;

            // If not all recipients have delivery records yet, it's still "Sent"
            if (deliveryCount < expectedRecipients)
                return DeliveryStatus.Sent;

            // Check if all recipients have read the message
            var allRead = deliveries.All(d => d.Status == DeliveryStatus.Read);
            if (allRead)
                return DeliveryStatus.Read;

            // Check if all recipients have at least received the message
            var allDelivered = deliveries.All(d => d.Status >= DeliveryStatus.Delivered);
            if (allDelivered)
                return DeliveryStatus.Delivered;

            // Otherwise, it's still just sent
            return DeliveryStatus.Sent;
        }
        #endregion
    }
}









