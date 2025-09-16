using AutoMapper;
using ChatApi.Core.Abstracts.ServicesContracts;
using ChatApi.Core.Bases;
using ChatApi.Core.Bases.Authentication;
using ChatApi.Core.Entities.IdentityEntities;
using ChatApi.Core.Enums;
using ChatApi.Core.Features.Authentication.Commands.RequestsModels;
using ChatApi.Core.Features.Users.Queries.Responses;
using MediatR;
using Microsoft.AspNetCore.Identity;
using School.Core.Features.Authentication.Commands.Models;

namespace ChatApi.Core.Features.Authentication.Commands.Handlers {
    public class AuthenticationCommandHandler : ResponseHandler, IRequestHandler<RegisterCommand, Response<string>>,
                                                                 IRequestHandler<SignInCommand, Response<JwtResult>>,
                                                                 IRequestHandler<RefreshTokenCommand, Response<JwtResult>> {


        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IApplicationUserService _applicationUserService;
        IAuthenticationService _authenticationService;
        ICurrentUserService _currentUserService;


        public AuthenticationCommandHandler(IApplicationUserService applicationUserService, UserManager<ApplicationUser> userManager, IMapper mapper, IAuthenticationService authenticationService, ICurrentUserService currentUserService) {
            _applicationUserService = applicationUserService;
            _userManager = userManager;
            _mapper = mapper;
            this._authenticationService = authenticationService;
            _currentUserService = currentUserService;
        }


        public async Task<Response<string>> Handle(RegisterCommand request, CancellationToken cancellationToken) {
            if (_currentUserService.IsAuthenticated)
                return BadRequest<string>("You are already signed in");

            var userMapped = _mapper.Map<ApplicationUser>(request);
            var serviceResult = await _applicationUserService.AddApplicationUser(userMapped, request.Password);

            return serviceResult switch {
                ServiceOperationStatus.AlreadyExists => Conflict<string>("Email or username already used"),
                ServiceOperationStatus.Succeeded => Created(),
                _ => BadRequest<string>(),
            };
        }

        public async Task<Response<JwtResult>> Handle(SignInCommand request, CancellationToken cancellationToken) {
            if (_currentUserService.IsAuthenticated)
                return BadRequest<JwtResult>("You are already signed in");

            var userFromDb = await _userManager.FindByNameAsync(request.Username);
            if (userFromDb is null)
                return Unauthorized<JwtResult>("Invalid username or password");

            bool isAuthenticated = await _userManager.CheckPasswordAsync(userFromDb, request.Password);
            if (!isAuthenticated)
                return Unauthorized<JwtResult>("Invalid username or password");

            //if (!userFromDb.EmailConfirmed)
            //    return Unauthorized<JwtResult>("Please confirm your email first");

            JwtResult? jwtResult = await _authenticationService.AuthenticateAsync(userFromDb);
            if (jwtResult is null)
                return BadRequest<JwtResult>("Something went wrong");

            jwtResult.User = _mapper.Map<GetUserByIdResponse>(userFromDb);
            return Success(jwtResult);

        }

        public async Task<Response<JwtResult>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken) {
            try {
                JwtResult? jwtResult = await _authenticationService.ReAuthenticateAsync(request.RefreshToken, request.AccessToken);
                return jwtResult is null ? Unauthorized<JwtResult>() : Success(jwtResult);

            }
            catch (Exception e) {
                return Unauthorized<JwtResult>();
            }

        }
    }
}
