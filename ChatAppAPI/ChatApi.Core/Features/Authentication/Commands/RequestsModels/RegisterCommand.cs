using ChatApi.Core.Bases;
using MediatR;

namespace ChatApi.Core.Features.Authentication.Commands.RequestsModels {
    public class RegisterCommand : IRequest<Response<string>> {
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string? Address { get; set; }
        public string? Country { get; set; }
        public string? PhoneNumber { get; set; }

    }
}
