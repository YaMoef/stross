using MediatR;
using Stross.Application.Slices.Subsonic.ResponseModels;
using Stross.Exception.Exceptions;
using Stross.SubsonicModels;

namespace Stross.Application.Behaviours;

public class SubsonicBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
where TRequest: IRequest<TResponse>
where TResponse: class, ISubsonicResponse
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            // Execute the handler
            TResponse response = await next(cancellationToken);

            // Set success status and version
            response.Response.Status = ResponseStatus.Ok;
            response.Response.Version = "1.16.1";

            return response;
        }
        catch (StrossException)
        {
            // Create error response
            Response errorResponse = new Response
            {
                Status = ResponseStatus.Failed,
                Version = "1.16.1"
            };

            // Since we can't modify the response directly, we need to create a new one
            // This assumes SubsonicBaseResponse is the concrete type being used
            return (TResponse)(object)new SubsonicBaseResponse(errorResponse);
        }
    }
}