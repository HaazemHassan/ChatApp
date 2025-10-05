using ChatApi.Core.Abstracts.InfrastructureAbstracts;
using ChatApi.Core.Abstracts.ServicesContracts;
using ChatApi.Core.Bases;
using ChatApi.Core.Entities.ChatEntities;
using ChatApi.Core.Entities.IdentityEntities;
using ChatApi.Core.Enums;
using ChatApi.Core.Enums.ChatEnums;
using ChatApi.Core.Features.Chat.Commands.RequestsModels;
using Microsoft.AspNetCore.Identity;

namespace ChatApi.Services.Services {
    public class ConversationsService : IConversationsService {
        private readonly IConversationRepository _conversationRepository;
        private readonly IConversationParticipantRepository _participantRepository;
        private readonly ITypingIndicatorRepository _typingIndicatorRepository;
        private readonly IMessagesService _messagesService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICurrentUserService _currentUserService;
        private readonly IApplicationUserService _applicationUserService;

        public ConversationsService(
            IConversationRepository conversationRepository,
            IConversationParticipantRepository participantRepository,
            ITypingIndicatorRepository typingIndicatorRepository,
            IMessagesService messagesService,
            UserManager<ApplicationUser> userManager,
            ICurrentUserService currentUserService,
            IApplicationUserService applicationUserService) {
            _conversationRepository = conversationRepository;
            _participantRepository = participantRepository;
            _typingIndicatorRepository = typingIndicatorRepository;
            _messagesService = messagesService;
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

        public async Task<ServiceOperationResult<string?>> AddParticipantAsync(int conversationId, int userId, ConversationParticipantRole role) {
            try {
                var conversation = await GetConversationByIdAsync(conversationId);
                if (conversation == null)
                    return ServiceOperationResult<string>.Failure(ServiceOperationStatus.DependencyNotExist, "Conversation not found");

                var existingParticipant = await _participantRepository.GetParticipantAsync(conversationId, userId);
                if (existingParticipant != null) {
                    if (existingParticipant.IsActive)
                        return ServiceOperationResult<string>.Failure(ServiceOperationStatus.AlreadyExists, "User is already a participant");

                    existingParticipant.IsActive = true;
                    existingParticipant.JoinedAt = DateTime.UtcNow;
                    existingParticipant.LeftAt = null;
                    await _participantRepository.UpdateAsync(existingParticipant);
                    return ServiceOperationResult<string>.Success("Participant re-added successfully");
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
                    await _messagesService.SendMessageAsync(systemMessage);
                }

                return ServiceOperationResult<string>.Success("Participant added successfully");
            }
            catch {
                return ServiceOperationResult<string>.Failure(ServiceOperationStatus.Failed, "Failed to add participant");
            }
        }

        public async Task<ServiceOperationResult<string?>> RemoveParticipantAsync(int conversationId, int userId) {
            try {
                var participant = await _participantRepository.GetParticipantAsync(conversationId, userId);
                if (participant == null || !participant.IsActive)
                    return ServiceOperationResult<string>.Failure(ServiceOperationStatus.NotFound, "Participant not found");

                participant.IsActive = false;
                participant.LeftAt = DateTime.UtcNow;
                await _participantRepository.UpdateAsync(participant);
                return ServiceOperationResult<string>.Success("Participant removed successfully");
            }
            catch {
                return ServiceOperationResult<string>.Failure(ServiceOperationStatus.Failed, "Failed to remove participant");
            }
        }

        public async Task<Conversation?> GetConversationByIdAsync(int conversationId) {
            return await _conversationRepository.GetConversationWithParticipantsAsync(conversationId);
        }

        public async Task<IEnumerable<Conversation>> GetUserConversationsAsync(int userId) {
            return await _conversationRepository.GetUserConversationsAsync(userId);
        }

        public async Task<ServiceOperationResult<string?>> UpdateTypingStatusAsync(int conversationId, int userId, bool isTyping) {
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

                return ServiceOperationResult<string>.Success("Typing status updated successfully");
            }
            catch {
                return ServiceOperationResult<string>.Failure(ServiceOperationStatus.Failed, "Failed to update typing status");
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

        public async Task<string> GetConversationTitle(int conversationId) {
            var conversation = await GetConversationByIdAsync(conversationId);
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
    }
}
