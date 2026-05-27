using Avalonia;
using Avalonia.Controls;
using System.Diagnostics.CodeAnalysis;

namespace PhotoManager.UI.Controls;

[ExcludeFromCodeCoverage]
public class DisposableImage : Image
{
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property != SourceProperty)
        {
            return;
        }

        if (!ReferenceEquals(change.OldValue, change.NewValue))
        {
            DisposeImageSource(change.OldValue);
        }
    }

    private static void DisposeImageSource(object? source)
    {
        if (source is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
