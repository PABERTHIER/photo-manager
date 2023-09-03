namespace PhotoManager.Tests.Unit.Infrastructure.Database.Storage;

[TestFixture]
public class BlobStorageTests
{
    private string? dataDirectory;
    private BlobStorage? _blobStorage;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
    }

    [SetUp]
    public void Setup()
    {
        _blobStorage = new();
    }

    [Test]
    [TestCase("18033543-defb-4d37-837b-d8063eda3a25.bin", 4, "Image_12.heic")] // The blobName is always like this FolderId + ".bin"
    [TestCase("f1f00403-0554-4201-9b6b-11a6b4cea3a9.bin", 7, "1336.JPG")]
    public void ReadFromBinaryFile_FileExists_ReturnsDeserializedObject(string blobFileName, int countExpected, string keyContained)
    {
        string blobFilePath = Path.Combine(dataDirectory!, "TestBackup\\v1.0", AssetConstants.Blobs, blobFileName);

        var deserializedObject = _blobStorage!.ReadFromBinaryFile(blobFilePath);

        Assert.IsNotNull(deserializedObject);
        Assert.IsInstanceOf<Dictionary<string, byte[]>?>(deserializedObject);
        Assert.AreEqual(deserializedObject!.Count, countExpected);
        Assert.IsTrue(deserializedObject.ContainsKey(keyContained));
    }

    [Test]
    public void ReadFromBinaryFile_FileDoesNotExist_ReturnsNull()
    {
        string blobFilePath = Path.Combine(dataDirectory!, "TestBackup\\v1.0\\Blobs\\eacd6c1b-d432-4424-9498-8c4c8d8940dd.bin");

        var result = _blobStorage!.ReadFromBinaryFile(blobFilePath);

        Assert.IsNull(result);
    }

    [Test]
    public void WriteToBinaryFile_DataIsCorrect_WritesDataToFile()
    {
        string binaryFilePath = Path.Combine(dataDirectory!, "abcd6c1b-d432-4424-9498-8c4c8d8940dd.bin");

        try
        {
            Dictionary<string, byte[]> data = new()
            {
                { "Image1.jpg", new byte[] { 1, 2, 3 } },
                { "Image2.png", new byte[] { 4, 5, 6 } }
            };

            _blobStorage!.WriteToBinaryFile(data, binaryFilePath);

            Assert.IsTrue(File.Exists(binaryFilePath));

            Dictionary<string, byte[]>? readData = _blobStorage!.ReadFromBinaryFile(binaryFilePath);
            Assert.IsNotNull(readData);
            Assert.AreEqual(data.Count, readData!.Count);
            CollectionAssert.AreEqual(data["Image1.jpg"], readData["Image1.jpg"]);
            CollectionAssert.AreEqual(data["Image2.png"], readData["Image2.png"]);
        }
        finally
        {
            File.Delete(binaryFilePath);
        }
    }

    [Test]
    public void WriteToBinaryFile_DataIsNull_ThrowsNullReferenceException()
    {
        string binaryFilePath = Path.Combine(dataDirectory!, "bbcd6c1b-d432-4424-9498-8c4c8d8940dd.bin");

        try
        {
            Dictionary<string, byte[]>? data = null;

            Assert.Throws<NullReferenceException>(() => _blobStorage!.WriteToBinaryFile(data!, binaryFilePath));
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
        string binaryFilePath = Path.Combine(dataDirectory!, "cbcd6c1b-d432-4424-9498-8c4c8d8940dd.bin");

        try
        {
            Dictionary<string, byte[]> data = new();

            _blobStorage!.WriteToBinaryFile(data, binaryFilePath);

            Assert.IsTrue(File.Exists(binaryFilePath));

            Dictionary<string, byte[]>? readData = _blobStorage!.ReadFromBinaryFile(binaryFilePath);
            Assert.IsNotNull(readData);
            Assert.AreEqual(data.Count, readData!.Count);
        }
        finally
        {
            File.Delete(binaryFilePath);
        }
    }

    [Test]
    public void WriteToBinaryFile_PathIsNull_ThrowsArgumentNullException()
    {
        string? binaryFilePath = null;

        Dictionary<string, byte[]> data = new()
        {
            { "Image1.jpg", new byte[] { 1, 2, 3 } },
            { "Image2.png", new byte[] { 4, 5, 6 } }
        };

        Assert.Throws<ArgumentNullException>(() => _blobStorage!.WriteToBinaryFile(data, binaryFilePath!));
    }
}
