using ChatApi.Core.Features.Authentication.Commands.RequestsModels;
using FluentValidation;

namespace ChatApi.Core.Features.Authentication.Commands.Validators {
    public class RegisterValidator : AbstractValidator<RegisterCommand> {

        public RegisterValidator() {
            ApplyValidaionRules();
        }
        public void ApplyValidaionRules() {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("{PropertyName} can't be empty")
                .NotNull().WithMessage("{PropertyName} can't be null")
                .MinimumLength(4)
                .MaximumLength(20)
                .Matches(@"^[\p{L}\s]+$").WithMessage(errorMessage: "Full name must contain only letters and spaces.");


            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("{PropertyName} can't be empty")
                .NotNull().WithMessage("{PropertyName} can't be null")
                .MinimumLength(4)
                .MaximumLength(20)
                .Matches(@"^[a-zA-Z0-9._]+$").WithMessage("{PropertyName} can only contain letters, numbers, dots, and underscores.");


            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("{PropertyName} can't be empty")
                .NotNull().WithMessage("{PropertyName} can't be null")
                .MaximumLength(35)
                .EmailAddress().WithMessage("Invalid email address format.")
                .Matches(@"@.+\..+").WithMessage("{PropertyName} must contain a valid domain.");


            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("{PropertyName} can't be empty")
                .NotNull().WithMessage("{PropertyName} can't be null")
                .MinimumLength(3).WithMessage("{PropertyName} must be at least of length 3");

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.Password).WithMessage("Password does not match");

            RuleFor(x => x.PhoneNumber)
                .Matches(expression: @"^\+?[0-9]\d{1,14}$")
                .WithMessage("Phone number is not valid.");
        }
    }
}