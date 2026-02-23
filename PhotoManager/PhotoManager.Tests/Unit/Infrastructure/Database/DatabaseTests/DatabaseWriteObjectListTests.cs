using Directories = PhotoManager.Tests.Unit.Constants.Directories;

namespace PhotoManager.Tests.Unit.Infrastructure.Database.DatabaseTests;

[TestFixture]
public class DatabaseWriteObjectListTests
{
    private string? _dataDirectory;

    private PhotoManager.Infrastructure.Database.Database? _database;
    private UserConfigurationService? _userConfigurationService;

    private string? _csvEscapedTextWithSemicolon;
    private string? _csvUnescapedTextWithSemicolon;
    private string? _csvEscapedTextWithPipe;
    private string? _csvUnescapedTextWithPipe;
    private string? _csvSomeUnescapedTextWithPipe;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);

        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();

        _userConfigurationService = new(configurationRootMock.Object);

        _csvEscapedTextWithSemicolon =
            "\"FolderId\";\"FileName\";\"ImageRotation\";\"PixelWidth\";\"PixelHeight\";\"ThumbnailPixelWidth\";\"ThumbnailPixelHeight\";\"ThumbnailCreationDateTime\";\"Hash\";\"CorruptedMessage\";\"IsCorrupted\";\"RotatedMessage\";\"IsRotated\"\r\n" +
            "\"876283c6-780e-4ad5-975c-be63044c087a\";\"20200720175810_3.jpg\";\"Rotate0\";\"1920\";\"1080\";\"200\";\"112\";\"8/19/2023 11:26:09\";\"4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4\";\"\";\"False\";\"\";\"False\"\r\n" +
            "\"876283c6-780e-4ad5-975c-be63044c087a\";\"20200720175816_3.jpg\";\"Rotate0\";\"1920\";\"1080\";\"200\";\"112\";\"8/19/2023 11:26:09\";\"0af8f118b7d606e5d174643727bd3c0c6028b52c50481585274fd572110b108c7a0d7901227f75a72b44c89335e002a65e8137ff5b238ab1c0bba0505e783124\";\"The asset is corrupted\";\"True\";\"The asset has been rotated\";\"True\"\r\n";

        _csvUnescapedTextWithSemicolon =
            "FolderId;FileName;ImageRotation;PixelWidth;PixelHeight;ThumbnailPixelWidth;ThumbnailPixelHeight;ThumbnailCreationDateTime;Hash;CorruptedMessage;IsCorrupted;RotatedMessage;IsRotated\r\n" +
            "876283c6-780e-4ad5-975c-be63044c087a;20200720175810_3.jpg;Rotate0;1920;1080;200;112;8/19/2023 11:26:09;4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4;;False;;False\r\n" +
            "876283c6-780e-4ad5-975c-be63044c087a;20200720175816_3.jpg;Rotate0;1920;1080;200;112;8/19/2023 11:26:09;0af8f118b7d606e5d174643727bd3c0c6028b52c50481585274fd572110b108c7a0d7901227f75a72b44c89335e002a65e8137ff5b238ab1c0bba0505e783124;The asset is corrupted;True;The asset has been rotated;True\r\n";

        _csvEscapedTextWithPipe =
            "\"FolderId\"|\"FileName\"|\"ImageRotation\"|\"PixelWidth\"|\"PixelHeight\"|\"ThumbnailPixelWidth\"|\"ThumbnailPixelHeight\"|\"ThumbnailCreationDateTime\"|\"Hash\"|\"CorruptedMessage\"|\"IsCorrupted\"|\"RotatedMessage\"|\"IsRotated\"\r\n" +
            "\"876283c6-780e-4ad5-975c-be63044c087a\"|\"20200720175810_3.jpg\"|\"Rotate0\"|\"1920\"|\"1080\"|\"200\"|\"112\"|\"8/19/2023 11:26:09\"|\"4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4\"|\"\"|\"False\"|\"\"|\"False\"\r\n" +
            "\"876283c6-780e-4ad5-975c-be63044c087a\"|\"20200720175816_3.jpg\"|\"Rotate0\"|\"1920\"|\"1080\"|\"200\"|\"112\"|\"8/19/2023 11:26:09\"|\"0af8f118b7d606e5d174643727bd3c0c6028b52c50481585274fd572110b108c7a0d7901227f75a72b44c89335e002a65e8137ff5b238ab1c0bba0505e783124\"|\"The asset is corrupted\"|\"True\"|\"The asset has been rotated\"|\"True\"\r\n";

        _csvUnescapedTextWithPipe =
            "FolderId|FileName|ImageRotation|PixelWidth|PixelHeight|ThumbnailPixelWidth|ThumbnailPixelHeight|ThumbnailCreationDateTime|Hash|CorruptedMessage|IsCorrupted|RotatedMessage|IsRotated\r\n" +
            "876283c6-780e-4ad5-975c-be63044c087a|20200720175810_3.jpg|Rotate0|1920|1080|200|112|8/19/2023 11:26:09|4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4||False||False\r\n" +
            "876283c6-780e-4ad5-975c-be63044c087a|20200720175816_3.jpg|Rotate0|1920|1080|200|112|8/19/2023 11:26:09|0af8f118b7d606e5d174643727bd3c0c6028b52c50481585274fd572110b108c7a0d7901227f75a72b44c89335e002a65e8137ff5b238ab1c0bba0505e783124|The asset is corrupted|True|The asset has been rotated|True\r\n";

        _csvSomeUnescapedTextWithPipe =
            "\"FolderId\"|FileName|ImageRotation|PixelWidth|PixelHeight|ThumbnailPixelWidth|ThumbnailPixelHeight|ThumbnailCreationDateTime|Hash|CorruptedMessage|IsCorrupted|RotatedMessage|\"IsRotated\"\r\n" +
            "\"876283c6-780e-4ad5-975c-be63044c087a\"|20200720175810_3.jpg|Rotate0|1920|1080|200|112|8/19/2023 11:26:09|4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4||False||\"False\"\r\n" +
            "\"876283c6-780e-4ad5-975c-be63044c087a\"|20200720175816_3.jpg|Rotate0|1920|1080|200|112|8/19/2023 11:26:09|0af8f118b7d606e5d174643727bd3c0c6028b52c50481585274fd572110b108c7a0d7901227f75a72b44c89335e002a65e8137ff5b238ab1c0bba0505e783124|The asset is corrupted|True|The asset has been rotated|\"True\"\r\n";
    }

    [SetUp]
    public void SetUp()
    {
        _database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
    }

    [Test]
    [TestCase("EscapedText", true)]
    [TestCase("UnescapedText", false)]
    public void WriteObjectList_AllColumnsAndSemicolonSeparator_WritesCorrectCsv(string csvType, bool escapeText)
    {
        string csv = csvType == "EscapedText" ? _csvEscapedTextWithSemicolon! : _csvUnescapedTextWithSemicolon!;

        string tableName = "assets" + Guid.NewGuid();
        string directoryPath = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS);
        string filePath = Path.Combine(directoryPath,
            _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, tableName + ".db");

        try
        {
            WriteObjectList(directoryPath, Constants.SEMICOLON_SEPARATOR, tableName, escapeText);

            Asserts(filePath, csv);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS), true);
        }
    }

    [Test]
    [TestCase("EscapedText", true)]
    [TestCase("UnescapedText", false)]
    public void WriteObjectList_AllColumnsAndPipeSeparator_WritesCorrectCsv(string csvType, bool escapeText)
    {
        string csv = csvType == "EscapedText" ? _csvEscapedTextWithPipe! : _csvUnescapedTextWithPipe!;

        string tableName = "assets" + Guid.NewGuid();
        string directoryPath = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS);
        string filePath = Path.Combine(directoryPath,
            _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, tableName + ".db");

        try
        {
            WriteObjectList(directoryPath, _userConfigurationService!.StorageSettings.Separator, tableName, escapeText);

            Asserts(filePath, csv);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS), true);
        }
    }

    [Test]
    public void WriteObjectList_AllColumnsAndPipeSeparatorWithoutDataTableProperties_ThrowsException()
    {
        string tableName = "assets" + Guid.NewGuid();
        string directoryPath = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS);
        string filePath = Path.Combine(directoryPath,
            _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, tableName + ".db");

        try
        {
            List<Asset> assets =
            [
                new()
                {
                    FolderId = new("876283c6-780e-4ad5-975c-be63044c087a"),
                    Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
                    FileName = "20200720175810_3.jpg",
                    ImageRotation = Rotation.Rotate0,
                    Pixel = new()
                    {
                        Asset = new() { Width = 1920, Height = 1080 },
                        Thumbnail = new() { Width = 200, Height = 112 }
                    },
                    FileProperties = new() { Size = 363888 },
                    ThumbnailCreationDateTime = new(2023, 8, 19, 11, 26, 09),
                    Hash =
                        "4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4",
                    Metadata = new()
                    {
                        Corrupted = new() { IsTrue = false, Message = null },
                        Rotated = new() { IsTrue = false, Message = null }
                    }
                },
                new()
                {
                    FolderId = new("876283c6-780e-4ad5-975c-be63044c087a"),
                    Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
                    FileName = "20200720175816_3.jpg",
                    ImageRotation = Rotation.Rotate0,
                    Pixel = new()
                    {
                        Asset = new() { Width = 1920, Height = 1080 },
                        Thumbnail = new() { Width = 200, Height = 112 }
                    },
                    FileProperties = new() { Size = 343633 },
                    ThumbnailCreationDateTime = new(2023, 8, 19, 11, 26, 09),
                    Hash =
                        "0af8f118b7d606e5d174643727bd3c0c6028b52c50481585274fd572110b108c7a0d7901227f75a72b44c89335e002a65e8137ff5b238ab1c0bba0505e783124",
                    Metadata = new()
                    {
                        Corrupted = new() { IsTrue = true, Message = "The asset is corrupted" },
                        Rotated = new() { IsTrue = true, Message = "The asset has been rotated" }
                    }
                }
            ];

            _database!.Initialize(
                directoryPath,
                _userConfigurationService!.StorageSettings.Separator,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);

            Exception? exception = Assert.Throws<Exception>(() =>
                _database!.WriteObjectList(assets, tableName, AssetConfigs.WriteFunc));

            Assert.That(exception?.Message,
                Is.EqualTo("Properties must be defined for the columns in the table NoTableName."));

            Assert.That(File.Exists(filePath), Is.False);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS), true);
        }
    }

    [Test]
    public void WriteObjectList_SomeColumnsWithEscapedTextAndPipeSeparator_WritesCorrectCsv()
    {
        string csv = _csvSomeUnescapedTextWithPipe!;

        string tableName = "assets" + Guid.NewGuid();
        string directoryPath = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS);
        string filePath = Path.Combine(directoryPath,
            _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, tableName + ".db");

        try
        {
            List<Asset> assets =
            [
                new()
                {
                    FolderId = new("876283c6-780e-4ad5-975c-be63044c087a"),
                    Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
                    FileName = "20200720175810_3.jpg",
                    ImageRotation = Rotation.Rotate0,
                    Pixel = new()
                    {
                        Asset = new() { Width = 1920, Height = 1080 },
                        Thumbnail = new() { Width = 200, Height = 112 }
                    },
                    FileProperties = new() { Size = 363888 },
                    ThumbnailCreationDateTime = new(2023, 8, 19, 11, 26, 09),
                    Hash =
                        "4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4",
                    Metadata = new()
                    {
                        Corrupted = new() { IsTrue = false, Message = null },
                        Rotated = new() { IsTrue = false, Message = null }
                    }
                },
                new()
                {
                    FolderId = new("876283c6-780e-4ad5-975c-be63044c087a"),
                    Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
                    FileName = "20200720175816_3.jpg",
                    ImageRotation = Rotation.Rotate0,
                    Pixel = new()
                    {
                        Asset = new() { Width = 1920, Height = 1080 },
                        Thumbnail = new() { Width = 200, Height = 112 }
                    },
                    FileProperties = new() { Size = 343633 },
                    ThumbnailCreationDateTime = new(2023, 8, 19, 11, 26, 09),
                    Hash =
                        "0af8f118b7d606e5d174643727bd3c0c6028b52c50481585274fd572110b108c7a0d7901227f75a72b44c89335e002a65e8137ff5b238ab1c0bba0505e783124",
                    Metadata = new()
                    {
                        Corrupted = new() { IsTrue = true, Message = "The asset is corrupted" },
                        Rotated = new() { IsTrue = true, Message = "The asset has been rotated" }
                    }
                }
            ];

            _database!.Initialize(
                directoryPath,
                _userConfigurationService!.StorageSettings.Separator,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);

            _database!.SetDataTableProperties(new()
            {
                TableName = tableName,
                ColumnProperties =
                [
                    new() { ColumnName = "FolderId", EscapeText = true },
                    new() { ColumnName = "FileName", EscapeText = false },
                    new() { ColumnName = "ImageRotation", EscapeText = false },
                    new() { ColumnName = "PixelWidth", EscapeText = false },
                    new() { ColumnName = "PixelHeight", EscapeText = false },
                    new() { ColumnName = "ThumbnailPixelWidth", EscapeText = false },
                    new() { ColumnName = "ThumbnailPixelHeight", EscapeText = false },
                    new() { ColumnName = "ThumbnailCreationDateTime", EscapeText = false },
                    new() { ColumnName = "Hash", EscapeText = false },
                    new() { ColumnName = "CorruptedMessage", EscapeText = false },
                    new() { ColumnName = "IsCorrupted", EscapeText = false },
                    new() { ColumnName = "RotatedMessage", EscapeText = false },
                    new() { ColumnName = "IsRotated", EscapeText = true }
                ]
            });

            _database!.WriteObjectList(assets, tableName, AssetConfigs.WriteFunc);

            Asserts(filePath, csv);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS), true);
        }
    }

    [Test]
    public void WriteObjectList_SomeColumnsInvalidsAndPipeSeparator_WritesIncorrectCsv()
    {
        string csv = _csvUnescapedTextWithPipe!;

        string tableName = "assets" + Guid.NewGuid();
        string directoryPath = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS);
        string filePath = Path.Combine(directoryPath,
            _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, tableName + ".db");

        try
        {
            List<Asset> assets =
            [
                new()
                {
                    FolderId = new("876283c6-780e-4ad5-975c-be63044c087a"),
                    Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
                    FileName = "20200720175810_3.jpg",
                    ImageRotation = Rotation.Rotate0,
                    Pixel = new()
                    {
                        Asset = new() { Width = 1920, Height = 1080 },
                        Thumbnail = new() { Width = 200, Height = 112 }
                    },
                    FileProperties = new() { Size = 363888 },
                    ThumbnailCreationDateTime = new(2023, 8, 19, 11, 26, 09),
                    Hash =
                        "4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4",
                    Metadata = new()
                    {
                        Corrupted = new() { IsTrue = false, Message = null },
                        Rotated = new() { IsTrue = false, Message = null }
                    }
                },
                new()
                {
                    FolderId = new("876283c6-780e-4ad5-975c-be63044c087a"),
                    Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
                    FileName = "20200720175816_3.jpg",
                    ImageRotation = Rotation.Rotate0,
                    Pixel = new()
                    {
                        Asset = new() { Width = 1920, Height = 1080 },
                        Thumbnail = new() { Width = 200, Height = 112 }
                    },
                    FileProperties = new() { Size = 343633 },
                    ThumbnailCreationDateTime = new(2023, 8, 19, 11, 26, 09),
                    Hash =
                        "0af8f118b7d606e5d174643727bd3c0c6028b52c50481585274fd572110b108c7a0d7901227f75a72b44c89335e002a65e8137ff5b238ab1c0bba0505e783124",
                    Metadata = new()
                    {
                        Corrupted = new() { IsTrue = true, Message = "The asset is corrupted" },
                        Rotated = new() { IsTrue = true, Message = "The asset has been rotated" }
                    }
                }
            ];

            _database!.Initialize(
                directoryPath,
                _userConfigurationService!.StorageSettings.Separator,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);

            _database!.SetDataTableProperties(new()
            {
                TableName = tableName,
                ColumnProperties =
                [
                    new() { ColumnName = "FolderId", EscapeText = false },
                    new() { ColumnName = "Toto", EscapeText = false },
                    new() { ColumnName = "ImageRotation", EscapeText = false },
                    new() { ColumnName = "PixelWidth", EscapeText = false },
                    new() { ColumnName = "PixelHeight", EscapeText = false },
                    new() { ColumnName = "ThumbnailPixelWidth", EscapeText = false },
                    new() { ColumnName = "ThumbnailPixelHeight", EscapeText = false },
                    new() { ColumnName = "ThumbnailCreationDateTime", EscapeText = false },
                    new() { ColumnName = "Tutu", EscapeText = false },
                    new() { ColumnName = "CorruptedMessage", EscapeText = false },
                    new() { ColumnName = "IsCorrupted", EscapeText = false },
                    new() { ColumnName = "RotatedMessage", EscapeText = false },
                    new() { ColumnName = "Tata", EscapeText = false }
                ]
            });

            _database!.WriteObjectList(assets, tableName, AssetConfigs.WriteFunc);

            Assert.That(File.Exists(filePath), Is.True);

            Assert.That(_database!.Diagnostics.LastWriteFilePath, Is.EqualTo(filePath));
            Assert.That(_database!.Diagnostics.LastWriteFileRaw, Is.Not.EqualTo(csv));

            string fileContent = File.ReadAllText(filePath);
            Assert.That(fileContent, Is.Not.EqualTo(csv));
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS), true);
        }
    }

    [Test]
    public void WriteObjectList_ListIsNullAndAllColumnsWithUnescapedTextAndPipeSeparator_ThrowsArgumentNullException()
    {
        string tableName = "assets" + Guid.NewGuid();
        string directoryPath = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS);
        string filePath = Path.Combine(directoryPath,
            _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, tableName + ".db");

        try
        {
            List<Asset>? assets = null;

            _database!.Initialize(
                directoryPath,
                _userConfigurationService!.StorageSettings.Separator,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);

            _database!.SetDataTableProperties(new()
            {
                TableName = tableName,
                ColumnProperties = AssetConfigs.ConfigureDataTable()
            });

            ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() =>
                _database!.WriteObjectList(assets!, tableName, AssetConfigs.WriteFunc));

            Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'list')"));
            Assert.That(File.Exists(filePath), Is.False);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS), true);
        }
    }

    [Test]
    [TestCase("")]
    [TestCase(" ")]
    public void WriteObjectList_TableNameIsInvalidAllColumnsWithEscapedTextAndPipeSeparator_ThrowsArgumentNullException(
        string tableName)
    {
        string directoryPath = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS);
        string filePath = Path.Combine(directoryPath,
            _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, tableName + ".db");

        try
        {
            List<Asset> assets =
            [
                new()
                {
                    FolderId = new("876283c6-780e-4ad5-975c-be63044c087a"),
                    Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
                    FileName = "20200720175810_3.jpg",
                    ImageRotation = Rotation.Rotate0,
                    Pixel = new()
                    {
                        Asset = new() { Width = 1920, Height = 1080 },
                        Thumbnail = new() { Width = 200, Height = 112 }
                    },
                    FileProperties = new() { Size = 363888 },
                    ThumbnailCreationDateTime = new(2023, 8, 19, 11, 26, 09),
                    Hash =
                        "4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4",
                    Metadata = new()
                    {
                        Corrupted = new() { IsTrue = false, Message = null },
                        Rotated = new() { IsTrue = false, Message = null }
                    }
                },
                new()
                {
                    FolderId = new("876283c6-780e-4ad5-975c-be63044c087a"),
                    Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
                    FileName = "20200720175816_3.jpg",
                    ImageRotation = Rotation.Rotate0,
                    Pixel = new()
                    {
                        Asset = new() { Width = 1920, Height = 1080 },
                        Thumbnail = new() { Width = 200, Height = 112 }
                    },
                    FileProperties = new() { Size = 343633 },
                    ThumbnailCreationDateTime = new(2023, 8, 19, 11, 26, 09),
                    Hash =
                        "0af8f118b7d606e5d174643727bd3c0c6028b52c50481585274fd572110b108c7a0d7901227f75a72b44c89335e002a65e8137ff5b238ab1c0bba0505e783124",
                    Metadata = new()
                    {
                        Corrupted = new() { IsTrue = true, Message = "The asset is corrupted" },
                        Rotated = new() { IsTrue = true, Message = "The asset has been rotated" }
                    }
                }
            ];

            _database!.Initialize(
                directoryPath,
                _userConfigurationService!.StorageSettings.Separator,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);

            _database!.SetDataTableProperties(new()
            {
                TableName = tableName,
                ColumnProperties = AssetConfigs.ConfigureDataTable()
            });

            ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() =>
                _database!.WriteObjectList(assets, tableName, AssetConfigs.WriteFunc));

            Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'tableName')"));
            Assert.That(File.Exists(filePath), Is.False);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS), true);
        }
    }

    [Test]
    public void
        WriteObjectList_TableNameIsNullAllColumnsWithEscapedTextAndPipeSeparator_ThrowsTwoArgumentNullException()
    {
        string? tableName = null;
        string directoryPath = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS);
        string filePath = Path.Combine(directoryPath,
            _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, tableName + ".db");

        try
        {
            List<Asset> assets =
            [
                new()
                {
                    FolderId = new("876283c6-780e-4ad5-975c-be63044c087a"),
                    Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
                    FileName = "20200720175810_3.jpg",
                    ImageRotation = Rotation.Rotate0,
                    Pixel = new()
                    {
                        Asset = new() { Width = 1920, Height = 1080 },
                        Thumbnail = new() { Width = 200, Height = 112 }
                    },
                    FileProperties = new() { Size = 363888 },
                    ThumbnailCreationDateTime = new(2023, 8, 19, 11, 26, 09),
                    Hash =
                        "4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4",
                    Metadata = new()
                    {
                        Corrupted = new() { IsTrue = false, Message = null },
                        Rotated = new() { IsTrue = false, Message = null }
                    }
                },
                new()
                {
                    FolderId = new("876283c6-780e-4ad5-975c-be63044c087a"),
                    Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
                    FileName = "20200720175816_3.jpg",
                    ImageRotation = Rotation.Rotate0,
                    Pixel = new()
                    {
                        Asset = new() { Width = 1920, Height = 1080 },
                        Thumbnail = new() { Width = 200, Height = 112 }
                    },
                    FileProperties = new() { Size = 343633 },
                    ThumbnailCreationDateTime = new(2023, 8, 19, 11, 26, 09),
                    Hash =
                        "0af8f118b7d606e5d174643727bd3c0c6028b52c50481585274fd572110b108c7a0d7901227f75a72b44c89335e002a65e8137ff5b238ab1c0bba0505e783124",
                    Metadata = new()
                    {
                        Corrupted = new() { IsTrue = true, Message = "The asset is corrupted" },
                        Rotated = new() { IsTrue = true, Message = "The asset has been rotated" }
                    }
                }
            ];

            _database!.Initialize(
                directoryPath,
                _userConfigurationService!.StorageSettings.Separator,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);

            ArgumentNullException? exception1 = Assert.Throws<ArgumentNullException>(() =>
                _database!.SetDataTableProperties(new()
                {
                    TableName = tableName!,
                    ColumnProperties = AssetConfigs.ConfigureDataTable()
                }));

            ArgumentNullException? exception2 = Assert.Throws<ArgumentNullException>(() =>
                _database!.WriteObjectList(assets, tableName!, AssetConfigs.WriteFunc));

            Assert.That(exception1?.Message, Is.EqualTo("Value cannot be null. (Parameter 'key')"));
            Assert.That(exception2?.Message, Is.EqualTo("Value cannot be null. (Parameter 'tableName')"));
            Assert.That(File.Exists(filePath), Is.False);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS), true);
        }
    }

    private void WriteObjectList(string directoryPath, char separator, string tableName, bool escapeText)
    {
        List<Asset> assets =
        [
            new()
            {
                FolderId = new("876283c6-780e-4ad5-975c-be63044c087a"),
                Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
                FileName = "20200720175810_3.jpg",
                ImageRotation = Rotation.Rotate0,
                Pixel = new()
                {
                    Asset = new() { Width = 1920, Height = 1080 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                FileProperties = new() { Size = 363888 },
                ThumbnailCreationDateTime = new(2023, 8, 19, 11, 26, 09),
                Hash =
                    "4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4",
                Metadata = new()
                {
                    Corrupted = new() { IsTrue = false, Message = null },
                    Rotated = new() { IsTrue = false, Message = null }
                }
            },
            new()
            {
                FolderId = new("876283c6-780e-4ad5-975c-be63044c087a"),
                Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
                FileName = "20200720175816_3.jpg",
                ImageRotation = Rotation.Rotate0,
                Pixel = new()
                {
                    Asset = new() { Width = 1920, Height = 1080 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                FileProperties = new() { Size = 343633 },
                ThumbnailCreationDateTime = new(2023, 8, 19, 11, 26, 09),
                Hash =
                    "0af8f118b7d606e5d174643727bd3c0c6028b52c50481585274fd572110b108c7a0d7901227f75a72b44c89335e002a65e8137ff5b238ab1c0bba0505e783124",
                Metadata = new()
                {
                    Corrupted = new() { IsTrue = true, Message = "The asset is corrupted" },
                    Rotated = new() { IsTrue = true, Message = "The asset has been rotated" }
                }
            }
        ];

        _database!.Initialize(
            directoryPath,
            separator,
            _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
            _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);

        _database!.SetDataTableProperties(new()
        {
            TableName = tableName,
            ColumnProperties =
            [
                new() { ColumnName = "FolderId", EscapeText = escapeText },
                new() { ColumnName = "FileName", EscapeText = escapeText },
                new() { ColumnName = "ImageRotation", EscapeText = escapeText },
                new() { ColumnName = "PixelWidth", EscapeText = escapeText },
                new() { ColumnName = "PixelHeight", EscapeText = escapeText },
                new() { ColumnName = "ThumbnailPixelWidth", EscapeText = escapeText },
                new() { ColumnName = "ThumbnailPixelHeight", EscapeText = escapeText },
                new() { ColumnName = "ThumbnailCreationDateTime", EscapeText = escapeText },
                new() { ColumnName = "Hash", EscapeText = escapeText },
                new() { ColumnName = "CorruptedMessage", EscapeText = escapeText },
                new() { ColumnName = "IsCorrupted", EscapeText = escapeText },
                new() { ColumnName = "RotatedMessage", EscapeText = escapeText },
                new() { ColumnName = "IsRotated", EscapeText = escapeText }
            ]
        });

        _database!.WriteObjectList(assets, tableName, AssetConfigs.WriteFunc);
    }

    private void Asserts(string filePath, string csv)
    {
        Assert.That(File.Exists(filePath), Is.True);

        Assert.That(_database!.Diagnostics.LastWriteFilePath, Is.EqualTo(filePath));
        Assert.That(_database!.Diagnostics.LastWriteFileRaw, Is.EqualTo(csv));

        string fileContent = File.ReadAllText(filePath);
        Assert.That(fileContent, Is.EqualTo(csv));
    }
}
