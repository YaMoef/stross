using FluentValidation;

namespace Stross.Application.Slices.Subsonic.InputModels;

/// <summary>
/// Input model for the Subsonic getCoverArt query.
/// Used to retrieve cover art images for songs, albums, or artists.
/// </summary>
public sealed record SubsonicGetCoverArtInput
{
    /// <summary>
    /// The ID of a song, album or artist.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// If specified, scale image to this size.
    /// </summary>
    public int? Size { get; init; }
}

public sealed class SubsonicGetCoverArtInputValidator : AbstractValidator<SubsonicGetCoverArtInput>
{
    public SubsonicGetCoverArtInputValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("ID is required")
            .Must(BeValidLong)
            .WithMessage("ID must be a valid number");

        RuleFor(x => x.Size)
            .GreaterThan(0)
            .WithMessage("Size must be greater than 0")
            .When(x => x.Size.HasValue);
    }

    private static bool BeValidLong(string id)
    {
        return long.TryParse(id, out long _);
    }
}