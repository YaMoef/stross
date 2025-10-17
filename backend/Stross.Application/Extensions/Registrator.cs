using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Stross.Application.Behaviours;

namespace Stross.Application.Extensions;

public static class Registrator
{
    public static IServiceCollection RegisterApplication(this IServiceCollection services)
    {
        services.RegisterServices();
        // services.AddMediatR(config =>
        // {
        //
        // });
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));

        return services;
    }
    private static IServiceCollection RegisterServices(this IServiceCollection services)
    {
        return services;
    }
}