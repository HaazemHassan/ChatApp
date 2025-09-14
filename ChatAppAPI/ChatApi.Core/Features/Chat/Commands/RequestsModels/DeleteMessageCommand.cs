using ChatApi.Core.Bases;
using MediatR;

namespace ChatApi.Core.Features.Chat.Commands.RequestsModels {
    public class DeleteMessageCommand : IRequest<Response<string>> {
        public int MessageId { get; set; }
    }
}