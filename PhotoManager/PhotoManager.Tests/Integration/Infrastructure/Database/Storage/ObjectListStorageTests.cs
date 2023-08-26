using NUnit.Framework;
using PhotoManager.Constants;
using PhotoManager.Domain;
using PhotoManager.Infrastructure.Database;
using PhotoManager.Infrastructure.Database.Storage;
using System.IO;
using System.Windows.Media.Imaging;

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
        string directoryName = Path.GetDirectoryName(typeof(ObjectListStorageTests).Assembly.Location) ?? "";
        dataDirectory = Path.Combine(directoryName, "TestFiles");
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
            TableName = "Folder",
            ColumnProperties = new ColumnProperties[]
            {
                new ColumnProperties { ColumnName = "FolderId" },
                new ColumnProperties { ColumnName = "Path" }
            }
        };
        _objectListStorage!.Initialize(dataTableProperties, pipeSeparator);

        var result = _objectListStorage!.ReadObjectList(
            dataFilePath,
            f =>
                new Folder
                {
                    FolderId = f[0],
                    Path = f[1]
                },
            new Diagnostics());

        Assert.IsInstanceOf<List<Folder>>(result);
        Assert.IsNotEmpty(result);
        Assert.AreEqual(3, result.Count);

        Folder? folder1 = result.FirstOrDefault(x => x.FolderId == "f1f00403-0554-4201-9b6b-11a6b4cea3a9");
        Assert.IsNotNull(folder1);
        Assert.AreEqual("D:\\Workspace\\PhotoManager\\Test", folder1!.Path);

        Folder? folder2 = result.FirstOrDefault(x => x.FolderId == "2c107211-1a1c-4e73-8e8b-35d18ca8ef85");
        Assert.IsNotNull(folder2);
        Assert.AreEqual("D:\\Workspace\\PhotoManager\\Test\\OutputVideoFirstFrame", folder2!.Path);

        Folder? folder3 = result.FirstOrDefault(x => x.FolderId == "18033543-defb-4d37-837b-d8063eda3a25");
        Assert.IsNotNull(folder3);
        Assert.AreEqual("D:\\Workspace\\PhotoManager\\Test\\toto", folder3!.Path);
    }

    [Test]
    public void ReadObjectList_AssetType_ReturnsNotEmptyList()
    {
        string dataFilePath = Path.Combine(dataDirectory!, "TestBackup\\v1.0\\Tables\\asset.db");
        DataTableProperties dataTableProperties = new()
        {
            TableName = "Asset",
            ColumnProperties = new ColumnProperties[]
            {
                new ColumnProperties { ColumnName = "FolderId" },
                new ColumnProperties { ColumnName = "FileName" },
                new ColumnProperties { ColumnName = "FileSize" },
                new ColumnProperties { ColumnName = "ImageRotation" },
                new ColumnProperties { ColumnName = "PixelWidth" },
                new ColumnProperties { ColumnName = "PixelHeight" },
                new ColumnProperties { ColumnName = "ThumbnailPixelWidth" },
                new ColumnProperties { ColumnName = "ThumbnailPixelHeight" },
                new ColumnProperties { ColumnName = "ThumbnailCreationDateTime" },
                new ColumnProperties { ColumnName = "Hash" },
                new ColumnProperties { ColumnName = "AssetCorruptedMessage" },
                new ColumnProperties { ColumnName = "IsAssetCorrupted" },
                new ColumnProperties { ColumnName = "AssetRotatedMessage" },
                new ColumnProperties { ColumnName = "IsAssetRotated" }
            }
        };
        _objectListStorage!.Initialize(dataTableProperties, pipeSeparator);

        var result = _objectListStorage!.ReadObjectList(
            dataFilePath,
            f =>
                new Asset
                {
                    FolderId = f[0],
                    FileName = f[1],
                    FileSize = long.Parse(f[2]),
                    ImageRotation = (Rotation)Enum.Parse(typeof(Rotation), f[3]),
                    PixelWidth = int.Parse(f[4]),
                    PixelHeight = int.Parse(f[5]),
                    ThumbnailPixelWidth = int.Parse(f[6]),
                    ThumbnailPixelHeight = int.Parse(f[7]),
                    ThumbnailCreationDateTime = DateTime.Parse(f[8]),
                    Hash = f[9],
                    AssetCorruptedMessage = f[10],
                    IsAssetCorrupted = bool.Parse(f[11]),
                    AssetRotatedMessage = f[12],
                    IsAssetRotated = bool.Parse(f[13])
                },
            new Diagnostics());

        Assert.IsInstanceOf<List<Asset>>(result);
        Assert.IsNotEmpty(result);
        Assert.AreEqual(11, result.Count);

        Asset? asset = result.FirstOrDefault(x => x.Hash == "ee43714d8b96d7ed3308d18afcb701444198c783fbe4103ce44e95aaf99c2095ae70e6e2035a7a438d1598fadaf5fe8cb0d541378387d20e91f26819fcc64b82");
        Assert.IsNotNull(asset);
        Assert.AreEqual("533.JPG", asset!.FileName);
        Assert.AreEqual("f1f00403-0554-4201-9b6b-11a6b4cea3a9", asset!.FolderId);
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
            TableName = "Import",
            ColumnProperties = new ColumnProperties[]
            {
                new ColumnProperties { ColumnName = "SourceDirectory" },
                new ColumnProperties { ColumnName = "DestinationDirectory" },
                new ColumnProperties { ColumnName = "IncludeSubFolders" },
                new ColumnProperties { ColumnName = "DeleteAssetsNotInSource" }
            }
        };
        _objectListStorage!.Initialize(dataTableProperties, pipeSeparator);

        var result = _objectListStorage!.ReadObjectList(
            dataFilePath,
            f =>
                new SyncAssetsDirectoriesDefinition
                {
                    SourceDirectory = f[0],
                    DestinationDirectory = f[1],
                    IncludeSubFolders = bool.Parse(f[2]),
                    DeleteAssetsNotInSource = f.Length > 3 && bool.Parse(f[3])
                },
            new Diagnostics());

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
    public void ReadObjectList_RecentTargetPathsType_ReturnsNotList()
    {
        string dataFilePath = Path.Combine(dataDirectory!, "TestBackup\\v1.0\\Tables\\recenttargetpaths.db");
        DataTableProperties dataTableProperties = new()
        {
            TableName = "RecentTargetPaths",
            ColumnProperties = new ColumnProperties[]
            {
                new ColumnProperties { ColumnName = "Path" }
            }
        };
        _objectListStorage!.Initialize(dataTableProperties, pipeSeparator);

        var result = _objectListStorage!.ReadObjectList(dataFilePath, f => f[0], new Diagnostics());

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
            TableName = "Import",
            ColumnProperties = new ColumnProperties[]
            {
                new ColumnProperties { ColumnName = "SourceDirectory" },
                new ColumnProperties { ColumnName = "DestinationDirectory" },
                new ColumnProperties { ColumnName = "IncludeSubFolders" },
                new ColumnProperties { ColumnName = "DeleteAssetsNotInSource" }
            }
        };
        _objectListStorage!.Initialize(dataTableProperties, pipeSeparator);

        var result = _objectListStorage!.ReadObjectList(
            dataFilePath,
            f =>
                new SyncAssetsDirectoriesDefinition
                {
                    SourceDirectory = f[0],
                    DestinationDirectory = f[1],
                    IncludeSubFolders = bool.Parse(f[2]),
                    DeleteAssetsNotInSource = f.Length > 3 && bool.Parse(f[3])
                },
            new Diagnostics());

        Assert.IsInstanceOf<List<SyncAssetsDirectoriesDefinition>>(result);
        Assert.IsEmpty(result);
    }

    [Test]
    public void ReadObjectList_FolderTypeWithoutProperties_ReturnsNotEmptyList()
    {
        string dataFilePath = Path.Combine(dataDirectory!, "TestBackup\\v1.0\\Tables\\folder.db");
        DataTableProperties? dataTableProperties = null;
        _objectListStorage!.Initialize(dataTableProperties!, pipeSeparator);

        var result = _objectListStorage!.ReadObjectList(
            dataFilePath,
            f =>
                new Folder
                {
                    FolderId = f[0],
                    Path = f[1]
                },
            new Diagnostics());

        Assert.IsInstanceOf<List<Folder>>(result);
        Assert.IsNotEmpty(result);
        Assert.AreEqual(3, result.Count);
        Assert.IsNotNull(result.FirstOrDefault(x => x.FolderId == "f1f00403-0554-4201-9b6b-11a6b4cea3a9"));
    }

    [Test]
    public void ReadObjectList_FolderTypeWithoutInitialize_ThrowsIndexOutOfRangeException()
    {
        string dataFilePath = Path.Combine(dataDirectory!, "TestBackup\\v1.0\\Tables\\folder.db");

        Assert.Throws<IndexOutOfRangeException>(() =>
        {
            _objectListStorage!.ReadObjectList(
            dataFilePath,
            f =>
                new Folder
                {
                    FolderId = f[0],
                    Path = f[1]
                },
            new Diagnostics());
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

        var result = _objectListStorage!.ReadObjectList(dataFilePath, f => f[0], new Diagnostics());

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

        var result = _objectListStorage!.ReadObjectList(dataFilePath, f => f[0], new Diagnostics());

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

        var result = _objectListStorage!.ReadObjectList(dataFilePath!, f => f[0], new Diagnostics());

        Assert.IsEmpty(result);
    }

    [Test]
    public void ReadObjectList_FolderTypeFuncIsNull_ThrowsNullReferenceException()
    {
        string dataFilePath = Path.Combine(dataDirectory!, "TestBackup\\v1.0\\Tables\\folder.db");
        DataTableProperties dataTableProperties = new()
        {
            TableName = "Folder",
            ColumnProperties = new ColumnProperties[]
            {
                new ColumnProperties { ColumnName = "FolderId" },
                new ColumnProperties { ColumnName = "Path" }
            }
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
                FolderId = "dfc8aab7-3543-48e7-9fdc-596ba733761e",
                Path = "D:\\Workspace\\PhotoManager\\Test"
            },
            new Folder
            {
                FolderId = "8f9dff55-4a15-411e-a4cb-7ec3024b2238",
                Path = "D:\\Workspace\\PhotoManager\\Test\\OutputVideoFirstFrame"
            },
            new Folder
            {
                FolderId = "db4d226f-6901-43f9-9e82-e6a052f627d2",
                Path = "D:\\Workspace\\PhotoManager\\Test\\toto"
            }
        };

        DataTableProperties dataTableProperties = new()
        {
            TableName = "Folder",
            ColumnProperties = new ColumnProperties[]
            {
                new ColumnProperties { ColumnName = "FolderId" },
                new ColumnProperties { ColumnName = "Path" }
            }
        };

        try
        {
            _objectListStorage!.Initialize(dataTableProperties, pipeSeparator);
            _objectListStorage!.WriteObjectList(
                dataFilePath,
                folders,
                (f, i) =>
                {
                    return i switch
                    {
                        0 => f.FolderId,
                        1 => f.Path,
                        _ => throw new ArgumentOutOfRangeException(nameof(i))
                    };
                },
                new Diagnostics());

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
                FolderId = "dfc8aab7-3543-48e7-9fdc-596ba733761e",
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
                FolderId = "dfc8aab7-3543-48e7-9fdc-596ba733761f",
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
            TableName = "Asset",
            ColumnProperties = new ColumnProperties[]
            {
                new ColumnProperties { ColumnName = "FolderId" },
                new ColumnProperties { ColumnName = "FileName" },
                new ColumnProperties { ColumnName = "FileSize" },
                new ColumnProperties { ColumnName = "ImageRotation" },
                new ColumnProperties { ColumnName = "PixelWidth" },
                new ColumnProperties { ColumnName = "PixelHeight" },
                new ColumnProperties { ColumnName = "ThumbnailPixelWidth" },
                new ColumnProperties { ColumnName = "ThumbnailPixelHeight" },
                new ColumnProperties { ColumnName = "ThumbnailCreationDateTime" },
                new ColumnProperties { ColumnName = "Hash" },
                new ColumnProperties { ColumnName = "AssetCorruptedMessage" },
                new ColumnProperties { ColumnName = "IsAssetCorrupted" },
                new ColumnProperties { ColumnName = "AssetRotatedMessage" },
                new ColumnProperties { ColumnName = "IsAssetRotated" }
            }
        };

        try
        {
            _objectListStorage!.Initialize(dataTableProperties, pipeSeparator);
            _objectListStorage!.WriteObjectList(
                dataFilePath,
                assets,
                (a, i) =>
                {
                    return i switch
                    {
                        0 => a.FolderId,
                        1 => a.FileName,
                        2 => a.FileSize,
                        3 => a.ImageRotation,
                        4 => a.PixelWidth,
                        5 => a.PixelHeight,
                        6 => a.ThumbnailPixelWidth,
                        7 => a.ThumbnailPixelHeight,
                        8 => a.ThumbnailCreationDateTime,
                        9 => a.Hash,
                        10 => a.AssetCorruptedMessage!,
                        11 => a.IsAssetCorrupted,
                        12 => a.AssetRotatedMessage!,
                        13 => a.IsAssetRotated,
                        _ => throw new ArgumentOutOfRangeException(nameof(i))
                    };
                },
                new Diagnostics());

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
            TableName = "Import",
            ColumnProperties = new ColumnProperties[]
            {
                new ColumnProperties { ColumnName = "SourceDirectory" },
                new ColumnProperties { ColumnName = "DestinationDirectory" },
                new ColumnProperties { ColumnName = "IncludeSubFolders" },
                new ColumnProperties { ColumnName = "DeleteAssetsNotInSource" }
            }
        };

        try
        {
            _objectListStorage!.Initialize(dataTableProperties, pipeSeparator);
            _objectListStorage!.WriteObjectList(
                dataFilePath,
                imports,
                (d, i) =>
                {
                    return i switch
                    {
                        0 => d.SourceDirectory,
                        1 => d.DestinationDirectory,
                        2 => d.IncludeSubFolders,
                        3 => d.DeleteAssetsNotInSource,
                        _ => throw new ArgumentOutOfRangeException(nameof(i))
                    };
                },
                new Diagnostics());

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
            TableName = "RecentTargetPaths",
            ColumnProperties = new ColumnProperties[]
            {
                new ColumnProperties { ColumnName = "Path" }
            }
        };

        try
        {
            _objectListStorage!.Initialize(dataTableProperties, pipeSeparator);
            _objectListStorage!.WriteObjectList(
                dataFilePath,
                recentTargetPaths,
                (p, i) =>
                {
                    return i switch
                    {
                        0 => p,
                        _ => throw new ArgumentOutOfRangeException(nameof(i))
                    };
                },
                new Diagnostics());

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
                FolderId = "dfc8aab7-3543-48e7-9fdc-596ba733761e",
                Path = "D:\\Workspace\\PhotoManager\\Test"
            },
            new Folder
            {
                FolderId = "8f9dff55-4a15-411e-a4cb-7ec3024b2238",
                Path = "D:\\Workspace\\PhotoManager\\Test\\OutputVideoFirstFrame"
            },
            new Folder
            {
                FolderId = "db4d226f-6901-43f9-9e82-e6a052f627d2",
                Path = "D:\\Workspace\\PhotoManager\\Test\\toto"
            }
        };

        DataTableProperties? dataTableProperties = null;

        _objectListStorage!.Initialize(dataTableProperties, pipeSeparator);

        var exception = Assert.Throws<Exception>(() =>
            _objectListStorage!.WriteObjectList(
                dataFilePath,
                folders,
                (f, i) =>
                {
                    return i switch
                    {
                        0 => f.FolderId,
                        1 => f.Path,
                        _ => throw new ArgumentOutOfRangeException(nameof(i))
                    };
                },
                new Diagnostics()));

        Assert.AreEqual($"Properties must be defined for the columns in the table NoTableName.", exception?.Message);

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
                FolderId = "dfc8aab7-3543-48e7-9fdc-596ba733761e",
                Path = "D:\\Workspace\\PhotoManager\\Test"
            },
            new Folder
            {
                FolderId = "8f9dff55-4a15-411e-a4cb-7ec3024b2238",
                Path = "D:\\Workspace\\PhotoManager\\Test\\OutputVideoFirstFrame"
            },
            new Folder
            {
                FolderId = "db4d226f-6901-43f9-9e82-e6a052f627d2",
                Path = "D:\\Workspace\\PhotoManager\\Test\\toto"
            }
        };

        DataTableProperties dataTableProperties = new()
        {
            TableName = "Folder",
            ColumnProperties = new ColumnProperties[]
            {
                new ColumnProperties { ColumnName = "FolderId" },
                new ColumnProperties { ColumnName = "Path" }
            }
        };

        var exception = Assert.Throws<Exception>(() =>
            _objectListStorage!.WriteObjectList(
                dataFilePath,
                folders,
                (f, i) =>
                {
                    return i switch
                    {
                        0 => f.FolderId,
                        1 => f.Path,
                        _ => throw new ArgumentOutOfRangeException(nameof(i))
                    };
                },
                new Diagnostics()));


        Assert.AreEqual($"Properties must be defined for the columns in the table NoTableName.", exception?.Message);

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

        Assert.Throws<DirectoryNotFoundException>(() =>
            _objectListStorage!.WriteObjectList(
                dataFilePath,
                nonExistents,
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

        Assert.Throws<ArgumentNullException>(() =>
            _objectListStorage!.WriteObjectList(
                dataFilePath!,
                nonExistents,
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
