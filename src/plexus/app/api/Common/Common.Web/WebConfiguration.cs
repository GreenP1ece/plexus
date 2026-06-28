using Common.Application.Contracts;
using Common.Web.ModelBinders;
using Common.Web.Services;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Web;

public static class WebConfiguration
{
    public static IServiceCollection AddWebComponents(this IServiceCollection services,
        Type applicationConfigurationType)
    {
        services
            .AddValidatorsFromAssemblyContaining(applicationConfigurationType) 
            .AddScoped<ICurrentUser, CurrentUserService>();

        return services;
    }

    public static IServiceCollection AddModelBinders(
        this IServiceCollection services)
    {
        services
            .AddControllers(options => options
                .ModelBinderProviders
                .Insert(0, new ImageModelBinderProvider()));

        services
            .Configure<ApiBehaviorOptions>(options => options
                .SuppressModelStateInvalidFilter = true);

        return services;
    }

    public static IServiceCollection AddExceptionHandling(
        this IServiceCollection services)
        => services
            .AddProblemDetails()
            .AddExceptionHandler<ValidationExceptionHandler>()
            .AddExceptionHandler<NotFoundExceptionHandler>()
            .AddExceptionHandler<DomainExceptionHandler>();
}