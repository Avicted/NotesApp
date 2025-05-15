using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using MediatR;

namespace NotesApp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register MediatR
        services.AddMediatR(Assembly.GetExecutingAssembly());

        // Register any other application-specific services
        // For example, registering validators, services, etc.

        return services;
    }
}
