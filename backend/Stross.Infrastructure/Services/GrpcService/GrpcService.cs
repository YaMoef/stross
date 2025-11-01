using Grpc.Net.Client;
using Stross.Domain.Entities;
using Stross.Infrastructure.Services.GrpcService.Models;
using Stross.Proto;

namespace Stross.Infrastructure.Services.GrpcService;

public class GrpcService : IGrpcService
{
    public async Task<DownloadedMusicTrack> DownloadMusicTrackAsync(string sourceUrl, Provider providerToUse, CancellationToken cancellationToken = default)
    {
        // Enable HTTP/2 over unencrypted connections for gRPC
        string targetPath = Guid.NewGuid().ToString();

        using GrpcChannel channel = GrpcChannel.ForAddress(providerToUse.Url);
        Downloader.DownloaderClient client = new Downloader.DownloaderClient(channel);
        DownloadMusicTrackReply reply = await client.DownloadMusicTrackAsync(new DownloadMusicTrackRequest
            { SourceUrl = sourceUrl, TargetLocationPath = targetPath }, cancellationToken:cancellationToken);

        return new DownloadedMusicTrack(reply.SourceUrl, reply.Title, reply.CreatorIds.ToList(), reply.MusicTrackPath, reply.ThumbnailPath, reply.MusicTrackUrl);
    }

    public async Task<FetchedCreatorMetadata> GetCreatorMetadataAsync(string creatorId, Provider providerToUse, CancellationToken cancellationToken = default)
    {
        using GrpcChannel channel = GrpcChannel.ForAddress(providerToUse.Url);
        Downloader.DownloaderClient client = new Downloader.DownloaderClient(channel);

        GetCreatorMetadataReply reply = await client.GetCreatorMetadataAsync(new GetCreatorMetadataRequest
            {
                CreatorId = creatorId
            },
            cancellationToken:cancellationToken);

        return new FetchedCreatorMetadata(reply.CreatorId, reply.CreatorName, reply.CreatorUrl, reply.CreatorThumbnailImageUrl);
    }
}