﻿using Reactive = System.Reactive;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryDeleteFolderTests
{
    private string? _dataDirectory;
    private string? _backupPath;
    private readonly DateTime _expectedFileModificationDateTime = new (2024, 06, 07, 08, 54, 37);
    private const string BACKUP_END_PATH = "DatabaseTests\\v1.0";

    private TestableAssetRepository? _testableAssetRepository;
    private PhotoManager.Infrastructure.Database.Database? _database;
    private UserConfigurationService? _userConfigurationService;
    private Mock<IStorageService>? _storageServiceMock;
    private Mock<IConfigurationRoot>? _configurationRootMock;

    private Asset? _asset1;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        _backupPath = Path.Combine(_dataDirectory, BACKUP_END_PATH);

        _configurationRootMock = new Mock<IConfigurationRoot>();
        _configurationRootMock.GetDefaultMockConfig();

        _storageServiceMock = new Mock<IStorageService>();
        _storageServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_backupPath);
    }

    [SetUp]
    public void SetUp()
    {
        _database = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        _userConfigurationService = new (_configurationRootMock!.Object);
        _testableAssetRepository = new (_database, _storageServiceMock!.Object, _userConfigurationService);

        _asset1 = new()
        {
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FolderId = new Guid("876283c6-780e-4ad5-975c-be63044c087a"),
            FileName = "Image 1.jpg",
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = 1920, Height = 1080 },
                Thumbnail = new() { Width = 200, Height = 112 }
            },
            FileProperties = new()
            {
                Size = 363888,
                Creation = DateTime.Now,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = DateTime.Now,
            Hash = "4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4",
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
    }

    [Test]
    public void DeleteFolder_FolderIsNull_ThrowsNullReferenceException()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            Folder? folder = null;

            NullReferenceException? exception = Assert.Throws<NullReferenceException>(() => _testableAssetRepository!.DeleteFolder(folder!));

            Assert.That(exception?.Message, Is.EqualTo("Object reference not set to an instance of an object."));

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void DeleteFolder_FolderExistsWithThumbnails_RemovesThumbnailsAndFolder()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        string folderPath = Path.Combine(_dataDirectory!, "NewFolder");

        try
        {
            Directory.CreateDirectory(folderPath);
            Folder folder = _testableAssetRepository!.AddFolder(folderPath);

            _asset1 = _asset1!.WithFolder(folder);

            byte[] assetData = [];

            _testableAssetRepository!.AddAsset(_asset1!, assetData);

            Assert.That(_testableAssetRepository.HasChanges(), Is.True);

            bool isContainingThumbnail = _testableAssetRepository!.ContainsThumbnail(_asset1!.Folder.Path, _asset1!.FileName);

            Assert.That(isContainingThumbnail, Is.True);

            _testableAssetRepository.SaveCatalog(_asset1!.Folder); // SaveThumbnails, write blob file, reset hasChanges to false

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(_asset1!.Folder.Path), Is.True);
            Assert.That(thumbnails[_asset1!.Folder.Path], Has.Count.EqualTo(1));
            Assert.That(thumbnails[_asset1!.Folder.Path].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[_asset1!.Folder.Path][_asset1.FileName], Is.EqualTo(assetData));

            _testableAssetRepository!.DeleteFolder(_asset1!.Folder);

            Folder[] folders = _testableAssetRepository!.GetFolders();

            Assert.That(thumbnails.ContainsKey(_asset1!.Folder.Path), Is.False);
            Assert.That(folders.Contains(_asset1!.Folder), Is.False);
            Assert.That(_testableAssetRepository.HasChanges(), Is.True);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            Directory.Delete(folderPath, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void DeleteFolder_FolderExistsNoThumbnails_RemovesOnlyFolder()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        string folderPath = Path.Combine(_dataDirectory!, "NewFolder");

        try
        {
            Directory.CreateDirectory(folderPath);
            Folder folder = _testableAssetRepository!.AddFolder(folderPath);

            _asset1 = _asset1!.WithFolder(folder);

            Assert.That(_testableAssetRepository.HasChanges(), Is.True);

            _testableAssetRepository.SaveCatalog(_asset1!.Folder); // SaveThumbnails, write blob file, reset hasChanges to false

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            Assert.That(assetsUpdatedEvents, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);
            Assert.That(thumbnails.ContainsKey(_asset1!.Folder.Path), Is.False);

            _testableAssetRepository!.DeleteFolder(_asset1!.Folder);

            Folder[] folders = _testableAssetRepository!.GetFolders();

            Assert.That(thumbnails.ContainsKey(_asset1!.Folder.Path), Is.False);
            Assert.That(folders.Contains(_asset1!.Folder), Is.False);
            Assert.That(_testableAssetRepository.HasChanges(), Is.True);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            Directory.Delete(folderPath, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void DeleteFolder_FolderExistsNoThumbnailsButCreated_RemovesFolderAndThumbnail()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        string folderPath = Path.Combine(_dataDirectory!, "NewFolder");

        try
        {
            Directory.CreateDirectory(folderPath);
            Folder folder = _testableAssetRepository!.AddFolder(folderPath);

            _asset1 = _asset1!.WithFolder(folder);

            Assert.That(_testableAssetRepository.HasChanges(), Is.True);

            bool isContainingThumbnail = _testableAssetRepository!.ContainsThumbnail(_asset1!.Folder.Path, _asset1!.FileName);

            Assert.That(isContainingThumbnail, Is.False);

            _testableAssetRepository.SaveCatalog(_asset1!.Folder); // SaveThumbnails, write blob file, reset hasChanges to false

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            Assert.That(assetsUpdatedEvents, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(_asset1!.Folder.Path), Is.True);
            Assert.That(thumbnails[_asset1!.Folder.Path], Is.Empty);

            _testableAssetRepository!.DeleteFolder(_asset1!.Folder);

            Folder[] folders = _testableAssetRepository!.GetFolders();

            Assert.That(thumbnails.ContainsKey(_asset1!.Folder.Path), Is.False);
            Assert.That(folders.Contains(_asset1!.Folder), Is.False);
            Assert.That(_testableAssetRepository.HasChanges(), Is.True);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            Directory.Delete(folderPath, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void DeleteFolder_FolderExistsNoThumbnailsButBinExists_RemovesFolderAndBinFile()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        string folderPath = Path.Combine(_dataDirectory!, "NewFolder");

        try
        {
            Directory.CreateDirectory(folderPath);
            Folder folder = _testableAssetRepository!.AddFolder(folderPath);

            _asset1 = _asset1!.WithFolder(folder);

            Assert.That(_testableAssetRepository.HasChanges(), Is.True);

            _testableAssetRepository.SaveCatalog(_asset1!.Folder); // SaveThumbnails, write blob file, reset hasChanges to false

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            Assert.That(assetsUpdatedEvents, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);
            Assert.That(thumbnails.ContainsKey(_asset1!.Folder.Path), Is.False);

            _database!.WriteBlob([], _asset1!.Folder.ThumbnailsFilename);

            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, _asset1.Folder.ThumbnailsFilename)), Is.True);

            _testableAssetRepository!.DeleteFolder(_asset1!.Folder);

            Assert.That(File.Exists(Path.Combine(_backupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, _asset1.Folder.ThumbnailsFilename)), Is.False);

            Folder[] folders = _testableAssetRepository!.GetFolders();

            Assert.That(thumbnails.ContainsKey(_asset1!.Folder.Path), Is.False);
            Assert.That(folders.Contains(_asset1!.Folder), Is.False);
            Assert.That(_testableAssetRepository.HasChanges(), Is.True);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            Directory.Delete(folderPath, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void DeleteFolder_FolderDoesNotExist_RemovesNothing()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        string folderPath = Path.Combine(_dataDirectory!, "NewFolder");

        try
        {
            Directory.CreateDirectory(folderPath);

            Folder folder = new()
            {
                Path = folderPath,
                Id = Guid.NewGuid(),
            };

            _testableAssetRepository!.DeleteFolder(folder);

            Folder[] folders = _testableAssetRepository!.GetFolders();

            Assert.That(folders.Contains(folder), Is.False);
            Assert.That(_testableAssetRepository.HasChanges(), Is.True);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            Directory.Delete(folderPath, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void DeleteFolder_ConcurrentAccess_ThumbnailsAndFoldersAreHandledSafely()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        string folderPath = Path.Combine(_dataDirectory!, "NewFolder");

        try
        {
            Directory.CreateDirectory(folderPath);
            Folder folder = _testableAssetRepository!.AddFolder(folderPath);

            _asset1 = _asset1!.WithFolder(folder);

            byte[] assetData = [];

            _testableAssetRepository!.AddAsset(_asset1!, assetData);

            Assert.That(_testableAssetRepository.HasChanges(), Is.True);

            bool isContainingThumbnail = _testableAssetRepository!.ContainsThumbnail(_asset1!.Folder.Path, _asset1!.FileName);

            Assert.That(isContainingThumbnail, Is.True);

            _testableAssetRepository.SaveCatalog(_asset1!.Folder); // SaveThumbnails, write blob file, reset hasChanges to false

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(_asset1!.Folder.Path), Is.True);
            Assert.That(thumbnails[_asset1!.Folder.Path], Has.Count.EqualTo(1));
            Assert.That(thumbnails[_asset1!.Folder.Path].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[_asset1!.Folder.Path][_asset1.FileName], Is.EqualTo(assetData));

            // Simulate concurrent access
            Parallel.Invoke(
                () => _testableAssetRepository!.DeleteFolder(_asset1!.Folder),
                () => _testableAssetRepository!.DeleteFolder(_asset1!.Folder),
                () => _testableAssetRepository!.DeleteFolder(_asset1!.Folder)
            );

            Folder[] folders = _testableAssetRepository!.GetFolders();

            Assert.That(thumbnails.ContainsKey(_asset1!.Folder.Path), Is.False);
            Assert.That(folders.Contains(_asset1!.Folder), Is.False);
            Assert.That(_testableAssetRepository.HasChanges(), Is.True);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            Directory.Delete(folderPath, true);
            assetsUpdatedSubscription.Dispose();
        }
    }
}
