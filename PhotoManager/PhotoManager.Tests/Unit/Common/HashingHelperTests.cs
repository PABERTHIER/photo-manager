using ImageMagick;
using DHashes = PhotoManager.Tests.Unit.Constants.DHashes;
using Directories = PhotoManager.Tests.Unit.Constants.Directories;
using FileNames = PhotoManager.Tests.Unit.Constants.FileNames;
using Hashes = PhotoManager.Tests.Unit.Constants.Hashes;
using MD5Hashes = PhotoManager.Tests.Unit.Constants.MD5Hashes;
using PHashes = PhotoManager.Tests.Unit.Constants.PHashes;

namespace PhotoManager.Tests.Unit.Common;

[TestFixture]
public class HashingHelperTests
{
    private string? _dataDirectory;

    private TestLogger<HashingHelperTests> _testLogger = new();

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
    }

    [SetUp]
    public void SetUp()
    {
        _testLogger = new TestLogger<HashingHelperTests>();
    }

    [TearDown]
    public void TearDown()
    {
        _testLogger.LoggingAssertTearDown();
    }

    [Test]
    [TestCase(FileNames.IMAGE_1_JPG, Hashes.IMAGE_1_JPG)]
    [TestCase(FileNames.IMAGE_1_90_DEG_JPG, Hashes.IMAGE_1_90_DEG_JPG)]
    [TestCase(FileNames.IMAGE_1_180_DEG_JPG, Hashes.IMAGE_1_180_DEG_JPG)]
    [TestCase(FileNames.IMAGE_1_270_DEG_JPG, Hashes.IMAGE_1_270_DEG_JPG)]
    [TestCase(FileNames.IMAGE_8_JPEG, Hashes.IMAGE_8_JPEG)]
    [TestCase(FileNames.IMAGE_10_PORTRAIT_PNG, Hashes.IMAGE_10_PORTRAIT_PNG)]
    [TestCase(FileNames.HOMER_GIF, Hashes.HOMER_GIF)]
    [TestCase(FileNames.IMAGE_11_90_DEG_HEIC, Hashes.IMAGE_11_90_DEG_HEIC)]
    public void CalculateHash_ValidImageBytes_ReturnsCorrectHash(string fileName, string expectedHash)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);
        byte[] imageBytes = File.ReadAllBytes(filePath);

        string hash = HashingHelper.CalculateHash(imageBytes);

        Assert.That(string.IsNullOrWhiteSpace(hash), Is.False);
        Assert.That(hash, Has.Length.EqualTo(Hashes.LENGTH));
        Assert.That(hash.ToLower(), Is.EqualTo(expectedHash));

        _testLogger.AssertLogExceptions([], typeof(HashingHelperTests));
    }

    [Test]
    public void CalculateHash_EmptyImageBytes_ReturnsDefaultHash()
    {
        byte[] imageBytes = [];

        string hash = HashingHelper.CalculateHash(imageBytes);

        Assert.That(string.IsNullOrWhiteSpace(hash), Is.False);
        Assert.That(hash, Has.Length.EqualTo(Hashes.LENGTH));
        Assert.That(hash.ToLower(), Is.EqualTo(Hashes.EMPTY_IMAGE));

        _testLogger.AssertLogExceptions([], typeof(HashingHelperTests));
    }

    [Test]
    public void CalculateHash_NullImageBytes_ReturnsDefaultHash()
    {
        byte[]? imageBytes = null;

        string hash = HashingHelper.CalculateHash(imageBytes!);

        Assert.That(string.IsNullOrWhiteSpace(hash), Is.False);
        Assert.That(hash, Has.Length.EqualTo(Hashes.LENGTH));
        Assert.That(hash.ToLower(), Is.EqualTo(Hashes.EMPTY_IMAGE));

        _testLogger.AssertLogExceptions([], typeof(HashingHelperTests));
    }

    [Test]
    [TestCase(FileNames.IMAGE_1_JPG, PHashes.IMAGE_1_JPG)]
    [TestCase(FileNames.IMAGE_1_90_DEG_JPG, PHashes.IMAGE_1_90_DEG_JPG)]
    [TestCase(FileNames.IMAGE_1_180_DEG_JPG, PHashes.IMAGE_1_180_DEG_JPG)]
    [TestCase(FileNames.IMAGE_1_270_DEG_JPG, PHashes.IMAGE_1_270_DEG_JPG)]
    [TestCase(FileNames.IMAGE_8_JPEG, PHashes.IMAGE_8_JPEG)]
    [TestCase(FileNames.IMAGE_10_PORTRAIT_PNG, PHashes.IMAGE_10_PORTRAIT_PNG)]
    [TestCase(FileNames.HOMER_GIF, PHashes.HOMER_GIF)]
    [TestCase(FileNames.IMAGE_11_90_DEG_HEIC, PHashes.IMAGE_11_90_DEG_HEIC)]
    public void CalculatePHash_ValidImagePath_ReturnsCorrectPHash(string fileName, string expectedHash)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);

        string? phash = HashingHelper.CalculatePHash(filePath, _testLogger);

        Assert.That(string.IsNullOrWhiteSpace(phash), Is.False);
        Assert.That(phash?.Length, Is.EqualTo(PHashes.LENGTH));
        Assert.That(phash?.ToLower(), Is.EqualTo(expectedHash));

        _testLogger.AssertLogExceptions([], typeof(HashingHelperTests));
    }

    [Test]
    public void CalculatePHash_ImageDoesNotExist_LogsItAndReturnsNull()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.NON_EXISTENT_IMAGE_PNG);

        string? phash = HashingHelper.CalculatePHash(filePath, _testLogger);

        Assert.That(phash, Is.Null);

        _testLogger.AssertLogExceptions(
            [new MagickBlobErrorException($"MagickImage is unable to open image {filePath}.")],
            typeof(HashingHelperTests));
    }

    [Test]
    public void CalculatePHash_ImagePathIsInvalid_ReturnsNull()
    {
        string? phash = HashingHelper.CalculatePHash(_dataDirectory!, _testLogger);

        Assert.That(phash, Is.Null);

        _testLogger.AssertLogExceptions(
            [new MagickBlobErrorException($"MagickImage is unable to open image {_dataDirectory}.")],
            typeof(HashingHelperTests));
    }

    [Test]
    public void CalculatePHash_ImagePathIsEmpty_LogsItAndReturnsNull()
    {
        const string filePath = "";

        string? phash = HashingHelper.CalculatePHash(filePath, _testLogger);

        Assert.That(phash, Is.Null);

        _testLogger.AssertLogExceptions(
        [
            new ArgumentException("Value cannot be null or empty. (Parameter 'fileName')")
        ], typeof(HashingHelperTests));
    }

    [Test]
    public void CalculatePHash_ImagePathIsNull_ReturnsNull()
    {
        string? filePath = null;

        string? phash = HashingHelper.CalculatePHash(filePath!, _testLogger);

        Assert.That(phash, Is.Null);

        _testLogger.AssertLogExceptions(
        [
            new ArgumentException("Value cannot be null or empty. (Parameter 'fileName')")
        ], typeof(HashingHelperTests));
    }

    [Test]
    [TestCase(FileNames.IMAGE_1_JPG, DHashes.IMAGE_1_JPG)]
    [TestCase(FileNames.IMAGE_1_90_DEG_JPG, DHashes.IMAGE_1_90_DEG_JPG)]
    [TestCase(FileNames.IMAGE_1_180_DEG_JPG, DHashes.IMAGE_1_180_DEG_JPG)]
    [TestCase(FileNames.IMAGE_1_270_DEG_JPG, DHashes.IMAGE_1_270_DEG_JPG)]
    [TestCase(FileNames.IMAGE_8_JPEG, DHashes.IMAGE_8_JPEG)]
    [TestCase(FileNames.IMAGE_10_PORTRAIT_PNG, DHashes.IMAGE_10_PORTRAIT_PNG)]
    [TestCase(FileNames.IMAGE_11_90_DEG_HEIC, DHashes.IMAGE_11_90_DEG_HEIC)]
    public void CalculateDHash_ValidImagePath_ReturnsCorrectDHash(string fileName, string expectedHash)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);

        string dHash = HashingHelper.CalculateDHash(filePath);

        Assert.That(string.IsNullOrWhiteSpace(dHash), Is.False);
        Assert.That(dHash, Has.Length.EqualTo(DHashes.LENGTH));
        Assert.That(dHash, Is.EqualTo(expectedHash));

        _testLogger.AssertLogExceptions([], typeof(HashingHelperTests));
    }

    [Test]
    [TestCase(FileNames.HOMER_GIF, DHashes.HOMER_GIF)]
    [TestCase(FileNames.IMAGE_11_HEIC, DHashes.IMAGE_11_HEIC)]
    public void CalculateDHash_NonWorkingDHash_ReturnsCorrectDHash(string fileName, string expectedHash)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);

        string dHash = HashingHelper.CalculateDHash(filePath);

        Assert.That(string.IsNullOrWhiteSpace(dHash), Is.False);
        Assert.That(dHash, Has.Length.EqualTo(DHashes.LENGTH));
        Assert.That(dHash, Is.EqualTo(expectedHash));

        _testLogger.AssertLogExceptions([], typeof(HashingHelperTests));
    }

    [Test]
    public void CalculateDHash_ImageDoesNotExist_ThrowsArgumentException()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.NON_EXISTENT_IMAGE_PNG);

        ArgumentException? exception = Assert.Throws<ArgumentException>(() => HashingHelper.CalculateDHash(filePath));

        Assert.That(exception?.Message, Is.EqualTo("Parameter is not valid."));

        _testLogger.AssertLogExceptions([], typeof(HashingHelperTests));
    }

    [Test]
    public void CalculateDHash_ImagePathIsInvalid_ThrowsArgumentException()
    {
        ArgumentException? exception =
            Assert.Throws<ArgumentException>(() => HashingHelper.CalculateDHash(_dataDirectory!));

        Assert.That(exception?.Message, Is.EqualTo("Parameter is not valid."));

        _testLogger.AssertLogExceptions([], typeof(HashingHelperTests));
    }

    [Test]
    public void CalculateDHash_ImagePathIsNull_ThrowsArgumentNullException()
    {
        string? filePath = null;

        ArgumentNullException? exception =
            Assert.Throws<ArgumentNullException>(() => HashingHelper.CalculateDHash(filePath!));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'path')"));

        _testLogger.AssertLogExceptions([], typeof(HashingHelperTests));
    }

    [Test]
    [TestCase(FileNames.IMAGE_1_JPG, MD5Hashes.IMAGE_1_JPG)]
    [TestCase(FileNames.IMAGE_1_90_DEG_JPG, MD5Hashes.IMAGE_1_90_DEG_JPG)]
    [TestCase(FileNames.IMAGE_1_180_DEG_JPG, MD5Hashes.IMAGE_1_180_DEG_JPG)]
    [TestCase(FileNames.IMAGE_1_270_DEG_JPG, MD5Hashes.IMAGE_1_270_DEG_JPG)]
    [TestCase(FileNames.IMAGE_8_JPEG, MD5Hashes.IMAGE_8_JPEG)]
    [TestCase(FileNames.IMAGE_10_PORTRAIT_PNG, MD5Hashes.IMAGE_10_PORTRAIT_PNG)]
    [TestCase(FileNames.HOMER_GIF, MD5Hashes.HOMER_GIF)]
    [TestCase(FileNames.IMAGE_11_90_DEG_HEIC, MD5Hashes.IMAGE_11_90_DEG_HEIC)]
    public void CalculateMD5Hash_ValidImageBytes_ReturnsCorrectMD5Hash(string fileName, string expectedHash)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);
        byte[] imageBytes = File.ReadAllBytes(filePath);

        string md5Hash = HashingHelper.CalculateMD5Hash(imageBytes);

        Assert.That(string.IsNullOrWhiteSpace(md5Hash), Is.False);
        Assert.That(md5Hash, Has.Length.EqualTo(MD5Hashes.LENGTH));
        Assert.That(md5Hash, Is.EqualTo(expectedHash));

        _testLogger.AssertLogExceptions([], typeof(HashingHelperTests));
    }

    [Test]
    public void CalculateMD5Hash_EmptyImageBytes_ReturnsSameMD5Hash()
    {
        byte[] imageBytes = [];

        string hash = HashingHelper.CalculateMD5Hash(imageBytes);

        Assert.That(string.IsNullOrWhiteSpace(hash), Is.False);
        Assert.That(hash, Has.Length.EqualTo(MD5Hashes.LENGTH));
        Assert.That(hash.ToLower(), Is.EqualTo(MD5Hashes.EMPTY_IMAGE));

        _testLogger.AssertLogExceptions([], typeof(HashingHelperTests));
    }

    [Test]
    public void CalculateMD5Hash_NullImageBytes_ThrowsArgumentNullException()
    {
        byte[]? imageBytes = null;

        ArgumentNullException? exception =
            Assert.Throws<ArgumentNullException>(() => HashingHelper.CalculateMD5Hash(imageBytes!));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'source')"));

        _testLogger.AssertLogExceptions([], typeof(HashingHelperTests));
    }

    [Test]
    [TestCase(Hashes.IMAGE_1_JPG)] // SHA512
    [TestCase(PHashes.IMAGE_1_JPG)] // PHash
    [TestCase(DHashes.IMAGE_1_JPG)] // DHash
    [TestCase(MD5Hashes.IMAGE_1_JPG)] // MD5Hash
    public void CalculateHammingDistance_SameHashes_ReturnsZero(string hash)
    {
        int distance = HashingHelper.CalculateHammingDistance(hash, hash, _testLogger);

        Assert.That(distance, Is.Zero);

        _testLogger.AssertLogExceptions([], typeof(HashingHelperTests));
    }

    [Test]
    [TestCase(Hashes.IMAGE_1_JPG, Hashes.IMAGE_1_90_DEG_JPG, 118)] // SHA512
    [TestCase(PHashes.IMAGE_1_JPG, PHashes.IMAGE_10_PORTRAIT_PNG, 21)] // PHash
    [TestCase(DHashes.IMAGE_1_JPG, DHashes.IMAGE_10_PORTRAIT_PNG, 14)] // DHash
    [TestCase(MD5Hashes.IMAGE_1_JPG, MD5Hashes.IMAGE_1_90_DEG_JPG, 32)] // MD5Hash
    public void CalculateHammingDistance_DifferentHashes_ReturnsCorrectDistance(string hash1, string hash2,
        int expectedDistance)
    {
        int distance = HashingHelper.CalculateHammingDistance(hash1, hash2, _testLogger);

        Assert.That(distance, Is.GreaterThanOrEqualTo(0));
        Assert.That(distance, Is.EqualTo(expectedDistance));

        _testLogger.AssertLogExceptions([], typeof(HashingHelperTests));
    }

    [Test]
    [TestCase(DHashes.IMAGE_1_JPG, MD5Hashes.IMAGE_1_JPG)]
    [TestCase(MD5Hashes.IMAGE_1_JPG, DHashes.IMAGE_1_JPG)]
    public void CalculateHammingDistance_IncorrectHashes_LogsItAndThrowsArgumentException(string? hash1, string? hash2)
    {
        Exception expectedException = new(
            $"Invalid arguments for hamming distance calculation. hash1: {hash1}, hash2: {hash2}");
        Exception[] expectedExceptions = [expectedException];

        ArgumentException? exception =
            Assert.Throws<ArgumentException>(() => HashingHelper.CalculateHammingDistance(hash1!, hash2!, _testLogger));

        Assert.That(exception?.Message, Is.EqualTo(expectedException.Message));

        _testLogger.AssertLogExceptions(expectedExceptions, typeof(HashingHelperTests));
    }

    [Test]
    [TestCase(DHashes.IMAGE_1_JPG, null)]
    [TestCase(null, DHashes.IMAGE_1_JPG)]
    [TestCase(null, null)]
    public void CalculateHammingDistance_HashesAreNull_ThrowsNullReferenceException(string? hash1, string? hash2)
    {
        NullReferenceException? exception = Assert.Throws<NullReferenceException>(() =>
            HashingHelper.CalculateHammingDistance(hash1!, hash2!, _testLogger));

        Assert.That(exception?.Message, Is.EqualTo("Object reference not set to an instance of an object."));

        _testLogger.AssertLogExceptions([], typeof(HashingHelperTests));
    }
}
