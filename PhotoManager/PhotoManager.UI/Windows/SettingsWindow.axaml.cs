using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PhotoManager.Application;
using PhotoManager.UI.Configuration;
using PhotoManager.UI.ViewModels;
using System.Diagnostics.CodeAnalysis;

namespace PhotoManager.UI.Windows;

/// <summary>
/// Window for viewing and editing the application settings.
/// </summary>
[ExcludeFromCodeCoverage]
public partial class SettingsWindow : Window
{
    private readonly ILogger<SettingsWindow> _logger;

    public SettingsWindow() : this(
        new(App.ServiceProvider?.GetRequiredService<IApplication>()
            ?? throw new InvalidOperationException("Application service is not available.")),
        App.ServiceProvider.GetRequiredService<ILogger<SettingsWindow>>()
        ?? throw new InvalidOperationException("SettingsWindow logger service is not available."))
    {
    }

    public SettingsWindow(SettingsViewModel viewModel, ILogger<SettingsWindow> logger)
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

    private SettingsViewModel ViewModel => (SettingsViewModel)DataContext!;

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void BrowseAssetsDirectory_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _ = BrowseAssetsDirectoryAsync();
    }

    private async Task BrowseAssetsDirectoryAsync()
    {
        string? selectedPath = await SelectFolderAsync("Select the assets directory");

        if (selectedPath != null)
        {
            ViewModel.AssetsDirectory = selectedPath;
        }
    }

    private void BrowseExemptedFolder_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _ = BrowseExemptedFolderAsync();
    }

    private async Task BrowseExemptedFolderAsync()
    {
        string? selectedPath = await SelectFolderAsync("Select the exempted folder");

        if (selectedPath != null)
        {
            ViewModel.ExemptedFolderPath = selectedPath;
        }
    }

    private async Task<string?> SelectFolderAsync(string title)
    {
        try
        {
            IReadOnlyList<IStorageFolder> folders = await StorageProvider.OpenFolderPickerAsync(new()
            {
                Title = title,
                AllowMultiple = false
            });

            return folders.Count == 0 ? null : folders[0].TryGetLocalPath();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExMessage}", ex.Message);
            await MessageDialogWindow.ShowAsync(this, ex.Message, "Folder selection failed");

            return null;
        }
    }

    private void Save_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _ = SaveAsync();
    }

    private async Task SaveAsync()
    {
        try
        {
            if (ViewModel.Save())
            {
                Avalonia.Application.Current!.RequestedThemeVariant =
                    ThemeSettingsReader.GetRequestedThemeVariant(ViewModel.ThemeMode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExMessage}", ex.Message);
            await MessageDialogWindow.ShowAsync(this, ex.Message, "Settings save failed");
        }
    }

    private void Close_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close();
    }
}
