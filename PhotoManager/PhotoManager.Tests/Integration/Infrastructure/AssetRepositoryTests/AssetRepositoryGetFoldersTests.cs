﻿using Microsoft.Extensions.Configuration;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryGetFoldersTests
{
    private string? dataDirectory;
    private const string backupEndPath = "DatabaseTests\\v1.0";
    private string? backupPath;

    private IAssetRepository? _assetRepository;
    private Mock<IStorageService>? _storageService;
    private Mock<IConfigurationRoot>? _configurationRoot;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        backupPath = Path.Combine(dataDirectory, backupEndPath);

        _configurationRoot = new Mock<IConfigurationRoot>();
        _configurationRoot
            .MockGetValue("appsettings:CatalogBatchSize", "100")
            .MockGetValue("appsettings:CatalogCooldownMinutes", "5")
            .MockGetValue("appsettings:BackupsToKeep", "2")
            .MockGetValue("appsettings:ThumbnailsDictionaryEntriesToKeep", "5");

        _storageService = new Mock<IStorageService>();
        _storageService!.Setup(x => x.ResolveDataDirectory(It.IsAny<double>())).Returns(backupPath);
    }

    [SetUp]
    public void Setup()
    {
        PhotoManager.Infrastructure.Database.Database database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        UserConfigurationService userConfigurationService = new(_configurationRoot!.Object);
        _assetRepository = new AssetRepository(database, _storageService!.Object, userConfigurationService);
    }

    [Test]
    public void GetFolders_Folders_ReturnsCorrectFolders()
    {
        try
        {
            string folderPath1 = Path.Combine(dataDirectory!, "TestFolder1");
            string folderPath2 = Path.Combine(dataDirectory!, "TestFolder2");

            Folder addedFolder1 = _assetRepository!.AddFolder(folderPath1);
            Folder addedFolder2 = _assetRepository!.AddFolder(folderPath2);

            Folder[] folders = _assetRepository!.GetFolders();

            Assert.AreEqual(2, folders.Length);
            Assert.AreEqual(folderPath1, folders[0].Path);
            Assert.AreEqual(addedFolder1.FolderId, folders[0].FolderId);
            Assert.AreEqual(folderPath2, folders[1].Path);
            Assert.AreEqual(addedFolder2.FolderId, folders[1].FolderId);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void GetFolders_NoFolders_ReturnsEmptyArray()
    {
        try
        {
            Folder[] folders = _assetRepository!.GetFolders();

            Assert.IsEmpty(folders);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void GetFolders_ConcurrentAccess_FoldersAreHandledSafely()
    {
        try
        {
            string folderPath1 = Path.Combine(dataDirectory!, "TestFolder1");
            string folderPath2 = Path.Combine(dataDirectory!, "TestFolder2");

            Folder addedFolder1 = _assetRepository!.AddFolder(folderPath1);
            Folder addedFolder2 = _assetRepository!.AddFolder(folderPath2);

            Folder[] folders = _assetRepository!.GetFolders();

            Folder[] folders1 = Array.Empty<Folder>();
            Folder[] folders2 = Array.Empty<Folder>();
            Folder[] folders3 = Array.Empty<Folder>();

            // Simulate concurrent access
            Parallel.Invoke(
                () => folders1 = _assetRepository!.GetFolders(),
                () => folders2 = _assetRepository!.GetFolders(),
                () => folders3 = _assetRepository!.GetFolders()
            );

            Assert.AreEqual(2, folders1.Length);
            Assert.AreEqual(folderPath1, folders1[0].Path);
            Assert.AreEqual(addedFolder1.FolderId, folders1[0].FolderId);
            Assert.AreEqual(folderPath2, folders1[1].Path);
            Assert.AreEqual(addedFolder2.FolderId, folders1[1].FolderId);

            Assert.AreEqual(2, folders2.Length);
            Assert.AreEqual(folderPath1, folders2[0].Path);
            Assert.AreEqual(addedFolder1.FolderId, folders2[0].FolderId);
            Assert.AreEqual(folderPath2, folders2[1].Path);
            Assert.AreEqual(addedFolder2.FolderId, folders2[1].FolderId);

            Assert.AreEqual(2, folders3.Length);
            Assert.AreEqual(folderPath1, folders3[0].Path);
            Assert.AreEqual(addedFolder1.FolderId, folders3[0].FolderId);
            Assert.AreEqual(folderPath2, folders3[1].Path);
            Assert.AreEqual(addedFolder2.FolderId, folders3[1].FolderId);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }
}