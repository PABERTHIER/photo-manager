using System.ComponentModel;
using System.Windows;

namespace PhotoManager.Tests.Integration.UI.ViewModels;

[TestFixture]
public class DuplicatedSetViewModelTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;
    private readonly DateTime _expectedFileModificationDateTime = new (2024, 06, 07, 08, 54, 37);
    private const string DATABASE_END_PATH = "v1.0";

    private DuplicatedSetViewModel? _duplicatedSetViewModel;
    private AssetRepository? _assetRepository;

    private Asset _asset2;
    private Asset _asset3;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        _databaseDirectory = Path.Combine(_dataDirectory, "DatabaseTests");
        _databasePath = Path.Combine(_databaseDirectory, DATABASE_END_PATH);
    }

    [SetUp]
    public void SetUp()
    {
        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();

        UserConfigurationService userConfigurationService = new (configurationRootMock.Object);

        Mock<IStorageService> storageServiceMock = new();
        storageServiceMock.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_databasePath!);
        storageServiceMock.Setup(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new BitmapImage());

        Database database = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        _assetRepository = new (database, storageServiceMock.Object, userConfigurationService);

        DateTime actualDate = DateTime.Now;

        _asset2 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "Image 9.png",
            Pixel = new()
            {
                Asset = new() { Width = 1280, Height = 720 },
                Thumbnail = new() { Width = 200, Height = 112 }
            },
            FileProperties = new()
            {
                Size = 126277,
                Creation = actualDate,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = "bcc994c14aa314dbc2dfbf48ffd34fa628dadcd86cdb8efda113b94a9035f15956cf039f5858b74cd7f404e98f7e84d9821b39aaa6cbbdc73228fa74ad2a5c20",
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
            FileName = "Image 9_duplicate.png",
            Pixel = new()
            {
                Asset = new() { Width = 1280, Height = 720 },
                Thumbnail = new() { Width = 200, Height = 112 }
            },
            FileProperties = new()
            {
                Size = 126277,
                Creation = actualDate,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = "bcc994c14aa314dbc2dfbf48ffd34fa628dadcd86cdb8efda113b94a9035f15956cf039f5858b74cd7f404e98f7e84d9821b39aaa6cbbdc73228fa74ad2a5c20",
            ImageData = new(),
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
    }

    [Test]
    public void Properties_ChangeVisibleToCollapsedAndThenVisibleForEachDuplicate_UpdatesVisibleAndDuplicatesCount()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder2");

        try
        {
            Folder folder = _assetRepository!.AddFolder(assetsDirectory);

            _asset3 = _asset3.WithFolder(folder);

            _duplicatedSetViewModel = [];

            (
                List<string> notifyPropertyChangedEvents,
                List<DuplicatedSetViewModel> duplicatedSetViewModelInstances
            ) = NotifyPropertyChangedEvents();

            DuplicatedAssetViewModel duplicatedAssetViewModel1 = new() { Asset = _asset2, ParentViewModel = _duplicatedSetViewModel };
            _duplicatedSetViewModel.Add(duplicatedAssetViewModel1);

            DuplicatedAssetViewModel duplicatedAssetViewModel2 = new() { Asset = _asset3, ParentViewModel = _duplicatedSetViewModel };
            _duplicatedSetViewModel.Add(duplicatedAssetViewModel2);

            CheckBeforeChanges(
                [duplicatedAssetViewModel1, duplicatedAssetViewModel2],
                _asset2.FileName,
                2,
                Visibility.Visible);

            duplicatedAssetViewModel1.Visible = Visibility.Collapsed;

            CheckAfterChanges(
                _duplicatedSetViewModel!,
                [duplicatedAssetViewModel1, duplicatedAssetViewModel2],
                _asset2.FileName,
                1,
                Visibility.Collapsed);

            duplicatedAssetViewModel2.Visible = Visibility.Collapsed;

            CheckAfterChanges(
                _duplicatedSetViewModel!,
                [duplicatedAssetViewModel1, duplicatedAssetViewModel2],
                _asset2.FileName,
                0,
                Visibility.Collapsed);

            duplicatedAssetViewModel1.Visible = Visibility.Visible;

            CheckAfterChanges(
                _duplicatedSetViewModel!,
                [duplicatedAssetViewModel1, duplicatedAssetViewModel2],
                _asset2.FileName,
                1,
                Visibility.Collapsed);

            duplicatedAssetViewModel2.Visible = Visibility.Visible;

            CheckAfterChanges(
                _duplicatedSetViewModel!,
                [duplicatedAssetViewModel1, duplicatedAssetViewModel2],
                _asset2.FileName,
                2,
                Visibility.Visible);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(8));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatesCount"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("Visible"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("DuplicatesCount"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("Visible"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("DuplicatesCount"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("Visible"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("DuplicatesCount"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("Visible"));

            CheckInstance(
                duplicatedSetViewModelInstances,
                [duplicatedAssetViewModel1, duplicatedAssetViewModel2],
                _asset2.FileName,
                2,
                Visibility.Visible);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void Properties_ChangeVisibleToCollapsedForEachDuplicate_UpdatesVisibleAndDuplicatesCount()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder2");

        try
        {
            Folder folder = _assetRepository!.AddFolder(assetsDirectory);

            _asset3 = _asset3.WithFolder(folder);

            _duplicatedSetViewModel = [];

            (
                List<string> notifyPropertyChangedEvents,
                List<DuplicatedSetViewModel> duplicatedSetViewModelInstances
            ) = NotifyPropertyChangedEvents();

            DuplicatedAssetViewModel duplicatedAssetViewModel1 = new() { Asset = _asset2, ParentViewModel = _duplicatedSetViewModel };
            _duplicatedSetViewModel.Add(duplicatedAssetViewModel1);

            DuplicatedAssetViewModel duplicatedAssetViewModel2 = new() { Asset = _asset3, ParentViewModel = _duplicatedSetViewModel };
            _duplicatedSetViewModel.Add(duplicatedAssetViewModel2);

            CheckBeforeChanges(
                [duplicatedAssetViewModel1, duplicatedAssetViewModel2],
                _asset2.FileName,
                2,
                Visibility.Visible);

            duplicatedAssetViewModel1.Visible = Visibility.Collapsed;

            CheckAfterChanges(
                _duplicatedSetViewModel!,
                [duplicatedAssetViewModel1, duplicatedAssetViewModel2],
                _asset2.FileName,
                1,
                Visibility.Collapsed);

            duplicatedAssetViewModel2.Visible = Visibility.Collapsed;

            CheckAfterChanges(
                _duplicatedSetViewModel!,
                [duplicatedAssetViewModel1, duplicatedAssetViewModel2],
                _asset2.FileName,
                0,
                Visibility.Collapsed);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(4));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatesCount"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("Visible"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("DuplicatesCount"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("Visible"));

            CheckInstance(
                duplicatedSetViewModelInstances,
                [duplicatedAssetViewModel1, duplicatedAssetViewModel2],
                _asset2.FileName,
                0,
                Visibility.Collapsed);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void Properties_ChangeVisibleToHiddenForEachDuplicate_UpdatesVisibleAndDuplicatesCount()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder2");

        try
        {
            Folder folder = _assetRepository!.AddFolder(assetsDirectory);

            _asset3 = _asset3.WithFolder(folder);

            _duplicatedSetViewModel = [];

            (
                List<string> notifyPropertyChangedEvents,
                List<DuplicatedSetViewModel> duplicatedSetViewModelInstances
            ) = NotifyPropertyChangedEvents();

            DuplicatedAssetViewModel duplicatedAssetViewModel1 = new() { Asset = _asset2, ParentViewModel = _duplicatedSetViewModel };
            _duplicatedSetViewModel.Add(duplicatedAssetViewModel1);

            DuplicatedAssetViewModel duplicatedAssetViewModel2 = new() { Asset = _asset3, ParentViewModel = _duplicatedSetViewModel };
            _duplicatedSetViewModel.Add(duplicatedAssetViewModel2);

            CheckBeforeChanges(
                [duplicatedAssetViewModel1, duplicatedAssetViewModel2],
                _asset2.FileName,
                2,
                Visibility.Visible);

            duplicatedAssetViewModel1.Visible = Visibility.Hidden;

            CheckAfterChanges(
                _duplicatedSetViewModel!,
                [duplicatedAssetViewModel1, duplicatedAssetViewModel2],
                _asset2.FileName,
                1,
                Visibility.Collapsed);

            duplicatedAssetViewModel2.Visible = Visibility.Hidden;

            CheckAfterChanges(
                _duplicatedSetViewModel!,
                [duplicatedAssetViewModel1, duplicatedAssetViewModel2],
                _asset2.FileName,
                0,
                Visibility.Collapsed);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(4));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatesCount"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("Visible"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("DuplicatesCount"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("Visible"));

            CheckInstance(
                duplicatedSetViewModelInstances,
                [duplicatedAssetViewModel1, duplicatedAssetViewModel2],
                _asset2.FileName,
                0,
                Visibility.Collapsed);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void Properties_ChangeVisibleToVisibleForEachDuplicate_DoesNothing()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder2");

        try
        {
            Folder folder = _assetRepository!.AddFolder(assetsDirectory);

            _asset3 = _asset3.WithFolder(folder);

            _duplicatedSetViewModel = [];

            (
                List<string> notifyPropertyChangedEvents,
                List<DuplicatedSetViewModel> duplicatedSetViewModelInstances
            ) = NotifyPropertyChangedEvents();

            DuplicatedAssetViewModel duplicatedAssetViewModel1 = new() { Asset = _asset2, ParentViewModel = _duplicatedSetViewModel };
            _duplicatedSetViewModel.Add(duplicatedAssetViewModel1);

            DuplicatedAssetViewModel duplicatedAssetViewModel2 = new() { Asset = _asset3, ParentViewModel = _duplicatedSetViewModel };
            _duplicatedSetViewModel.Add(duplicatedAssetViewModel2);

            CheckBeforeChanges(
                [duplicatedAssetViewModel1, duplicatedAssetViewModel2],
                _asset2.FileName,
                2,
                Visibility.Visible);

            duplicatedAssetViewModel1.Visible = Visibility.Visible;

            CheckAfterChanges(
                _duplicatedSetViewModel!,
                [duplicatedAssetViewModel1, duplicatedAssetViewModel2],
                _asset2.FileName,
                2,
                Visibility.Visible);

            duplicatedAssetViewModel2.Visible = Visibility.Visible;

            CheckAfterChanges(
                _duplicatedSetViewModel!,
                [duplicatedAssetViewModel1, duplicatedAssetViewModel2],
                _asset2.FileName,
                2,
                Visibility.Visible);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(4));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatesCount"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("Visible"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("DuplicatesCount"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("Visible"));

            CheckInstance(
                duplicatedSetViewModelInstances,
                [duplicatedAssetViewModel1, duplicatedAssetViewModel2],
                _asset2.FileName,
                2,
                Visibility.Visible);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void Properties_ChangeVisibleToCollapsedForOneDuplicate_UpdatesVisibleAndDuplicatesCount()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder2");

        try
        {
            Folder folder = _assetRepository!.AddFolder(assetsDirectory);

            _asset3 = _asset3.WithFolder(folder);

            _duplicatedSetViewModel = [];

            (
                List<string> notifyPropertyChangedEvents,
                List<DuplicatedSetViewModel> duplicatedSetViewModelInstances
            ) = NotifyPropertyChangedEvents();

            DuplicatedAssetViewModel duplicatedAssetViewModel1 = new() { Asset = _asset2, ParentViewModel = _duplicatedSetViewModel };
            _duplicatedSetViewModel.Add(duplicatedAssetViewModel1);

            CheckBeforeChanges(
                [duplicatedAssetViewModel1],
                _asset2.FileName,
                1,
                Visibility.Collapsed);

            duplicatedAssetViewModel1.Visible = Visibility.Collapsed;

            CheckAfterChanges(
                _duplicatedSetViewModel!,
                [duplicatedAssetViewModel1],
                _asset2.FileName,
                0,
                Visibility.Collapsed);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(2));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatesCount"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("Visible"));

            CheckInstance(
                duplicatedSetViewModelInstances,
                [duplicatedAssetViewModel1],
                _asset2.FileName,
                0,
                Visibility.Collapsed);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void Properties_NoDuplicatedAssetViewModel_HasDefaultState()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder2");

        try
        {
            Folder folder = _assetRepository!.AddFolder(assetsDirectory);

            _asset3 = _asset3.WithFolder(folder);

            _duplicatedSetViewModel = [];

            (
                List<string> notifyPropertyChangedEvents,
                List<DuplicatedSetViewModel> duplicatedSetViewModelInstances
            ) = NotifyPropertyChangedEvents();

            CheckBeforeChanges(
                [],
                null,
                0,
                Visibility.Collapsed);

            CheckAfterChanges(
                _duplicatedSetViewModel!,
                [],
                null,
                0,
                Visibility.Collapsed);

            Assert.That(notifyPropertyChangedEvents, Is.Empty);
            Assert.That(duplicatedSetViewModelInstances, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    private (List<string> notifyPropertyChangedEvents, List<DuplicatedSetViewModel> duplicatedSetViewModelInstances) NotifyPropertyChangedEvents()
    {
        List<string> notifyPropertyChangedEvents = [];
        List<DuplicatedSetViewModel> duplicatedSetViewModelInstances = [];

        _duplicatedSetViewModel!.PropertyChanged += delegate(object? sender, PropertyChangedEventArgs e)
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
        Visibility expectedVisible)
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
                Assert.That(_duplicatedSetViewModel![i].Visible, Is.EqualTo(expectedDuplicatedSetViewModel[i].Visible));
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
        Visibility expectedVisible)
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
                Assert.That(duplicatedSetViewModelInstance[i].Visible, Is.EqualTo(expectedDuplicatedSetViewModel[i].Visible));
                AssertAssetPropertyValidity(duplicatedSetViewModelInstance[i].Asset, expectedDuplicatedSetViewModel[i].Asset);

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
        Visibility expectedVisible)
    {
        Assert.That(duplicatedSetViewModel.FileName, Is.EqualTo(expectedDuplicatedSetViewModel.FileName));
        Assert.That(duplicatedSetViewModel.FileName, Is.EqualTo(expectedDuplicatedSetViewModel[0].Asset.FileName));
        Assert.That(duplicatedSetViewModel.FileName, Is.EqualTo(expectedFileName));

        Assert.That(duplicatedSetViewModel.DuplicatesCount, Is.EqualTo(expectedDuplicatedSetViewModel.DuplicatesCount));
        Assert.That(duplicatedSetViewModel.DuplicatesCount, Is.EqualTo(expectedDuplicatesCount));

        Assert.That(duplicatedSetViewModel.Visible, Is.EqualTo(expectedVisible));
    }

    private static void CheckInstance(
        List<DuplicatedSetViewModel> duplicatedSetViewModelInstances,
        DuplicatedSetViewModel expectedDuplicatedSetViewModel,
        string? expectedFileName,
        int expectedDuplicatesCount,
        Visibility expectedVisible)
    {
        int duplicatedSetViewModelInstancesCount = duplicatedSetViewModelInstances.Count;

        if (duplicatedSetViewModelInstancesCount > 1)
        {
            Assert.That(duplicatedSetViewModelInstances[duplicatedSetViewModelInstancesCount - 2], Is.EqualTo(duplicatedSetViewModelInstances[0]));
            // No need to go deeper, same instance because ref updated each time
            Assert.That(duplicatedSetViewModelInstances[duplicatedSetViewModelInstancesCount - 1], Is.EqualTo(duplicatedSetViewModelInstances[duplicatedSetViewModelInstancesCount - 2]));
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
