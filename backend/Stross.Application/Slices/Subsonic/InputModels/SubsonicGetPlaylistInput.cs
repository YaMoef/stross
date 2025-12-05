using FluentValidation;

namespace Stross.Application.Slices.Subsonic.InputModels;

public sealed record SubsonicGetPlaylistInput(string Id);

public sealed class SubsonicGetPlaylistInputValidator : AbstractValidator<SubsonicGetPlaylistInput>
{
    public SubsonicGetPlaylistInputValidator()
    {
        RuleFor(input => input.Id)
            .NotEmpty()
            .WithMessage("Playlist id is required");
    }
}
