namespace PhotoManager.Tests.Integration.Infrastructure.Database.Storage;

[TestFixture]
public class ObjectListStorageTests
{
    private string? dataDirectory;
    private ObjectListStorage? _objectListStorage;

    private readonly char pipeSeparator = AssetConstants.Separator.ToCharArray().First();

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
    }

    [SetUp]
    public void Setup()
    {
        _objectListStorage = new ObjectListStorage();
    }

    [Test]
    public void ReadObjectList_FolderType_ReturnsNotEmptyList()
    {
        string dataFilePath = Path.Combine(dataDirectory!, "TestBackup\\v1.0\\Tables\\folder.db");
        DataTableProperties dataTableProperties = new()
        {
            TableName = AssetConstants.FolderTableName,
            ColumnProperties = FolderConfigs.ConfigureDataTable()
        };
        _objectListStorage!.Initialize(dataTableProperties, pipeSeparator);

        var result = _objectListStorage!.ReadObjectList(dataFilePath, FolderConfigs.ReadFunc, new Diagnostics());

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
        string dataFilePath = Path.Combine(dataDirectory!, "TestBackup\\v1.0\\Tables\\asset.db");
        DataTableProperties dataTableProperties = new()
        {
            TableName = AssetConstants.AssetTableName,
            ColumnProperties = AssetConfigs.ConfigureDataTable()
        };
        _objectListStorage!.Initialize(dataTableProperties, pipeSeparator);

        var result = _objectListStorage!.ReadObjectList(dataFilePath, AssetConfigs.ReadFunc, new Diagnostics());

        Assert.IsInstanceOf<List<Asset>>(result);
        Assert.IsNotEmpty(result);
        Assert.AreEqual(11, result.Count);

        Asset? asset = result.FirstOrDefault(x => x.Hash == "ee43714d8b96d7ed3308d18afcb701444198c783fbe4103ce44e95aaf99c2095ae70e6e2035a7a438d1598fadaf5fe8cb0d541378387d20e91f26819fcc64b82");
        Assert.IsNotNull(asset);
        Assert.AreEqual("533.JPG", asset!.FileName);
        Assert.AreEqual(new Guid("f1f00403-0554-4201-9b6b-11a6b4cea3a9"), asset!.FolderId);
        Assert.AreEqual(2986996, asset!.FileSize);
        Assert.AreEqual(Rotation.Rotate270, asset!.ImageRotation);
        Assert.AreEqual(3072, asset!.PixelWidth);
        Assert.AreEqual(4080, asset!.PixelHeight);
        Assert.AreEqual(113, asset!.ThumbnailPixelWidth);
        Assert.AreEqual(150, asset!.ThumbnailPixelHeight);
        Assert.AreEqual(new DateTime(2023, 8, 25, 12, 01, 21), asset!.ThumbnailCreationDateTime);
        Assert.AreEqual(string.Empty, asset!.AssetCorruptedMessage);
        Assert.AreEqual(false, asset!.IsAssetCorrupted);
        Assert.AreEqual("The asset has been rotated", asset!.AssetRotatedMessage);
        Assert.AreEqual(true, asset!.IsAssetRotated);
    }

    [Test]
    public void ReadObjectList_ImportType_ReturnsNotEmptyList()
    {
        string dataFilePath = Path.Combine(dataDirectory!, "TestBackup\\v1.0\\Tables\\import.db");
        DataTableProperties dataTableProperties = new()
        {
            TableName = AssetConstants.ImportTableName,
            ColumnProperties = SyncDefinitionConfigs.ConfigureDataTable()
        };
        _objectListStorage!.Initialize(dataTableProperties, pipeSeparator);

        var result = _objectListStorage!.ReadObjectList(dataFilePath, SyncDefinitionConfigs.ReadFunc, new Diagnostics());

        Assert.IsInstanceOf<List<SyncAssetsDirectoriesDefinition>>(result);
        Assert.IsNotEmpty(result);
        Assert.AreEqual(2, result.Count);

        SyncAssetsDirectoriesDefinition? firstImport = result.FirstOrDefault(x => x.SourceDirectory == "C:\\Toto\\Screenshots");
        Assert.IsNotNull(firstImport);
        Assert.AreEqual("C:\\Images\\Toto", firstImport!.DestinationDirectory);
        Assert.AreEqual(false, firstImport.IncludeSubFolders);
        Assert.AreEqual(false, firstImport.DeleteAssetsNotInSource);

        SyncAssetsDirectoriesDefinition? secondImport = result.FirstOrDefault(x => x.SourceDirectory == "C:\\Tutu\\Screenshots");
        Assert.IsNotNull(secondImport);
        Assert.AreEqual("C:\\Images\\Tutu", secondImport!.DestinationDirectory);
        Assert.AreEqual(false, secondImport.IncludeSubFolders);
        Assert.AreEqual(false, secondImport.DeleteAssetsNotInSource);
    }

    [Test]
    public void ReadObjectList_RecentTargetPathsType_ReturnsNotEmptyList()
    {
        string dataFilePath = Path.Combine(dataDirectory!, "TestBackup\\v1.0\\Tables\\recenttargetpaths.db");
        DataTableProperties dataTableProperties = new()
        {
            TableName = AssetConstants.RecentTargetPathsTableName,
            ColumnProperties = RecentPathsConfigs.ConfigureDataTable()
        };
        _objectListStorage!.Initialize(dataTableProperties, pipeSeparator);

        var result = _objectListStorage!.ReadObjectList(dataFilePath, RecentPathsConfigs.ReadFunc, new Diagnostics());

        Assert.IsNotEmpty(result);
        Assert.AreEqual(2, result.Count);
        Assert.IsTrue(result.Any(x => x == "D:\\Workspace\\PhotoManager\\Toto"));
        Assert.IsTrue(result.Any(x => x == "D:\\Workspace\\PhotoManager\\Tutu"));
    }

    [Test]
    public void ReadObjectList_ImportTypeEmpty_ReturnsEmptyList()
    {
        string dataFilePath = Path.Combine(dataDirectory!, "TestBackup\\v1.0\\Tables\\import_empty.db");
        DataTableProperties dataTableProperties = new()
        {
            TableName = AssetConstants.ImportTableName,
            ColumnProperties = SyncDefinitionConfigs.ConfigureDataTable()
        };
        _objectListStorage!.Initialize(dataTableProperties, pipeSeparator);

        var result = _objectListStorage!.ReadObjectList(dataFilePath, SyncDefinitionConfigs.ReadFunc, new Diagnostics());

        Assert.IsInstanceOf<List<SyncAssetsDirectoriesDefinition>>(result);
        Assert.IsEmpty(result);
    }

    [Test]
    public void ReadObjectList_FolderTypeWithoutProperties_ReturnsNotEmptyList()
    {
        string dataFilePath = Path.Combine(dataDirectory!, "TestBackup\\v1.0\\Tables\\folder.db");
        DataTableProperties? dataTableProperties = null;
        _objectListStorage!.Initialize(dataTableProperties!, pipeSeparator);

        var result = _objectListStorage!.ReadObjectList(dataFilePath, FolderConfigs.ReadFunc, new Diagnostics());

        Assert.IsInstanceOf<List<Folder>>(result);
        Assert.IsNotEmpty(result);
        Assert.AreEqual(3, result.Count);
        Assert.IsNotNull(result.FirstOrDefault(x => x.FolderId == new Guid("f1f00403-0554-4201-9b6b-11a6b4cea3a9")));
    }

    [Test]
    public void ReadObjectList_FolderTypeWithoutInitialize_ThrowsFormatException()
    {
        string dataFilePath = Path.Combine(dataDirectory!, "TestBackup\\v1.0\\Tables\\folder.db");

        var exception = Assert.Throws<FormatException>(() =>
        {
            _objectListStorage!.ReadObjectList(dataFilePath, FolderConfigs.ReadFunc, new Diagnostics());
        });
        Assert.AreEqual("Guid should contain 32 digits with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx).", exception?.Message);
    }

    [Test]
    public void ReadObjectList_ImportTypeWithoutInitialize_ThrowsIndexOutOfRangeException()
    {
        string dataFilePath = Path.Combine(dataDirectory!, "TestBackup\\v1.0\\Tables\\import.db");

        Assert.Throws<IndexOutOfRangeException>(() =>
        {
            _objectListStorage!.ReadObjectList(dataFilePath, SyncDefinitionConfigs.ReadFunc, new Diagnostics());
        });
    }

    [Test]
    public void ReadObjectList_AssetTypeButReadingWrongFile_ThrowsFormatException()
    {
        string dataFilePath = Path.Combine(dataDirectory!, "TestBackup\\v1.0\\Tables\\folder.db");
        DataTableProperties dataTableProperties = new()
        {
            TableName = AssetConstants.AssetTableName,
            ColumnProperties = AssetConfigs.ConfigureDataTable()
        };
        _objectListStorage!.Initialize(dataTableProperties, pipeSeparator);

        Assert.Throws<FormatException>(() =>
        {
            _objectListStorage!.ReadObjectList(dataFilePath, AssetConfigs.ReadFunc, new Diagnostics());
        });
    }

    [Test]
    public void ReadObjectList_FileDoesNotExist_ReturnsEmptyList()
    {
        string dataFilePath = Path.Combine(dataDirectory!, "TestBackup\\v1.0\\Tables\\nonExistent.db");
        DataTableProperties dataTableProperties = new()
        {
            TableName = "NonExistent",
            ColumnProperties = new ColumnProperties[]
            {
                new ColumnProperties { ColumnName = "NonExistent" }
            }
        };
        _objectListStorage!.Initialize(dataTableProperties, pipeSeparator);

        var result = _objectListStorage!.ReadObjectList(dataFilePath, RecentPathsConfigs.ReadFunc, new Diagnostics());

        Assert.IsEmpty(result);
    }

    [Test]
    public void ReadObjectList_PathDoesNotExist_ReturnsEmptyList()
    {
        string dataFilePath = Path.Combine(dataDirectory!, "Toto\\nonExistent.db");
        DataTableProperties dataTableProperties = new()
        {
            TableName = "NonExistent",
            ColumnProperties = new ColumnProperties[]
            {
                new ColumnProperties { ColumnName = "NonExistent" }
            }
        };
        _objectListStorage!.Initialize(dataTableProperties, pipeSeparator);

        var result = _objectListStorage!.ReadObjectList(dataFilePath, RecentPathsConfigs.ReadFunc, new Diagnostics());

        Assert.IsEmpty(result);
    }

    [Test]
    public void ReadObjectList_PathIsNull_ReturnsEmptyList()
    {
        string? dataFilePath = null;
        DataTableProperties dataTableProperties = new()
        {
            TableName = "NonExistent",
            ColumnProperties = new ColumnProperties[]
            {
                new ColumnProperties { ColumnName = "NonExistent" }
            }
        };
        _objectListStorage!.Initialize(dataTableProperties, pipeSeparator);

        var result = _objectListStorage!.ReadObjectList(dataFilePath!, RecentPathsConfigs.ReadFunc, new Diagnostics());

        Assert.IsEmpty(result);
    }

    [Test]
    public void ReadObjectList_FolderTypeFuncIsNull_ThrowsNullReferenceException()
    {
        string dataFilePath = Path.Combine(dataDirectory!, "TestBackup\\v1.0\\Tables\\folder.db");
        DataTableProperties dataTableProperties = new()
        {
            TableName = AssetConstants.FolderTableName,
            ColumnProperties = FolderConfigs.ConfigureDataTable()
        };
        _objectListStorage!.Initialize(dataTableProperties, pipeSeparator);

        Func<string[], object>? f = null;

        Assert.Throws<NullReferenceException>(() => _objectListStorage!.ReadObjectList(dataFilePath!, f!, new Diagnostics()));
    }

    [Test]
    public void WriteObjectList_FolderType_WritesCorrectCsv()
    {
        string dataFilePath = Path.Combine(dataDirectory!, "folder.db");
        List<Folder> folders = new()
        {
            new Folder
            {
                FolderId = new Guid("dfc8aab7-3543-48e7-9fdc-596ba733761e"),
                Path = "D:\\Workspace\\PhotoManager\\Test"
            },
            new Folder
            {
                FolderId = new Guid("8f9dff55-4a15-411e-a4cb-7ec3024b2238"),
                Path = "D:\\Workspace\\PhotoManager\\Test\\OutputVideoFirstFrame"
            },
            new Folder
            {
                FolderId = new Guid("db4d226f-6901-43f9-9e82-e6a052f627d2"),
                Path = "D:\\Workspace\\PhotoManager\\Test\\toto"
            }
        };

        DataTableProperties dataTableProperties = new()
        {
            TableName = AssetConstants.FolderTableName,
            ColumnProperties = FolderConfigs.ConfigureDataTable()
        };

        try
        {
            _objectListStorage!.Initialize(dataTableProperties, pipeSeparator);
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
        string dataFilePath = Path.Combine(dataDirectory!, "asset.db");
        List<Asset> assets = new()
        {
            new Asset
            {
                FolderId = new Guid("dfc8aab7-3543-48e7-9fdc-596ba733761e"),
                FileName = "1336.JPG",
                FileSize = 4526710,
                ImageRotation = Rotation.Rotate0,
                PixelWidth = 4032,
                PixelHeight = 3024,
                ThumbnailPixelWidth = 200,
                ThumbnailPixelHeight = 150,
                ThumbnailCreationDateTime = new DateTime(2023, 8, 19, 11, 26, 05),
                Hash = "fb0bb3887c28d022113c5c52a65f9da14eab0806842f902b618806dedeb7c53da1ed26ae5bcaf9a358e81efead71c4807ea38054e44f426857f8bf2c1fd1c7aa",
                AssetCorruptedMessage = null,
                IsAssetCorrupted = false,
                AssetRotatedMessage = null,
                IsAssetRotated = false
            },
            new Asset
            {
                FolderId = new Guid("dfc8aab7-3543-48e7-9fdc-596ba733761f"),
                FileName = "1452.DNG",
                FileSize = 5286168,
                ImageRotation = Rotation.Rotate90,
                PixelWidth = 4032,
                PixelHeight = 3024,
                ThumbnailPixelWidth = 200,
                ThumbnailPixelHeight = 150,
                ThumbnailCreationDateTime = new DateTime(2023, 8, 19, 11, 26, 05),
                Hash = "e9680ad893bee9471a55421c0ea30075723f3ad274caec6300f7ce44adefb8a68b1a4f65ea25543a39f60d63af1681eca61bfb6b6f18d38c8dbb0672e2018e58",
                AssetCorruptedMessage = null,
                IsAssetCorrupted = false,
                AssetRotatedMessage = "The asset has been rotated",
                IsAssetRotated = true
            }
        };

        DataTableProperties dataTableProperties = new()
        {
            TableName = AssetConstants.AssetTableName,
            ColumnProperties = AssetConfigs.ConfigureDataTable()
        };

        try
        {
            _objectListStorage!.Initialize(dataTableProperties, pipeSeparator);
            _objectListStorage!.WriteObjectList(dataFilePath, assets, AssetConfigs.WriteFunc, new Diagnostics());

            Assert.IsTrue(File.Exists(dataFilePath));
        }
        finally
        {
            File.Delete(dataFilePath);
        }
    }

    [Test]
    public void WriteObjectList_ImportType_WritesCorrectCsv()
    {
        string dataFilePath = Path.Combine(dataDirectory!, "import.db");
        List<SyncAssetsDirectoriesDefinition> imports = new()
        {
            new SyncAssetsDirectoriesDefinition
            {
                SourceDirectory = Path.Combine(dataDirectory!, "toto"),
                DestinationDirectory = Path.Combine(dataDirectory!, "tutu")
            }
        };

        DataTableProperties dataTableProperties = new()
        {
            TableName = AssetConstants.ImportTableName,
            ColumnProperties = SyncDefinitionConfigs.ConfigureDataTable()
        };

        try
        {
            _objectListStorage!.Initialize(dataTableProperties, pipeSeparator);
            _objectListStorage!.WriteObjectList(dataFilePath, imports, SyncDefinitionConfigs.WriteFunc, new Diagnostics());

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
        string dataFilePath = Path.Combine(dataDirectory!, "recenttargetpaths.db");
        List<string> recentTargetPaths = new()
        {
            Path.Combine(dataDirectory!, "toto"),
            Path.Combine(dataDirectory!, "tutu")
        };

        DataTableProperties dataTableProperties = new()
        {
            TableName = AssetConstants.RecentTargetPathsTableName,
            ColumnProperties = RecentPathsConfigs.ConfigureDataTable()
        };

        try
        {
            _objectListStorage!.Initialize(dataTableProperties, pipeSeparator);
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
        string dataFilePath = Path.Combine(dataDirectory!, "folder.db");
        List<Folder> folders = new()
        {
            new Folder
            {
                FolderId = new Guid("dfc8aab7-3543-48e7-9fdc-596ba733761e"),
                Path = "D:\\Workspace\\PhotoManager\\Test"
            },
            new Folder
            {
                FolderId = new Guid("8f9dff55-4a15-411e-a4cb-7ec3024b2238"),
                Path = "D:\\Workspace\\PhotoManager\\Test\\OutputVideoFirstFrame"
            },
            new Folder
            {
                FolderId = new Guid("db4d226f-6901-43f9-9e82-e6a052f627d2"),
                Path = "D:\\Workspace\\PhotoManager\\Test\\toto"
            }
        };

        DataTableProperties? dataTableProperties = null;

        _objectListStorage!.Initialize(dataTableProperties, pipeSeparator);

        var exception = Assert.Throws<Exception>(() => _objectListStorage!.WriteObjectList(dataFilePath, folders, FolderConfigs.WriteFunc, new Diagnostics()));

        Assert.AreEqual("Properties must be defined for the columns in the table NoTableName.", exception?.Message);

        Assert.IsFalse(File.Exists(dataFilePath));
    }

    [Test]
    public void WriteObjectList_FolderTypeWithoutInitialize_ThrowsException()
    {
        string dataFilePath = Path.Combine(dataDirectory!, "folder.db");
        List<Folder> folders = new()
        {
            new Folder
            {
                FolderId = new Guid("dfc8aab7-3543-48e7-9fdc-596ba733761e"),
                Path = "D:\\Workspace\\PhotoManager\\Test"
            },
            new Folder
            {
                FolderId = new Guid("8f9dff55-4a15-411e-a4cb-7ec3024b2238"),
                Path = "D:\\Workspace\\PhotoManager\\Test\\OutputVideoFirstFrame"
            },
            new Folder
            {
                FolderId = new Guid("db4d226f-6901-43f9-9e82-e6a052f627d2"),
                Path = "D:\\Workspace\\PhotoManager\\Test\\toto"
            }
        };

        DataTableProperties dataTableProperties = new()
        {
            TableName = AssetConstants.FolderTableName,
            ColumnProperties = FolderConfigs.ConfigureDataTable()
        };

        var exception = Assert.Throws<Exception>(() => _objectListStorage!.WriteObjectList(dataFilePath, folders, FolderConfigs.WriteFunc, new Diagnostics()));


        Assert.AreEqual("Properties must be defined for the columns in the table NoTableName.", exception?.Message);

        Assert.IsFalse(File.Exists(dataFilePath));
    }

    [Test]
    public void WriteObjectList_PathDoesNotExist_ThrowsDirectoryNotFoundException()
    {
        string dataFilePath = Path.Combine(dataDirectory!, "nonExistent", "nonExistent.db");
        List<string> nonExistents = new()
        {
            "D:\\Workspace\\PhotoManager\\Test"
        };

        DataTableProperties dataTableProperties = new()
        {
            TableName = "NonExistent",
            ColumnProperties = new ColumnProperties[]
            {
                new ColumnProperties { ColumnName = "NonExistent" }
            }
        };
        _objectListStorage!.Initialize(dataTableProperties, pipeSeparator);

        Assert.Throws<DirectoryNotFoundException>(() => _objectListStorage!.WriteObjectList(dataFilePath, nonExistents, RecentPathsConfigs.WriteFunc, new Diagnostics()));

        Assert.IsFalse(File.Exists(dataFilePath));
    }

    [Test]
    public void WriteObjectList_PathIsNull_ThrowsArgumentNullException()
    {
        string? dataFilePath = null;
        List<string> nonExistents = new()
        {
            "D:\\Workspace\\PhotoManager\\Test"
        };

        DataTableProperties dataTableProperties = new()
        {
            TableName = "NonExistent",
            ColumnProperties = new ColumnProperties[]
            {
                new ColumnProperties { ColumnName = "NonExistent" }
            }
        };
        _objectListStorage!.Initialize(dataTableProperties, pipeSeparator);

        Assert.Throws<ArgumentNullException>(() => _objectListStorage!.WriteObjectList(dataFilePath!, nonExistents, RecentPathsConfigs.WriteFunc, new Diagnostics()));

        Assert.IsFalse(File.Exists(dataFilePath));
    }

    [Test]
    public void WriteObjectList_ListIsNull_ThrowsNullReferenceException()
    {
        string dataFilePath = Path.Combine(dataDirectory!, "nonExistent.db");
        List<string>? nonExistents = null;

        DataTableProperties dataTableProperties = new()
        {
            TableName = "NonExistent",
            ColumnProperties = new ColumnProperties[]
            {
                new ColumnProperties { ColumnName = "NonExistent" }
            }
        };
        _objectListStorage!.Initialize(dataTableProperties, pipeSeparator);

        Assert.Throws<NullReferenceException>(() =>
            _objectListStorage!.WriteObjectList(
                dataFilePath,
                nonExistents!,
                (f, i) =>
                {
                    return i switch
                    {
                        0 => f,
                        _ => throw new ArgumentOutOfRangeException(nameof(i))
                    };
                },
                new Diagnostics()));

        Assert.IsFalse(File.Exists(dataFilePath));
    }

    [Test]
    public void WriteObjectList_FuncIsNull_ThrowsNullReferenceException()
    {
        string dataFilePath = Path.Combine(dataDirectory!, "nonExistent.db");
        List<string> nonExistents = new()
        {
            "D:\\Workspace\\PhotoManager\\Test"
        };

        DataTableProperties dataTableProperties = new()
        {
            TableName = "NonExistent",
            ColumnProperties = new ColumnProperties[]
            {
                new ColumnProperties { ColumnName = "NonExistent" }
            }
        };
        _objectListStorage!.Initialize(dataTableProperties, pipeSeparator);
        Func<string, int, object>? f = null;

        Assert.Throws<NullReferenceException>(() =>
            _objectListStorage!.WriteObjectList(
                dataFilePath,
                nonExistents,
                f!,
                new Diagnostics()));

        Assert.IsFalse(File.Exists(dataFilePath));
    }
}
