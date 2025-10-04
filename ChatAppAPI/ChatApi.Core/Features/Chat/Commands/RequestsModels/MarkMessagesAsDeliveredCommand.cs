using ChatApi.Core.Bases;
using MediatR;

namespace ChatApi.Core.Features.Chat.Commands.RequestsModels {
    public class MarkMessagesAsDeliveredCommand : IRequest<Response<string>> {
        public List<int> MessageIds { get; set; } = new();
    }
}