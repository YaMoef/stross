using Grpc.Core;
using Proto;

namespace Stross.Downloader.Services;

public class YtDlpService: YTDownloader.YTDownloaderBase
{
    public override async Task<YtDownloadReply> Download(YtDownloadRequest request, ServerCallContext context)
    {
        YtDlp downloader = new YtDlp();
        string uid = new Guid().ToString();
        try
        {
            await downloader.Download(request.Url.ToString(), "/home/brent/stross/" + uid);
        }
        catch (Exception ex)
        {
            return new YtDownloadReply() { Error = ex.Message, Succeeded = false };
        }
        return new YtDownloadReply() { Error = "", Succeeded = false };
    }
}