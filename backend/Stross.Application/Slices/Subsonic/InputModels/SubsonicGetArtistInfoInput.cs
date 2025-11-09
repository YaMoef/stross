using FluentValidation;

namespace Stross.Application.Slices.Subsonic.InputModels;

public sealed record SubsonicGetArtistInfoInput(
    string Id,
    int Count = 20,
    bool IncludeNotPresent = false
);

internal sealed class SubsonicGetArtistInfoInputValidator : AbstractValidator<SubsonicGetArtistInfoInput>
{
    public SubsonicGetArtistInfoInputValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Id is required for getArtistInfo");

        RuleFor(x => x.Count)
            .GreaterThan(0)
            .LessThanOrEqualTo(500)
            .WithMessage("Count must be between 1 and 500");
    }
}