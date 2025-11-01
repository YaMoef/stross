using Stross.Domain.Seedwork;

namespace Stross.Domain.Entities;

public class ExternalCreatorMusicTrack : BaseEntity
{
    public bool IsDefaultMusicTrackProvider { get; private set; } = true;

    public string ExternalId { get; private set; }
    public string ExternalName { get; private set; }
    public string ThumbnailLocation { get; private set; }
    public string ExternalUrl { get; private set; }

    public Creator Creator { get; private set; }
    public long CreatorId { get; private set; }

    public Provider Provider { get; private set; }
    public long MusicTrackProviderId { get; private set; }

    private ExternalCreatorMusicTrack()
    {
    }

    public ExternalCreatorMusicTrack(
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
        MusicTrackProviderId = provider.Id;
        ExternalId = externalId;
        ExternalName = externalName;
        ThumbnailLocation = thumbnailLocation;
        ExternalUrl = externalUrl;
    }

    public ExternalCreatorMusicTrack SetExternalName(string name)
    {
        ExternalName = name;

        return this;
    }

    public ExternalCreatorMusicTrack SetExternalUrl(string url)
    {
        ExternalUrl = url;

        return this;
    }
}