namespace Stross.API.Middleware;

/// <summary>
/// Middleware that automatically strips file extensions from Subsonic API request paths.
/// This allows Subsonic endpoints to respond to paths with any extension (e.g., .view, .json, .xml) without duplicating registrations.
/// </summary>
public class SubsonicSuffixMiddleware
{
    private readonly RequestDelegate _next;

    public SubsonicSuffixMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public Task InvokeAsync(HttpContext context)
    {
        string originalPath = context.Request.Path.Value ?? string.Empty;
        
        string cleanedPath = StripFileExtension(originalPath);

        if (cleanedPath != originalPath)
            context.Request.Path = new PathString(cleanedPath);

        return _next(context);
    }

    /// <summary>
    /// Strips any file extension from the last segment of the path.
    /// </summary>
    /// <param name="path">The original path</param>
    /// <returns>The path with the file extension removed from the last segment</returns>
    private static string StripFileExtension(string path)
    {
        int lastSlashIndex = path.LastIndexOf('/');
        int lastDotIndex = path.LastIndexOf('.');

        // Only strip extension if:
        // 1. There's a dot in the path
        // 2. The dot comes after the last slash (so it's in the last segment)
        // 3. There's at least one character after the dot (valid extension)
        if (lastDotIndex > lastSlashIndex && lastDotIndex > 0 && lastDotIndex < path.Length - 1)
            return path[..lastDotIndex];

        return path;
    }
}

internal static class SubonicSuffixMiddlewareExtensions
{
    /// <summary>
    /// Adds middleware that automatically strips file extensions from Subsonic API request paths.
    /// This allows Subsonic endpoints to respond to paths with any extension (e.g., .view, .json, .xml) without duplicating registrations.
    /// </summary>
    /// <param name="app">The web application builder</param>
    /// <returns>The web application for method chaining</returns>
    internal static WebApplication UseSubsonicSuffixMiddleware(this WebApplication app)
    {
        app.UseWhen(p => p.Request.Path.StartsWithSegments("/rest"), config => config.UseMiddleware<SubsonicSuffixMiddleware>());

        return app;
    }
}