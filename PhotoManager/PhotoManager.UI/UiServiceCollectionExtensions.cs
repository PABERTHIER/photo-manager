using Microsoft.Extensions.DependencyInjection;
using PhotoManager.UI.ViewModels;
using PhotoManager.UI.Windows;

namespace PhotoManager.UI;

public static class UiServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddUi()
        {
            services.AddSingleton<MainWindow>();
            services.AddSingleton<ApplicationViewModel>();
        }
    }
}
