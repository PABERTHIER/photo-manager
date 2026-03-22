using System.IO.Compression;
using Directories = PhotoManager.Tests.Integration.Constants.Directories;
using Tables = PhotoManager.Tests.Integration.Constants.Tables;

namespace PhotoManager.Tests.Integration.Infrastructure.Database.DatabaseTests;

[TestFixture]
public class DatabaseWriteBackupTests
{
    private string? _dataDirectory;

    private PhotoManager.Infrastructure.Database.Database? _database;
    private UserConfigurationService? _userConfigurationService;
    private TestLogger<PhotoManager.Infrastructure.Database.Database>? _testLogger;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);

        IConfigurationRoot configurationRootMock = Substitute.For<IConfigurationRoot>();
        configurationRootMock.GetDefaultMockConfig();

        _userConfigurationService = new(configurationRootMock);
    }

    [SetUp]
    public void SetUp()
    {
        _testLogger = new();
        _database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage(), _testLogger);
    }

    [TearDown]
    public void TearDown()
    {
        _testLogger!.LoggingAssertTearDown();
    }

    [Test]
    public void WriteBackup_ZipContainsAllExpectedEntriesAndCorrectContent_WritesBackup()
    {
        DateTime backupDate = new(2023, 08, 19);
        string backupName = backupDate.ToString("yyyyMMdd") + ".zip";

        string sourcePath = Path.Combine(_dataDirectory!, Directories.TEST_BACKUP, Constants.DATABASE_END_PATH);
        string directoryPath = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS);
        string backupFilePath = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS, backupName);
        string extractPath = Path.Combine(_dataDirectory!, "DatabaseTests_Extracted");

        try
        {
            CopyDirectory(sourcePath, directoryPath);

            _database!.Initialize(
                directoryPath,
                _userConfigurationService!.StorageSettings.Separator,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);

            bool backupCreated = _database!.WriteBackup(backupDate);

            Assert.That(backupCreated, Is.True);
            Assert.That(File.Exists(backupFilePath), Is.True);

            using (ZipArchive zipArchive = ZipFile.OpenRead(backupFilePath))
            {
                string[] entryNames = [.. zipArchive.Entries.Select(e => e.FullName)];

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(entryNames, Has.Member($"{Directories.TABLES}/{Tables.ASSETS_DB}"));
                    Assert.That(entryNames, Has.Member($"{Directories.TABLES}/{Tables.FOLDERS_DB}"));
                    Assert.That(entryNames, Has.Member($"{Directories.TABLES}/{Tables.RECENT_TARGET_PATHS_DB}"));
                    Assert.That(entryNames,
                        Has.Member($"{Directories.TABLES}/{Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_DB}"));
                    Assert.That(entryNames,
                        Has.Member($"{Directories.TABLES}/{Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_EMPTY_DB}"));
                    Assert.That(entryNames, Has.Member($"{Directories.BLOBS}/{Constants.Blobs.BLOB_18033543}"));
                    Assert.That(entryNames, Has.Member($"{Directories.BLOBS}/{Constants.Blobs.BLOB_F1F00403}"));
                }
            }

            ZipFile.ExtractToDirectory(backupFilePath, extractPath);

            ObjectListStorage objectListStorage = new();
            BlobStorage blobStorage = new();

            AssertFoldersContent(objectListStorage, extractPath);
            AssertAssetsContent(objectListStorage, extractPath);
            AssertRecentTargetPathsContent(objectListStorage, extractPath);
            AssertSyncDefinitionsContent(objectListStorage, extractPath);
            AssertSyncDefinitionsEmptyContent(objectListStorage, extractPath);
            AssertBlobContent(blobStorage, extractPath, Constants.Blobs.BLOB_18033543, 4, "Image_12.heic");
            AssertBlobContent(blobStorage, extractPath, Constants.Blobs.BLOB_F1F00403, 7, "1336.JPG");

            _testLogger!.AssertLogExceptions([], typeof(PhotoManager.Infrastructure.Database.Database));
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS), true);

            if (Directory.Exists(extractPath))
            {
                Directory.Delete(extractPath, true);
            }
        }
    }

    [Test]
    public void WriteBackup_ZipEntryContentMatchesSourceFiles_WritesBackup()
    {
        DateTime backupDate = new(2023, 08, 19);
        string backupName = backupDate.ToString("yyyyMMdd") + ".zip";

        string sourcePath = Path.Combine(_dataDirectory!, Directories.TEST_BACKUP, Constants.DATABASE_END_PATH);
        string directoryPath = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS);
        string backupFilePath = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS, backupName);

        try
        {
            CopyDirectory(sourcePath, directoryPath);

            _database!.Initialize(
                directoryPath,
                _userConfigurationService!.StorageSettings.Separator,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);

            _database!.WriteBackup(backupDate);

            using (ZipArchive zipArchive = ZipFile.OpenRead(backupFilePath))
            {
                using (Assert.EnterMultipleScope())
                {
                    AssertZipEntryContentMatchesSourceFile(zipArchive,
                        $"{Directories.TABLES}/{Tables.ASSETS_DB}",
                        Path.Combine(directoryPath, Directories.TABLES, Tables.ASSETS_DB));

                    AssertZipEntryContentMatchesSourceFile(zipArchive,
                        $"{Directories.TABLES}/{Tables.FOLDERS_DB}",
                        Path.Combine(directoryPath, Directories.TABLES, Tables.FOLDERS_DB));

                    AssertZipEntryContentMatchesSourceFile(zipArchive,
                        $"{Directories.TABLES}/{Tables.RECENT_TARGET_PATHS_DB}",
                        Path.Combine(directoryPath, Directories.TABLES, Tables.RECENT_TARGET_PATHS_DB));

                    AssertZipEntryContentMatchesSourceFile(zipArchive,
                        $"{Directories.TABLES}/{Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_DB}",
                        Path.Combine(directoryPath, Directories.TABLES, Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_DB));

                    AssertZipEntryContentMatchesSourceFile(zipArchive,
                        $"{Directories.TABLES}/{Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_EMPTY_DB}",
                        Path.Combine(directoryPath, Directories.TABLES,
                            Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_EMPTY_DB));

                    AssertZipEntryContentMatchesSourceFile(zipArchive,
                        $"{Directories.BLOBS}/{Constants.Blobs.BLOB_18033543}",
                        Path.Combine(directoryPath, Directories.BLOBS, Constants.Blobs.BLOB_18033543));

                    AssertZipEntryContentMatchesSourceFile(zipArchive,
                        $"{Directories.BLOBS}/{Constants.Blobs.BLOB_F1F00403}",
                        Path.Combine(directoryPath, Directories.BLOBS, Constants.Blobs.BLOB_F1F00403));
                }

                _testLogger!.AssertLogExceptions([], typeof(PhotoManager.Infrastructure.Database.Database));
            }
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS), true);
        }
    }

    private void AssertFoldersContent(ObjectListStorage objectListStorage, string extractPath)
    {
        DataTableProperties foldersTableProps = new()
        {
            TableName = _userConfigurationService!.StorageSettings.TablesSettings.FoldersTableName,
            ColumnProperties = FolderConfigs.ConfigureDataTable()
        };
        objectListStorage.Initialize(foldersTableProps, _userConfigurationService!.StorageSettings.Separator);

        List<Folder> folders = objectListStorage.ReadObjectList(
            Path.Combine(extractPath, Directories.TABLES, Tables.FOLDERS_DB), FolderConfigs.ReadFunc, new());

        using (Assert.EnterMultipleScope())
        {
            Assert.That(folders, Has.Count.EqualTo(3));

            Folder? folder1 = folders.FirstOrDefault(f => f.Id == new Guid("f1f00403-0554-4201-9b6b-11a6b4cea3a9"));
            Assert.That(folder1, Is.Not.Null);
            Assert.That(folder1!.Path, Is.EqualTo("D:\\Workspace\\PhotoManager\\Test"));

            Folder? folder2 = folders.FirstOrDefault(f => f.Id == new Guid("2c107211-1a1c-4e73-8e8b-35d18ca8ef85"));
            Assert.That(folder2, Is.Not.Null);
            Assert.That(folder2!.Path, Is.EqualTo("D:\\Workspace\\PhotoManager\\Test\\OutputVideoFirstFrame"));

            Folder? folder3 = folders.FirstOrDefault(f => f.Id == new Guid("18033543-defb-4d37-837b-d8063eda3a25"));
            Assert.That(folder3, Is.Not.Null);
            Assert.That(folder3!.Path, Is.EqualTo("D:\\Workspace\\PhotoManager\\Test\\toto"));
        }
    }

    private void AssertAssetsContent(ObjectListStorage objectListStorage, string extractPath)
    {
        DataTableProperties assetsTableProps = new()
        {
            TableName = _userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName,
            ColumnProperties = AssetConfigs.ConfigureDataTable()
        };
        objectListStorage.Initialize(assetsTableProps, _userConfigurationService!.StorageSettings.Separator);

        List<Asset> assets = objectListStorage.ReadObjectList(
            Path.Combine(extractPath, Directories.TABLES, Tables.ASSETS_DB), AssetConfigs.ReadFunc, new());

        using (Assert.EnterMultipleScope())
        {
            Assert.That(assets, Has.Count.EqualTo(11));

            Asset? asset = assets.FirstOrDefault(a =>
                a.Hash ==
                "ee43714d8b96d7ed3308d18afcb701444198c783fbe4103ce44e95aaf99c2095ae70e6e2035a7a438d1598fadaf5fe8cb0d541378387d20e91f26819fcc64b82");
            Assert.That(asset, Is.Not.Null);
            Assert.That(asset!.FileName, Is.EqualTo("533.JPG"));
            Assert.That(asset.FolderId, Is.EqualTo(new Guid("f1f00403-0554-4201-9b6b-11a6b4cea3a9")));
            Assert.That(asset.ImageRotation, Is.EqualTo(Rotation.Rotate270));
        }
    }

    private void AssertRecentTargetPathsContent(ObjectListStorage objectListStorage, string extractPath)
    {
        DataTableProperties recentPathsTableProps = new()
        {
            TableName = _userConfigurationService!.StorageSettings.TablesSettings.RecentTargetPathsTableName,
            ColumnProperties = RecentPathsConfigs.ConfigureDataTable()
        };
        objectListStorage.Initialize(recentPathsTableProps, _userConfigurationService!.StorageSettings.Separator);

        List<string> recentPaths = objectListStorage.ReadObjectList(
            Path.Combine(extractPath, Directories.TABLES, Tables.RECENT_TARGET_PATHS_DB),
            RecentPathsConfigs.ReadFunc, new());

        using (Assert.EnterMultipleScope())
        {
            Assert.That(recentPaths, Has.Count.EqualTo(2));
            Assert.That(recentPaths, Has.Member("D:\\Workspace\\PhotoManager\\Toto"));
            Assert.That(recentPaths, Has.Member("D:\\Workspace\\PhotoManager\\Tutu"));
        }
    }

    private void AssertSyncDefinitionsContent(ObjectListStorage objectListStorage, string extractPath)
    {
        DataTableProperties syncTableProps = new()
        {
            TableName = _userConfigurationService!.StorageSettings.TablesSettings
                .SyncAssetsDirectoriesDefinitionsTableName,
            ColumnProperties = SyncAssetsDirectoriesDefinitionConfigs.ConfigureDataTable()
        };
        objectListStorage.Initialize(syncTableProps, _userConfigurationService!.StorageSettings.Separator);

        List<SyncAssetsDirectoriesDefinition> syncDefs = objectListStorage.ReadObjectList(
            Path.Combine(extractPath, Directories.TABLES, Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_DB),
            SyncAssetsDirectoriesDefinitionConfigs.ReadFunc, new());

        using (Assert.EnterMultipleScope())
        {
            Assert.That(syncDefs, Has.Count.EqualTo(2));

            SyncAssetsDirectoriesDefinition? syncAssetsDirectoriesDefinition1 =
                syncDefs.FirstOrDefault(d => d.SourceDirectory == "C:\\Toto\\Screenshots");
            Assert.That(syncAssetsDirectoriesDefinition1, Is.Not.Null);
            Assert.That(syncAssetsDirectoriesDefinition1!.DestinationDirectory, Is.EqualTo("C:\\Images\\Toto"));
            Assert.That(syncAssetsDirectoriesDefinition1.IncludeSubFolders, Is.False);
            Assert.That(syncAssetsDirectoriesDefinition1.DeleteAssetsNotInSource, Is.False);

            SyncAssetsDirectoriesDefinition? syncAssetsDirectoriesDefinition2 =
                syncDefs.FirstOrDefault(d => d.SourceDirectory == "C:\\Tutu\\Screenshots");
            Assert.That(syncAssetsDirectoriesDefinition2, Is.Not.Null);
            Assert.That(syncAssetsDirectoriesDefinition2!.DestinationDirectory, Is.EqualTo("C:\\Images\\Tutu"));
        }
    }

    private static void AssertSyncDefinitionsEmptyContent(ObjectListStorage objectListStorage, string extractPath)
    {
        List<SyncAssetsDirectoriesDefinition> emptySyncDefs = objectListStorage.ReadObjectList(
            Path.Combine(extractPath, Directories.TABLES, Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_EMPTY_DB),
            SyncAssetsDirectoriesDefinitionConfigs.ReadFunc, new());

        Assert.That(emptySyncDefs, Is.Empty);
    }

    private static void AssertBlobContent(
        BlobStorage blobStorage,
        string extractPath,
        string blobFileName,
        int expectedCount,
        string expectedKey)
    {
        Dictionary<string, byte[]>? blob = blobStorage.ReadFromBinaryFile(
            Path.Combine(extractPath, Directories.BLOBS, blobFileName));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(blob, Is.Not.Null);
            Assert.That(blob!, Has.Count.EqualTo(expectedCount));
            Assert.That(blob!.ContainsKey(expectedKey), Is.True);
        }
    }

    private static void AssertZipEntryContentMatchesSourceFile(
        ZipArchive zipArchive,
        string entryName,
        string sourceFilePath)
    {
        ZipArchiveEntry? entry = zipArchive.GetEntry(entryName);
        Assert.That(entry, Is.Not.Null, $"Entry '{entryName}' not found in zip archive.");

        using (Stream entryStream = entry!.Open())
        {
            using (MemoryStream entryContent = new())
            {
                entryStream.CopyTo(entryContent);

                byte[] expectedContent = File.ReadAllBytes(sourceFilePath);

                Assert.That(entryContent.ToArray(), Is.EqualTo(expectedContent));
            }
        }
    }

    private static void CopyDirectory(string sourceDir, string targetDir)
    {
        Directory.CreateDirectory(targetDir);

        foreach (string file in Directory.GetFiles(sourceDir))
        {
            File.Copy(file, Path.Combine(targetDir, Path.GetFileName(file)), true);
        }

        foreach (string subDir in Directory.GetDirectories(sourceDir))
        {
            CopyDirectory(subDir, Path.Combine(targetDir, Path.GetFileName(subDir)));
        }
    }
}
