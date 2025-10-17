namespace Stross.Infrastructure.Services.GrpcService;

public interface IGrpcService
{
    public Task<bool> SendDownloadYtAudioAsync(string url, CancellationToken cancellationToken = default);
}