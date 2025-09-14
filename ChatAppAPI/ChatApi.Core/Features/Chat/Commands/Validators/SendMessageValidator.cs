using ChatApi.Core.Features.Chat.Commands.RequestsModels;
using FluentValidation;

namespace ChatApi.Core.Features.Chat.Commands.Validators {
    public class SendMessageValidator : AbstractValidator<SendMessageCommand> {
        public SendMessageValidator() {
            RuleFor(x => x.ConversationId)
                .GreaterThan(0)
                .WithMessage("Invalid conversation ID");

            RuleFor(x => x.Content)
                .NotEmpty()
                .WithMessage("Message content is required")
                .MaximumLength(2000)
                .WithMessage("Message content cannot exceed 2000 characters");

            RuleFor(x => x.MessageType)
                .IsInEnum()
                .WithMessage("Invalid message type");

            RuleFor(x => x.ReplyToMessageId)
                .GreaterThan(0)
                .WithMessage("Invalid reply message ID")
                .When(x => x.ReplyToMessageId.HasValue);
        }
    }
}