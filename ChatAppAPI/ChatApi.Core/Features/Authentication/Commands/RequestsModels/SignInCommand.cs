using ChatApi.Core.Bases;
using ChatApi.Core.Bases.Authentication;
using MediatR;

namespace ChatApi.Core.Features.Authentication.Commands.RequestsModels {
    public class SignInCommand : IRequest<Response<JwtResult>> {
        public string Username { get; set; }
        public string Password { get; set; }

    }
}
