using ChatApi.Core.Bases;
using ChatApi.Core.Enums.ChatEnums;
using ChatApi.Core.Features.Chat.Commands.Responses;
using MediatR;

namespace ChatApi.Core.Features.Chat.Commands.RequestsModels {
    public class SendMessageCommand : IRequest<Response<SendMessageResponse>> {
        public int ConversationId { get; set; }
        public string Content { get; set; }
        public MessageType MessageType { get; set; } = MessageType.Text;
        public int? ReplyToMessageId { get; set; }
    }
}