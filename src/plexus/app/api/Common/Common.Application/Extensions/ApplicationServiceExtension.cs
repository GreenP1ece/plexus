using System.Reflection;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Application.Extensions;

public static class MapperRegistrationExtensions
{
    public static IServiceCollection AddMappers(
        this IServiceCollection services,
        Assembly assembly)
    {
        services.Scan(scan => scan
            .FromAssemblies(assembly)
            .AddClasses(classes => classes
                .AssignableTo<IMapper>()
                .Where(t => t is { IsAbstract: false, IsInterface: false }))
            .AsSelf()
            .AsImplementedInterfaces()
            .WithSingletonLifetime());

        return services;
    }
}