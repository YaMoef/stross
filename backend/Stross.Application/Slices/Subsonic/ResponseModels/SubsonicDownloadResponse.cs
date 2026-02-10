namespace Stross.Application.Slices.Subsonic.ResponseModels;

/// <summary>
/// Response model for the Subsonic download operation.
/// Contains the file path and content type information needed for downloading.
/// </summary>
public sealed record SubsonicDownloadResponse(string FilePath, string ContentType, long FileSize, string FileName);
