using FluentValidation;

namespace Stross.Application.Slices.Subsonic.InputModels;

/// <summary>
/// Input model for the Subsonic star command.
/// Used to star songs, albums and artists.
/// </summary>
public sealed record SubsonicStarInput
{
    /// <summary>
    /// ID of a song to star. Multiple parameters allowed.
    /// </summary>
    public string[]? Id { get; init; }

    /// <summary>
    /// ID of an album to star. Multiple parameters allowed.
    /// </summary>
    public string[]? AlbumId { get; init; }

    /// <summary>
    /// ID of an artist to star. Multiple parameters allowed.
    /// </summary>
    public string[]? ArtistId { get; init; }
}

public sealed class SubsonicStarInputValidator : AbstractValidator<SubsonicStarInput>
{
    public SubsonicStarInputValidator()
    {
        RuleFor(x => x)
            .Must(HasAtLeastOneId)
            .WithMessage("At least one id, albumId or artistId must be provided");
    }

    private static bool HasAtLeastOneId(SubsonicStarInput input)
    {
        bool hasSongIds = input.Id is { Length: > 0 };
        bool hasAlbumIds = input.AlbumId is { Length: > 0 };
        bool hasArtistIds = input.ArtistId is { Length: > 0 };

        return hasSongIds || hasAlbumIds || hasArtistIds;
    }
}
