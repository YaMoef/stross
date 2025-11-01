using Stross.Domain.Seedwork;

namespace Stross.Domain.Entities;

public class Creator : BaseEntity
{
    public string Name { get; private set; }

    private readonly List<ExternalCreatorMusicTrack> _externalCreatorMusicTrack = [];

    private readonly List<MusicTrack> _musicTracks = [];

    public IReadOnlyCollection<ExternalCreatorMusicTrack> ExternalCreatorMusicTrack => _externalCreatorMusicTrack;

    public IReadOnlyCollection<MusicTrack> MusicTracks => _musicTracks;

    private Creator()
    {
    }

    public Creator(
        Provider provider,
        string externalId,
        string externalName,
        string thumbnailLocation,
        string externalUrl)
    {
        Name = externalName;

        _externalCreatorMusicTrack.Add(new ExternalCreatorMusicTrack(this, provider, externalId,
            externalName, thumbnailLocation, externalUrl));
    }
}