using ChatApi.Bases;
using ChatApi.Core.Abstracts.ServicesContracts;
using ChatApi.Core.Features.Chat.Queries.RequestsModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatApi.Controllers {
    [Authorize]
    public class ChatController : BaseController {
        private readonly ICurrentUserService _currentUserService;
        public ChatController(IMediator mediator, ICurrentUserService currentUserService) : base(mediator) {
            _currentUserService = currentUserService;
        }

        [HttpGet("conversations")]
        public async Task<IActionResult> GetUserConversations() {
            var response = await mediator.Send(new GetUserConversationsQuery());
            return NewResult(response);
        }

        [HttpGet("conversations/{conversationId:int}/messages")]
        public async Task<IActionResult> GetConversationMessages([FromRoute] int conversationId) {
            var query = new GetConversationMessagesQuery { ConversationId = conversationId };

            var response = await mediator.Send(query);
            return NewResult(response);
        }



        [HttpGet("conversations/new/{username}")]
        public async Task<IActionResult> GetNewConversation([FromRoute] string username) {
            var response = await mediator.Send(new GetNewConversationQuery { Username = username });
            return NewResult(response);
        }


    }
}