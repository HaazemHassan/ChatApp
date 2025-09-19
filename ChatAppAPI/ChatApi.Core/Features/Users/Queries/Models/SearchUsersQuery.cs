using ChatApi.Core.Bases;
using ChatApi.Core.Features.Users.Queries.Responses;
using MediatR;

namespace ChatApi.Core.Features.Users.Queries.Models {
    public class SearchUsersQuery : IRequest<Response<List<SearchUsersResponse>>> {
        public string Username { get; set; }
    }
}