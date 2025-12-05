using Stross.API.Helpers;
using Stross.Application.Slices.Subsonic.ResponseModels;
using Stross.Application.Slices.Subsonic.Services;
using Stross.Exception.Exceptions;
using Stross.SubsonicModels;

namespace Stross.API.Middleware;

public class SubsonicErrorHandlerMiddleware
{
    private readonly RequestDelegate _next;

    public SubsonicErrorHandlerMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ISubsonicResponseFormatService subsonicContext)
    {
        try
        {
            await _next(context);
        }
        catch (StrossException ex)
        {
            Response result = new()
            {
                Status = ResponseStatus.Failed,
                Error = new Error
                {
                    Code = 0,
                    Message = ex.Message
                }
            };
            IResult response = SubsonicResponseHelper.CreateSubsonicResult(new SubsonicBaseResponse(result)
            {
                Format = subsonicContext.ResponseFormat
            });

            await response.ExecuteAsync(context);
        }
    }
}

internal static class SubsonicErrorHandlerMiddlewareExtensions
{
    /// <summary>
    /// Adds middleware that will do error handling for Subsonic API requests.
    /// </summary>
    /// <param name="app">The web application</param>
    /// <returns>The web application for method chaining</returns>
    internal static WebApplication UseSubsonicErrorHandlerMiddleware(this WebApplication app)
    {
        app.UseWhen(p => p.Request.Path.StartsWithSegments("/rest"), config => config.UseMiddleware<SubsonicErrorHandlerMiddleware>());

        return app;
    }
}