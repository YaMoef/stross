using FluentValidation;

namespace Stross.Application.Slices.Subsonic.InputModels;

public sealed record SubsonicGetArtistInput(string Id);

internal sealed class SubsonicGetArtistInputValidator : AbstractValidator<SubsonicGetArtistInput>
{
    public SubsonicGetArtistInputValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Id is required for getArtist");
    }
}