using FluentValidation;

namespace Stross.Application.Slices.Subsonic.InputModels;

public sealed record SubsonicDeletePlaylistInput(string Id);

public sealed class SubsonicDeletePlaylistInputValidator : AbstractValidator<SubsonicDeletePlaylistInput>
{
    public SubsonicDeletePlaylistInputValidator()
    {
        RuleFor(input => input.Id)
            .NotEmpty()
            .WithMessage("Playlist id is required");
    }
}
