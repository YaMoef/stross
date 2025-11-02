using FluentValidation;

namespace Stross.Application.Slices.Subsonic.InputModels;

public sealed record SubsonicGetStarredInput(string? MusicFolderId = null);

public sealed class SubsonicGetStarredInputValidator : AbstractValidator<SubsonicGetStarredInput>
{
    public SubsonicGetStarredInputValidator()
    {
        RuleFor(input => input.MusicFolderId)
            .Must(musicFolderId => string.IsNullOrEmpty(musicFolderId) || !string.IsNullOrWhiteSpace(musicFolderId))
            .WithMessage("MusicFolderId must not be whitespace only when provided");
    }
}