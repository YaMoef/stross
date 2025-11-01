using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Stross.Application.Slices.MusicTrack;

public static class MusicTrackComposer
{
    public static IHostApplicationBuilder AddMusicTrackSlice(this IHostApplicationBuilder builder)
    {
        builder.Services.RegisterMusicTrackSliceServices();

        return builder;
    }

    private static IServiceCollection RegisterMusicTrackSliceServices(this IServiceCollection services)
    {

        return services;
    }
}