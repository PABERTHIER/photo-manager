using PhotoManager.UI.Services;
using PhotoManager.UI.Windows;

namespace PhotoManager.UI;

public static class UiServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddUi()
        {
            services.AddSingleton<ISingleInstanceService, SingleInstanceService>();
            services.AddSingleton<MainWindow>();
            services.AddSingleton<ApplicationViewModel>();
        }
    }
}
