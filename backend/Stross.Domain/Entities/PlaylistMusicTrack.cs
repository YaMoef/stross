using Stross.Domain.Seedwork;

namespace Stross.Domain.Entities;

public class PlaylistMusicTrack : BaseEntity
{
    public int Order { get; private set; }

    public Playlist Playlist { get; private set; }
    public long PlaylistId { get; private set; }

    public MusicTrack MusicTrack { get; private set; }
    public long MusicTrackId { get; private set; }

    private PlaylistMusicTrack()
    {

    }

    public PlaylistMusicTrack(Playlist playlist, MusicTrack musicTrack, int order)
    {
        Playlist = playlist;
        MusicTrack = musicTrack;
        Order = order;
    }

    internal PlaylistMusicTrack SetOrder(int order)
    {
        Order = order;

        return this;
    }
}