using ChatApi.Core.Features.Authentication.Commands.RequestsModels;
using FluentValidation;

namespace ChatApi.Core.Features.Authentication.Commands.Validators {
    public class SignInValidator : AbstractValidator<SignInCommand> {
        public SignInValidator() {
            ApplyValidaionRules();
        }
        public void ApplyValidaionRules() {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("{PropertyName} can't be empty")
                .NotNull().WithMessage("{PropertyName} is required");


            RuleFor(x => x.Password)
             .NotEmpty().WithMessage("{PropertyName} can't be empty")
                .NotNull().WithMessage("{PropertyName} is required");

        }

    }
}