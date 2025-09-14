using ChatApi.Core.Bases;
using MediatR;

namespace ChatApi.Core.Features.Chat.Commands.RequestsModels {
    public class MarkMessageAsReadCommand : IRequest<Response<string>> {
        public int MessageId { get; set; }
    }
}