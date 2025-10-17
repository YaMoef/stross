using Grpc.Core;
using Stross.Downloader.YT.Downloaders;
using Stross.Proto;

namespace Stross.Downloader.YT.Services;

public class YtDlpService: Proto.Downloader.DownloaderBase
{
    private readonly YtDlp _downloader;
    private readonly ILogger<YtDlpService> _logger;

    public YtDlpService(YtDlp downloader, ILogger<YtDlpService> logger)
    {
        _downloader = downloader;
        _logger = logger;
    }
    
    public override async Task<DownloadReply> Download(DownloadRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Received download request for URL: {Url}", request.SourceUrl);

        try
        {
            string outputPath = await _downloader.Download(request.SourceUrl, request.TargetLocationPath);
            _logger.LogInformation("Download completed successfully. Output path: {OutputPath}", outputPath);
            
            return new DownloadReply { Error = "", Succeeded = true };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Download failed for URL: {Url}", request.SourceUrl);

            return new DownloadReply { Error = ex.Message, Succeeded = false };
        }
    }
}