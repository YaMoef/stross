using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Stross.Domain.Entities;
using Stross.Exception.Exceptions;
using Stross.Infrastructure.Services.GrpcService.Models;
using Stross.Proto;

namespace Stross.Infrastructure.Services.GrpcService;

public class GrpcService : IGrpcService
{
    private readonly ILogger<GrpcService> _logger;

    public GrpcService(ILogger<GrpcService> logger)
    {
        _logger = logger;
    }

    public async Task<DownloadedMusicTrack> DownloadMusicTrackAsync(string sourceUrl, Provider providerToUse, CancellationToken cancellationToken = default)
    {
        // Enable HTTP/2 over unencrypted connections for gRPC
        string targetPath = Guid.NewGuid().ToString();

        using GrpcChannel channel = GrpcChannel.ForAddress(providerToUse.Url);
        Downloader.DownloaderClient client = new Downloader.DownloaderClient(channel);
        DownloadMusicTrackReply reply = await client.DownloadMusicTrackAsync(new DownloadMusicTrackRequest
        {
            SourceUrl = sourceUrl,
            TargetLocationPath = targetPath
        }, cancellationToken: cancellationToken);

        if (!reply.Succeeded)
            throw new ProviderException("Failed to download music track. Reason: " + reply.Error);

        return new DownloadedMusicTrack(reply.SourceUrl, reply.Title, reply.CreatorIds.ToList(), reply.MusicTrackPath, reply.ThumbnailPath);
    }

    public async Task<FetchedCreatorMetadata> GetCreatorMetadataAsync(string creatorId, Provider providerToUse, CancellationToken cancellationToken = default)
    {
        using GrpcChannel channel = GrpcChannel.ForAddress(providerToUse.Url);
        Downloader.DownloaderClient client = new Downloader.DownloaderClient(channel);

        GetCreatorMetadataReply reply = await client.GetCreatorMetadataAsync(new GetCreatorMetadataRequest
        {
            CreatorId = creatorId
        },
            cancellationToken: cancellationToken);

        return new FetchedCreatorMetadata(reply.CreatorId, reply.CreatorName, reply.CreatorUrl, reply.CreatorThumbnailImageUrl);
    }

    public async Task<bool> PingAsync(string providerUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            using GrpcChannel channel = GrpcChannel.ForAddress(providerUrl);
            Downloader.DownloaderClient client = new Downloader.DownloaderClient(channel);

            PingReply reply = await client.PingAsync(new PingRequest(), cancellationToken: cancellationToken);

            return reply.Ready;
        }
        catch (System.Exception ex)
        {
            _logger.LogWarning(ex, "Failed to ping provider: {ProviderUrl}", providerUrl);

            return false;
        }
    }

    public Task<bool> PingAsync(Provider provider, CancellationToken cancellationToken = default)
    {
        return PingAsync(provider.Url, cancellationToken);
    }

    public async Task<string> GetSanitizedSourceUrlAsync(string sourceUrl, Provider providerToUse, CancellationToken cancellationToken = default)
    {
        using GrpcChannel channel = GrpcChannel.ForAddress(providerToUse.Url);
        Downloader.DownloaderClient client = new Downloader.DownloaderClient(channel);

        SanitizeSourceUrlReply reply = await client.SanitizeSourceUrlAsync(new SanitizeSourceUrlRequest
        {
            SourceUrl = sourceUrl
        }, cancellationToken: cancellationToken);

        if (reply.Error == "Invalid URL provided.")
            throw new ValidationException(reply.Error);

        return reply.SanitizedUrl;
    }
}