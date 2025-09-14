using ChatApi.Core.Features.Chat.Queries.RequestsModels;
using FluentValidation;

namespace ChatApi.Core.Features.Chat.Queries.Validators {
    public class GetNewConversationValidator : AbstractValidator<GetNewConversationQuery> {
        public GetNewConversationValidator() {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required")
                .NotNull().WithMessage("Username cannot be null")
                .Length(3, 50).WithMessage("Username must be between 3 and 50 characters");
        }
    }
}