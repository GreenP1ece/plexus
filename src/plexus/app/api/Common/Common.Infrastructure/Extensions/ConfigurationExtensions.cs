
using Microsoft.Extensions.Configuration;

namespace Common.Infrastructure.Extensions;
internal static class ConfigurationExtensions
{
    public static string GetRequiredValue(
        this IConfiguration configuration,
        string key)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var value = configuration[key];

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"Configuration value '{key}' is not configured.");
        }

        return value;
    }

    public static string GetApplicationConnectionString(
        this IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString =
            Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
            ?? configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Database connection string is not configured. " +
                "Set DB_CONNECTION_STRING env var or ConnectionStrings:DefaultConnection in appsettings.");
        }

        return connectionString;
    }
}