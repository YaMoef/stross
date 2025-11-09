using Stross.Domain.Seedwork;

namespace Stross.Domain.Entities;

public class Creator : BaseEntity
{
    public string Name { get; private set; }

    private readonly List<ExternalCreator> _externalCreators = [];

    private readonly List<MusicTrack> _musicTracks = [];

    private readonly List<Album> _albums = [];

    public IReadOnlyCollection<ExternalCreator> ExternalCreators => _externalCreators;

    public IReadOnlyCollection<MusicTrack> MusicTracks => _musicTracks;

    public IReadOnlyCollection<Album> Albums => _albums;

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

        _externalCreators.Add(new ExternalCreator(this, provider, externalId,
            externalName, thumbnailLocation, externalUrl));
    }

    public Creator AddAlbum(Album album)
    {
        _albums.Add(album);

        return this;
    }
}