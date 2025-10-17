
using Stross.Downloader.YT.Services;
using Stross.Downloader.YT.Configuration;
using Stross.Downloader.YT.Downloaders;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();  
builder.Logging.AddConsole();

// Bind configuration
builder.Services.AddOptions<DownloaderConfig>().BindConfiguration(DownloaderConfig.SectionName);

builder.Services.AddHttpClient();

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddScoped<YtDlp>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<YtDlpService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();

