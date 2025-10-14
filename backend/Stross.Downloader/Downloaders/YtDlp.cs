using NYoutubeDL;
using NYoutubeDL.Helpers;
using NYoutubeDL.Options;
using Stross.Downloader.Exceptions;

namespace Stross.Downloader;

public class YtDlp
{
    private readonly YoutubeDLP _youtubeDlp;

    public YtDlp()
    {
        _youtubeDlp = new YoutubeDLP();
    }

    public async Task Download(string url, string outputPath, string ytdlPath="/home/brent/stross/yt-dlp")
    {
        Uri parsedUri = new Uri(url);
        _youtubeDlp.VideoUrl = parsedUri.ToString();
        _youtubeDlp.Options.FilesystemOptions.Output = outputPath;
        _youtubeDlp.Options.PostProcessingOptions.ExtractAudio = true; // only download audio
        _youtubeDlp.YoutubeDlPath = ytdlPath;
        _youtubeDlp.Options.PostProcessingOptions.AudioFormat = Enums.AudioFormat.mp3;
        _youtubeDlp.Options.PostProcessingOptions.AudioQuality = "10"; // Download on best quality
        await _youtubeDlp.DownloadAsync();
        if (_youtubeDlp.Info.Errors.Count >= 1)
            throw new YtDlpException(_youtubeDlp.Info.Errors.First());;
    }
}