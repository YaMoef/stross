using FluentValidation;

namespace Stross.Application.Slices.Subsonic.InputModels;

public sealed record SubsonicGetPlaylistsInput(string? Username = null);

public sealed class SubsonicGetPlaylistsInputValidator : AbstractValidator<SubsonicGetPlaylistsInput>
{
    public SubsonicGetPlaylistsInputValidator()
    {
        RuleFor(input => input.Username)
            .Must(username => string.IsNullOrEmpty(username) || !string.IsNullOrWhiteSpace(username))
            .WithMessage("Username must not be whitespace only when provided");
    }
}