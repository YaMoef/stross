using MediatR;
using Stross.Application.Slices.MusicTrack.Commands;
using Stross.Application.Slices.MusicTrack.InputModels;

namespace Stross.API.Endpoints;

internal static class MusicTrackEndpoints
{
    internal static IEndpointRouteBuilder MapMusicTrackEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder musicTrackGroup = endpoints.MapGroup("/music-tracks");

        #region Music Track Management

        // Download music track endpoint - downloads a music track from a provider (POST /music-tracks/download)
        musicTrackGroup.MapPost("download",
                async (DownloadMusicTrackInput input, IMediator sender, CancellationToken cancellationToken) =>
                {
                    DownloadMusicTrackCommand command = new DownloadMusicTrackCommand(input);

                    long result = await sender.Send(command, cancellationToken);

                    return Results.Ok(result);
                })
            .WithName("DownloadMusicTrack")
            .WithSummary("Download a music track from a provider")
            .WithTags("Music Track Management");

        #endregion

        return endpoints;
    }
}