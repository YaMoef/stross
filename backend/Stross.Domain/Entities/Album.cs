using Stross.Domain.Seedwork;

namespace Stross.Domain.Entities;

public class Album : BaseEntity
{
    public string Name { get; private set; }

    public Genre? Genre { get; private set; }
    public long GenreId { get; private set; }

    private readonly List<Creator> _creators = [];

    private readonly List<ExternalAlbum> _externalAlbums = [];

    private readonly List<MusicTrack> _musicTracks = [];

    public IReadOnlyCollection<ExternalAlbum> ExternalAlbums => _externalAlbums;

    public IReadOnlyCollection<MusicTrack> MusicTracks => _musicTracks;

    public IReadOnlyCollection<Creator> Creators => _creators;

    private Album()
    {
    }

    public Album(
        Creator creator,
        Provider provider,
        Genre genre,
        string externalId,
        string externalName,
        string thumbnailLocation,
        string externalUrl)
    {
        Name = externalName;
        Genre = genre;
        GenreId = genre.Id;

        _creators.Add(creator);
        _externalAlbums.Add(new ExternalAlbum(provider, externalId,
            externalName, thumbnailLocation, externalUrl));
    }

    public Album(
        Creator creator,
        Genre genre,
        string name)
    {
        Name = name;
        Genre = genre;
        GenreId = genre.Id;

        _creators.Add(creator);
    }
}