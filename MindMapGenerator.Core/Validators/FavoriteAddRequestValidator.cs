using FluentValidation;
using MindMapGenerator.Core.Dtos.FavoriteDto;

namespace MindMapGenerator.Core.Validators
{
    public class FavoriteAddRequestValidator : AbstractValidator<FavoriteAddRequest>
    {
        public FavoriteAddRequestValidator()
        {
            RuleFor(x => x.DiagramID)
                .NotEmpty()
                .WithMessage("DiagramID is required");
        }
    }
}
