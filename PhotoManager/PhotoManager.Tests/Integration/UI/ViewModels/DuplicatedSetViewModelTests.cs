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
public class DuplicatedSetViewModelTests
{
    private string? _assetsDirectory;
    private string? _databaseDirectory;

    private DuplicatedSetViewModel? _duplicatedSetViewModel;
    private TestableAssetRepository? _testableAssetRepository;

    private Asset? _asset2;
    private Asset? _asset3;

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

        UserConfigurationService userConfigurationService = new(configurationRootMock);

        IPathProviderService pathProviderServiceMock = Substitute.For<IPathProviderService>();
        pathProviderServiceMock.ResolveDatabaseDirectory().Returns(_databaseDirectory);

        ImageProcessingService imageProcessingService = new(new TestLogger<ImageProcessingService>());
        FileOperationsService fileOperationsService = new(userConfigurationService,
            new TestLogger<FileOperationsService>());
        ImageMetadataService imageMetadataService = new(fileOperationsService, new TestLogger<ImageMetadataService>());
        SqliteConnectionFactory sqliteConnectionFactory = new(new TestLogger<SqliteConnectionFactory>());
        SqliteBackupService sqliteBackupService = new(sqliteConnectionFactory);
        SqlitePersistenceContext sqlitePersistenceContext = new(
            sqliteConnectionFactory, sqliteBackupService, new TestLogger<SqlitePersistenceContext>());
        _testableAssetRepository = new(pathProviderServiceMock, imageProcessingService,
            imageMetadataService, userConfigurationService, sqlitePersistenceContext,
            new TestLogger<AssetRepository>());

        DateTime actualDate = DateTime.Now;

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
                Creation = actualDate,
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
    }

    [TearDown]
    public void TearDown()
    {
        _testableAssetRepository?.Dispose();
        TearDownHelper.DeleteTempDbDirectories(_databaseDirectory!);
    }

    [Test]
    public void FileName_EmptySet_ReturnsEmptyString()
    {
        _duplicatedSetViewModel = [];

        Assert.That(_duplicatedSetViewModel.FileName, Is.EqualTo(string.Empty));
    }

    [Test]
    public void Properties_ChangeVisibleToCollapsedAndThenVisibleForEachDuplicate_UpdatesVisibleAndDuplicatesCount()
    {
        string assetsDirectory = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);

        Folder folder = _testableAssetRepository!.AddFolder(assetsDirectory);

        _asset3 = _asset3!.WithFolder(folder);

        _duplicatedSetViewModel = [];

        (
            List<string> notifyPropertyChangedEvents,
            List<DuplicatedSetViewModel> duplicatedSetViewModelInstances
        ) = NotifyPropertyChangedEvents();

        DuplicatedAssetViewModel duplicatedAssetViewModel1 =
            new() { Asset = _asset2!, ParentViewModel = _duplicatedSetViewModel };
        _duplicatedSetViewModel.Add(duplicatedAssetViewModel1);

        DuplicatedAssetViewModel duplicatedAssetViewModel2 =
            new() { Asset = _asset3, ParentViewModel = _duplicatedSetViewModel };
        _duplicatedSetViewModel.Add(duplicatedAssetViewModel2);

        CheckBeforeChanges(
            [duplicatedAssetViewModel1, duplicatedAssetViewModel2],
            _asset2!.FileName,
            2,
            true);

        duplicatedAssetViewModel1.IsVisible = false;

        CheckAfterChanges(
            _duplicatedSetViewModel!,
            [duplicatedAssetViewModel1, duplicatedAssetViewModel2],
            _asset2.FileName,
            1,
            false);

        duplicatedAssetViewModel2.IsVisible = false;

        CheckAfterChanges(
            _duplicatedSetViewModel!,
            [duplicatedAssetViewModel1, duplicatedAssetViewModel2],
            _asset2.FileName,
            0,
            false);

        duplicatedAssetViewModel1.IsVisible = true;

        CheckAfterChanges(
            _duplicatedSetViewModel!,
            [duplicatedAssetViewModel1, duplicatedAssetViewModel2],
            _asset2.FileName,
            1,
            false);

        duplicatedAssetViewModel2.IsVisible = true;

        CheckAfterChanges(
            _duplicatedSetViewModel!,
            [duplicatedAssetViewModel1, duplicatedAssetViewModel2],
            _asset2.FileName,
            2,
            true);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(8));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatesCount"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("IsVisible"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("DuplicatesCount"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("IsVisible"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("DuplicatesCount"));
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("IsVisible"));
        Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("DuplicatesCount"));
        Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("IsVisible"));

        CheckInstance(
            duplicatedSetViewModelInstances,
            [duplicatedAssetViewModel1, duplicatedAssetViewModel2],
            _asset2.FileName,
            2,
            true);
    }

    [Test]
    public void Properties_ChangeVisibleToCollapsedForEachDuplicate_UpdatesVisibleAndDuplicatesCount()
    {
        string assetsDirectory = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);

        Folder folder = _testableAssetRepository!.AddFolder(assetsDirectory);

        _asset3 = _asset3!.WithFolder(folder);

        _duplicatedSetViewModel = [];

        (
            List<string> notifyPropertyChangedEvents,
            List<DuplicatedSetViewModel> duplicatedSetViewModelInstances
        ) = NotifyPropertyChangedEvents();

        DuplicatedAssetViewModel duplicatedAssetViewModel1 =
            new() { Asset = _asset2!, ParentViewModel = _duplicatedSetViewModel };
        _duplicatedSetViewModel.Add(duplicatedAssetViewModel1);

        DuplicatedAssetViewModel duplicatedAssetViewModel2 =
            new() { Asset = _asset3, ParentViewModel = _duplicatedSetViewModel };
        _duplicatedSetViewModel.Add(duplicatedAssetViewModel2);

        CheckBeforeChanges(
            [duplicatedAssetViewModel1, duplicatedAssetViewModel2],
            _asset2!.FileName,
            2,
            true);

        duplicatedAssetViewModel1.IsVisible = false;

        CheckAfterChanges(
            _duplicatedSetViewModel!,
            [duplicatedAssetViewModel1, duplicatedAssetViewModel2],
            _asset2.FileName,
            1,
            false);

        duplicatedAssetViewModel2.IsVisible = false;

        CheckAfterChanges(
            _duplicatedSetViewModel!,
            [duplicatedAssetViewModel1, duplicatedAssetViewModel2],
            _asset2.FileName,
            0,
            false);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(4));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatesCount"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("IsVisible"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("DuplicatesCount"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("IsVisible"));

        CheckInstance(
            duplicatedSetViewModelInstances,
            [duplicatedAssetViewModel1, duplicatedAssetViewModel2],
            _asset2.FileName,
            0,
            false);
    }

    [Test]
    public void Properties_ChangeVisibleToHiddenForEachDuplicate_UpdatesVisibleAndDuplicatesCount()
    {
        string assetsDirectory = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);

        Folder folder = _testableAssetRepository!.AddFolder(assetsDirectory);

        _asset3 = _asset3!.WithFolder(folder);

        _duplicatedSetViewModel = [];

        (
            List<string> notifyPropertyChangedEvents,
            List<DuplicatedSetViewModel> duplicatedSetViewModelInstances
        ) = NotifyPropertyChangedEvents();

        DuplicatedAssetViewModel duplicatedAssetViewModel1 =
            new() { Asset = _asset2!, ParentViewModel = _duplicatedSetViewModel };
        _duplicatedSetViewModel.Add(duplicatedAssetViewModel1);

        DuplicatedAssetViewModel duplicatedAssetViewModel2 =
            new() { Asset = _asset3, ParentViewModel = _duplicatedSetViewModel };
        _duplicatedSetViewModel.Add(duplicatedAssetViewModel2);

        CheckBeforeChanges(
            [duplicatedAssetViewModel1, duplicatedAssetViewModel2],
            _asset2!.FileName,
            2,
            true);

        duplicatedAssetViewModel1.IsVisible = false;

        CheckAfterChanges(
            _duplicatedSetViewModel!,
            [duplicatedAssetViewModel1, duplicatedAssetViewModel2],
            _asset2.FileName,
            1,
            false);

        duplicatedAssetViewModel2.IsVisible = false;

        CheckAfterChanges(
            _duplicatedSetViewModel!,
            [duplicatedAssetViewModel1, duplicatedAssetViewModel2],
            _asset2.FileName,
            0,
            false);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(4));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatesCount"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("IsVisible"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("DuplicatesCount"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("IsVisible"));

        CheckInstance(
            duplicatedSetViewModelInstances,
            [duplicatedAssetViewModel1, duplicatedAssetViewModel2],
            _asset2.FileName,
            0,
            false);
    }

    [Test]
    public void Properties_ChangeVisibleToVisibleForEachDuplicate_DoesNothing()
    {
        string assetsDirectory = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);

        Folder folder = _testableAssetRepository!.AddFolder(assetsDirectory);

        _asset3 = _asset3!.WithFolder(folder);

        _duplicatedSetViewModel = [];

        (
            List<string> notifyPropertyChangedEvents,
            List<DuplicatedSetViewModel> duplicatedSetViewModelInstances
        ) = NotifyPropertyChangedEvents();

        DuplicatedAssetViewModel duplicatedAssetViewModel1 =
            new() { Asset = _asset2!, ParentViewModel = _duplicatedSetViewModel };
        _duplicatedSetViewModel.Add(duplicatedAssetViewModel1);

        DuplicatedAssetViewModel duplicatedAssetViewModel2 =
            new() { Asset = _asset3, ParentViewModel = _duplicatedSetViewModel };
        _duplicatedSetViewModel.Add(duplicatedAssetViewModel2);

        CheckBeforeChanges(
            [duplicatedAssetViewModel1, duplicatedAssetViewModel2],
            _asset2!.FileName,
            2,
            true);

        duplicatedAssetViewModel1.IsVisible = true;

        CheckAfterChanges(
            _duplicatedSetViewModel!,
            [duplicatedAssetViewModel1, duplicatedAssetViewModel2],
            _asset2.FileName,
            2,
            true);

        duplicatedAssetViewModel2.IsVisible = true;

        CheckAfterChanges(
            _duplicatedSetViewModel!,
            [duplicatedAssetViewModel1, duplicatedAssetViewModel2],
            _asset2.FileName,
            2,
            true);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(4));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatesCount"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("IsVisible"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("DuplicatesCount"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("IsVisible"));

        CheckInstance(
            duplicatedSetViewModelInstances,
            [duplicatedAssetViewModel1, duplicatedAssetViewModel2],
            _asset2.FileName,
            2,
            true);
    }

    [Test]
    public void Properties_ChangeVisibleToCollapsedForOneDuplicate_UpdatesVisibleAndDuplicatesCount()
    {
        string assetsDirectory = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);

        Folder folder = _testableAssetRepository!.AddFolder(assetsDirectory);

        _asset3 = _asset3!.WithFolder(folder);

        _duplicatedSetViewModel = [];

        (
            List<string> notifyPropertyChangedEvents,
            List<DuplicatedSetViewModel> duplicatedSetViewModelInstances
        ) = NotifyPropertyChangedEvents();

        DuplicatedAssetViewModel duplicatedAssetViewModel1 =
            new() { Asset = _asset2!, ParentViewModel = _duplicatedSetViewModel };
        _duplicatedSetViewModel.Add(duplicatedAssetViewModel1);

        CheckBeforeChanges(
            [duplicatedAssetViewModel1],
            _asset2!.FileName,
            1,
            false);

        duplicatedAssetViewModel1.IsVisible = false;

        CheckAfterChanges(
            _duplicatedSetViewModel!,
            [duplicatedAssetViewModel1],
            _asset2.FileName,
            0,
            false);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(2));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatesCount"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("IsVisible"));

        CheckInstance(
            duplicatedSetViewModelInstances,
            [duplicatedAssetViewModel1],
            _asset2.FileName,
            0,
            false);
    }

    [Test]
    public void Properties_NoDuplicatedAssetViewModel_HasDefaultState()
    {
        string assetsDirectory = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);

        Folder folder = _testableAssetRepository!.AddFolder(assetsDirectory);

        _asset3 = _asset3!.WithFolder(folder);

        _duplicatedSetViewModel = [];

        (
            List<string> notifyPropertyChangedEvents,
            List<DuplicatedSetViewModel> duplicatedSetViewModelInstances
        ) = NotifyPropertyChangedEvents();

        CheckBeforeChanges(
            [],
            null,
            0,
            false);

        CheckAfterChanges(
            _duplicatedSetViewModel!,
            [],
            null,
            0,
            false);

        Assert.That(notifyPropertyChangedEvents, Is.Empty);
        Assert.That(duplicatedSetViewModelInstances, Is.Empty);
    }

    private (List<string> notifyPropertyChangedEvents, List<DuplicatedSetViewModel> duplicatedSetViewModelInstances)
        NotifyPropertyChangedEvents()
    {
        List<string> notifyPropertyChangedEvents = [];
        List<DuplicatedSetViewModel> duplicatedSetViewModelInstances = [];

        _duplicatedSetViewModel!.PropertyChanged += delegate (object? sender, PropertyChangedEventArgs e)
        {
            notifyPropertyChangedEvents.Add(e.PropertyName!);
            duplicatedSetViewModelInstances.Add((DuplicatedSetViewModel)sender!);
        };

        return (notifyPropertyChangedEvents, duplicatedSetViewModelInstances);
    }

    private void CheckBeforeChanges(
        DuplicatedSetViewModel expectedDuplicatedSetViewModel,
        string? expectedFileName,
        int expectedDuplicatesCount,
        bool expectedVisible)
    {
        if (expectedDuplicatedSetViewModel.Count > 0)
        {
            AssertDuplicatedSetViewModelValidity(
                _duplicatedSetViewModel!,
                expectedDuplicatedSetViewModel,
                expectedFileName,
                expectedDuplicatesCount,
                expectedVisible);

            for (int i = 0; i < expectedDuplicatedSetViewModel.Count; i++)
            {
                Assert.That(_duplicatedSetViewModel![i].IsVisible,
                    Is.EqualTo(expectedDuplicatedSetViewModel[i].IsVisible));
                AssertAssetPropertyValidity(_duplicatedSetViewModel![i].Asset, expectedDuplicatedSetViewModel[i].Asset);

                AssertDuplicatedSetViewModelValidity(
                    _duplicatedSetViewModel,
                    expectedDuplicatedSetViewModel[i].ParentViewModel,
                    expectedFileName,
                    expectedDuplicatesCount,
                    expectedVisible);
            }
        }
        else
        {
            Assert.That(_duplicatedSetViewModel, Is.Empty);
        }
    }

    private static void CheckAfterChanges(
        DuplicatedSetViewModel duplicatedSetViewModelInstance,
        DuplicatedSetViewModel expectedDuplicatedSetViewModel,
        string? expectedFileName,
        int expectedDuplicatesCount,
        bool expectedVisible)
    {
        if (expectedDuplicatedSetViewModel.Count > 0)
        {
            AssertDuplicatedSetViewModelValidity(
                duplicatedSetViewModelInstance,
                expectedDuplicatedSetViewModel,
                expectedFileName,
                expectedDuplicatesCount,
                expectedVisible);

            for (int i = 0; i < expectedDuplicatedSetViewModel.Count; i++)
            {
                Assert.That(duplicatedSetViewModelInstance[i].IsVisible,
                    Is.EqualTo(expectedDuplicatedSetViewModel[i].IsVisible));
                AssertAssetPropertyValidity(duplicatedSetViewModelInstance[i].Asset,
                    expectedDuplicatedSetViewModel[i].Asset);

                AssertDuplicatedSetViewModelValidity(
                    duplicatedSetViewModelInstance,
                    expectedDuplicatedSetViewModel[i].ParentViewModel,
                    expectedFileName,
                    expectedDuplicatesCount,
                    expectedVisible);
            }
        }
        else
        {
            Assert.That(duplicatedSetViewModelInstance, Is.Empty);
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

    private static void AssertDuplicatedSetViewModelValidity(
        DuplicatedSetViewModel duplicatedSetViewModel,
        DuplicatedSetViewModel expectedDuplicatedSetViewModel,
        string? expectedFileName,
        int expectedDuplicatesCount,
        bool expectedVisible)
    {
        Assert.That(duplicatedSetViewModel.FileName, Is.EqualTo(expectedDuplicatedSetViewModel.FileName));
        Assert.That(duplicatedSetViewModel.FileName, Is.EqualTo(expectedDuplicatedSetViewModel[0].Asset.FileName));
        Assert.That(duplicatedSetViewModel.FileName, Is.EqualTo(expectedFileName));

        Assert.That(duplicatedSetViewModel.DuplicatesCount, Is.EqualTo(expectedDuplicatedSetViewModel.DuplicatesCount));
        Assert.That(duplicatedSetViewModel.DuplicatesCount, Is.EqualTo(expectedDuplicatesCount));

        Assert.That(duplicatedSetViewModel.IsVisible, Is.EqualTo(expectedVisible));
    }

    private static void CheckInstance(
        List<DuplicatedSetViewModel> duplicatedSetViewModelInstances,
        DuplicatedSetViewModel expectedDuplicatedSetViewModel,
        string? expectedFileName,
        int expectedDuplicatesCount,
        bool expectedVisible)
    {
        int duplicatedSetViewModelInstancesCount = duplicatedSetViewModelInstances.Count;

        if (duplicatedSetViewModelInstancesCount > 1)
        {
            Assert.That(duplicatedSetViewModelInstances[duplicatedSetViewModelInstancesCount - 2],
                Is.EqualTo(duplicatedSetViewModelInstances[0]));
            // No need to go deeper, same instance because ref updated each time
            Assert.That(duplicatedSetViewModelInstances[duplicatedSetViewModelInstancesCount - 1],
                Is.EqualTo(duplicatedSetViewModelInstances[duplicatedSetViewModelInstancesCount - 2]));
        }

        if (duplicatedSetViewModelInstancesCount > 0)
        {
            CheckAfterChanges(
                duplicatedSetViewModelInstances[0],
                expectedDuplicatedSetViewModel,
                expectedFileName,
                expectedDuplicatesCount,
                expectedVisible);
        }
    }
}
