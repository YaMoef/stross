using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using Proto;

namespace Stross.Infrastructure.Services.GrpcService;

public class GrpcService : IGrpcService
{
    public async Task<bool> SendDownloadYtAudio(string url)
    {
        AppContext.SetSwitch(
            "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
        var channel = GrpcChannel.ForAddress("http://localhost:5000", new GrpcChannelOptions
        {
            HttpHandler = new GrpcWebHandler(new HttpClientHandler())
        });
        
        //using var channel = GrpcChannel.ForAddress("http://localhost:5000").
        var client = new YTDownloader.YTDownloaderClient(channel);
        var reply = await client.DownloadAsync(new YtDownloadRequest{Url = url});
        Console.Out.WriteLine("File download succceeded: "+reply.Succeeded);
        return reply.Succeeded;
    }
}