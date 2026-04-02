---
description: "Guide for WPF work in PhotoManager: UI conventions and cross-layer WPF types"
---

# WPF Reference Guide for PhotoManager

Answer the following about WPF in this project: ${input:question:What do you want to know or do with WPF in this project?}

---

## Key Facts to Apply

### MVVM Structure

- All ViewModels inherit `BaseViewModel` → implements `INotifyPropertyChanged`
- Multi-step process VMs inherit `BaseProcessViewModel<TC, TR>`
- Use the C# 11 `field` keyword in property setters with `NotifyPropertyChanged`
- `SortableObservableCollection<T>` (in ViewModels/) is the project type for sortable lists

### Project Structure

```
Windows/      — {Feature}Window (modal: ShowDialog, modeless: Show)
Controls/     — {Feature}UserControl (reusable composed controls)
ViewModels/   — {Feature}ViewModel, base classes, Enums/
Converters/   — {Feature}Converter : IValueConverter or IMultiValueConverter
Models/       — {Action}EventHandler delegates + {Action}EventArgs classes
```

### DI Wiring

- `App.xaml.cs` builds the DI container, exposes `static IServiceProvider? App.ServiceProvider`
- Singletons in DI: `MainWindow`, `ApplicationViewModel` (registered in `UiServiceCollectionExtensions`)
- XAML-instantiated UserControls resolve services via `App.ServiceProvider?.GetService<T>()`
- Dialog windows + their VMs are created on-demand in code-behind (not via DI)

### Async in WPF

- Always use `Dispatcher.InvokeAsync()` to marshal domain callbacks to the UI thread
- Always use `.ConfigureAwait(true)` in WPF async methods (preserves UI sync context)

### Converters

- `ConvertBack` must `throw new NotImplementedException()` for read-only converters
- Register as `StaticResource` in the consuming XAML resources section

### Styles

- Global styles live in `App.xaml` Application.Resources
- Use `<Style.Triggers>` with `<Trigger Property="IsMouseOver">` for hover effects
- Reference with `Style="{StaticResource StyleName}"`

### Visibility Pattern

Prefer computed properties over converters for visibility:

```csharp
public Visibility ThumbnailsVisible => AppMode == AppMode.Thumbnails ? Visibility.Visible : Visibility.Hidden;
```

### Naming Conventions

| Element        | Pattern                |
| -------------- | ---------------------- |
| Window         | `{Feature}Window`      |
| UserControl    | `{Feature}UserControl` |
| ViewModel      | `{Feature}ViewModel`   |
| Converter      | `{Feature}Converter`   |
| Event delegate | `{Action}EventHandler` |
| Event args     | `{Action}EventArgs`    |
