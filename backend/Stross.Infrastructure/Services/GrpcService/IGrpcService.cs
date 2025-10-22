using Stross.Proto;

namespace Stross.Infrastructure.Services.GrpcService;

public interface IGrpcService
{
    public Task<DownloadMusicTrackReply> SendDownloadYtAudioAsync(string url, CancellationToken cancellationToken = default);
    public Task<GetCreatorMetadataReply> GetCreatorMetadataAsync(string creatorId, CancellationToken cancellationToken = default);
}