using ChatApi.Core.Bases;
using MediatR;

namespace ChatApi.Core.Features.Users.Queries.Models {
    public class CheckEmailAvailabilityQuery : IRequest<Response<bool>> {
        public string Email { get; set; }
    }
}
