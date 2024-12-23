namespace PhotoManager.Tests.Unit.Infrastructure.Database.Storage;

[TestFixture]
public class BlobStorageTests
{
    private string? _dataDirectory;

    private BlobStorage? _blobStorage;
    private UserConfigurationService? _userConfigurationService;
    private Mock<IConfigurationRoot>? _configurationRootMock;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");

        _configurationRootMock = new Mock<IConfigurationRoot>();
        _configurationRootMock.GetDefaultMockConfig();

        _userConfigurationService = new (_configurationRootMock!.Object);
    }

    [SetUp]
    public void SetUp()
    {
        _blobStorage = new();
    }

    [Test]
    [TestCase("18033543-defb-4d37-837b-d8063eda3a25.bin", 4, "Image_12.heic")] // The blobName is always like this: Folder.Id + ".bin"
    [TestCase("f1f00403-0554-4201-9b6b-11a6b4cea3a9.bin", 7, "1336.JPG")]
    public void ReadFromBinaryFile_FileExists_ReturnsDeserializedObject(string blobFileName, int countExpected, string keyContained)
    {
        string blobFilePath = Path.Combine(_dataDirectory!, "TestBackup\\v1.0", _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, blobFileName);

        Dictionary<string, byte[]>? deserializedObject = _blobStorage!.ReadFromBinaryFile(blobFilePath);

        Assert.That(deserializedObject, Is.Not.Null);
        Assert.That(deserializedObject, Is.InstanceOf<Dictionary<string, byte[]>?>());
        Assert.That(deserializedObject!, Has.Count.EqualTo(countExpected));
        Assert.That(deserializedObject.ContainsKey(keyContained), Is.True);
    }

    [Test]
    public void ReadFromBinaryFile_FileDoesNotExist_ReturnsNull()
    {
        string blobFilePath = Path.Combine(_dataDirectory!, "TestBackup\\v1.0\\Blobs\\eacd6c1b-d432-4424-9498-8c4c8d8940dd.bin");

        Dictionary<string, byte[]>? result = _blobStorage!.ReadFromBinaryFile(blobFilePath);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void ReadFromBinaryFile_FilePathIsInvalid_ReturnsNull()
    {
        Dictionary<string, byte[]>? result = _blobStorage!.ReadFromBinaryFile(_dataDirectory!);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void ReadFromBinaryFile_FilePathIsNull_ReturnsNull()
    {
        string? blobFilePath = null;

        Dictionary<string, byte[]>? result = _blobStorage!.ReadFromBinaryFile(blobFilePath!);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void WriteToBinaryFile_DataIsCorrect_WritesDataToFile()
    {
        string binaryFilePath = Path.Combine(_dataDirectory!, "abcd6c1b-d432-4424-9498-8c4c8d8940dd.bin");

        try
        {
            Dictionary<string, byte[]> data = new()
            {
                { "Image1.jpg", [1, 2, 3]},
                { "Image2.png", [4, 5, 6]}
            };

            _blobStorage!.WriteToBinaryFile(data, binaryFilePath);

            Assert.That(File.Exists(binaryFilePath), Is.True);

            Dictionary<string, byte[]>? dataRead = _blobStorage!.ReadFromBinaryFile(binaryFilePath);
            Assert.That(dataRead, Is.Not.Null);
            Assert.That(dataRead!, Has.Count.EqualTo(data.Count));
            Assert.That(dataRead["Image1.jpg"], Is.EqualTo(data["Image1.jpg"]).AsCollection);
            Assert.That(dataRead["Image2.png"], Is.EqualTo(data["Image2.png"]).AsCollection);
        }
        finally
        {
            File.Delete(binaryFilePath);
        }
    }

    [Test]
    public void WriteToBinaryFile_DataIsNull_ThrowsNullReferenceException()
    {
        string binaryFilePath = Path.Combine(_dataDirectory!, "bbcd6c1b-d432-4424-9498-8c4c8d8940dd.bin");

        try
        {
            Dictionary<string, byte[]>? data = null;

            NullReferenceException? exception = Assert.Throws<NullReferenceException>(() => _blobStorage!.WriteToBinaryFile(data!, binaryFilePath));

            Assert.That(exception?.Message, Is.EqualTo("Object reference not set to an instance of an object."));
            Assert.That(File.Exists(binaryFilePath), Is.True);
        }
        finally
        {
            File.Delete(binaryFilePath);
        }
    }

    [Test]
    public void WriteToBinaryFile_DataIsEmpty_WritesNothingInTheFile()
    {
        string binaryFilePath = Path.Combine(_dataDirectory!, "cbcd6c1b-d432-4424-9498-8c4c8d8940dd.bin");

        try
        {
            Dictionary<string, byte[]> data = [];

            _blobStorage!.WriteToBinaryFile(data, binaryFilePath);

            Assert.That(File.Exists(binaryFilePath), Is.True);

            Dictionary<string, byte[]>? dataRead = _blobStorage!.ReadFromBinaryFile(binaryFilePath);
            Assert.That(dataRead, Is.Not.Null);
            Assert.That(dataRead!, Has.Count.EqualTo(data.Count));
        }
        finally
        {
            File.Delete(binaryFilePath);
        }
    }

    [Test]
    public void WriteToBinaryFile_FilePathIsInvalid_ThrowsUnauthorizedAccessException()
    {
        Dictionary<string, byte[]> data = new()
        {
            { "Image1.jpg", [1, 2, 3]},
            { "Image2.png", [4, 5, 6]}
        };

        UnauthorizedAccessException? exception = Assert.Throws<UnauthorizedAccessException>(() => _blobStorage!.WriteToBinaryFile(data, _dataDirectory!));

        Assert.That(exception?.Message, Is.EqualTo($"Access to the path '{_dataDirectory!}' is denied."));
    }

    [Test]
    public void WriteToBinaryFile_FilePathIsNull_ThrowsArgumentNullException()
    {
        string? binaryFilePath = null;

        Dictionary<string, byte[]> data = new()
        {
            { "Image1.jpg", [1, 2, 3]},
            { "Image2.png", [4, 5, 6]}
        };

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _blobStorage!.WriteToBinaryFile(data, binaryFilePath!));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'path')"));
    }
}
