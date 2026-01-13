using Microsoft.AspNetCore.Builder;
using Stross.Downloader.SoundCloud.Configuration;
using Stross.Downloader.SoundCloud.Constants;
using Stross.Downloader.SoundCloud.Downloaders;
using Stross.Downloader.SoundCloud.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Bind configuration
builder.Services.AddOptions<DownloaderConfig>().BindConfiguration(DownloaderConfig.SectionName);

builder.Services.AddHttpClient();
builder.Services.AddHttpClient(Clients.SoundCloudClient, client =>
    {
        client.BaseAddress = new Uri("https://soundcloud.com/");
    });

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddScoped<SoundCloud>();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<SoundCloudService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();