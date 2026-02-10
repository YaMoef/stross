using Grpc.Core;
using Stross.Downloader.YT.Downloaders;
using Stross.Downloader.YT.Exceptions;
using Stross.Downloader.YT.Models;
using Stross.Proto;

namespace Stross.Downloader.YT.Services;

public class YtDlpService : Proto.Downloader.DownloaderBase
{
    private readonly YtDlp _downloader;
    private readonly ILogger<YtDlpService> _logger;

    public YtDlpService(YtDlp downloader, ILogger<YtDlpService> logger)
    {
        _downloader = downloader;
        _logger = logger;
    }

    public override async Task<DownloadMusicTrackReply> DownloadMusicTrack(DownloadMusicTrackRequest request,
        ServerCallContext context)
    {
        try
        {
            MusicTrackMetadata outputData =
                await _downloader.DownloadMusicTrack(request.SourceUrl, request.TargetLocationPath);

            DownloadMusicTrackReply reply = new DownloadMusicTrackReply
            {
                Error = "",
                Succeeded = true,

                Title = outputData.Title,
                SourceUrl = outputData.SourceUrl,
                MusicTrackPath = outputData.MusicTrackPath,
                ThumbnailPath = outputData.ThumbnailPath,
                CreatorIds =
                {
                    outputData.CreatorIds
                }
            };

            return reply;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Download failed for URL: {Url}", request.SourceUrl);

            return new DownloadMusicTrackReply
            {
                Error = ex.Message,
                Succeeded = false
            };
        }
    }

    public override async Task<GetCreatorMetadataReply> GetCreatorMetadata(GetCreatorMetadataRequest request,
        ServerCallContext context)
    {
        try
        {
            CreatorMetadata creatorMetadata = await _downloader.GetCreatorMetadataAsync(request.CreatorId);

            return new GetCreatorMetadataReply
            {
                Error = "",
                Succeeded = true,

                CreatorId = creatorMetadata.Id,
                CreatorName = creatorMetadata.Name,
                CreatorThumbnailImageUrl = creatorMetadata.ThumbnailUrl,
                CreatorUrl = creatorMetadata.Url
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get metadata for creatorId: {CreatorId}", request.CreatorId);

            return new GetCreatorMetadataReply
            {
                Error = ex.Message,
                Succeeded = false
            };
        }
    }

    public override Task<PingReply> Ping(PingRequest request, ServerCallContext context)
    {
        return Task.FromResult(new PingReply
        {
            Ready = true
        });
    }

    public override Task<SanitizeSourceUrlReply> SanitizeSourceUrl(SanitizeSourceUrlRequest request, ServerCallContext context)
    {
        try
        {
            return Task.FromResult(new SanitizeSourceUrlReply
            {
                SanitizedUrl = YtDlp.SanitizeYoutubeUrl(request.SourceUrl)
            });
        }
        catch (YtDlpException ex)
        {
            _logger.LogWarning(ex, "Invalid URL provided.");

            return Task.FromResult(new SanitizeSourceUrlReply
            {
                Succeeded = false,
                Error = "Invalid URL provided."
            });
        }
    }
}
