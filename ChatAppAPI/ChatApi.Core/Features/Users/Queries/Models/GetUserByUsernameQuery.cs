using ChatApi.Core.Bases;
using ChatApi.Core.Features.Users.Queries.Responses;
using MediatR;

namespace ChatApi.Core.Features.Users.Queries.Models {
    public class GetUserByUsernameQuery : IRequest<Response<GetUserByUsernameResponse>> {
        public string Username { get; set; }
    }
}