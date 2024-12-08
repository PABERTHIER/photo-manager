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
    private const char SEMICOLON_SEPARATOR = ';';

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");

        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();

        _userConfigurationService = new (configurationRootMock.Object);

        _csvEscapedTextWithSemicolon = "\"FolderId\";\"FileName\";\"ImageRotation\";\"PixelWidth\";\"PixelHeight\";\"ThumbnailPixelWidth\";\"ThumbnailPixelHeight\";\"ThumbnailCreationDateTime\";\"Hash\";\"AssetCorruptedMessage\";\"IsAssetCorrupted\";\"AssetRotatedMessage\";\"IsAssetRotated\"\r\n" +
            "\"876283c6-780e-4ad5-975c-be63044c087a\";\"20200720175810_3.jpg\";\"Rotate0\";\"1920\";\"1080\";\"200\";\"112\";\"8/19/2023 11:26:09\";\"4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4\";\"\";\"false\";\"\";\"false\"\r\n" +
            "\"876283c6-780e-4ad5-975c-be63044c087a\";\"20200720175816_3.jpg\";\"Rotate0\";\"1920\";\"1080\";\"200\";\"112\";\"8/19/2023 11:26:09\";\"0af8f118b7d606e5d174643727bd3c0c6028b52c50481585274fd572110b108c7a0d7901227f75a72b44c89335e002a65e8137ff5b238ab1c0bba0505e783124\";\"The asset is corrupted\";\"true\";\"The asset has been rotated\";\"true\"\r\n";

        _csvUnescapedTextWithSemicolon = "FolderId;FileName;ImageRotation;PixelWidth;PixelHeight;ThumbnailPixelWidth;ThumbnailPixelHeight;ThumbnailCreationDateTime;Hash;AssetCorruptedMessage;IsAssetCorrupted;AssetRotatedMessage;IsAssetRotated\r\n" +
            "876283c6-780e-4ad5-975c-be63044c087a;20200720175810_3.jpg;Rotate0;1920;1080;200;112;8/19/2023 11:26:09;4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4;;false;;false\r\n" +
            "876283c6-780e-4ad5-975c-be63044c087a;20200720175816_3.jpg;Rotate0;1920;1080;200;112;8/19/2023 11:26:09;0af8f118b7d606e5d174643727bd3c0c6028b52c50481585274fd572110b108c7a0d7901227f75a72b44c89335e002a65e8137ff5b238ab1c0bba0505e783124;The asset is corrupted;true;The asset has been rotated;true\r\n";

        _csvEscapedTextWithPipe = "\"FolderId\"|\"FileName\"|\"ImageRotation\"|\"PixelWidth\"|\"PixelHeight\"|\"ThumbnailPixelWidth\"|\"ThumbnailPixelHeight\"|\"ThumbnailCreationDateTime\"|\"Hash\"|\"AssetCorruptedMessage\"|\"IsAssetCorrupted\"|\"AssetRotatedMessage\"|\"IsAssetRotated\"\r\n" +
            "\"876283c6-780e-4ad5-975c-be63044c087a\"|\"20200720175810_3.jpg\"|\"Rotate0\"|\"1920\"|\"1080\"|\"200\"|\"112\"|\"8/19/2023 11:26:09\"|\"4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4\"|\"\"|\"false\"|\"\"|\"false\"\r\n" +
            "\"876283c6-780e-4ad5-975c-be63044c087a\"|\"20200720175816_3.jpg\"|\"Rotate0\"|\"1920\"|\"1080\"|\"200\"|\"112\"|\"8/19/2023 11:26:09\"|\"0af8f118b7d606e5d174643727bd3c0c6028b52c50481585274fd572110b108c7a0d7901227f75a72b44c89335e002a65e8137ff5b238ab1c0bba0505e783124\"|\"The asset is corrupted\"|\"true\"|\"The asset has been rotated\"|\"true\"\r\n";

        _csvUnescapedTextWithPipe = "FolderId|FileName|ImageRotation|PixelWidth|PixelHeight|ThumbnailPixelWidth|ThumbnailPixelHeight|ThumbnailCreationDateTime|Hash|AssetCorruptedMessage|IsAssetCorrupted|AssetRotatedMessage|IsAssetRotated\r\n" +
            "876283c6-780e-4ad5-975c-be63044c087a|20200720175810_3.jpg|Rotate0|1920|1080|200|112|8/19/2023 11:26:09|4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4||false||false\r\n" +
            "876283c6-780e-4ad5-975c-be63044c087a|20200720175816_3.jpg|Rotate0|1920|1080|200|112|8/19/2023 11:26:09|0af8f118b7d606e5d174643727bd3c0c6028b52c50481585274fd572110b108c7a0d7901227f75a72b44c89335e002a65e8137ff5b238ab1c0bba0505e783124|The asset is corrupted|true|The asset has been rotated|true\r\n";

        _csvSomeUnescapedTextWithPipe = "\"FolderId\"|FileName|ImageRotation|PixelWidth|PixelHeight|ThumbnailPixelWidth|ThumbnailPixelHeight|ThumbnailCreationDateTime|Hash|AssetCorruptedMessage|IsAssetCorrupted|AssetRotatedMessage|\"IsAssetRotated\"\r\n" +
            "\"876283c6-780e-4ad5-975c-be63044c087a\"|20200720175810_3.jpg|Rotate0|1920|1080|200|112|8/19/2023 11:26:09|4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4||false||\"false\"\r\n" +
            "\"876283c6-780e-4ad5-975c-be63044c087a\"|20200720175816_3.jpg|Rotate0|1920|1080|200|112|8/19/2023 11:26:09|0af8f118b7d606e5d174643727bd3c0c6028b52c50481585274fd572110b108c7a0d7901227f75a72b44c89335e002a65e8137ff5b238ab1c0bba0505e783124|The asset is corrupted|true|The asset has been rotated|\"true\"\r\n";

        _csvInvalid = "FolderId|ImageRotation\r\n" + "1920|false|toto\r\n";
    }

    [SetUp]
    public void SetUp()
    {
        _database = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
    }

    [Test]
    [TestCase("EscapedText", true)]
    [TestCase("UnescapedText", false)]
    public void ReadObjectList_AllColumnsAndSemicolonSeparator_ReturnsAssets(string csvType, bool escapeText)
    {
        string csv = csvType == "EscapedText" ? _csvEscapedTextWithSemicolon! : _csvUnescapedTextWithSemicolon!;

        string tableName = "assets" + Guid.NewGuid();
        string directoryPath = Path.Combine(_dataDirectory!, "DatabaseTests");
        string filePath = Path.Combine(directoryPath, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, tableName + ".db");

        try
        {
            List<Asset> assets = ReadObjectList(directoryPath, SEMICOLON_SEPARATOR, filePath, csv, tableName, escapeText);

            Asserts(assets, filePath, csv);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests_Backups"), true);
        }
    }

    [Test]
    [TestCase("EscapedText", true)]
    [TestCase("UnescapedText", false)]
    public void ReadObjectList_AllColumnsAndPipeSeparator_ReturnsAssets(string csvType, bool escapeText)
    {
        string csv = csvType == "EscapedText" ? _csvEscapedTextWithPipe! : _csvUnescapedTextWithPipe!;

        string tableName = "assets" + Guid.NewGuid();
        string directoryPath = Path.Combine(_dataDirectory!, "DatabaseTests");
        string filePath = Path.Combine(directoryPath, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, tableName + ".db");

        try
        {
            List<Asset> assets = ReadObjectList(directoryPath, _userConfigurationService!.StorageSettings.Separator, filePath, csv, tableName, escapeText);

            Asserts(assets, filePath, csv);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests_Backups"), true);
        }
    }

    [Test]
    public void ReadObjectList_AllColumnsAndPipeSeparatorEscapedTextWithoutDataTableProperties_ThrowsArgumentException()
    {
        string csv = _csvEscapedTextWithPipe!;

        string tableName = "assets" + Guid.NewGuid();
        string directoryPath = Path.Combine(_dataDirectory!, "DatabaseTests");
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
            Assert.AreEqual(exceptionMessage, exception?.Message);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests_Backups"), true);
        }
    }

    [Test]
    public void ReadObjectList_AllColumnsAndPipeSeparatorUnescapedTextWithoutDataTableProperties_ReturnsAssets()
    {
        string csv = _csvUnescapedTextWithPipe!;

        string tableName = "assets" + Guid.NewGuid();
        string directoryPath = Path.Combine(_dataDirectory!, "DatabaseTests");
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
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests_Backups"), true);
        }
    }

    [Test]
    public void ReadObjectList_SomeColumnsWithEscapedTextAndPipeSeparator_ReturnsAssets()
    {
        string csv = _csvSomeUnescapedTextWithPipe!;

        string tableName = "assets" + Guid.NewGuid();
        string directoryPath = Path.Combine(_dataDirectory!, "DatabaseTests");
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
                    new() { ColumnName = "AssetCorruptedMessage", EscapeText = false },
                    new() { ColumnName = "IsAssetCorrupted", EscapeText = false },
                    new() { ColumnName = "AssetRotatedMessage", EscapeText = false },
                    new() { ColumnName = "IsAssetRotated", EscapeText = true }
                ]
            });

            List<Asset> assets = _database!.ReadObjectList(tableName, AssetConfigs.ReadFunc);

            Asserts(assets, filePath, csv);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests_Backups"), true);
        }
    }

    [Test]
    public void ReadObjectList_SomeColumnsInvalidsAndPipeSeparator_ReturnsAssets()
    {
        string csv = _csvUnescapedTextWithPipe!;

        string tableName = "assets" + Guid.NewGuid();
        string directoryPath = Path.Combine(_dataDirectory!, "DatabaseTests");
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
                    new() { ColumnName = "AssetCorruptedMessage", EscapeText = false },
                    new() { ColumnName = "IsAssetCorrupted", EscapeText = false },
                    new() { ColumnName = "AssetRotatedMessage", EscapeText = false },
                    new() { ColumnName = "Tata", EscapeText = false }
                ]
            });

            List<Asset> assets = _database!.ReadObjectList(tableName, AssetConfigs.ReadFunc);

            Asserts(assets, filePath, csv);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests_Backups"), true);
        }
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void ReadObjectList_CsvIsNullAndPipeSeparator_ReturnsEmptyList(bool escapeText)
    {
        string? csv = null;

        string tableName = "assets" + Guid.NewGuid();
        string directoryPath = Path.Combine(_dataDirectory!, "DatabaseTests");
        string filePath = Path.Combine(directoryPath, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, tableName + ".db");

        try
        {
            List<Asset> assets = ReadObjectList(directoryPath, _userConfigurationService!.StorageSettings.Separator, filePath, csv!, tableName, escapeText);

            Assert.IsEmpty(assets);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests_Backups"), true);
        }
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void ReadObjectList_CsvIsEmptyAndPipeSeparator_ReturnsEmptyList(bool escapeText)
    {
        string csv = string.Empty;

        string tableName = "assets" + Guid.NewGuid();
        string directoryPath = Path.Combine(_dataDirectory!, "DatabaseTests");
        string filePath = Path.Combine(directoryPath, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, tableName + ".db");

        try
        {
            List<Asset> assets = ReadObjectList(directoryPath, _userConfigurationService!.StorageSettings.Separator, filePath, csv, tableName, escapeText);

            Assert.IsEmpty(assets);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests_Backups"), true);
        }
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void ReadObjectList_CsvIsIncorrectPipeSeparator_ThrowsArgumentException(bool escapeText)
    {
        string csv = _csvInvalid!;

        string tableName = "assets" + Guid.NewGuid();
        string directoryPath = Path.Combine(_dataDirectory!, "DatabaseTests");
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

            Assert.AreEqual(exceptionMessage, exception?.Message);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests_Backups"), true);
        }
    }

    [Test]
    public void ReadObjectList_FuncIsNull_ThrowsArgumentException()
    {
        string csv = _csvUnescapedTextWithPipe!;
        string tableName = "assets" + Guid.NewGuid();
        string directoryPath = Path.Combine(_dataDirectory!, "DatabaseTests");
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
            Assert.AreEqual(exceptionMessage, exception?.Message);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests_Backups"), true);
        }
    }

    [Test]
    public void ReadObjectList_ErrorInObjectListStorage_ThrowsArgumentException()
    {
        string csv = _csvUnescapedTextWithPipe!;
        string tableName = "assets" + Guid.NewGuid();
        string directoryPath = Path.Combine(_dataDirectory!, "DatabaseTests");
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
            PhotoManager.Infrastructure.Database.Database database = new (objectListStorageMock.Object, new BlobStorage(), new BackupStorage());

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
            Assert.AreEqual(exceptionMessage, exception?.Message);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests_Backups"), true);
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
                new() { ColumnName = "AssetCorruptedMessage", EscapeText = escapeText },
                new() { ColumnName = "IsAssetCorrupted", EscapeText = escapeText },
                new() { ColumnName = "AssetRotatedMessage", EscapeText = escapeText },
                new() { ColumnName = "IsAssetRotated", EscapeText = escapeText }
            ]
        });

        List<Asset> assets = _database!.ReadObjectList(tableName, AssetConfigs.ReadFunc);

        return assets;
    }

    private void Asserts(List<Asset> assets, string filePath, string csv)
    {
        Assert.AreEqual(2, assets.Count);

        Asset asset1 = assets[0];
        Assert.AreEqual(new Guid("876283c6-780e-4ad5-975c-be63044c087a"), asset1.FolderId);
        Assert.AreEqual("20200720175810_3.jpg", asset1.FileName);
        Assert.AreEqual(Rotation.Rotate0, asset1.ImageRotation);
        Assert.AreEqual(1920, asset1.Pixel.Asset.Width);
        Assert.AreEqual(1080, asset1.Pixel.Asset.Height);
        Assert.AreEqual(200, asset1.Pixel.Thumbnail.Width);
        Assert.AreEqual(112, asset1.Pixel.Thumbnail.Height);
        Assert.AreEqual(new DateTime(2023, 8, 19, 11, 26, 09), asset1.ThumbnailCreationDateTime);
        Assert.AreEqual("4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4", asset1.Hash);
        Assert.AreEqual("", asset1.AssetCorruptedMessage);
        Assert.AreEqual(false, asset1.IsAssetCorrupted);
        Assert.AreEqual("", asset1.AssetRotatedMessage);
        Assert.AreEqual(false, asset1.IsAssetRotated);

        Asset asset2 = assets[1];
        Assert.AreEqual(new Guid("876283c6-780e-4ad5-975c-be63044c087a"), asset2.FolderId);
        Assert.AreEqual("20200720175816_3.jpg", asset2.FileName);
        Assert.AreEqual(Rotation.Rotate0, asset2.ImageRotation);
        Assert.AreEqual(1920, asset2.Pixel.Asset.Width);
        Assert.AreEqual(1080, asset2.Pixel.Asset.Height);
        Assert.AreEqual(200, asset2.Pixel.Thumbnail.Width);
        Assert.AreEqual(112, asset2.Pixel.Thumbnail.Height);
        Assert.AreEqual(new DateTime(2023, 8, 19, 11, 26, 09), asset2.ThumbnailCreationDateTime);
        Assert.AreEqual("0af8f118b7d606e5d174643727bd3c0c6028b52c50481585274fd572110b108c7a0d7901227f75a72b44c89335e002a65e8137ff5b238ab1c0bba0505e783124", asset2.Hash);
        Assert.AreEqual("The asset is corrupted", asset2.AssetCorruptedMessage);
        Assert.AreEqual(true, asset2.IsAssetCorrupted);
        Assert.AreEqual("The asset has been rotated", asset2.AssetRotatedMessage);
        Assert.AreEqual(true, asset2.IsAssetRotated);

        Assert.AreEqual(filePath, _database!.Diagnostics.LastReadFilePath);
        Assert.AreEqual(csv, _database!.Diagnostics.LastReadFileRaw);
    }
}
