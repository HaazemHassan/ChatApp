using ChatApi.Core.Bases;
using ChatApi.Core.Enums.ChatEnums;
using ChatApi.Core.Features.Chat.Queries.Responses;
using MediatR;

namespace ChatApi.Core.Features.Chat.Commands.RequestsModels {
    public class SendDirectMessageCommand : IRequest<Response<MessageResponse>> {
        public int SenderId { get; set; }
        public int RecipientUserId { get; set; }
        public string Content { get; set; }
        public MessageType MessageType { get; set; } = MessageType.Text;
    }
}