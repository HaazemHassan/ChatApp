using ChatApi.Core.Bases;
using MediatR;

namespace ChatApi.Core.Features.Users.Queries.Models {
    public class CheckUsernameAvailabilityQuery : IRequest<Response<bool>> {
        public string Username { get; set; }
    }
}
