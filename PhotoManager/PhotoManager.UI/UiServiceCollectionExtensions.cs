using Microsoft.Extensions.DependencyInjection;
using PhotoManager.UI.ViewModels;
using PhotoManager.UI.Windows;

namespace PhotoManager.UI;

public static class UiServiceCollectionExtensions
{
    public static void AddUi(this IServiceCollection services)
    {
        services.AddSingleton<MainWindow>();
        services.AddSingleton<ApplicationViewModel>();
    }
}
