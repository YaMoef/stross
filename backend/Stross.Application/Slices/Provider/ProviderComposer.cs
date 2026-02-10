using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Stross.Application.Slices.Provider;

public static class ProviderComposer
{
    public static IHostApplicationBuilder AddProviderSlice(this IHostApplicationBuilder builder)
    {
        builder.Services.RegisterProviderSliceServices();

        return builder;
    }

    private static IServiceCollection RegisterProviderSliceServices(this IServiceCollection services)
    {

        return services;
    }
}
