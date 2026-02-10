namespace Stross.Downloader.SoundCloud.Models;

public record MusicTrackMetadata(
    string SourceUrl,
    string Title,
    IReadOnlyCollection<string> CreatorIds,
    string MusicTrackPath,
    string ThumbnailPath);
