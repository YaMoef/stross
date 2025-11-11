using Stross.Domain.Seedwork;

namespace Stross.Domain.Entities;

public class MusicTrack : BaseEntity
{
    public string AudioFileLocation { get; private set; }
    public string OriginalName { get; private set; }
    public string FriendlyName { get; private set; }
    public string ThumbnailLocation { get; private set; }
    public string ExternalUrl { get; private set; }
    public int Duration { get; private set; }
    public long Size { get; private set; }

    public Provider? Provider { get; private set; }
    public long ProviderId { get; private set; }

    public Album? Album { get; private set; }
    public long AlbumId { get; private set; }

    public Genre? Genre { get; private set; }
    public long GenreId { get; private set; }


    private readonly List<Creator> _creators = [];
    public IReadOnlyCollection<Creator> Creators => _creators;


    private MusicTrack()
    {
    }

    public MusicTrack(
        Provider provider,
        Album album,
        Genre genre,
        string audioFileLocation,
        string name,
        string thumbnailLocation,
        IReadOnlyCollection<Creator> creators,
        string externalUrl,
        int duration,
        long size)
    {
        Provider = provider;
        ProviderId = provider.Id;

        Album = album;
        AlbumId = album.Id;

        Genre = genre;
        GenreId = genre.Id;

        _creators.AddRange(creators);

        AudioFileLocation = audioFileLocation;
        OriginalName = name;
        FriendlyName = name;
        ThumbnailLocation = thumbnailLocation;
        ExternalUrl = externalUrl;
        Duration = duration;
        Size = size;
    }
}