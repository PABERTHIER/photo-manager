---
description: "Guide for Avalonia UI work in PhotoManager: UI conventions and MVVM patterns"
---

# Avalonia UI Reference Guide for PhotoManager

Answer the following about Avalonia UI in this project: ${input:question:What do you want to know or do with Avalonia UI in this project?}

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
Converters/   — {Feature}Converter : IValueConverter
Models/       — {Action}EventHandler delegates + {Action}EventArgs classes
Services/     — UI-layer services (single-instance, theme)
Configuration/ — UI settings classes
```

### DI Wiring

- `App.axaml.cs` builds the DI container, exposes `static IServiceProvider? App.ServiceProvider`
- Singletons in DI: `MainWindow`, `ApplicationViewModel` (registered in `UiServiceCollectionExtensions`)
- AXAML-instantiated UserControls resolve services via `App.ServiceProvider?.GetService<T>()`
- Dialog windows + their VMs are created on-demand in code-behind (not via DI)

### Async in Avalonia

- Always use `Dispatcher.UIThread.InvokeAsync()` to marshal domain callbacks to the UI thread
- Always use `.ConfigureAwait(true)` in UI async methods (preserves synchronization context)

### Converters

- `ConvertBack` must `throw new NotImplementedException()` for read-only converters
- Register as resources in the consuming AXAML file

### Styles

- Avalonia Fluent theme with System/Light/Dark mode support
- Global styles live in `App.axaml` Application.Styles
- Use Avalonia `Styles` and `ControlTheme` for custom styling

### Visibility Pattern

Prefer computed `bool` properties for `IsVisible` bindings (not WPF's Visibility enum):

```csharp
public bool IsThumbnailsVisible => AppMode == AppMode.Thumbnails;
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
