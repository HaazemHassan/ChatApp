using ChatApi.Core.Bases;
using ChatApi.Core.Features.Chat.Queries.Responses;
using MediatR;

namespace ChatApi.Core.Features.Chat.Queries.RequestsModels {
    public class GetConversationMessagesQuery : IRequest<Response<GetConversationMessagesResponse>> {
        public int ConversationId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        
        // Helper property to calculate skip
        public int Skip => (PageNumber - 1) * PageSize;
        public int Take => PageSize;
    }
}