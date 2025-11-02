using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Stross.Application.Slices.Thumbnail;

public static class ThumbnailComposer
{
    public static IHostApplicationBuilder AddThumbnailSlice(this IHostApplicationBuilder builder)
    {
        builder.Services.RegisterThumbnailSliceServices();

        return builder;
    }

    private static IServiceCollection RegisterThumbnailSliceServices(this IServiceCollection services)
    {

        return services;
    }
}