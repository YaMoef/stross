using Stross.Domain.Seedwork;

namespace Stross.Domain.Entities;

public class Genre : BaseEntity
{
    public string Name { get; private set; }


    private readonly List<Album> _albums = [];

    private readonly List<MusicTrack> _musicTracks = [];

    public IReadOnlyCollection<Album> Albums => _albums;

    public IReadOnlyCollection<MusicTrack> MusicTracks => _musicTracks;


    public Genre(string name)
    {
        Name = name;
    }
}