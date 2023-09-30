namespace PhotoManager.Tests.Unit.Infrastructure.TablesConfig;

[TestFixture]
public class AssetConfigsTests
{
    private readonly Guid folderId = Guid.NewGuid();
    private string[]? validValues;
    private string[]? tooManyValues;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        validValues = new string[]
        {
            folderId.ToString(),
            "Image 1.jpg",
            "1000",
            "0",
            "1920",
            "1080",
            "120",
            "60",
            "2023-08-30T12:00:00",
            "4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4",
            "The asset is corrupted",
            "True",
            null!,
            "False"
        };

        tooManyValues = new string[]
        {
            folderId.ToString(),
            "Image 1.jpg",
            "1000",
            "0",
            "1920",
            "1080",
            "120",
            "60",
            "2023-08-30T12:00:00",
            "4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4",
            "The asset is corrupted",
            "True",
            null!,
            "False",
            "False",
            "False"
        };
    }

    [Test]
    public void ConfigureDataTable_ReturnsCorrectColumnNames()
    {
        ColumnProperties[] columns = AssetConfigs.ConfigureDataTable();

        Assert.IsNotNull(columns);
        Assert.AreEqual(14, columns.Length);

        Assert.AreEqual("FolderId", columns[0].ColumnName);
        Assert.AreEqual("FileName", columns[1].ColumnName);
        Assert.AreEqual("FileSize", columns[2].ColumnName);
        Assert.AreEqual("ImageRotation", columns[3].ColumnName);
        Assert.AreEqual("PixelWidth", columns[4].ColumnName);
        Assert.AreEqual("PixelHeight", columns[5].ColumnName);
        Assert.AreEqual("ThumbnailPixelWidth", columns[6].ColumnName);
        Assert.AreEqual("ThumbnailPixelHeight", columns[7].ColumnName);
        Assert.AreEqual("ThumbnailCreationDateTime", columns[8].ColumnName);
        Assert.AreEqual("Hash", columns[9].ColumnName);
        Assert.AreEqual("AssetCorruptedMessage", columns[10].ColumnName);
        Assert.AreEqual("IsAssetCorrupted", columns[11].ColumnName);
        Assert.AreEqual("AssetRotatedMessage", columns[12].ColumnName);
        Assert.AreEqual("IsAssetRotated", columns[13].ColumnName);
    }

    [Test]
    public void ReadFunc_ValidValues_ParsesStringArrayIntoAsset()
    {
        Asset asset = AssetConfigs.ReadFunc(validValues!);

        Assert.IsNotNull(asset);
        Assert.AreEqual(folderId, asset.FolderId);
        Assert.AreEqual("Image 1.jpg", asset.FileName);
        Assert.AreEqual(1000, asset.FileSize);
        Assert.AreEqual(Rotation.Rotate0, asset.ImageRotation);
        Assert.AreEqual(1920, asset.PixelWidth);
        Assert.AreEqual(1080, asset.PixelHeight);
        Assert.AreEqual(120, asset.ThumbnailPixelWidth);
        Assert.AreEqual(60, asset.ThumbnailPixelHeight);
        Assert.AreEqual(new DateTime(2023, 08, 30, 12, 0, 0), asset.ThumbnailCreationDateTime);
        Assert.AreEqual("4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4", asset.Hash);
        Assert.AreEqual("The asset is corrupted", asset.AssetCorruptedMessage);
        Assert.IsTrue(asset.IsAssetCorrupted);
        Assert.AreEqual(null, asset.AssetRotatedMessage);
        Assert.IsFalse(asset.IsAssetRotated);
    }

    [Test]
    public void ReadFunc_TooManyValues_ParsesStringArrayIntoAsset()
    {
        Asset asset = AssetConfigs.ReadFunc(tooManyValues!);

        Assert.IsNotNull(asset);
        Assert.AreEqual(folderId, asset.FolderId);
        Assert.AreEqual("Image 1.jpg", asset.FileName);
        Assert.AreEqual(1000, asset.FileSize);
        Assert.AreEqual(Rotation.Rotate0, asset.ImageRotation);
        Assert.AreEqual(1920, asset.PixelWidth);
        Assert.AreEqual(1080, asset.PixelHeight);
        Assert.AreEqual(120, asset.ThumbnailPixelWidth);
        Assert.AreEqual(60, asset.ThumbnailPixelHeight);
        Assert.AreEqual(new DateTime(2023, 08, 30, 12, 0, 0), asset.ThumbnailCreationDateTime);
        Assert.AreEqual("4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4", asset.Hash);
        Assert.AreEqual("The asset is corrupted", asset.AssetCorruptedMessage);
        Assert.IsTrue(asset.IsAssetCorrupted);
        Assert.AreEqual(null, asset.AssetRotatedMessage);
        Assert.IsFalse(asset.IsAssetRotated);
    }

    [Test]
    public void ReadFunc_NullValues_ThrowsArgumentNullException()
    {
        string[] nullValues = new string[14];

        var exception = Assert.Throws<ArgumentNullException>(() => AssetConfigs.ReadFunc(nullValues));

        Assert.AreEqual("Value cannot be null. (Parameter 'g')", exception?.Message);
    }

    [Test]
    public void ReadFunc_EmptyArray_ThrowsIndexOutOfRangeException()
    {
        string[] emptyArrray = Array.Empty<string>();

        Assert.Throws<IndexOutOfRangeException>(() => AssetConfigs.ReadFunc(emptyArrray));
    }

    [Test]
    public void ReadFunc_InvalidValues_ThrowsFormatException()
    {
        string[] invalidValues = new string[]
        {
            folderId.ToString(),
            "Image 1.jpg",
            "toto",
            "0",
            "1920",
            "toto",
            "120",
            "60",
            "60",
            "4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4",
            "15",
            "15",
            null!,
            "False"
        };

        var exception = Assert.Throws<FormatException>(() => AssetConfigs.ReadFunc(invalidValues));

        Assert.AreEqual("The input string 'toto' was not in a correct format.", exception?.Message);
    }

    [Test]
    public void WriteFunc_AssetWithValidValues_ConvertsAssetPropertiesToIndexInArray()
    {
        Asset asset = new ()
        {
            FolderId = folderId,
            FileName = "Image 1.jpg",
            FileSize = 1000,
            ImageRotation = Rotation.Rotate0,
            PixelWidth = 1920,
            PixelHeight = 1080,
            ThumbnailPixelWidth = 120,
            ThumbnailPixelHeight = 60,
            ThumbnailCreationDateTime = new DateTime(2023, 08, 30, 12, 0, 0),
            Hash = "4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4",
            AssetCorruptedMessage = "The asset is corrupted",
            IsAssetCorrupted = true,
            AssetRotatedMessage = null,
            IsAssetRotated = false
        };

        object[] result = new object[14];
        for (int i = 0; i < 14; i++)
        {
            result[i] = AssetConfigs.WriteFunc(asset, i);
        }

        Assert.AreEqual(folderId, result[0]);
        Assert.AreEqual("Image 1.jpg", result[1]);
        Assert.AreEqual(1000, result[2]);
        Assert.AreEqual(Rotation.Rotate0, result[3]);
        Assert.AreEqual(1920, result[4]);
        Assert.AreEqual(1080, result[5]);
        Assert.AreEqual(120, result[6]);
        Assert.AreEqual(60, result[7]);
        Assert.AreEqual(new DateTime(2023, 08, 30, 12, 0, 0), result[8]);
        Assert.AreEqual("4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4", result[9]);
        Assert.AreEqual("The asset is corrupted", result[10]);
        Assert.AreEqual(true, result[11]);
        Assert.AreEqual(null, result[12]);
        Assert.AreEqual(false, result[13]);
    }

    [Test]
    public void WriteFunc_AssetWithPartialValues_ConvertsAssetPropertiesToIndexInArrayWithSomeDefaultValues()
    {
        Asset asset = new()
        {
            FolderId = folderId,
            ThumbnailCreationDateTime = new DateTime(2023, 08, 30, 12, 0, 0),
            Hash = "4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4",
            AssetCorruptedMessage = "The asset is corrupted",
            AssetRotatedMessage = null,
            IsAssetRotated = false
        };

        object[] result = new object[14];
        for (int i = 0; i < 14; i++)
        {
            result[i] = AssetConfigs.WriteFunc(asset, i);
        }

        Assert.AreEqual(folderId, result[0]);
        Assert.AreEqual(null, result[1]);
        Assert.AreEqual(0, result[2]);
        Assert.AreEqual(Rotation.Rotate0, result[3]);
        Assert.AreEqual(0, result[4]);
        Assert.AreEqual(0, result[5]);
        Assert.AreEqual(0, result[6]);
        Assert.AreEqual(0, result[7]);
        Assert.AreEqual(new DateTime(2023, 08, 30, 12, 0, 0), result[8]);
        Assert.AreEqual("4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4", result[9]);
        Assert.AreEqual("The asset is corrupted", result[10]);
        Assert.AreEqual(false, result[11]);
        Assert.AreEqual(null, result[12]);
        Assert.AreEqual(false, result[13]);
    }

    [Test]
    public void WriteFunc_IndexOutOfRange_ThrowsArgumentOutOfRangeException()
    {
        Asset asset = new()
        {
            FolderId = folderId,
            FileName = "Image 1.jpg",
            FileSize = 1000,
            ImageRotation = Rotation.Rotate0,
            PixelWidth = 1920,
            PixelHeight = 1080,
            ThumbnailPixelWidth = 120,
            ThumbnailPixelHeight = 60,
            ThumbnailCreationDateTime = new DateTime(2023, 08, 30, 12, 0, 0),
            Hash = "4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4",
            AssetCorruptedMessage = "The asset is corrupted",
            IsAssetCorrupted = true,
            AssetRotatedMessage = null,
            IsAssetRotated = false
        };

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => AssetConfigs.WriteFunc(asset, 15));

        Assert.AreEqual("Specified argument was out of the range of valid values. (Parameter 'i')", exception?.Message);
    }

    [Test]
    public void WriteFunc_AssetIsNull_ThrowsNullReferenceException()
    {
        Asset? asset = null;

        for (int i = 0; i < 14; i++)
        {
            Assert.Throws<NullReferenceException>(() => AssetConfigs.WriteFunc(asset!, i));
        }
    }
}