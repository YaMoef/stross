using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Stross.API.Endpoints;
using Stross.Application;
using Stross.Application.Slices.MusicTrack;
using Stross.Config;
using Stross.Infrastructure;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.AddInfrastructure();
builder.AddApplication();

builder.Services.AddOptions<DatabaseConfig>().BindConfiguration(DatabaseConfig.SectionName);
builder.Services.AddOptions<StrossStorageConfig>().BindConfiguration(StrossStorageConfig.SectionName);

// Add OpenAPI services
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000");
        policy.AllowCredentials();
        policy.AllowAnyMethod();
        policy.WithHeaders("Authorization");
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

builder.AddMusicTrackSlice();

WebApplication app = builder.Build();

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
}

app.UseRouting();

app.UseCors();
app.UseAuthentication();

app.UseAuthorization();

app
    .MapGroup("v1")
    .MapMusicTrackEndpoints();

app.Run();