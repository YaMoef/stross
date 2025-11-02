using FluentValidation;

namespace Stross.Application.Slices.Subsonic.InputModels;

public sealed record SubsonicGetArtistsInput(string? MusicFolderId = null);

public sealed class SubsonicGetArtistsInputValidator : AbstractValidator<SubsonicGetArtistsInput>
{
    public SubsonicGetArtistsInputValidator()
    {
        RuleFor(input => input.MusicFolderId)
            .Must(musicFolderId => string.IsNullOrEmpty(musicFolderId) || !string.IsNullOrWhiteSpace(musicFolderId))
            .WithMessage("MusicFolderId must not be whitespace only when provided");
    }
}