using Stross.Domain.Entities;
using Stross.SubsonicModels;

namespace Stross.Application.Slices.Subsonic.Mappings;

public static class SubsonicMappingExtensions
{
    public static SubsonicModels.Playlist ToSubsonicPlaylistResponse(this Domain.Entities.Playlist playlist)
    {
        SubsonicModels.Playlist response = new SubsonicModels.Playlist
        {
            Id = playlist.Id.ToString(),
            Name = playlist.Name,
            Comment = playlist.Comment,
            Owner = playlist.Owner.UserName,
            Public = playlist.Public,
            SongCount = playlist.SongCount,
            Duration = playlist.Duration,
            Created = playlist.CreatedAt,
            Changed = playlist.UpdatedAt ?? playlist.CreatedAt,
            CoverArt = playlist.CoverArtLocation
        };

        return response;
    }

    public static PlaylistWithSongs ToSubsonicPlaylistWithSongsResponse(this Domain.Entities.Playlist playlist, Dictionary<long, DateTime>? starredSongs)
    {
        SubsonicModels.Playlist playlistResponse = playlist.ToSubsonicPlaylistResponse();

        PlaylistWithSongs response = new PlaylistWithSongs
        {
            Id = playlistResponse.Id,
            Name = playlistResponse.Name,
            Comment = playlistResponse.Comment,
            Owner = playlistResponse.Owner,
            Public = playlistResponse.Public,
            SongCount = playlistResponse.SongCount,
            Duration = playlistResponse.Duration,
            Created = playlistResponse.Created,
            Changed = playlistResponse.Changed,
            CoverArt = playlistResponse.CoverArt,
            Entry = playlist.PlaylistMusicTracks
                .OrderBy(t => t.Order)
                .Select(t => t.MusicTrack.ToSubsonicSongResponse(starredSongs?.GetValueOrDefault(t.MusicTrackId)))
                .ToList()
        };

        return response;
    }

    public static Artist ToSubsonicArtistResponse(this Creator creator, DateTime? starredDate)
    {
        Artist artist = new Artist
        {
            Id = creator.Id.ToString(),
            Name = creator.Name,
            ArtistImageUrl = "/api/v1/thumbnails/creator/" + creator.Id + "?type=thumbnail"
            // UserRatingSpecified = 
            // UserRating = null, // TODO: Implement user rating functionality
            // AverageRating = null // TODO: Implement average rating functionality
            // AverageRatingSpecified = 
        };

        if (starredDate is not null)
            artist.Starred = starredDate.Value;

        return artist;
    }

    public static Child ToSubsonicSongResponse(this Domain.Entities.MusicTrack musicTrack, DateTime? starredDate)
    {
        string? artistName = musicTrack.Creators.FirstOrDefault()?.Name;
        string? artistId = musicTrack.Creators.FirstOrDefault()?.Id.ToString();

        Child child = new Child
        {
            Id = musicTrack.Id.ToString(),
            Parent = artistId,
            Title = musicTrack.FriendlyName,
            IsDir = false,
            Artist = artistName,
            ArtistId = artistId,
            Album = musicTrack.Album?.Name,
            AlbumId = musicTrack.AlbumId.ToString(),
            CoverArt = musicTrack.Id.ToString(),
            Duration = musicTrack.Duration,
            // BitRate = null, // TODO: Extract bitrate from audio file metadata
            Path = musicTrack.AudioFileLocation,
            Suffix = Path.GetExtension(musicTrack.AudioFileLocation)?.TrimStart('.'),
            ContentType = GetContentTypeFromExtension(Path.GetExtension(musicTrack.AudioFileLocation)),
            Size = musicTrack.Size,
            Created = musicTrack.CreatedAt,
            // Year = null, // TODO: Extract year from metadata
            Genre = musicTrack.GenreId.ToString(),
            Type = MediaType.Music
        };

        if (starredDate is not null)
            child.Starred = starredDate.Value;

        return child;
    }

    public static ArtistId3 ToSubsonicArtistID3Response(this Creator creator, DateTime? starredDate)
    {
        ArtistId3 artist = new ArtistId3
        {
            Id = creator.Id.ToString(),
            Name = creator.Name,
            CoverArt = creator.Id.ToString(),
            ArtistImageUrl = "/api/v1/thumbnails/creator/" + creator.Id + "?type=thumbnail",
            AlbumCount = creator.Albums.Count,
        };

        if (starredDate is not null)
            artist.Starred = starredDate.Value;

        return artist;
    }

    public static MusicFolder ToSubsonicMusicFolderResponse(this Domain.Entities.Provider provider)
    {
        return new MusicFolder
        {
            Id = (int)provider.Id,
            Name = provider.Name
        };
    }

    public static Child ToSubsonicDirectoryChildResponse(this Creator creator)
    {
        return new Child
        {
            Id = creator.Id.ToString(),
            Title = creator.Name,
            IsDir = true, // Creators are directories in the directory structure
            Artist = creator.Name
            // No other properties needed for directory entries
        };
    }

    public static Artist ToSubsonicIndexArtistResponse(this Creator creator, DateTime? starredDate)
    {
        Artist artist = new Artist
        {
            Id = creator.Id.ToString(),
            Name = creator.Name,
        };

        if (starredDate is not null)
            artist.Starred = starredDate.Value;

        return artist;
    }

    public static ArtistWithAlbumsId3 ToSubsonicArtistWithAlbumsResponse(this Creator creator, DateTime? starredDate, Dictionary<long, DateTime>? starredAlbums)
    {
        ArtistWithAlbumsId3 artist = new ArtistWithAlbumsId3
        {
            Id = creator.Id.ToString(),
            Name = creator.Name,
            ArtistImageUrl = $"/api/v1/thumbnails/creator/{creator.Id}?type=thumbnail",
            CoverArt = creator.Id.ToString(),
            AlbumCount = creator.Albums.Count,
            Album = creator.Albums.Select(a => a.ToSubsonicAlbumId3Response(starredAlbums?.GetValueOrDefault(a.Id))).ToList(),
        };

        if (starredDate is not null)
            artist.Starred = starredDate.Value;

        return artist;
    }

    public static AlbumId3 ToSubsonicAlbumId3Response(this Album album, DateTime? starredDate)
    {
        Creator? primaryCreator = album.Creators.FirstOrDefault();

        AlbumId3 albumTransformed = new AlbumId3
        {
            Id = album.Id.ToString(),
            Name = album.Name,
            Artist = primaryCreator?.Name,
            ArtistId = primaryCreator?.Id.ToString(),
            CoverArt = album.Id.ToString(),
            SongCount = album.MusicTracks.Count,
            Duration = album.MusicTracks.Sum(m => m.Duration),
            Created = album.CreatedAt,
            // Year = null, // TODO: Extract year from album metadata
            Genre = album.Genre!.Name,
        };

        if (starredDate is not null)
            albumTransformed.Starred = starredDate.Value;

        return albumTransformed;
    }

    public static AlbumWithSongsId3 ToSubsonicAlbumWithSongsResponse(this Album album, DateTime? albumStarredDate, Dictionary<long, DateTime>? starredSongs)
    {
        Creator? primaryCreator = album.Creators.FirstOrDefault();

        AlbumWithSongsId3 albumTransformed = new AlbumWithSongsId3
        {
            Id = album.Id.ToString(),
            Name = album.Name,
            Artist = primaryCreator?.Name,
            ArtistId = primaryCreator?.Id.ToString(),
            CoverArt = album.Id.ToString(),
            SongCount = album.MusicTracks.Count,
            Duration = album.MusicTracks.Sum(m => m.Duration),
            Created = album.CreatedAt,
            Song = album.MusicTracks.Select(t => t.ToSubsonicSongResponse(starredSongs?.GetValueOrDefault(t.Id))).ToList(),
            // Year = null, // TODO: Extract year from album metadata
            Genre = album.Genre!.Name,
        };

        if (albumStarredDate is not null)
            albumTransformed.Starred = albumStarredDate.Value;

        return albumTransformed;
    }

    public static Child ToSubsonicAlbumListResponse(this Album album, DateTime? starredDate)
    {
        Creator? primaryCreator = album.Creators.FirstOrDefault();

        Child child = new Child
        {
            Id = album.Id.ToString(),
            Title = album.Name,
            Album = album.Name,
            Artist = primaryCreator?.Name,
            ArtistId = primaryCreator?.Id.ToString(),
            IsDir = true, // Albums are directories in album list context
            CoverArt = album.Id.ToString(),
            Created = album.CreatedAt,
            // Year = null, // TODO: Extract year from album metadata
            Genre = album.Genre?.Name,
            Type = MediaType.Music,
            Duration = album.MusicTracks.Sum(m => m.Duration),
            Size = album.MusicTracks.Sum(m => m.Size)
        };

        if (starredDate is not null)
            child.Starred = starredDate.Value;

        return child;
    }

    private static string? GetContentTypeFromExtension(string? extension)
    {
        return extension?.ToLowerInvariant() switch
        {
            ".mp3" => "audio/mpeg",
            ".flac" => "audio/flac",
            ".ogg" => "audio/ogg",
            ".wav" => "audio/wav",
            ".m4a" => "audio/mp4",
            ".aac" => "audio/aac",
            _ => "audio/mpeg" // Default to mp3
        };
    }
}
