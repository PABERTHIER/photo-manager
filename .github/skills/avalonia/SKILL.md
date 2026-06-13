---
name: avalonia
description: >
  Avalonia UI reference guide for PhotoManager: UI conventions, MVVM patterns, and cross-platform UI patterns.
  Use this skill when working on UI code, adding image-processing methods, or needing to understand
  the Avalonia UI layer of the application.
---

You are an expert on the Avalonia UI usage in PhotoManager. Answer or implement the following: ${input:question:What Avalonia UI topic or task can I help you with?}

Use the facts below as your authoritative reference.

---

## UI Project Structure

```
PhotoManager.UI/
├── App.axaml / App.axaml.cs       — DI container, Serilog setup, single-instance enforcement
├── Program.cs                      — Application entry point (BuildAvaloniaApp)
├── Windows/                        — {Feature}Window  (ShowDialog = modal, Show = modeless)
│   ├── MainWindow.axaml/.cs        — Main shell, async catalog background work
│   ├── FindDuplicatedAssetsWindow  — Duplicate detection workflow
│   ├── SyncAssetsWindow            — Sync workflow
│   ├── FolderNavigationWindow      — Folder picker
│   └── AboutWindow                 — About dialog
├── Controls/                       — {Feature}UserControl  (reusable composed controls)
│   ├── ViewerUserControl           — Full-screen image viewer
│   ├── ThumbnailsUserControl       — Virtualized thumbnail list
│   └── FolderNavigationControl     — TreeView folder browser
├── ViewModels/                     — {Feature}ViewModel, base classes, Enums/
├── Converters/                     — {Feature}Converter
├── Models/                         — {Action}EventHandler delegates + {Action}EventArgs classes
├── Services/                       — UI-layer services (single-instance, theme)
└── Configuration/                  — UI settings classes
```

---

## MVVM Patterns

### ViewModel Base Classes

- `BaseViewModel` — implements `INotifyPropertyChanged`; call `NotifyPropertyChanged(nameof(Prop))`
- `BaseProcessViewModel<TC, TR>` — for multi-step process workflows (Describe → Configure → Run → Results)
- All ViewModels inherit one of the above; no external MVVM framework is used

### Property Pattern (C# 11 `field` keyword)

```csharp
public SortCriteria SortCriteria
{
    get;
    private set
    {
        field = value;
        NotifyPropertyChanged(nameof(SortCriteria));
    }
}
```

### Visibility — Prefer Computed bool Properties

```csharp
public bool IsThumbnailsVisible => AppMode == AppMode.Thumbnails;
```

### Collections

- `ObservableCollection<T>` — dynamic UI lists
- `SortableObservableCollection<T>` (project type in `ViewModels/`) — raises `Reset` on Sort()

---

## Dependency Injection

- DI chain in `App.axaml.cs`: `AddInfrastructure()` → `AddDomain()` → `AddApplication()` → `AddUi()`
- `App.ServiceProvider` (static) is the escape hatch for AXAML-instantiated UserControls:
  ```csharp
  _logger = App.ServiceProvider?.GetService<ILogger<ThumbnailsUserControl>>()
            ?? NullLogger<ThumbnailsUserControl>.Instance;
  ```
- Singletons registered in `UiServiceCollectionExtensions`: `MainWindow`, `ApplicationViewModel`
- Dialog windows and their ViewModels are **created on-demand** in code-behind (not via DI):
  ```csharp
  FindDuplicatedAssetsViewModel vm = new(_application);
  FindDuplicatedAssetsWindow window = new(vm, _loggerFactory.CreateLogger<FindDuplicatedAssetsWindow>());
  await window.ShowDialog(this);
  ```

---

## Async & Thread Safety

- Background work launched in window `Opened` event handler
- Always use `Dispatcher.UIThread.InvokeAsync()` to marshal domain callbacks back to the UI thread
- Always use `.ConfigureAwait(true)` in UI async methods (preserves synchronization context)

```csharp
_catalogTask = ViewModel.CatalogAssets(
    e => Dispatcher.UIThread.InvokeAsync(() => ViewModel.NotifyCatalogChange(e)),
    _cancellationTokenSource.Token);
await _catalogTask.ConfigureAwait(true);
```

---

## Value Converters

All converters live in `Converters/` and follow this pattern:

| Converter             | Input → Output                                     |
| --------------------- | -------------------------------------------------- |
| `FileNameConverter`   | `string` → escapes `_` for display                 |
| `FileSizeConverter`   | `long` (bytes) → formatted `KB`/`MB`/`GB` string   |
| `PixelSizeConverter`  | `Asset` → `"W×H pixels"` string                    |
| `TernaryConverter`    | `(bool, object)` multi-binding → conditional value |
| `VisibilityConverter` | type name → `true` or `false` (IsVisible)          |

- `ConvertBack` always `throw new NotImplementedException()` for read-only converters
- Register as resources in the AXAML that uses them

---

## Custom Events (in `Models/`)

```csharp
public delegate void FolderAddedEventHandler(object sender, FolderAddedEventArgs e);

public class FolderAddedEventArgs : EventArgs
{
    public required Folder Folder { get; init; }
}
```

Pattern: `{Action}EventHandler` delegate + `{Action}EventArgs` class.

---

## AXAML Patterns

- **Theme**: Avalonia Fluent theme with configurable System/Light/Dark mode
- **Global styles** in `App.axaml` (`Application.Styles`)
- **Performance**: `VirtualizingStackPanel` in `ItemsControl` for large photo lists
- **DataGrid**: `Avalonia.Controls.DataGrid` for tabular data
- **No resource dictionaries** beyond App.axaml for global resources
- **Image rendering**: Use `Stretch="Uniform"` or `Stretch="UniformToFill"` on `Image` controls

---

## Cross-Platform Image Handling

The image pipeline uses **SkiaSharp** for cross-platform image operations (replacing WPF's BitmapImage):

- `BitmapHelper.cs` uses SkiaSharp (`SKBitmap`, `SKImage`, `SKCodec`) for image loading and rotation
- `ExifHelper.cs` uses SkiaSharp for EXIF orientation reading
- Image data is passed as `byte[]` between layers, decoded to `Avalonia.Media.Imaging.Bitmap` only in the UI layer
- The `Rotation` enum is a custom domain enum (not WPF's `System.Windows.Media.Imaging.Rotation`)

---

## Naming Conventions

| Element           | Pattern                     | Example                      |
| ----------------- | --------------------------- | ---------------------------- |
| Window            | `{Feature}Window`           | `FindDuplicatedAssetsWindow` |
| UserControl       | `{Feature}UserControl`      | `ThumbnailsUserControl`      |
| ViewModel         | `{Feature}ViewModel`        | `ApplicationViewModel`       |
| Process ViewModel | `{Feature}ProcessViewModel` | `SyncAssetsViewModel`        |
| Converter         | `{Feature}Converter`        | `FileSizeConverter`          |
| Event delegate    | `{Action}EventHandler`      | `FolderAddedEventHandler`    |
| Event args        | `{Action}EventArgs`         | `FolderAddedEventArgs`       |
