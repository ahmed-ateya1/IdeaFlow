using FluentValidation;
using MindMapGenerator.Core.Dtos.DiagramDto;

namespace MindMapGenerator.Core.Validators
{
    public class DiagramAddRequestValidator : AbstractValidator<DiagramAddRequest>
    {
        public DiagramAddRequestValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("Name is required");

            RuleFor(x => x.Title)
                .MaximumLength(100)
                .WithMessage("Name can not be longer than 100 characters");

            RuleFor(x=>x.ContentJson)
                .NotEmpty()
                .WithMessage("Content is required");
        }
    }
}
