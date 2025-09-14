using ChatApi.Core.Bases;
using ChatApi.Core.Features.Chat.Queries.Responses;
using MediatR;

namespace ChatApi.Core.Features.Chat.Queries.RequestsModels {
    public class GetConversationMessagesQuery : IRequest<Response<GetConversationMessagesResponse>> {
        public int ConversationId { get; set; }
        public int Skip { get; set; } = 0;
        public int Take { get; set; } = 50;
    }
}