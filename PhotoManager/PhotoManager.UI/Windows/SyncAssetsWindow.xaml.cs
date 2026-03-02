using Microsoft.Extensions.Logging;
using PhotoManager.Domain;
using PhotoManager.UI.ViewModels;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PhotoManager.UI.Windows;

/// <summary>
/// Interaction logic for SyncAssetsWindow.xaml
/// </summary>
[ExcludeFromCodeCoverage]
public partial class SyncAssetsWindow
{
    private readonly ILogger<SyncAssetsWindow> _logger;

    public SyncAssetsWindow(SyncAssetsViewModel viewModel, ILogger<SyncAssetsWindow> logger)
    {
        _logger = logger;

        try
        {
            InitializeComponent();

            DataContext = viewModel;
            Initialize();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExMessage}", ex.Message);
        }
    }

    public SyncAssetsViewModel ViewModel => (SyncAssetsViewModel)DataContext;

    private void Initialize()
    {
        SyncAssetsConfiguration configuration = ViewModel.GetProcessConfiguration();
        ViewModel.Definitions = [.. configuration.Definitions];
    }

    private void ContinueButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        try
        {
            Cursor = Cursors.Wait;
            ViewModel.AdvanceStep();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExMessage}", ex.Message);
        }
        finally
        {
            Cursor = Cursors.Arrow;
        }
    }

    private void DeleteLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        try
        {
            DeleteDefinition(((TextBlock)e.Source).DataContext);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExMessage}", ex.Message);
        }
    }

    private void MoveUpLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        try
        {
            MoveUpDefinition(((TextBlock)e.Source).DataContext);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExMessage}", ex.Message);
        }
    }

    private void MoveDownLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        try
        {
            MoveDownDefinition(((TextBlock)e.Source).DataContext);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExMessage}", ex.Message);
        }
    }

    private void SaveButton_MouseLeftButtonDown(object sender, RoutedEventArgs e)
    {
        try
        {
            Cursor = Cursors.Wait;
            Save(ViewModel.Definitions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExMessage}", ex.Message);
        }
        finally
        {
            Cursor = Cursors.Arrow;
        }
    }

    // TODO: async task
    private async void RunButton_MouseLeftButtonDown(object sender, RoutedEventArgs e)
    {
        try
        {
            Cursor = Cursors.Wait;
            await RunProcess().ConfigureAwait(true);
            ViewModel.AdvanceStep();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExMessage}", ex.Message);
        }
        finally
        {
            Cursor = Cursors.Arrow;
        }
    }

    private void CloseButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        try
        {
            Close();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExMessage}", ex.Message);
        }
    }

    private void DeleteDefinition(object selected)
    {
        // Evaluates if it is an existing item or the NewItemPlaceholder.
        if (selected is SyncAssetsDirectoriesDefinition definition)
        {
            ViewModel.DeleteDefinition(definition);
        }
    }

    private void MoveUpDefinition(object selected)
    {
        // Evaluates if it is an existing item or the NewItemPlaceholder.
        if (selected is SyncAssetsDirectoriesDefinition definition)
        {
            ViewModel.MoveUpDefinition(definition);
        }
    }

    private void MoveDownDefinition(object selected)
    {
        // Evaluates if it is an existing item or the NewItemPlaceholder.
        if (selected is SyncAssetsDirectoriesDefinition definition)
        {
            ViewModel.MoveDownDefinition(definition);
        }
    }

    private void Save(ObservableCollection<SyncAssetsDirectoriesDefinition> definitions)
    {
        SyncAssetsConfiguration syncAssetsConfiguration = new();
        syncAssetsConfiguration.Definitions.AddRange(definitions);
        ViewModel.SetProcessConfiguration(syncAssetsConfiguration);
    }

    private async Task RunProcess()
    {
        Save(ViewModel.Definitions);
        await ViewModel.RunProcessAsync(e => Dispatcher.Invoke(() => ViewModel.NotifyProcessStatusChanged(e)));
    }
}
