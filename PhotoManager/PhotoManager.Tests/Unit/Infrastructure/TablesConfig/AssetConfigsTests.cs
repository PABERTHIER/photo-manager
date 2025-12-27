namespace PhotoManager.Tests.Unit.Infrastructure.TablesConfig;

[TestFixture]
public class AssetConfigsTests
{
    private readonly Guid _folderId = Guid.NewGuid();
    private string[]? _validValues;
    private string[]? _tooManyValues;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _validValues =
        [
            _folderId.ToString(),
            "Image 1.jpg",
            "0",
            "1920",
            "1080",
            "120",
            "60",
            "12/30/2023 12:00:00",
            "4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4",
            "The asset is corrupted",
            "True",
            null!,
            "False"
        ];

        _tooManyValues =
        [
            _folderId.ToString(),
            "Image 1.jpg",
            "0",
            "1920",
            "1080",
            "120",
            "60",
            "12/30/2023 12:00:00",
            "4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4",
            "The asset is corrupted",
            "True",
            null!,
            "False",
            "False",
            "False",
            "1000"
        ];
    }

    [Test]
    public void ConfigureDataTable_ReturnsCorrectColumnNames()
    {
        ColumnProperties[] columns = AssetConfigs.ConfigureDataTable();

        Assert.That(columns, Is.Not.Null);
        Assert.That(columns, Has.Length.EqualTo(13));

        Assert.That(columns[0].ColumnName, Is.EqualTo("FolderId"));
        Assert.That(columns[1].ColumnName, Is.EqualTo("FileName"));
        Assert.That(columns[2].ColumnName, Is.EqualTo("ImageRotation"));
        Assert.That(columns[3].ColumnName, Is.EqualTo("PixelWidth"));
        Assert.That(columns[4].ColumnName, Is.EqualTo("PixelHeight"));
        Assert.That(columns[5].ColumnName, Is.EqualTo("ThumbnailPixelWidth"));
        Assert.That(columns[6].ColumnName, Is.EqualTo("ThumbnailPixelHeight"));
        Assert.That(columns[7].ColumnName, Is.EqualTo("ThumbnailCreationDateTime"));
        Assert.That(columns[8].ColumnName, Is.EqualTo("Hash"));
        Assert.That(columns[9].ColumnName, Is.EqualTo("CorruptedMessage"));
        Assert.That(columns[10].ColumnName, Is.EqualTo("IsCorrupted"));
        Assert.That(columns[11].ColumnName, Is.EqualTo("RotatedMessage"));
        Assert.That(columns[12].ColumnName, Is.EqualTo("IsRotated"));
    }

    [Test]
    public void ReadFunc_ValidValues_ParsesStringArrayIntoAsset()
    {
        Asset asset = AssetConfigs.ReadFunc(_validValues!);

        Assert.That(asset, Is.Not.Null);
        Assert.That(asset.FolderId, Is.EqualTo(_folderId));
        Assert.That(asset.FileName, Is.EqualTo("Image 1.jpg"));
        Assert.That(asset.ImageRotation, Is.EqualTo(Rotation.Rotate0));
        Assert.That(asset.Pixel.Asset.Width, Is.EqualTo(1920));
        Assert.That(asset.Pixel.Asset.Height, Is.EqualTo(1080));
        Assert.That(asset.Pixel.Thumbnail.Width, Is.EqualTo(120));
        Assert.That(asset.Pixel.Thumbnail.Height, Is.EqualTo(60));
        Assert.That(asset.ThumbnailCreationDateTime, Is.EqualTo(new DateTime(2023, 12, 30, 12, 0, 0)));
        Assert.That(asset.Hash, Is.EqualTo("4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4"));
        Assert.That(asset.Metadata.Corrupted.IsTrue, Is.True);
        Assert.That(asset.Metadata.Corrupted.Message, Is.EqualTo("The asset is corrupted"));
        Assert.That(asset.Metadata.Rotated.IsTrue, Is.False);
        Assert.That(asset.Metadata.Rotated.Message, Is.Null);
    }

    [Test]
    public void ReadFunc_TooManyValues_ParsesStringArrayIntoAsset()
    {
        Asset asset = AssetConfigs.ReadFunc(_tooManyValues!);

        Assert.That(asset, Is.Not.Null);
        Assert.That(asset.FolderId, Is.EqualTo(_folderId));
        Assert.That(asset.FileName, Is.EqualTo("Image 1.jpg"));
        Assert.That(asset.ImageRotation, Is.EqualTo(Rotation.Rotate0));
        Assert.That(asset.Pixel.Asset.Width, Is.EqualTo(1920));
        Assert.That(asset.Pixel.Asset.Height, Is.EqualTo(1080));
        Assert.That(asset.Pixel.Thumbnail.Width, Is.EqualTo(120));
        Assert.That(asset.Pixel.Thumbnail.Height, Is.EqualTo(60));
        Assert.That(asset.ThumbnailCreationDateTime, Is.EqualTo(new DateTime(2023, 12, 30, 12, 0, 0)));
        Assert.That(asset.Hash, Is.EqualTo("4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4"));
        Assert.That(asset.Metadata.Corrupted.IsTrue, Is.True);
        Assert.That(asset.Metadata.Corrupted.Message, Is.EqualTo("The asset is corrupted"));
        Assert.That(asset.Metadata.Rotated.IsTrue, Is.False);
        Assert.That(asset.Metadata.Rotated.Message, Is.Null);
    }

    [Test]
    public void ReadFunc_NullValues_ThrowsArgumentNullException()
    {
        string[] nullValues = new string[13];

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => AssetConfigs.ReadFunc(nullValues));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'g')"));
    }

    [Test]
    public void ReadFunc_EmptyArray_ThrowsIndexOutOfRangeException()
    {
        string[] emptyArray = [];

        IndexOutOfRangeException? exception = Assert.Throws<IndexOutOfRangeException>(() => AssetConfigs.ReadFunc(emptyArray));

        Assert.That(exception?.Message, Is.EqualTo("Index was outside the bounds of the array."));
    }

    [Test]
    public void ReadFunc_InvalidValues_ThrowsFormatException()
    {
        string[] invalidValues =
        [
            _folderId.ToString(),
            "Image 1.jpg",
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
        ];

        FormatException? exception = Assert.Throws<FormatException>(() => AssetConfigs.ReadFunc(invalidValues));

        Assert.That(exception?.Message, Is.EqualTo("The input string 'toto' was not in a correct format."));
    }

    [Test]
    public void WriteFunc_AssetWithValidValues_ConvertsAssetPropertiesToIndexInArray()
    {
        Asset asset = new()
        {
            FolderId = _folderId,
            Folder = new() { Id = _folderId, Path = "" },
            FileName = "Image 1.jpg",
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = 1920, Height = 1080 },
                Thumbnail = new() { Width = 120, Height = 60 }
            },
            FileProperties = new() { Size = 1000 },
            ThumbnailCreationDateTime = new (2023, 08, 30, 12, 0, 0),
            Hash = "4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4",
            Metadata = new()
            {
                Corrupted = new() { IsTrue = true, Message = "The asset is corrupted" },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };

        object[] result = new object[13];
        for (int i = 0; i < 13; i++)
        {
            result[i] = AssetConfigs.WriteFunc(asset, i);
        }

        Assert.That(result[0], Is.EqualTo(_folderId));
        Assert.That(result[1], Is.EqualTo("Image 1.jpg"));
        Assert.That(result[2], Is.EqualTo(Rotation.Rotate0));
        Assert.That(result[3], Is.EqualTo(1920));
        Assert.That(result[4], Is.EqualTo(1080));
        Assert.That(result[5], Is.EqualTo(120));
        Assert.That(result[6], Is.EqualTo(60));
        Assert.That(result[7], Is.EqualTo(new DateTime(2023, 08, 30, 12, 0, 0).ToString("M/dd/yyyy HH:mm:ss")));
        Assert.That(result[8], Is.EqualTo("4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4"));
        Assert.That(result[9], Is.EqualTo("The asset is corrupted"));
        Assert.That(result[10], Is.True);
        Assert.That(result[11], Is.Null);
        Assert.That(result[12], Is.False);
    }

    [Test]
    public void WriteFunc_AssetWithPartialValues_ConvertsAssetPropertiesToIndexInArrayWithSomeDefaultValues()
    {
        Asset asset = new()
        {
            FolderId = _folderId,
            Folder = new() { Id = _folderId, Path = "" },
            FileName = "toto.jpg",
            Pixel = new()
            {
                Asset = new(),
                Thumbnail = new()
            },
            ThumbnailCreationDateTime = new (2023, 08, 30, 12, 0, 0),
            Hash = "4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4",
            Metadata = new()
            {
                Corrupted = new() { Message = "The asset is corrupted" },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };

        object[] result = new object[13];
        for (int i = 0; i < 13; i++)
        {
            result[i] = AssetConfigs.WriteFunc(asset, i);
        }

        Assert.That(result[0], Is.EqualTo(_folderId));
        Assert.That(result[1], Is.EqualTo("toto.jpg"));
        Assert.That(result[2], Is.EqualTo(Rotation.Rotate0));
        Assert.That(result[3], Is.EqualTo(0));
        Assert.That(result[4], Is.EqualTo(0));
        Assert.That(result[5], Is.EqualTo(0));
        Assert.That(result[6], Is.EqualTo(0));
        Assert.That(result[7], Is.EqualTo(new DateTime(2023, 08, 30, 12, 0, 0).ToString("M/dd/yyyy HH:mm:ss")));
        Assert.That(result[8], Is.EqualTo("4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4"));
        Assert.That(result[9], Is.EqualTo("The asset is corrupted"));
        Assert.That(result[10], Is.False);
        Assert.That(result[11], Is.Null);
        Assert.That(result[12], Is.False);
    }

    [Test]
    public void WriteFunc_IndexOutOfRange_ThrowsArgumentOutOfRangeException()
    {
        Asset asset = new()
        {
            FolderId = _folderId,
            Folder = new() { Id = _folderId, Path = "" },
            FileName = "Image 1.jpg",
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = 1920, Height = 1080 },
                Thumbnail = new() { Width = 120, Height = 60 }
            },
            FileProperties = new() { Size = 1000 },
            ThumbnailCreationDateTime = new (2023, 08, 30, 12, 0, 0),
            Hash = "4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4",
            Metadata = new()
            {
                Corrupted = new() { IsTrue = true, Message = "The asset is corrupted" },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };

        ArgumentOutOfRangeException? exception = Assert.Throws<ArgumentOutOfRangeException>(() => AssetConfigs.WriteFunc(asset, 15));

        Assert.That(exception?.Message, Is.EqualTo("Specified argument was out of the range of valid values. (Parameter 'i')"));
    }

    [Test]
    public void WriteFunc_AssetIsNull_ThrowsNullReferenceException()
    {
        Asset? asset = null;

        for (int i = 0; i < 13; i++)
        {
            NullReferenceException? exception = Assert.Throws<NullReferenceException>(() => AssetConfigs.WriteFunc(asset!, i));

            Assert.That(exception?.Message, Is.EqualTo("Object reference not set to an instance of an object."));
        }
    }
}
