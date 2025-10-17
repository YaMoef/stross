using Grpc.Net.Client;
using Stross.Proto;

namespace Stross.Infrastructure.Services.GrpcService;

public class GrpcService : IGrpcService
{
    public async Task<bool> SendDownloadYtAudioAsync(string url, CancellationToken cancellationToken = default)
    {
        // Enable HTTP/2 over unencrypted connections for gRPC
        string targetPath = Guid.NewGuid().ToString();

        using GrpcChannel channel = GrpcChannel.ForAddress("http://localhost:5288");
        Downloader.DownloaderClient client = new Downloader.DownloaderClient(channel);
        DownloadReply reply = await client.DownloadAsync(new DownloadRequest
            { SourceUrl = url, TargetLocationPath = targetPath }, cancellationToken: cancellationToken);

        return reply.Succeeded;
    }
}