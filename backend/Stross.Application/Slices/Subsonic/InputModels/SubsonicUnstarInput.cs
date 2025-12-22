using FluentValidation;

namespace Stross.Application.Slices.Subsonic.InputModels;

/// <summary>
/// Input model for the Subsonic unstar command.
/// Used to unstar songs, albums and artists.
/// </summary>
public sealed record SubsonicUnstarInput
{
    /// <summary>
    /// ID of a song to unstar. Multiple parameters allowed.
    /// </summary>
    public string[]? Id { get; init; }

    /// <summary>
    /// ID of an album to unstar. Multiple parameters allowed.
    /// </summary>
    public string[]? AlbumId { get; init; }

    /// <summary>
    /// ID of an artist to unstar. Multiple parameters allowed.
    /// </summary>
    public string[]? ArtistId { get; init; }
}

public sealed class SubsonicUnstarInputValidator : AbstractValidator<SubsonicUnstarInput>
{
    public SubsonicUnstarInputValidator()
    {
        RuleFor(x => x)
            .Must(HasAtLeastOneId)
            .WithMessage("At least one id, albumId or artistId must be provided");
    }

    private static bool HasAtLeastOneId(SubsonicUnstarInput input)
    {
        bool hasSongIds = input.Id is { Length: > 0 };
        bool hasAlbumIds = input.AlbumId is { Length: > 0 };
        bool hasArtistIds = input.ArtistId is { Length: > 0 };

        return hasSongIds || hasAlbumIds || hasArtistIds;
    }
}
