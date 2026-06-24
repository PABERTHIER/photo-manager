using System.ComponentModel;
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
    private string? _assetsDirectory;
    private string? _databaseDirectory;

    private DuplicatedAssetViewModel? _duplicatedAssetViewModel;
    private TestableAssetRepository? _testableAssetRepository;

    private Asset? _asset1;
    private Asset? _asset2;
    private Asset? _asset3;
    private Asset? _asset4;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _assetsDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_assetsDirectory, Directories.DATABASE_TESTS);
    }

    [SetUp]
    public void SetUp()
    {
        IConfigurationRoot configurationRootMock = Substitute.For<IConfigurationRoot>();
        configurationRootMock.GetDefaultMockConfig();

        UserConfigurationService userConfigurationService = configurationRootMock.CreateUserConfigurationService();

        IPathProviderService pathProviderServiceMock = Substitute.For<IPathProviderService>();
        pathProviderServiceMock.ResolveDatabaseDirectory().Returns(_databaseDirectory);

        ImageProcessingService imageProcessingService = new(new TestLogger<ImageProcessingService>());
        FileOperationsService fileOperationsService = new(userConfigurationService,
            new TestLogger<FileOperationsService>());
        ImageMetadataService imageMetadataService = new(fileOperationsService, new TestLogger<ImageMetadataService>());
        SqlitePersistenceContext sqlitePersistenceContext =
            PersistenceContextTestHelper.CreateInitializedContext(pathProviderServiceMock.ResolveDatabaseDirectory());
        _testableAssetRepository = new(imageProcessingService, imageMetadataService, userConfigurationService,
            sqlitePersistenceContext, new TestLogger<AssetRepository>());

        DateTime actualDate = DateTime.Now;

        _asset1 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames.IMAGE_1_DUPLICATE_JPG,
            Pixel = new()
            {
                Asset = new()
                {
                    Width = PixelWidthAsset.IMAGE_1_DUPLICATE_JPG,
                    Height = PixelHeightAsset.IMAGE_1_DUPLICATE_JPG
                },
                Thumbnail = new()
                {
                    Width = ThumbnailWidthAsset.IMAGE_1_DUPLICATE_JPG,
                    Height = ThumbnailHeightAsset.IMAGE_1_DUPLICATE_JPG
                }
            },
            FileProperties = new()
            {
                Size = FileSize.IMAGE_1_DUPLICATE_JPG,
                Creation = FileDatesHelper.GetExpectedCreationDate(ModificationDate.Default),
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = ImageRotation.Rotate0,
            Hash = Hashes.IMAGE_1_DUPLICATE_JPG,
            ImageData = SkiaImageData.Empty(),
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
                Creation = FileDatesHelper.GetExpectedCreationDate(ModificationDate.Default),
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = ImageRotation.Rotate0,
            Hash = Hashes.IMAGE_9_PNG,
            ImageData = SkiaImageData.Empty(),
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
                Asset = new()
                {
                    Width = PixelWidthAsset.IMAGE_9_DUPLICATE_PNG,
                    Height = PixelHeightAsset.IMAGE_9_DUPLICATE_PNG
                },
                Thumbnail = new()
                {
                    Width = ThumbnailWidthAsset.IMAGE_9_DUPLICATE_PNG,
                    Height = ThumbnailHeightAsset.IMAGE_9_DUPLICATE_PNG
                }
            },
            FileProperties = new()
            {
                Size = FileSize.IMAGE_9_DUPLICATE_PNG,
                Creation = FileDatesHelper.GetExpectedCreationDate(ModificationDate.Default),
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = ImageRotation.Rotate0,
            Hash = Hashes.IMAGE_9_DUPLICATE_PNG,
            ImageData = SkiaImageData.Empty(),
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
                Thumbnail = new()
                {
                    Width = ThumbnailWidthAsset.IMAGE_11_HEIC,
                    Height = ThumbnailHeightAsset.IMAGE_11_HEIC
                }
            },
            FileProperties = new()
            {
                Size = FileSize.IMAGE_11_HEIC,
                Creation = FileDatesHelper.GetExpectedCreationDate(ModificationDate.Default),
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = ImageRotation.Rotate0,
            Hash = Hashes.IMAGE_11_HEIC,
            ImageData = SkiaImageData.Empty(),
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
    }

    [TearDown]
    public void TearDown()
    {
        _testableAssetRepository?.Dispose();
        TearDownHelper.DeleteTempDbDirectories(_databaseDirectory!);
    }

    [Test]
    public void Properties_AssetAndVisibleAndParentViewModelAndUpdateVisible_UpdatesVisible()
    {
        string assetsDirectory = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);

        Folder folder = _testableAssetRepository!.AddFolder(assetsDirectory);

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
            true,
            [duplicatedAssetViewModel1, duplicatedAssetViewModel2],
            _asset2!.FileName,
            2);

        _duplicatedAssetViewModel!.IsVisible = false;

        CheckAfterChanges(
            _duplicatedAssetViewModel!,
            _asset3,
            false,
            [duplicatedAssetViewModel1, duplicatedAssetViewModel2],
            _asset2.FileName,
            2);

        _duplicatedAssetViewModel!.IsVisible = false;

        CheckAfterChanges(
            _duplicatedAssetViewModel!,
            _asset3,
            false,
            [duplicatedAssetViewModel1, duplicatedAssetViewModel2],
            _asset2.FileName,
            2);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(2));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("IsVisible"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("IsVisible"));

        CheckInstance(
            duplicatedAssetViewModelInstances,
            _asset3,
            false,
            [duplicatedAssetViewModel1, duplicatedAssetViewModel2],
            _asset2.FileName,
            2);
    }

    [Test]
    public void Properties_AssetAndVisibleAndUpdateVisible_UpdatesVisible()
    {
        string assetsDirectory = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);

        Folder folder = _testableAssetRepository!.AddFolder(assetsDirectory);

        _asset1 = _asset1!.WithFolder(folder);

        _duplicatedAssetViewModel = new() { Asset = _asset1, IsVisible = false };

        (
            List<string> notifyPropertyChangedEvents,
            List<DuplicatedAssetViewModel> duplicatedAssetViewModelInstances
        ) = NotifyPropertyChangedEvents();

        CheckBeforeChanges(_asset1, false, [], null, 0);

        _duplicatedAssetViewModel!.IsVisible = false;

        CheckAfterChanges(_duplicatedAssetViewModel!, _asset1, false, [], null, 0);

        _duplicatedAssetViewModel!.IsVisible = true;

        CheckAfterChanges(_duplicatedAssetViewModel!, _asset1, true, [], null, 0);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(2));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("IsVisible"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("IsVisible"));

        CheckInstance(duplicatedAssetViewModelInstances, _asset1, true, [], null, 0);
    }

    [Test]
    public void Properties_AssetAndUpdateVisible_UpdatesVisible()
    {
        string assetsDirectory = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);

        Folder folder = _testableAssetRepository!.AddFolder(assetsDirectory);

        _asset4 = _asset4!.WithFolder(folder);

        _duplicatedAssetViewModel = new() { Asset = _asset4 };

        (
            List<string> notifyPropertyChangedEvents,
            List<DuplicatedAssetViewModel> duplicatedAssetViewModelInstances
        ) = NotifyPropertyChangedEvents();

        CheckBeforeChanges(_asset4, true, [], null, 0);

        _duplicatedAssetViewModel!.IsVisible = false;

        CheckAfterChanges(_duplicatedAssetViewModel!, _asset4, false, [], null, 0);

        _duplicatedAssetViewModel!.IsVisible = false;

        CheckAfterChanges(_duplicatedAssetViewModel!, _asset4, false, [], null, 0);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(2));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("IsVisible"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("IsVisible"));

        CheckInstance(duplicatedAssetViewModelInstances, _asset4, false, [], null, 0);
    }

    private (List<string> notifyPropertyChangedEvents, List<DuplicatedAssetViewModel> duplicatedAssetViewModelInstances)
        NotifyPropertyChangedEvents()
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
        bool expectedVisible,
        DuplicatedSetViewModel expectedParentViewModel,
        string? expectedParentViewModelFileName,
        int expectedParentViewModelDuplicatesCount)
    {
        if (expectedAsset != null)
        {
            AssertAssetPropertyValidity(_duplicatedAssetViewModel!.Asset, expectedAsset);
            AssertDuplicatedAssetProjectedProperties(_duplicatedAssetViewModel, expectedAsset);
        }
        else
        {
            Assert.That(_duplicatedAssetViewModel!.Asset, Is.Null);
        }

        Assert.That(_duplicatedAssetViewModel!.IsVisible, Is.EqualTo(expectedVisible));

        if (expectedParentViewModel.Count > 0)
        {
            Assert.That(_duplicatedAssetViewModel!.ParentViewModel.FileName,
                Is.EqualTo(expectedParentViewModel.FileName));
            Assert.That(_duplicatedAssetViewModel!.ParentViewModel.FileName,
                Is.EqualTo(expectedParentViewModel[0].Asset.FileName));
            Assert.That(_duplicatedAssetViewModel!.ParentViewModel.FileName,
                Is.EqualTo(expectedParentViewModelFileName));

            Assert.That(_duplicatedAssetViewModel!.ParentViewModel.DuplicatesCount,
                Is.EqualTo(expectedParentViewModel.DuplicatesCount));
            Assert.That(_duplicatedAssetViewModel!.ParentViewModel.DuplicatesCount,
                Is.EqualTo(expectedParentViewModel.Count));
            Assert.That(_duplicatedAssetViewModel!.ParentViewModel.DuplicatesCount,
                Is.EqualTo(expectedParentViewModelDuplicatesCount));

            Assert.That(_duplicatedAssetViewModel!.ParentViewModel.IsVisible,
                Is.EqualTo(expectedParentViewModel.IsVisible));

            for (int i = 0; i < expectedParentViewModel.Count; i++)
            {
                Assert.That(_duplicatedAssetViewModel!.ParentViewModel[i].IsVisible,
                    Is.EqualTo(expectedParentViewModel[i].IsVisible));
                AssertAssetPropertyValidity(_duplicatedAssetViewModel!.ParentViewModel[i].Asset,
                    expectedParentViewModel[i].Asset);
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
        bool expectedVisible,
        DuplicatedSetViewModel expectedParentViewModel,
        string? expectedParentViewModelFileName,
        int expectedParentViewModelDuplicatesCount)
    {
        if (expectedAsset != null)
        {
            AssertAssetPropertyValidity(duplicatedAssetViewModelInstance.Asset, expectedAsset);
            AssertDuplicatedAssetProjectedProperties(duplicatedAssetViewModelInstance, expectedAsset);
        }
        else
        {
            Assert.That(duplicatedAssetViewModelInstance.Asset, Is.Null);
        }

        Assert.That(duplicatedAssetViewModelInstance.IsVisible, Is.EqualTo(expectedVisible));

        if (expectedParentViewModel.Count > 0)
        {
            Assert.That(duplicatedAssetViewModelInstance.ParentViewModel.FileName,
                Is.EqualTo(expectedParentViewModel.FileName));
            Assert.That(duplicatedAssetViewModelInstance.ParentViewModel.FileName,
                Is.EqualTo(expectedParentViewModel[0].Asset.FileName));
            Assert.That(duplicatedAssetViewModelInstance.ParentViewModel.FileName,
                Is.EqualTo(expectedParentViewModelFileName));

            Assert.That(duplicatedAssetViewModelInstance.ParentViewModel.DuplicatesCount,
                Is.EqualTo(expectedParentViewModel.DuplicatesCount));
            Assert.That(duplicatedAssetViewModelInstance.ParentViewModel.DuplicatesCount,
                Is.EqualTo(expectedParentViewModel.Count));
            Assert.That(duplicatedAssetViewModelInstance.ParentViewModel.DuplicatesCount,
                Is.EqualTo(expectedParentViewModelDuplicatesCount));

            Assert.That(duplicatedAssetViewModelInstance.ParentViewModel.IsVisible,
                Is.EqualTo(expectedParentViewModel.IsVisible));

            for (int i = 0; i < expectedParentViewModel.Count; i++)
            {
                AssertAssetPropertyValidity(duplicatedAssetViewModelInstance.ParentViewModel[i].Asset,
                    expectedParentViewModel[i].Asset);
                Assert.That(duplicatedAssetViewModelInstance.ParentViewModel[i].IsVisible,
                    Is.EqualTo(expectedParentViewModel[i].IsVisible));
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

    private static void AssertDuplicatedAssetProjectedProperties(DuplicatedAssetViewModel duplicatedAssetViewModel,
        Asset expectedAsset)
    {
        Assert.That(duplicatedAssetViewModel.FullPath, Is.EqualTo(expectedAsset.FullPath));
        Assert.That(duplicatedAssetViewModel.PixelSize,
            Is.EqualTo($"{expectedAsset.Pixel.Asset.Width}x{expectedAsset.Pixel.Asset.Height} pixels"));
        Assert.That(duplicatedAssetViewModel.FileSize, Is.EqualTo(expectedAsset.FileProperties.Size));
        Assert.That(duplicatedAssetViewModel.FileCreation, Is.EqualTo(expectedAsset.FileProperties.Creation));
        Assert.That(duplicatedAssetViewModel.FileModification, Is.EqualTo(expectedAsset.FileProperties.Modification));
    }

    private static void CheckInstance(
        List<DuplicatedAssetViewModel> duplicatedAssetViewModelInstances,
        Asset? expectedAsset,
        bool expectedVisible,
        DuplicatedSetViewModel expectedDuplicatedSetViewModel,
        string? expectedParentViewModelFileName,
        int expectedParentViewModelDuplicatesCount)
    {
        int duplicatedAssetViewModelInstancesCount = duplicatedAssetViewModelInstances.Count;

        if (duplicatedAssetViewModelInstancesCount > 1)
        {
            Assert.That(duplicatedAssetViewModelInstances[duplicatedAssetViewModelInstancesCount - 2],
                Is.EqualTo(duplicatedAssetViewModelInstances[0]));
            // No need to go deeper, same instance because ref updated each time
            Assert.That(duplicatedAssetViewModelInstances[duplicatedAssetViewModelInstancesCount - 1],
                Is.EqualTo(duplicatedAssetViewModelInstances[duplicatedAssetViewModelInstancesCount - 2]));
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
