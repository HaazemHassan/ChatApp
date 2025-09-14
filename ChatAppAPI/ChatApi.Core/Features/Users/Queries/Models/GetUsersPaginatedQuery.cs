using ChatApi.Core.Bases;
using ChatApi.Core.Features.Users.Queries.Responses;
using MediatR;

namespace ChatApi.Core.Features.Users.Queries.Models {
    public class GetUsersPaginatedQuery : IRequest<PaginatedResult<GetUsersPaginatedResponse>> {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
