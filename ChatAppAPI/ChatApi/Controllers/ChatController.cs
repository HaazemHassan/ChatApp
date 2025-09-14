using ChatApi.Bases;
using ChatApi.Core.Abstracts.ServicesContracts;
using ChatApi.Core.Features.Chat.Commands.RequestsModels;
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

        // Get user conversations for initial load only
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

        [HttpPost("conversations/send-direct-message")]
        public async Task<IActionResult> SendDirectMessage(SendDirectMessageCommand command) {
            command.SenderId = _currentUserService.UserId!.Value;

            var response = await mediator.Send(command);
            return NewResult(response); ;
        }

        [HttpGet("conversations/{conversationId:int}")]
        public async Task<IActionResult> GetConversationById([FromRoute] int conversationId) {
            var response = await mediator.Send(new GetConversationByIdQuery { ConversationId = conversationId });
            return NewResult(response);
        }

        [HttpGet("conversations/new/{username}")]
        public async Task<IActionResult> GetNewConversation([FromRoute] string username) {
            var response = await mediator.Send(new GetNewConversationQuery { Username = username });
            return NewResult(response);
        }

    }
}