using ChatApi.Core.Bases;
using ChatApi.Core.Bases.Authentication;
using MediatR;

namespace School.Core.Features.Authentication.Commands.Models {
    public class RefreshTokenCommand : IRequest<Response<JwtResult>> {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }

    }
}
