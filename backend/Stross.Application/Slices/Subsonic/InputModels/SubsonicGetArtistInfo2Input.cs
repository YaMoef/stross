using FluentValidation;

namespace Stross.Application.Slices.Subsonic.InputModels;

public sealed record SubsonicGetArtistInfo2Input(
    string Id,
    int Count = 20,
    bool IncludeNotPresent = false
);

internal sealed class SubsonicGetArtistInfo2InputValidator : AbstractValidator<SubsonicGetArtistInfo2Input>
{
    public SubsonicGetArtistInfo2InputValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Id is required for getArtistInfo2");

        RuleFor(x => x.Count)
            .GreaterThan(0)
            .LessThanOrEqualTo(500)
            .WithMessage("Count must be between 1 and 500");
    }
}