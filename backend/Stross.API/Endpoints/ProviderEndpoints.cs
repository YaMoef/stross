using MediatR;
using Stross.Application.Slices.Provider.Commands;
using Stross.Application.Slices.Provider.InputModels;
using Stross.Application.Slices.Provider.Queries;
using Stross.Application.Slices.Provider.ResponseModels;

namespace Stross.API.Endpoints;

internal static class ProviderEndpoints
{
    internal static IEndpointRouteBuilder MapProviderEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder providerGroup = endpoints.MapGroup("/providers");

        #region Provider Management

        // Get all providers endpoint - returns a list of all providers (GET /providers)
        providerGroup.MapGet("",
                async (IMediator sender, CancellationToken cancellationToken) =>
                {
                    GetAllProvidersQuery query = new GetAllProvidersQuery();

                    List<ProviderResponse> result = await sender.Send(query, cancellationToken);

                    return Results.Ok(result);
                })
            .WithName("GetAllProviders")
            .WithSummary("Get all providers")
            .WithTags("Provider Management");

        // Get provider by ID endpoint - returns a single provider with full details (GET /providers/{id})
        providerGroup.MapGet("{id:long}",
                async (long id, IMediator sender, CancellationToken cancellationToken) =>
                {
                    GetProviderByIdQuery query = new GetProviderByIdQuery(id);

                    ProviderDetailsResponse result = await sender.Send(query, cancellationToken);

                    return Results.Ok(result);
                })
            .WithName("GetProviderById")
            .WithSummary("Get a provider by ID")
            .WithTags("Provider Management");

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

        // Update provider endpoint - updates an existing provider (PUT /providers/{id})
        providerGroup.MapPut("{id:long}",
                async (long id, UpdateProviderInput input, IMediator sender, CancellationToken cancellationToken) =>
                {
                    UpdateProviderCommand command = new UpdateProviderCommand(id, input);

                    ProviderDetailsResponse result = await sender.Send(command, cancellationToken);

                    return Results.Ok(result);
                })
            .WithName("UpdateProvider")
            .WithSummary("Update an existing provider")
            .WithTags("Provider Management");

        // Delete provider endpoint - deletes a provider (DELETE /providers/{id})
        providerGroup.MapDelete("{id:long}",
                async (long id, IMediator sender, CancellationToken cancellationToken) =>
                {
                    DeleteProviderCommand command = new DeleteProviderCommand(id);

                    await sender.Send(command, cancellationToken);

                    return Results.NoContent();
                })
            .WithName("DeleteProvider")
            .WithSummary("Delete a provider")
            .WithTags("Provider Management");

        #endregion

        return endpoints;
    }
}
