using Stross.Domain.Seedwork;

namespace Stross.Domain.Entities;

public class Playlist : BaseEntity
{
    public string Name { get; private set; }
    public string Comment { get; private set; }
    public bool Public { get; private set; }
    public int SongCount => PlaylistMusicTracks.Count;
    public int Duration => PlaylistMusicTracks.Sum(x => x.MusicTrack.Duration);
    public string? CoverArtLocation { get; private set; }

    public User Owner { get; private set; }
    public long OwnerId { get; private set; }

    private readonly List<User> _contributors = [];
    public IReadOnlyCollection<User> Contributors => _contributors;

    private readonly List<PlaylistMusicTrack> _playlistMusicTracks = [];
    public IReadOnlyCollection<PlaylistMusicTrack> PlaylistMusicTracks => _playlistMusicTracks;

    private Playlist()
    {

    }

    public Playlist(string name, string comment, User owner)
    {
        Name = name;
        Comment = comment;
        Public = false;
        Owner = owner;
    }

    public Playlist AddContributor(User contributor)
    {
        if (!_contributors.Contains(contributor))
            _contributors.Add(contributor);

        return this;
    }

    public Playlist RemoveContributor(User contributor)
    {
        _contributors.Remove(contributor);

        return this;
    }

    public Playlist AddMusicTrack(MusicTrack musicTrack)
    {
        // we do not validate duplicates here, it is possible to add a track twice if desired
        int lastOrder = _playlistMusicTracks.MaxBy(t => t.Order)?.Order ?? -1;

        _playlistMusicTracks.Add(new PlaylistMusicTrack(this, musicTrack, lastOrder + 1));

        return this;
    }

    public Playlist RemoveMusicTrackByOrder(int order)
    {
        PlaylistMusicTrack? entryToDelete = null;

        foreach (PlaylistMusicTrack? playlistMusicTrack in _playlistMusicTracks.OrderBy(m => m.Order))
        {
            if (playlistMusicTrack.Order == order)
                entryToDelete = playlistMusicTrack;

            if (entryToDelete is not null)
                playlistMusicTrack.SetOrder(playlistMusicTrack.Order - 1);
        }

        if (entryToDelete is not null)
            _playlistMusicTracks.Remove(entryToDelete);

        return this;
    }

    public Playlist RemoveMusicTrackByOrders(IReadOnlyCollection<int>? orders)
    {
        List<PlaylistMusicTrack> entriesToDelete = _playlistMusicTracks.Where(m => orders.Contains(m.Order)).ToList();

        foreach (PlaylistMusicTrack entryToDelete in entriesToDelete)
        {
            _playlistMusicTracks.Remove(entryToDelete);
        }

        for (int i = 0; i < _playlistMusicTracks.Count; i++)
        {
            _playlistMusicTracks[i].SetOrder(i);
        }

        return this;
    }

    public Playlist RemoveMusicTrack(PlaylistMusicTrack musicTrackToDelete)
    {
        return RemoveMusicTrackByOrder(musicTrackToDelete.Order);
    }

    public Playlist ChangeName(string name)
    {
        Name = name;

        return this;
    }

    public Playlist ChangeComment(string comment)
    {
        Comment = comment;

        return this;
    }

    public Playlist SetPublic(bool isPublic)
    {
        Public = isPublic;

        return this;
    }
}
