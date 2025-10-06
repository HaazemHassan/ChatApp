using ChatApi.Core.Features.Users.Queries.Models;
using FluentValidation;

namespace ChatApi.Core.Features.Users.Queries.Validators {
    public class CheckUsernameAvailabilityValidator : AbstractValidator<CheckUsernameAvailabilityQuery> {
        public CheckUsernameAvailabilityValidator() {
            ApplyValidationRules();
        }

        public void ApplyValidationRules() {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("{PropertyName} can't be empty")
                .NotNull().WithMessage("{PropertyName} is required")
                .MinimumLength(3).WithMessage("{PropertyName} must be at least 3 characters")
                .MaximumLength(50).WithMessage("{PropertyName} cannot exceed 50 characters")
                .Matches(@"^[a-zA-Z0-9._-]+$").WithMessage("{PropertyName} can only contain letters, numbers, dots, underscores, and hyphens");
        }
    }
}
