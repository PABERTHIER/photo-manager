using Directories = PhotoManager.Tests.Integration.Constants.Directories;
using FileNames = PhotoManager.Tests.Integration.Constants.FileNames;
using FileSize = PhotoManager.Tests.Integration.Constants.FileSize;
using Hashes = PhotoManager.Tests.Integration.Constants.Hashes;
using PixelHeightAsset = PhotoManager.Tests.Integration.Constants.PixelHeightAsset;
using PixelWidthAsset = PhotoManager.Tests.Integration.Constants.PixelWidthAsset;
using Tables = PhotoManager.Tests.Integration.Constants.Tables;
using ThumbnailHeightAsset = PhotoManager.Tests.Integration.Constants.ThumbnailHeightAsset;
using ThumbnailWidthAsset = PhotoManager.Tests.Integration.Constants.ThumbnailWidthAsset;

namespace PhotoManager.Tests.Integration.Infrastructure.Database.Storage;

[TestFixture]
public class ObjectListStorageTests
{
    private string? _dataDirectory;

    private ObjectListStorage? _objectListStorage;
    private UserConfigurationService? _userConfigurationService;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);

        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();

        _userConfigurationService = new(configurationRootMock.Object);
    }

    [SetUp]
    public void SetUp()
    {
        _objectListStorage = new ObjectListStorage();
    }

    [Test]
    public void ReadObjectList_FolderType_ReturnsNotEmptyList()
    {
        string dataFilePath = Path.Combine(_dataDirectory!, Directories.TEST_BACKUP, Constants.DATABASE_END_PATH, $"{Directories.TABLES}\\{Tables.FOLDERS_DB}");
        DataTableProperties dataTableProperties = new()
        {
            TableName = _userConfigurationService!.StorageSettings.TablesSettings.FoldersTableName,
            ColumnProperties = FolderConfigs.ConfigureDataTable()
        };

        _objectListStorage!.Initialize(dataTableProperties, _userConfigurationService!.StorageSettings.Separator);

        List<Folder> result = _objectListStorage!.ReadObjectList(dataFilePath, FolderConfigs.ReadFunc, new Diagnostics());

        Assert.That(result, Is.InstanceOf<List<Folder>>());
        Assert.That(result, Is.Not.Empty);
        Assert.That(result, Has.Count.EqualTo(3));

        Folder? folder1 = result.FirstOrDefault(x => x.Id == new Guid("f1f00403-0554-4201-9b6b-11a6b4cea3a9"));
        Assert.That(folder1, Is.Not.Null);
        Assert.That(folder1!.Path, Is.EqualTo("D:\\Workspace\\PhotoManager\\Test"));

        Folder? folder2 = result.FirstOrDefault(x => x.Id == new Guid("2c107211-1a1c-4e73-8e8b-35d18ca8ef85"));
        Assert.That(folder2, Is.Not.Null);
        Assert.That(folder2!.Path, Is.EqualTo("D:\\Workspace\\PhotoManager\\Test\\OutputVideoFirstFrame"));

        Folder? folder3 = result.FirstOrDefault(x => x.Id == new Guid("18033543-defb-4d37-837b-d8063eda3a25"));
        Assert.That(folder3, Is.Not.Null);
        Assert.That(folder3!.Path, Is.EqualTo("D:\\Workspace\\PhotoManager\\Test\\toto"));
    }

    [Test]
    public void ReadObjectList_AssetType_ReturnsNotEmptyList()
    {
        string dataFilePath = Path.Combine(_dataDirectory!, Directories.TEST_BACKUP, Constants.DATABASE_END_PATH, $"{Directories.TABLES}\\{Tables.ASSETS_DB}");
        DataTableProperties dataTableProperties = new()
        {
            TableName = _userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName,
            ColumnProperties = AssetConfigs.ConfigureDataTable()
        };

        _objectListStorage!.Initialize(dataTableProperties, _userConfigurationService!.StorageSettings.Separator);

        List<Asset> result = _objectListStorage!.ReadObjectList(dataFilePath, AssetConfigs.ReadFunc, new Diagnostics());

        Assert.That(result, Is.InstanceOf<List<Asset>>());
        Assert.That(result, Is.Not.Empty);
        Assert.That(result, Has.Count.EqualTo(11));

        Asset? asset = result.FirstOrDefault(x => x.Hash == "ee43714d8b96d7ed3308d18afcb701444198c783fbe4103ce44e95aaf99c2095ae70e6e2035a7a438d1598fadaf5fe8cb0d541378387d20e91f26819fcc64b82");
        Assert.That(asset, Is.Not.Null);
        Assert.That(asset!.FileName, Is.EqualTo("533.JPG"));
        Assert.That(asset.FolderId, Is.EqualTo(new Guid("f1f00403-0554-4201-9b6b-11a6b4cea3a9")));
        Assert.That(asset.ImageRotation, Is.EqualTo(Rotation.Rotate270));
        Assert.That(asset.Pixel.Asset.Width, Is.EqualTo(3072));
        Assert.That(asset.Pixel.Asset.Height, Is.EqualTo(4080));
        Assert.That(asset.Pixel.Thumbnail.Width, Is.EqualTo(113));
        Assert.That(asset.Pixel.Thumbnail.Height, Is.EqualTo(150));
        Assert.That(asset.ThumbnailCreationDateTime, Is.EqualTo(new DateTime(2023, 8, 25, 12, 01, 21)));
        Assert.That(asset.Metadata.Corrupted.IsTrue, Is.False);
        Assert.That(asset.Metadata.Corrupted.Message, Is.EqualTo(string.Empty));
        Assert.That(asset.Metadata.Rotated.IsTrue, Is.True);
        Assert.That(asset.Metadata.Rotated.Message, Is.EqualTo("The asset has been rotated"));
    }

    [Test]
    public void ReadObjectList_SyncAssetsDirectoriesDefinitionType_ReturnsNotEmptyList()
    {
        string dataFilePath = Path.Combine(_dataDirectory!, Directories.TEST_BACKUP, Constants.DATABASE_END_PATH, $"{Directories.TABLES}\\{Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_DB}");
        DataTableProperties dataTableProperties = new()
        {
            TableName = _userConfigurationService!.StorageSettings.TablesSettings.SyncAssetsDirectoriesDefinitionsTableName,
            ColumnProperties = SyncAssetsDirectoriesDefinitionConfigs.ConfigureDataTable()
        };

        _objectListStorage!.Initialize(dataTableProperties, _userConfigurationService!.StorageSettings.Separator);

        List<SyncAssetsDirectoriesDefinition> result = _objectListStorage!.ReadObjectList(dataFilePath, SyncAssetsDirectoriesDefinitionConfigs.ReadFunc, new Diagnostics());

        Assert.That(result, Is.InstanceOf<List<SyncAssetsDirectoriesDefinition>>());
        Assert.That(result, Is.Not.Empty);
        Assert.That(result, Has.Count.EqualTo(2));

        SyncAssetsDirectoriesDefinition? firstSyncAssetsDirectoriesDefinition = result.FirstOrDefault(x => x.SourceDirectory == "C:\\Toto\\Screenshots");
        Assert.That(firstSyncAssetsDirectoriesDefinition, Is.Not.Null);
        Assert.That(firstSyncAssetsDirectoriesDefinition!.DestinationDirectory, Is.EqualTo("C:\\Images\\Toto"));
        Assert.That(firstSyncAssetsDirectoriesDefinition.IncludeSubFolders, Is.False);
        Assert.That(firstSyncAssetsDirectoriesDefinition.DeleteAssetsNotInSource, Is.False);

        SyncAssetsDirectoriesDefinition? secondSyncAssetsDirectoriesDefinition = result.FirstOrDefault(x => x.SourceDirectory == "C:\\Tutu\\Screenshots");
        Assert.That(secondSyncAssetsDirectoriesDefinition, Is.Not.Null);
        Assert.That(secondSyncAssetsDirectoriesDefinition!.DestinationDirectory, Is.EqualTo("C:\\Images\\Tutu"));
        Assert.That(secondSyncAssetsDirectoriesDefinition.IncludeSubFolders, Is.False);
        Assert.That(secondSyncAssetsDirectoriesDefinition.DeleteAssetsNotInSource, Is.False);
    }

    [Test]
    public void ReadObjectList_RecentTargetPathsType_ReturnsNotEmptyList()
    {
        string dataFilePath = Path.Combine(_dataDirectory!, Directories.TEST_BACKUP, Constants.DATABASE_END_PATH, $"{Directories.TABLES}\\{Tables.RECENT_TARGET_PATHS_DB}");
        DataTableProperties dataTableProperties = new()
        {
            TableName = _userConfigurationService!.StorageSettings.TablesSettings.RecentTargetPathsTableName,
            ColumnProperties = RecentPathsConfigs.ConfigureDataTable()
        };

        _objectListStorage!.Initialize(dataTableProperties, _userConfigurationService!.StorageSettings.Separator);

        List<string> result = _objectListStorage!.ReadObjectList(dataFilePath, RecentPathsConfigs.ReadFunc, new Diagnostics());

        Assert.That(result, Is.Not.Empty);
        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result.Any(x => x == "D:\\Workspace\\PhotoManager\\Toto"), Is.True);
        Assert.That(result.Any(x => x == "D:\\Workspace\\PhotoManager\\Tutu"), Is.True);
    }

    [Test]
    public void ReadObjectList_SyncAssetsDirectoriesDefinitionTypeEmpty_ReturnsEmptyList()
    {
        string dataFilePath = Path.Combine(_dataDirectory!, Directories.TEST_BACKUP, Constants.DATABASE_END_PATH, $"{Directories.TABLES}\\syncassetsdirectoriesdefinitions_empty.db");
        DataTableProperties dataTableProperties = new()
        {
            TableName = _userConfigurationService!.StorageSettings.TablesSettings.SyncAssetsDirectoriesDefinitionsTableName,
            ColumnProperties = SyncAssetsDirectoriesDefinitionConfigs.ConfigureDataTable()
        };

        _objectListStorage!.Initialize(dataTableProperties, _userConfigurationService!.StorageSettings.Separator);

        List<SyncAssetsDirectoriesDefinition> result = _objectListStorage!.ReadObjectList(dataFilePath, SyncAssetsDirectoriesDefinitionConfigs.ReadFunc, new Diagnostics());

        Assert.That(result, Is.InstanceOf<List<SyncAssetsDirectoriesDefinition>>());
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void ReadObjectList_FolderTypeWithoutProperties_ReturnsNotEmptyList()
    {
        string dataFilePath = Path.Combine(_dataDirectory!, Directories.TEST_BACKUP, Constants.DATABASE_END_PATH, $"{Directories.TABLES}\\{Tables.FOLDERS_DB}");
        DataTableProperties? dataTableProperties = null;
        _objectListStorage!.Initialize(dataTableProperties!, _userConfigurationService!.StorageSettings.Separator);

        List<Folder> result = _objectListStorage!.ReadObjectList(dataFilePath, FolderConfigs.ReadFunc, new Diagnostics());

        Assert.That(result, Is.InstanceOf<List<Folder>>());
        Assert.That(result, Is.Not.Empty);
        Assert.That(result, Has.Count.EqualTo(3));
        Assert.That(result.FirstOrDefault(x => x.Id == new Guid("f1f00403-0554-4201-9b6b-11a6b4cea3a9")), Is.Not.Null);
    }

    [Test]
    public void ReadObjectList_FolderTypeWithoutInitialize_ThrowsFormatException()
    {
        string dataFilePath = Path.Combine(_dataDirectory!, Directories.TEST_BACKUP, Constants.DATABASE_END_PATH, $"{Directories.TABLES}\\{Tables.FOLDERS_DB}");

        FormatException? exception = Assert.Throws<FormatException>(() =>
        {
            _objectListStorage!.ReadObjectList(dataFilePath, FolderConfigs.ReadFunc, new Diagnostics());
        });

        Assert.That(exception?.Message, Is.EqualTo("Guid should contain 32 digits with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx)."));
    }

    [Test]
    public void ReadObjectList_SyncAssetsDirectoriesDefinitionTypeWithoutInitialize_ThrowsIndexOutOfRangeException()
    {
        string dataFilePath = Path.Combine(_dataDirectory!, Directories.TEST_BACKUP, Constants.DATABASE_END_PATH, $"{Directories.TABLES}\\{Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_DB}");

        IndexOutOfRangeException? exception = Assert.Throws<IndexOutOfRangeException>(() =>
        {
            _objectListStorage!.ReadObjectList(dataFilePath, SyncAssetsDirectoriesDefinitionConfigs.ReadFunc, new Diagnostics());
        });

        Assert.That(exception?.Message, Is.EqualTo("Index was outside the bounds of the array."));
    }

    [Test]
    public void ReadObjectList_AssetTypeButReadingWrongFile_ThrowsArgumentException()
    {
        string dataFilePath = Path.Combine(_dataDirectory!, Directories.TEST_BACKUP, Constants.DATABASE_END_PATH, $"{Directories.TABLES}\\{Tables.FOLDERS_DB}");
        DataTableProperties dataTableProperties = new()
        {
            TableName = _userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName,
            ColumnProperties = AssetConfigs.ConfigureDataTable()
        };

        _objectListStorage!.Initialize(dataTableProperties, _userConfigurationService!.StorageSettings.Separator);

        ArgumentException? exception = Assert.Throws<ArgumentException>(() =>
        {
            _objectListStorage!.ReadObjectList(dataFilePath, AssetConfigs.ReadFunc, new Diagnostics());
        });

        Assert.That(exception?.Message, Is.EqualTo("Requested value 'D:\\Workspace\\PhotoManager\\Test' was not found."));
    }

    [Test]
    public void ReadObjectList_FileDoesNotExist_ReturnsEmptyList()
    {
        string dataFilePath = Path.Combine(_dataDirectory!, Directories.TEST_BACKUP, Constants.DATABASE_END_PATH, $"{Directories.TABLES}\\nonExistent.db");
        DataTableProperties dataTableProperties = new()
        {
            TableName = "NonExistent",
            ColumnProperties =
            [
                new() { ColumnName = "NonExistent" }
            ]
        };

        _objectListStorage!.Initialize(dataTableProperties, _userConfigurationService!.StorageSettings.Separator);

        List<string> result = _objectListStorage!.ReadObjectList(dataFilePath, RecentPathsConfigs.ReadFunc, new Diagnostics());

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void ReadObjectList_FilePathIsInvalid_ReturnsEmptyList()
    {
        DataTableProperties dataTableProperties = new()
        {
            TableName = "NonExistent",
            ColumnProperties =
            [
                new() { ColumnName = "NonExistent" }
            ]
        };

        _objectListStorage!.Initialize(dataTableProperties, _userConfigurationService!.StorageSettings.Separator);

        List<string> result = _objectListStorage!.ReadObjectList(_dataDirectory!, RecentPathsConfigs.ReadFunc, new Diagnostics());

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void ReadObjectList_FilePathDoesNotExist_ReturnsEmptyList()
    {
        string dataFilePath = Path.Combine(_dataDirectory!, "Toto\\nonExistent.db");
        DataTableProperties dataTableProperties = new()
        {
            TableName = "NonExistent",
            ColumnProperties =
            [
                new() { ColumnName = "NonExistent" }
            ]
        };

        _objectListStorage!.Initialize(dataTableProperties, _userConfigurationService!.StorageSettings.Separator);

        List<string> result = _objectListStorage!.ReadObjectList(dataFilePath, RecentPathsConfigs.ReadFunc, new Diagnostics());

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void ReadObjectList_FilePathIsNull_ReturnsEmptyList()
    {
        string? dataFilePath = null;
        DataTableProperties dataTableProperties = new()
        {
            TableName = "NonExistent",
            ColumnProperties =
            [
                new() { ColumnName = "NonExistent" }
            ]
        };

        _objectListStorage!.Initialize(dataTableProperties, _userConfigurationService!.StorageSettings.Separator);

        List<string> result = _objectListStorage!.ReadObjectList(dataFilePath!, RecentPathsConfigs.ReadFunc, new Diagnostics());

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void ReadObjectList_FolderTypeFuncIsNull_ThrowsNullReferenceException()
    {
        string dataFilePath = Path.Combine(_dataDirectory!, Directories.TEST_BACKUP, Constants.DATABASE_END_PATH, $"{Directories.TABLES}\\{Tables.FOLDERS_DB}");
        DataTableProperties dataTableProperties = new()
        {
            TableName = _userConfigurationService!.StorageSettings.TablesSettings.FoldersTableName,
            ColumnProperties = FolderConfigs.ConfigureDataTable()
        };

        _objectListStorage!.Initialize(dataTableProperties, _userConfigurationService!.StorageSettings.Separator);

        Func<string[], object>? func = null;

        NullReferenceException? exception = Assert.Throws<NullReferenceException>(() => _objectListStorage!.ReadObjectList(dataFilePath, func!, new Diagnostics()));

        Assert.That(exception?.Message, Is.EqualTo("Object reference not set to an instance of an object."));
    }

    [Test]
    public void WriteObjectList_FolderType_WritesCorrectCsv()
    {
        string dataFilePath = Path.Combine(_dataDirectory!, Tables.FOLDERS_DB);
        List<Folder> folders =
        [
            new()
            {
                Id = new Guid("dfc8aab7-3543-48e7-9fdc-596ba733761e"),
                Path = "D:\\Workspace\\PhotoManager\\Test"
            },

            new()
            {
                Id = new Guid("8f9dff55-4a15-411e-a4cb-7ec3024b2238"),
                Path = "D:\\Workspace\\PhotoManager\\Test\\OutputVideoFirstFrame"
            },

            new()
            {
                Id = new Guid("db4d226f-6901-43f9-9e82-e6a052f627d2"),
                Path = "D:\\Workspace\\PhotoManager\\Test\\toto"
            }
        ];

        DataTableProperties dataTableProperties = new()
        {
            TableName = _userConfigurationService!.StorageSettings.TablesSettings.FoldersTableName,
            ColumnProperties = FolderConfigs.ConfigureDataTable()
        };

        try
        {
            _objectListStorage!.Initialize(dataTableProperties, _userConfigurationService!.StorageSettings.Separator);
            _objectListStorage!.WriteObjectList(dataFilePath, folders, FolderConfigs.WriteFunc, new Diagnostics());

            Assert.That(File.Exists(dataFilePath), Is.True);
        }
        finally
        {
            File.Delete(dataFilePath);
        }
    }

    [Test]
    public void WriteObjectList_AssetType_WritesCorrectCsv()
    {
        string dataFilePath = Path.Combine(_dataDirectory!, Tables.ASSETS_DB);
        List<Asset> assets =
        [
            new()
            {
                FolderId = new Guid("dfc8aab7-3543-48e7-9fdc-596ba733761e"),
                Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
                FileName = FileNames._1336_JPG,
                ImageRotation = Rotation.Rotate0,
                Pixel = new()
                {
                    Asset = new() { Width = PixelWidthAsset._1336_JPG, Height = PixelHeightAsset._1336_JPG },
                    Thumbnail = new() { Width = ThumbnailWidthAsset._1336_JPG, Height = ThumbnailHeightAsset._1336_JPG }
                },
                FileProperties = new() { Size = FileSize._1336_JPG },
                ThumbnailCreationDateTime = new (2023, 8, 19, 11, 26, 05),
                Hash = Hashes._1336_JPG,
                Metadata = new()
                {
                    Corrupted = new() { IsTrue = false, Message = null },
                    Rotated = new() { IsTrue = false, Message = null }
                }
            },
            new()
            {
                FolderId = new Guid("dfc8aab7-3543-48e7-9fdc-596ba733761f"),
                Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
                FileName = "1452.DNG",
                ImageRotation = Rotation.Rotate90,
                Pixel = new()
                {
                    Asset = new() { Width = 4032, Height = 3024 },
                    Thumbnail = new() { Width = 200, Height = 150 }
                },
                FileProperties = new() { Size = 5286168 },
                ThumbnailCreationDateTime = new (2023, 8, 19, 11, 26, 05),
                Hash =
                    "e9680ad893bee9471a55421c0ea30075723f3ad274caec6300f7ce44adefb8a68b1a4f65ea25543a39f60d63af1681eca61bfb6b6f18d38c8dbb0672e2018e58",
                Metadata = new()
                {
                    Corrupted = new() { IsTrue = false, Message = null },
                    Rotated = new() { IsTrue = true, Message = "The asset has been rotated" }
                }
            }
        ];

        DataTableProperties dataTableProperties = new()
        {
            TableName = _userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName,
            ColumnProperties = AssetConfigs.ConfigureDataTable()
        };

        try
        {
            _objectListStorage!.Initialize(dataTableProperties, _userConfigurationService!.StorageSettings.Separator);
            _objectListStorage!.WriteObjectList(dataFilePath, assets, AssetConfigs.WriteFunc, new Diagnostics());

            Assert.That(File.Exists(dataFilePath), Is.True);
        }
        finally
        {
            File.Delete(dataFilePath);
        }
    }

    [Test]
    public void WriteObjectList_SyncAssetsDirectoriesDefinitionType_WritesCorrectCsv()
    {
        string dataFilePath = Path.Combine(_dataDirectory!, Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_DB);
        List<SyncAssetsDirectoriesDefinition> syncAssetsDirectoriesDefinitions =
        [
            new()
            {
                SourceDirectory = Path.Combine(_dataDirectory!, Directories.NON_EXISTENT_FOLDER),
                DestinationDirectory = Path.Combine(_dataDirectory!, Directories.NON_EXISTENT_FOLDER_1)
            }
        ];

        DataTableProperties dataTableProperties = new()
        {
            TableName = _userConfigurationService!.StorageSettings.TablesSettings.SyncAssetsDirectoriesDefinitionsTableName,
            ColumnProperties = SyncAssetsDirectoriesDefinitionConfigs.ConfigureDataTable()
        };

        try
        {
            _objectListStorage!.Initialize(dataTableProperties, _userConfigurationService!.StorageSettings.Separator);
            _objectListStorage!.WriteObjectList(dataFilePath, syncAssetsDirectoriesDefinitions, SyncAssetsDirectoriesDefinitionConfigs.WriteFunc, new Diagnostics());

            Assert.That(File.Exists(dataFilePath), Is.True);
        }
        finally
        {
            File.Delete(dataFilePath);
        }
    }

    [Test]
    public void WriteObjectList_RecentTargetPathsType_WritesCorrectCsv()
    {
        string dataFilePath = Path.Combine(_dataDirectory!, Tables.RECENT_TARGET_PATHS_DB);
        List<string> recentTargetPaths =
        [
            Path.Combine(_dataDirectory!, Directories.NON_EXISTENT_FOLDER),
            Path.Combine(_dataDirectory!, Directories.NON_EXISTENT_FOLDER_1)
        ];

        DataTableProperties dataTableProperties = new()
        {
            TableName = _userConfigurationService!.StorageSettings.TablesSettings.RecentTargetPathsTableName,
            ColumnProperties = RecentPathsConfigs.ConfigureDataTable()
        };

        try
        {
            _objectListStorage!.Initialize(dataTableProperties, _userConfigurationService!.StorageSettings.Separator);
            _objectListStorage!.WriteObjectList(dataFilePath, recentTargetPaths, RecentPathsConfigs.WriteFunc, new Diagnostics());

            Assert.That(File.Exists(dataFilePath), Is.True);
        }
        finally
        {
            File.Delete(dataFilePath);
        }
    }

    [Test]
    public void WriteObjectList_FolderTypeWithoutProperties_ThrowsException()
    {
        string dataFilePath = Path.Combine(_dataDirectory!, Tables.FOLDERS_DB);
        List<Folder> folders =
        [
            new()
            {
                Id = new Guid("dfc8aab7-3543-48e7-9fdc-596ba733761e"),
                Path = "D:\\Workspace\\PhotoManager\\Test"
            },

            new()
            {
                Id = new Guid("8f9dff55-4a15-411e-a4cb-7ec3024b2238"),
                Path = "D:\\Workspace\\PhotoManager\\Test\\OutputVideoFirstFrame"
            },

            new()
            {
                Id = new Guid("db4d226f-6901-43f9-9e82-e6a052f627d2"),
                Path = "D:\\Workspace\\PhotoManager\\Test\\toto"
            }
        ];

        DataTableProperties? dataTableProperties = null;

        _objectListStorage!.Initialize(dataTableProperties, _userConfigurationService!.StorageSettings.Separator);

        Exception? exception = Assert.Throws<Exception>(() => _objectListStorage!.WriteObjectList(dataFilePath, folders, FolderConfigs.WriteFunc, new Diagnostics()));

        Assert.That(exception?.Message, Is.EqualTo("Properties must be defined for the columns in the table NoTableName."));

        Assert.That(File.Exists(dataFilePath), Is.False);
    }

    [Test]
    public void WriteObjectList_FolderTypeWithoutInitialize_ThrowsException()
    {
        string dataFilePath = Path.Combine(_dataDirectory!, Tables.FOLDERS_DB);
        List<Folder> folders =
        [
            new()
            {
                Id = new Guid("dfc8aab7-3543-48e7-9fdc-596ba733761e"),
                Path = "D:\\Workspace\\PhotoManager\\Test"
            },

            new()
            {
                Id = new Guid("8f9dff55-4a15-411e-a4cb-7ec3024b2238"),
                Path = "D:\\Workspace\\PhotoManager\\Test\\OutputVideoFirstFrame"
            },

            new()
            {
                Id = new Guid("db4d226f-6901-43f9-9e82-e6a052f627d2"),
                Path = "D:\\Workspace\\PhotoManager\\Test\\toto"
            }
        ];

        Exception? exception = Assert.Throws<Exception>(() => _objectListStorage!.WriteObjectList(dataFilePath, folders, FolderConfigs.WriteFunc, new Diagnostics()));

        Assert.That(exception?.Message, Is.EqualTo("Properties must be defined for the columns in the table NoTableName."));

        Assert.That(File.Exists(dataFilePath), Is.False);
    }

    [Test]
    public void WriteObjectList_FilePathDoesNotExist_ThrowsDirectoryNotFoundException()
    {
        string dataFilePath = Path.Combine(_dataDirectory!, Directories.NON_EXISTENT_FOLDER, "nonExistent.db");
        List<string> nonExistentValues = ["D:\\Workspace\\PhotoManager\\Test"];

        DataTableProperties dataTableProperties = new()
        {
            TableName = "NonExistent",
            ColumnProperties =
            [
                new() { ColumnName = "NonExistent" }
            ]
        };

        _objectListStorage!.Initialize(dataTableProperties, _userConfigurationService!.StorageSettings.Separator);

        DirectoryNotFoundException? exception = Assert.Throws<DirectoryNotFoundException>(() =>
        {
            _objectListStorage!.WriteObjectList(dataFilePath, nonExistentValues, RecentPathsConfigs.WriteFunc, new Diagnostics());
        });

        Assert.That(exception?.Message, Is.EqualTo($"Could not find a part of the path '{dataFilePath}'."));
        Assert.That(File.Exists(dataFilePath), Is.False);
    }

    [Test]
    public void WriteObjectList_FilePathIsInvalid_ThrowsUnauthorizedAccessException()
    {
        List<string> nonExistentValues = ["D:\\Workspace\\PhotoManager\\Test"];

        DataTableProperties dataTableProperties = new()
        {
            TableName = "NonExistent",
            ColumnProperties =
            [
                new() { ColumnName = "NonExistent" }
            ]
        };

        _objectListStorage!.Initialize(dataTableProperties, _userConfigurationService!.StorageSettings.Separator);

        UnauthorizedAccessException? exception = Assert.Throws<UnauthorizedAccessException>(() =>
        {
            _objectListStorage!.WriteObjectList(_dataDirectory!, nonExistentValues, RecentPathsConfigs.WriteFunc, new Diagnostics());
        });

        Assert.That(exception?.Message, Is.EqualTo($"Access to the path '{_dataDirectory!}' is denied."));
    }

    [Test]
    public void WriteObjectList_FilePathIsNull_ThrowsArgumentNullException()
    {
        string? dataFilePath = null;
        List<string> nonExistentValues = ["D:\\Workspace\\PhotoManager\\Test"];

        DataTableProperties dataTableProperties = new()
        {
            TableName = "NonExistent",
            ColumnProperties =
            [
                new() { ColumnName = "NonExistent" }
            ]
        };

        _objectListStorage!.Initialize(dataTableProperties, _userConfigurationService!.StorageSettings.Separator);

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() =>
        {
            _objectListStorage!.WriteObjectList(dataFilePath!, nonExistentValues, RecentPathsConfigs.WriteFunc, new Diagnostics());
        });

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'path')"));
        Assert.That(File.Exists(dataFilePath), Is.False);
    }

    [Test]
    public void WriteObjectList_ListIsNull_ThrowsNullReferenceException()
    {
        string dataFilePath = Path.Combine(_dataDirectory!, "nonExistent.db");
        List<string>? nonExistentValues = null;

        DataTableProperties dataTableProperties = new()
        {
            TableName = "NonExistent",
            ColumnProperties =
            [
                new() { ColumnName = "NonExistent" }
            ]
        };

        _objectListStorage!.Initialize(dataTableProperties, _userConfigurationService!.StorageSettings.Separator);

        NullReferenceException? exception = Assert.Throws<NullReferenceException>(() =>
            _objectListStorage!.WriteObjectList(
                dataFilePath,
                nonExistentValues!,
                (f, i) =>
                {
                    return i switch
                    {
                        0 => f,
                        _ => throw new ArgumentOutOfRangeException(nameof(i))
                    };
                },
                new Diagnostics()));

        Assert.That(exception?.Message, Is.EqualTo("Object reference not set to an instance of an object."));
        Assert.That(File.Exists(dataFilePath), Is.False);
    }

    [Test]
    public void WriteObjectList_FuncIsNull_ThrowsNullReferenceException()
    {
        string dataFilePath = Path.Combine(_dataDirectory!, "nonExistent.db");
        List<string> nonExistentValues = ["D:\\Workspace\\PhotoManager\\Test"];

        DataTableProperties dataTableProperties = new()
        {
            TableName = "NonExistent",
            ColumnProperties =
            [
                new() { ColumnName = "NonExistent" }
            ]
        };

        _objectListStorage!.Initialize(dataTableProperties, _userConfigurationService!.StorageSettings.Separator);
        Func<string, int, object>? f = null;

        NullReferenceException? exception = Assert.Throws<NullReferenceException>(() =>
            _objectListStorage!.WriteObjectList(
                dataFilePath,
                nonExistentValues,
                f!,
                new Diagnostics()));

        Assert.That(exception?.Message, Is.EqualTo("Object reference not set to an instance of an object."));
        Assert.That(File.Exists(dataFilePath), Is.False);
    }

    [Test]
    public void EscapeText_PropertiesIsNull_ReturnsFalse()
    {
        TestableBaseCsvStorage testableBaseCsvStorage = new();

        bool result = testableBaseCsvStorage.EscapeTextTest("UnknownColumnName");

        Assert.That(result, Is.False);
    }

    private class TestableBaseCsvStorage : BaseCsvStorage
    {
        public bool EscapeTextTest(string columnName) => EscapeText(columnName);
    }
}
