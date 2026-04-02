---
name: wpf
description: >
  WPF reference guide for PhotoManager: UI conventions, MVVM patterns, and cross-layer WPF type usage.
  Use this skill when working on UI code, adding image-processing methods, or needing to understand
  where WPF types appear across the codebase.
---

You are an expert on the WPF usage in PhotoManager. Answer or implement the following: ${input:question:What WPF topic or task can I help you with?}

Use the facts below as your authoritative reference.

---

## UI Project Structure

```
PhotoManager.UI/
├── App.xaml / App.xaml.cs        — DI container, Serilog setup, single-instance Mutex
├── Windows/                       — {Feature}Window  (ShowDialog = modal, Show = modeless)
│   ├── MainWindow.xaml/.cs        — Main shell, async catalog background work
│   ├── FindDuplicatedAssetsWindow — Duplicate detection workflow
│   ├── SyncAssetsWindow           — Sync workflow
│   ├── FolderNavigationWindow     — Folder picker
│   └── AboutWindow                — About dialog
├── Controls/                      — {Feature}UserControl  (reusable composed controls)
│   ├── ViewerUserControl          — Full-screen image viewer
│   ├── ThumbnailsUserControl      — Virtualized thumbnail list
│   └── FolderNavigationControl    — TreeView folder browser
├── ViewModels/                    — {Feature}ViewModel, base classes, Enums/
├── Converters/                    — {Feature}Converter
└── Models/                        — {Action}EventHandler delegates + {Action}EventArgs classes
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

### Visibility — Prefer Computed Properties over Converters
```csharp
public Visibility ThumbnailsVisible => AppMode == AppMode.Thumbnails ? Visibility.Visible : Visibility.Hidden;
```

### Collections
- `ObservableCollection<T>` — dynamic UI lists
- `SortableObservableCollection<T>` (project type in `ViewModels/`) — raises `Reset` on Sort()

---

## Dependency Injection

- DI chain in `App.xaml.cs`: `AddInfrastructure()` → `AddDomain()` → `AddApplication()` → `AddUi()`
- `App.ServiceProvider` (static) is the escape hatch for XAML-instantiated UserControls:
  ```csharp
  _logger = App.ServiceProvider?.GetService<ILogger<ThumbnailsUserControl>>()
            ?? NullLogger<ThumbnailsUserControl>.Instance;
  ```
- Singletons registered in `UiServiceCollectionExtensions`: `MainWindow`, `ApplicationViewModel`
- Dialog windows and their ViewModels are **created on-demand** in code-behind (not via DI):
  ```csharp
  FindDuplicatedAssetsViewModel vm = new(_application);
  FindDuplicatedAssetsWindow window = new(vm, _loggerFactory.CreateLogger<FindDuplicatedAssetsWindow>());
  window.ShowDialog();
  ```

---

## Async & Thread Safety

- Background work launched in `Window_Loaded` via `StartBackgroundWorkAsync()`
- Always use `Dispatcher.InvokeAsync()` to marshal domain callbacks back to the UI thread
- Always use `.ConfigureAwait(true)` in WPF async methods (preserves WPF synchronization context)
```csharp
_catalogTask = ViewModel.CatalogAssets(
    e => Dispatcher.InvokeAsync(() => ViewModel.NotifyCatalogChange(e)),
    _cancellationTokenSource.Token);
await _catalogTask.ConfigureAwait(true);
```

---

## Value Converters

All 5 converters live in `Converters/` and follow this pattern:

| Converter | Input → Output |
|-----------|---------------|
| `FileNameConverter` | `string` → escapes `_` for XAML label display |
| `FileSizeConverter` | `long` (bytes) → formatted `KB`/`MB`/`GB` string |
| `PixelSizeConverter` | `Asset` → `"W×H pixels"` string |
| `TernaryConverter` | `(bool, object)` multi-binding → conditional value |
| `VisibilityConverter` | type name → `Visibility.Visible` or `.Hidden` |

- `ConvertBack` always `throw new NotImplementedException()` for read-only converters
- Register as `StaticResource` in the XAML that uses them

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

## XAML Patterns

- **Global styles** in `App.xaml` (`Application.Resources`) — dark theme (`#FF3C3C3C` / White)
- **Hover effects** via `<Style.Triggers><Trigger Property="IsMouseOver" Value="True">`
- **Effects**: `BlurEffect` (Radius=100) and `DropShadowEffect` used in ViewerUserControl
- **Performance**: `VirtualizingStackPanel` in ThumbnailsUserControl for large photo lists
- **Render quality**: `RenderOptions.BitmapScalingMode="HighQuality"` on Image controls
- **No resource dictionaries** beyond App.xaml; no animation storyboards; no attached behaviors

---

## WPF Types Used Across ALL Layers

`System.Windows.Media.Imaging` is **globally imported** via `GlobalUsings.cs` in:
`PhotoManager.Common`, `PhotoManager.Domain`, `PhotoManager.Infrastructure`, `PhotoManager.Tests`

This is **by design**. Do NOT replace these with custom abstractions unless performing an explicit architectural refactor.

### BitmapImage
Primary image carrier throughout the system:

| Location | Usage |
|----------|-------|
| `Common/BitmapHelper.cs` | Created by all `Load*` methods (10 methods); set `.Rotation`, `.CacheOption`, `.CreateOptions`, `.StreamSource` |
| `Common/BitmapHelper.cs` | Parameter in `GetJpegBitmapImage()`, `GetPngBitmapImage()`, `GetGifBitmapImage()`, `GetBitmapImage()` |
| `Application/IApplication.cs` | Return type of `LoadBitmapImageFromPath()`, `LoadBitmapHeicImageFromPath()` |
| `Application/Application.cs` | Return type (delegates to service) |
| `UI/ViewModels/ApplicationViewModel.cs` | Calls the two IApplication methods above |
| `UI/Controls/ViewerUserControl.xaml.cs` | Bound to `Image.Source` for display |

### Rotation (`System.Windows.Media.Imaging.Rotation`)
WPF enum with values `Rotate0`, `Rotate90`, `Rotate180`, `Rotate270`:

| Location | Usage |
|----------|-------|
| `Common/BitmapHelper.cs` | Parameter in all 6 `Load*` methods; assigned to `BitmapImage.Rotation`; switch in `MagickImageApplyRotation()` |
| `Common/ExifHelper.cs` | Return type of `GetImageRotation()` — reads EXIF data and maps to `Rotation.*` |
| `Application/IApplication.cs` | Parameter in `LoadBitmapImageFromPath()`, `LoadBitmapHeicImageFromPath()` |
| `Application/Application.cs` | Parameter (passed through to service) |

### BitmapFrame
| Location | Usage |
|----------|-------|
| `Common/ExifHelper.cs` | `BitmapFrame.Create(stream)` to access `BitmapMetadata` for EXIF orientation |
| `Common/BitmapHelper.cs` | `BitmapFrame.Create(image)` before encoding |

### BitmapMetadata
| Location | Usage |
|----------|-------|
| `Common/ExifHelper.cs` | Cast from `BitmapFrame.Metadata`; `GetQuery("/app1/ifd/exif:{uint=274}")` for EXIF orientation tag |

### BitmapEncoder (+ JpegBitmapEncoder, PngBitmapEncoder, GifBitmapEncoder)
| Location | Usage |
|----------|-------|
| `Common/BitmapHelper.cs` | Base type parameter in `GetBitmapImage()`; instantiated in format-specific overloads |

### BitmapCacheOption & BitmapCreateOptions
| Location | Usage |
|----------|-------|
| `Common/BitmapHelper.cs` | `BitmapCacheOption.OnLoad` and `BitmapCreateOptions.IgnoreColorProfile` set on every `BitmapImage` |

### System.Windows.Visibility
| Location | Usage |
|----------|-------|
| `UI/ViewModels/ApplicationViewModel.cs` | `ThumbnailsVisible`, `ViewerVisible` computed properties |
| `UI/ViewModels/DuplicatedAssetViewModel.cs` | `Visible` property for XAML binding |
| `UI/Converters/VisibilityConverter.cs` | Returns `Visibility.Hidden` / `Visibility.Visible` |

### System.Windows.FrameworkElement
| Location | Usage |
|----------|-------|
| `UI/Windows/FindDuplicatedAssetsWindow.xaml.cs` | `(FrameworkElement)e.Source` cast to read `DataContext` in mouse handlers |

---

## Naming Conventions

| Element | Pattern | Example |
|---------|---------|---------|
| Window | `{Feature}Window` | `FindDuplicatedAssetsWindow` |
| UserControl | `{Feature}UserControl` | `ThumbnailsUserControl` |
| ViewModel | `{Feature}ViewModel` | `ApplicationViewModel` |
| Process ViewModel | `{Feature}ProcessViewModel` | `SyncAssetsViewModel` |
| Converter | `{Feature}Converter` | `FileSizeConverter` |
| Event delegate | `{Action}EventHandler` | `FolderAddedEventHandler` |
| Event args | `{Action}EventArgs` | `FolderAddedEventArgs` |
