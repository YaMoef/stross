using MediatR;
using Stross.Application.Slices.Provider.Commands;
using Stross.Application.Slices.Provider.InputModels;

namespace Stross.API.Endpoints;

internal static class ProviderEndpoints
{
    internal static IEndpointRouteBuilder MapProviderEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGroup("/providers")
            .MapPost("",
                async (AddProviderInput input, IMediator sender, CancellationToken cancellationToken) =>
                {
                    AddProviderCommand command = new AddProviderCommand(input);

                    long result = await sender.Send(command, cancellationToken);

                    return Results.Ok(result);
                });

        return endpoints;
    }
}