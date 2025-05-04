using Microsoft.Extensions.DependencyInjection;

namespace PhotoManager.Application;

public static class ApplicationServiceCollectionExtensions
{
    public static void AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IApplication, Application>();
    }
}
