namespace Stross.Downloader.YT.Models;

public record MusicTrackMetadata(string SourceUrl, string Title, IReadOnlyCollection<string> CreatorIds, string TargetLocationPath);