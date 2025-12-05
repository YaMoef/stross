using Stross.Application.Slices.Subsonic.ResponseModels;
using Stross.Application.Slices.Subsonic.Services;

namespace Stross.API.Middleware;

/// <summary>
/// Middleware that captures the 'f' parameter from Subsonic API requests and stores it in the request context.
/// This allows the SubsonicBehaviour to determine the appropriate response format (JSON/XML).
/// </summary>
public class SubsonicFormatCaptureMiddleware
{
    private readonly RequestDelegate _next;

    public SubsonicFormatCaptureMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ISubsonicResponseFormatService subsonicContext)
    {
        // Check the 'f' parameter
        string? format = context.Request.Query["f"].FirstOrDefault();

        subsonicContext.SetResponseFormat(SubsonicResponseFormat.Xml);

        if (string.Equals(format, "json", StringComparison.OrdinalIgnoreCase) || string.Equals(format, "jsonp", StringComparison.OrdinalIgnoreCase))
            subsonicContext.SetResponseFormat(SubsonicResponseFormat.Json);

        await _next(context);
    }
}

internal static class SubsonicFormatCaptureMiddlewareExtensions
{
    /// <summary>
    /// Adds middleware that captures the 'f' parameter from Subsonic API requests and stores it in the request context.
    /// </summary>
    /// <param name="app">The web application</param>
    /// <returns>The web application for method chaining</returns>
    internal static WebApplication UseSubsonicFormatCaptureMiddleware(this WebApplication app)
    {
        app.UseWhen(p => p.Request.Path.StartsWithSegments("/rest"), config => config.UseMiddleware<SubsonicFormatCaptureMiddleware>());

        return app;
    }
}