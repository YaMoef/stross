using Grpc.Net.Client;
using Stross.Proto;

namespace Stross.Infrastructure.Services.GrpcService;

public class GrpcService : IGrpcService
{
    public async Task<DownloadMusicTrackReply> SendDownloadYtAudioAsync(string url, CancellationToken cancellationToken = default)
    {
        // Enable HTTP/2 over unencrypted connections for gRPC
        string targetPath = Guid.NewGuid().ToString();

        using GrpcChannel channel = GrpcChannel.ForAddress("http://localhost:5288");
        Downloader.DownloaderClient client = new Downloader.DownloaderClient(channel);
        DownloadMusicTrackReply reply = await client.DownloadMusicTrackAsync(new DownloadMusicTrackRequest()
            { SourceUrl = url, TargetLocationPath = targetPath }, cancellationToken: cancellationToken);

        return reply;
    }

    public async Task<GetCreatorMetadataReply> GetCreatorMetadataAsync(string creatorId, CancellationToken cancellationToken = default)
    {
        using GrpcChannel channel = GrpcChannel.ForAddress("http://localhost:5288");
        Downloader.DownloaderClient client = new Downloader.DownloaderClient(channel);

        GetCreatorMetadataReply reply = await client.GetCreatorMetadataAsync(new GetCreatorMetadataRequest()
            {
                CreatorId = creatorId
            },
        cancellationToken: cancellationToken);

        return reply;
    }
}