using Application.CQRS.Template;
using FluentValidation;

namespace Application.Behaviours.Template;

public class AddTemplateCommandValidator: AbstractValidator<AddTemplateCommand>
{
    public AddTemplateCommandValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty()
            .WithMessage("Id is required");
    }
}