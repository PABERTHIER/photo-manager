---
applyTo: "**/PhotoManager.UI/**/*.cs,**/PhotoManager.UI/**/*.xaml"
---

# WPF Code Standards for PhotoManager

## Project Structure
```
PhotoManager.UI/
├── Windows/       — Main window + dialog windows ({Feature}Window)
├── Controls/      — Reusable UserControls ({Feature}UserControl)
├── ViewModels/    — MVVM ViewModels ({Feature}ViewModel), base classes, enums
├── Converters/    — IValueConverter / IMultiValueConverter implementations
└── Models/        — Custom event delegates and EventArgs
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
Use computed properties returning `Visibility` directly — do not rely on converters for simple show/hide:

```csharp
public Visibility ThumbnailsVisible => AppMode == AppMode.Thumbnails ? Visibility.Visible : Visibility.Hidden;
```

### Collections
- Use `ObservableCollection<T>` for dynamic UI lists
- Use `SortableObservableCollection<T>` (project type) when sorting is needed — it raises `Reset` notification

## Naming Conventions

| Element | Pattern | Example |
|---------|---------|---------|
| Window | `{Feature}Window` | `FindDuplicatedAssetsWindow` |
| UserControl | `{Feature}UserControl` | `ThumbnailsUserControl` |
| ViewModel | `{Feature}ViewModel` | `ApplicationViewModel` |
| Converter | `{Feature}Converter` | `FileSizeConverter` |
| Event delegate | `{Action}EventHandler` | `FolderAddedEventHandler` |
| Event args | `{Action}EventArgs` | `FolderAddedEventArgs` |

## Value Converters
- Implement `IValueConverter` or `IMultiValueConverter`
- `ConvertBack` must `throw new NotImplementedException()` for read-only converters
- Register converters as `StaticResource` in the relevant `Resources` section

```csharp
public class FileSizeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) { ... }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
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
- XAML-instantiated controls (UserControls) resolve services from the static `App.ServiceProvider`:

```csharp
_logger = App.ServiceProvider?.GetService<ILogger<ThumbnailsUserControl>>()
          ?? NullLogger<ThumbnailsUserControl>.Instance;
```

- Dialog windows and their ViewModels are created on-demand in code-behind (not from DI):

```csharp
FindDuplicatedAssetsViewModel vm = new(_application);
FindDuplicatedAssetsWindow window = new(vm, _loggerFactory.CreateLogger<FindDuplicatedAssetsWindow>());
window.ShowDialog();
```

## Async & Thread Safety
- Use `Dispatcher.InvokeAsync()` to marshal domain callbacks back to the UI thread
- Always use `.ConfigureAwait(true)` in WPF async methods to preserve the UI synchronization context

```csharp
_catalogTask = ViewModel.CatalogAssets(
    e => Dispatcher.InvokeAsync(() => ViewModel.NotifyCatalogChange(e)),
    _cancellationTokenSource.Token);

await _catalogTask.ConfigureAwait(true);
```

## WPF Types Used Across All Layers

The following WPF types from `System.Windows.Media.Imaging` are used throughout **all layers** (not just UI):
- `BitmapImage` — used as image data carrier in `Asset.ImageData`, service interfaces, and repository
- `Rotation` — WPF enum used for image rotation in `Asset.ImageRotation`, service signatures, and DB mapping

These are **intentionally present** in Domain, Application, Infrastructure, and Common via `GlobalUsings.cs`.
Do **not** remove or replace them with non-WPF abstractions unless performing an explicit architectural refactor.

When adding new image-related methods to interfaces or services, use `BitmapImage` and `Rotation` consistently
with the existing codebase pattern.
