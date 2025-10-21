using ChatApi.Core.Bases;
using ChatApi.Core.Bases.Authentication;
using MediatR;

namespace ChatApi.Core.Features.Authentication.Commands.RequestsModels {
    public class GoogleLoginCommand : IRequest<Response<JwtResult>> {
        public string IdToken { get; set; } = string.Empty;
    }
}
