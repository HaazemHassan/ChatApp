using ChatApi.Bases;
using ChatApi.Core.Features.Users.Queries.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatApi.Controllers {

    public class ApplicationUserController : BaseController {
        public ApplicationUserController(IMediator mediator) : base(mediator) { }



        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] GetUsersPaginatedQuery query) {
            var result = await mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] GetUserByIdQuery query) {
            var result = await mediator.Send(query);
            return NewResult(result);
        }

        [HttpGet("{Username}")]
        public async Task<IActionResult> GetByUsername([FromRoute] GetUserByUsernameQuery query) {
            var result = await mediator.Send(query);
            return NewResult(result);
        }

        [Authorize]
        [HttpGet("search")]
        public async Task<IActionResult> SearchUsers([FromQuery] SearchUsersQuery query) {
            var result = await mediator.Send(query);
            return NewResult(result);
        }

        [HttpGet("check-username")]
        public async Task<IActionResult> CheckUsernameAvailability([FromQuery] CheckUsernameAvailabilityQuery query) {
            var result = await mediator.Send(query);
            return NewResult(result);
        }

        [HttpGet("check-email")]
        public async Task<IActionResult> CheckEmailAvailability([FromQuery] CheckEmailAvailabilityQuery query) {
            var result = await mediator.Send(query);
            return NewResult(result);
        }



        //[HttpPatch("{id:int}")]
        //public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateUserCommand command) {
        //    command.Id = id;
        //    var result = await mediator.Send(command);
        //    return NewResult(result);
        //}

        //[HttpDelete("{id:int}")]
        //public async Task<IActionResult> Delete([FromRoute] DeleteUserByIdCommand command) {
        //    var result = await mediator.Send(command);
        //    return NewResult(result);
        //}

        //[HttpPatch("update-password/{id:int}")]
        //public async Task<IActionResult> UpdatePassword([FromRoute] int id, [FromBody] ChangePasswordCommand command) {
        //    command.Id = id;
        //    var result = await mediator.Send(command);
        //    return NewResult(result);
        //}

        //[HttpPatch("reset-password")]
        //[Authorize(policy: "ResetPasswordPolicy")]
        //public async Task<IActionResult> ResetPassword([FromForm] ResetPasswordCommand command) {
        //    var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //    if (int.TryParse(userIdClaim, out var userId)) {
        //        command.UserId = userId;
        //    }
        //    else {
        //        return Unauthorized("Invalid token or user ID not found.");
        //    }
        //    var result = await mediator.Send(command);
        //    return NewResult(result);
        //}
    }
}
