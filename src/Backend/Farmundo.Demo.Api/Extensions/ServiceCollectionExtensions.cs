using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Farmundo.Demo.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiVersioningAndSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            var xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly);
            foreach (var xml in xmlFiles)
                c.IncludeXmlComments(xml, includeControllerXmlComments: true);
        });
        return services;
    }
}
