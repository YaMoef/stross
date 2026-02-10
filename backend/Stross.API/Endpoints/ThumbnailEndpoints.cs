using MediatR;
using Stross.Application.Slices.Thumbnail.InputModels;
using Stross.Application.Slices.Thumbnail.Queries;
using Stross.Application.Slices.Thumbnail.ResponseModels;

namespace Stross.API.Endpoints;

internal static class ThumbnailEndpoints
{
    internal static IEndpointRouteBuilder MapThumbnailEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder thumbnailGroup = endpoints.MapGroup("/thumbnails");

        #region Thumbnail Retrieval

        // Get creator thumbnail endpoint - retrieves thumbnail for a creator (GET /thumbnails/creator/{id})
        thumbnailGroup.MapGet("/creator/{id:long}",
                async (long id, IMediator sender, CancellationToken cancellationToken) =>
                {
                    GetThumbnailInput input = new GetThumbnailInput(id, ThumbnailType.Creator);
                    GetThumbnailQuery query = new GetThumbnailQuery(input);

                    GetThumbnailResponse result = await sender.Send(query, cancellationToken);

                    if (!File.Exists(result.ThumbnailLocation))
                        return Results.NotFound("Thumbnail file not found on disk");

                    byte[] fileBytes = await File.ReadAllBytesAsync(result.ThumbnailLocation, cancellationToken);

                    return Results.File(fileBytes, result.ContentType);
                })
            .WithName("GetCreatorThumbnail")
            .WithSummary("Get thumbnail for a creator")
            .WithDescription("Retrieves the thumbnail image for a creator by their ID")
            .WithTags("Thumbnail Retrieval")
            .Produces(200, contentType: "image/jpeg")
            .Produces(200, contentType: "image/png")
            .Produces(404);

        // Get music track thumbnail endpoint - retrieves thumbnail for a music track (GET /thumbnails/music-track/{id})
        thumbnailGroup.MapGet("/music-track/{id:long}",
                async (long id, IMediator sender, CancellationToken cancellationToken) =>
                {
                    GetThumbnailInput input = new GetThumbnailInput(id, ThumbnailType.MusicTrack);
                    GetThumbnailQuery query = new GetThumbnailQuery(input);

                    GetThumbnailResponse result = await sender.Send(query, cancellationToken);

                    if (!File.Exists(result.ThumbnailLocation))
                        return Results.NotFound("Thumbnail file not found on disk");

                    byte[] fileBytes = await File.ReadAllBytesAsync(result.ThumbnailLocation, cancellationToken);

                    return Results.File(fileBytes, result.ContentType);
                })
            .WithName("GetMusicTrackThumbnail")
            .WithSummary("Get thumbnail for a music track")
            .WithDescription("Retrieves the thumbnail image for a music track by its ID")
            .WithTags("Thumbnail Retrieval")
            .Produces(200, contentType: "image/jpeg")
            .Produces(200, contentType: "image/png")
            .Produces(404);

        #endregion

        return endpoints;
    }
}
