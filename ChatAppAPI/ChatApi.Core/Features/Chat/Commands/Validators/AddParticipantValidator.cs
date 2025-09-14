using ChatApi.Core.Features.Chat.Commands.RequestsModels;
using FluentValidation;

namespace ChatApi.Core.Features.Chat.Commands.Validators {
    public class AddParticipantValidator : AbstractValidator<AddParticipantCommand> {
        public AddParticipantValidator() {
            RuleFor(x => x.ConversationId)
                .GreaterThan(0)
                .WithMessage("Invalid conversation ID");

            RuleFor(x => x.UserId)
                .GreaterThan(0)
                .WithMessage("Invalid user ID");

            RuleFor(x => x.Role)
                .IsInEnum()
                .WithMessage("Invalid participant role");
        }
    }
}