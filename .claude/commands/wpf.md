# WPF Reference for PhotoManager

Answer the following WPF question or perform the WPF task for this project: $ARGUMENTS

Use the facts below to guide your answer or implementation.

---

## Project Structure

```
PhotoManager.UI/
├── Windows/      — {Feature}Window (modal: ShowDialog, modeless: Show)
├── Controls/     — {Feature}UserControl (reusable composed controls)
├── ViewModels/   — {Feature}ViewModel, base classes, Enums/
├── Converters/   — {Feature}Converter : IValueConverter or IMultiValueConverter
└── Models/       — {Action}EventHandler delegates + {Action}EventArgs classes
```

## MVVM

- All ViewModels → inherit `BaseViewModel` (implements `INotifyPropertyChanged`)
- Multi-step process VMs → inherit `BaseProcessViewModel<TC, TR>`
- Use C# 11 `field` keyword in property setters:
  ```csharp
  public string Title { get; private set { field = value; NotifyPropertyChanged(nameof(Title)); } }
  ```
- `SortableObservableCollection<T>` (in ViewModels/) for sortable lists — raises `Reset` on sort

## DI Wiring

- `App.xaml.cs` builds the container; exposes `static IServiceProvider? App.ServiceProvider`
- Registered as singletons: `MainWindow`, `ApplicationViewModel` (in `UiServiceCollectionExtensions`)
- XAML-instantiated UserControls → use `App.ServiceProvider?.GetService<T>() ?? NullLogger.Instance`
- Dialog windows + ViewModels → created on-demand in code-behind (not from DI)

## Async Rules

- Use `Dispatcher.InvokeAsync()` to marshal domain callbacks onto the UI thread
- Use `.ConfigureAwait(true)` in all WPF async methods (preserves UI synchronization context)

## Converters

- Implement `IValueConverter` or `IMultiValueConverter`
- `ConvertBack` → `throw new NotImplementedException()` for read-only converters
- Register as `StaticResource` in the relevant XAML resources section

## Visibility Pattern

Prefer computed properties returning `Visibility` over converter bindings:
```csharp
public Visibility ThumbnailsVisible => AppMode == AppMode.Thumbnails ? Visibility.Visible : Visibility.Hidden;
```

## Custom Events

Define delegate + args in `Models/`:
```csharp
public delegate void FolderAddedEventHandler(object sender, FolderAddedEventArgs e);
public class FolderAddedEventArgs : EventArgs { public required Folder Folder { get; init; } }
```

## Naming Conventions

| Element | Pattern | Example |
|---------|---------|---------|
| Window | `{Feature}Window` | `FindDuplicatedAssetsWindow` |
| UserControl | `{Feature}UserControl` | `ThumbnailsUserControl` |
| ViewModel | `{Feature}ViewModel` | `ApplicationViewModel` |
| Converter | `{Feature}Converter` | `FileSizeConverter` |
| Event delegate | `{Action}EventHandler` | `FolderAddedEventHandler` |
| Event args | `{Action}EventArgs` | `FolderAddedEventArgs` |

## Styles

- Global styles in `App.xaml` under `Application.Resources`
- Hover effects via `<Style.Triggers>` with `Trigger Property="IsMouseOver"`
- Reference styles with `Style="{StaticResource StyleName}"`
