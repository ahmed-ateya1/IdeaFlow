using FluentValidation;
using MindMapGenerator.Core.Dtos.AuthenticationDto;

namespace MindMapGenerator.Core.Validators
{
    public class RegisterValidator : AbstractValidator<RegisterDTO>
    {
        public RegisterValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email is not valid");
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required");
         
        }
    }
}
