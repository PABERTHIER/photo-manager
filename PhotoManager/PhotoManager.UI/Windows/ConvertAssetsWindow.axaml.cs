using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PhotoManager.Application;
using PhotoManager.UI.ViewModels;
using System.Diagnostics.CodeAnalysis;

namespace PhotoManager.UI.Windows;

/// <summary>
/// Window for converting assets to other formats.
/// </summary>
[ExcludeFromCodeCoverage]
public partial class ConvertAssetsWindow : Window
{
    private readonly ILogger<ConvertAssetsWindow> _logger;

    public ConvertAssetsWindow() : this(
        new(App.ServiceProvider?.GetRequiredService<IApplication>()
            ?? throw new InvalidOperationException("Application service is not available.")),
        App.ServiceProvider.GetRequiredService<ILogger<ConvertAssetsWindow>>()
        ?? throw new InvalidOperationException("ConvertAssetsWindow logger service is not available."))
    {
    }

    public ConvertAssetsWindow(ConvertAssetsViewModel viewModel, ILogger<ConvertAssetsWindow> logger)
    {
        _logger = logger;

        try
        {
            InitializeComponent();
            DataContext = viewModel;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExMessage}", ex.Message);
            throw;
        }
    }

    private ConvertAssetsViewModel ViewModel => (ConvertAssetsViewModel)DataContext!;

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void SelectImages_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _ = SelectImagesAsync();
    }

    private async Task SelectImagesAsync()
    {
        try
        {
            FilePickerFileType imageFiles = new("Supported images")
            {
                Patterns = ["*.heic", "*.heif", "*.png", "*.jpg", "*.jpeg"]
            };
            IReadOnlyList<IStorageFile> files = await StorageProvider.OpenFilePickerAsync(new()
            {
                Title = "Select images to convert",
                AllowMultiple = true,
                FileTypeFilter = [imageFiles]
            });

            ViewModel.SetSelectedFilePaths(files.Select(file => file.TryGetLocalPath()).OfType<string>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExMessage}", ex.Message);
            await MessageDialogWindow.ShowAsync(this, ex.Message, "Image selection failed");
        }
    }

    private void Convert_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _ = ConvertAsync();
    }

    private async Task ConvertAsync()
    {
        try
        {
            await ViewModel.ConvertAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExMessage}", ex.Message);
            await MessageDialogWindow.ShowAsync(this, ex.Message, "Conversion failed");
        }
    }

    private void Close_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close();
    }
}
