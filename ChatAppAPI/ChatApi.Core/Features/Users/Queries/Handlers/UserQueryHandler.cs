using AutoMapper;
using ChatApi.Core.Bases;
using ChatApi.Core.Entities.IdentityEntities;
using ChatApi.Core.Features.Users.Queries.Models;
using ChatApi.Core.Features.Users.Queries.Responses;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace ChatApi.Core.Features.Users.Queries.Handlers {
    public class UserQueryHandler : ResponseHandler,
                                    IRequestHandler<GetUserByIdQuery, Response<GetUserByIdResponse>>,
                                    IRequestHandler<GetUserByUsernameQuery, Response<GetUserByUsernameResponse>> {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;



        public UserQueryHandler(UserManager<ApplicationUser> userManager, IMapper mapper) {
            _userManager = userManager;
            _mapper = mapper;
        }

        //public async Task<PaginatedResult<GetUsersPaginatedResponse>> Handle(GetUsersPaginatedQuery request, CancellationToken cancellationToken) {
        //    var usersQuerable = _userManager.Users;
        //    var usersPaginatedResult = await _mapper.ProjectTo<GetUsersPaginatedResponse>(usersQuerable)
        //                        .ToPaginatedResultAsync(request.PageNumber, request.PageSize);
        //    return usersPaginatedResult;
        //}

        public async Task<Response<GetUserByIdResponse>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken) {
            if (request.Id <= 0)
                return BadRequest<GetUserByIdResponse>();

            var user = await _userManager.FindByIdAsync(request.Id.ToString());
            if (user is null)
                return NotFound<GetUserByIdResponse>();

            var userResponse = _mapper.Map<GetUserByIdResponse>(user);
            return Success(userResponse);
        }

        public async Task<Response<GetUserByUsernameResponse>> Handle(GetUserByUsernameQuery request, CancellationToken cancellationToken) {
            if (string.IsNullOrWhiteSpace(request.Username))
                return BadRequest<GetUserByUsernameResponse>("Username is required");

            var user = await _userManager.FindByNameAsync(request.Username);
            if (user is null)
                return NotFound<GetUserByUsernameResponse>("User not found");

            var userResponse = _mapper.Map<GetUserByUsernameResponse>(user);
            return Success(userResponse);
        }
    }
}
