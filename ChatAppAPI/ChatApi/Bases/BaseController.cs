using ChatApi.Core.Bases;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ChatApi.Bases {
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController : ControllerBase {

        protected IMediator mediator;

        public BaseController(IMediator mediator) {
            this.mediator = mediator;

        }

        public IActionResult NewResult<T>(Response<T> response) {
            switch (response.StatusCode) {
                case HttpStatusCode.OK:
                return new OkObjectResult(response);
                case HttpStatusCode.Created:
                return new CreatedResult(string.Empty, response);
                case HttpStatusCode.Unauthorized:
                return new UnauthorizedObjectResult(response);
                case HttpStatusCode.Forbidden:
                return new ObjectResult(response) { StatusCode = 403 };
                case HttpStatusCode.BadRequest:
                return new BadRequestObjectResult(response);
                case HttpStatusCode.NotFound:
                return new NotFoundObjectResult(response);
                case HttpStatusCode.Accepted:
                return new AcceptedResult(string.Empty, response);
                case HttpStatusCode.Conflict:
                return new ConflictObjectResult(response);
                default:
                return new BadRequestObjectResult(response);
            }


        }
    }
}
