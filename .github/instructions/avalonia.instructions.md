---
applyTo: "**/PhotoManager.UI/**/*.cs,**/PhotoManager.UI/**/*.axaml"
---

# Avalonia UI Code Standards for PhotoManager

## Project Structure

```
PhotoManager.UI/
├── Windows/       — Main window + dialog windows ({Feature}Window)
├── Controls/      — Reusable UserControls ({Feature}UserControl)
├── ViewModels/    — MVVM ViewModels ({Feature}ViewModel), base classes, enums
├── Converters/    — IValueConverter implementations
├── Models/        — Custom event delegates and EventArgs
├── Services/      — UI-layer services (single-instance, theme)
├── Configuration/ — UI settings classes
└── Assets/        — Application icons and resources
```

## MVVM Patterns

### ViewModel Base Classes

- All ViewModels inherit from `BaseViewModel` (implements `INotifyPropertyChanged`)
- Multi-step process ViewModels inherit from `BaseProcessViewModel<TC, TR>`
- Call `NotifyPropertyChanged(nameof(Property))` inside property setters

```csharp
public string Title
{
    get;
    private set
    {
        field = value;  // C# 11 field keyword
        NotifyPropertyChanged(nameof(Title));
    }
}
```

### Computed Visibility Properties

Use computed properties returning `bool` for `IsVisible` bindings — do not rely on converters for simple show/hide:

```csharp
public bool IsThumbnailsVisible => AppMode == AppMode.Thumbnails;
```

### Collections

- Use `ObservableCollection<T>` for dynamic UI lists
- Use `SortableObservableCollection<T>` (project type) when sorting is needed — it raises `Reset` notification

## Naming Conventions

| Element        | Pattern                | Example                      |
| -------------- | ---------------------- | ---------------------------- |
| Window         | `{Feature}Window`      | `FindDuplicatedAssetsWindow` |
| UserControl    | `{Feature}UserControl` | `ThumbnailsUserControl`      |
| ViewModel      | `{Feature}ViewModel`   | `ApplicationViewModel`       |
| Converter      | `{Feature}Converter`   | `FileSizeConverter`          |
| Event delegate | `{Action}EventHandler` | `FolderAddedEventHandler`    |
| Event args     | `{Action}EventArgs`    | `FolderAddedEventArgs`       |

## Value Converters

- Implement `IValueConverter` (Avalonia's `Avalonia.Data.Converters.IValueConverter`)
- `ConvertBack` must `throw new NotImplementedException()` for read-only converters
- Register converters as resources in the relevant AXAML file

```csharp
public class FileSizeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) { ... }
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
```

## Custom Events

Define event delegates and args in `Models/`:

```csharp
public delegate void FolderAddedEventHandler(object sender, FolderAddedEventArgs e);

public class FolderAddedEventArgs : EventArgs
{
    public required Folder Folder { get; init; }
}
```

## Dependency Injection

- `MainWindow` and `ApplicationViewModel` are registered as **singletons** in `UiServiceCollectionExtensions`
- AXAML-instantiated controls resolve services from the static `App.ServiceProvider`:

```csharp
_logger = App.ServiceProvider?.GetService<ILogger<ThumbnailsUserControl>>()
          ?? NullLogger<ThumbnailsUserControl>.Instance;
```

- Dialog windows and their ViewModels are created on-demand in code-behind (not from DI):

```csharp
FindDuplicatedAssetsViewModel vm = new(_application);
FindDuplicatedAssetsWindow window = new(vm, _loggerFactory.CreateLogger<FindDuplicatedAssetsWindow>());
window.ShowDialog(this);
```

## Async & Thread Safety

- Use `Dispatcher.UIThread.InvokeAsync()` to marshal domain callbacks back to the UI thread
- Always use `.ConfigureAwait(true)` in UI async methods to preserve the synchronization context

```csharp
_catalogTask = ViewModel.CatalogAssets(
    e => Dispatcher.UIThread.InvokeAsync(() => ViewModel.NotifyCatalogChange(e)),
    _cancellationTokenSource.Token);

await _catalogTask.ConfigureAwait(true);
```

## Avalonia-Specific Patterns

- Use `IsVisible` (bool) instead of WPF's `Visibility` enum
- Use Avalonia Fluent theme (`FluentTheme`) with configurable theme mode (System/Light/Dark)
- AXAML files use `.axaml` extension (not `.xaml`)
- Code-behind uses `.axaml.cs` extension
- Use `Avalonia.Controls.DataGrid` for tabular data display
- Use `VirtualizingStackPanel` in `ItemsControl` for large lists (performance)
