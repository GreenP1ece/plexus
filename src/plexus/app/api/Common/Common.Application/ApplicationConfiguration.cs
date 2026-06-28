
using System.Reflection;
using Common.Application.Behaviours;
using Common.Application.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Common.Application.Extensions;
namespace Common.Application;
public static class ApplicationConfiguration
{
    public static IServiceCollection AddCommonApplication(
        this IServiceCollection services,
        IConfiguration configuration,
        Assembly assembly)
        => services
            .Configure<ApplicationSettings>(
                configuration.GetSection(nameof(ApplicationSettings)),
                options => options.BindNonPublicProperties = true)
            .AddEventHandlers(assembly)
            .AddMediator(options =>
            {
                options.ServiceLifetime = ServiceLifetime.Scoped;
            })
            .AddMappers(assembly)
            .AddTransient(
                typeof(Mediator.IPipelineBehavior<,>),
                typeof(RequestValidationBehavior<,>));


    private static IServiceCollection AddEventHandlers(
        this IServiceCollection services,
        Assembly assembly)
        => services.Scan(scan => scan
            .FromAssemblies(assembly)
            .AddClasses(classes => classes
                .AssignableTo(typeof(IEventHandler<>)))
            .AsImplementedInterfaces()
            .WithTransientLifetime());
}