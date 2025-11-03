using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using MediatR;
using Stross.Application.Shared.Helpers;
using Stross.Application.Slices.Subsonic.Commands;
using Stross.Application.Slices.Subsonic.InputModels;
using Stross.Application.Slices.Subsonic.Queries;
using Stross.Application.Slices.Subsonic.ResponseModels;

namespace Stross.API.Endpoints;

internal static class SubsonicEndpoints
{
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters =
        {
            new JsonStringEnumConverter()
        },
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static IResult CreateSubsonicResult(SubsonicBaseResponse response)
    {
        if (response.Format == SubsonicResponseFormat.Xml)
        {
            string xmlContent = XmlSerializationHelper.SerializeSubsonicResponse(response);

            return Results.Content(xmlContent, "application/xml", Encoding.UTF8);
        }

        return Results.Json(response, JsonOptions);
    }

    internal static IEndpointRouteBuilder MapSubsonicEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder subsonicGroup = endpoints.MapGroup("/rest/");

        // Ping endpoint - test connectivity (GET /subsonic/rest/ping)
        subsonicGroup.MapGet("ping",
                async (IMediator sender, CancellationToken cancellationToken) =>
                {
                    SubsonicPingCommand command = new SubsonicPingCommand();
                    SubsonicBaseResponse result = await sender.Send(command, cancellationToken);

                    return CreateSubsonicResult(result);
                })
            .WithName("SubsonicPing")
            .WithSummary("Test connectivity with the Subsonic server")
            .WithOpenApi();

        // Search endpoint (deprecated since 1.4.0, but still supported) (GET /subsonic/rest/search)
        subsonicGroup.MapGet("search",
                async ([AsParameters]SubsonicSearchInput input, IMediator sender, CancellationToken cancellationToken) =>
                {
                    SubsonicSearchQuery query = new SubsonicSearchQuery(input);
                    SubsonicBaseResponse result = await sender.Send(query, cancellationToken);

                    return CreateSubsonicResult(result);
                })
            .WithName("SubsonicSearch")
            .WithSummary("Search for files (deprecated, use search2 instead)")
            .WithOpenApi();

        // Search2 endpoint - returns albums, artists and songs (GET /subsonic/rest/search2)
        subsonicGroup.MapGet("search2",
                async ([AsParameters]SubsonicSearch2Input input, IMediator sender, CancellationToken cancellationToken) =>
                {
                    SubsonicSearch2Query query = new SubsonicSearch2Query(input);
                    SubsonicBaseResponse result = await sender.Send(query, cancellationToken);

                    return CreateSubsonicResult(result);
                })
            .WithName("SubsonicSearch2")
            .WithSummary("Search for albums, artists and songs")
            .WithOpenApi();

        // Search3 endpoint - similar to search2 but organizes music according to ID3 tags (GET /subsonic/rest/search3)
        subsonicGroup.MapGet("search3",
                async ([AsParameters]SubsonicSearch3Input input, IMediator sender, CancellationToken cancellationToken) =>
                {
                    SubsonicSearch3Query query = new SubsonicSearch3Query(input);
                    SubsonicBaseResponse result = await sender.Send(query, cancellationToken);

                    return CreateSubsonicResult(result);
                })
            .WithName("SubsonicSearch3")
            .WithSummary("Search for albums, artists and songs organized by ID3 tags")
            .WithOpenApi();

        // GetMusicFolders endpoint - returns available music folders (GET /subsonic/rest/getMusicFolders)
        subsonicGroup.MapGet("getMusicFolders",
                async (IMediator sender, CancellationToken cancellationToken) =>
                {
                    SubsonicGetMusicFoldersInput input = new SubsonicGetMusicFoldersInput();
                    SubsonicGetMusicFoldersQuery query = new SubsonicGetMusicFoldersQuery(input);
                    SubsonicBaseResponse result = await sender.Send(query, cancellationToken);

                    return CreateSubsonicResult(result);
                })
            .WithName("SubsonicGetMusicFolders")
            .WithSummary("Returns available music folders")
            .WithOpenApi();

        // GetIndexes endpoint - returns an indexed structure of all artists (GET /subsonic/rest/getIndexes)
        subsonicGroup.MapGet("getIndexes",
                async ([AsParameters]SubsonicGetIndexesInput input, IMediator sender, CancellationToken cancellationToken) =>
                {
                    SubsonicGetIndexesQuery query = new SubsonicGetIndexesQuery(input);
                    SubsonicBaseResponse result = await sender.Send(query, cancellationToken);

                    return CreateSubsonicResult(result);
                })
            .WithName("SubsonicGetIndexes")
            .WithSummary("Returns an indexed structure of all artists")
            .WithOpenApi();

        // GetMusicDirectory endpoint - returns a listing of all files in a music directory (GET /subsonic/rest/getMusicDirectory)
        subsonicGroup.MapGet("getMusicDirectory",
                async ([AsParameters]SubsonicGetMusicDirectoryInput input, IMediator sender, CancellationToken cancellationToken) =>
                {
                    SubsonicGetMusicDirectoryQuery query = new SubsonicGetMusicDirectoryQuery(input);
                    SubsonicBaseResponse result = await sender.Send(query, cancellationToken);

                    return CreateSubsonicResult(result);
                })
            .WithName("SubsonicGetMusicDirectory")
            .WithSummary("Returns a listing of all files in a music directory")
            .WithOpenApi();

        // GetGenres endpoint - returns all genres (GET /subsonic/rest/getGenres)
        subsonicGroup.MapGet("getGenres",
                async (IMediator sender, CancellationToken cancellationToken) =>
                {
                    SubsonicGetGenresInput input = new SubsonicGetGenresInput();
                    SubsonicGetGenresQuery query = new SubsonicGetGenresQuery(input);
                    SubsonicBaseResponse result = await sender.Send(query, cancellationToken);

                    return CreateSubsonicResult(result);
                })
            .WithName("SubsonicGetGenres")
            .WithSummary("Returns all genres")
            .WithOpenApi();

        // GetArtists endpoint - returns an indexed structure of all artists organized by ID3 tags (GET /subsonic/rest/getArtists)
        subsonicGroup.MapGet("getArtists",
                async ([AsParameters]SubsonicGetArtistsInput input, IMediator sender, CancellationToken cancellationToken) =>
                {
                    SubsonicGetArtistsQuery query = new SubsonicGetArtistsQuery(input);
                    SubsonicBaseResponse result = await sender.Send(query, cancellationToken);

                    return CreateSubsonicResult(result);
                })
            .WithName("SubsonicGetArtists")
            .WithSummary("Returns an indexed structure of all artists organized by ID3 tags")
            .WithOpenApi();

        // GetPlaylists endpoint - returns all playlists (GET /subsonic/rest/getPlaylists)
        subsonicGroup.MapGet("getPlaylists",
                async ([AsParameters]SubsonicGetPlaylistsInput input, IMediator sender, CancellationToken cancellationToken) =>
                {
                    SubsonicGetPlaylistsQuery query = new SubsonicGetPlaylistsQuery(input);
                    SubsonicBaseResponse result = await sender.Send(query, cancellationToken);

                    return CreateSubsonicResult(result);
                })
            .WithName("SubsonicGetPlaylists")
            .WithSummary("Returns all playlists")
            .WithOpenApi();

        // GetBookmarks endpoint - returns all bookmarks (GET /subsonic/rest/getBookmarks)
        subsonicGroup.MapGet("getBookmarks",
                async (IMediator sender, CancellationToken cancellationToken) =>
                {
                    SubsonicGetBookmarksInput input = new SubsonicGetBookmarksInput();
                    SubsonicGetBookmarksQuery query = new SubsonicGetBookmarksQuery(input);
                    SubsonicBaseResponse result = await sender.Send(query, cancellationToken);

                    return CreateSubsonicResult(result);
                })
            .WithName("SubsonicGetBookmarks")
            .WithSummary("Returns all bookmarks")
            .WithOpenApi();

        // GetStarred endpoint - returns starred songs, albums and artists (GET /subsonic/rest/getStarred)
        subsonicGroup.MapGet("getStarred",
                async ([AsParameters]SubsonicGetStarredInput input, IMediator sender, CancellationToken cancellationToken) =>
                {
                    SubsonicGetStarredQuery query = new SubsonicGetStarredQuery(input);
                    SubsonicBaseResponse result = await sender.Send(query, cancellationToken);

                    return CreateSubsonicResult(result);
                })
            .WithName("SubsonicGetStarred")
            .WithSummary("Returns starred songs, albums and artists")
            .WithOpenApi();

        // GetStarred2 endpoint - returns starred songs, albums and artists organized by ID3 tags (GET /subsonic/rest/getStarred2)
        subsonicGroup.MapGet("getStarred2",
                async ([AsParameters]SubsonicGetStarred2Input input, IMediator sender, CancellationToken cancellationToken) =>
                {
                    SubsonicGetStarred2Query query = new SubsonicGetStarred2Query(input);
                    SubsonicBaseResponse result = await sender.Send(query, cancellationToken);

                    return CreateSubsonicResult(result);
                })
            .WithName("SubsonicGetStarred2")
            .WithSummary("Returns starred songs, albums and artists organized by ID3 tags")
            .WithOpenApi();

        // GetSong endpoint - returns details for a specific song (GET /subsonic/rest/getSong)
        subsonicGroup.MapGet("getSong",
                async ([AsParameters]SubsonicGetSongInput input, IMediator sender, CancellationToken cancellationToken) =>
                {
                    SubsonicGetSongQuery query = new SubsonicGetSongQuery(input);
                    SubsonicBaseResponse result = await sender.Send(query, cancellationToken);

                    return CreateSubsonicResult(result);
                })
            .WithName("SubsonicGetSong")
            .WithSummary("Returns details for a specific song")
            .WithOpenApi();

        // Scrobble endpoint - registers the local playback of a track (GET /subsonic/rest/scrobble)
        subsonicGroup.MapGet("scrobble",
                async ([AsParameters]SubsonicScrobbleInput input, IMediator sender, CancellationToken cancellationToken) =>
                {
                    SubsonicScrobbleCommand command = new SubsonicScrobbleCommand(input);
                    SubsonicBaseResponse result = await sender.Send(command, cancellationToken);

                    return CreateSubsonicResult(result);
                })
            .WithName("SubsonicScrobble")
            .WithSummary("Registers the local playback of a track")
            .WithOpenApi();

        // Stream endpoint - streams audio content for a song (GET /subsonic/rest/stream)
        subsonicGroup.MapGet("stream",
                async ([AsParameters]SubsonicStreamInput input, IMediator sender, CancellationToken cancellationToken) =>
                {
                    SubsonicStreamQuery query = new SubsonicStreamQuery(input);
                    SubsonicStreamResponse result = await sender.Send(query, cancellationToken);

                    if (!File.Exists(result.FilePath))
                        return Results.NotFound("Audio file not found on disk");

                    // Use FileStream for streaming large files efficiently
                    FileStream fileStream = new FileStream(result.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);

                    return Results.Stream(fileStream, result.ContentType, result.FileName, enableRangeProcessing:true);
                })
            .WithName("SubsonicStream")
            .WithSummary("Streams audio content for a song")
            .WithOpenApi()
            .Produces(200)
            .Produces(404)
            .ProducesValidationProblem();

        // Download endpoint - downloads audio files for a song (GET /subsonic/rest/download)
        subsonicGroup.MapGet("download",
                async ([AsParameters]SubsonicDownloadInput input, IMediator sender, CancellationToken cancellationToken) =>
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
            .WithOpenApi()
            .Produces(200)
            .Produces(404)
            .ProducesValidationProblem();

        return endpoints;
    }
}