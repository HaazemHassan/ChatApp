using ChatApi.Core.Features.Chat.Commands.RequestsModels;
using FluentValidation;

namespace ChatApi.Core.Features.Chat.Commands.Validators {
    public class UpdateTypingStatusValidator : AbstractValidator<UpdateTypingStatusCommand> {
        public UpdateTypingStatusValidator() {
            RuleFor(x => x.ConversationId)
                .GreaterThan(0)
                .WithMessage("Invalid conversation ID");
        }
    }
}