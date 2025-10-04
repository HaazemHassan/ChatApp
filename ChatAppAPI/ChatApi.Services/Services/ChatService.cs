using ChatApi.Core.Abstracts.InfrastructureAbstracts;
using ChatApi.Core.Abstracts.ServicesContracts;
using ChatApi.Core.Bases;
using ChatApi.Core.Entities.ChatEntities;
using ChatApi.Core.Entities.IdentityEntities;
using ChatApi.Core.Enums;
using ChatApi.Core.Enums.ChatEnums;
using ChatApi.Core.Features.Chat.Commands.RequestsModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ChatApi.Services.Services {
    public class ChatService : IChatService {
        private readonly IConversationRepository _conversationRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IConversationParticipantRepository _participantRepository;
        private readonly ITypingIndicatorRepository _typingIndicatorRepository;
        private readonly IMessageDeliveryRepository _messageDeliveryRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICurrentUserService _currentUserService;
        private readonly IApplicationUserService _applicationUserService;

        public ChatService(
            IConversationRepository conversationRepository,
            IMessageRepository messageRepository,
            IConversationParticipantRepository participantRepository,
            ITypingIndicatorRepository typingIndicatorRepository,
            IMessageDeliveryRepository messageDeliveryRepository,
            UserManager<ApplicationUser> userManager,
            ICurrentUserService currentUserService,
            IApplicationUserService applicationUserService) {
            _conversationRepository = conversationRepository;
            _messageRepository = messageRepository;
            _participantRepository = participantRepository;
            _typingIndicatorRepository = typingIndicatorRepository;
            _messageDeliveryRepository = messageDeliveryRepository;
            _userManager = userManager;
            _currentUserService = currentUserService;
            _applicationUserService = applicationUserService;
        }


        public async Task<ServiceOperationResult<Conversation?>> CreateConversationAsync(CreateConversationCommand request) {
            if (_currentUserService.UserId is null)
                return ServiceOperationResult<Conversation>.Failure(ServiceOperationStatus.Unauthorized, "User not authenticated");

            var currentUserId = _currentUserService.UserId.Value;

            bool isCurrentUserParticipant = request.ParticipantIds.Contains(currentUserId);
            if (!isCurrentUserParticipant)
                return ServiceOperationResult<Conversation>.Failure(ServiceOperationStatus.Forbidden, "Creator must be a participant in the conversation");

            if (request.Type == ConversationType.Direct) {

                if (request.Title is not null)
                    return ServiceOperationResult<Conversation>.Failure(ServiceOperationStatus.InvalidParameters, "Direct conversations cannot have a title");

                if (request.ParticipantIds.Count != 2)
                    return ServiceOperationResult<Conversation>.Failure(ServiceOperationStatus.InvalidParameters, "Direct conversations must have exactly two participants");
                int firstUserId = request.ParticipantIds.FirstOrDefault();
                int secondUserId = request.ParticipantIds.LastOrDefault();
                if (firstUserId == secondUserId)
                    return ServiceOperationResult<Conversation>.Failure(ServiceOperationStatus.InvalidParameters, "Cannot create a direct conversation with oneself");

                var existingConversation = await GetDirectConversationBetweenUsersAsync(firstUserId, secondUserId);
                if (existingConversation != null)
                    return ServiceOperationResult<Conversation>.Failure(ServiceOperationStatus.AlreadyExists, "Direct conversation already exists");
            }
            else {
                if (string.IsNullOrWhiteSpace(request.Title))
                    return ServiceOperationResult<Conversation>.Failure(ServiceOperationStatus.InvalidParameters, "Group conversations must have a title");

            }

            var conversation = new Conversation {
                Title = request.Title,
                Type = request.Type,
                CreatedByUserId = currentUserId
            };
            await _conversationRepository.AddAsync(conversation);

            //add participants to the conversations
            var allParticipants = new List<int>(request.ParticipantIds);
            foreach (var participantId in allParticipants) {
                var role = participantId == currentUserId ? ConversationParticipantRole.Owner : ConversationParticipantRole.Member;
                await AddParticipantAsync(conversation.Id, participantId, role);
            }

            // Get the created conversation with details included
            var createdConversation = await GetConversationByIdAsync(conversation.Id);
            return ServiceOperationResult<Conversation>.Success(createdConversation);
        }

        public async Task<ServiceOperationStatus> AddParticipantAsync(int conversationId, int userId, ConversationParticipantRole role) {
            try {
                var conversation = await GetConversationByIdAsync(conversationId);
                if (conversation == null)
                    return ServiceOperationStatus.DependencyNotExist;

                var existingParticipant = await _participantRepository.GetParticipantAsync(conversationId, userId);
                if (existingParticipant != null) {
                    if (existingParticipant.IsActive)
                        return ServiceOperationStatus.AlreadyExists;

                    existingParticipant.IsActive = true;
                    existingParticipant.JoinedAt = DateTime.UtcNow;
                    existingParticipant.LeftAt = null;
                    await _participantRepository.UpdateAsync(existingParticipant);
                    return ServiceOperationStatus.Succeeded;
                }

                var participant = new ConversationParticipant {
                    ConversationId = conversationId,
                    UserId = userId,
                    Role = role,
                    JoinedAt = DateTime.UtcNow,
                    IsActive = true
                };

                await _participantRepository.AddAsync(participant);

                //Add system messages to the group chat about the new participant

                if (conversation.Type == ConversationType.Group && role != ConversationParticipantRole.Owner) {

                    var user = await _userManager.FindByIdAsync(userId.ToString());
                    var systemMessage = new Message {
                        Content = $"@{user?.UserName} was added to the group.",
                        ConversationId = conversationId,
                        MessageType = MessageType.System,
                    };
                    await SendMessageAsync(systemMessage);
                }


                return ServiceOperationStatus.Succeeded;
            }
            catch {
                return ServiceOperationStatus.Failed;
            }
        }

        public async Task<ServiceOperationStatus> RemoveParticipantAsync(int conversationId, int userId) {
            try {
                var participant = await _participantRepository.GetParticipantAsync(conversationId, userId);
                if (participant == null || !participant.IsActive)
                    return ServiceOperationStatus.NotFound;

                participant.IsActive = false;
                participant.LeftAt = DateTime.UtcNow;
                await _participantRepository.UpdateAsync(participant);
                return ServiceOperationStatus.Succeeded;
            }
            catch {
                return ServiceOperationStatus.Failed;
            }
        }

        public async Task<ServiceOperationStatus> SendMessageAsync(Message message) {
            try {
                await _messageRepository.AddAsync(message);

                var conversation = await _conversationRepository.GetByIdAsync(message.ConversationId);
                if (conversation != null) {
                    conversation.LastMessageAt = DateTime.UtcNow;
                    await _conversationRepository.UpdateAsync(conversation);
                }

                return ServiceOperationStatus.Succeeded;
            }
            catch {
                return ServiceOperationStatus.Failed;
            }
        }

        public async Task<ServiceOperationStatus> EditMessageAsync(int messageId, string newContent) {
            try {
                var message = await _messageRepository.GetByIdAsync(messageId);
                if (message == null || message.IsDeleted)
                    return ServiceOperationStatus.NotFound;

                message.Content = newContent;
                message.EditedAt = DateTime.UtcNow;
                await _messageRepository.UpdateAsync(message);
                return ServiceOperationStatus.Succeeded;
            }
            catch {
                return ServiceOperationStatus.Failed;
            }
        }

        public async Task<ServiceOperationStatus> DeleteMessageAsync(int messageId) {
            try {
                var message = await _messageRepository.GetByIdAsync(messageId);
                if (message == null || message.IsDeleted)
                    return ServiceOperationStatus.NotFound;

                message.IsDeleted = true;
                await _messageRepository.UpdateAsync(message);
                return ServiceOperationStatus.Succeeded;
            }
            catch {
                return ServiceOperationStatus.Failed;
            }
        }

        public async Task<Conversation?> GetConversationByIdAsync(int conversationId) {
            return await _conversationRepository.GetConversationWithParticipantsAsync(conversationId);
        }

        public async Task<IEnumerable<Conversation>> GetUserConversationsAsync(int userId) {
            return await _conversationRepository.GetUserConversationsAsync(userId);
        }

        public async Task<IEnumerable<Message>> GetConversationMessagesAsync(int conversationId, int skip = 0, int take = 50) {
            return await _messageRepository.GetConversationMessagesAsync(conversationId, skip, take);
        }

        public async Task<IEnumerable<Message>> GetConversationMessagesWithDeliveryAsync(int conversationId, int userId, int skip = 0, int take = 50) {
            return await _messageRepository.GetConversationMessagesWithDeliveryAsync(conversationId, userId, skip, take);
        }

        public async Task<ServiceOperationStatus> MarkMessageAsReadAsync(int messageId, int userId) {
            var result = await MarkMessagesAsReadAsync(new List<int> { messageId }, userId);
            return result.Status;
        }

        public async Task<ServiceOperationResult<string>> MarkMessagesAsReadAsync(List<int> messageIds, int userId) {
            if (messageIds == null || !messageIds.Any())
                return ServiceOperationResult<string>.Failure(ServiceOperationStatus.InvalidParameters, "Message IDs list cannot be empty");

            var messages = await _messageRepository.GetTableNoTracking()
                .Where(m => messageIds.Contains(m.Id))
                .ToListAsync();

            if (!messages.Any())
                return ServiceOperationResult<string>.Failure(ServiceOperationStatus.NotFound, "No messages found");

            var participantsToUpdate = new List<ConversationParticipant>();
            var processedConversations = new HashSet<int>();

            foreach (var message in messages) {
                // Skip if conversation already processed (optimization for multiple messages in same conversation)
                if (processedConversations.Contains(message.ConversationId))
                    continue;

                var participant = await _participantRepository.GetParticipantAsync(message.ConversationId, userId);
                if (participant == null)
                    continue;

                // Find the latest message in this conversation from our list
                var latestMessageInConversation = messages
                    .Where(m => m.ConversationId == message.ConversationId)
                    .OrderByDescending(m => m.SentAt)
                    .First();

                participant.LastReadMessageId = latestMessageInConversation.Id;
                participant.LastReadAt = DateTime.UtcNow;
                participantsToUpdate.Add(participant);
                processedConversations.Add(message.ConversationId);
            }

            if (participantsToUpdate.Any()) {
                await _participantRepository.UpdateRangeAsync(participantsToUpdate);
            }

            return ServiceOperationResult<string>.Success("Messages marked as read successfully");
        }

        public async Task<ServiceOperationStatus> MarkMessageAsDeliveredAsync(int messageId, int userId) {
            var result = await MarkMessagesAsDeliveredAsync(new List<int> { messageId }, userId);
            return result.Status;
        }

        public async Task<ServiceOperationResult<string>> MarkMessagesAsDeliveredAsync(List<int> messageIds, int userId) {
            if (messageIds == null || !messageIds.Any())
                return ServiceOperationResult<string>.Failure(ServiceOperationStatus.InvalidParameters, "Message IDs list cannot be empty");

            var messages = await _messageRepository.GetTableNoTracking()
                .Where(m => messageIds.Contains(m.Id))
                .ToListAsync();

            if (!messages.Any())
                return ServiceOperationResult<string>.Failure(ServiceOperationStatus.NotFound, "No messages found");

            var deliveriesToAdd = new List<MessageDelivery>();
            var deliveriesToUpdate = new List<MessageDelivery>();

            foreach (var message in messages) {
                // Skip messages sent by the same user
                if (message.SenderId == userId)
                    continue;

                // Check if user is participant in the conversation
                if (!await (IsUserInConversationAsync(userId, message.ConversationId)))
                    continue;

                var existingDelivery = await _messageDeliveryRepository.GetUserMessageDeliveryAsync(message.Id, userId);

                if (existingDelivery == null) {
                    deliveriesToAdd.Add(new MessageDelivery {
                        MessageId = message.Id,
                        UserId = userId,
                        Status = DeliveryStatus.Delivered,
                        DeliveredAt = DateTime.UtcNow
                    });
                }
                else if (existingDelivery.Status == DeliveryStatus.Sent) {
                    existingDelivery.Status = DeliveryStatus.Delivered;
                    existingDelivery.DeliveredAt = DateTime.UtcNow;
                    deliveriesToUpdate.Add(existingDelivery);
                }
            }

            if (deliveriesToAdd.Any())
                await _messageDeliveryRepository.AddRangeAsync(deliveriesToAdd);

            if (deliveriesToUpdate.Any())
                await _messageDeliveryRepository.UpdateRangeAsync(deliveriesToUpdate);

            return ServiceOperationResult<string>.Success("Messages marked as delivered successfully");
        }

        public async Task<ServiceOperationStatus> UpdateTypingStatusAsync(int conversationId, int userId, bool isTyping) {
            try {
                var existingIndicator = await _typingIndicatorRepository.GetUserTypingIndicatorAsync(conversationId, userId);

                if (existingIndicator == null) {
                    if (isTyping) {
                        var indicator = new TypingIndicator {
                            ConversationId = conversationId,
                            UserId = userId,
                            IsTyping = true,
                            LastTypingAt = DateTime.UtcNow
                        };
                        await _typingIndicatorRepository.AddAsync(indicator);
                    }
                }
                else {
                    existingIndicator.IsTyping = isTyping;
                    existingIndicator.LastTypingAt = DateTime.UtcNow;
                    await _typingIndicatorRepository.UpdateAsync(existingIndicator);
                }

                return ServiceOperationStatus.Succeeded;
            }
            catch {
                return ServiceOperationStatus.Failed;
            }
        }

        public async Task<IEnumerable<TypingIndicator>> GetActiveTypingIndicatorsAsync(int conversationId) {
            return await _typingIndicatorRepository.GetActiveTypingIndicatorsAsync(conversationId);
        }

        public async Task<bool> IsUserInConversationAsync(int userId, int conversationId) {
            try {
                var participant = await _participantRepository.GetParticipantAsync(conversationId, userId);
                return participant != null && participant.IsActive;
            }
            catch {
                return false;
            }
        }

        public async Task<Message?> GetMessageWithSenderAsync(int messageId) {
            try {
                return await _messageRepository.GetMessageWithSenderAsync(messageId);
            }
            catch {
                return null;
            }
        }

        public async Task<Message?> GetMessageWithDeliveryAsync(int messageId, int userId) {
            try {
                return await _messageRepository.GetMessageWithDeliveryAsync(messageId, userId);
            }
            catch {
                return null;
            }
        }

        public async Task<Conversation?> GetDirectConversationBetweenUsersAsync(int userId1, int userId2) {
            try {
                return await _conversationRepository.GetDirectConversationBetweenUsersAsync(userId1, userId2);
            }
            catch {
                return null;
            }
        }

        public async Task<bool> HasDirectConversationWith(int user1Id, int user2Id) {
            try {
                var conversation = await _conversationRepository.GetDirectConversationBetweenUsersAsync(user1Id, user2Id);
                return conversation != null;
            }
            catch {
                return false;
            }
        }


        public async Task<Message?> GetLastMessageInConversationAsync(int conversationId) {
            return await _messageRepository.GetTableNoTracking()
                .Where(m => m.ConversationId == conversationId && !m.IsDeleted)
                .OrderByDescending(m => m.SentAt)
                .FirstOrDefaultAsync();
        }



        #region helpers
        public async Task<string> GetConversationTitle(int convesationId) {
            var conversation = await GetConversationByIdAsync(convesationId);
            if (conversation is null)
                return null;
            if (conversation.Type == ConversationType.Direct) {
                var recipient = conversation.Participants
                  .FirstOrDefault(p => p.UserId != _currentUserService.UserId);

                if (recipient == null)
                    return "Unknown User";

                return await _applicationUserService.GetFullName(recipient.UserId);
            }
            return conversation.Title;
        }

        public async Task<IEnumerable<int>> GetUndeliveredMessageIdsForUserAsync(int userId) {
            try {
                // Get all conversations where the user is a participant
                var userConversations = await GetUserConversationsAsync(userId);
                var conversationIds = userConversations.Select(c => c.Id).ToList();

                if (!conversationIds.Any())
                    return new List<int>();

                // Get all messages in user's conversations that were sent by others
                var messages = await _messageRepository.GetTableNoTracking()
                    .Where(m => conversationIds.Contains(m.ConversationId)
                               && m.SenderId != userId
                               && !m.IsDeleted)
                    .ToListAsync();

                var undeliveredMessageIds = new List<int>();

                foreach (var message in messages) {
                    // Check if there's a delivery record for this user
                    var delivery = await _messageDeliveryRepository.GetUserMessageDeliveryAsync(message.Id, userId);

                    // If no delivery record exists, or the message is still marked as "Sent"
                    if (delivery == null || delivery.Status == DeliveryStatus.Sent) {
                        undeliveredMessageIds.Add(message.Id);
                    }
                }

                return undeliveredMessageIds;
            }
            catch {
                return new List<int>();
            }
        }

        #endregion


    }
}