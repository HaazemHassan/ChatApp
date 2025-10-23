using ChatApi.Bases;
using ChatApi.Core.Abstracts.ServicesContracts;
using ChatApi.Core.Features.Authentication.Commands.RequestsModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using School.Core.Features.Authentication.Commands.Models;

namespace ChatApi.Controllers {

    public class AuthenticationController : BaseController {
        private readonly ITokenService _tokenService;

        public AuthenticationController(IMediator mediator, ITokenService tokenService) : base(mediator) {
            _tokenService = tokenService;
        }

        [HttpPost(template: "register")]
        public async Task<IActionResult> Create([FromBody] RegisterCommand command) {
            var result = await mediator.Send(command);
            return NewResult(result);
        }


        [HttpPost("login")]
        [EnableRateLimiting("loginLimiter")]
        public async Task<IActionResult> Login([FromBody] SignInCommand command) {
            var result = await mediator.Send(command);
            if (result.Succeeded && result.Data is not null)
            {
                var refreshtoken = result.Data.RefreshToken!.Token;
                var cookieOptions = _tokenService.GetRefreshTokenCookieOptions();
                Response.Cookies.Append("refreshToken", refreshtoken, cookieOptions);
                result.Data.RefreshToken = null;  //must return another response model without refresh token but i will keep it null for now
            }

            return NewResult(result);
        }

        [HttpPost("google-login")]
        [EnableRateLimiting("loginLimiter")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginCommand command) {
            var result = await mediator.Send(command);
            if (result.Succeeded && result.Data is not null)
            {
                var refreshtoken = result.Data.RefreshToken!.Token;
                var cookieOptions = _tokenService.GetRefreshTokenCookieOptions();
                Response.Cookies.Append("refreshToken", refreshtoken, cookieOptions);
                result.Data.RefreshToken = null;  
            }
            return NewResult(result);
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command) {
            command.RefreshToken = Request.Cookies["refreshToken"];
            var result = await mediator.Send(command);
             if (result.Succeeded && result.Data is not null)
            {
                var refreshtoken = result.Data.RefreshToken!.Token;
                var cookieOptions = _tokenService.GetRefreshTokenCookieOptions();
                Response.Cookies.Append("refreshToken", refreshtoken, cookieOptions);
                result.Data.RefreshToken = null;  
            }
            return NewResult(result);
        }


        //    [HttpGet("Confirm-email")]
        //    public async Task<IActionResult> ConfirmEmail([FromQuery] ConfirmEmailCommand command) {
        //        var result = await mediator.Send(command);
        //        return NewResult(result);
        //    }

        //    [HttpGet("resend-confirmation-email")]
        //    public async Task<IActionResult> ResendConfirmationEmail([FromQuery] ResendConfirmationEmailCommand command) {
        //        var result = await mediator.Send(command);
        //        return NewResult(result);
        //    }

        //    [HttpPost("password-reset/send-email")]
        //    public async Task<IActionResult> PasswordResetEmail([FromForm] SendResetPasswordCodeCommand command) {
        //        var result = await _mediator.Send(command);
        //        return NewResult(result);
        //    }

        //    [HttpPost("password-reset/verify-code")]
        //    public async Task<IActionResult> VerifyPasswordResetCode([FromForm] VerifyResetPasswordCodeCommand command) {
        //        var result = await _mediator.Send(command);
        //        return NewResult(result);
        //    }
        //}
    }
}