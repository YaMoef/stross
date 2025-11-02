using FluentValidation;

namespace Stross.Application.Slices.Subsonic.InputModels;

public sealed record SubsonicGetIndexesInput(string? MusicFolderId = null, long? IfModifiedSince = null);

public sealed class SubsonicGetIndexesInputValidator : AbstractValidator<SubsonicGetIndexesInput>
{
    public SubsonicGetIndexesInputValidator()
    {
        RuleFor(input => input.MusicFolderId)
            .Must(musicFolderId => string.IsNullOrEmpty(musicFolderId) || !string.IsNullOrWhiteSpace(musicFolderId))
            .WithMessage("MusicFolderId must not be whitespace only when provided");

        RuleFor(input => input.IfModifiedSince)
            .GreaterThan(0)
            .When(input => input.IfModifiedSince.HasValue)
            .WithMessage("IfModifiedSince must be greater than 0 when provided");
    }
}