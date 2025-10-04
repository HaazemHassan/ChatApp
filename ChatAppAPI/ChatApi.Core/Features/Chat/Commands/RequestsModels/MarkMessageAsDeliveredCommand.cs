using ChatApi.Core.Bases;
using MediatR;

namespace ChatApi.Core.Features.Chat.Commands.RequestsModels {
    public class MarkMessageAsDeliveredCommand : IRequest<Response<string>> {
        public int MessageId { get; set; }
    }
}