using System.Collections.Specialized;
using System.Web;
using Microsoft.Extensions.Options;
using NYoutubeDL;
using NYoutubeDL.Helpers;
using Stross.Downloader.YT.Configuration;
using Stross.Downloader.YT.Exceptions;

namespace Stross.Downloader.YT.Downloaders;

public class YtDlp
{
    private readonly YoutubeDLP _youtubeDlp;
    private readonly DownloaderConfig _config;
    private readonly ILogger<YtDlp> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public YtDlp(IOptions<DownloaderConfig> config, ILogger<YtDlp> logger, IHttpClientFactory httpClientFactory)
    {
        _config = config.Value;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _youtubeDlp = new YoutubeDLP();
    }

    public async Task<string> Download(string sourceUrl, string targetLocationPath, CancellationToken cancellationToken = default)
    {
        Uri parsedUri = new Uri(sourceUrl);
        string startPath = Path.Combine(_config.OutputPath, targetLocationPath);
        string outputPathAudio = Path.Combine(startPath, $"1.{_config.AudioFormat}");
        string outputPathThumbnail = Path.Combine(startPath, "2.jpg");
        
        NameValueCollection parsed = HttpUtility.ParseQueryString(parsedUri.Query.Split('?').Skip(1).FirstOrDefault() ?? "");

        string videoId;
        
        if(parsed.AllKeys.Contains("v") && !string.IsNullOrEmpty(parsed["v"]))
            videoId = parsed["v"]!;
        else
            throw new YtDlpException("Video ID not found in URL");
        
        _logger.LogInformation("Starting download for URL: {SourceUrl} to path: {OutputPathAudio}", sourceUrl, outputPathAudio);
        
        _youtubeDlp.VideoUrl = parsedUri.ToString();
        _youtubeDlp.Options.FilesystemOptions.Output = outputPathAudio;
        _youtubeDlp.Options.PostProcessingOptions.ExtractAudio = true;
        _youtubeDlp.YoutubeDlPath = _config.YtDlpPath;
        
        // Set audio format based on configuration
        if (Enum.TryParse<Enums.AudioFormat>(_config.AudioFormat, true, out Enums.AudioFormat audioFormat))
        {
            _youtubeDlp.Options.PostProcessingOptions.AudioFormat = audioFormat;
        }

        await _youtubeDlp.DownloadAsync();
        
        if (_youtubeDlp.Info.Errors.Count >= 1)
        {
            string error = _youtubeDlp.Info.Errors.First();
            _logger.LogError("Download failed: {Error}", error);

            throw new YtDlpException(error);
        }

        // Download thumbnail
        await DownloadThumbnailAsync(videoId, outputPathThumbnail, cancellationToken);
        
        _logger.LogInformation("Download completed successfully for URL: {SourceUrl}", sourceUrl);

        return outputPathAudio;
    }

    private async Task DownloadThumbnailAsync(string videoId, string outputPath, CancellationToken cancellationToken = default)
    {
        string thumbnailUrl = $"https://img.youtube.com/vi/{videoId}/maxresdefault.jpg";
        
        _logger.LogInformation("Starting thumbnail download for video ID: {VideoId} to path: {OutputPath}", videoId, outputPath);
        
        try
        {
            using HttpClient httpClient = _httpClientFactory.CreateClient();
            using HttpResponseMessage response = await httpClient.GetAsync(thumbnailUrl, cancellationToken);
            
            response.EnsureSuccessStatusCode();
            
            using FileStream fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
            await response.Content.CopyToAsync(fileStream, cancellationToken);
            
            _logger.LogInformation("Thumbnail downloaded successfully for video ID: {VideoId}", videoId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download thumbnail for video ID: {VideoId}", videoId);
            
            throw new YtDlpException($"Failed to download thumbnail: {ex.Message}");
        }
    }
}