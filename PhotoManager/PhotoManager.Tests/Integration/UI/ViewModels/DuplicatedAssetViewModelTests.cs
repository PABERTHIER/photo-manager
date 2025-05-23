﻿using System.ComponentModel;
using System.Windows;

namespace PhotoManager.Tests.Integration.UI.ViewModels;

[TestFixture]
public class DuplicatedAssetViewModelTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;
    private readonly DateTime _expectedFileModificationDateTime = new (2024, 06, 07, 08, 54, 37);
    private const string DATABASE_END_PATH = "v1.0";

    private DuplicatedAssetViewModel? _duplicatedAssetViewModel;
    private AssetRepository? _assetRepository;

    private Asset _asset1;
    private Asset _asset2;
    private Asset _asset3;
    private Asset _asset4;

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

        _asset1 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "Image 1_duplicate.jpg",
            Pixel = new()
            {
                Asset = new() { Width = 1280, Height = 720 },
                Thumbnail = new() { Width = 200, Height = 112 }
            },
            FileProperties = new()
            {
                Size = 29857,
                Creation = actualDate,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9",
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
        _asset4 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "Image_11.heic",
            Pixel = new()
            {
                Asset = new() { Width = 3024, Height = 4032 },
                Thumbnail = new() { Width = 112, Height = 150 }
            },
            FileProperties = new()
            {
                Size = 1411940,
                Creation = actualDate,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = "f52bd860f5ad7f81a92919e5fb5769d3e86778b2ade74832fbd3029435c85e59cb64b3c2ce425445a49917953e6e913c72b81e48976041a4439cb65e92baf18d",
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
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder2");

        try
        {
            Folder folder = _assetRepository!.AddFolder(assetsDirectory);

            _asset3 = _asset3.WithFolder(folder);

            DuplicatedAssetViewModel duplicatedAssetViewModel1 = new() { Asset = _asset2 };
            DuplicatedAssetViewModel duplicatedAssetViewModel2 = new() { Asset = _asset3 };

            _duplicatedAssetViewModel = new()
            {
                Asset = _asset3,
                ParentViewModel = [ duplicatedAssetViewModel1, duplicatedAssetViewModel2 ]
            };

            (
                List<string> notifyPropertyChangedEvents,
                List<DuplicatedAssetViewModel> duplicatedAssetViewModelInstances
            ) = NotifyPropertyChangedEvents();

            CheckBeforeChanges(
                _asset3,
                Visibility.Visible,
                [duplicatedAssetViewModel1, duplicatedAssetViewModel2],
                _asset2.FileName,
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
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder2");

        try
        {
            Folder folder = _assetRepository!.AddFolder(assetsDirectory);

            _asset1 = _asset1.WithFolder(folder);

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
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder2");

        try
        {
            Folder folder = _assetRepository!.AddFolder(assetsDirectory);

            _asset4 = _asset4.WithFolder(folder);

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

        _duplicatedAssetViewModel!.PropertyChanged += delegate(object? sender, PropertyChangedEventArgs e)
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
