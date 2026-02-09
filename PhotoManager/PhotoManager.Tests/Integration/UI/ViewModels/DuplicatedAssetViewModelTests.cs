using System.ComponentModel;
using System.Windows;
using Directories = PhotoManager.Tests.Integration.Constants.Directories;
using FileNames = PhotoManager.Tests.Integration.Constants.FileNames;
using FileSize = PhotoManager.Tests.Integration.Constants.FileSize;
using Hashes = PhotoManager.Tests.Integration.Constants.Hashes;
using ModificationDate = PhotoManager.Tests.Integration.Constants.ModificationDate;
using PixelHeightAsset = PhotoManager.Tests.Integration.Constants.PixelHeightAsset;
using PixelWidthAsset = PhotoManager.Tests.Integration.Constants.PixelWidthAsset;
using ThumbnailHeightAsset = PhotoManager.Tests.Integration.Constants.ThumbnailHeightAsset;
using ThumbnailWidthAsset = PhotoManager.Tests.Integration.Constants.ThumbnailWidthAsset;

namespace PhotoManager.Tests.Integration.UI.ViewModels;

[TestFixture]
public class DuplicatedAssetViewModelTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;

    private DuplicatedAssetViewModel? _duplicatedAssetViewModel;
    private AssetRepository? _assetRepository;

    private Asset? _asset1;
    private Asset? _asset2;
    private Asset? _asset3;
    private Asset? _asset4;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_dataDirectory, Directories.DATABASE_TESTS);
        _databasePath = Path.Combine(_databaseDirectory, Constants.DATABASE_END_PATH);
    }

    [SetUp]
    public void SetUp()
    {
        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();

        UserConfigurationService userConfigurationService = new(configurationRootMock.Object);

        Mock<IStorageService> storageServiceMock = new();
        storageServiceMock.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_databasePath!);
        storageServiceMock.Setup(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new BitmapImage());

        Database database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        _assetRepository = new(database, storageServiceMock.Object, userConfigurationService);

        DateTime actualDate = DateTime.Now;

        _asset1 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames.IMAGE_1_DUPLICATE_JPG,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_1_DUPLICATE_JPG, Height = PixelHeightAsset.IMAGE_1_DUPLICATE_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_1_DUPLICATE_JPG, Height = ThumbnailHeightAsset.IMAGE_1_DUPLICATE_JPG }
            },
            FileProperties = new()
            {
                Size = FileSize.IMAGE_1_DUPLICATE_JPG,
                Creation = actualDate,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = Hashes.IMAGE_1_DUPLICATE_JPG,
            ImageData = new(),
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset2 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames.IMAGE_9_PNG,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_9_PNG, Height = PixelHeightAsset.IMAGE_9_PNG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_9_PNG, Height = ThumbnailHeightAsset.IMAGE_9_PNG }
            },
            FileProperties = new()
            {
                Size = FileSize.IMAGE_9_PNG,
                Creation = actualDate,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = Hashes.IMAGE_9_PNG,
            ImageData = new(),
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset3 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames.IMAGE_9_DUPLICATE_PNG,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_9_DUPLICATE_PNG, Height = PixelHeightAsset.IMAGE_9_DUPLICATE_PNG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_9_DUPLICATE_PNG, Height = ThumbnailHeightAsset.IMAGE_9_DUPLICATE_PNG }
            },
            FileProperties = new()
            {
                Size = FileSize.IMAGE_9_DUPLICATE_PNG,
                Creation = actualDate,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = Hashes.IMAGE_9_DUPLICATE_PNG,
            ImageData = new(),
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset4 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames.IMAGE_11_HEIC,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_11_HEIC, Height = PixelHeightAsset.IMAGE_11_HEIC },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_11_HEIC, Height = ThumbnailHeightAsset.IMAGE_11_HEIC }
            },
            FileProperties = new()
            {
                Size = FileSize.IMAGE_11_HEIC,
                Creation = actualDate,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = Hashes.IMAGE_11_HEIC,
            ImageData = new(),
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
    }

    [Test]
    public void Properties_AssetAndVisibleAndParentViewModelAndUpdateVisible_UpdatesVisible()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);

        try
        {
            Folder folder = _assetRepository!.AddFolder(assetsDirectory);

            _asset3 = _asset3!.WithFolder(folder);

            DuplicatedAssetViewModel duplicatedAssetViewModel1 = new() { Asset = _asset2! };
            DuplicatedAssetViewModel duplicatedAssetViewModel2 = new() { Asset = _asset3 };

            _duplicatedAssetViewModel = new()
            {
                Asset = _asset3,
                ParentViewModel = [duplicatedAssetViewModel1, duplicatedAssetViewModel2]
            };

            (
                List<string> notifyPropertyChangedEvents,
                List<DuplicatedAssetViewModel> duplicatedAssetViewModelInstances
            ) = NotifyPropertyChangedEvents();

            CheckBeforeChanges(
                _asset3,
                Visibility.Visible,
                [duplicatedAssetViewModel1, duplicatedAssetViewModel2],
                _asset2!.FileName,
                2);

            _duplicatedAssetViewModel!.Visible = Visibility.Collapsed;

            CheckAfterChanges(
                _duplicatedAssetViewModel!,
                _asset3,
                Visibility.Collapsed,
                [duplicatedAssetViewModel1, duplicatedAssetViewModel2],
                _asset2.FileName,
                2);

            _duplicatedAssetViewModel!.Visible = Visibility.Hidden;

            CheckAfterChanges(
                _duplicatedAssetViewModel!,
                _asset3,
                Visibility.Hidden,
                [duplicatedAssetViewModel1, duplicatedAssetViewModel2],
                _asset2.FileName,
                2);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(2));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Visible"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("Visible"));

            CheckInstance(
                duplicatedAssetViewModelInstances,
                _asset3,
                Visibility.Hidden,
                [duplicatedAssetViewModel1, duplicatedAssetViewModel2],
                _asset2.FileName,
                2);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void Properties_AssetAndVisibleAndUpdateVisible_UpdatesVisible()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);

        try
        {
            Folder folder = _assetRepository!.AddFolder(assetsDirectory);

            _asset1 = _asset1!.WithFolder(folder);

            _duplicatedAssetViewModel = new() { Asset = _asset1, Visible = Visibility.Hidden };

            (
                List<string> notifyPropertyChangedEvents,
                List<DuplicatedAssetViewModel> duplicatedAssetViewModelInstances
            ) = NotifyPropertyChangedEvents();

            CheckBeforeChanges(_asset1, Visibility.Hidden, [], null, 0);

            _duplicatedAssetViewModel!.Visible = Visibility.Collapsed;

            CheckAfterChanges(_duplicatedAssetViewModel!, _asset1, Visibility.Collapsed, [], null, 0);

            _duplicatedAssetViewModel!.Visible = Visibility.Visible;

            CheckAfterChanges(_duplicatedAssetViewModel!, _asset1, Visibility.Visible, [], null, 0);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(2));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Visible"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("Visible"));

            CheckInstance(duplicatedAssetViewModelInstances, _asset1, Visibility.Visible, [], null, 0);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void Properties_AssetAndUpdateVisible_UpdatesVisible()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);

        try
        {
            Folder folder = _assetRepository!.AddFolder(assetsDirectory);

            _asset4 = _asset4!.WithFolder(folder);

            _duplicatedAssetViewModel = new() { Asset = _asset4 };

            (
                List<string> notifyPropertyChangedEvents,
                List<DuplicatedAssetViewModel> duplicatedAssetViewModelInstances
            ) = NotifyPropertyChangedEvents();

            CheckBeforeChanges(_asset4, Visibility.Visible, [], null, 0);

            _duplicatedAssetViewModel!.Visible = Visibility.Collapsed;

            CheckAfterChanges(_duplicatedAssetViewModel!, _asset4, Visibility.Collapsed, [], null, 0);

            _duplicatedAssetViewModel!.Visible = Visibility.Hidden;

            CheckAfterChanges(_duplicatedAssetViewModel!, _asset4, Visibility.Hidden, [], null, 0);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(2));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Visible"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("Visible"));

            CheckInstance(duplicatedAssetViewModelInstances, _asset4, Visibility.Hidden, [], null, 0);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    private (List<string> notifyPropertyChangedEvents, List<DuplicatedAssetViewModel> duplicatedAssetViewModelInstances) NotifyPropertyChangedEvents()
    {
        List<string> notifyPropertyChangedEvents = [];
        List<DuplicatedAssetViewModel> duplicatedAssetViewModelInstances = [];

        _duplicatedAssetViewModel!.PropertyChanged += delegate (object? sender, PropertyChangedEventArgs e)
        {
            notifyPropertyChangedEvents.Add(e.PropertyName!);
            duplicatedAssetViewModelInstances.Add((DuplicatedAssetViewModel)sender!);
        };

        return (notifyPropertyChangedEvents, duplicatedAssetViewModelInstances);
    }

    private void CheckBeforeChanges(
        Asset? expectedAsset,
        Visibility expectedVisible,
        DuplicatedSetViewModel expectedParentViewModel,
        string? expectedParentViewModelFileName,
        int expectedParentViewModelDuplicatesCount)
    {
        if (expectedAsset != null)
        {
            AssertAssetPropertyValidity(_duplicatedAssetViewModel!.Asset, expectedAsset);
        }
        else
        {
            Assert.That(_duplicatedAssetViewModel!.Asset, Is.Null);
        }

        Assert.That(_duplicatedAssetViewModel!.Visible, Is.EqualTo(expectedVisible));

        if (expectedParentViewModel.Count > 0)
        {
            Assert.That(_duplicatedAssetViewModel!.ParentViewModel.FileName, Is.EqualTo(expectedParentViewModel.FileName));
            Assert.That(_duplicatedAssetViewModel!.ParentViewModel.FileName, Is.EqualTo(expectedParentViewModel[0].Asset.FileName));
            Assert.That(_duplicatedAssetViewModel!.ParentViewModel.FileName, Is.EqualTo(expectedParentViewModelFileName));

            Assert.That(_duplicatedAssetViewModel!.ParentViewModel.DuplicatesCount, Is.EqualTo(expectedParentViewModel.DuplicatesCount));
            Assert.That(_duplicatedAssetViewModel!.ParentViewModel.DuplicatesCount, Is.EqualTo(expectedParentViewModel.Count));
            Assert.That(_duplicatedAssetViewModel!.ParentViewModel.DuplicatesCount, Is.EqualTo(expectedParentViewModelDuplicatesCount));

            Assert.That(_duplicatedAssetViewModel!.ParentViewModel.Visible, Is.EqualTo(expectedParentViewModel.Visible));

            for (int i = 0; i < expectedParentViewModel.Count; i++)
            {
                Assert.That(_duplicatedAssetViewModel!.ParentViewModel[i].Visible, Is.EqualTo(expectedParentViewModel[i].Visible));
                AssertAssetPropertyValidity(_duplicatedAssetViewModel!.ParentViewModel[i].Asset, expectedParentViewModel[i].Asset);
            }
        }
        else
        {
            Assert.That(_duplicatedAssetViewModel!.ParentViewModel, Is.Empty);
        }
    }

    private static void CheckAfterChanges(
        DuplicatedAssetViewModel duplicatedAssetViewModelInstance,
        Asset? expectedAsset,
        Visibility expectedVisible,
        DuplicatedSetViewModel expectedParentViewModel,
        string? expectedParentViewModelFileName,
        int expectedParentViewModelDuplicatesCount)
    {
        if (expectedAsset != null)
        {
            AssertAssetPropertyValidity(duplicatedAssetViewModelInstance.Asset, expectedAsset);
        }
        else
        {
            Assert.That(duplicatedAssetViewModelInstance.Asset, Is.Null);
        }

        Assert.That(duplicatedAssetViewModelInstance.Visible, Is.EqualTo(expectedVisible));

        if (expectedParentViewModel.Count > 0)
        {
            Assert.That(duplicatedAssetViewModelInstance.ParentViewModel.FileName, Is.EqualTo(expectedParentViewModel.FileName));
            Assert.That(duplicatedAssetViewModelInstance.ParentViewModel.FileName, Is.EqualTo(expectedParentViewModel[0].Asset.FileName));
            Assert.That(duplicatedAssetViewModelInstance.ParentViewModel.FileName, Is.EqualTo(expectedParentViewModelFileName));

            Assert.That(duplicatedAssetViewModelInstance.ParentViewModel.DuplicatesCount, Is.EqualTo(expectedParentViewModel.DuplicatesCount));
            Assert.That(duplicatedAssetViewModelInstance.ParentViewModel.DuplicatesCount, Is.EqualTo(expectedParentViewModel.Count));
            Assert.That(duplicatedAssetViewModelInstance.ParentViewModel.DuplicatesCount, Is.EqualTo(expectedParentViewModelDuplicatesCount));

            Assert.That(duplicatedAssetViewModelInstance.ParentViewModel.Visible, Is.EqualTo(expectedParentViewModel.Visible));

            for (int i = 0; i < expectedParentViewModel.Count; i++)
            {
                AssertAssetPropertyValidity(duplicatedAssetViewModelInstance.ParentViewModel[i].Asset, expectedParentViewModel[i].Asset);
                Assert.That(duplicatedAssetViewModelInstance.ParentViewModel[i].Visible, Is.EqualTo(expectedParentViewModel[i].Visible));
            }
        }
        else
        {
            Assert.That(duplicatedAssetViewModelInstance.ParentViewModel, Is.Empty);
        }
    }

    private static void AssertAssetPropertyValidity(Asset asset, Asset expectedAsset)
    {
        CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(
            asset,
            expectedAsset,
            expectedAsset.FullPath,
            expectedAsset.Folder.Path,
            expectedAsset.Folder);
        Assert.That(asset.ImageData, Is.Not.Null); // Unlike below (Application, CatalogAssetsService), it is set here
    }

    private static void CheckInstance(
        List<DuplicatedAssetViewModel> duplicatedAssetViewModelInstances,
        Asset? expectedAsset,
        Visibility expectedVisible,
        DuplicatedSetViewModel expectedDuplicatedSetViewModel,
        string? expectedParentViewModelFileName,
        int expectedParentViewModelDuplicatesCount)
    {
        int duplicatedAssetViewModelInstancesCount = duplicatedAssetViewModelInstances.Count;

        if (duplicatedAssetViewModelInstancesCount > 1)
        {
            Assert.That(duplicatedAssetViewModelInstances[duplicatedAssetViewModelInstancesCount - 2], Is.EqualTo(duplicatedAssetViewModelInstances[0]));
            // No need to go deeper, same instance because ref updated each time
            Assert.That(duplicatedAssetViewModelInstances[duplicatedAssetViewModelInstancesCount - 1], Is.EqualTo(duplicatedAssetViewModelInstances[duplicatedAssetViewModelInstancesCount - 2]));
        }

        if (duplicatedAssetViewModelInstancesCount > 0)
        {
            CheckAfterChanges(
                duplicatedAssetViewModelInstances[0],
                expectedAsset,
                expectedVisible,
                expectedDuplicatedSetViewModel,
                expectedParentViewModelFileName,
                expectedParentViewModelDuplicatesCount);
        }
    }
}
