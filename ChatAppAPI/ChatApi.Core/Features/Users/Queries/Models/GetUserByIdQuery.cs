using ChatApi.Core.Bases;
using ChatApi.Core.Features.Users.Queries.Responses;
using MediatR;

namespace ChatApi.Core.Features.Users.Queries.Models {
    public class GetUserByIdQuery : IRequest<Response<GetUserByIdResponse>> {
        public int Id { get; set; }
    }
}
