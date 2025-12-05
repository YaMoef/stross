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

        public static PlaylistWithSongs ToSubsonicPlaylistWithSongsResponse(this Domain.Entities.Playlist playlist)
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
                    .Select(t => t.MusicTrack.ToSubsonicSongResponse())
                    .ToList()
            };

            return response;
        }

    public static Artist ToSubsonicArtistResponse(this Creator creator)
    {
        return new Artist
        {
            Id = creator.Id.ToString(),
            Name = creator.Name,
            // StarredSpecified = 
            // Starred = null, // TODO: Implement starring functionality
            ArtistImageUrl = "/api/v1/thumbnails/creator/" + creator.Id + "?type=thumbnail"
            // UserRatingSpecified = 
            // UserRating = null, // TODO: Implement user rating functionality
            // AverageRating = null // TODO: Implement average rating functionality
            // AverageRatingSpecified = 
        };
    }

    public static Child ToSubsonicSongResponse(this Domain.Entities.MusicTrack musicTrack)
    {
        string? artistName = musicTrack.Creators.FirstOrDefault()?.Name;
        string? artistId = musicTrack.Creators.FirstOrDefault()?.Id.ToString();

        return new Child
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
    }

    public static ArtistId3 ToSubsonicArtistID3Response(this Creator creator)
    {
        return new ArtistId3
        {
            Id = creator.Id.ToString(),
            Name = creator.Name,
            CoverArt = creator.Id.ToString(),
            ArtistImageUrl = "/api/v1/thumbnails/creator/" + creator.Id + "?type=thumbnail",
            AlbumCount = creator.Albums.Count
            // Starred = null // TODO: Implement starring functionality
        };
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

    public static Artist ToSubsonicIndexArtistResponse(this Creator creator)
    {
        return new Artist
        {
            Id = creator.Id.ToString(),
            Name = creator.Name
            // Starred = null // TODO: Implement starring functionality
        };
    }

    public static ArtistWithAlbumsId3 ToSubsonicArtistWithAlbumsResponse(this Creator creator)
    {
        return new ArtistWithAlbumsId3
        {
            Id = creator.Id.ToString(),
            Name = creator.Name,
            ArtistImageUrl = $"/api/v1/thumbnails/creator/{creator.Id}?type=thumbnail",
            CoverArt = creator.Id.ToString(),
            AlbumCount = creator.Albums.Count,
            Album = creator.Albums.Select(a => a.ToSubsonicAlbumId3Response()).ToList()
            // Starred = null // TODO: Implement starring functionality
        };
    }

    public static AlbumId3 ToSubsonicAlbumId3Response(this Album album)
    {
        Creator? primaryCreator = album.Creators.FirstOrDefault();

        return new AlbumId3
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
            Genre = album.Genre!.Name
        };
    }

    public static AlbumWithSongsId3 ToSubsonicAlbumWithSongsResponse(this Album album)
    {
        Creator? primaryCreator = album.Creators.FirstOrDefault();

        return new AlbumWithSongsId3
        {
            Id = album.Id.ToString(),
            Name = album.Name,
            Artist = primaryCreator?.Name,
            ArtistId = primaryCreator?.Id.ToString(),
            CoverArt = album.Id.ToString(),
            SongCount = album.MusicTracks.Count,
            Duration = album.MusicTracks.Sum(m => m.Duration),
            Created = album.CreatedAt,
            Song = album.MusicTracks.Select(t => t.ToSubsonicSongResponse()).ToList(),
            // Year = null, // TODO: Extract year from album metadata
            Genre = album.Genre!.Name
        };
    }

    public static Child ToSubsonicAlbumListResponse(this Album album)
    {
        Creator? primaryCreator = album.Creators.FirstOrDefault();

        return new Child
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