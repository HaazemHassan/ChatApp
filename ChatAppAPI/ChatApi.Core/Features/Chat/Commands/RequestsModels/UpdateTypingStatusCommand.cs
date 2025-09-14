using ChatApi.Core.Bases;
using MediatR;

namespace ChatApi.Core.Features.Chat.Commands.RequestsModels {
    public class UpdateTypingStatusCommand : IRequest<Response<string>> {
        public int ConversationId { get; set; }
        public bool IsTyping { get; set; }
    }
}