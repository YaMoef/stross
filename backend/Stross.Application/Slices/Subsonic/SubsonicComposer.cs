using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Stross.Application.Slices.Subsonic;

public static class SubsonicComposer
{
    public static IHostApplicationBuilder AddSubsonicSlice(this IHostApplicationBuilder builder)
    {
        builder.Services.RegisterSubsonicSliceServices();

        return builder;
    }

    private static IServiceCollection RegisterSubsonicSliceServices(this IServiceCollection services)
    {

        return services;
    }
}
