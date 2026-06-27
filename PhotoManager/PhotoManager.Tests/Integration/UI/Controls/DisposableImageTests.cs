using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using PhotoManager.UI.Controls;

namespace PhotoManager.Tests.Integration.UI.Controls;

[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public class DisposableImageTests
{
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        AvaloniaTestSetup.EnsureInitialized();
    }

    [Test]
    public Task Source_ChangedToDifferentImage_DisposesPreviousSource()
    {
        return AvaloniaTestSetup.RunOnUiThreadAsync(() =>
        {
            DisposableImage image = new();
            IImage firstSource = Substitute.For<IImage, IDisposable>();
            IImage secondSource = Substitute.For<IImage, IDisposable>();

            image.Source = firstSource;
            image.Source = secondSource;

            using (Assert.EnterMultipleScope())
            {
                ((IDisposable)firstSource).Received(1).Dispose();
                ((IDisposable)secondSource).DidNotReceive().Dispose();
            }
        });
    }

    [Test]
    public Task Source_ChangedToSameImage_DoesNotDispose()
    {
        return AvaloniaTestSetup.RunOnUiThreadAsync(() =>
        {
            DisposableImage image = new();
            IImage source = Substitute.For<IImage, IDisposable>();

            image.Source = source;
            image.Source = source;

            ((IDisposable)source).DidNotReceive().Dispose();
        });
    }

    [Test]
    public Task NonSourceProperty_Changed_DoesNotDisposeSource()
    {
        return AvaloniaTestSetup.RunOnUiThreadAsync(() =>
        {
            DisposableImage image = new();
            IImage source = Substitute.For<IImage, IDisposable>();

            image.Source = source;
            image.Width = 123;

            ((IDisposable)source).DidNotReceive().Dispose();
        });
    }

    [Test]
    public Task DetachedFromVisualTree_WithSource_DisposesCurrentSource()
    {
        return AvaloniaTestSetup.RunOnUiThreadAsync(() =>
        {
            Window window = new();
            DisposableImage image = new();
            IImage source = Substitute.For<IImage, IDisposable>();

            try
            {
                window.Content = image;
                window.Show();
                Dispatcher.UIThread.RunJobs();

                image.Source = source;

                window.Content = null;
                Dispatcher.UIThread.RunJobs();

                ((IDisposable)source).Received(1).Dispose();
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Test]
    public Task Source_ReassignedAfterDetach_DoesNotDisposeDetachedSourceTwice()
    {
        return AvaloniaTestSetup.RunOnUiThreadAsync(() =>
        {
            Window window = new();
            DisposableImage image = new();
            IImage detached = Substitute.For<IImage, IDisposable>();
            IImage reattached = Substitute.For<IImage, IDisposable>();

            try
            {
                window.Content = image;
                window.Show();
                Dispatcher.UIThread.RunJobs();

                image.Source = detached;

                window.Content = null; // Detach disposes the current source
                Dispatcher.UIThread.RunJobs();

                window.Content = image; // Reattach mimics virtualization recycling the same control
                Dispatcher.UIThread.RunJobs();

                image.Source = reattached; // Binding pushes a new source whose OldValue is the disposed one

                using (Assert.EnterMultipleScope())
                {
                    ((IDisposable)detached).Received(1).Dispose();
                    ((IDisposable)reattached).DidNotReceive().Dispose();
                }
            }
            finally
            {
                window.Close();
            }
        });
    }
}
