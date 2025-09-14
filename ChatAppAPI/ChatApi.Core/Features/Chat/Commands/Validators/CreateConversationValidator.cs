using ChatApi.Core.Features.Chat.Commands.RequestsModels;
using FluentValidation;

namespace ChatApi.Core.Features.Chat.Commands.Validators {
    public class CreateConversationValidator : AbstractValidator<CreateConversationCommand> {
        public CreateConversationValidator() {
            RuleFor(x => x.Type)
                .IsInEnum()
                .WithMessage("Invalid conversation type");

            RuleFor(x => x.ParticipantIds)
                .NotEmpty()
                .WithMessage("At least one participant is required")
                .Must(x => x.Count >= 2)
                .WithMessage("At least two participants are required for a conversation");

            RuleFor(x => x.Title)
                .MaximumLength(100)
                .WithMessage("Title cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.Title));
        }
    }
}