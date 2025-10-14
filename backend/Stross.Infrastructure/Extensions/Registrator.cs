using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Stross.Infrastructure.Services.GrpcService;

namespace Stross.Infrastructure.Extensions;

public static class Registrator
{
    public static IServiceCollection RegisterInfrastructure(this IServiceCollection services)
    {
        services.RegisterDbContext();
        services.RegisterApiServices();

        return services;
    }

    private static IServiceCollection RegisterDbContext(this IServiceCollection services)
    {
        services.AddDbContext<StrossContext>(options =>
            options.UseNpgsql(options =>
            {
                
            })
        );

        return services;
    }

    private static IServiceCollection RegisterApiServices(this IServiceCollection services)
    {
        services.AddScoped<IGrpcService, GrpcService>();

        return services;
    }
}