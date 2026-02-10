using MediatR;
using Stross.Application.Slices.Subsonic.ResponseModels;
using Stross.Application.Slices.Subsonic.Services;
using Stross.Exception.Exceptions;
using Stross.SubsonicModels;

namespace Stross.Application.Behaviours;

public class SubsonicBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
where TRequest : IRequest<TResponse>
where TResponse : class, ISubsonicResponse
{
    private readonly ISubsonicResponseFormatService _subsonicContext;

    public SubsonicBehaviour(ISubsonicResponseFormatService subsonicContext)
    {
        _subsonicContext = subsonicContext;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            // Execute the handler
            TResponse response = await next(cancellationToken);

            // Set success status and version
            response.Response.Status = ResponseStatus.Ok;
            response.Response.Version = "1.16.1";

            // Set the response format based on request context
            if (response is SubsonicBaseResponse baseResponse)
            {
                SubsonicBaseResponse updatedResponse = baseResponse with
                {
                    Format = _subsonicContext.ResponseFormat
                };

                return (TResponse)(object)updatedResponse;
            }

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
            SubsonicBaseResponse errorBaseResponse = new SubsonicBaseResponse(errorResponse) with
            {
                Format = _subsonicContext.ResponseFormat
            };

            return (TResponse)(object)errorBaseResponse;
        }
    }
}
