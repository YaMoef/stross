namespace Stross.Downloader.YT.Configuration;

public class DownloaderConfig
{
    public static readonly string SectionName = nameof(DownloaderConfig);
    
    public required string YtDlpPath { get; init; }
    public required string OutputPath { get; init; }
    public required string AudioFormat { get; init; }
}