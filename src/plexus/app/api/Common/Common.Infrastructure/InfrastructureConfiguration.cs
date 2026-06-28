using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Common.Infrastructure.Events;
using Common.Domain;
using Common.Application.Contracts;

namespace Common.Infrastructure;

public static class InfrastructureConfiguration
{
    public enum DbProvider
    {
        PostgreSQL,
        SqlServer
    }

    public static IServiceCollection AddDBStorage<TDbContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        Assembly assembly,
        string connectionStringName = "DefaultConnectionPostgres",
        DbProvider provider = DbProvider.PostgreSQL)
        where TDbContext : DbContext
        => services
            .AddDatabase<TDbContext>(configuration, connectionStringName, provider)
            .AddRepositories(assembly);

    public static IServiceCollection AddEventSourcing(this IServiceCollection services)
        => services.AddTransient<IEventDispatcher, EventDispatcher>();

    public static IHttpClientBuilder ConfigureDefaultHttpClientHandler(
        this IHttpClientBuilder builder)
        => builder
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(5)
            })
            .SetHandlerLifetime(Timeout.InfiniteTimeSpan);


    private static IServiceCollection AddDatabase<TDbContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionStringName,
        DbProvider provider)
        where TDbContext : DbContext
        {
        var connectionString = configuration.GetConnectionString(connectionStringName)
            ?? throw new InvalidOperationException(
                $"Connection string '{connectionStringName}' not found.");

        return services
            .AddDbContext<TDbContext>(options =>
            {
                _ = provider switch
                {
                    DbProvider.PostgreSQL => options.UseNpgsql(
                        connectionString,
                        npgsql => npgsql
                            .EnableRetryOnFailure(
                                maxRetryCount: 5,
                                maxRetryDelay: TimeSpan.FromSeconds(15),
                                errorCodesToAdd: null)
                            .MigrationsAssembly(typeof(TDbContext).Assembly.FullName)),

                    DbProvider.SqlServer => options.UseSqlServer(
                        connectionString,
                        sql => sql
                            .EnableRetryOnFailure(
                                maxRetryCount: 10,
                                maxRetryDelay: TimeSpan.FromSeconds(30),
                                errorNumbersToAdd: null)
                            .MigrationsAssembly(typeof(TDbContext).Assembly.FullName)),

                    _ => throw new NotSupportedException($"Provider '{provider}' is not supported.")
                };
            });
    }

    internal static IServiceCollection AddRepositories(
        this IServiceCollection services,
        Assembly assembly)
        => services.Scan(scan => scan
            .FromAssemblies(assembly)
            .AddClasses(classes => classes
                .AssignableTo(typeof(IDomainRepository<>))
                .AssignableTo(typeof(IQueryRepository<>)))
            .AsImplementedInterfaces()
            .WithTransientLifetime());
}