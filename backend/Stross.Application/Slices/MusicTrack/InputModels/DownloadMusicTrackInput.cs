namespace Stross.Application.Slices.MusicTrack.InputModels;

public sealed record DownloadMusicTrackInput(long ProviderId, string SourceUrl);