using FluentValidation;

namespace Stross.Application.Slices.Subsonic.InputModels;

public sealed record SubsonicGetStarred2Input(string? MusicFolderId = null);

public sealed class SubsonicGetStarred2InputValidator : AbstractValidator<SubsonicGetStarred2Input>
{
    public SubsonicGetStarred2InputValidator()
    {
        RuleFor(input => input.MusicFolderId)
            .Must(musicFolderId => string.IsNullOrEmpty(musicFolderId) || !string.IsNullOrWhiteSpace(musicFolderId))
            .WithMessage("MusicFolderId must not be whitespace only when provided");
    }
}
