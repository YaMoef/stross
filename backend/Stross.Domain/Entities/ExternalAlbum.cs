using Stross.Domain.Seedwork;

namespace Stross.Domain.Entities;

public class ExternalAlbum : BaseEntity
{
    public bool IsDefaultExternalAlbum { get; private set; } = true;

    public string ExternalId { get; private set; }
    public string ExternalName { get; private set; }
    public string ThumbnailLocation { get; private set; }
    public string ExternalUrl { get; private set; }

    public Provider? Provider { get; private set; }
    public long ProviderId { get; private set; }

    private ExternalAlbum()
    {
    }

    public ExternalAlbum(
        Provider provider,
        string externalId,
        string externalName,
        string thumbnailLocation,
        string externalUrl)
    {
        Provider = provider;

        ExternalId = externalId;
        ExternalName = externalName;
        ThumbnailLocation = thumbnailLocation;
        ExternalUrl = externalUrl;
    }
}