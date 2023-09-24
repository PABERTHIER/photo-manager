namespace PhotoManager.Tests.Unit.Infrastructure.Database.DatabaseTests;

[TestFixture]
public class DatabaseReadObjectListTests
{
    private string? dataDirectory;
    private PhotoManager.Infrastructure.Database.Database? _database;

    private string? csvEscapedTextWithSemicolon;
    private string? csvUnescapedTextWithSemicolon;
    private string? csvEscapedTextWithPipe;
    private string? csvUnescapedTextWithPipe;
    private string? csvSomeUnescapedTextWithPipe;
    private string? csvInvalid;
    private readonly char pipeSeparator = AssetConstants.Separator.ToCharArray().First();
    private readonly char semicolonSeparator = ';';

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");

        csvEscapedTextWithSemicolon = "\"FolderId\";\"FileName\";\"FileSize\";\"ImageRotation\";\"PixelWidth\";\"PixelHeight\";\"ThumbnailPixelWidth\";\"ThumbnailPixelHeight\";\"ThumbnailCreationDateTime\";\"Hash\";\"AssetCorruptedMessage\";\"IsAssetCorrupted\";\"AssetRotatedMessage\";\"IsAssetRotated\"\r\n" +
            "\"876283c6-780e-4ad5-975c-be63044c087a\";\"20200720175810_3.jpg\";\"363888\";\"Rotate0\";\"1920\";\"1080\";\"200\";\"112\";\"8/19/2023 11:26:09\";\"4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4\";\"\";\"false\";\"\";\"false\"\r\n" +
            "\"876283c6-780e-4ad5-975c-be63044c087a\";\"20200720175816_3.jpg\";\"343633\";\"Rotate0\";\"1920\";\"1080\";\"200\";\"112\";\"8/19/2023 11:26:09\";\"0af8f118b7d606e5d174643727bd3c0c6028b52c50481585274fd572110b108c7a0d7901227f75a72b44c89335e002a65e8137ff5b238ab1c0bba0505e783124\";\"The asset is corrupted\";\"true\";\"The asset has been rotated\";\"true\"\r\n";

        csvUnescapedTextWithSemicolon = "FolderId;FileName;FileSize;ImageRotation;PixelWidth;PixelHeight;ThumbnailPixelWidth;ThumbnailPixelHeight;ThumbnailCreationDateTime;Hash;AssetCorruptedMessage;IsAssetCorrupted;AssetRotatedMessage;IsAssetRotated\r\n" +
            "876283c6-780e-4ad5-975c-be63044c087a;20200720175810_3.jpg;363888;Rotate0;1920;1080;200;112;8/19/2023 11:26:09;4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4;;false;;false\r\n" +
            "876283c6-780e-4ad5-975c-be63044c087a;20200720175816_3.jpg;343633;Rotate0;1920;1080;200;112;8/19/2023 11:26:09;0af8f118b7d606e5d174643727bd3c0c6028b52c50481585274fd572110b108c7a0d7901227f75a72b44c89335e002a65e8137ff5b238ab1c0bba0505e783124;The asset is corrupted;true;The asset has been rotated;true\r\n";

        csvEscapedTextWithPipe = "\"FolderId\"|\"FileName\"|\"FileSize\"|\"ImageRotation\"|\"PixelWidth\"|\"PixelHeight\"|\"ThumbnailPixelWidth\"|\"ThumbnailPixelHeight\"|\"ThumbnailCreationDateTime\"|\"Hash\"|\"AssetCorruptedMessage\"|\"IsAssetCorrupted\"|\"AssetRotatedMessage\"|\"IsAssetRotated\"\r\n" +
            "\"876283c6-780e-4ad5-975c-be63044c087a\"|\"20200720175810_3.jpg\"|\"363888\"|\"Rotate0\"|\"1920\"|\"1080\"|\"200\"|\"112\"|\"8/19/2023 11:26:09\"|\"4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4\"|\"\"|\"false\"|\"\"|\"false\"\r\n" +
            "\"876283c6-780e-4ad5-975c-be63044c087a\"|\"20200720175816_3.jpg\"|\"343633\"|\"Rotate0\"|\"1920\"|\"1080\"|\"200\"|\"112\"|\"8/19/2023 11:26:09\"|\"0af8f118b7d606e5d174643727bd3c0c6028b52c50481585274fd572110b108c7a0d7901227f75a72b44c89335e002a65e8137ff5b238ab1c0bba0505e783124\"|\"The asset is corrupted\"|\"true\"|\"The asset has been rotated\"|\"true\"\r\n";

        csvUnescapedTextWithPipe = "FolderId|FileName|FileSize|ImageRotation|PixelWidth|PixelHeight|ThumbnailPixelWidth|ThumbnailPixelHeight|ThumbnailCreationDateTime|Hash|AssetCorruptedMessage|IsAssetCorrupted|AssetRotatedMessage|IsAssetRotated\r\n" +
            "876283c6-780e-4ad5-975c-be63044c087a|20200720175810_3.jpg|363888|Rotate0|1920|1080|200|112|8/19/2023 11:26:09|4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4||false||false\r\n" +
            "876283c6-780e-4ad5-975c-be63044c087a|20200720175816_3.jpg|343633|Rotate0|1920|1080|200|112|8/19/2023 11:26:09|0af8f118b7d606e5d174643727bd3c0c6028b52c50481585274fd572110b108c7a0d7901227f75a72b44c89335e002a65e8137ff5b238ab1c0bba0505e783124|The asset is corrupted|true|The asset has been rotated|true\r\n";

        csvSomeUnescapedTextWithPipe = "\"FolderId\"|FileName|FileSize|ImageRotation|PixelWidth|PixelHeight|ThumbnailPixelWidth|ThumbnailPixelHeight|ThumbnailCreationDateTime|Hash|AssetCorruptedMessage|IsAssetCorrupted|AssetRotatedMessage|\"IsAssetRotated\"\r\n" +
            "\"876283c6-780e-4ad5-975c-be63044c087a\"|20200720175810_3.jpg|363888|Rotate0|1920|1080|200|112|8/19/2023 11:26:09|4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4||false||\"false\"\r\n" +
            "\"876283c6-780e-4ad5-975c-be63044c087a\"|20200720175816_3.jpg|343633|Rotate0|1920|1080|200|112|8/19/2023 11:26:09|0af8f118b7d606e5d174643727bd3c0c6028b52c50481585274fd572110b108c7a0d7901227f75a72b44c89335e002a65e8137ff5b238ab1c0bba0505e783124|The asset is corrupted|true|The asset has been rotated|\"true\"\r\n";

        csvInvalid = "FolderId|FileSize\r\n" + "1920|false|toto\r\n";
    }

    [SetUp]
    public void Setup()
    {
        _database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
    }

    [Test]
    [TestCase("EscapedText", true)]
    [TestCase("UnescapedText", false)]
    public void ReadObjectList_AllColumnsAndSemicolonSeparator_ReturnsAssets(string csvType, bool escapeText)
    {
        string csv;

        if (csvType == "EscapedText")
        {
            csv = csvEscapedTextWithSemicolon!;
        }
        else
        {
            csv = csvUnescapedTextWithSemicolon!;
        }

        string tableName = "assets" + Guid.NewGuid();
        string directoryPath = Path.Combine(dataDirectory!, "DatabaseTests");
        string filePath = Path.Combine(directoryPath, AssetConstants.Tables, tableName + ".db");

        try
        {
            List<Asset> assets = ReadObjectList(directoryPath, semicolonSeparator, filePath, csv, tableName, escapeText);

            Asserts(assets, filePath, csv!);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests_Backups"), true);
        }
    }

    [Test]
    [TestCase("EscapedText", true)]
    [TestCase("UnescapedText", false)]
    public void ReadObjectList_AllColumnsAndPipeSeparator_ReturnsAssets(string csvType, bool escapeText)
    {
        string csv;

        if (csvType == "EscapedText")
        {
            csv = csvEscapedTextWithPipe!;
        }
        else
        {
            csv = csvUnescapedTextWithPipe!;
        }

        string tableName = "assets" + Guid.NewGuid();
        string directoryPath = Path.Combine(dataDirectory!, "DatabaseTests");
        string filePath = Path.Combine(directoryPath, AssetConstants.Tables, tableName + ".db");

        try
        {
            List<Asset> assets = ReadObjectList(directoryPath, pipeSeparator, filePath, csv, tableName, escapeText);

            Asserts(assets, filePath, csv!);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests_Backups"), true);
        }
    }

    [Test]
    public void ReadObjectList_AllColumnsAndPipeSeparatorEscapedTextWithoutDataTableProperties_ThrowsArgumentException()
    {
        string csv = csvEscapedTextWithPipe!;

        string tableName = "assets" + Guid.NewGuid();
        string directoryPath = Path.Combine(dataDirectory!, "DatabaseTests");
        string filePath = Path.Combine(directoryPath, AssetConstants.Tables, tableName + ".db");
        string exceptionMessage = $"Error while trying to read data table {tableName}.\n" +
            $"DataDirectory: {directoryPath}\n" +
            $"Separator: {pipeSeparator}\n" +
            $"LastReadFilePath: {filePath}\n" +
            $"LastReadFileRaw: {csv}";

        try
        {
            _database!.Initialize(directoryPath, pipeSeparator);

            File.WriteAllText(filePath, csv);

            ArgumentException? exception = Assert.Throws<ArgumentException>(() => _database!.ReadObjectList(tableName, AssetConfigs.ReadFunc));
            Assert.AreEqual(exceptionMessage, exception?.Message);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests_Backups"), true);
        }
    }

    [Test]
    public void ReadObjectList_AllColumnsAndPipeSeparatorUnescapedTextWithoutDataTableProperties_ReturnsAssets()
    {
        string csv = csvUnescapedTextWithPipe!;

        string tableName = "assets" + Guid.NewGuid();
        string directoryPath = Path.Combine(dataDirectory!, "DatabaseTests");
        string filePath = Path.Combine(directoryPath, AssetConstants.Tables, tableName + ".db");

        try
        {
            _database!.Initialize(directoryPath, pipeSeparator);

            File.WriteAllText(filePath, csv);

            List<Asset> assets = _database!.ReadObjectList(tableName, AssetConfigs.ReadFunc);

            Asserts(assets, filePath, csv!);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests_Backups"), true);
        }
    }

    [Test]
    public void ReadObjectList_SomeColumnsWithEscapedTextAndPipeSeparator_ReturnsAssets()
    {
        string csv = csvSomeUnescapedTextWithPipe!;

        string tableName = "assets" + Guid.NewGuid();
        string directoryPath = Path.Combine(dataDirectory!, "DatabaseTests");
        string filePath = Path.Combine(directoryPath, AssetConstants.Tables, tableName + ".db");

        try
        {
            _database!.Initialize(directoryPath, pipeSeparator);

            File.WriteAllText(filePath, csv);

            _database!.SetDataTableProperties(new DataTableProperties
            {
                TableName = tableName,
                ColumnProperties = new ColumnProperties[]
                {
                    new ColumnProperties { ColumnName = "FolderId", EscapeText = true },
                    new ColumnProperties { ColumnName = "FileName", EscapeText = false },
                    new ColumnProperties { ColumnName = "FileSize", EscapeText = false },
                    new ColumnProperties { ColumnName = "ImageRotation", EscapeText = false },
                    new ColumnProperties { ColumnName = "PixelWidth", EscapeText = false },
                    new ColumnProperties { ColumnName = "PixelHeight", EscapeText = false },
                    new ColumnProperties { ColumnName = "ThumbnailPixelWidth", EscapeText = false },
                    new ColumnProperties { ColumnName = "ThumbnailPixelHeight", EscapeText = false },
                    new ColumnProperties { ColumnName = "ThumbnailCreationDateTime", EscapeText = false },
                    new ColumnProperties { ColumnName = "Hash", EscapeText = false },
                    new ColumnProperties { ColumnName = "AssetCorruptedMessage", EscapeText = false },
                    new ColumnProperties { ColumnName = "IsAssetCorrupted", EscapeText = false },
                    new ColumnProperties { ColumnName = "AssetRotatedMessage", EscapeText = false },
                    new ColumnProperties { ColumnName = "IsAssetRotated", EscapeText = true }
                }
            });

            List<Asset> assets = _database!.ReadObjectList(tableName, AssetConfigs.ReadFunc);

            Asserts(assets, filePath, csv!);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests_Backups"), true);
        }
    }

    [Test]
    public void ReadObjectList_SomeColumnsInvalidsAndPipeSeparator_ReturnsAssets()
    {
        string csv = csvUnescapedTextWithPipe!;

        string tableName = "assets" + Guid.NewGuid();
        string directoryPath = Path.Combine(dataDirectory!, "DatabaseTests");
        string filePath = Path.Combine(directoryPath, AssetConstants.Tables, tableName + ".db");

        try
        {
            _database!.Initialize(directoryPath, pipeSeparator);

            File.WriteAllText(filePath, csv);

            _database!.SetDataTableProperties(new DataTableProperties
            {
                TableName = tableName,
                ColumnProperties = new ColumnProperties[]
                {
                    new ColumnProperties { ColumnName = "FolderId", EscapeText = false },
                    new ColumnProperties { ColumnName = "Toto", EscapeText = false },
                    new ColumnProperties { ColumnName = "FileSize", EscapeText = false },
                    new ColumnProperties { ColumnName = "ImageRotation", EscapeText = false },
                    new ColumnProperties { ColumnName = "PixelWidth", EscapeText = false },
                    new ColumnProperties { ColumnName = "PixelHeight", EscapeText = false },
                    new ColumnProperties { ColumnName = "ThumbnailPixelWidth", EscapeText = false },
                    new ColumnProperties { ColumnName = "ThumbnailPixelHeight", EscapeText = false },
                    new ColumnProperties { ColumnName = "ThumbnailCreationDateTime", EscapeText = false },
                    new ColumnProperties { ColumnName = "Tutu", EscapeText = false },
                    new ColumnProperties { ColumnName = "AssetCorruptedMessage", EscapeText = false },
                    new ColumnProperties { ColumnName = "IsAssetCorrupted", EscapeText = false },
                    new ColumnProperties { ColumnName = "AssetRotatedMessage", EscapeText = false },
                    new ColumnProperties { ColumnName = "Tata", EscapeText = false }
                }
            });

            List<Asset> assets = _database!.ReadObjectList(tableName, AssetConfigs.ReadFunc);

            Asserts(assets, filePath, csv!);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests_Backups"), true);
        }
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void ReadObjectList_CsvIsNullAndPipeSeparator_ReturnsEmptyList(bool escapeText)
    {
        string? csv = null;

        string tableName = "assets" + Guid.NewGuid();
        string directoryPath = Path.Combine(dataDirectory!, "DatabaseTests");
        string filePath = Path.Combine(directoryPath, AssetConstants.Tables, tableName + ".db");

        try
        {
            List<Asset> assets = ReadObjectList(directoryPath, pipeSeparator, filePath, csv!, tableName, escapeText);

            Assert.IsEmpty(assets);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests_Backups"), true);
        }
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void ReadObjectList_CsvIsEmptyAndPipeSeparator_ReturnsEmptyList(bool escapeText)
    {
        string csv = string.Empty;

        string tableName = "assets" + Guid.NewGuid();
        string directoryPath = Path.Combine(dataDirectory!, "DatabaseTests");
        string filePath = Path.Combine(directoryPath, AssetConstants.Tables, tableName + ".db");

        try
        {
            List<Asset> assets = ReadObjectList(directoryPath, pipeSeparator, filePath, csv, tableName, escapeText);

            Assert.IsEmpty(assets);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests_Backups"), true);
        }
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void ReadObjectList_CsvIsIncorrectPipeSeparator_ThrowsArgumentException(bool escapeText)
    {
        string csv = csvInvalid!;

        string tableName = "assets" + Guid.NewGuid();
        string directoryPath = Path.Combine(dataDirectory!, "DatabaseTests");
        string filePath = Path.Combine(directoryPath, AssetConstants.Tables, tableName + ".db");
        string exceptionMessage = $"Error while trying to read data table {tableName}.\n" +
            $"DataDirectory: {directoryPath}\n" +
            $"Separator: {pipeSeparator}\n" +
            $"LastReadFilePath: {filePath}\n" +
            $"LastReadFileRaw: {csv}";

        try
        {
            ArgumentException? exception = Assert.Throws<ArgumentException>(() => ReadObjectList(directoryPath, pipeSeparator, filePath, csv, tableName, escapeText));
            Assert.AreEqual(exceptionMessage, exception?.Message);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests_Backups"), true);
        }
    }

    [Test]
    public void ReadObjectList_FuncIsNull_ThrowsArgumentException()
    {
        string csv = csvUnescapedTextWithPipe!;
        string tableName = "assets" + Guid.NewGuid();
        string directoryPath = Path.Combine(dataDirectory!, "DatabaseTests");
        string filePath = Path.Combine(directoryPath, AssetConstants.Tables, tableName + ".db");
        Func<string[], Asset>? nullFunc = null;
        string exceptionMessage = $"Error while trying to read data table {tableName}.\n" +
            $"DataDirectory: {directoryPath}\n" +
            $"Separator: {pipeSeparator}\n" +
            $"LastReadFilePath: {filePath}\n" +
            $"LastReadFileRaw: {csv}";

        try
        {
            _database!.Initialize(directoryPath, pipeSeparator);

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
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests_Backups"), true);
        }
    }

    [Test]
    public void ReadObjectList_ErrorInObjectListStorage_ThrowsArgumentException()
    {
        string csv = csvUnescapedTextWithPipe!;
        string tableName = "assets" + Guid.NewGuid();
        string directoryPath = Path.Combine(dataDirectory!, "DatabaseTests");
        string filePath = Path.Combine(directoryPath, AssetConstants.Tables, tableName + ".db");
        string exceptionMessage = $"Error while trying to read data table {tableName}.\n" +
            $"DataDirectory: {directoryPath}\n" +
            $"Separator: {pipeSeparator}\n" +
            $"LastReadFilePath: {filePath}\n" +
            $"LastReadFileRaw: {null}";

        try
        {
            Mock<IObjectListStorage> objectListStorageMock = new();
            objectListStorageMock.Setup(x => x.ReadObjectList(It.IsAny<string>(), It.IsAny<Func<string[], Asset>>(), It.IsAny<Diagnostics>())).Throws(new Exception());
            PhotoManager.Infrastructure.Database.Database? database = new(objectListStorageMock.Object, new BlobStorage(), new BackupStorage());

            database!.Initialize(directoryPath, pipeSeparator);

            File.WriteAllText(filePath, csv);

            database!.SetDataTableProperties(new DataTableProperties
            {
                TableName = tableName,
                ColumnProperties = AssetConfigs.ConfigureDataTable()
            });

            ArgumentException? exception = Assert.Throws<ArgumentException>(() => database!.ReadObjectList(tableName, AssetConfigs.ReadFunc));
            Assert.AreEqual(exceptionMessage, exception?.Message);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests_Backups"), true);
        }
    }

    private List<Asset> ReadObjectList(string directoryPath, char separator, string filePath, string csv, string tableName, bool escapeText)
    {
        _database!.Initialize(directoryPath, separator);

        File.WriteAllText(filePath, csv);

        _database!.SetDataTableProperties(new DataTableProperties
        {
            TableName = tableName,
            ColumnProperties = new ColumnProperties[]
            {
                new ColumnProperties { ColumnName = "FolderId", EscapeText = escapeText },
                new ColumnProperties { ColumnName = "FileName", EscapeText = escapeText },
                new ColumnProperties { ColumnName = "FileSize", EscapeText = escapeText },
                new ColumnProperties { ColumnName = "ImageRotation", EscapeText = escapeText },
                new ColumnProperties { ColumnName = "PixelWidth", EscapeText = escapeText },
                new ColumnProperties { ColumnName = "PixelHeight", EscapeText = escapeText },
                new ColumnProperties { ColumnName = "ThumbnailPixelWidth", EscapeText = escapeText },
                new ColumnProperties { ColumnName = "ThumbnailPixelHeight", EscapeText = escapeText },
                new ColumnProperties { ColumnName = "ThumbnailCreationDateTime", EscapeText = escapeText },
                new ColumnProperties { ColumnName = "Hash", EscapeText = escapeText },
                new ColumnProperties { ColumnName = "AssetCorruptedMessage", EscapeText = escapeText },
                new ColumnProperties { ColumnName = "IsAssetCorrupted", EscapeText = escapeText },
                new ColumnProperties { ColumnName = "AssetRotatedMessage", EscapeText = escapeText },
                new ColumnProperties { ColumnName = "IsAssetRotated", EscapeText = escapeText }
            }
        });

        List<Asset> assets = _database!.ReadObjectList(tableName, AssetConfigs.ReadFunc);

        return assets;
    }

    private void Asserts(List<Asset> assets, string filePath, string csv)
    {
        Assert.AreEqual(2, assets.Count);

        Asset asset1 = assets[0];
        Assert.AreEqual("876283c6-780e-4ad5-975c-be63044c087a", asset1.FolderId);
        Assert.AreEqual("20200720175810_3.jpg", asset1.FileName);
        Assert.AreEqual(363888, asset1.FileSize);
        Assert.AreEqual(Rotation.Rotate0, asset1.ImageRotation);
        Assert.AreEqual(1920, asset1.PixelWidth);
        Assert.AreEqual(1080, asset1.PixelHeight);
        Assert.AreEqual(200, asset1.ThumbnailPixelWidth);
        Assert.AreEqual(112, asset1.ThumbnailPixelHeight);
        Assert.AreEqual(new DateTime(2023, 8, 19, 11, 26, 09), asset1.ThumbnailCreationDateTime);
        Assert.AreEqual("4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4", asset1.Hash);
        Assert.AreEqual("", asset1.AssetCorruptedMessage);
        Assert.AreEqual(false, asset1.IsAssetCorrupted);
        Assert.AreEqual("", asset1.AssetRotatedMessage);
        Assert.AreEqual(false, asset1.IsAssetRotated);

        Asset asset2 = assets[1];
        Assert.AreEqual("876283c6-780e-4ad5-975c-be63044c087a", asset2.FolderId);
        Assert.AreEqual("20200720175816_3.jpg", asset2.FileName);
        Assert.AreEqual(343633, asset2.FileSize);
        Assert.AreEqual(Rotation.Rotate0, asset2.ImageRotation);
        Assert.AreEqual(1920, asset2.PixelWidth);
        Assert.AreEqual(1080, asset2.PixelHeight);
        Assert.AreEqual(200, asset2.ThumbnailPixelWidth);
        Assert.AreEqual(112, asset2.ThumbnailPixelHeight);
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
