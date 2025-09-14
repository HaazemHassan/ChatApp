using ChatApi.Core.Bases;
using MediatR;

namespace ChatApi.Core.Features.Chat.Commands.RequestsModels {
    public class EditMessageCommand : IRequest<Response<string>> {
        public int MessageId { get; set; }
        public string NewContent { get; set; }
    }
}