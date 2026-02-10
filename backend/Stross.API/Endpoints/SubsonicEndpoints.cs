using MediatR;
using Stross.API.Helpers;
using Stross.Application.Slices.Subsonic.Commands;
using Stross.Application.Slices.Subsonic.InputModels;
using Stross.Application.Slices.Subsonic.Queries;
using Stross.Application.Slices.Subsonic.ResponseModels;

namespace Stross.API.Endpoints;

internal static class SubsonicEndpoints
{
    internal static IEndpointRouteBuilder MapSubsonicEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder subsonicGroup = endpoints.MapGroup("/rest/").RequireAuthorization();

        #region System

        // Ping endpoint - test connectivity (GET /subsonic/rest/ping)
        subsonicGroup.MapGet("ping",
                async (IMediator sender, CancellationToken cancellationToken) =>
                {
                    SubsonicPingCommand command = new SubsonicPingCommand();
                    SubsonicBaseResponse result = await sender.Send(command, cancellationToken);

                    return SubsonicResponseHelper.CreateSubsonicResult(result);
                })
            .WithName("SubsonicPing")
            .WithSummary("Test connectivity with the Subsonic server")
            .WithTags("System");

        #endregion

        #region Searching

        // Search endpoint (deprecated since 1.4.0, but still supported) (GET /subsonic/rest/search)
        subsonicGroup.MapGet("search",
                async ([AsParameters] SubsonicSearchInput input, IMediator sender, CancellationToken cancellationToken) =>
                {
                    SubsonicSearchQuery query = new SubsonicSearchQuery(input);
                    SubsonicBaseResponse result = await sender.Send(query, cancellationToken);

                    return SubsonicResponseHelper.CreateSubsonicResult(result);
                })
            .WithName("SubsonicSearch")
            .WithSummary("Search for files (deprecated, use search2 instead)")
            .WithTags("Searching");

        // Search2 endpoint - returns albums, artists and songs (GET /subsonic/rest/search2)
        subsonicGroup.MapGet("search2",
                async ([AsParameters] SubsonicSearch2Input input, IMediator sender, CancellationToken cancellationToken) =>
                {
                    SubsonicSearch2Query query = new SubsonicSearch2Query(input);
                    SubsonicBaseResponse result = await sender.Send(query, cancellationToken);

                    return SubsonicResponseHelper.CreateSubsonicResult(result);
                })
            .WithName("SubsonicSearch2")
            .WithSummary("Search for albums, artists and songs")
            .WithTags("Searching");

        // Search3 endpoint - similar to search2 but organizes music according to ID3 tags (GET /subsonic/rest/search3)
        subsonicGroup.MapGet("search3",
                async ([AsParameters] SubsonicSearch3Input input, IMediator sender, CancellationToken cancellationToken) =>
                {
                    SubsonicSearch3Query query = new SubsonicSearch3Query(input);
                    SubsonicBaseResponse result = await sender.Send(query, cancellationToken);

                    return SubsonicResponseHelper.CreateSubsonicResult(result);
                })
            .WithName("SubsonicSearch3")
            .WithSummary("Search for albums, artists and songs organized by ID3 tags")
            .WithTags("Searching");

        #endregion

        #region Browsing

        // GetMusicFolders endpoint - returns available music folders (GET /subsonic/rest/getMusicFolders)
        subsonicGroup.MapGet("getMusicFolders",
                async (IMediator sender, CancellationToken cancellationToken) =>
                {
                    SubsonicGetMusicFoldersInput input = new SubsonicGetMusicFoldersInput();
                    SubsonicGetMusicFoldersQuery query = new SubsonicGetMusicFoldersQuery(input);
                    SubsonicBaseResponse result = await sender.Send(query, cancellationToken);

                    return SubsonicResponseHelper.CreateSubsonicResult(result);
                })
            .WithName("SubsonicGetMusicFolders")
            .WithSummary("Returns available music folders")
            .WithTags("Browsing");

        // GetIndexes endpoint - returns an indexed structure of all artists (GET /subsonic/rest/getIndexes)
        subsonicGroup.MapGet("getIndexes",
                async ([AsParameters] SubsonicGetIndexesInput input, IMediator sender, CancellationToken cancellationToken) =>
                {
                    SubsonicGetIndexesQuery query = new SubsonicGetIndexesQuery(input);
                    SubsonicBaseResponse result = await sender.Send(query, cancellationToken);

                    return SubsonicResponseHelper.CreateSubsonicResult(result);
                })
            .WithName("SubsonicGetIndexes")
            .WithSummary("Returns an indexed structure of all artists")
            .WithTags("Browsing");

        // GetMusicDirectory endpoint - returns a listing of all files in a music directory (GET /subsonic/rest/getMusicDirectory)
        subsonicGroup.MapGet("getMusicDirectory",
                async ([AsParameters] SubsonicGetMusicDirectoryInput input, IMediator sender, CancellationToken cancellationToken) =>
                {
                    SubsonicGetMusicDirectoryQuery query = new SubsonicGetMusicDirectoryQuery(input);
                    SubsonicBaseResponse result = await sender.Send(query, cancellationToken);

                    return SubsonicResponseHelper.CreateSubsonicResult(result);
                })
            .WithName("SubsonicGetMusicDirectory")
            .WithSummary("Returns a listing of all files in a music directory")
            .WithTags("Browsing");

        // GetGenres endpoint - returns all genres (GET /subsonic/rest/getGenres)
        subsonicGroup.MapGet("getGenres",
                async (IMediator sender, CancellationToken cancellationToken) =>
                {
                    SubsonicGetGenresInput input = new SubsonicGetGenresInput();
                    SubsonicGetGenresQuery query = new SubsonicGetGenresQuery(input);
                    SubsonicBaseResponse result = await sender.Send(query, cancellationToken);

                    return SubsonicResponseHelper.CreateSubsonicResult(result);
                })
            .WithName("SubsonicGetGenres")
            .WithSummary("Returns all genres")
            .WithTags("Browsing");

        // GetArtists endpoint - returns an indexed structure of all artists organized by ID3 tags (GET /subsonic/rest/getArtists)
        subsonicGroup.MapGet("getArtists",
                async ([AsParameters] SubsonicGetArtistsInput input, IMediator sender, CancellationToken cancellationToken) =>
                {
                    SubsonicGetArtistsQuery query = new SubsonicGetArtistsQuery(input);
                    SubsonicBaseResponse result = await sender.Send(query, cancellationToken);

                    return SubsonicResponseHelper.CreateSubsonicResult(result);
                })
            .WithName("SubsonicGetArtists")
            .WithSummary("Returns an indexed structure of all artists organized by ID3 tags")
            .WithTags("Browsing");

        // GetArtist endpoint - returns details for an artist, including a list of albums (GET /subsonic/rest/getArtist)
        subsonicGroup.MapGet("getArtist",
                async ([AsParameters] SubsonicGetArtistInput input, IMediator sender, CancellationToken cancellationToken) =>
                {
                    SubsonicGetArtistQuery query = new SubsonicGetArtistQuery(input);
                    SubsonicBaseResponse result = await sender.Send(query, cancellationToken);

                    return SubsonicResponseHelper.CreateSubsonicResult(result);
                })
            .WithName("SubsonicGetArtist")
            .WithSummary("Returns details for an artist, including a list of albums")
            .WithTags("Browsing");

        // GetAlbum endpoint - returns details for an album, including a list of songs (GET /subsonic/rest/getAlbum)
        subsonicGroup.MapGet("getAlbum",
                async ([AsParameters] SubsonicGetAlbumInput input, IMediator sender, CancellationToken cancellationToken) =>
                {
                    SubsonicGetAlbumQuery query = new SubsonicGetAlbumQuery(input);
                    SubsonicBaseResponse result = await sender.Send(query, cancellationToken);

                    return SubsonicResponseHelper.CreateSubsonicResult(result);
                })
            .WithName("SubsonicGetAlbum")
            .WithSummary("Returns details for an album, including a list of songs")
            .WithTags("Browsing");

        // GetArtistInfo endpoint - returns artist info with biography, image URLs and similar artists (GET /subsonic/rest/getArtistInfo)
        subsonicGroup.MapGet("getArtistInfo",
                async ([AsParameters] SubsonicGetArtistInfoInput input, IMediator sender, CancellationToken cancellationToken) =>
                {
                    SubsonicGetArtistInfoQuery query = new SubsonicGetArtistInfoQuery(input);
                    SubsonicBaseResponse result = await sender.Send(query, cancellationToken);

                    return SubsonicResponseHelper.CreateSubsonicResult(result);
                })
            .WithName("SubsonicGetArtistInfo")
            .WithSummary("Returns artist info with biography, image URLs and similar artists")
            .WithTags("Browsing");

        // GetArtistInfo2 endpoint - similar to getArtistInfo but organizes music according to ID3 tags (GET /subsonic/rest/getArtistInfo2)
        subsonicGroup.MapGet("getArtistInfo2",
                async ([AsParameters] SubsonicGetArtistInfo2Input input, IMediator sender, CancellationToken cancellationToken) =>
                {
                    SubsonicGetArtistInfo2Query query = new SubsonicGetArtistInfo2Query(input);
                    SubsonicBaseResponse result = await sender.Send(query, cancellationToken);

                    return SubsonicResponseHelper.CreateSubsonicResult(result);
                })
            .WithName("SubsonicGetArtistInfo2")
            .WithSummary("Returns artist info with biography, image URLs and similar artists organized by ID3 tags")
            .WithTags("Browsing");

        #endregion

        #region Playlists

        // GetPlaylists endpoint - returns all playlists (GET /subsonic/rest/getPlaylists)
        subsonicGroup.MapGet("getPlaylists",
                async ([AsParameters] SubsonicGetPlaylistsInput input, IMediator sender, CancellationToken cancellationToken) =>
                {
                    SubsonicGetPlaylistsQuery query = new SubsonicGetPlaylistsQuery(input);
                    SubsonicBaseResponse result = await sender.Send(query, cancellationToken);

                    return SubsonicResponseHelper.CreateSubsonicResult(result);
                })
            .WithName("SubsonicGetPlaylists")
            .WithSummary("Returns all playlists")
            .WithTags("Playlists");

        // GetPlaylist endpoint - returns a single playlist with songs (GET /subsonic/rest/getPlaylist)
        subsonicGroup.MapGet("getPlaylist",
                async ([AsParameters] SubsonicGetPlaylistInput input, IMediator sender, CancellationToken cancellationToken) =>
                {
                    SubsonicGetPlaylistQuery query = new SubsonicGetPlaylistQuery(input);
                    SubsonicBaseResponse result = await sender.Send(query, cancellationToken);

                    return SubsonicResponseHelper.CreateSubsonicResult(result);
                })
            .WithName("SubsonicGetPlaylist")
            .WithSummary("Returns a single playlist with its songs")
            .WithTags("Playlists");

        // createPlaylist endpoint - creates a new playlist (GET /subsonic/rest/createPlaylist)
        subsonicGroup.MapGet("createPlaylist",
                async ([AsParameters] SubsonicCreatePlaylistInput input, IMediator sender, CancellationToken cancellationToken) =>
                {
                    SubsonicCreatePlaylistCommand command = new SubsonicCreatePlaylistCommand(input);
                    SubsonicBaseResponse result = await sender.Send(command, cancellationToken);

                    return SubsonicResponseHelper.CreateSubsonicResult(result);
                })
            .WithName("SubsonicCreatePlaylist")
            .WithSummary("Creates a new playlist")
            .WithTags("Playlists");

        // updatePlaylist endpoint - updates an existing playlist (GET /subsonic/rest/updatePlaylist)
        subsonicGroup.MapGet("updatePlaylist",
                async ([AsParameters] SubsonicUpdatePlaylistInput input, IMediator sender, CancellationToken cancellationToken) =>
                {
                    SubsonicUpdatePlaylistCommand command = new SubsonicUpdatePlaylistCommand(input);
                    SubsonicBaseResponse result = await sender.Send(command, cancellationToken);

                    return SubsonicResponseHelper.CreateSubsonicResult(result);
                })
            .WithName("SubsonicUpdatePlaylist")
            .WithSummary("Updates an existing playlist")
            .WithTags("Playlists");

        // deletePlaylist endpoint - deletes an existing playlist (GET /subsonic/rest/deletePlaylist)
        subsonicGroup.MapGet("deletePlaylist",
                async ([AsParameters] SubsonicDeletePlaylistInput input, IMediator sender, CancellationToken cancellationToken) =>
                {
                    SubsonicDeletePlaylistCommand command = new SubsonicDeletePlaylistCommand(input);
                    SubsonicBaseResponse result = await sender.Send(command, cancellationToken);

                    return SubsonicResponseHelper.CreateSubsonicResult(result);
                })
            .WithName("SubsonicDeletePlaylist")
            .WithSummary("Deletes an existing playlist")
            .WithTags("Playlists");

        #endregion

        #region Album/song lists

        // GetAlbumList endpoint - returns a list of albums based on various criteria (GET /subsonic/rest/getAlbumList)
        subsonicGroup.MapGet("getAlbumList",
                async ([AsParameters] SubsonicGetAlbumListInput input, IMediator sender, CancellationToken cancellationToken) =>
                {
                    SubsonicGetAlbumListQuery query = new SubsonicGetAlbumListQuery(input);
                    SubsonicBaseResponse result = await sender.Send(query, cancellationToken);

                    return SubsonicResponseHelper.CreateSubsonicResult(result);
                })
            .WithName("SubsonicGetAlbumList")
            .WithSummary("Returns a list of albums based on various criteria")
            .WithTags("Album/song lists");

        // GetAlbumList2 endpoint - similar to getAlbumList, but organizes music according to ID3 tags (GET /subsonic/rest/getAlbumList2)
        subsonicGroup.MapGet("getAlbumList2",
                async ([AsParameters] SubsonicGetAlbumList2Input input, IMediator sender, CancellationToken cancellationToken) =>
                {
                    SubsonicGetAlbumList2Query query = new SubsonicGetAlbumList2Query(input);
                    SubsonicBaseResponse result = await sender.Send(query, cancellationToken);

                    return SubsonicResponseHelper.CreateSubsonicResult(result);
                })
            .WithName("SubsonicGetAlbumList2")
            .WithSummary("Returns a list of albums based on various criteria, organized according to ID3 tags")
            .WithTags("Album/song lists");

        // GetStarred endpoint - returns starred songs, albums and artists (GET /subsonic/rest/getStarred)
        subsonicGroup.MapGet("getStarred",
                async ([AsParameters] SubsonicGetStarredInput input, IMediator sender, CancellationToken cancellationToken) =>
                {
                    SubsonicGetStarredQuery query = new SubsonicGetStarredQuery(input);
                    SubsonicBaseResponse result = await sender.Send(query, cancellationToken);

                    return SubsonicResponseHelper.CreateSubsonicResult(result);
                })
            .WithName("SubsonicGetStarred")
            .WithSummary("Returns starred songs, albums and artists")
            .WithTags("Album/song lists");

        // GetStarred2 endpoint - returns starred songs, albums and artists organized by ID3 tags (GET /subsonic/rest/getStarred2)
        subsonicGroup.MapGet("getStarred2",
                async ([AsParameters] SubsonicGetStarred2Input input, IMediator sender, CancellationToken cancellationToken) =>
                {
                    SubsonicGetStarred2Query query = new SubsonicGetStarred2Query(input);
                    SubsonicBaseResponse result = await sender.Send(query, cancellationToken);

                    return SubsonicResponseHelper.CreateSubsonicResult(result);
                })
            .WithName("SubsonicGetStarred2")
            .WithSummary("Returns starred songs, albums and artists organized by ID3 tags")
            .WithTags("Album/song lists");

        // GetSong endpoint - returns details for a specific song (GET /subsonic/rest/getSong)
        subsonicGroup.MapGet("getSong",
                async ([AsParameters] SubsonicGetSongInput input, IMediator sender, CancellationToken cancellationToken) =>
                {
                    SubsonicGetSongQuery query = new SubsonicGetSongQuery(input);
                    SubsonicBaseResponse result = await sender.Send(query, cancellationToken);

                    return SubsonicResponseHelper.CreateSubsonicResult(result);
                })
            .WithName("SubsonicGetSong")
            .WithSummary("Returns details for a specific song")
            .WithTags("Album/song lists");

        #endregion

        #region Media annotation

        // Scrobble endpoint - registers the local playback of a track (GET /subsonic/rest/scrobble)
        subsonicGroup.MapGet("scrobble",
                async ([AsParameters] SubsonicScrobbleInput input, IMediator sender, CancellationToken cancellationToken) =>
                {
                    SubsonicScrobbleCommand command = new SubsonicScrobbleCommand(input);
                    SubsonicBaseResponse result = await sender.Send(command, cancellationToken);

                    return SubsonicResponseHelper.CreateSubsonicResult(result);
                })
            .WithName("SubsonicScrobble")
            .WithSummary("Registers the local playback of a track")
            .WithTags("Media annotation");

        // Star endpoint - stars songs, albums and artists (GET /subsonic/rest/star)
        subsonicGroup.MapGet("star",
                async ([AsParameters] SubsonicStarInput input, IMediator sender, CancellationToken cancellationToken) =>
                {
                    SubsonicStarCommand command = new SubsonicStarCommand(input);
                    SubsonicBaseResponse result = await sender.Send(command, cancellationToken);

                    return SubsonicResponseHelper.CreateSubsonicResult(result);
                })
            .WithName("SubsonicStar")
            .WithSummary("Stars songs, albums and artists")
            .WithTags("Media annotation");

        // Unstar endpoint - unstars songs, albums and artists (GET /subsonic/rest/unstar)
        subsonicGroup.MapGet("unstar",
                async ([AsParameters] SubsonicUnstarInput input, IMediator sender, CancellationToken cancellationToken) =>
                {
                    SubsonicUnstarCommand command = new SubsonicUnstarCommand(input);
                    SubsonicBaseResponse result = await sender.Send(command, cancellationToken);

                    return SubsonicResponseHelper.CreateSubsonicResult(result);
                })
            .WithName("SubsonicUnstar")
            .WithSummary("Unstars songs, albums and artists")
            .WithTags("Media annotation");

        #endregion

        #region Bookmarks

        // GetBookmarks endpoint - returns all bookmarks (GET /subsonic/rest/getBookmarks)
        subsonicGroup.MapGet("getBookmarks",
                async (IMediator sender, CancellationToken cancellationToken) =>
                {
                    SubsonicGetBookmarksInput input = new SubsonicGetBookmarksInput();
                    SubsonicGetBookmarksQuery query = new SubsonicGetBookmarksQuery(input);
                    SubsonicBaseResponse result = await sender.Send(query, cancellationToken);

                    return SubsonicResponseHelper.CreateSubsonicResult(result);
                })
            .WithName("SubsonicGetBookmarks")
            .WithSummary("Returns all bookmarks")
            .WithTags("Bookmarks");

        #endregion

        #region Media retrieval

        // Stream endpoint - streams audio content for a song (GET /subsonic/rest/stream)
        subsonicGroup.MapGet("stream",
                async ([AsParameters] SubsonicStreamInput input, IMediator sender, CancellationToken cancellationToken) =>
                {
                    SubsonicStreamQuery query = new SubsonicStreamQuery(input);
                    SubsonicStreamResponse result = await sender.Send(query, cancellationToken);

                    if (!File.Exists(result.FilePath))
                        return Results.NotFound("Audio file not found on disk");

                    // Use FileStream for streaming large files efficiently
                    FileStream fileStream = new FileStream(result.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);

                    return Results.Stream(fileStream, result.ContentType, result.FileName, enableRangeProcessing: true);
                })
            .WithName("SubsonicStream")
            .WithSummary("Streams audio content for a song")
            .WithTags("Media retrieval")
            .Produces(200)
            .Produces(404)
            .ProducesValidationProblem();

        // Download endpoint - downloads audio files for a song (GET /subsonic/rest/download)
        subsonicGroup.MapGet("download",
                async ([AsParameters] SubsonicDownloadInput input, IMediator sender, CancellationToken cancellationToken) =>
                {
                    SubsonicDownloadQuery query = new SubsonicDownloadQuery(input);
                    SubsonicDownloadResponse result = await sender.Send(query, cancellationToken);

                    if (!File.Exists(result.FilePath))
                        return Results.NotFound("Audio file not found on disk");

                    byte[] fileBytes = await File.ReadAllBytesAsync(result.FilePath, cancellationToken);

                    return Results.File(fileBytes, result.ContentType, result.FileName);
                })
            .WithName("SubsonicDownload")
            .WithSummary("Downloads audio files for a song")
            .WithTags("Media retrieval")
            .Produces(200)
            .Produces(404)
            .ProducesValidationProblem();

        // GetCoverArt endpoint - returns cover art images (GET /subsonic/rest/getCoverArt)
        subsonicGroup.MapGet("getCoverArt",
                async ([AsParameters] SubsonicGetCoverArtInput input, IMediator sender, CancellationToken cancellationToken) =>
                {
                    SubsonicGetCoverArtQuery query = new SubsonicGetCoverArtQuery(input);
                    SubsonicGetCoverArtResponse result = await sender.Send(query, cancellationToken);

                    if (!File.Exists(result.FilePath))
                        return Results.NotFound("Cover art image not found on disk");

                    byte[] fileBytes = await File.ReadAllBytesAsync(result.FilePath, cancellationToken);

                    return Results.File(fileBytes, result.ContentType, result.FileName);
                })
            .WithName("SubsonicGetCoverArt")
            .WithSummary("Returns cover art images for songs, albums, or artists")
            .WithTags("Media retrieval")
            .Produces(200, contentType: "image/jpeg")
            .Produces(200, contentType: "image/png")
            .Produces(404)
            .ProducesValidationProblem();

        #endregion

        return endpoints;
    }
}