namespace PhotoManager.Tests.Unit.Infrastructure.Database.DatabaseTests;
// If used, pass the tests into NUnit
public class DatabaseDataTableStorageTests
{
    //[Fact]
    //public void ReadDataTable_AllColumnsWithUnescapedText()
    //{
    //    string csv = "FolderId;FileName;FileSize;ImageRotation;PixelWidth;PixelHeight;ThumbnailPixelWidth;ThumbnailPixelHeight;ThumbnailCreationDateTime;Hash\r\n" +
    //        "876283c6-780e-4ad5-975c-be63044c087a;20200720175810_3.jpg;363888;Rotate0;1920;1080;200;112;25/07/2020 9:45:47;4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4\r\n" +
    //        "876283c6-780e-4ad5-975c-be63044c087a;20200720175816_3.jpg;343633;Rotate0;1920;1080;200;112;25/07/2020 9:45:47;0af8f118b7d606e5d174643727bd3c0c6028b52c50481585274fd572110b108c7a0d7901227f75a72b44c89335e002a65e8137ff5b238ab1c0bba0505e783124\r\n";
    //    string tableName = "assets" + Guid.NewGuid();
    //    string filePath = Path.Combine("TestData", AssetConstants.Tables, tableName + ".db");

    //    IDatabase database = new Database(new ObjectListStorage(),
    //        new DataTableStorage(),
    //        new BlobStorage(),
    //        new BackupStorage());
    //    database.Initialize("TestData", ';');

    //    File.WriteAllText(filePath, csv);

    //    database.SetDataTableProperties(new DataTableProperties
    //    {
    //        TableName = tableName,
    //        ColumnProperties = new ColumnProperties[]
    //        {
    //            new ColumnProperties { ColumnName = "FolderId", EscapeText = false },
    //            new ColumnProperties { ColumnName = "FileName", EscapeText = false },
    //            new ColumnProperties { ColumnName = "FileSize", EscapeText = false },
    //            new ColumnProperties { ColumnName = "ImageRotation", EscapeText = false },
    //            new ColumnProperties { ColumnName = "PixelWidth", EscapeText = false },
    //            new ColumnProperties { ColumnName = "PixelHeight", EscapeText = false },
    //            new ColumnProperties { ColumnName = "ThumbnailPixelWidth", EscapeText = false },
    //            new ColumnProperties { ColumnName = "ThumbnailPixelHeight", EscapeText = false },
    //            new ColumnProperties { ColumnName = "ThumbnailCreationDateTime", EscapeText = false },
    //            new ColumnProperties { ColumnName = "Hash", EscapeText = false }
    //        }
    //    });

    //    DataTable table = database.ReadDataTable(tableName);

    //    table.Columns.Count.Should().Be(10);
    //    table.Columns[0].ColumnName.Should().Be("FolderId");
    //    table.Columns[1].ColumnName.Should().Be("FileName");
    //    table.Columns[2].ColumnName.Should().Be("FileSize");
    //    table.Columns[3].ColumnName.Should().Be("ImageRotation");
    //    table.Columns[4].ColumnName.Should().Be("PixelWidth");
    //    table.Columns[5].ColumnName.Should().Be("PixelHeight");
    //    table.Columns[6].ColumnName.Should().Be("ThumbnailPixelWidth");
    //    table.Columns[7].ColumnName.Should().Be("ThumbnailPixelHeight");
    //    table.Columns[8].ColumnName.Should().Be("ThumbnailCreationDateTime");
    //    table.Columns[9].ColumnName.Should().Be("Hash");

    //    table.TableName.Should().Be(tableName);
    //    table.Rows.Count.Should().Be(2);

    //    table.Rows[0]["FolderId"].Should().Be("876283c6-780e-4ad5-975c-be63044c087a");
    //    table.Rows[0]["FileName"].Should().Be("20200720175810_3.jpg");
    //    table.Rows[0]["FileSize"].Should().Be("363888");
    //    table.Rows[0]["ImageRotation"].Should().Be("Rotate0");
    //    table.Rows[0]["PixelWidth"].Should().Be("1920");
    //    table.Rows[0]["PixelHeight"].Should().Be("1080");
    //    table.Rows[0]["ThumbnailPixelWidth"].Should().Be("200");
    //    table.Rows[0]["ThumbnailPixelHeight"].Should().Be("112");
    //    table.Rows[0]["ThumbnailCreationDateTime"].Should().Be("25/07/2020 9:45:47");
    //    table.Rows[0]["Hash"].Should().Be("4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4");

    //    table.Rows[1]["FolderId"].Should().Be("876283c6-780e-4ad5-975c-be63044c087a");
    //    table.Rows[1]["FileName"].Should().Be("20200720175816_3.jpg");
    //    table.Rows[1]["FileSize"].Should().Be("343633");
    //    table.Rows[1]["ImageRotation"].Should().Be("Rotate0");
    //    table.Rows[1]["PixelWidth"].Should().Be("1920");
    //    table.Rows[1]["PixelHeight"].Should().Be("1080");
    //    table.Rows[1]["ThumbnailPixelWidth"].Should().Be("200");
    //    table.Rows[1]["ThumbnailPixelHeight"].Should().Be("112");
    //    table.Rows[1]["ThumbnailCreationDateTime"].Should().Be("25/07/2020 9:45:47");
    //    table.Rows[1]["Hash"].Should().Be("0af8f118b7d606e5d174643727bd3c0c6028b52c50481585274fd572110b108c7a0d7901227f75a72b44c89335e002a65e8137ff5b238ab1c0bba0505e783124");

    //    database.Diagnostics.LastReadFilePath.Should().Be(filePath);
    //    database.Diagnostics.LastReadFileRaw.Should().Be(csv);
    //}

    //[Fact]
    //public void ReadDataTable_AllColumnsWithUnescapedTextWithoutDataTableProperties()
    //{
    //    string csv = "FolderId;FileName;FileSize;ImageRotation;PixelWidth;PixelHeight;ThumbnailPixelWidth;ThumbnailPixelHeight;ThumbnailCreationDateTime;Hash\r\n" +
    //        "876283c6-780e-4ad5-975c-be63044c087a;20200720175810_3.jpg;363888;Rotate0;1920;1080;200;112;25/07/2020 9:45:47;4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4\r\n" +
    //        "876283c6-780e-4ad5-975c-be63044c087a;20200720175816_3.jpg;343633;Rotate0;1920;1080;200;112;25/07/2020 9:45:47;0af8f118b7d606e5d174643727bd3c0c6028b52c50481585274fd572110b108c7a0d7901227f75a72b44c89335e002a65e8137ff5b238ab1c0bba0505e783124\r\n";
    //    string tableName = "assets" + Guid.NewGuid();
    //    string filePath = Path.Combine("TestData", AssetConstants.Tables, tableName + ".db");

    //    IDatabase database = new Database(new ObjectListStorage(),
    //        new DataTableStorage(),
    //        new BlobStorage(),
    //        new BackupStorage());
    //    database.Initialize("TestData", ';');

    //    File.WriteAllText(filePath, csv);

    //    DataTable table = database.ReadDataTable(tableName);

    //    table.Columns.Count.Should().Be(10);
    //    table.Columns[0].ColumnName.Should().Be("FolderId");
    //    table.Columns[1].ColumnName.Should().Be("FileName");
    //    table.Columns[2].ColumnName.Should().Be("FileSize");
    //    table.Columns[3].ColumnName.Should().Be("ImageRotation");
    //    table.Columns[4].ColumnName.Should().Be("PixelWidth");
    //    table.Columns[5].ColumnName.Should().Be("PixelHeight");
    //    table.Columns[6].ColumnName.Should().Be("ThumbnailPixelWidth");
    //    table.Columns[7].ColumnName.Should().Be("ThumbnailPixelHeight");
    //    table.Columns[8].ColumnName.Should().Be("ThumbnailCreationDateTime");
    //    table.Columns[9].ColumnName.Should().Be("Hash");

    //    table.TableName.Should().Be(tableName);
    //    table.Rows.Count.Should().Be(2);

    //    table.Rows[0]["FolderId"].Should().Be("876283c6-780e-4ad5-975c-be63044c087a");
    //    table.Rows[0]["FileName"].Should().Be("20200720175810_3.jpg");
    //    table.Rows[0]["FileSize"].Should().Be("363888");
    //    table.Rows[0]["ImageRotation"].Should().Be("Rotate0");
    //    table.Rows[0]["PixelWidth"].Should().Be("1920");
    //    table.Rows[0]["PixelHeight"].Should().Be("1080");
    //    table.Rows[0]["ThumbnailPixelWidth"].Should().Be("200");
    //    table.Rows[0]["ThumbnailPixelHeight"].Should().Be("112");
    //    table.Rows[0]["ThumbnailCreationDateTime"].Should().Be("25/07/2020 9:45:47");
    //    table.Rows[0]["Hash"].Should().Be("4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4");

    //    table.Rows[1]["FolderId"].Should().Be("876283c6-780e-4ad5-975c-be63044c087a");
    //    table.Rows[1]["FileName"].Should().Be("20200720175816_3.jpg");
    //    table.Rows[1]["FileSize"].Should().Be("343633");
    //    table.Rows[1]["ImageRotation"].Should().Be("Rotate0");
    //    table.Rows[1]["PixelWidth"].Should().Be("1920");
    //    table.Rows[1]["PixelHeight"].Should().Be("1080");
    //    table.Rows[1]["ThumbnailPixelWidth"].Should().Be("200");
    //    table.Rows[1]["ThumbnailPixelHeight"].Should().Be("112");
    //    table.Rows[1]["ThumbnailCreationDateTime"].Should().Be("25/07/2020 9:45:47");
    //    table.Rows[1]["Hash"].Should().Be("0af8f118b7d606e5d174643727bd3c0c6028b52c50481585274fd572110b108c7a0d7901227f75a72b44c89335e002a65e8137ff5b238ab1c0bba0505e783124");

    //    database.Diagnostics.LastReadFilePath.Should().Be(filePath);
    //    database.Diagnostics.LastReadFileRaw.Should().Be(csv);
    //}

    //[Fact]
    //public void ReadDataTable_AllColumnsWithEscapedText()
    //{
    //    string csv = "\"FolderId\";\"FileName\";\"FileSize\";\"ImageRotation\";\"PixelWidth\";\"PixelHeight\";\"ThumbnailPixelWidth\";\"ThumbnailPixelHeight\";\"ThumbnailCreationDateTime\";\"Description\";\"Hash\"\r\n" +
    //        "\"876283c6-780e-4ad5-975c-be63044c087a\";\"20200720175810_3.jpg\";\"363888\";\"Rotate0\";\"1920\";\"1080\";\"200\";\"112\";\"25/07/2020 9:45:47\";\"First file description\";\"4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4\"\r\n" +
    //        "\"876283c6-780e-4ad5-975c-be63044c087a\";\"20200720175816_3.jpg\";\"343633\";\"Rotate0\";\"1920\";\"1080\";\"200\";\"112\";\"25/07/2020 9:45:47\";\"Second file description; Includes separator character escaped.\";\"0af8f118b7d606e5d174643727bd3c0c6028b52c50481585274fd572110b108c7a0d7901227f75a72b44c89335e002a65e8137ff5b238ab1c0bba0505e783124\"\r\n";
    //    string tableName = "assets" + Guid.NewGuid();
    //    string filePath = Path.Combine("TestData", AssetConstants.Tables, tableName + ".db");

    //    IDatabase database = new Database(new ObjectListStorage(),
    //        new DataTableStorage(),
    //        new BlobStorage(),
    //        new BackupStorage());
    //    database.Initialize("TestData", ';');

    //    File.WriteAllText(filePath, csv);

    //    database.SetDataTableProperties(new DataTableProperties
    //    {
    //        TableName = tableName,
    //        ColumnProperties = new ColumnProperties[]
    //        {
    //            new ColumnProperties { ColumnName = "FolderId", EscapeText = true },
    //            new ColumnProperties { ColumnName = "FileName", EscapeText = true },
    //            new ColumnProperties { ColumnName = "FileSize", EscapeText = true },
    //            new ColumnProperties { ColumnName = "ImageRotation", EscapeText = true },
    //            new ColumnProperties { ColumnName = "PixelWidth", EscapeText = true },
    //            new ColumnProperties { ColumnName = "PixelHeight", EscapeText = true },
    //            new ColumnProperties { ColumnName = "ThumbnailPixelWidth", EscapeText = true },
    //            new ColumnProperties { ColumnName = "ThumbnailPixelHeight", EscapeText = true },
    //            new ColumnProperties { ColumnName = "ThumbnailCreationDateTime", EscapeText = true },
    //            new ColumnProperties { ColumnName = "Description", EscapeText = true },
    //            new ColumnProperties { ColumnName = "Hash", EscapeText = true }
    //        }
    //    });

    //    DataTable table = database.ReadDataTable(tableName);

    //    table.Columns.Count.Should().Be(11);
    //    table.Columns[0].ColumnName.Should().Be("FolderId");
    //    table.Columns[1].ColumnName.Should().Be("FileName");
    //    table.Columns[2].ColumnName.Should().Be("FileSize");
    //    table.Columns[3].ColumnName.Should().Be("ImageRotation");
    //    table.Columns[4].ColumnName.Should().Be("PixelWidth");
    //    table.Columns[5].ColumnName.Should().Be("PixelHeight");
    //    table.Columns[6].ColumnName.Should().Be("ThumbnailPixelWidth");
    //    table.Columns[7].ColumnName.Should().Be("ThumbnailPixelHeight");
    //    table.Columns[8].ColumnName.Should().Be("ThumbnailCreationDateTime");
    //    table.Columns[9].ColumnName.Should().Be("Description");
    //    table.Columns[10].ColumnName.Should().Be("Hash");

    //    table.TableName.Should().Be(tableName);
    //    table.Rows.Count.Should().Be(2);

    //    table.Rows[0]["FolderId"].Should().Be("876283c6-780e-4ad5-975c-be63044c087a");
    //    table.Rows[0]["FileName"].Should().Be("20200720175810_3.jpg");
    //    table.Rows[0]["FileSize"].Should().Be("363888");
    //    table.Rows[0]["ImageRotation"].Should().Be("Rotate0");
    //    table.Rows[0]["PixelWidth"].Should().Be("1920");
    //    table.Rows[0]["PixelHeight"].Should().Be("1080");
    //    table.Rows[0]["ThumbnailPixelWidth"].Should().Be("200");
    //    table.Rows[0]["ThumbnailPixelHeight"].Should().Be("112");
    //    table.Rows[0]["ThumbnailCreationDateTime"].Should().Be("25/07/2020 9:45:47");
    //    table.Rows[0]["Description"].Should().Be("First file description");
    //    table.Rows[0]["Hash"].Should().Be("4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4");

    //    table.Rows[1]["FolderId"].Should().Be("876283c6-780e-4ad5-975c-be63044c087a");
    //    table.Rows[1]["FileName"].Should().Be("20200720175816_3.jpg");
    //    table.Rows[1]["FileSize"].Should().Be("343633");
    //    table.Rows[1]["ImageRotation"].Should().Be("Rotate0");
    //    table.Rows[1]["PixelWidth"].Should().Be("1920");
    //    table.Rows[1]["PixelHeight"].Should().Be("1080");
    //    table.Rows[1]["ThumbnailPixelWidth"].Should().Be("200");
    //    table.Rows[1]["ThumbnailPixelHeight"].Should().Be("112");
    //    table.Rows[1]["ThumbnailCreationDateTime"].Should().Be("25/07/2020 9:45:47");
    //    table.Rows[1]["Description"].Should().Be("Second file description; Includes separator character escaped.");
    //    table.Rows[1]["Hash"].Should().Be("0af8f118b7d606e5d174643727bd3c0c6028b52c50481585274fd572110b108c7a0d7901227f75a72b44c89335e002a65e8137ff5b238ab1c0bba0505e783124");

    //    database.Diagnostics.LastReadFilePath.Should().Be(filePath);
    //    database.Diagnostics.LastReadFileRaw.Should().Be(csv);
    //}

    //[Fact]
    //public void ReadDataTable_SomeColumnsWithEscapedText()
    //{
    //    string csv = "\"FolderId\";FileName;FileSize;ImageRotation;PixelWidth;PixelHeight;ThumbnailPixelWidth;ThumbnailPixelHeight;ThumbnailCreationDateTime;\"Description\";Hash\r\n" +
    //        "\"876283c6-780e-4ad5-975c-be63044c087a\";20200720175810_3.jpg;363888;Rotate0;1920;1080;200;112;25/07/2020 9:45:47;\"First file description\";4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4\r\n" +
    //        "\"876283c6-780e-4ad5-975c-be63044c087a\";20200720175816_3.jpg;343633;Rotate0;1920;1080;200;112;25/07/2020 9:45:47;\"Second file description; Includes separator character escaped.\";0af8f118b7d606e5d174643727bd3c0c6028b52c50481585274fd572110b108c7a0d7901227f75a72b44c89335e002a65e8137ff5b238ab1c0bba0505e783124\r\n";
    //    string tableName = "assets" + Guid.NewGuid();
    //    string filePath = Path.Combine("TestData", AssetConstants.Tables, tableName + ".db");

    //    IDatabase database = new Database(new ObjectListStorage(),
    //        new DataTableStorage(),
    //        new BlobStorage(),
    //        new BackupStorage());
    //    database.Initialize("TestData", ';');

    //    File.WriteAllText(filePath, csv);

    //    database.SetDataTableProperties(new DataTableProperties
    //    {
    //        TableName = tableName,
    //        ColumnProperties = new ColumnProperties[]
    //        {
    //            new ColumnProperties { ColumnName = "FolderId", EscapeText = true },
    //            new ColumnProperties { ColumnName = "FileName", EscapeText = false },
    //            new ColumnProperties { ColumnName = "FileSize", EscapeText = false },
    //            new ColumnProperties { ColumnName = "ImageRotation", EscapeText = false },
    //            new ColumnProperties { ColumnName = "PixelWidth", EscapeText = false },
    //            new ColumnProperties { ColumnName = "PixelHeight", EscapeText = false },
    //            new ColumnProperties { ColumnName = "ThumbnailPixelWidth", EscapeText = false },
    //            new ColumnProperties { ColumnName = "ThumbnailPixelHeight", EscapeText = false },
    //            new ColumnProperties { ColumnName = "ThumbnailCreationDateTime", EscapeText = false },
    //            new ColumnProperties { ColumnName = "Description", EscapeText = true },
    //            new ColumnProperties { ColumnName = "Hash", EscapeText = false },
    //        }
    //    });

    //    DataTable table = database.ReadDataTable(tableName);

    //    table.Columns.Count.Should().Be(11);
    //    table.Columns[0].ColumnName.Should().Be("FolderId");
    //    table.Columns[1].ColumnName.Should().Be("FileName");
    //    table.Columns[2].ColumnName.Should().Be("FileSize");
    //    table.Columns[3].ColumnName.Should().Be("ImageRotation");
    //    table.Columns[4].ColumnName.Should().Be("PixelWidth");
    //    table.Columns[5].ColumnName.Should().Be("PixelHeight");
    //    table.Columns[6].ColumnName.Should().Be("ThumbnailPixelWidth");
    //    table.Columns[7].ColumnName.Should().Be("ThumbnailPixelHeight");
    //    table.Columns[8].ColumnName.Should().Be("ThumbnailCreationDateTime");
    //    table.Columns[9].ColumnName.Should().Be("Description");
    //    table.Columns[10].ColumnName.Should().Be("Hash");

    //    table.TableName.Should().Be(tableName);
    //    table.Rows.Count.Should().Be(2);

    //    table.Rows[0]["FolderId"].Should().Be("876283c6-780e-4ad5-975c-be63044c087a");
    //    table.Rows[0]["FileName"].Should().Be("20200720175810_3.jpg");
    //    table.Rows[0]["FileSize"].Should().Be("363888");
    //    table.Rows[0]["ImageRotation"].Should().Be("Rotate0");
    //    table.Rows[0]["PixelWidth"].Should().Be("1920");
    //    table.Rows[0]["PixelHeight"].Should().Be("1080");
    //    table.Rows[0]["ThumbnailPixelWidth"].Should().Be("200");
    //    table.Rows[0]["ThumbnailPixelHeight"].Should().Be("112");
    //    table.Rows[0]["ThumbnailCreationDateTime"].Should().Be("25/07/2020 9:45:47");
    //    table.Rows[0]["Description"].Should().Be("First file description");
    //    table.Rows[0]["Hash"].Should().Be("4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4");

    //    table.Rows[1]["FolderId"].Should().Be("876283c6-780e-4ad5-975c-be63044c087a");
    //    table.Rows[1]["FileName"].Should().Be("20200720175816_3.jpg");
    //    table.Rows[1]["FileSize"].Should().Be("343633");
    //    table.Rows[1]["ImageRotation"].Should().Be("Rotate0");
    //    table.Rows[1]["PixelWidth"].Should().Be("1920");
    //    table.Rows[1]["PixelHeight"].Should().Be("1080");
    //    table.Rows[1]["ThumbnailPixelWidth"].Should().Be("200");
    //    table.Rows[1]["ThumbnailPixelHeight"].Should().Be("112");
    //    table.Rows[1]["ThumbnailCreationDateTime"].Should().Be("25/07/2020 9:45:47");
    //    table.Rows[1]["Description"].Should().Be("Second file description; Includes separator character escaped.");
    //    table.Rows[1]["Hash"].Should().Be("0af8f118b7d606e5d174643727bd3c0c6028b52c50481585274fd572110b108c7a0d7901227f75a72b44c89335e002a65e8137ff5b238ab1c0bba0505e783124");

    //    database.Diagnostics.LastReadFilePath.Should().Be(filePath);
    //    database.Diagnostics.LastReadFileRaw.Should().Be(csv);
    //}

    //[Fact]
    //public void WriteDataTable_NullDataTable_ThrowException()
    //{
    //    DataTable table = new();

    //    IDatabase database = new Database(new ObjectListStorage(),
    //        new DataTableStorage(),
    //        new BlobStorage(),
    //        new BackupStorage());
    //    database.Initialize("TestData", ';');

    //    Action action = () => database.WriteDataTable(null);
    //    action.Should().Throw<ArgumentNullException>();
    //}

    //[Fact]
    //public void WriteDataTable_DataTableWithNoTableName_ThrowException()
    //{
    //    DataTable table = new();

    //    IDatabase database = new Database(new ObjectListStorage(),
    //        new DataTableStorage(),
    //        new BlobStorage(),
    //        new BackupStorage());
    //    database.Initialize("TestData", ';');

    //    Action action = () => database.WriteDataTable(new DataTable());
    //    action.Should().Throw<ArgumentException>();
    //}

    //[Fact]
    //public void WriteDataTable_DataTableWithNoColumns_ThrowException()
    //{
    //    DataTable table = new();

    //    IDatabase database = new Database(new ObjectListStorage(),
    //        new DataTableStorage(),
    //        new BlobStorage(),
    //        new BackupStorage());
    //    database.Initialize("TestData", ';');

    //    Action action = () => database.WriteDataTable(new DataTable("Data"));
    //    action.Should().Throw<ArgumentException>();
    //}

    //[Fact]
    //public void WriteDataTable_DataColumnWithNoName_ThrowException()
    //{
    //    string tableName = "assets" + Guid.NewGuid();
    //    string filePath = Path.Combine("TestData", AssetConstants.Tables, tableName + ".db");

    //    DataTable table = new(tableName);
    //    table.Columns.Add("FolderId");
    //    table.Columns.Add(" ");

    //    table.AcceptChanges();

    //    IDatabase database = new Database(new ObjectListStorage(),
    //        new DataTableStorage(),
    //        new BlobStorage(),
    //        new BackupStorage());
    //    database.Initialize("TestData", ';');

    //    Action action = () => database.WriteDataTable(table);
    //    action.Should().Throw<ArgumentException>();
    //}

    //[Fact]
    //public void WriteDataTable_AllColumnsWithUnescapedTextWithoutDataTableProperties()
    //{
    //    string expected = "FolderId;FileName;FileSize;ImageRotation;PixelWidth;PixelHeight;ThumbnailPixelWidth;ThumbnailPixelHeight;ThumbnailCreationDateTime;Hash\r\n" +
    //        "876283c6-780e-4ad5-975c-be63044c087a;20200720175810_3.jpg;363888;Rotate0;1920;1080;200;112;25/07/2020 9:45:47;4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4\r\n" +
    //        "876283c6-780e-4ad5-975c-be63044c087a;20200720175816_3.jpg;343633;Rotate0;1920;1080;200;112;25/07/2020 9:45:47;0af8f118b7d606e5d174643727bd3c0c6028b52c50481585274fd572110b108c7a0d7901227f75a72b44c89335e002a65e8137ff5b238ab1c0bba0505e783124\r\n";
    //    string tableName = "assets" + Guid.NewGuid();
    //    string filePath = Path.Combine("TestData", AssetConstants.Tables, tableName + ".db");

    //    DataTable table = new(tableName);
    //    table.Columns.Add("FolderId");
    //    table.Columns.Add("FileName");
    //    table.Columns.Add("FileSize");
    //    table.Columns.Add("ImageRotation");
    //    table.Columns.Add("PixelWidth");
    //    table.Columns.Add("PixelHeight");
    //    table.Columns.Add("ThumbnailPixelWidth");
    //    table.Columns.Add("ThumbnailPixelHeight");
    //    table.Columns.Add("ThumbnailCreationDateTime");
    //    table.Columns.Add("Hash");

    //    DataRow row = table.NewRow();
    //    row["FolderId"] = "876283c6-780e-4ad5-975c-be63044c087a";
    //    row["FileName"] = "20200720175810_3.jpg";
    //    row["FileSize"] = "363888";
    //    row["ImageRotation"] = "Rotate0";
    //    row["PixelWidth"] = "1920";
    //    row["PixelHeight"] = "1080";
    //    row["ThumbnailPixelWidth"] = "200";
    //    row["ThumbnailPixelHeight"] = "112";
    //    row["ThumbnailCreationDateTime"] = "25/07/2020 9:45:47";
    //    row["Hash"] = "4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4";
    //    table.Rows.Add(row);

    //    row = table.NewRow();
    //    row["FolderId"] = "876283c6-780e-4ad5-975c-be63044c087a";
    //    row["FileName"] = "20200720175816_3.jpg";
    //    row["FileSize"] = "343633";
    //    row["ImageRotation"] = "Rotate0";
    //    row["PixelWidth"] = "1920";
    //    row["PixelHeight"] = "1080";
    //    row["ThumbnailPixelWidth"] = "200";
    //    row["ThumbnailPixelHeight"] = "112";
    //    row["ThumbnailCreationDateTime"] = "25/07/2020 9:45:47";
    //    row["Hash"] = "0af8f118b7d606e5d174643727bd3c0c6028b52c50481585274fd572110b108c7a0d7901227f75a72b44c89335e002a65e8137ff5b238ab1c0bba0505e783124";
    //    table.Rows.Add(row);

    //    table.AcceptChanges();

    //    IDatabase database = new Database(new ObjectListStorage(),
    //        new DataTableStorage(),
    //        new BlobStorage(),
    //        new BackupStorage());
    //    database.Initialize("TestData", ';');
    //    database.WriteDataTable(table);

    //    database.Diagnostics.LastWriteFilePath.Should().Be(filePath);
    //    database.Diagnostics.LastWriteFileRaw.Should().Be(expected);

    //    string content = File.ReadAllText(filePath);
    //    content.Should().Be(expected);
    //}

    //[Fact]
    //public void WriteDataTable_AllColumnsWithEscapedText()
    //{
    //    string expected = "\"FolderId\";\"FileName\";\"FileSize\";\"ImageRotation\";\"PixelWidth\";\"PixelHeight\";\"ThumbnailPixelWidth\";\"ThumbnailPixelHeight\";\"ThumbnailCreationDateTime\";\"Description\";\"Hash\"\r\n" +
    //        "\"876283c6-780e-4ad5-975c-be63044c087a\";\"20200720175810_3.jpg\";\"363888\";\"Rotate0\";\"1920\";\"1080\";\"200\";\"112\";\"25/07/2020 9:45:47\";\"First file description\";\"4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4\"\r\n" +
    //        "\"876283c6-780e-4ad5-975c-be63044c087a\";\"20200720175816_3.jpg\";\"343633\";\"Rotate0\";\"1920\";\"1080\";\"200\";\"112\";\"25/07/2020 9:45:47\";\"Second file description; Includes separator character escaped.\";\"0af8f118b7d606e5d174643727bd3c0c6028b52c50481585274fd572110b108c7a0d7901227f75a72b44c89335e002a65e8137ff5b238ab1c0bba0505e783124\"\r\n";
    //    string tableName = "assets" + Guid.NewGuid();
    //    string filePath = Path.Combine("TestData", AssetConstants.Tables, tableName + ".db");

    //    DataTable table = new(tableName);
    //    table.Columns.Add("FolderId");
    //    table.Columns.Add("FileName");
    //    table.Columns.Add("FileSize");
    //    table.Columns.Add("ImageRotation");
    //    table.Columns.Add("PixelWidth");
    //    table.Columns.Add("PixelHeight");
    //    table.Columns.Add("ThumbnailPixelWidth");
    //    table.Columns.Add("ThumbnailPixelHeight");
    //    table.Columns.Add("ThumbnailCreationDateTime");
    //    table.Columns.Add("Description");
    //    table.Columns.Add("Hash");

    //    DataRow row = table.NewRow();
    //    row["FolderId"] = "876283c6-780e-4ad5-975c-be63044c087a";
    //    row["FileName"] = "20200720175810_3.jpg";
    //    row["FileSize"] = "363888";
    //    row["ImageRotation"] = "Rotate0";
    //    row["PixelWidth"] = "1920";
    //    row["PixelHeight"] = "1080";
    //    row["ThumbnailPixelWidth"] = "200";
    //    row["ThumbnailPixelHeight"] = "112";
    //    row["ThumbnailCreationDateTime"] = "25/07/2020 9:45:47";
    //    row["Description"] = "First file description";
    //    row["Hash"] = "4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4";
    //    table.Rows.Add(row);

    //    row = table.NewRow();
    //    row["FolderId"] = "876283c6-780e-4ad5-975c-be63044c087a";
    //    row["FileName"] = "20200720175816_3.jpg";
    //    row["FileSize"] = "343633";
    //    row["ImageRotation"] = "Rotate0";
    //    row["PixelWidth"] = "1920";
    //    row["PixelHeight"] = "1080";
    //    row["ThumbnailPixelWidth"] = "200";
    //    row["ThumbnailPixelHeight"] = "112";
    //    row["ThumbnailCreationDateTime"] = "25/07/2020 9:45:47";
    //    row["Description"] = "Second file description; Includes separator character escaped.";
    //    row["Hash"] = "0af8f118b7d606e5d174643727bd3c0c6028b52c50481585274fd572110b108c7a0d7901227f75a72b44c89335e002a65e8137ff5b238ab1c0bba0505e783124";
    //    table.Rows.Add(row);

    //    table.AcceptChanges();

    //    IDatabase database = new Database(new ObjectListStorage(),
    //        new DataTableStorage(),
    //        new BlobStorage(),
    //        new BackupStorage());
    //    database.Initialize("TestData", ';');
    //    database.SetDataTableProperties(new DataTableProperties
    //    {
    //        TableName = tableName,
    //        ColumnProperties = new ColumnProperties[]
    //        {
    //            new ColumnProperties { ColumnName = "FolderId", EscapeText = true },
    //            new ColumnProperties { ColumnName = "FileName", EscapeText = true },
    //            new ColumnProperties { ColumnName = "FileSize", EscapeText = true },
    //            new ColumnProperties { ColumnName = "ImageRotation", EscapeText = true },
    //            new ColumnProperties { ColumnName = "PixelWidth", EscapeText = true },
    //            new ColumnProperties { ColumnName = "PixelHeight", EscapeText = true },
    //            new ColumnProperties { ColumnName = "ThumbnailPixelWidth", EscapeText = true },
    //            new ColumnProperties { ColumnName = "ThumbnailPixelHeight", EscapeText = true },
    //            new ColumnProperties { ColumnName = "ThumbnailCreationDateTime", EscapeText = true },
    //            new ColumnProperties { ColumnName = "Description", EscapeText = true },
    //            new ColumnProperties { ColumnName = "Hash", EscapeText = true }
    //        }
    //    });

    //    database.WriteDataTable(table);

    //    database.Diagnostics.LastWriteFilePath.Should().Be(filePath);
    //    database.Diagnostics.LastWriteFileRaw.Should().Be(expected);

    //    string content = File.ReadAllText(filePath);
    //    content.Should().Be(expected);
    //}

    //[Fact]
    //public void WriteDataTable_SomeColumnsWithEscapedText()
    //{
    //    string expected = "\"FolderId\";FileName;FileSize;ImageRotation;PixelWidth;PixelHeight;ThumbnailPixelWidth;ThumbnailPixelHeight;ThumbnailCreationDateTime;\"Description\";Hash\r\n" +
    //        "\"876283c6-780e-4ad5-975c-be63044c087a\";20200720175810_3.jpg;363888;Rotate0;1920;1080;200;112;25/07/2020 9:45:47;\"First file description\";4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4\r\n" +
    //        "\"876283c6-780e-4ad5-975c-be63044c087a\";20200720175816_3.jpg;343633;Rotate0;1920;1080;200;112;25/07/2020 9:45:47;\"Second file description; Includes separator character escaped.\";0af8f118b7d606e5d174643727bd3c0c6028b52c50481585274fd572110b108c7a0d7901227f75a72b44c89335e002a65e8137ff5b238ab1c0bba0505e783124\r\n";
    //    string tableName = "assets" + Guid.NewGuid();
    //    string filePath = Path.Combine("TestData", AssetConstants.Tables, tableName + ".db");

    //    DataTable table = new(tableName);
    //    table.Columns.Add("FolderId");
    //    table.Columns.Add("FileName");
    //    table.Columns.Add("FileSize");
    //    table.Columns.Add("ImageRotation");
    //    table.Columns.Add("PixelWidth");
    //    table.Columns.Add("PixelHeight");
    //    table.Columns.Add("ThumbnailPixelWidth");
    //    table.Columns.Add("ThumbnailPixelHeight");
    //    table.Columns.Add("ThumbnailCreationDateTime");
    //    table.Columns.Add("Description");
    //    table.Columns.Add("Hash");

    //    DataRow row = table.NewRow();
    //    row["FolderId"] = "876283c6-780e-4ad5-975c-be63044c087a";
    //    row["FileName"] = "20200720175810_3.jpg";
    //    row["FileSize"] = "363888";
    //    row["ImageRotation"] = "Rotate0";
    //    row["PixelWidth"] = "1920";
    //    row["PixelHeight"] = "1080";
    //    row["ThumbnailPixelWidth"] = "200";
    //    row["ThumbnailPixelHeight"] = "112";
    //    row["ThumbnailCreationDateTime"] = "25/07/2020 9:45:47";
    //    row["Description"] = "First file description";
    //    row["Hash"] = "4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4";
    //    table.Rows.Add(row);

    //    row = table.NewRow();
    //    row["FolderId"] = "876283c6-780e-4ad5-975c-be63044c087a";
    //    row["FileName"] = "20200720175816_3.jpg";
    //    row["FileSize"] = "343633";
    //    row["ImageRotation"] = "Rotate0";
    //    row["PixelWidth"] = "1920";
    //    row["PixelHeight"] = "1080";
    //    row["ThumbnailPixelWidth"] = "200";
    //    row["ThumbnailPixelHeight"] = "112";
    //    row["ThumbnailCreationDateTime"] = "25/07/2020 9:45:47";
    //    row["Description"] = "Second file description; Includes separator character escaped.";
    //    row["Hash"] = "0af8f118b7d606e5d174643727bd3c0c6028b52c50481585274fd572110b108c7a0d7901227f75a72b44c89335e002a65e8137ff5b238ab1c0bba0505e783124";
    //    table.Rows.Add(row);

    //    table.AcceptChanges();

    //    IDatabase database = new Database(new ObjectListStorage(),
    //        new DataTableStorage(),
    //        new BlobStorage(),
    //        new BackupStorage());
    //    database.Initialize("TestData", ';');
    //    database.SetDataTableProperties(new DataTableProperties
    //    {
    //        TableName = tableName,
    //        ColumnProperties = new ColumnProperties[]
    //        {
    //            new ColumnProperties { ColumnName = "FolderId", EscapeText = true },
    //            new ColumnProperties { ColumnName = "Description", EscapeText = true }
    //        }
    //    });

    //    database.WriteDataTable(table);

    //    database.Diagnostics.LastWriteFilePath.Should().Be(filePath);
    //    database.Diagnostics.LastWriteFileRaw.Should().Be(expected);

    //    string content = File.ReadAllText(filePath);
    //    content.Should().Be(expected);
    //}
}
