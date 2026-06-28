namespace PhotoManager.UI.Windows;

/// <summary>
/// Window for configuring and running asset synchronization between folders.
/// </summary>
[ExcludeFromCodeCoverage]
public partial class SyncAssetsWindow : Window
{
    private readonly ILogger<SyncAssetsWindow> _logger;

    public SyncAssetsWindow()
    {
        _logger = NullLogger<SyncAssetsWindow>.Instance;
        InitializeComponent();
    }

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

    public SyncAssetsViewModel ViewModel => (SyncAssetsViewModel)DataContext!;

    private void Initialize()
    {
        SyncAssetsConfiguration configuration = ViewModel.GetProcessConfiguration();
        ViewModel.Definitions = CreateObservableCollection(configuration.Definitions);
    }

    private static ObservableCollection<SyncAssetsDirectoriesDefinition> CreateObservableCollection(
        List<SyncAssetsDirectoriesDefinition> definitions)
    {
        ObservableCollection<SyncAssetsDirectoriesDefinition> result = [];

        for (int i = 0; i < definitions.Count; i++)
        {
            result.Add(definitions[i]);
        }

        return result;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void ContinueButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        try
        {
            Cursor = new(StandardCursorType.Wait);
            ViewModel.AdvanceStep();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExMessage}", ex.Message);
        }
        finally
        {
            Cursor = new(StandardCursorType.Arrow);
        }
    }

    private void DeleteButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        try
        {
            DeleteDefinition(((Control)sender!).DataContext);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExMessage}", ex.Message);
        }
    }

    private void AddButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        try
        {
            ViewModel.AddDefinition();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExMessage}", ex.Message);
        }
    }

    private void MoveUpButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        try
        {
            MoveUpDefinition(((Control)sender!).DataContext);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExMessage}", ex.Message);
        }
    }

    private void MoveDownButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        try
        {
            MoveDownDefinition(((Control)sender!).DataContext);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExMessage}", ex.Message);
        }
    }

    private void SaveButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        try
        {
            Cursor = new(StandardCursorType.Wait);
            Save(ViewModel.Definitions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExMessage}", ex.Message);
        }
        finally
        {
            Cursor = new(StandardCursorType.Arrow);
        }
    }

    private void RunButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _ = RunButtonClickAsync();
    }

    private async Task RunButtonClickAsync()
    {
        try
        {
            Cursor = new(StandardCursorType.Wait);
            await RunProcess();
            ViewModel.AdvanceStep();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExMessage}", ex.Message);
        }
        finally
        {
            Cursor = new(StandardCursorType.Arrow);
        }
    }

    private void CloseButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
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

    private void DeleteDefinition(object? selected)
    {
        if (selected is SyncAssetsDirectoriesDefinition definition)
        {
            ViewModel.DeleteDefinition(definition);
        }
    }

    private void MoveUpDefinition(object? selected)
    {
        if (selected is SyncAssetsDirectoriesDefinition definition)
        {
            ViewModel.MoveUpDefinition(definition);
        }
    }

    private void MoveDownDefinition(object? selected)
    {
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
        await ViewModel.RunProcessAsync(
            e => Avalonia.Threading.Dispatcher.UIThread.Invoke(() => ViewModel.NotifyProcessStatusChanged(e)));
    }
}
