using ChatApi.Core.Features.Users.Queries.Models;
using FluentValidation;

namespace ChatApi.Core.Features.Users.Queries.Validators {
    public class CheckEmailAvailabilityValidator : AbstractValidator<CheckEmailAvailabilityQuery> {
        public CheckEmailAvailabilityValidator() {
            ApplyValidationRules();
        }

        public void ApplyValidationRules() {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("{PropertyName} can't be empty")
                .NotNull().WithMessage("{PropertyName} is required")
                .EmailAddress().WithMessage("{PropertyName} is not a valid email address")
                .MaximumLength(100).WithMessage("{PropertyName} cannot exceed 100 characters");
        }
    }
}
