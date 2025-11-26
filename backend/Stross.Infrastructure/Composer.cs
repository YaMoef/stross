using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Stross.Config;
using Stross.Infrastructure.Services.AudioFileMetadataService;
using Stross.Infrastructure.Services.AuthenticationService;
using Stross.Infrastructure.Services.GrpcService;
using Stross.Infrastructure.Services.ThumbnailService;

namespace Stross.Infrastructure;

public static class Composer
{
    public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder app)
    {
        app.Services.AddInfrastructureServices();
        app.Services.RegisterDbContext();
        app.Services.AddHttpClient();

        return app;
    }

    private static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddSingleton<IGrpcService, GrpcService>();
        services.AddSingleton<IThumbnailService, ThumbnailService>();
        services.AddSingleton<IAudioFileMetadataService, AudioFileMetadataService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();

        return services;
    }

    private static IServiceCollection RegisterDbContext(this IServiceCollection services)
    {
        services.AddDbContext<StrossContext>((sp, options) =>
            {
                DatabaseConfig dbConfig = sp.GetRequiredService<IOptionsSnapshot<DatabaseConfig>>().Value;

                options.UseNpgsql(dbConfig.ConnectionString);
            }
        );

        return services;
    }
}