using FluentValidation;
using MindMapGenerator.Core.Dtos.AuthenticationDto;

namespace MindMapGenerator.Core.Validators
{
    public class ChangePasswordValidator : AbstractValidator<ChangePasswordRequest>
    {
        public ChangePasswordValidator()
        {
            RuleFor(x => x.OldPassword)
                .NotEmpty().WithMessage("Old Password is required");
            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("New Password is required");
            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("Confirm Password is required")
                .Equal(x => x.NewPassword).WithMessage("Password and Confirm Password must be the same");
        }
    }
}
