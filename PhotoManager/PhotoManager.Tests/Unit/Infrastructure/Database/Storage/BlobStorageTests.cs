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

        Assert.IsNotNull(deserializedObject);
        Assert.IsInstanceOf<Dictionary<string, byte[]>?>(deserializedObject);
        Assert.AreEqual(countExpected, deserializedObject!.Count);
        Assert.IsTrue(deserializedObject.ContainsKey(keyContained));
    }

    [Test]
    public void ReadFromBinaryFile_FileDoesNotExist_ReturnsNull()
    {
        string blobFilePath = Path.Combine(_dataDirectory!, "TestBackup\\v1.0\\Blobs\\eacd6c1b-d432-4424-9498-8c4c8d8940dd.bin");

        Dictionary<string, byte[]>? result = _blobStorage!.ReadFromBinaryFile(blobFilePath);

        Assert.IsNull(result);
    }

    [Test]
    public void ReadFromBinaryFile_FilePathIsInvalid_ReturnsNull()
    {
        Dictionary<string, byte[]>? result = _blobStorage!.ReadFromBinaryFile(_dataDirectory!);

        Assert.IsNull(result);
    }

    [Test]
    public void ReadFromBinaryFile_FilePathIsNull_ReturnsNull()
    {
        string? blobFilePath = null;

        Dictionary<string, byte[]>? result = _blobStorage!.ReadFromBinaryFile(blobFilePath!);

        Assert.IsNull(result);
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

            Assert.IsTrue(File.Exists(binaryFilePath));

            Dictionary<string, byte[]>? dataRead = _blobStorage!.ReadFromBinaryFile(binaryFilePath);
            Assert.IsNotNull(dataRead);
            Assert.AreEqual(data.Count, dataRead!.Count);
            CollectionAssert.AreEqual(data["Image1.jpg"], dataRead["Image1.jpg"]);
            CollectionAssert.AreEqual(data["Image2.png"], dataRead["Image2.png"]);
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

            Assert.AreEqual("Object reference not set to an instance of an object.", exception?.Message);
            Assert.IsTrue(File.Exists(binaryFilePath));
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

            Assert.IsTrue(File.Exists(binaryFilePath));

            Dictionary<string, byte[]>? dataRead = _blobStorage!.ReadFromBinaryFile(binaryFilePath);
            Assert.IsNotNull(dataRead);
            Assert.AreEqual(data.Count, dataRead!.Count);
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

        Assert.AreEqual($"Access to the path '{_dataDirectory!}' is denied.", exception?.Message);
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

        Assert.AreEqual("Value cannot be null. (Parameter 'path')", exception?.Message);
    }
}
