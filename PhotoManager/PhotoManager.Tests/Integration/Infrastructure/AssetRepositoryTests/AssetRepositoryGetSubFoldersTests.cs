using Microsoft.Extensions.Configuration;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryGetSubFoldersTests
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
    public void GetSubFolders_ParentHasSubFolders_ReturnsMatchingSubFolders()
    {
        try
        {
            string parentFolderPath1 = Path.Combine(dataDirectory!, "TestFolder1");
            string parentFolderPath2 = Path.Combine(dataDirectory!, "TestFolder2");

            string childFolderPath1 = Path.Combine(parentFolderPath1, "TestSubFolder1");
            string childFolderPath2 = Path.Combine(parentFolderPath2, "TestSubFolder2");
            string childFolderPath3 = Path.Combine(parentFolderPath2, "TestSubFolder2");

            Folder parentFolder1 = _assetRepository!.AddFolder(parentFolderPath1);
            Folder parentFolder2 = _assetRepository!.AddFolder(parentFolderPath2);

            Folder childFolder1 = _assetRepository!.AddFolder(childFolderPath1);
            Folder childFolder2 = _assetRepository!.AddFolder(childFolderPath2);
            Folder childFolder3 = _assetRepository!.AddFolder(childFolderPath3);

            Folder[] parentFolders1 = _assetRepository!.GetSubFolders(parentFolder1, includeHidden: false);
            Folder[] parentFolders2 = _assetRepository!.GetSubFolders(parentFolder2, includeHidden: false);

            Folder[] childFolders1 = _assetRepository!.GetSubFolders(childFolder1, includeHidden: false);
            Folder[] childFolders2 = _assetRepository!.GetSubFolders(childFolder2, includeHidden: false);
            Folder[] childFolders3 = _assetRepository!.GetSubFolders(childFolder3, includeHidden: false);

            Assert.IsNotEmpty(parentFolders1);
            Assert.AreEqual(1, parentFolders1.Length);
            Assert.AreEqual(childFolderPath1, parentFolders1[0].Path);

            Assert.IsNotEmpty(parentFolders2);
            Assert.AreEqual(2, parentFolders2.Length);
            Assert.AreEqual(childFolderPath2, parentFolders2[0].Path);
            Assert.AreEqual(childFolderPath3, parentFolders2[1].Path);

            Assert.IsEmpty(childFolders1);
            Assert.IsEmpty(childFolders2);
            Assert.IsEmpty(childFolders3);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void GetSubFolders_ParentHasNoSubFolders_ReturnsEmptyArray()
    {
        try
        {
            string parentFolderPath1 = Path.Combine(dataDirectory!, "TestFolder1");
            string parentFolderPath2 = Path.Combine(dataDirectory!, "TestFolder2");

            Folder parentFolder1 = _assetRepository!.AddFolder(parentFolderPath1);
            Folder parentFolder2 = _assetRepository!.AddFolder(parentFolderPath2);

            Folder[] parentFolders1 = _assetRepository!.GetSubFolders(parentFolder1, includeHidden: false);
            Folder[] parentFolders2 = _assetRepository!.GetSubFolders(parentFolder2, includeHidden: false);

            Assert.IsEmpty(parentFolders1);
            Assert.IsEmpty(parentFolders2);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void GetSubFolders_NoFoldersRegistered_ReturnsEmptyArray()
    {
        try
        {
            string parentFolderPath1 = Path.Combine(dataDirectory!, "TestFolder1");
            string parentFolderPath2 = Path.Combine(dataDirectory!, "TestFolder2");

            Folder parentFolder1 = new() { Path = parentFolderPath1 };
            Folder parentFolder2 = new() { Path = parentFolderPath2 };

            Folder[] parentFolders1 = _assetRepository!.GetSubFolders(parentFolder1, includeHidden: false);
            Folder[] parentFolders2 = _assetRepository!.GetSubFolders(parentFolder2, includeHidden: false);

            Assert.IsEmpty(parentFolders1);
            Assert.IsEmpty(parentFolders2);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void GetSubFolders_ParentFolderIsNull_ThrowsArgumentException()
    {
        try
        {
            Folder? parentFolder1 = null;

            string parentFolderPath2 = Path.Combine(dataDirectory!, "TestFolder2");

            _assetRepository!.AddFolder(parentFolderPath2); // At least one folder to trigger the Where on folders

            Assert.Throws<ArgumentException>(() => _assetRepository!.GetSubFolders(parentFolder1!, includeHidden: false));
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }
}
