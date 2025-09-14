using ChatApi.Core.Bases;
using ChatApi.Core.Enums.ChatEnums;
using ChatApi.Core.Features.Chat.Commands.Responses;
using MediatR;

namespace ChatApi.Core.Features.Chat.Commands.RequestsModels {
    public class CreateConversationCommand : IRequest<Response<CreateConversationResponse>> {
        public string? Title { get; set; }
        public ConversationType Type { get; set; } = ConversationType.Direct;
        public List<int> ParticipantIds { get; set; } = new();
    }
}