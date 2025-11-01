namespace Stross.Infrastructure.Services.ThumbnailService;

public interface IThumbnailService
{
    public Task GetThumbnailUrlAsync(string sourceUrl, string targetLocationPath, CancellationToken cancellationToken = default);
}