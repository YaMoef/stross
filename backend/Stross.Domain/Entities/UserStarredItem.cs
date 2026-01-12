using Stross.Domain.Seedwork;

namespace Stross.Domain.Entities;

public class UserStarredItem : BaseEntity
{
    public User User { get; private set; } = null!;
    public long UserId { get; private set; }

    public MusicTrack? MusicTrack { get; private set; }
    public long? MusicTrackId { get; private set; }

    public long? AlbumId { get; private set; }
    public Album? Album { get; private set; }

    public long? ArtistId { get; private set; }
    public Creator? Artist { get; private set; }

    private UserStarredItem()
    {
    }

    public UserStarredItem(User user, MusicTrack musicTrack)
    {
        User = user;
        UserId = user.Id;

        MusicTrack = musicTrack;
        MusicTrackId = musicTrack.Id;
    }


    public UserStarredItem(User user, Album album)
    {
        User = user;
        UserId = user.Id;

        Album = album;
        AlbumId = album.Id;
    }

    public UserStarredItem(User user, Creator artist)
    {
        User = user;
        UserId = user.Id;

        Artist = artist;
        ArtistId = artist.Id;
    }
}