using MediatR;
using Stross.Application.Slices.Provider.Commands;
using Stross.Application.Slices.Provider.InputModels;

namespace Stross.API.Endpoints;

internal static class ProviderEndpoints
{
    internal static IEndpointRouteBuilder MapProviderEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder providerGroup = endpoints.MapGroup("/providers");

        #region Provider Management

        // Add provider endpoint - adds a new provider (POST /providers)
        providerGroup.MapPost("",
                async (AddProviderInput input, IMediator sender, CancellationToken cancellationToken) =>
                {
                    AddProviderCommand command = new AddProviderCommand(input);

                    long result = await sender.Send(command, cancellationToken);

                    return Results.Ok(result);
                })
            .WithName("AddProvider")
            .WithSummary("Add a new provider")
            .WithTags("Provider Management");

        #endregion

        return endpoints;
    }
}