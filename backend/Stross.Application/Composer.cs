using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Stross.Application.Behaviours;
using Stross.Application.Slices.MusicTrack;

namespace Stross.Application;

public static class Composer
{
    public static IHostApplicationBuilder AddApplication(this IHostApplicationBuilder app)
    {
        app.Services.AddApplicationServices();

        app.AddMusicTrackSlice();

        return app;
    }

    private static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(opt => opt.RegisterServicesFromAssembly(typeof(Composer).Assembly));
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));

        return services;
    }
}