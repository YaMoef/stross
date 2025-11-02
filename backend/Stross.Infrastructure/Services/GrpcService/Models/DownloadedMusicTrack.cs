namespace Stross.Infrastructure.Services.GrpcService.Models;

public record DownloadedMusicTrack(string SourceUrl, string Title, List<string> CreatorIds, string MusicTrackPath, string ThumbnailPath);