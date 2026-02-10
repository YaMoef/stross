namespace Stross.Infrastructure.Services.ThumbnailService;

public class ThumbnailService : IThumbnailService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ThumbnailService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task GetThumbnailUrlAsync(string sourceUrl, string targetLocationPath, CancellationToken cancellationToken = default)
    {
        // TODO: check if file should be updated

        if (File.Exists(targetLocationPath))
            File.Delete(targetLocationPath);

        string directoryLocation = Path.GetDirectoryName(targetLocationPath)!;

        if (!Directory.Exists(directoryLocation))
            Directory.CreateDirectory(directoryLocation);

        using HttpClient httpClient = _httpClientFactory.CreateClient();
        using HttpResponseMessage response = await httpClient.GetAsync(sourceUrl, cancellationToken);

        response.EnsureSuccessStatusCode();

        using FileStream fileStream = new FileStream(targetLocationPath, FileMode.Create, FileAccess.Write);
        await response.Content.CopyToAsync(fileStream, cancellationToken);
    }
}
