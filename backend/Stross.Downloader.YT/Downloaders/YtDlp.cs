using System.Collections.Specialized;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Linq;
using Microsoft.Extensions.Options;
using NYoutubeDL;
using NYoutubeDL.Helpers;
using Stross.Downloader.YT.Configuration;
using Stross.Downloader.YT.Constants;
using Stross.Downloader.YT.Exceptions;
using Stross.Downloader.YT.Models;

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

    public async Task<MusicTrackMetadata> DownloadMusicTrack(string sourceUrl, string targetLocationPath,
        CancellationToken cancellationToken = default)
    {
        Uri parsedUri = new Uri(sourceUrl);
        string relativeStartPath = Path.Combine("music-tracks", targetLocationPath);
        string fullStartPath = Path.Combine(_config.OutputPath, relativeStartPath);

        string relativePathAudio = Path.Combine(relativeStartPath, $"1.{_config.AudioFormat}");
        string relativePathThumbnail = Path.Combine(relativeStartPath, "2.jpg");

        string fullOutputPathAudio = Path.Combine(_config.OutputPath, relativePathAudio);
        string fullOutputPathThumbnail = Path.Combine(_config.OutputPath, relativePathThumbnail);

        if (!Path.Exists(fullStartPath))
            Directory.CreateDirectory(fullStartPath);

        NameValueCollection parsed =
            HttpUtility.ParseQueryString(parsedUri.Query.Split('?').Skip(1).FirstOrDefault() ?? "");

        string videoId;

        if (parsed.AllKeys.Contains("v") && !string.IsNullOrEmpty(parsed["v"]))
            videoId = parsed["v"]!;
        else
            throw new YtDlpException("Video ID not found in URL");

        string sanitizedUrl = $"https://www.youtube.com/watch?v={videoId}";

        _logger.LogInformation("Loading metadata for URL: {SanitizedUrl}", sanitizedUrl);
        YoutubeVideoMetadata videoMetadata = await GetVideoMetaDataAsync(parsedUri.ToString(), cancellationToken);
        string channelId = await GetChannelIdAsync(videoMetadata.AuthorUrl, cancellationToken);
        await DownloadThumbnailAsync(videoId, fullOutputPathThumbnail, cancellationToken);
        _logger.LogDebug("Done loading metadata");

        _logger.LogInformation("Starting download for URL: {SanitizedUrl} to path: {OutputPathAudio}", sanitizedUrl,
            fullOutputPathAudio);
        _youtubeDlp.VideoUrl = parsedUri.ToString();
        _youtubeDlp.Options.FilesystemOptions.Output = fullOutputPathAudio;
        _youtubeDlp.Options.PostProcessingOptions.ExtractAudio = true;
        _youtubeDlp.YoutubeDlPath = _config.YtDlpPath;

        // Set audio format based on configuration
        if (Enum.TryParse<Enums.AudioFormat>(_config.AudioFormat, true, out Enums.AudioFormat audioFormat))
            _youtubeDlp.Options.PostProcessingOptions.AudioFormat = audioFormat;

        await _youtubeDlp.DownloadAsync();

        if (_youtubeDlp.Info.Errors.Count >= 1)
        {
            string error = _youtubeDlp.Info.Errors.First();
            _logger.LogError("Download failed: {Error}", error);

            throw new YtDlpException(error);
        }

        _logger.LogDebug("Download completed successfully for URL: {SanitizedUrl}", sanitizedUrl);

        return new MusicTrackMetadata(sanitizedUrl, videoMetadata.Title, [channelId], relativePathAudio,
            relativePathThumbnail);
    }

    public async Task<CreatorMetadata> GetCreatorMetadataAsync(string creatorId,
        CancellationToken cancellationToken = default)
    {
        string creatorName = await GetCreatorNameAsync(creatorId, cancellationToken);
        string creatorUrl = await GetCreatorUrlAsync(creatorId, cancellationToken);
        string creatorThumbnailUrl = await GetCreatorThumbnailUrlAsync(creatorId, cancellationToken);

        return new CreatorMetadata(creatorName, creatorUrl, creatorId, creatorThumbnailUrl);
    }

    private async Task<string> GetCreatorNameAsync(string creatorId, CancellationToken cancellationToken = default)
    {
        string rssFeedUrl = $"https://www.youtube.com/feeds/videos.xml?channel_id={creatorId}";

        using HttpClient authorNameClient = _httpClientFactory.CreateClient();

        string xml = await authorNameClient.GetStringAsync(rssFeedUrl, cancellationToken);

        XNamespace atom = "http://www.w3.org/2005/Atom";
        XDocument doc = XDocument.Parse(xml);

        string? channelTitle = doc.Root?.Element(atom + "title")?.Value;

        return channelTitle;
    }

    private Task<string> GetCreatorUrlAsync(string creatorId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult($"https://www.youtube.com/channel/{creatorId}");
    }

    private async Task<string> GetCreatorThumbnailUrlAsync(string creatorId,
        CancellationToken cancellationToken = default)
    {
        string url = await GetCreatorUrlAsync(creatorId, cancellationToken);

        using HttpClient youtubeChannelIdHttpClient = _httpClientFactory.CreateClient(Clients.YoutubeChannelIdClient);

        string html = await youtubeChannelIdHttpClient.GetStringAsync(url, cancellationToken);

        Match match = Regex.Match(html,
            @"https://yt3\.googleusercontent\.com/[A-Za-z0-9\-_]+=s\d+-c-k-c0x00ffffff-no-rj");
        if (match.Success)
            return match.Value;

        throw new YtDlpException("Failed to get channel thumbnail URL");
    }

    private async Task DownloadThumbnailAsync(string videoId, string outputPath,
        CancellationToken cancellationToken = default)
    {
        string thumbnailUrl = $"https://img.youtube.com/vi/{videoId}/maxresdefault.jpg";

        _logger.LogInformation("Starting thumbnail download for video ID: {VideoId} to path: {OutputPath}", videoId,
            outputPath);

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

    private async Task<YoutubeVideoMetadata> GetVideoMetaDataAsync(string videoUrl,
        CancellationToken cancellationToken = default)
    {
        string metaDataUrl = $"https://youtube.com/oembed?url={videoUrl}&format=json";

        using HttpClient httpClient = _httpClientFactory.CreateClient();

        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, metaDataUrl);

        using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);

        response.EnsureSuccessStatusCode();

        string json = await response.Content.ReadAsStringAsync(cancellationToken);

        YoutubeVideoMetadata metadata = JsonSerializer.Deserialize<YoutubeVideoMetadata>(json) ??
                                        throw new YtDlpException("Failed to parse metadata");

        return metadata;
    }

    private async Task<string> GetChannelIdAsync(string authorUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            using HttpClient httpClient = _httpClientFactory.CreateClient(Clients.YoutubeChannelIdClient);

            // Make the request
            using HttpResponseMessage response = await httpClient.GetAsync(authorUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            string htmlContent = await response.Content.ReadAsStringAsync(cancellationToken);

            // Use regex to find the channel URL pattern
            Match match = Regex.Match(htmlContent, @"https://www\.youtube\.com/channel/(UC[0-9A-Za-z_-]{22})");

            if (!match.Success)
                match = Regex.Match(htmlContent, @"""channelId"":\s*""(UC[0-9A-Za-z_-]{22})""");

            if (match.Success)
                return match.Groups[1].Value; // Return just the channel ID part

            throw new YtDlpException($"Channel ID not found in the response from URL: {authorUrl}");
        }
        catch (Exception ex) when (!(ex is YtDlpException))
        {
            _logger.LogError(ex, "Failed to get channel ID from URL: {AuthorUrl}", authorUrl);

            throw new YtDlpException($"Failed to get channel ID: {ex.Message}");
        }
    }
}