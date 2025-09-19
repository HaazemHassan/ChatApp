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
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICurrentUserService _currentUserService;
        private readonly IApplicationUserService _applicationUserService;

        public ChatService(
            IConversationRepository conversationRepository,
            IMessageRepository messageRepository,
            IConversationParticipantRepository participantRepository,
            ITypingIndicatorRepository typingIndicatorRepository,
            UserManager<ApplicationUser> userManager,
            ICurrentUserService currentUserService,
            IApplicationUserService applicationUserService) {
            _conversationRepository = conversationRepository;
            _messageRepository = messageRepository;
            _participantRepository = participantRepository;
            _typingIndicatorRepository = typingIndicatorRepository;
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

            return ServiceOperationResult<Conversation>.Success(conversation);


        }

        public async Task<ServiceOperationStatus> AddParticipantAsync(int conversationId, int userId, ConversationParticipantRole role) {
            try {
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

        public async Task<ServiceOperationStatus> MarkMessageAsReadAsync(int messageId, int userId) {
            try {
                var message = await _messageRepository.GetByIdAsync(messageId);
                if (message == null)
                    return ServiceOperationStatus.NotFound;

                var participant = await _participantRepository.GetParticipantAsync(message.ConversationId, userId);
                if (participant == null)
                    return ServiceOperationStatus.NotFound;

                participant.LastReadMessageId = messageId;
                participant.LastReadAt = DateTime.UtcNow;
                await _participantRepository.UpdateAsync(participant);
                return ServiceOperationStatus.Succeeded;
            }
            catch {
                return ServiceOperationStatus.Failed;
            }
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

        #endregion


    }
}