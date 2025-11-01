using MediatR;
using Stross.Application.Slices.MusicTrack.Commands;
using Stross.Application.Slices.MusicTrack.InputModels;

namespace Stross.API.Endpoints;

public static class MusicTrackEndpoints
{
    public static IEndpointRouteBuilder MapMusicTrackEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGroup("/music-track")
            .MapPost("download",
                async (DownloadMusicTrackInput input, IMediator sender, CancellationToken cancellationToken) =>
                {
                    DownloadMusicTrackCommand command = new DownloadMusicTrackCommand(input);

                    long result = await sender.Send(command, cancellationToken);

                    return Results.Ok(result);
                });

        return endpoints;
    }
}