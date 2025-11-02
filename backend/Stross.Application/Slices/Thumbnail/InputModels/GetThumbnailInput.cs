using FluentValidation;

namespace Stross.Application.Slices.Thumbnail.InputModels;

public sealed record GetThumbnailInput(long Id, ThumbnailType Type);

internal sealed class GetThumbnailInputValidator : AbstractValidator<GetThumbnailInput>
{
    public GetThumbnailInputValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Id must be a valid positive number");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Type must be a valid thumbnail type");
    }
}