using Microsoft.OpenApi;

namespace Demo.Web.Api.Bootstrap;

internal static class SwaggerExtensions
{
    /// <summary>
    /// Configures Swagger/OpenAPI for the Demo API
    /// </summary>
    public static IServiceCollection AddApiSwagger(this IServiceCollection services)
    {
        services
           .AddSwaggerGen(options =>
           {
               options.AddSecurityDefinition(
                   name: "Bearer",
                   securityScheme:
                       new OpenApiSecurityScheme
                       {
                           Description = @"Please enter JWT with Bearer into field. Example: 'Bearer 12345abcdef'",
                           Name = "Authorization",
                           In = ParameterLocation.Header,
                           Type = SecuritySchemeType.ApiKey,
                           Scheme = "Bearer"
                       });

               options.AddSecurityRequirement(document =>
                   new OpenApiSecurityRequirement
                   {
                       [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                   });
           });

        return services;
    }
}
