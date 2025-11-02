namespace Stross.Application.Slices.Subsonic.ResponseModels;

/// <summary>
/// Response model for the Subsonic stream operation.
/// Contains the file path and content type information needed for streaming.
/// </summary>
public sealed record SubsonicStreamResponse(string FilePath, string ContentType, long FileSize, string FileName);