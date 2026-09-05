using Avalonia.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PhotoManager.Application;
using PhotoManager.UI.ViewModels.Enums;
using PhotoManager.UI.Windows;

namespace PhotoManager.Tests.Integration.UI.Windows.MainWindw;

// Covers the Window_KeyDown dispatch (key + modifier mapping). The underlying actions are covered by the other
// MainWindow fixtures; here only the keyboard routing is exercised, through the real handler.
[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public class MainWindowKeyDownTests
{
    private const string InitialFolderPath = @"C:\Photos";

    private TestLogger<MainWindow> _testLogger = new();

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        AvaloniaTestSetup.EnsureInitialized();
    }

    [SetUp]
    public void SetUp()
    {
        _testLogger = new();
    }

    [TearDown]
    public void TearDown()
    {
        _testLogger.LoggingAssertTearDown();
    }

    [Test]
    public Task WindowKeyDown_F1_TogglesAppMode()
    {
        IApplication application = CreateApplication();
        ApplicationViewModel applicationViewModel = new(application);

        return AvaloniaTestSetup.RunOnUiThreadAsync(() =>
        {
            MainWindow? window = null;

            try
            {
                window = new(applicationViewModel, application, new SingleLoggerFactory(_testLogger));

                Assert.That(applicationViewModel.AppMode, Is.EqualTo(AppMode.Thumbnails));

                RaiseKeyDown(window, Key.F1);
                Assert.That(applicationViewModel.AppMode, Is.EqualTo(AppMode.Viewer));

                RaiseKeyDown(window, Key.F1);
                Assert.That(applicationViewModel.AppMode, Is.EqualTo(AppMode.Thumbnails));

                _testLogger.AssertLogExceptions([], typeof(MainWindow));
            }
            finally
            {
                window?.Close();
            }
        });
    }

    [Test]
    [TestCase(Key.Right)]
    [TestCase(Key.PageDown)]
    public Task WindowKeyDown_NextKey_AdvancesViewerPosition(Key key)
    {
        IApplication application = CreateApplication();
        ApplicationViewModel applicationViewModel = new(application);

        applicationViewModel.SetAssets(InitialFolderPath, [CreateAsset("first.jpg"), CreateAsset("second.jpg")]);

        return AvaloniaTestSetup.RunOnUiThreadAsync(() =>
        {
            MainWindow? window = null;

            try
            {
                window = new(applicationViewModel, application, new SingleLoggerFactory(_testLogger));

                applicationViewModel.SetViewerPosition(0);

                RaiseKeyDown(window, key);

                Assert.That(applicationViewModel.ViewerPosition, Is.EqualTo(1));

                _testLogger.AssertLogExceptions([], typeof(MainWindow));
            }
            finally
            {
                window?.Close();
            }
        });
    }

    [Test]
    [TestCase(Key.Left)]
    [TestCase(Key.PageUp)]
    public Task WindowKeyDown_PreviousKey_DecrementsViewerPosition(Key key)
    {
        IApplication application = CreateApplication();
        ApplicationViewModel applicationViewModel = new(application);

        applicationViewModel.SetAssets(InitialFolderPath, [CreateAsset("first.jpg"), CreateAsset("second.jpg")]);

        return AvaloniaTestSetup.RunOnUiThreadAsync(() =>
        {
            MainWindow? window = null;

            try
            {
                window = new(applicationViewModel, application, new SingleLoggerFactory(_testLogger));

                applicationViewModel.SetViewerPosition(1);

                RaiseKeyDown(window, key);

                Assert.That(applicationViewModel.ViewerPosition, Is.Zero);

                _testLogger.AssertLogExceptions([], typeof(MainWindow));
            }
            finally
            {
                window?.Close();
            }
        });
    }

    [Test]
    public Task WindowKeyDown_Delete_DeletesSelectedAssets()
    {
        IApplication application = CreateApplication();
        ApplicationViewModel applicationViewModel = new(application);

        Asset firstAsset = CreateAsset("first.jpg");
        Asset secondAsset = CreateAsset("second.jpg");

        applicationViewModel.SetAssets(InitialFolderPath, [firstAsset, secondAsset]);

        return AvaloniaTestSetup.RunOnUiThreadAsync(() =>
        {
            MainWindow? window = null;

            try
            {
                window = new(applicationViewModel, application, new SingleLoggerFactory(_testLogger));

                applicationViewModel.SetSelectedAssets([firstAsset]);

                RaiseKeyDown(window, Key.Delete);

                Assert.That(applicationViewModel.ObservableAssets, Has.Count.EqualTo(1));
                Assert.That(applicationViewModel.ObservableAssets, Does.Not.Contain(firstAsset));
                Assert.That(applicationViewModel.ObservableAssets, Does.Contain(secondAsset));

                application.Received(1).DeleteAssets(Arg.Is<Asset[]>(a => a.Length == 1 && a[0] == firstAsset));

                _testLogger.AssertLogExceptions([], typeof(MainWindow));
            }
            finally
            {
                window?.Close();
            }
        });
    }

    [Test]
    public Task WindowKeyDown_ControlDelete_DoesNotDelete()
    {
        IApplication application = CreateApplication();
        ApplicationViewModel applicationViewModel = new(application);

        Asset firstAsset = CreateAsset("first.jpg");
        Asset secondAsset = CreateAsset("second.jpg");

        applicationViewModel.SetAssets(InitialFolderPath, [firstAsset, secondAsset]);

        return AvaloniaTestSetup.RunOnUiThreadAsync(() =>
        {
            MainWindow? window = null;

            try
            {
                window = new(applicationViewModel, application, new SingleLoggerFactory(_testLogger));

                applicationViewModel.SetSelectedAssets([firstAsset]);

                // With the command modifier held, the handler only maps C/M; Delete must be ignored.
                RaiseKeyDown(window, Key.Delete, KeyModifiers.Control);

                Assert.That(applicationViewModel.ObservableAssets, Has.Count.EqualTo(2));
                Assert.That(applicationViewModel.ObservableAssets[0], Is.EqualTo(firstAsset));
                Assert.That(applicationViewModel.ObservableAssets[1], Is.EqualTo(secondAsset));

                application.DidNotReceive().DeleteAssets(Arg.Any<Asset[]>());

                _testLogger.AssertLogExceptions([], typeof(MainWindow));
            }
            finally
            {
                window?.Close();
            }
        });
    }

    [Test]
    [TestCase(Key.C)]
    [TestCase(Key.M)]
    public Task WindowKeyDown_ControlCopyOrMoveWithoutSelection_DoesNothing(Key key)
    {
        IApplication application = CreateApplication();
        ApplicationViewModel applicationViewModel = new(application);

        return AvaloniaTestSetup.RunOnUiThreadAsync(() =>
        {
            MainWindow? window = null;

            try
            {
                window = new(applicationViewModel, application, new SingleLoggerFactory(_testLogger));

                // Routes to MoveAssets, which returns early when no asset is selected (no move window opened).
                RaiseKeyDown(window, key, KeyModifiers.Control);

                Assert.That(applicationViewModel.ObservableAssets, Is.Empty);

                application.DidNotReceive().MoveAssets(Arg.Any<Asset[]>(), Arg.Any<Folder>(), Arg.Any<bool>());

                _testLogger.AssertLogExceptions([], typeof(MainWindow));
            }
            finally
            {
                window?.Close();
            }
        });
    }

    [Test]
    public Task WindowKeyDown_UnhandledKey_DoesNothing()
    {
        IApplication application = CreateApplication();
        ApplicationViewModel applicationViewModel = new(application);

        applicationViewModel.SetAssets(InitialFolderPath, [CreateAsset("first.jpg"), CreateAsset("second.jpg")]);

        return AvaloniaTestSetup.RunOnUiThreadAsync(() =>
        {
            MainWindow? window = null;

            try
            {
                window = new(applicationViewModel, application, new SingleLoggerFactory(_testLogger));

                applicationViewModel.SetViewerPosition(0);

                RaiseKeyDown(window, Key.A);

                Assert.That(applicationViewModel.ViewerPosition, Is.Zero);
                Assert.That(applicationViewModel.AppMode, Is.EqualTo(AppMode.Thumbnails));

                _testLogger.AssertLogExceptions([], typeof(MainWindow));
            }
            finally
            {
                window?.Close();
            }
        });
    }

    private static void RaiseKeyDown(MainWindow window, Key key, KeyModifiers modifiers = KeyModifiers.None)
    {
        KeyEventArgs args = new() { Key = key, KeyModifiers = modifiers };
        AvaloniaTestSetup.InvokeNonPublicInstanceMethod(window, "Window_KeyDown", window, args);
    }

    private static IApplication CreateApplication()
    {
        IApplication application = Substitute.For<IApplication>();

        application.GetInitialFolderPath().Returns(InitialFolderPath);
        application.GetAboutInformation(Arg.Any<System.Reflection.Assembly>()).Returns(new AboutInformation
        {
            Product = "PhotoManager",
            Version = Constants.VERSION
        });
        application.GetRecentTargetPaths().Returns([]);
        application.GetRootCatalogFolders().Returns([]);
        application.GetSyncAssetsConfiguration().Returns(new SyncAssetsConfiguration());

        return application;
    }

    private static Asset CreateAsset(string fileName)
    {
        return AssetBuilder.Create()
            .WithFolderPath(InitialFolderPath, Guid.NewGuid())
            .WithFileName(fileName)
            .WithPixels(100, 80, 100, 80)
            .WithFileProperties(1024, new(2026, 1, 1), new(2026, 1, 1))
            .WithThumbnailCreationDateTime(new(2026, 1, 1))
            .WithRotation(ImageRotation.Rotate0)
            .WithHash(fileName)
            .WithImageData(SkiaImageData.Empty())
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
    }

    private sealed class SingleLoggerFactory(ILogger<MainWindow> logger) : ILoggerFactory
    {
        public ILogger CreateLogger(string categoryName)
        {
            return categoryName == typeof(MainWindow).FullName ? logger : NullLogger.Instance;
        }

        public void AddProvider(ILoggerProvider provider)
        {
        }

        public void Dispose()
        {
        }
    }
}
