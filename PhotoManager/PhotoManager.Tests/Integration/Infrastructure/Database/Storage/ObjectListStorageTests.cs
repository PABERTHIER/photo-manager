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
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");

        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();

        _userConfigurationService = new (configurationRootMock.Object);
    }

    [SetUp]
    public void SetUp()
    {
        _objectListStorage = new ObjectListStorage();
    }

    [Test]
    public void ReadObjectList_FolderType_ReturnsNotEmptyList()
    {
        string dataFilePath = Path.Combine(_dataDirectory!, "TestBackup\\v1.0\\Tables\\folders.db");
        DataTableProperties dataTableProperties = new()
        {
            TableName = _userConfigurationService!.StorageSettings.TablesSettings.FoldersTableName,
            ColumnProperties = FolderConfigs.ConfigureDataTable()
        };

        _objectListStorage!.Initialize(dataTableProperties, _userConfigurationService!.StorageSettings.Separator);

        List<Folder> result = _objectListStorage!.ReadObjectList(dataFilePath, FolderConfigs.ReadFunc, new Diagnostics());

        Assert.IsInstanceOf<List<Folder>>(result);
        Assert.IsNotEmpty(result);
        Assert.AreEqual(3, result.Count);

        Folder? folder1 = result.FirstOrDefault(x => x.FolderId == new Guid("f1f00403-0554-4201-9b6b-11a6b4cea3a9"));
        Assert.IsNotNull(folder1);
        Assert.AreEqual("D:\\Workspace\\PhotoManager\\Test", folder1!.Path);

        Folder? folder2 = result.FirstOrDefault(x => x.FolderId == new Guid("2c107211-1a1c-4e73-8e8b-35d18ca8ef85"));
        Assert.IsNotNull(folder2);
        Assert.AreEqual("D:\\Workspace\\PhotoManager\\Test\\OutputVideoFirstFrame", folder2!.Path);

        Folder? folder3 = result.FirstOrDefault(x => x.FolderId == new Guid("18033543-defb-4d37-837b-d8063eda3a25"));
        Assert.IsNotNull(folder3);
        Assert.AreEqual("D:\\Workspace\\PhotoManager\\Test\\toto", folder3!.Path);
    }

    [Test]
    public void ReadObjectList_AssetType_ReturnsNotEmptyList()
    {
        string dataFilePath = Path.Combine(_dataDirectory!, "TestBackup\\v1.0\\Tables\\assets.db");
        DataTableProperties dataTableProperties = new()
        {
            TableName = _userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName,
            ColumnProperties = AssetConfigs.ConfigureDataTable()
        };

        _objectListStorage!.Initialize(dataTableProperties, _userConfigurationService!.StorageSettings.Separator);

        List<Asset> result = _objectListStorage!.ReadObjectList(dataFilePath, AssetConfigs.ReadFunc, new Diagnostics());

        Assert.IsInstanceOf<List<Asset>>(result);
        Assert.IsNotEmpty(result);
        Assert.AreEqual(11, result.Count);

        Asset? asset = result.FirstOrDefault(x => x.Hash == "ee43714d8b96d7ed3308d18afcb701444198c783fbe4103ce44e95aaf99c2095ae70e6e2035a7a438d1598fadaf5fe8cb0d541378387d20e91f26819fcc64b82");
        Assert.IsNotNull(asset);
        Assert.AreEqual("533.JPG", asset!.FileName);
        Assert.AreEqual(new Guid("f1f00403-0554-4201-9b6b-11a6b4cea3a9"), asset.FolderId);
        Assert.AreEqual(2986996, asset.FileSize);
        Assert.AreEqual(Rotation.Rotate270, asset.ImageRotation);
        Assert.AreEqual(3072, asset.Pixel.Asset.Width);
        Assert.AreEqual(4080, asset.Pixel.Asset.Height);
        Assert.AreEqual(113, asset.Pixel.Thumbnail.Width);
        Assert.AreEqual(150, asset.Pixel.Thumbnail.Height);
        Assert.AreEqual(new DateTime(2023, 8, 25, 12, 01, 21), asset.ThumbnailCreationDateTime);
        Assert.AreEqual(string.Empty, asset.AssetCorruptedMessage);
        Assert.AreEqual(false, asset.IsAssetCorrupted);
        Assert.AreEqual("The asset has been rotated", asset.AssetRotatedMessage);
        Assert.AreEqual(true, asset.IsAssetRotated);
    }

    [Test]
    public void ReadObjectList_SyncAssetsDirectoriesDefinitionType_ReturnsNotEmptyList()
    {
        string dataFilePath = Path.Combine(_dataDirectory!, "TestBackup\\v1.0\\Tables\\syncassetsdirectoriesdefinitions.db");
        DataTableProperties dataTableProperties = new()
        {
            TableName = _userConfigurationService!.StorageSettings.TablesSettings.SyncAssetsDirectoriesDefinitionsTableName,
            ColumnProperties = SyncAssetsDirectoriesDefinitionConfigs.ConfigureDataTable()
        };

        _objectListStorage!.Initialize(dataTableProperties, _userConfigurationService!.StorageSettings.Separator);

        List<SyncAssetsDirectoriesDefinition> result = _objectListStorage!.ReadObjectList(dataFilePath, SyncAssetsDirectoriesDefinitionConfigs.ReadFunc, new Diagnostics());

        Assert.IsInstanceOf<List<SyncAssetsDirectoriesDefinition>>(result);
        Assert.IsNotEmpty(result);
        Assert.AreEqual(2, result.Count);

        SyncAssetsDirectoriesDefinition? firstSyncAssetsDirectoriesDefinition = result.FirstOrDefault(x => x.SourceDirectory == "C:\\Toto\\Screenshots");
        Assert.IsNotNull(firstSyncAssetsDirectoriesDefinition);
        Assert.AreEqual("C:\\Images\\Toto", firstSyncAssetsDirectoriesDefinition!.DestinationDirectory);
        Assert.AreEqual(false, firstSyncAssetsDirectoriesDefinition.IncludeSubFolders);
        Assert.AreEqual(false, firstSyncAssetsDirectoriesDefinition.DeleteAssetsNotInSource);

        SyncAssetsDirectoriesDefinition? secondSyncAssetsDirectoriesDefinition = result.FirstOrDefault(x => x.SourceDirectory == "C:\\Tutu\\Screenshots");
        Assert.IsNotNull(secondSyncAssetsDirectoriesDefinition);
        Assert.AreEqual("C:\\Images\\Tutu", secondSyncAssetsDirectoriesDefinition!.DestinationDirectory);
        Assert.AreEqual(false, secondSyncAssetsDirectoriesDefinition.IncludeSubFolders);
        Assert.AreEqual(false, secondSyncAssetsDirectoriesDefinition.DeleteAssetsNotInSource);
    }

    [Test]
    public void ReadObjectList_RecentTargetPathsType_ReturnsNotEmptyList()
    {
        string dataFilePath = Path.Combine(_dataDirectory!, "TestBackup\\v1.0\\Tables\\recenttargetpaths.db");
        DataTableProperties dataTableProperties = new()
        {
            TableName = _userConfigurationService!.StorageSettings.TablesSettings.RecentTargetPathsTableName,
            ColumnProperties = RecentPathsConfigs.ConfigureDataTable()
        };

        _objectListStorage!.Initialize(dataTableProperties, _userConfigurationService!.StorageSettings.Separator);

        List<string> result = _objectListStorage!.ReadObjectList(dataFilePath, RecentPathsConfigs.ReadFunc, new Diagnostics());

        Assert.IsNotEmpty(result);
        Assert.AreEqual(2, result.Count);
        Assert.IsTrue(result.Any(x => x == "D:\\Workspace\\PhotoManager\\Toto"));
        Assert.IsTrue(result.Any(x => x == "D:\\Workspace\\PhotoManager\\Tutu"));
    }

    [Test]
    public void ReadObjectList_SyncAssetsDirectoriesDefinitionTypeEmpty_ReturnsEmptyList()
    {
        string dataFilePath = Path.Combine(_dataDirectory!, "TestBackup\\v1.0\\Tables\\syncassetsdirectoriesdefinitions_empty.db");
        DataTableProperties dataTableProperties = new()
        {
            TableName = _userConfigurationService!.StorageSettings.TablesSettings.SyncAssetsDirectoriesDefinitionsTableName,
            ColumnProperties = SyncAssetsDirectoriesDefinitionConfigs.ConfigureDataTable()
        };

        _objectListStorage!.Initialize(dataTableProperties, _userConfigurationService!.StorageSettings.Separator);

        List<SyncAssetsDirectoriesDefinition> result = _objectListStorage!.ReadObjectList(dataFilePath, SyncAssetsDirectoriesDefinitionConfigs.ReadFunc, new Diagnostics());

        Assert.IsInstanceOf<List<SyncAssetsDirectoriesDefinition>>(result);
        Assert.IsEmpty(result);
    }

    [Test]
    public void ReadObjectList_FolderTypeWithoutProperties_ReturnsNotEmptyList()
    {
        string dataFilePath = Path.Combine(_dataDirectory!, "TestBackup\\v1.0\\Tables\\folders.db");
        DataTableProperties? dataTableProperties = null;
        _objectListStorage!.Initialize(dataTableProperties!, _userConfigurationService!.StorageSettings.Separator);

        List<Folder> result = _objectListStorage!.ReadObjectList(dataFilePath, FolderConfigs.ReadFunc, new Diagnostics());

        Assert.IsInstanceOf<List<Folder>>(result);
        Assert.IsNotEmpty(result);
        Assert.AreEqual(3, result.Count);
        Assert.IsNotNull(result.FirstOrDefault(x => x.FolderId == new Guid("f1f00403-0554-4201-9b6b-11a6b4cea3a9")));
    }

    [Test]
    public void ReadObjectList_FolderTypeWithoutInitialize_ThrowsFormatException()
    {
        string dataFilePath = Path.Combine(_dataDirectory!, "TestBackup\\v1.0\\Tables\\folders.db");

        FormatException? exception = Assert.Throws<FormatException>(() =>
        {
            _objectListStorage!.ReadObjectList(dataFilePath, FolderConfigs.ReadFunc, new Diagnostics());
        });

        Assert.AreEqual("Guid should contain 32 digits with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx).", exception?.Message);
    }

    [Test]
    public void ReadObjectList_SyncAssetsDirectoriesDefinitionTypeWithoutInitialize_ThrowsIndexOutOfRangeException()
    {
        string dataFilePath = Path.Combine(_dataDirectory!, "TestBackup\\v1.0\\Tables\\syncassetsdirectoriesdefinitions.db");

        IndexOutOfRangeException? exception = Assert.Throws<IndexOutOfRangeException>(() =>
        {
            _objectListStorage!.ReadObjectList(dataFilePath, SyncAssetsDirectoriesDefinitionConfigs.ReadFunc, new Diagnostics());
        });

        Assert.AreEqual("Index was outside the bounds of the array.", exception?.Message);
    }

    [Test]
    public void ReadObjectList_AssetTypeButReadingWrongFile_ThrowsFormatException()
    {
        string dataFilePath = Path.Combine(_dataDirectory!, "TestBackup\\v1.0\\Tables\\folders.db");
        DataTableProperties dataTableProperties = new()
        {
            TableName = _userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName,
            ColumnProperties = AssetConfigs.ConfigureDataTable()
        };

        _objectListStorage!.Initialize(dataTableProperties, _userConfigurationService!.StorageSettings.Separator);

        FormatException? exception = Assert.Throws<FormatException>(() =>
        {
            _objectListStorage!.ReadObjectList(dataFilePath, AssetConfigs.ReadFunc, new Diagnostics());
        });

        Assert.AreEqual("The input string 'D:\\Workspace\\PhotoManager\\Test' was not in a correct format.", exception?.Message);
    }

    [Test]
    public void ReadObjectList_FileDoesNotExist_ReturnsEmptyList()
    {
        string dataFilePath = Path.Combine(_dataDirectory!, "TestBackup\\v1.0\\Tables\\nonExistent.db");
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

        Assert.IsEmpty(result);
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

        Assert.IsEmpty(result);
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

        Assert.IsEmpty(result);
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

        Assert.IsEmpty(result);
    }

    [Test]
    public void ReadObjectList_FolderTypeFuncIsNull_ThrowsNullReferenceException()
    {
        string dataFilePath = Path.Combine(_dataDirectory!, "TestBackup\\v1.0\\Tables\\folders.db");
        DataTableProperties dataTableProperties = new()
        {
            TableName = _userConfigurationService!.StorageSettings.TablesSettings.FoldersTableName,
            ColumnProperties = FolderConfigs.ConfigureDataTable()
        };

        _objectListStorage!.Initialize(dataTableProperties, _userConfigurationService!.StorageSettings.Separator);

        Func<string[], object>? func = null;

        NullReferenceException? exception = Assert.Throws<NullReferenceException>(() => _objectListStorage!.ReadObjectList(dataFilePath, func!, new Diagnostics()));

        Assert.AreEqual("Object reference not set to an instance of an object.", exception?.Message);
    }

    [Test]
    public void WriteObjectList_FolderType_WritesCorrectCsv()
    {
        string dataFilePath = Path.Combine(_dataDirectory!, "folders.db");
        List<Folder> folders =
        [
            new()
            {
                FolderId = new Guid("dfc8aab7-3543-48e7-9fdc-596ba733761e"),
                Path = "D:\\Workspace\\PhotoManager\\Test"
            },

            new()
            {
                FolderId = new Guid("8f9dff55-4a15-411e-a4cb-7ec3024b2238"),
                Path = "D:\\Workspace\\PhotoManager\\Test\\OutputVideoFirstFrame"
            },

            new()
            {
                FolderId = new Guid("db4d226f-6901-43f9-9e82-e6a052f627d2"),
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

            Assert.IsTrue(File.Exists(dataFilePath));
        }
        finally
        {
            File.Delete(dataFilePath);
        }
    }

    [Test]
    public void WriteObjectList_AssetType_WritesCorrectCsv()
    {
        string dataFilePath = Path.Combine(_dataDirectory!, "assets.db");
        List<Asset> assets =
        [
            new()
            {
                FolderId = new Guid("dfc8aab7-3543-48e7-9fdc-596ba733761e"),
                Folder = new() { Path = "" },
                FileName = "1336.JPG",
                FileSize = 4526710,
                ImageRotation = Rotation.Rotate0,
                Pixel = new()
                {
                    Asset = new() { Width = 4032, Height = 3024 },
                    Thumbnail = new() { Width = 200, Height = 150 }
                },
                ThumbnailCreationDateTime = new DateTime(2023, 8, 19, 11, 26, 05),
                Hash =
                    "fb0bb3887c28d022113c5c52a65f9da14eab0806842f902b618806dedeb7c53da1ed26ae5bcaf9a358e81efead71c4807ea38054e44f426857f8bf2c1fd1c7aa",
                AssetCorruptedMessage = null,
                IsAssetCorrupted = false,
                AssetRotatedMessage = null,
                IsAssetRotated = false
            },

            new()
            {
                FolderId = new Guid("dfc8aab7-3543-48e7-9fdc-596ba733761f"),
                Folder = new() { Path = "" },
                FileName = "1452.DNG",
                FileSize = 5286168,
                ImageRotation = Rotation.Rotate90,
                Pixel = new()
                {
                    Asset = new() { Width = 4032, Height = 3024 },
                    Thumbnail = new() { Width = 200, Height = 150 }
                },
                ThumbnailCreationDateTime = new DateTime(2023, 8, 19, 11, 26, 05),
                Hash =
                    "e9680ad893bee9471a55421c0ea30075723f3ad274caec6300f7ce44adefb8a68b1a4f65ea25543a39f60d63af1681eca61bfb6b6f18d38c8dbb0672e2018e58",
                AssetCorruptedMessage = null,
                IsAssetCorrupted = false,
                AssetRotatedMessage = "The asset has been rotated",
                IsAssetRotated = true
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

            Assert.IsTrue(File.Exists(dataFilePath));
        }
        finally
        {
            File.Delete(dataFilePath);
        }
    }

    [Test]
    public void WriteObjectList_SyncAssetsDirectoriesDefinitionType_WritesCorrectCsv()
    {
        string dataFilePath = Path.Combine(_dataDirectory!, "syncassetsdirectoriesdefinitions.db");
        List<SyncAssetsDirectoriesDefinition> syncAssetsDirectoriesDefinitions =
        [
            new()
            {
                SourceDirectory = Path.Combine(_dataDirectory!, "toto"),
                DestinationDirectory = Path.Combine(_dataDirectory!, "tutu")
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

            Assert.IsTrue(File.Exists(dataFilePath));
        }
        finally
        {
            File.Delete(dataFilePath);
        }
    }

    [Test]
    public void WriteObjectList_RecentTargetPathsType_WritesCorrectCsv()
    {
        string dataFilePath = Path.Combine(_dataDirectory!, "recenttargetpaths.db");
        List<string> recentTargetPaths =
        [
            Path.Combine(_dataDirectory!, "toto"),
            Path.Combine(_dataDirectory!, "tutu")
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

            Assert.IsTrue(File.Exists(dataFilePath));
        }
        finally
        {
            File.Delete(dataFilePath);
        }
    }

    [Test]
    public void WriteObjectList_FolderTypeWithoutProperties_ThrowsException()
    {
        string dataFilePath = Path.Combine(_dataDirectory!, "folders.db");
        List<Folder> folders =
        [
            new()
            {
                FolderId = new Guid("dfc8aab7-3543-48e7-9fdc-596ba733761e"),
                Path = "D:\\Workspace\\PhotoManager\\Test"
            },

            new()
            {
                FolderId = new Guid("8f9dff55-4a15-411e-a4cb-7ec3024b2238"),
                Path = "D:\\Workspace\\PhotoManager\\Test\\OutputVideoFirstFrame"
            },

            new()
            {
                FolderId = new Guid("db4d226f-6901-43f9-9e82-e6a052f627d2"),
                Path = "D:\\Workspace\\PhotoManager\\Test\\toto"
            }
        ];

        DataTableProperties? dataTableProperties = null;

        _objectListStorage!.Initialize(dataTableProperties, _userConfigurationService!.StorageSettings.Separator);

        Exception? exception = Assert.Throws<Exception>(() => _objectListStorage!.WriteObjectList(dataFilePath, folders, FolderConfigs.WriteFunc, new Diagnostics()));

        Assert.AreEqual("Properties must be defined for the columns in the table NoTableName.", exception?.Message);

        Assert.IsFalse(File.Exists(dataFilePath));
    }

    [Test]
    public void WriteObjectList_FolderTypeWithoutInitialize_ThrowsException()
    {
        string dataFilePath = Path.Combine(_dataDirectory!, "folders.db");
        List<Folder> folders =
        [
            new()
            {
                FolderId = new Guid("dfc8aab7-3543-48e7-9fdc-596ba733761e"),
                Path = "D:\\Workspace\\PhotoManager\\Test"
            },

            new()
            {
                FolderId = new Guid("8f9dff55-4a15-411e-a4cb-7ec3024b2238"),
                Path = "D:\\Workspace\\PhotoManager\\Test\\OutputVideoFirstFrame"
            },

            new()
            {
                FolderId = new Guid("db4d226f-6901-43f9-9e82-e6a052f627d2"),
                Path = "D:\\Workspace\\PhotoManager\\Test\\toto"
            }
        ];

        Exception? exception = Assert.Throws<Exception>(() => _objectListStorage!.WriteObjectList(dataFilePath, folders, FolderConfigs.WriteFunc, new Diagnostics()));

        Assert.AreEqual("Properties must be defined for the columns in the table NoTableName.", exception?.Message);

        Assert.IsFalse(File.Exists(dataFilePath));
    }

    [Test]
    public void WriteObjectList_FilePathDoesNotExist_ThrowsDirectoryNotFoundException()
    {
        string dataFilePath = Path.Combine(_dataDirectory!, "nonExistent", "nonExistent.db");
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

        Assert.AreEqual($"Could not find a part of the path '{dataFilePath}'.", exception?.Message);
        Assert.IsFalse(File.Exists(dataFilePath));
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

        Assert.AreEqual($"Access to the path '{_dataDirectory!}' is denied.", exception?.Message);
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

        Assert.AreEqual("Value cannot be null. (Parameter 'path')", exception?.Message);
        Assert.IsFalse(File.Exists(dataFilePath));
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

        Assert.AreEqual("Object reference not set to an instance of an object.", exception?.Message);
        Assert.IsFalse(File.Exists(dataFilePath));
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

        Assert.AreEqual("Object reference not set to an instance of an object.", exception?.Message);
        Assert.IsFalse(File.Exists(dataFilePath));
    }
}
