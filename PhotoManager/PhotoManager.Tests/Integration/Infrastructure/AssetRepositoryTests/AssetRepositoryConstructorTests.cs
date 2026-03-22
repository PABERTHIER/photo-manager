using Directories = PhotoManager.Tests.Integration.Constants.Directories;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

public class AssetRepositoryConstructorTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;

    private Mock<IPathProviderService>? _pathProviderServiceMock;
    private Mock<IConfigurationRoot>? _configurationRootMock;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_dataDirectory, Directories.DATABASE_TESTS);
        _databasePath = Path.Combine(_databaseDirectory, Constants.DATABASE_END_PATH);

        _configurationRootMock = new();
        _configurationRootMock.GetDefaultMockConfig();

        _pathProviderServiceMock = new();
        _pathProviderServiceMock.Setup(x => x.ResolveDataDirectory()).Returns(_databasePath);
    }

    [Test]
    public void Constructor_ReadCatalogThrowsException_LogsItAndThrowsException()
    {
        try
        {
            // Create the database directory structure
            Directory.CreateDirectory(_databaseDirectory!);
            string tablesDirectory = Path.Combine(_databasePath!, _configurationRootMock!.Object
                .GetValue<string>(UserConfigurationKeys.TABLES_FOLDER_NAME)!);
            Directory.CreateDirectory(tablesDirectory);

            // Create a corrupted Folders.db file with invalid GUID that will cause ReadCatalog to fail
            string foldersFilePath = Path.Combine(tablesDirectory, "folders.db");
            File.WriteAllText(foldersFilePath, "FolderId,Path\ninvalid-guid-value,/test/path");

            TestLogger<AssetRepository> testLogger = new();
            PhotoManager.Infrastructure.Database.Database database = new(new ObjectListStorage(),
                new BlobStorage(), new BackupStorage(),
                new TestLogger<PhotoManager.Infrastructure.Database.Database>());
            UserConfigurationService userConfigurationService = new(_configurationRootMock.Object);
            ImageProcessingService imageProcessingService = new(new TestLogger<ImageProcessingService>());
            FileOperationsService fileOperationsService = new(userConfigurationService,
                new TestLogger<FileOperationsService>());
            ImageMetadataService imageMetadataService = new(fileOperationsService,
                new TestLogger<ImageMetadataService>());

            using (Assert.EnterMultipleScope())
            {
                ArgumentException? exception = Assert.Throws<ArgumentException>(() =>
                    new AssetRepository(database, _pathProviderServiceMock!.Object, imageProcessingService,
                        imageMetadataService, userConfigurationService, testLogger));

                Assert.That(exception?.Message, Does.Contain("Error while trying to read data table"));

                Exception expectedException = new(exception?.Message);
                testLogger.AssertLogExceptions([expectedException], typeof(AssetRepository));
            }
        }
        finally
        {
            if (Directory.Exists(_databaseDirectory!))
            {
                Directory.Delete(_databaseDirectory!, true);
            }
        }
    }
}
