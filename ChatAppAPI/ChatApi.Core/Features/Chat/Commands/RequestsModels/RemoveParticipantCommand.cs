using ChatApi.Core.Bases;
using MediatR;

namespace ChatApi.Core.Features.Chat.Commands.RequestsModels {
    public class RemoveParticipantCommand : IRequest<Response<string>> {
        public int ConversationId { get; set; }
        public int UserId { get; set; }
    }
}