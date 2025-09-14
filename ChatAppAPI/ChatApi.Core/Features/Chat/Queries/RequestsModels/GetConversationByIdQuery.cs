using ChatApi.Core.Bases;
using ChatApi.Core.Features.Chat.Queries.Responses;
using MediatR;

namespace ChatApi.Core.Features.Chat.Queries.RequestsModels {
    public class GetConversationByIdQuery : IRequest<Response<GetConversationByIdResponse>> {
        public int ConversationId { get; set; }
    }
}