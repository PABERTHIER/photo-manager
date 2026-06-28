namespace PhotoManager.UI.Controls;

[ExcludeFromCodeCoverage]
public class DisposableImage : Image
{
    // The source is disposed on detach. When virtualization recycles this control, it is detached (disposing the current source)
    // then reattached, and the binding pushes a new source whose OldValue is that already-disposed instance.
    // Tracking it here prevents the swap below from disposing the same bitmap a second time.
    private object? _detachedSource;

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property != SourceProperty)
        {
            return;
        }

        if (!ReferenceEquals(change.OldValue, change.NewValue)
            && !ReferenceEquals(change.OldValue, _detachedSource))
        {
            DisposeImageSource(change.OldValue);
        }

        _detachedSource = null;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        // Swaps dispose the previous source; this frees the last one when the control leaves the visual tree
        // (window closed, ListBox virtualization teardown), which would otherwise linger until the GC finalizer reclaims its native memory.
        _detachedSource = Source;
        DisposeImageSource(Source);

        base.OnDetachedFromVisualTree(e);
    }

    private static void DisposeImageSource(object? source)
    {
        if (source is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
