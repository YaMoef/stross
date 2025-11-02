using Stross.Domain.Seedwork;

namespace Stross.Domain.Entities;

public class MusicTrack : BaseEntity
{
    public string AudioFileLocation { get; private set; }
    public string OriginalName { get; private set; }
    public string FriendlyName { get; private set; }
    public string ThumbnailLocation { get; private set; }
    public string ExternalUrl { get; private set; }

    public Provider? Provider { get; private set; }
    public long ProviderId { get; private set; }

    private readonly List<Creator> _creators = [];
    public IReadOnlyCollection<Creator> Creators => _creators;


    private MusicTrack()
    {
    }

    public MusicTrack(
        Provider provider,
        string audioFileLocation,
        string name,
        string thumbnailLocation,
        IReadOnlyCollection<Creator> creators,
        string externalUrl)
    {
        Provider = provider;
        ProviderId = provider.Id;
        _creators.AddRange(creators);

        AudioFileLocation = audioFileLocation;
        OriginalName = name;
        FriendlyName = name;
        ThumbnailLocation = thumbnailLocation;
        ExternalUrl = externalUrl;
    }
}