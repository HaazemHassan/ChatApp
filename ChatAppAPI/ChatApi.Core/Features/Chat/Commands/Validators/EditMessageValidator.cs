using ChatApi.Core.Features.Chat.Commands.RequestsModels;
using FluentValidation;

namespace ChatApi.Core.Features.Chat.Commands.Validators {
    public class EditMessageValidator : AbstractValidator<EditMessageCommand> {
        public EditMessageValidator() {
            RuleFor(x => x.MessageId)
                .GreaterThan(0)
                .WithMessage("Invalid message ID");

            RuleFor(x => x.NewContent)
                .NotEmpty()
                .WithMessage("Message content is required")
                .MaximumLength(2000)
                .WithMessage("Message content cannot exceed 2000 characters");
        }
    }
}