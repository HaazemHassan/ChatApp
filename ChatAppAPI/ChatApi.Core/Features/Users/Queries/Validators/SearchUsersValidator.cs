using ChatApi.Core.Features.Users.Queries.Models;
using FluentValidation;

namespace ChatApi.Core.Features.Users.Queries.Validators {
    public class SearchUsersValidator : AbstractValidator<SearchUsersQuery> {
        public SearchUsersValidator() {
            ApplyValidationRules();
        }

        public void ApplyValidationRules() {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("{PropertyName} can't be empty")
                .NotNull().WithMessage("{PropertyName} is required")
                .MinimumLength(2).WithMessage("{PropertyName} must be at least 2 characters")
                .MaximumLength(50).WithMessage("{PropertyName} cannot exceed 50 characters")
                .Matches(@"^[a-zA-Z0-9._-]+$").WithMessage("{PropertyName} can only contain letters, numbers, dots, underscores, and hyphens");
        }
    }
}