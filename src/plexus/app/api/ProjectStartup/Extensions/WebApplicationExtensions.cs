using System.Security.Claims;

namespace Api.Extensions;

internal static class WebApplicationExtensions
{
    private const string DevCorsPolicy = "Dev";

    public static IServiceCollection AddApiCors(
        this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(DevCorsPolicy, policy => policy
                .WithOrigins("http://localhost:5044")
                .AllowAnyHeader()
                .AllowAnyMethod());
        });

        return services;
    }

    public static IServiceCollection AddApiControllers(
        this IServiceCollection services)
    {
        services.AddControllers();

        return services;
    }

    public static WebApplication UseApiPipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseApiSwagger();
        }
        else
        {
            app.UseHttpsRedirection();
        }

        app.UseStaticFiles();

        app.UseRouting();

        app.UseCors(DevCorsPolicy);

        app.UseAuthentication();

        app.UseAuthorization();

        return app;
    }

    public static WebApplication MapApiEndpoints(this WebApplication app)
    {
        app.MapControllers();

        app.MapGet("users/me", (ClaimsPrincipal user) => Results.Ok(new
        {
            UserId = user.FindFirstValue(ClaimTypes.NameIdentifier),
            Email = user.FindFirstValue(ClaimTypes.Email),
            Username = user.FindFirstValue("preferred_username"),
            Claims = user.Claims.Select(c => new
            {
                c.Type,
                c.Value
            })
        }))
        .RequireAuthorization();

        return app;
    }
}