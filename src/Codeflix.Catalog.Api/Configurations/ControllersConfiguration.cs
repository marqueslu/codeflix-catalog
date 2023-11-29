using Codeflix.Catalog.Api.Configurations.Polices;
using Codeflix.Catalog.Api.Filters;

namespace Codeflix.Catalog.Api.Configurations;

public static class ControllersConfiguration
{
    public static IServiceCollection AddAndConfigureControllers(this IServiceCollection services)
    {
        services
            .AddControllers(options =>
                options.Filters.Add(typeof(ApiGlobalExceptionFilter))
            )
            .AddJsonOptions(
                jsonOptions => { jsonOptions.JsonSerializerOptions.PropertyNamingPolicy = new JsonSnakeCasePolicy(); });
        services.AddDocumentation();
        return services;
    }

    private static IServiceCollection AddDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        return services;
    }

    public static WebApplication UseDocumentation(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment()) return app;
        app.UseSwagger();
        app.UseSwaggerUI(c =>
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Codeflix"));

        return app;
    }
}