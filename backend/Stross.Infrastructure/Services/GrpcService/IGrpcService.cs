namespace Stross.Infrastructure.Services.GrpcService;

public interface IGrpcService
{
    public Task<bool> SendDownloadYtAudio(string url);
}