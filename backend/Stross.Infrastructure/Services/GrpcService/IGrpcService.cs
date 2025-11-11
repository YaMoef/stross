using Stross.Domain.Entities;
using Stross.Infrastructure.Services.GrpcService.Models;
using Stross.Proto;

namespace Stross.Infrastructure.Services.GrpcService;

public interface IGrpcService
{
    public Task<DownloadedMusicTrack> DownloadMusicTrackAsync(string sourceUrl, Provider providerToUse, CancellationToken cancellationToken = default);
    public Task<FetchedCreatorMetadata> GetCreatorMetadataAsync(string creatorId, Provider providerToUse, CancellationToken cancellationToken = default);
    public Task<bool> PingAsync(string providerUrl, CancellationToken cancellationToken = default);
    public Task<bool> PingAsync(Provider provider, CancellationToken cancellationToken = default);
    public Task<string> GetSanitizedSourceUrlAsync(string sourceUrl, Provider providerToUse, CancellationToken cancellationToken = default);
}