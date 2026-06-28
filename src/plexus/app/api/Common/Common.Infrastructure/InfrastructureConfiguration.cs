using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Common.Infrastructure.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
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

    public static IServiceCollection AddTokenAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var keycloak = KeycloakSettings.FromConfiguration(configuration);

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MetadataAddress      = keycloak.MetadataAddress;
                options.Authority            = keycloak.Authority;
                options.Audience             = keycloak.Audience;
                options.RequireHttpsMetadata = !environment.IsDevelopment();

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer              = keycloak.Issuer,
                    ValidAudience            = keycloak.Audience,
                    ValidateIssuer           = true,
                    ValidateAudience         = true,
                    ValidateLifetime         = true,
                    ValidateIssuerSigningKey = true,
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = AddKeycloakRealmRoles
                };
            });

        services.AddHttpContextAccessor();

        return services;
    }

    public static IServiceCollection AddApiAuthorization(
        this IServiceCollection services)
    {
        services.AddAuthorization();
        return services;
    }

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

    private static Task AddKeycloakRealmRoles(TokenValidatedContext context)
    {
        if (context.Principal?.Identity is not ClaimsIdentity claimsIdentity)
            return Task.CompletedTask;

        var realmAccess = context.Principal.FindFirst("realm_access")?.Value;

        if (string.IsNullOrWhiteSpace(realmAccess))
            return Task.CompletedTask;

        try
        {
            using var doc = JsonDocument.Parse(realmAccess);

            if (!doc.RootElement.TryGetProperty("roles", out var roles) ||
                roles.ValueKind is not JsonValueKind.Array)
                return Task.CompletedTask;

            foreach (var role in roles.EnumerateArray())
            {
                var roleName = role.GetString();

                if (string.IsNullOrWhiteSpace(roleName)) continue;

                var exists = claimsIdentity.Claims
                    .Any(c => c.Type == ClaimTypes.Role && c.Value == roleName);

                if (!exists)
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, roleName));
            }
        }
        catch (JsonException) { /* невалидный JSON — пропускаем */ }

        return Task.CompletedTask;
    }
}