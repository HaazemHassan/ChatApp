using ChatApi.Core.Abstracts.InfrastructureAbstracts;
using ChatApi.Core.Abstracts.ServicesContracts;
using ChatApi.Core.Bases;
using ChatApi.Core.Entities.ChatEntities;
using ChatApi.Core.Enums;
using ChatApi.Core.Enums.ChatEnums;
using Microsoft.EntityFrameworkCore;

namespace ChatApi.Services.Services {
    public class MessagesService : IMessagesService {
        private readonly IMessageRepository _messageRepository;
        private readonly IMessageDeliveryRepository _messageDeliveryRepository;
        private readonly IConversationRepository _conversationRepository;
        private readonly IConversationParticipantRepository _participantRepository;

        public MessagesService(
            IMessageRepository messageRepository,
            IMessageDeliveryRepository messageDeliveryRepository,
            IConversationRepository conversationRepository,
            IConversationParticipantRepository participantRepository) {
            _messageRepository = messageRepository;
            _messageDeliveryRepository = messageDeliveryRepository;
            _conversationRepository = conversationRepository;
            _participantRepository = participantRepository;
        }

        public async Task<ServiceOperationResult<string?>> SendMessageAsync(Message message) {
            try {
                await _messageRepository.AddAsync(message);

                var conversation = await _conversationRepository.GetByIdAsync(message.ConversationId);
                if (conversation != null) {
                    conversation.LastMessageAt = DateTime.UtcNow;
                    await _conversationRepository.UpdateAsync(conversation);
                }

                return ServiceOperationResult<string>.Success("Message sent successfully");
            }
            catch {
                return ServiceOperationResult<string>.Failure(ServiceOperationStatus.Failed, "Failed to send message");
            }
        }

        public async Task<ServiceOperationResult<string?>> EditMessageAsync(int messageId, string newContent) {
            try {
                var message = await _messageRepository.GetByIdAsync(messageId);
                if (message == null || message.IsDeleted)
                    return ServiceOperationResult<string>.Failure(ServiceOperationStatus.NotFound, "Message not found");

                message.Content = newContent;
                message.EditedAt = DateTime.UtcNow;
                await _messageRepository.UpdateAsync(message);
                return ServiceOperationResult<string>.Success("Message edited successfully");
            }
            catch {
                return ServiceOperationResult<string>.Failure(ServiceOperationStatus.Failed, "Failed to edit message");
            }
        }

        public async Task<ServiceOperationResult<string?>> DeleteMessageAsync(int messageId) {
            try {
                var message = await _messageRepository.GetByIdAsync(messageId);
                if (message == null || message.IsDeleted)
                    return ServiceOperationResult<string>.Failure(ServiceOperationStatus.NotFound, "Message not found");

                message.IsDeleted = true;
                await _messageRepository.UpdateAsync(message);
                return ServiceOperationResult<string>.Success("Message deleted successfully");
            }
            catch {
                return ServiceOperationResult<string>.Failure(ServiceOperationStatus.Failed, "Failed to delete message");
            }
        }

        public async Task<IEnumerable<Message>> GetConversationMessagesAsync(int conversationId, int skip = 0, int take = 50) {
            return await _messageRepository.GetConversationMessagesAsync(conversationId, skip, take);
        }

        public async Task<IEnumerable<Message>> GetConversationMessagesWithDeliveryAsync(int conversationId, int userId, int skip = 0, int take = 50) {
            return await _messageRepository.GetConversationMessagesWithDeliveryAsync(conversationId, userId, skip, take);
        }

        public async Task<ServiceOperationResult<string?>> MarkMessagesAsReadAsync(List<int> messageIds, int userId) {
            if (messageIds == null || !messageIds.Any())
                return ServiceOperationResult<string>.Failure(ServiceOperationStatus.InvalidParameters, "Message IDs list cannot be empty");

            var messages = await _messageRepository.GetTableNoTracking()
                .Where(m => messageIds.Contains(m.Id))
                .ToListAsync();

            if (!messages.Any())
                return ServiceOperationResult<string>.Failure(ServiceOperationStatus.NotFound, "No messages found");

            var (deliveriesToAdd, deliveriesToUpdate) = await PrepareMessageDeliveriesForReadAsync(messages, userId);
            var participantsToUpdate = await PrepareParticipantsForReadAsync(messages, userId);

            await _messageDeliveryRepository.AddRangeAsync(deliveriesToAdd);
            await _messageDeliveryRepository.UpdateRangeAsync(deliveriesToUpdate);
            await _participantRepository.UpdateRangeAsync(participantsToUpdate);
            return ServiceOperationResult<string>.Success("Messages marked as read successfully");
        }

        private async Task<(List<MessageDelivery> toAdd, List<MessageDelivery> toUpdate)> PrepareMessageDeliveriesForReadAsync(List<Message> messages, int userId) {
            var deliveriesToAdd = new List<MessageDelivery>();
            var deliveriesToUpdate = new List<MessageDelivery>();

            foreach (var message in messages) {
                // Skip messages sent by the same user
                if (message.SenderId == userId)
                    continue;

                // Check if user is participant in the conversation
                if (!await IsUserInConversationAsync(userId, message.ConversationId))
                    continue;

                var existingDelivery = await _messageDeliveryRepository.GetUserMessageDeliveryAsync(message.Id, userId);

                if (existingDelivery is null) {
                    deliveriesToAdd.Add(new MessageDelivery {
                        MessageId = message.Id,
                        UserId = userId,
                        Status = DeliveryStatus.Read,
                        DeliveredAt = DateTime.UtcNow,
                        ReadAt = DateTime.UtcNow
                    });
                }
                else if (existingDelivery.Status != DeliveryStatus.Read) {
                    existingDelivery.Status = DeliveryStatus.Read;
                    existingDelivery.DeliveredAt = existingDelivery.DeliveredAt ?? DateTime.UtcNow;
                    existingDelivery.ReadAt = DateTime.UtcNow;
                    deliveriesToUpdate.Add(existingDelivery);
                }
            }

            return (deliveriesToAdd, deliveriesToUpdate);
        }

        private async Task<List<ConversationParticipant>> PrepareParticipantsForReadAsync(List<Message> messages, int userId) {
            var participantsToUpdate = new List<ConversationParticipant>();
            var processedConversations = new HashSet<int>();

            foreach (var message in messages) {
                if (message.SenderId == userId)
                    continue;

                // Skip if conversation already processed (optimization for multiple messages in same conversation)
                if (processedConversations.Contains(message.ConversationId))
                    continue;

                var participant = await _participantRepository.GetParticipantAsync(message.ConversationId, userId);
                if (participant == null)
                    continue;

                var latestMessageInConversation = messages
                    .Where(m => m.ConversationId == message.ConversationId)
                    .OrderByDescending(m => m.SentAt)
                    .First();

                participant.LastReadMessageId = latestMessageInConversation.Id;
                participant.LastReadAt = DateTime.UtcNow;
                participantsToUpdate.Add(participant);
                processedConversations.Add(message.ConversationId);
            }

            return participantsToUpdate;
        }

        public async Task<ServiceOperationResult<string?>> MarkMessagesAsDeliveredAsync(List<int> messageIds, int userId) {
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
                if (!await IsUserInConversationAsync(userId, message.ConversationId))
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

        public async Task<Message?> GetLastMessageInConversationAsync(int conversationId) {
            return await _messageRepository.GetTableNoTracking()
                .Where(m => m.ConversationId == conversationId && !m.IsDeleted)
                .OrderByDescending(m => m.SentAt)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<int>> GetUndeliveredMessageIdsForUserAsync(int userId) {
            try {
                // Get all conversations where the user is a participant
                var userConversations = await _conversationRepository.GetUserConversationsAsync(userId);
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

        public async Task<bool> AreAllParticipantsDeliveredAsync(Message message, DeliveryStatus requiredStatus) {
            if (message.Conversation == null)
                return false;

            // Get active participants excluding the sender
            var expectedRecipients = message.Conversation.Participants
                .Where(p => p.IsActive && p.UserId != message.SenderId)
                .Select(p => p.UserId)
                .ToList();

            if (!expectedRecipients.Any())
                return true; // No recipients, consider as delivered/read

            // Get all deliveries for this message
            var deliveries = await _messageDeliveryRepository.GetMessageDeliveriesAsync(message.Id);

            // Check if all expected recipients have the required delivery status
            foreach (var recipientId in expectedRecipients) {
                var delivery = deliveries.FirstOrDefault(d => d.UserId == recipientId);

                // If no delivery record exists or status is less than required, return false
                if (delivery == null || delivery.Status < requiredStatus)
                    return false;
            }

            return true;
        }

        public DeliveryStatus GetDeliveryStatus(Message message) {
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

        private async Task<bool> IsUserInConversationAsync(int userId, int conversationId) {
            try {
                var participant = await _participantRepository.GetParticipantAsync(conversationId, userId);
                return participant != null && participant.IsActive;
            }
            catch {
                return false;
            }
        }
    }
}
