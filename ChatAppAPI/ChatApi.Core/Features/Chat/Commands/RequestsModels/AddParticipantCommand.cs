using ChatApi.Core.Bases;
using ChatApi.Core.Enums.ChatEnums;
using MediatR;

namespace ChatApi.Core.Features.Chat.Commands.RequestsModels {
    public class AddParticipantCommand : IRequest<Response<string>> {
        public int ConversationId { get; set; }
        public int UserId { get; set; }
        public ConversationParticipantRole Role { get; set; } = ConversationParticipantRole.Member;
    }
}