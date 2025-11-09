using Stross.Domain.Seedwork;

namespace Stross.Domain.Entities;

public class ExternalCreator : BaseEntity
{
    // TODO: rename this flag
    public bool IsDefaultExternalCreator { get; private set; } = true;

    public string ExternalId { get; private set; }
    public string ExternalName { get; private set; }
    public string ThumbnailLocation { get; private set; }
    public string ExternalUrl { get; private set; }

    public Creator? Creator { get; private set; }
    public long CreatorId { get; private set; }

    public Provider? Provider { get; private set; }
    public long ProviderId { get; private set; }

    private ExternalCreator()
    {
    }

    public ExternalCreator(
        Creator creator,
        Provider provider,
        string externalId,
        string externalName,
        string thumbnailLocation,
        string externalUrl)
    {
        Creator = creator;
        CreatorId = creator.Id;
        Provider = provider;
        ProviderId = provider.Id;
        ExternalId = externalId;
        ExternalName = externalName;
        ThumbnailLocation = thumbnailLocation;
        ExternalUrl = externalUrl;
    }

    public ExternalCreator SetExternalName(string name)
    {
        ExternalName = name;

        return this;
    }

    public ExternalCreator SetExternalUrl(string url)
    {
        ExternalUrl = url;

        return this;
    }
}