using Directories = PhotoManager.Tests.Unit.Constants.Directories;

namespace PhotoManager.Tests.Unit.Infrastructure.Database.DatabaseTests;

[TestFixture]
public class DatabaseReadObjectListTests
{
    private string? _dataDirectory;

    private PhotoManager.Infrastructure.Database.Database? _database;
    private UserConfigurationService? _userConfigurationService;

    private string? _csvEscapedTextWithSemicolon;
    private string? _csvUnescapedTextWithSemicolon;
    private string? _csvEscapedTextWithPipe;
    private string? _csvUnescapedTextWithPipe;
    private string? _csvSomeUnescapedTextWithPipe;
    private string? _csvInvalid;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);

        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();

        _userConfigurationService = new(configurationRootMock.Object);

        _csvEscapedTextWithSemicolon = "\"FolderId\";\"FileName\";\"ImageRotation\";\"PixelWidth\";\"PixelHeight\";\"ThumbnailPixelWidth\";\"ThumbnailPixelHeight\";\"ThumbnailCreationDateTime\";\"Hash\";\"CorruptedMessage\";\"IsCorrupted\";\"RotatedMessage\";\"IsRotated\"\r\n" +
            "\"876283c6-780e-4ad5-975c-be63044c087a\";\"20200720175810_3.jpg\";\"Rotate0\";\"1920\";\"1080\";\"200\";\"112\";\"8/19/2023 11:26:09\";\"4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4\";\"\";\"false\";\"\";\"false\"\r\n" +
            "\"876283c6-780e-4ad5-975c-be63044c087a\";\"20200720175816_3.jpg\";\"Rotate0\";\"1920\";\"1080\";\"200\";\"112\";\"8/19/2023 11:26:09\";\"0af8f118b7d606e5d174643727bd3c0c6028b52c50481585274fd572110b108c7a0d7901227f75a72b44c89335e002a65e8137ff5b238ab1c0bba0505e783124\";\"The asset is corrupted\";\"true\";\"The asset has been rotated\";\"true\"\r\n";

        _csvUnescapedTextWithSemicolon = "FolderId;FileName;ImageRotation;PixelWidth;PixelHeight;ThumbnailPixelWidth;ThumbnailPixelHeight;ThumbnailCreationDateTime;Hash;CorruptedMessage;IsCorrupted;RotatedMessage;IsRotated\r\n" +
            "876283c6-780e-4ad5-975c-be63044c087a;20200720175810_3.jpg;Rotate0;1920;1080;200;112;8/19/2023 11:26:09;4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4;;false;;false\r\n" +
            "876283c6-780e-4ad5-975c-be63044c087a;20200720175816_3.jpg;Rotate0;1920;1080;200;112;8/19/2023 11:26:09;0af8f118b7d606e5d174643727bd3c0c6028b52c50481585274fd572110b108c7a0d7901227f75a72b44c89335e002a65e8137ff5b238ab1c0bba0505e783124;The asset is corrupted;true;The asset has been rotated;true\r\n";

        _csvEscapedTextWithPipe = "\"FolderId\"|\"FileName\"|\"ImageRotation\"|\"PixelWidth\"|\"PixelHeight\"|\"ThumbnailPixelWidth\"|\"ThumbnailPixelHeight\"|\"ThumbnailCreationDateTime\"|\"Hash\"|\"CorruptedMessage\"|\"IsCorrupted\"|\"RotatedMessage\"|\"IsRotated\"\r\n" +
            "\"876283c6-780e-4ad5-975c-be63044c087a\"|\"20200720175810_3.jpg\"|\"Rotate0\"|\"1920\"|\"1080\"|\"200\"|\"112\"|\"8/19/2023 11:26:09\"|\"4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4\"|\"\"|\"false\"|\"\"|\"false\"\r\n" +
            "\"876283c6-780e-4ad5-975c-be63044c087a\"|\"20200720175816_3.jpg\"|\"Rotate0\"|\"1920\"|\"1080\"|\"200\"|\"112\"|\"8/19/2023 11:26:09\"|\"0af8f118b7d606e5d174643727bd3c0c6028b52c50481585274fd572110b108c7a0d7901227f75a72b44c89335e002a65e8137ff5b238ab1c0bba0505e783124\"|\"The asset is corrupted\"|\"true\"|\"The asset has been rotated\"|\"true\"\r\n";

        _csvUnescapedTextWithPipe = "FolderId|FileName|ImageRotation|PixelWidth|PixelHeight|ThumbnailPixelWidth|ThumbnailPixelHeight|ThumbnailCreationDateTime|Hash|CorruptedMessage|IsCorrupted|RotatedMessage|IsRotated\r\n" +
            "876283c6-780e-4ad5-975c-be63044c087a|20200720175810_3.jpg|Rotate0|1920|1080|200|112|8/19/2023 11:26:09|4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4||false||false\r\n" +
            "876283c6-780e-4ad5-975c-be63044c087a|20200720175816_3.jpg|Rotate0|1920|1080|200|112|8/19/2023 11:26:09|0af8f118b7d606e5d174643727bd3c0c6028b52c50481585274fd572110b108c7a0d7901227f75a72b44c89335e002a65e8137ff5b238ab1c0bba0505e783124|The asset is corrupted|true|The asset has been rotated|true\r\n";

        _csvSomeUnescapedTextWithPipe = "\"FolderId\"|FileName|ImageRotation|PixelWidth|PixelHeight|ThumbnailPixelWidth|ThumbnailPixelHeight|ThumbnailCreationDateTime|Hash|CorruptedMessage|IsCorrupted|RotatedMessage|\"IsRotated\"\r\n" +
            "\"876283c6-780e-4ad5-975c-be63044c087a\"|20200720175810_3.jpg|Rotate0|1920|1080|200|112|8/19/2023 11:26:09|4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4||false||\"false\"\r\n" +
            "\"876283c6-780e-4ad5-975c-be63044c087a\"|20200720175816_3.jpg|Rotate0|1920|1080|200|112|8/19/2023 11:26:09|0af8f118b7d606e5d174643727bd3c0c6028b52c50481585274fd572110b108c7a0d7901227f75a72b44c89335e002a65e8137ff5b238ab1c0bba0505e783124|The asset is corrupted|true|The asset has been rotated|\"true\"\r\n";

        _csvInvalid = "FolderId|ImageRotation\r\n" + "1920|false|toto\r\n";
    }

    [SetUp]
    public void SetUp()
    {
        _database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
    }

    [Test]
    [TestCase("EscapedText", true)]
    [TestCase("UnescapedText", false)]
    public void ReadObjectList_AllColumnsAndSemicolonSeparator_ReturnsAssets(string csvType, bool escapeText)
    {
        string csv = csvType == "EscapedText" ? _csvEscapedTextWithSemicolon! : _csvUnescapedTextWithSemicolon!;

        string tableName = "assets" + Guid.NewGuid();
        string directoryPath = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS);
        string filePath = Path.Combine(directoryPath, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, tableName + ".db");

        try
        {
            List<Asset> assets = ReadObjectList(directoryPath, Constants.SEMICOLON_SEPARATOR, filePath, csv, tableName, escapeText);

            Asserts(assets, filePath, csv);
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
    public void ReadObjectList_AllColumnsAndPipeSeparator_ReturnsAssets(string csvType, bool escapeText)
    {
        string csv = csvType == "EscapedText" ? _csvEscapedTextWithPipe! : _csvUnescapedTextWithPipe!;

        string tableName = "assets" + Guid.NewGuid();
        string directoryPath = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS);
        string filePath = Path.Combine(directoryPath, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, tableName + ".db");

        try
        {
            List<Asset> assets = ReadObjectList(directoryPath, _userConfigurationService!.StorageSettings.Separator, filePath, csv, tableName, escapeText);

            Asserts(assets, filePath, csv);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS), true);
        }
    }

    [Test]
    public void ReadObjectList_AllColumnsAndPipeSeparatorEscapedTextWithoutDataTableProperties_ThrowsArgumentException()
    {
        string csv = _csvEscapedTextWithPipe!;

        string tableName = "assets" + Guid.NewGuid();
        string directoryPath = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS);
        string filePath = Path.Combine(directoryPath, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, tableName + ".db");
        string exceptionMessage = $"Error while trying to read data table {tableName}.\n" +
            $"DataDirectory: {directoryPath}\n" +
            $"Separator: {_userConfigurationService!.StorageSettings.Separator}\n" +
            $"LastReadFilePath: {filePath}\n" +
            $"LastReadFileRaw: {csv}";

        try
        {
            _database!.Initialize(
                directoryPath,
                _userConfigurationService!.StorageSettings.Separator,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);

            File.WriteAllText(filePath, csv);

            ArgumentException? exception = Assert.Throws<ArgumentException>(() => _database!.ReadObjectList(tableName, AssetConfigs.ReadFunc));
            Assert.That(exception?.Message, Is.EqualTo(exceptionMessage));
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS), true);
        }
    }

    [Test]
    public void ReadObjectList_AllColumnsAndPipeSeparatorUnescapedTextWithoutDataTableProperties_ReturnsAssets()
    {
        string csv = _csvUnescapedTextWithPipe!;

        string tableName = "assets" + Guid.NewGuid();
        string directoryPath = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS);
        string filePath = Path.Combine(directoryPath, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, tableName + ".db");

        try
        {
            _database!.Initialize(
                directoryPath,
                _userConfigurationService!.StorageSettings.Separator,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);

            File.WriteAllText(filePath, csv);

            List<Asset> assets = _database!.ReadObjectList(tableName, AssetConfigs.ReadFunc);

            Asserts(assets, filePath, csv);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS), true);
        }
    }

    [Test]
    public void ReadObjectList_SomeColumnsWithEscapedTextAndPipeSeparator_ReturnsAssets()
    {
        string csv = _csvSomeUnescapedTextWithPipe!;

        string tableName = "assets" + Guid.NewGuid();
        string directoryPath = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS);
        string filePath = Path.Combine(directoryPath, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, tableName + ".db");

        try
        {
            _database!.Initialize(
                directoryPath,
                _userConfigurationService!.StorageSettings.Separator,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);

            File.WriteAllText(filePath, csv);

            _database!.SetDataTableProperties(new DataTableProperties
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

            List<Asset> assets = _database!.ReadObjectList(tableName, AssetConfigs.ReadFunc);

            Asserts(assets, filePath, csv);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS), true);
        }
    }

    [Test]
    public void ReadObjectList_SomeColumnsInvalidsAndPipeSeparator_ReturnsAssets()
    {
        string csv = _csvUnescapedTextWithPipe!;

        string tableName = "assets" + Guid.NewGuid();
        string directoryPath = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS);
        string filePath = Path.Combine(directoryPath, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, tableName + ".db");

        try
        {
            _database!.Initialize(
                directoryPath,
                _userConfigurationService!.StorageSettings.Separator,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);

            File.WriteAllText(filePath, csv);

            _database!.SetDataTableProperties(new DataTableProperties
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

            List<Asset> assets = _database!.ReadObjectList(tableName, AssetConfigs.ReadFunc);

            Asserts(assets, filePath, csv);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS), true);
        }
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void ReadObjectList_CsvIsNullAndPipeSeparator_ReturnsEmptyList(bool escapeText)
    {
        string? csv = null;

        string tableName = "assets" + Guid.NewGuid();
        string directoryPath = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS);
        string filePath = Path.Combine(directoryPath, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, tableName + ".db");

        try
        {
            List<Asset> assets = ReadObjectList(directoryPath, _userConfigurationService!.StorageSettings.Separator, filePath, csv!, tableName, escapeText);

            Assert.That(assets, Is.Empty);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS), true);
        }
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void ReadObjectList_CsvIsEmptyAndPipeSeparator_ReturnsEmptyList(bool escapeText)
    {
        string csv = string.Empty;

        string tableName = "assets" + Guid.NewGuid();
        string directoryPath = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS);
        string filePath = Path.Combine(directoryPath, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, tableName + ".db");

        try
        {
            List<Asset> assets = ReadObjectList(directoryPath, _userConfigurationService!.StorageSettings.Separator, filePath, csv, tableName, escapeText);

            Assert.That(assets, Is.Empty);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS), true);
        }
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void ReadObjectList_CsvIsIncorrectPipeSeparator_ThrowsArgumentException(bool escapeText)
    {
        string csv = _csvInvalid!;

        string tableName = "assets" + Guid.NewGuid();
        string directoryPath = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS);
        string filePath = Path.Combine(directoryPath, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, tableName + ".db");
        string exceptionMessage = $"Error while trying to read data table {tableName}.\n" +
            $"DataDirectory: {directoryPath}\n" +
            $"Separator: {_userConfigurationService!.StorageSettings.Separator}\n" +
            $"LastReadFilePath: {filePath}\n" +
            $"LastReadFileRaw: {csv}";

        try
        {
            ArgumentException? exception = Assert.Throws<ArgumentException>(() =>
            {
                ReadObjectList(directoryPath, _userConfigurationService!.StorageSettings.Separator, filePath, csv, tableName, escapeText);
            });

            Assert.That(exception?.Message, Is.EqualTo(exceptionMessage));
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS), true);
        }
    }

    [Test]
    public void ReadObjectList_FuncIsNull_ThrowsArgumentException()
    {
        string csv = _csvUnescapedTextWithPipe!;
        string tableName = "assets" + Guid.NewGuid();
        string directoryPath = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS);
        string filePath = Path.Combine(directoryPath, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, tableName + ".db");
        Func<string[], Asset>? nullFunc = null;
        string exceptionMessage = $"Error while trying to read data table {tableName}.\n" +
            $"DataDirectory: {directoryPath}\n" +
            $"Separator: {_userConfigurationService!.StorageSettings.Separator}\n" +
            $"LastReadFilePath: {filePath}\n" +
            $"LastReadFileRaw: {csv}";

        try
        {
            _database!.Initialize(
                directoryPath,
                _userConfigurationService!.StorageSettings.Separator,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);

            File.WriteAllText(filePath, csv);

            _database!.SetDataTableProperties(new DataTableProperties
            {
                TableName = tableName,
                ColumnProperties = AssetConfigs.ConfigureDataTable()
            });

            ArgumentException? exception = Assert.Throws<ArgumentException>(() => _database!.ReadObjectList(tableName, nullFunc!));
            Assert.That(exception?.Message, Is.EqualTo(exceptionMessage));
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS), true);
        }
    }

    [Test]
    public void ReadObjectList_ErrorInObjectListStorage_ThrowsArgumentException()
    {
        string csv = _csvUnescapedTextWithPipe!;
        string tableName = "assets" + Guid.NewGuid();
        string directoryPath = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS);
        string filePath = Path.Combine(directoryPath, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, tableName + ".db");
        string exceptionMessage = $"Error while trying to read data table {tableName}.\n" +
            $"DataDirectory: {directoryPath}\n" +
            $"Separator: {_userConfigurationService!.StorageSettings.Separator}\n" +
            $"LastReadFilePath: {filePath}\n" +
            $"LastReadFileRaw: {null}";

        try
        {
            Mock<IObjectListStorage> objectListStorageMock = new();
            objectListStorageMock.Setup(x => x.ReadObjectList(It.IsAny<string>(), It.IsAny<Func<string[], Asset>>(), It.IsAny<Diagnostics>())).Throws(new Exception());
            PhotoManager.Infrastructure.Database.Database database = new(objectListStorageMock.Object, new BlobStorage(), new BackupStorage());

            database.Initialize(
                directoryPath,
                _userConfigurationService!.StorageSettings.Separator,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);

            File.WriteAllText(filePath, csv);

            database.SetDataTableProperties(new DataTableProperties
            {
                TableName = tableName,
                ColumnProperties = AssetConfigs.ConfigureDataTable()
            });

            ArgumentException? exception = Assert.Throws<ArgumentException>(() => database.ReadObjectList(tableName, AssetConfigs.ReadFunc));
            Assert.That(exception?.Message, Is.EqualTo(exceptionMessage));
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS), true);
        }
    }

    private List<Asset> ReadObjectList(string directoryPath, char separator, string filePath, string csv, string tableName, bool escapeText)
    {
        _database!.Initialize(
                directoryPath,
                separator,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);

        File.WriteAllText(filePath, csv);

        _database!.SetDataTableProperties(new DataTableProperties
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

        List<Asset> assets = _database!.ReadObjectList(tableName, AssetConfigs.ReadFunc);

        return assets;
    }

    private void Asserts(List<Asset> assets, string filePath, string csv)
    {
        Assert.That(assets, Has.Count.EqualTo(2));

        Asset asset1 = assets[0];
        Assert.That(asset1.FolderId, Is.EqualTo(new Guid("876283c6-780e-4ad5-975c-be63044c087a")));
        Assert.That(asset1.FileName, Is.EqualTo("20200720175810_3.jpg"));
        Assert.That(asset1.ImageRotation, Is.EqualTo(Rotation.Rotate0));
        Assert.That(asset1.Pixel.Asset.Width, Is.EqualTo(1920));
        Assert.That(asset1.Pixel.Asset.Height, Is.EqualTo(1080));
        Assert.That(asset1.Pixel.Thumbnail.Width, Is.EqualTo(200));
        Assert.That(asset1.Pixel.Thumbnail.Height, Is.EqualTo(112));
        Assert.That(asset1.ThumbnailCreationDateTime, Is.EqualTo(new DateTime(2023, 8, 19, 11, 26, 09)));
        Assert.That(asset1.Hash, Is.EqualTo("4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4"));
        Assert.That(asset1.Metadata.Corrupted.IsTrue, Is.False);
        Assert.That(asset1.Metadata.Corrupted.Message, Is.EqualTo(""));
        Assert.That(asset1.Metadata.Rotated.IsTrue, Is.False);
        Assert.That(asset1.Metadata.Rotated.Message, Is.EqualTo(""));

        Asset asset2 = assets[1];
        Assert.That(asset2.FolderId, Is.EqualTo(new Guid("876283c6-780e-4ad5-975c-be63044c087a")));
        Assert.That(asset2.FileName, Is.EqualTo("20200720175816_3.jpg"));
        Assert.That(asset2.ImageRotation, Is.EqualTo(Rotation.Rotate0));
        Assert.That(asset2.Pixel.Asset.Width, Is.EqualTo(1920));
        Assert.That(asset2.Pixel.Asset.Height, Is.EqualTo(1080));
        Assert.That(asset2.Pixel.Thumbnail.Width, Is.EqualTo(200));
        Assert.That(asset2.Pixel.Thumbnail.Height, Is.EqualTo(112));
        Assert.That(asset2.ThumbnailCreationDateTime, Is.EqualTo(new DateTime(2023, 8, 19, 11, 26, 09)));
        Assert.That(asset2.Hash, Is.EqualTo("0af8f118b7d606e5d174643727bd3c0c6028b52c50481585274fd572110b108c7a0d7901227f75a72b44c89335e002a65e8137ff5b238ab1c0bba0505e783124"));
        Assert.That(asset2.Metadata.Corrupted.IsTrue, Is.True);
        Assert.That(asset2.Metadata.Corrupted.Message, Is.EqualTo("The asset is corrupted"));
        Assert.That(asset2.Metadata.Rotated.IsTrue, Is.True);
        Assert.That(asset2.Metadata.Rotated.Message, Is.EqualTo("The asset has been rotated"));

        Assert.That(_database!.Diagnostics.LastReadFilePath, Is.EqualTo(filePath));
        Assert.That(_database!.Diagnostics.LastReadFileRaw, Is.EqualTo(csv));
    }
}
