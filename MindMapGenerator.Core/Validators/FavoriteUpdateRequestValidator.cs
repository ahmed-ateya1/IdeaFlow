using FluentValidation;
using MindMapGenerator.Core.Dtos.FavoriteDto;

namespace MindMapGenerator.Core.Validators
{
    public class FavoriteUpdateRequestValidator : AbstractValidator<FavoriteUpdateRequest>
    {
        public FavoriteUpdateRequestValidator()
        {
            RuleFor(x => x.DiagramID)
                .NotEmpty()
                .WithMessage("DiagramID is required");
            RuleFor(x => x.FavoriteID)
                .NotEmpty()
                .WithMessage("FavoriteID is required");
        }
    }
}
