using Microsoft.Extensions.Configuration;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryAddFolderTests
{
    private string? dataDirectory;
    private const string backupEndPath = "DatabaseTests\\v1.0";
    private string? backupPath;
    private readonly Guid defaultGuid = Guid.Empty;

    private IAssetRepository? _assetRepository;
    private Mock<IStorageService>? _storageServiceMock;
    private Mock<IConfigurationRoot>? _configurationRootMock;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        backupPath = Path.Combine(dataDirectory, backupEndPath);

        _configurationRootMock = new Mock<IConfigurationRoot>();
        _configurationRootMock.GetDefaultMockConfig();

        _storageServiceMock = new Mock<IStorageService>();
        _storageServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<double>())).Returns(backupPath);
    }

    [SetUp]
    public void Setup()
    {
        PhotoManager.Infrastructure.Database.Database database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        UserConfigurationService userConfigurationService = new(_configurationRootMock!.Object);
        _assetRepository = new AssetRepository(database, _storageServiceMock!.Object, userConfigurationService);
    }

    [Test]
    public void AddFolder_DifferentPaths_AddsFoldersToList()
    {
        try
        {
            string folderPath1 = Path.Combine(dataDirectory!, "TestFolder1");
            string folderPath2 = Path.Combine(dataDirectory!, "TestFolder2");

            Folder addedFolder1 = _assetRepository!.AddFolder(folderPath1);
            Folder addedFolder2 = _assetRepository!.AddFolder(folderPath2);

            Assert.AreEqual(folderPath1, addedFolder1.Path);
            Assert.IsFalse(addedFolder1.FolderId == defaultGuid);

            Assert.AreEqual(folderPath2, addedFolder2.Path);
            Assert.IsFalse(addedFolder2.FolderId == defaultGuid);

            Assert.AreNotEqual(addedFolder1.Path, addedFolder2.Path);
            Assert.AreNotEqual(addedFolder1.FolderId, addedFolder2.FolderId);

            Assert.IsTrue(_assetRepository!.HasChanges());

            Folder? folderByPath1 = _assetRepository!.GetFolderByPath(folderPath1);
            Folder? folderByPath2 = _assetRepository!.GetFolderByPath(folderPath2);

            Assert.IsNotNull(folderByPath1);
            Assert.IsNotNull(folderByPath2);

            Assert.AreEqual(folderPath1, folderByPath1!.Path);
            Assert.AreEqual(addedFolder1.FolderId, folderByPath1.FolderId);

            Assert.AreEqual(folderPath2, folderByPath2!.Path);
            Assert.AreEqual(addedFolder2.FolderId, folderByPath2.FolderId);

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
    public void AddFolder_SamePath_AddsFoldersToList()
    {
        try
        {
            string folderPath1 = Path.Combine(dataDirectory!, "TestFolder1");

            Folder addedFolder1 = _assetRepository!.AddFolder(folderPath1);
            Folder addedFolder2 = _assetRepository!.AddFolder(folderPath1);

            Assert.AreEqual(folderPath1, addedFolder1.Path);
            Assert.IsFalse(addedFolder1.FolderId == defaultGuid);

            Assert.AreEqual(folderPath1, addedFolder2.Path);
            Assert.IsFalse(addedFolder2.FolderId == defaultGuid);

            Assert.AreEqual(addedFolder1.Path, addedFolder2.Path);
            Assert.AreNotEqual(addedFolder1.FolderId, addedFolder2.FolderId);

            Assert.IsTrue(_assetRepository!.HasChanges());

            Folder? folderByPath1 = _assetRepository!.GetFolderByPath(folderPath1);
            Folder? folderByPath2 = _assetRepository!.GetFolderByPath(folderPath1);

            Assert.IsNotNull(folderByPath1);
            Assert.IsNotNull(folderByPath2);

            Assert.AreEqual(folderPath1, folderByPath1!.Path);
            Assert.AreEqual(addedFolder1.FolderId, folderByPath1.FolderId);

            Assert.AreEqual(folderPath1, folderByPath2!.Path);
            Assert.AreNotEqual(addedFolder2.FolderId, folderByPath2.FolderId);

            Folder[] folders = _assetRepository!.GetFolders();

            Assert.AreEqual(2, folders.Length);
            Assert.AreEqual(folderPath1, folders[0].Path);
            Assert.AreEqual(addedFolder1.FolderId, folders[0].FolderId);
            Assert.AreEqual(folderPath1, folders[1].Path);
            Assert.AreEqual(addedFolder2.FolderId, folders[1].FolderId);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void AddFolder_ConcurrentAccess_FoldersAreHandledSafely()
    {
        try
        {
            string folderPath1 = Path.Combine(dataDirectory!, "TestFolder1");
            string folderPath2 = Path.Combine(dataDirectory!, "TestFolder2");

            Folder addedFolder1 = new() { Path = dataDirectory! };
            Folder addedFolder2 = new() { Path = dataDirectory! };

            // Simulate concurrent access
            Parallel.Invoke(
                () => addedFolder1 = _assetRepository!.AddFolder(folderPath1),
                () => addedFolder2 = _assetRepository!.AddFolder(folderPath2)
            );

            Assert.AreEqual(folderPath1, addedFolder1.Path);
            Assert.IsFalse(addedFolder1.FolderId == defaultGuid);

            Assert.AreEqual(folderPath2, addedFolder2.Path);
            Assert.IsFalse(addedFolder2.FolderId == defaultGuid);

            Assert.AreNotEqual(addedFolder1.Path, addedFolder2.Path);
            Assert.AreNotEqual(addedFolder1.FolderId, addedFolder2.FolderId);

            Assert.IsTrue(_assetRepository!.HasChanges());

            Folder? folderByPath1 = _assetRepository!.GetFolderByPath(folderPath1);
            Folder? folderByPath2 = _assetRepository!.GetFolderByPath(folderPath2);

            Assert.IsNotNull(folderByPath1);
            Assert.IsNotNull(folderByPath2);

            Assert.AreEqual(folderPath1, folderByPath1!.Path);
            Assert.AreEqual(addedFolder1.FolderId, folderByPath1.FolderId);

            Assert.AreEqual(folderPath2, folderByPath2!.Path);
            Assert.AreEqual(addedFolder2.FolderId, folderByPath2.FolderId);

            Folder[] folders = _assetRepository!.GetFolders();

            Assert.AreEqual(2, folders.Length);
            Folder? folder1 = folders.FirstOrDefault(x => x.Path == folderPath1);
            Folder? folder2 = folders.FirstOrDefault(x => x.Path == folderPath2);

            Assert.IsNotNull(folder1);
            Assert.IsNotNull(folder2);

            Assert.AreEqual(addedFolder1.FolderId, folder1!.FolderId);
            Assert.AreEqual(addedFolder2.FolderId, folder2!.FolderId);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }
}
