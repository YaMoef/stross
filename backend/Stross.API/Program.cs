using System.Net;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using Stross.Abstractions.Accessors;
using Stross.API.Accessors;
using Stross.API.AuthenticationHandlers;
using Stross.API.Endpoints;
using Stross.API.Middleware;
using Stross.Application;
using Stross.Application.Slices.MusicTrack;
using Stross.Application.Slices.Provider;
using Stross.Application.Slices.Subsonic;
using Stross.Application.Slices.Thumbnail;
using Stross.Config;
using Stross.Domain.Entities;
using Stross.Infrastructure;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.AddInfrastructure();
builder.AddApplication();

builder.Services.AddOptions<DatabaseConfig>().BindConfiguration(DatabaseConfig.SectionName);
builder.Services.AddOptions<StrossStorageConfig>().BindConfiguration(StrossStorageConfig.SectionName);
builder.Services.AddOptions<StrossAdminAccount>().BindConfiguration(StrossAdminAccount.SectionName);

// Add OpenAPI services
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin();
        // policy.AllowCredentials();
        policy.AllowAnyMethod();
        policy.AllowAnyHeader();
    });
});

builder.Services.AddAuthorization();

builder.Services.AddProblemDetails(opts =>
{
    opts.CustomizeProblemDetails = ctx =>
    {
        // Helps to correlate logs and errors with the specific request.
        ctx.ProblemDetails.Extensions.Add("request-id", ctx.HttpContext.TraceIdentifier);

        ctx.ProblemDetails.Extensions.Add("correlation-id", ctx.HttpContext.Request.Headers["X-Correlation-ID"]);
        ctx.ProblemDetails.Extensions.Add("timestamp", DateTime.UtcNow);
    };
});

string? reverseProxyHost = builder.Configuration.GetValue<string?>("ReverseProxyHost");

if (!string.IsNullOrEmpty(reverseProxyHost))
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders =
            ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;

        bool isIpParseable = IPAddress.TryParse(reverseProxyHost, out IPAddress? ip);

        if (isIpParseable)
        {
            if (ip is not null)
                options.KnownProxies.Add(ip);
        }
        else
        {
            IPAddress[] addresses = Dns.GetHostAddresses(reverseProxyHost);
            options.KnownProxies.Add(addresses[0]);
        }
    });

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = SubsonicAuthenticationOptions.DefaultScheme;
        options.DefaultChallengeScheme = SubsonicAuthenticationOptions.DefaultScheme;
    })
    .AddScheme<SubsonicAuthenticationOptions, SubsonicAuthenticationHandler>(
        SubsonicAuthenticationOptions.DefaultScheme,
        _ => { });

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserAccessor, UserAccessor>();

builder.AddMusicTrackSlice();
builder.AddProviderSlice();
builder.AddSubsonicSlice();
builder.AddThumbnailSlice();

WebApplication app = builder.Build();

if (!string.IsNullOrEmpty(reverseProxyHost))
    app.UseForwardedHeaders();

if (app.Environment.IsDevelopment()) // development
{
    app.UseDeveloperExceptionPage();

    // Map OpenAPI endpoint
    app.MapOpenApi();

    // Add Scalar API documentation
    app.MapScalarApiReference(options =>
    {
        options.Title = "Stross API";
        options.Theme = ScalarTheme.Purple;
        options.ShowSidebar = true;
        options.DarkMode = true;
    });
}
else // production
{
}

using (IServiceScope scope = app.Services.CreateScope())
{
    StrossContext dbContext = scope.ServiceProvider.GetRequiredService<StrossContext>();
    ILogger logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    StrossAdminAccount adminAccount = scope.ServiceProvider.GetRequiredService<IOptions<StrossAdminAccount>>().Value;

    // Check and apply pending migrations
    IEnumerable<string> pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();

    if (pendingMigrations.Any())
    {
        logger.LogInformation("Applying pending migrations...");
        await dbContext.Database.MigrateAsync();
        logger.LogInformation("Migrations applied successfully.");
    }
    else
    {
        logger.LogInformation("No pending migrations found.");
    }

    if (!await dbContext.Users.AnyAsync(u => u.IsDefaultUser))
    {
        logger.LogInformation("Creating default admin user...");

        dbContext.Users.Add(new User(adminAccount.UserName, adminAccount.DisplayName)
        {
            IsDefaultUser = true
        });

        await dbContext.SaveChangesAsync();

        logger.LogInformation("Default admin user with userName {UserName} created successfully.", adminAccount.UserName);
    }
}

app.UseSubsonicSuffixMiddleware();
app.UseSubsonicFormatCaptureMiddleware();
app.UseSubsonicErrorHandlerMiddleware();

app.UseRouting();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app
    .MapGroup("api/v1")
    .MapMusicTrackEndpoints()
    .MapProviderEndpoints()
    .MapThumbnailEndpoints();

app.MapSubsonicEndpoints();

app.Run();