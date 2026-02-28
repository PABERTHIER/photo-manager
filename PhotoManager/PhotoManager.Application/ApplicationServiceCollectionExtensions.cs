using Microsoft.Extensions.DependencyInjection;

namespace PhotoManager.Application;

public static class ApplicationServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddApplication()
        {
            services.AddSingleton<IApplication, Application>();
        }
    }
}
