namespace Stross.Application.Slices.Subsonic.ResponseModels;

/// <summary>
/// Response model for the Subsonic getCoverArt query.
/// Contains the binary image data and content type for serving cover art.
/// </summary>
public sealed record SubsonicGetCoverArtResponse(string FilePath, string ContentType, string? FileName = null);