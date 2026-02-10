using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Options;
using NYoutubeDL;
using NYoutubeDL.Helpers;
using Stross.Downloader.SoundCloud.Configuration;
using Stross.Downloader.SoundCloud.Constants;
using Stross.Downloader.SoundCloud.Exceptions;
using Stross.Downloader.SoundCloud.Models;

namespace Stross.Downloader.SoundCloud.Downloaders;

public class SoundCloud
{
    private readonly YoutubeDLP _youtubeDlp;
    private readonly DownloaderConfig _config;
    private readonly ILogger<SoundCloud> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    private static readonly HashSet<string> AllowedHosts = new(StringComparer.OrdinalIgnoreCase)
    {
        "soundcloud.com",
        "www.soundcloud.com",
        "m.soundcloud.com"
    };

    public SoundCloud(IOptions<DownloaderConfig> config, ILogger<SoundCloud> logger, IHttpClientFactory httpClientFactory)
    {
        _config = config.Value;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _youtubeDlp = new YoutubeDLP();
    }

    public async Task<MusicTrackMetadata> DownloadMusicTrack(string sourceUrl, string targetLocationPath,
        CancellationToken cancellationToken = default)
    {
        string relativeStartPath = Path.Combine("music-tracks", targetLocationPath);
        string fullStartPath = Path.Combine(_config.OutputPath, relativeStartPath);

        string relativePathAudio = Path.Combine(relativeStartPath, $"1.{_config.AudioFormat}");
        string relativePathThumbnail = Path.Combine(relativeStartPath, "2.jpg");

        string fullOutputPathAudio = Path.Combine(_config.OutputPath, relativePathAudio);
        string fullOutputPathThumbnail = Path.Combine(_config.OutputPath, relativePathThumbnail);

        if (!Path.Exists(fullStartPath))
            Directory.CreateDirectory(fullStartPath);

        string sanitizedUrl = SanitizeSoundCloudUrl(sourceUrl);

        _logger.LogInformation("Loading metadata for URL: {SanitizedUrl}", sanitizedUrl);
        SoundCloudEmbed trackData = await GetSoundCloudEmbedDataAsync(sanitizedUrl, cancellationToken);
        string creatorId = GetCreatorFromSoundcloudUrl(sanitizedUrl);
        CreatorMetadata creatorMetadata = await GetCreatorMetadataAsync(creatorId, cancellationToken);

        string trackTitle = trackData.Title.EndsWith(" by " + creatorMetadata.Name)
            ? trackData.Title.Substring(0, trackData.Title.Length - creatorMetadata.Name.Length - 4)
            : trackData.Title;

        await DownloadTrackThumbnail(trackData.ThumbnailUrl, fullOutputPathThumbnail, cancellationToken);

        _logger.LogDebug("Done loading metadata");

        _logger.LogInformation("Starting download for URL: {SanitizedUrl} to path: {OutputPathAudio}", sanitizedUrl,
            fullOutputPathAudio);
        _youtubeDlp.VideoUrl = sanitizedUrl;
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

            throw new SoundCloudException(error);
        }

        if (!File.Exists(fullOutputPathAudio))
            throw new SoundCloudException("Audio file not found after download");

        _logger.LogDebug("Download completed successfully for URL: {SanitizedUrl}", sanitizedUrl);

        return new MusicTrackMetadata(sanitizedUrl, trackTitle, [creatorId], relativePathAudio,
            relativePathThumbnail);
    }

    public async Task<CreatorMetadata> GetCreatorMetadataAsync(string creatorId, CancellationToken cancellationToken = default)
    {
        string url = $"https://soundcloud.com/{creatorId}";

        SoundCloudEmbed data = await GetSoundCloudEmbedDataAsync(url, cancellationToken);

        return new CreatorMetadata(data.Title, url, creatorId, TransformThumbnailUrl(data.ThumbnailUrl));
    }

    private async Task<bool> DownloadTrackThumbnail(string originalTrackThumbnailUrl, string outputPath, CancellationToken cancellationToken = default)
    {
        string originalJpgThumbnail = TransformThumbnailUrl(originalTrackThumbnailUrl);

        if (await DownloadThumbnailFromUrlAsync(originalJpgThumbnail, outputPath, cancellationToken))
            return true;

        _logger.LogWarning("Failed to download original jpg thumbnail for URL: {Url}", originalTrackThumbnailUrl);

        string originalPngThumbnail = originalTrackThumbnailUrl.Replace(".jpg", ".png");

        if (await DownloadThumbnailFromUrlAsync(originalPngThumbnail, outputPath, cancellationToken))
            return true;

        _logger.LogWarning("Failed to download original png thumbnail for URL: {Url}", originalTrackThumbnailUrl);

        return await DownloadThumbnailFromUrlAsync(originalTrackThumbnailUrl, outputPath, cancellationToken);
    }

    private async Task<bool> DownloadThumbnailFromUrlAsync(string thumbnailUrl, string outputPath, CancellationToken cancellationToken = default)
    {
        using HttpClient httpClient = _httpClientFactory.CreateClient();
        using HttpResponseMessage response = await httpClient.GetAsync(thumbnailUrl, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return false;

        response.EnsureSuccessStatusCode();

        using FileStream fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
        await response.Content.CopyToAsync(fileStream, cancellationToken);

        return true;
    }

    private string TransformThumbnailUrl(string url)
    {
        string result = string.Join('-', url.Split('-').SkipLast(1));

        return result + "-original.jpg";
    }

    private async Task<SoundCloudEmbed> GetSoundCloudEmbedDataAsync(string url, CancellationToken cancellationToken = default)
    {
        using HttpClient client = _httpClientFactory.CreateClient(Clients.SoundCloudClient);
        using HttpRequestMessage requestMessage = new();
        requestMessage.RequestUri = new Uri($"oembed?format=json&url={url}", UriKind.Relative);
        requestMessage.Method = HttpMethod.Get;

        using HttpResponseMessage response = await client.SendAsync(requestMessage, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new SoundCloudException($"Failed to retrieve SoundCloud embed data for URL: {url}");

        SoundCloudEmbed? embedData = JsonSerializer.Deserialize<SoundCloudEmbed>(await response.Content.ReadAsStringAsync(cancellationToken));

        return embedData ?? throw new SoundCloudException($"Failed to parse SoundCloud embed data for URL: {url}");
    }

    private static string GetCreatorFromSoundcloudUrl(string url)
    {
        Uri uri = new(url);

        return uri.Segments.Skip(1).FirstOrDefault()?.TrimEnd('/') ?? throw new SoundCloudException("Failed to load creator id from SoundCloud URL");
    }

    private static string GetTrackIdFromSoundcloudUrl(string url)
    {
        Uri uri = new(url);

        // since this is the last parameter, trimming of / is not needed
        return uri.Segments.Skip(2).FirstOrDefault() ?? throw new SoundCloudException("Failed to load track id from SoundCloud URL");
    }

    internal static string SanitizeSoundCloudUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new SoundCloudException("SoundCloud URL cannot be null or empty");

        if (!Uri.TryCreate(url.Trim(), UriKind.Absolute, out Uri? uri))
            throw new SoundCloudException("SoundCloud URL is invalid");

        // Reject non-SoundCloud domains (short links handled separately)
        if (!AllowedHosts.Contains(uri.Host))
            throw new SoundCloudException("SoundCloud URL is invalid");

        // Normalize host
        string host = "soundcloud.com";

        // Keep only the path (strip ? and #)
        string path = uri.AbsolutePath.TrimEnd('/');

        // Basic structure check: /artist/track
        string[] segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length < 2)
            throw new SoundCloudException("SoundCloud URL is invalid");

        return $"https://{host}/{segments[0]}/{segments[1]}";
    }
}
