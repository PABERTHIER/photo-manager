using ImageMagick;
using Directories = PhotoManager.Tests.Unit.Constants.Directories;
using FileNames = PhotoManager.Tests.Unit.Constants.FileNames;
using Hashes = PhotoManager.Tests.Unit.Constants.Hashes;
using DHashes = PhotoManager.Tests.Unit.Constants.DHashes;
using MD5Hashes = PhotoManager.Tests.Unit.Constants.MD5Hashes;
using PHashes = PhotoManager.Tests.Unit.Constants.PHashes;

namespace PhotoManager.Tests.Unit.Common;

[TestFixture]
public class HashingHelperTests
{
    private string? _dataDirectory;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
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
    }

    [Test]
    public void CalculateHash_EmptyImageBytes_ReturnsSameHash()
    {
        byte[] imageBytes = [];

        string hash = HashingHelper.CalculateHash(imageBytes);

        Assert.That(string.IsNullOrWhiteSpace(hash), Is.False);
        Assert.That(hash, Has.Length.EqualTo(Hashes.LENGTH));
        Assert.That(hash.ToLower(), Is.EqualTo(Hashes.EMPTY_IMAGE));
    }

    [Test]
    public void CalculateHash_NullImageBytes_ThrowsArgumentNullException()
    {
        byte[]? imageBytes = null;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => HashingHelper.CalculateHash(imageBytes!));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'source')"));
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

        string? phash = HashingHelper.CalculatePHash(filePath);

        Assert.That(string.IsNullOrWhiteSpace(phash), Is.False);
        Assert.That(phash?.Length, Is.EqualTo(PHashes.LENGTH));
        Assert.That(phash?.ToLower(), Is.EqualTo(expectedHash));
    }

    [Test]
    public void CalculatePHash_ImageDoesNotExist_ThrowsMagickBlobErrorException()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.NON_EXISTENT_IMAGE_PNG);

        MagickBlobErrorException? exception = Assert.Throws<MagickBlobErrorException>(() => HashingHelper.CalculatePHash(filePath));

        Assert.That(exception?.Message, Does.StartWith($"unable to open image '{filePath}': No such file or directory @ error/blob.c/OpenBlob/"));
    }

    [Test]
    public void CalculatePHash_ImagePathIsInvalid_ThrowsMagickMissingDelegateErrorException()
    {
        MagickMissingDelegateErrorException? exception = Assert.Throws<MagickMissingDelegateErrorException>(() => HashingHelper.CalculatePHash(_dataDirectory!));

        Assert.That(exception?.Message, Does.StartWith($"no decode delegate for this image format `{_dataDirectory!}' @ error/constitute.c/ReadImage/"));
    }

    [Test]
    public void CalculatePHash_ImagePathIsNull_ThrowsArgumentNullException()
    {
        string? filePath = null;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => HashingHelper.CalculatePHash(filePath!));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null or empty. (Parameter 'fileName')"));
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
    }

    [Test]
    public void CalculateDHash_ImageDoesNotExist_ThrowsArgumentException()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.NON_EXISTENT_IMAGE_PNG);

        ArgumentException? exception = Assert.Throws<ArgumentException>(() => HashingHelper.CalculateDHash(filePath));

        Assert.That(exception?.Message, Is.EqualTo("Parameter is not valid."));
    }

    [Test]
    public void CalculateDHash_ImagePathIsInvalid_ThrowsArgumentException()
    {
        ArgumentException? exception = Assert.Throws<ArgumentException>(() => HashingHelper.CalculateDHash(_dataDirectory!));

        Assert.That(exception?.Message, Is.EqualTo("Parameter is not valid."));
    }

    [Test]
    public void CalculateDHash_ImagePathIsNull_ThrowsArgumentNullException()
    {
        string? filePath = null;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => HashingHelper.CalculateDHash(filePath!));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'path')"));
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
    }

    [Test]
    public void CalculateMD5Hash_EmptyImageBytes_ReturnsSameMD5Hash()
    {
        byte[] imageBytes = [];

        string hash = HashingHelper.CalculateMD5Hash(imageBytes);

        Assert.That(string.IsNullOrWhiteSpace(hash), Is.False);
        Assert.That(hash, Has.Length.EqualTo(MD5Hashes.LENGTH));
        Assert.That(hash.ToLower(), Is.EqualTo(MD5Hashes.EMPTY_IMAGE));
    }

    [Test]
    public void CalculateMD5Hash_NullImageBytes_ThrowsArgumentNullException()
    {
        byte[]? imageBytes = null;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => HashingHelper.CalculateMD5Hash(imageBytes!));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'source')"));
    }

    [Test]
    [TestCase(Hashes.IMAGE_1_JPG)] // SHA512
    [TestCase(PHashes.IMAGE_1_JPG)] // PHash
    [TestCase(DHashes.IMAGE_1_JPG)] // DHash
    [TestCase(MD5Hashes.IMAGE_1_JPG)] // MD5Hash
    public void CalculateHammingDistance_SameHashes_ReturnsZero(string hash)
    {
        int distance = HashingHelper.CalculateHammingDistance(hash, hash);

        Assert.That(distance, Is.EqualTo(0));
    }

    [Test]
    [TestCase(Hashes.IMAGE_1_JPG, Hashes.IMAGE_1_90_DEG_JPG, 118)] // SHA512
    [TestCase(PHashes.IMAGE_1_JPG, PHashes.IMAGE_10_PORTRAIT_PNG, 21)] // PHash
    [TestCase(DHashes.IMAGE_1_JPG, DHashes.IMAGE_10_PORTRAIT_PNG, 14)] // DHash
    [TestCase(MD5Hashes.IMAGE_1_JPG, MD5Hashes.IMAGE_1_90_DEG_JPG, 32)] // MD5Hash
    public void CalculateHammingDistance_DifferentHashes_ReturnsCorrectDistance(string hash1, string hash2, int expectedDistance)
    {
        int distance = HashingHelper.CalculateHammingDistance(hash1, hash2);

        Assert.That(distance, Is.GreaterThanOrEqualTo(0));
        Assert.That(distance, Is.EqualTo(expectedDistance));
    }

    [Test]
    [TestCase(DHashes.IMAGE_1_JPG, MD5Hashes.IMAGE_1_JPG)]
    [TestCase(MD5Hashes.IMAGE_1_JPG, DHashes.IMAGE_1_JPG)]
    [TestCase(DHashes.IMAGE_1_JPG, null)]
    [TestCase(null, DHashes.IMAGE_1_JPG)]
    [TestCase(null, null)]
    public void CalculateHammingDistance_IncorrectHashes_ThrowsArgumentException(string? hash1, string? hash2)
    {
        ArgumentException? exception = Assert.Throws<ArgumentException>(() => HashingHelper.CalculateHammingDistance(hash1!, hash2!));

        Assert.That(exception?.Message, Is.EqualTo("Invalid arguments for hamming distance calculation."));
    }
}
