using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PhotoManager.Application;
using PhotoManager.UI.Controls;
using PhotoManager.UI.Windows;

namespace PhotoManager.Tests.Integration.UI.Windows.MainWindw;

[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public class MainWindowStartupTests
{
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
    public async Task Constructor_AvaloniaControlsLoaded_BindsFolderNavigationViewModel()
    {
        const string initialFolderPath = @"C:\Photos";
        IApplication application = CreateApplication(initialFolderPath);

        ApplicationViewModel applicationViewModel = new(application);
        MainWindow? window = null;

        await AvaloniaTestSetup.RunOnUiThreadAsync(() =>
        {
            try
            {
                Assert.DoesNotThrow(() =>
                    window = new MainWindow(applicationViewModel, application, new SingleLoggerFactory(_testLogger)));

                FolderNavigationControl? folderNavigationControl =
                    window!.FindControl<FolderNavigationControl>("FolderTreeView");

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(folderNavigationControl, Is.Not.Null);
                    Assert.That(folderNavigationControl!.DataContext, Is.TypeOf<FolderNavigationViewModel>());
                    Assert.That(folderNavigationControl.SelectedPath, Is.EqualTo(initialFolderPath));
                }

                _testLogger.AssertLogExceptions([], typeof(MainWindow));
            }
            finally
            {
                window?.Close();
            }
        });
    }

    [Test]
    public async Task ThumbnailsSelection_SelectedAsset_UpdatesViewerPositionAndSelectedAssets()
    {
        const string initialFolderPath = @"C:\Photos";
        IApplication application = CreateApplication(initialFolderPath);
        ApplicationViewModel applicationViewModel = new(application);
        Asset firstAsset = CreateAsset(initialFolderPath, "first.jpg");
        Asset secondAsset = CreateAsset(initialFolderPath, "second.jpg");
        applicationViewModel.SetAssets(initialFolderPath, [firstAsset, secondAsset]);

        await AvaloniaTestSetup.RunOnUiThreadAsync(() =>
        {
            ThumbnailsUserControl control = new()
            {
                DataContext = applicationViewModel
            };

            try
            {
                ListBox listBox = control.FindControl<ListBox>("ThumbnailsListBox")
                    ?? throw new InvalidOperationException("ThumbnailsListBox was not found.");

                listBox.SelectedIndex = 1;

                Asset[] expectedAssets = [secondAsset];

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(applicationViewModel.ViewerPosition, Is.EqualTo(1));
                    Assert.That(applicationViewModel.CurrentAsset, Is.SameAs(secondAsset));
                    Assert.That(applicationViewModel.SelectedAssets, Is.EqualTo(expectedAssets));
                }
            }
            finally
            {
                control.DataContext = null;
            }
        });
    }

    [Test]
    public async Task SyncAssetsAddDefinitionButton_Click_AddsBlankDefinition()
    {
        IApplication application = CreateApplication(@"C:\Photos");
        SyncAssetsViewModel syncAssetsViewModel = new(application);
        SyncAssetsWindow? window = null;

        await AvaloniaTestSetup.RunOnUiThreadAsync(() =>
        {
            try
            {
                window = new SyncAssetsWindow(syncAssetsViewModel, NullLogger<SyncAssetsWindow>.Instance);
                Button addDefinitionButton = window.FindControl<Button>("AddDefinitionButton")
                    ?? throw new InvalidOperationException("AddDefinitionButton was not found.");

                addDefinitionButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(syncAssetsViewModel.Definitions, Has.Count.EqualTo(1));
                    Assert.That(syncAssetsViewModel.Definitions[0].SourceDirectory, Is.EqualTo(string.Empty));
                    Assert.That(syncAssetsViewModel.Definitions[0].DestinationDirectory, Is.EqualTo(string.Empty));
                }
            }
            finally
            {
                window?.Close();
            }
        });
    }

    [Test]
    public async Task Constructor_AboutInformation_BindsTitleAndProductText()
    {
        AboutInformation aboutInformation = new()
        {
            Product = "PhotoManager",
            Author = "Toto",
            Version = Constants.VERSION
        };
        AboutWindow? window = null;

        await AvaloniaTestSetup.RunOnUiThreadAsync(() =>
        {
            try
            {
                window = new AboutWindow(aboutInformation, NullLogger<AboutWindow>.Instance);
                TextBlock productTextBlock = window.FindControl<TextBlock>("ProductTextBlock")
                    ?? throw new InvalidOperationException("ProductTextBlock was not found.");

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(window.Title, Is.EqualTo($"About PhotoManager {Constants.VERSION}"));
                    Assert.That(productTextBlock.Text, Is.EqualTo("PhotoManager"));
                    Assert.That(window.DataContext, Is.EqualTo(aboutInformation));
                }
            }
            finally
            {
                window?.Close();
            }
        });
    }

    [Test]
    public async Task Constructor_MessageAndTitle_BindsMessageText()
    {
        MessageDialogWindow? window = null;

        await AvaloniaTestSetup.RunOnUiThreadAsync(() =>
        {
            try
            {
                window = new("Hello", "Information");
                TextBlock messageTextBlock = window.FindControl<TextBlock>("MessageTextBlock")
                    ?? throw new InvalidOperationException("MessageTextBlock was not found.");

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(window.Title, Is.EqualTo("Information"));
                    Assert.That(messageTextBlock.Text, Is.EqualTo("Hello"));
                }
            }
            finally
            {
                window?.Close();
            }
        });
    }

    [Test]
    public async Task ShowImage_CurrentAsset_LoadsForegroundAndBackgroundImages()
    {
        const string initialFolderPath = @"C:\Photos";
        IApplication application = CreateApplication(initialFolderPath);
        Asset asset = CreateAsset(initialFolderPath, "first.jpg");
        application.LoadBitmapImageFromPath(asset.FullPath, asset.ImageRotation).Returns(SkiaImageData.Empty());
        ApplicationViewModel applicationViewModel = new(application);
        applicationViewModel.SetAssets(initialFolderPath, [asset]);

        await AvaloniaTestSetup.RunOnUiThreadAsync(() =>
        {
            ViewerUserControl control = new()
            {
                DataContext = applicationViewModel
            };

            try
            {
                control.ShowImage();
                Image image = control.FindControl<Image>("Image")
                    ?? throw new InvalidOperationException("Image was not found.");
                Image backgroundImage = control.FindControl<Image>("BackgroundImage")
                    ?? throw new InvalidOperationException("BackgroundImage was not found.");

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(image.Source, Is.Not.Null);
                    Assert.That(backgroundImage.Source, Is.SameAs(image.Source));
                }
            }
            finally
            {
                control.DataContext = null;
            }
        });
    }

    [Test]
    public async Task GoToFolderAsync_SelectedPath_LoadsAssetsAndSelectsFirstAsset()
    {
        const string initialFolderPath = @"C:\Photos";
        const string selectedFolderPath = @"C:\Photos\Child";
        IApplication application = CreateApplication(initialFolderPath);
        Asset firstAsset = CreateAsset(selectedFolderPath, "first.jpg");
        Asset secondAsset = CreateAsset(selectedFolderPath, "second.jpg");
        application.GetAssetsByPath(selectedFolderPath).Returns([firstAsset, secondAsset]);
        ApplicationViewModel applicationViewModel = new(application);

        await AvaloniaTestSetup.RunOnUiThreadAsync(async () =>
        {
            ThumbnailsUserControl control = new()
            {
                DataContext = applicationViewModel
            };

            try
            {
                await control.GoToFolderAsync(application, selectedFolderPath);
                ListBox listBox = control.FindControl<ListBox>("ThumbnailsListBox")
                    ?? throw new InvalidOperationException("ThumbnailsListBox was not found.");
                Asset[] expectedAssets = [firstAsset, secondAsset];

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(applicationViewModel.ObservableAssets, Is.EqualTo(expectedAssets));
                    Assert.That(applicationViewModel.CurrentFolderPath, Is.EqualTo(selectedFolderPath));
                    Assert.That(applicationViewModel.ViewerPosition, Is.Zero);
                    Assert.That(listBox.ItemsSource, Is.SameAs(applicationViewModel.ObservableAssets));
                }
            }
            finally
            {
                control.DataContext = null;
            }
        });
    }

    [Test]
    public async Task NextButton_Click_CurrentAsset_ShowsNextImage()
    {
        const string initialFolderPath = @"C:\Photos";
        IApplication application = CreateApplication(initialFolderPath);
        Asset firstAsset = CreateAsset(initialFolderPath, "first.jpg");
        Asset secondAsset = CreateAsset(initialFolderPath, "second.jpg");
        application.LoadBitmapImageFromPath(firstAsset.FullPath, firstAsset.ImageRotation)
            .Returns(SkiaImageData.Empty());
        application.LoadBitmapImageFromPath(secondAsset.FullPath, secondAsset.ImageRotation)
            .Returns(SkiaImageData.Empty());
        ApplicationViewModel applicationViewModel = new(application);
        applicationViewModel.SetAssets(initialFolderPath, [firstAsset, secondAsset]);

        await AvaloniaTestSetup.RunOnUiThreadAsync(() =>
        {
            ViewerUserControl control = new()
            {
                DataContext = applicationViewModel
            };

            try
            {
                Button nextButton = control.FindControl<Button>("NextButton")
                    ?? throw new InvalidOperationException("NextButton was not found.");

                nextButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(applicationViewModel.ViewerPosition, Is.EqualTo(1));
                    Assert.That(applicationViewModel.CurrentAsset, Is.SameAs(secondAsset));
                    Assert.That(control.FindControl<Image>("Image")?.Source, Is.Not.Null);
                }
            }
            finally
            {
                control.DataContext = null;
            }
        });
    }

    [Test]
    public async Task Initialize_RootAndChildFolders_SelectsRequestedPath()
    {
        const string initialFolderPath = @"C:\Photos";
        IApplication application = CreateApplication(initialFolderPath);
        Folder rootFolder = new() { Id = Guid.NewGuid(), Path = initialFolderPath };
        Folder childFolder = new() { Id = Guid.NewGuid(), Path = @"C:\Photos\Child" };
        application.GetRootCatalogFolders().Returns([rootFolder]);
        application.GetSubFolders(Arg.Any<Folder>()).Returns([]);
        application.GetSubFolders(rootFolder).Returns([childFolder]);
        ApplicationViewModel applicationViewModel = new(application);
        FolderNavigationViewModel folderNavigationViewModel = new(applicationViewModel, rootFolder, []);

        await AvaloniaTestSetup.RunOnUiThreadAsync(() =>
        {
            FolderNavigationControl control = new()
            {
                DataContext = folderNavigationViewModel,
                SelectedPath = childFolder.Path
            };

            try
            {
                control.Initialize();
                TreeView treeView = control.FindControl<TreeView>("FoldersTreeView")
                    ?? throw new InvalidOperationException("FoldersTreeView was not found.");
                TreeViewItem rootItem = (TreeViewItem)treeView.Items[0]!;
                TreeViewItem childItem = (TreeViewItem)rootItem.Items[0]!;

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(rootItem.Tag, Is.SameAs(rootFolder));
                    Assert.That(rootItem.IsExpanded, Is.True);
                    Assert.That(childItem.Tag, Is.SameAs(childFolder));
                    Assert.That(childItem.IsSelected, Is.True);
                    Assert.That(control.SelectedPath, Is.EqualTo(childFolder.Path));
                }
            }
            finally
            {
                control.DataContext = null;
            }
        });
    }

    [Test]
    public async Task Constructor_SourceFolder_BindsControlAndSelectedPath()
    {
        const string initialFolderPath = @"C:\Photos";
        IApplication application = CreateApplication(initialFolderPath);
        Folder sourceFolder = new() { Id = Guid.NewGuid(), Path = initialFolderPath };
        application.GetRootCatalogFolders().Returns([sourceFolder]);
        ApplicationViewModel applicationViewModel = new(application);
        FolderNavigationViewModel folderNavigationViewModel = new(applicationViewModel, sourceFolder, []);
        FolderNavigationWindow? window = null;

        await AvaloniaTestSetup.RunOnUiThreadAsync(() =>
        {
            try
            {
                window = new(folderNavigationViewModel, NullLogger<FolderNavigationWindow>.Instance);
                FolderNavigationControl folderNavigationControl =
                    window.FindControl<FolderNavigationControl>("FolderTreeView")
                    ?? throw new InvalidOperationException("FolderTreeView was not found.");

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(window.DataContext, Is.SameAs(folderNavigationViewModel));
                    Assert.That(folderNavigationControl.DataContext, Is.SameAs(folderNavigationViewModel));
                    Assert.That(folderNavigationControl.SelectedPath, Is.EqualTo(sourceFolder.Path));
                }
            }
            finally
            {
                window?.Close();
            }
        });
    }

    private static IApplication CreateApplication(string initialFolderPath)
    {
        IApplication application = Substitute.For<IApplication>();
        application.GetInitialFolderPath().Returns(initialFolderPath);
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

    private static Asset CreateAsset(string folderPath, string fileName)
    {
        return AssetBuilder.Create()
            .WithFolderPath(folderPath, Guid.NewGuid())
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
