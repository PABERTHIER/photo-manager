# Avalonia UI Reference for PhotoManager

Answer the following Avalonia UI question or perform the UI task for this project: $ARGUMENTS

Use the facts below to guide your answer or implementation.

---

## Project Structure

```
PhotoManager.UI/
├── Windows/      — {Feature}Window (modal: ShowDialog, modeless: Show)
├── Controls/     — {Feature}UserControl (reusable composed controls)
├── ViewModels/   — {Feature}ViewModel, base classes, Enums/
├── Converters/   — {Feature}Converter : IValueConverter
├── Models/       — {Action}EventHandler delegates + {Action}EventArgs classes
├── Services/     — UI-layer services (single-instance, theme)
└── Configuration/ — UI settings classes
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

- `App.axaml.cs` builds the container; exposes `static IServiceProvider? App.ServiceProvider`
- Registered as singletons: `MainWindow`, `ApplicationViewModel` (in `UiServiceCollectionExtensions`)
- AXAML-instantiated UserControls → use `App.ServiceProvider?.GetService<T>() ?? NullLogger.Instance`
- Dialog windows + ViewModels → created on-demand in code-behind (not from DI)

## Async Rules

- Use `Dispatcher.UIThread.InvokeAsync()` to marshal domain callbacks onto the UI thread
- Use `.ConfigureAwait(true)` in all UI async methods (preserves synchronization context)

## Converters

- Implement `IValueConverter` (Avalonia's `Avalonia.Data.Converters.IValueConverter`)
- `ConvertBack` → `throw new NotImplementedException()` for read-only converters
- Register as resources in the relevant AXAML file

## Visibility Pattern

Prefer computed `bool` properties for `IsVisible` bindings:
```csharp
public bool IsThumbnailsVisible => AppMode == AppMode.Thumbnails;
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

- Avalonia Fluent theme with System/Light/Dark mode support
- Global styles in `App.axaml` under `Application.Styles`
- Use Avalonia `Styles` and `ControlTheme` for custom styling
