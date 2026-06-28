
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

}