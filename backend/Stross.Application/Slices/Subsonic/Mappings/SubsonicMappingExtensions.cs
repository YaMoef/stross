using Stross.Domain.Entities;
using Stross.SubsonicModels;

namespace Stross.Application.Slices.Subsonic.Mappings;

public static class SubsonicMappingExtensions
{
    public static Artist ToSubsonicArtistResponse(this Creator creator)
    {
        return new Artist
        {
            Id = creator.Id.ToString(),
            Name = creator.Name,
            // Starred = null, // TODO: Implement starring functionality
            ArtistImageUrl = "/api/v1/thumbnails/creator/" + creator.Id + "?type=thumbnail"
            // UserRating = null, // TODO: Implement user rating functionality
            // AverageRating = null // TODO: Implement average rating functionality
        };
    }

    public static Child ToSubsonicSongResponse(this Domain.Entities.MusicTrack musicTrack)
    {
        string? artistName = musicTrack.Creators.FirstOrDefault()?.Name;
        string? artistId = musicTrack.Creators.FirstOrDefault()?.Id.ToString();

        return new Child
        {
            Id = musicTrack.Id.ToString(),
            Title = musicTrack.FriendlyName,
            IsDir = false,
            Artist = artistName,
            ArtistId = artistId,
            Album = null, // TODO: Map from album when album entity is implemented
            AlbumId = null, // TODO: Map from album ID when album entity is implemented
            CoverArt = "/api/v1/thumbnails/music-track/" + musicTrack.Id + "?type=thumbnail",
            // Duration = null, // TODO: Extract duration from audio file metadata
            // BitRate = null, // TODO: Extract bitrate from audio file metadata
            Path = musicTrack.AudioFileLocation,
            Suffix = Path.GetExtension(musicTrack.AudioFileLocation)?.TrimStart('.'),
            ContentType = GetContentTypeFromExtension(Path.GetExtension(musicTrack.AudioFileLocation)),
            // Size = null, // TODO: Get file size from audio file
            Created = musicTrack.CreatedAt,
            // Year = null, // TODO: Extract year from metadata
            // Genre = null, // TODO: Extract genre from metadata
            Type = MediaType.Music
        };
    }

    public static ArtistId3 ToSubsonicArtistID3Response(this Creator creator)
    {
        return new ArtistId3
        {
            Id = creator.Id.ToString(),
            Name = creator.Name,
            CoverArt = null, // TODO: Map from creator thumbnail/image when available
            ArtistImageUrl = "/api/v1/thumbnails/creator/" + creator.Id + "?type=thumbnail",
            AlbumCount = 0 // TODO: Calculate album count when album functionality is implemented
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