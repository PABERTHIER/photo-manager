using ImageMagick;
using DHashes = PhotoManager.Tests.Integration.Constants.DHashes;
using Directories = PhotoManager.Tests.Integration.Constants.Directories;
using FileNames = PhotoManager.Tests.Integration.Constants.FileNames;
using Hashes = PhotoManager.Tests.Integration.Constants.Hashes;
using MD5Hashes = PhotoManager.Tests.Integration.Constants.MD5Hashes;
using PHashes = PhotoManager.Tests.Integration.Constants.PHashes;

namespace PhotoManager.Tests.Integration.Infrastructure;

[TestFixture]
public class AssetHashCalculatorServiceTests
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
    [TestCase(FileNames.IMAGE_11_HEIC, Hashes.IMAGE_11_HEIC)]
    [TestCase(FileNames.IMAGE_11_90_DEG_HEIC, Hashes.IMAGE_11_90_DEG_HEIC)]
    [TestCase(FileNames.IMAGE_11_180_DEG_HEIC, Hashes.IMAGE_11_180_DEG_HEIC)]
    [TestCase(FileNames.IMAGE_11_270_DEG_HEIC, Hashes.IMAGE_11_270_DEG_HEIC)]
    public void CalculateHash_DefaultHashAndValidImageBytesAndFilePath_ReturnsCorrectHash(string fileName, string expectedHash)
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, false, false);
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService);

        string filePath = Path.Combine(_dataDirectory!, fileName);
        byte[] imageBytes = File.ReadAllBytes(filePath);

        string hash = assetHashCalculatorService.CalculateHash(imageBytes, filePath);

        Assert.That(string.IsNullOrWhiteSpace(hash), Is.False);
        Assert.That(hash, Has.Length.EqualTo(Hashes.LENGTH));
        Assert.That(hash.ToLower(), Is.EqualTo(expectedHash));
    }

    [Test]
    public void CalculateHash_DefaultHashAndEmptyImageBytes_ReturnsSameHash()
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, false, false);
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService);

        byte[] imageBytes = [];
        string? filePath = null;
        const string expectedHash = Hashes.EMPTY_IMAGE;

        string hash = assetHashCalculatorService.CalculateHash(imageBytes, filePath!);

        Assert.That(string.IsNullOrWhiteSpace(hash), Is.False);
        Assert.That(hash, Has.Length.EqualTo(Hashes.LENGTH));
        Assert.That(hash.ToLower(), Is.EqualTo(expectedHash));
    }

    [Test]
    public void CalculateHash_DefaultHashAndNullImageBytes_ThrowsArgumentNullException()
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, false, false);
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService);

        byte[]? imageBytes = null;
        string? filePath = null;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => assetHashCalculatorService.CalculateHash(imageBytes!, filePath!));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'source')"));
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
    public void CalculateHash_DefaultHashAndValidImageBytesAndNullFilePath_ReturnsCorrectHash(string fileName, string expectedHash)
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, false, false);
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService);

        string filePath = Path.Combine(_dataDirectory!, fileName);
        byte[] imageBytes = File.ReadAllBytes(filePath);

        string hash = assetHashCalculatorService.CalculateHash(imageBytes, null!);

        Assert.That(string.IsNullOrWhiteSpace(hash), Is.False);
        Assert.That(hash, Has.Length.EqualTo(Hashes.LENGTH));
        Assert.That(hash.ToLower(), Is.EqualTo(expectedHash));
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
    public void CalculateHash_DefaultHashAndValidImageBytesAndInvalidFilePath_ReturnsCorrectHash(string fileName, string expectedHash)
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, false, false);
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService);

        string filePath = Path.Combine(_dataDirectory!, fileName);
        byte[] imageBytes = File.ReadAllBytes(filePath);

        string hash = assetHashCalculatorService.CalculateHash(imageBytes, $"invalid_path/{FileNames.NON_EXISTENT_FILE_JPG}");

        Assert.That(string.IsNullOrWhiteSpace(hash), Is.False);
        Assert.That(hash, Has.Length.EqualTo(Hashes.LENGTH));
        Assert.That(hash.ToLower(), Is.EqualTo(expectedHash));
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
    public void CalculateHash_DefaultHashAndValidImageBytesAndDirectoryFilePath_ReturnsCorrectHash(string fileName, string expectedHash)
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, false, false);
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService);

        string filePath = Path.Combine(_dataDirectory!, fileName);
        byte[] imageBytes = File.ReadAllBytes(filePath);

        string hash = assetHashCalculatorService.CalculateHash(imageBytes, _dataDirectory!);

        Assert.That(string.IsNullOrWhiteSpace(hash), Is.False);
        Assert.That(hash, Has.Length.EqualTo(Hashes.LENGTH));
        Assert.That(hash.ToLower(), Is.EqualTo(expectedHash));
    }

    [Test]
    [TestCase(FileNames.IMAGE_1_JPG, PHashes.IMAGE_1_JPG)]
    [TestCase(FileNames.IMAGE_1_90_DEG_JPG, PHashes.IMAGE_1_90_DEG_JPG)]
    [TestCase(FileNames.IMAGE_1_180_DEG_JPG, PHashes.IMAGE_1_180_DEG_JPG)]
    [TestCase(FileNames.IMAGE_1_270_DEG_JPG, PHashes.IMAGE_1_270_DEG_JPG)]
    [TestCase(FileNames.IMAGE_8_JPEG, PHashes.IMAGE_8_JPEG)]
    [TestCase(FileNames.IMAGE_10_PORTRAIT_PNG, PHashes.IMAGE_10_PORTRAIT_PNG)]
    [TestCase(FileNames.HOMER_GIF, PHashes.HOMER_GIF)]
    [TestCase(FileNames.IMAGE_11_HEIC, PHashes.IMAGE_11_HEIC)]
    [TestCase(FileNames.IMAGE_11_90_DEG_HEIC, PHashes.IMAGE_11_90_DEG_HEIC)]
    [TestCase(FileNames.IMAGE_11_180_DEG_HEIC, PHashes.IMAGE_11_180_DEG_HEIC)]
    [TestCase(FileNames.IMAGE_11_270_DEG_HEIC, PHashes.IMAGE_11_270_DEG_HEIC)]
    public void CalculateHash_PHashAndValidImageBytesAndFilePath_ReturnsCorrectPHash(string fileName, string expectedHash)
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(true, false, false);
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService);

        string filePath = Path.Combine(_dataDirectory!, fileName);
        byte[] imageBytes = File.ReadAllBytes(filePath);

        string hash = assetHashCalculatorService.CalculateHash(imageBytes, filePath);

        Assert.That(string.IsNullOrWhiteSpace(hash), Is.False);
        Assert.That(hash, Has.Length.EqualTo(PHashes.LENGTH));
        Assert.That(hash.ToLower(), Is.EqualTo(expectedHash));
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
    public void CalculateHash_PHashAndEmptyImageBytesAndFilePath_ReturnsCorrectPHash(string fileName, string expectedHash)
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(true, false, false);
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService);

        byte[] imageBytes = [];
        string filePath = Path.Combine(_dataDirectory!, fileName);

        string hash = assetHashCalculatorService.CalculateHash(imageBytes, filePath);

        Assert.That(string.IsNullOrWhiteSpace(hash), Is.False);
        Assert.That(hash, Has.Length.EqualTo(PHashes.LENGTH));
        Assert.That(hash.ToLower(), Is.EqualTo(expectedHash));
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
    public void CalculateHash_PHashAndNullImageBytesAndFilePath_ReturnsCorrectPHash(string fileName, string expectedHash)
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(true, false, false);
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService);

        byte[]? imageBytes = null;
        string filePath = Path.Combine(_dataDirectory!, fileName);

        string hash = assetHashCalculatorService.CalculateHash(imageBytes!, filePath);

        Assert.That(string.IsNullOrWhiteSpace(hash), Is.False);
        Assert.That(hash, Has.Length.EqualTo(PHashes.LENGTH));
        Assert.That(hash.ToLower(), Is.EqualTo(expectedHash));
    }

    [Test]
    public void CalculateHash_PHashAndImageDoesNotExist_ThrowsMagickBlobErrorException()
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(true, false, false);
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService);

        byte[]? imageBytes = null;
        string filePath = Path.Combine(_dataDirectory!, FileNames.NON_EXISTENT_IMAGE_PNG);

        MagickBlobErrorException? exception = Assert.Throws<MagickBlobErrorException>(() => assetHashCalculatorService.CalculateHash(imageBytes!, filePath));

        Assert.That(exception?.Message, Does.StartWith($"unable to open image '{filePath}': No such file or directory @ error/blob.c/OpenBlob/"));
    }

    [Test]
    public void CalculateHash_PHashAndImagePathIsInvalid_ThrowsMagickMissingDelegateErrorException()
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(true, false, false);
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService);

        byte[]? imageBytes = null;

        MagickMissingDelegateErrorException? exception = Assert.Throws<MagickMissingDelegateErrorException>(() => assetHashCalculatorService.CalculateHash(imageBytes!, _dataDirectory!));

        Assert.That(exception?.Message, Does.StartWith($"no decode delegate for this image format `{_dataDirectory!}' @ error/constitute.c/ReadImage/"));
    }

    [Test]
    public void CalculateHash_PHashAndImagePathIsNull_ThrowsArgumentNullException()
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(true, false, false);
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService);

        byte[]? imageBytes = null;
        string? filePath = null;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => assetHashCalculatorService.CalculateHash(imageBytes!, filePath!));

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
    [TestCase(FileNames.IMAGE_11_180_DEG_HEIC, DHashes.IMAGE_11_180_DEG_HEIC)]
    public void CalculateHash_DHashAndValidImageBytesAndFilePath_ReturnsCorrectDHash(string fileName, string expectedHash)
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, true, false);
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService);

        string filePath = Path.Combine(_dataDirectory!, fileName);
        byte[] imageBytes = File.ReadAllBytes(filePath);

        string hash = assetHashCalculatorService.CalculateHash(imageBytes, filePath);

        Assert.That(string.IsNullOrWhiteSpace(hash), Is.False);
        Assert.That(hash, Has.Length.EqualTo(DHashes.LENGTH));
        Assert.That(hash, Is.EqualTo(expectedHash));
    }

    [Test]
    [TestCase(FileNames.HOMER_GIF, DHashes.HOMER_GIF)]
    [TestCase(FileNames.IMAGE_11_HEIC, DHashes.IMAGE_11_HEIC)]
    [TestCase(FileNames.IMAGE_11_270_DEG_HEIC, DHashes.IMAGE_11_270_DEG_HEIC)]
    public void CalculateHash_NonWorkingDHash_ReturnsCorrectDHash(string fileName, string expectedHash)
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, true, false);
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService);

        string filePath = Path.Combine(_dataDirectory!, fileName);
        byte[] imageBytes = File.ReadAllBytes(filePath);

        string hash = assetHashCalculatorService.CalculateHash(imageBytes, filePath);

        Assert.That(string.IsNullOrWhiteSpace(hash), Is.False);
        Assert.That(hash, Has.Length.EqualTo(DHashes.LENGTH));
        Assert.That(hash, Is.EqualTo(expectedHash));
    }

    [Test]
    [TestCase(FileNames.IMAGE_1_JPG, DHashes.IMAGE_1_JPG)]
    [TestCase(FileNames.IMAGE_1_90_DEG_JPG, DHashes.IMAGE_1_90_DEG_JPG)]
    [TestCase(FileNames.IMAGE_1_180_DEG_JPG, DHashes.IMAGE_1_180_DEG_JPG)]
    [TestCase(FileNames.IMAGE_1_270_DEG_JPG, DHashes.IMAGE_1_270_DEG_JPG)]
    [TestCase(FileNames.IMAGE_8_JPEG, DHashes.IMAGE_8_JPEG)]
    [TestCase(FileNames.IMAGE_10_PORTRAIT_PNG, DHashes.IMAGE_10_PORTRAIT_PNG)]
    [TestCase(FileNames.IMAGE_11_90_DEG_HEIC, DHashes.IMAGE_11_90_DEG_HEIC)]
    public void CalculateHash_DHashAndEmptyImageBytesAndFilePath_ReturnsCorrectDHash(string fileName, string expectedHash)
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, true, false);
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService);

        byte[] imageBytes = [];
        string filePath = Path.Combine(_dataDirectory!, fileName);

        string hash = assetHashCalculatorService.CalculateHash(imageBytes, filePath);

        Assert.That(string.IsNullOrWhiteSpace(hash), Is.False);
        Assert.That(hash, Has.Length.EqualTo(DHashes.LENGTH));
        Assert.That(hash, Is.EqualTo(expectedHash));
    }

    [Test]
    [TestCase(FileNames.HOMER_GIF, DHashes.HOMER_GIF)]
    [TestCase(FileNames.IMAGE_11_HEIC, DHashes.IMAGE_11_HEIC)]
    public void CalculateHash_DHashAndEmptyImageBytesAndNonWorkingDHash_ReturnsCorrectDHash(string fileName, string expectedHash)
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, true, false);
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService);

        byte[] imageBytes = [];
        string filePath = Path.Combine(_dataDirectory!, fileName);

        string hash = assetHashCalculatorService.CalculateHash(imageBytes, filePath);

        Assert.That(string.IsNullOrWhiteSpace(hash), Is.False);
        Assert.That(hash, Has.Length.EqualTo(DHashes.LENGTH));
        Assert.That(hash, Is.EqualTo(expectedHash));
    }

    [Test]
    [TestCase(FileNames.IMAGE_1_JPG, DHashes.IMAGE_1_JPG)]
    [TestCase(FileNames.IMAGE_1_90_DEG_JPG, DHashes.IMAGE_1_90_DEG_JPG)]
    [TestCase(FileNames.IMAGE_1_180_DEG_JPG, DHashes.IMAGE_1_180_DEG_JPG)]
    [TestCase(FileNames.IMAGE_1_270_DEG_JPG, DHashes.IMAGE_1_270_DEG_JPG)]
    [TestCase(FileNames.IMAGE_8_JPEG, DHashes.IMAGE_8_JPEG)]
    [TestCase(FileNames.IMAGE_10_PORTRAIT_PNG, DHashes.IMAGE_10_PORTRAIT_PNG)]
    [TestCase(FileNames.IMAGE_11_90_DEG_HEIC, DHashes.IMAGE_11_90_DEG_HEIC)]
    public void CalculateHash_DHashAndNullImageBytesAndFilePath_ReturnsCorrectDHash(string fileName, string expectedHash)
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, true, false);
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService);

        byte[]? imageBytes = null;
        string filePath = Path.Combine(_dataDirectory!, fileName);

        string hash = assetHashCalculatorService.CalculateHash(imageBytes!, filePath);

        Assert.That(string.IsNullOrWhiteSpace(hash), Is.False);
        Assert.That(hash, Has.Length.EqualTo(DHashes.LENGTH));
        Assert.That(hash, Is.EqualTo(expectedHash));
    }

    [Test]
    [TestCase(FileNames.HOMER_GIF, DHashes.HOMER_GIF)]
    [TestCase(FileNames.IMAGE_11_HEIC, DHashes.IMAGE_11_HEIC)]
    public void CalculateHash_DHashAndNullImageBytesAndNonWorkingDHash_ReturnsCorrectDHash(string fileName, string expectedHash)
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, true, false);
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService);

        byte[]? imageBytes = null;
        string filePath = Path.Combine(_dataDirectory!, fileName);

        string hash = assetHashCalculatorService.CalculateHash(imageBytes!, filePath);

        Assert.That(string.IsNullOrWhiteSpace(hash), Is.False);
        Assert.That(hash, Has.Length.EqualTo(DHashes.LENGTH));
        Assert.That(hash, Is.EqualTo(expectedHash));
    }

    [Test]
    public void CalculateHash_DHashAndImageDoesNotExist_ThrowsArgumentException()
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, true, false);
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService);

        byte[]? imageBytes = null;
        string filePath = Path.Combine(_dataDirectory!, FileNames.NON_EXISTENT_IMAGE_PNG);

        ArgumentException? exception = Assert.Throws<ArgumentException>(() => assetHashCalculatorService.CalculateHash(imageBytes!, filePath));

        Assert.That(exception?.Message, Is.EqualTo("Parameter is not valid."));
    }

    [Test]
    public void CalculateHash_DHashAndImagePathIsInvalid_ThrowsArgumentException()
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, true, false);
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService);

        byte[]? imageBytes = null;

        ArgumentException? exception = Assert.Throws<ArgumentException>(() => assetHashCalculatorService.CalculateHash(imageBytes!, _dataDirectory!));

        Assert.That(exception?.Message, Is.EqualTo("Parameter is not valid."));
    }

    [Test]
    public void CalculateHash_DHashAndImagePathIsNull_ThrowsArgumentNullException()
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, true, false);
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService);

        byte[]? imageBytes = null;
        string? filePath = null;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => assetHashCalculatorService.CalculateHash(imageBytes!, filePath!));

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
    [TestCase(FileNames.IMAGE_11_HEIC, MD5Hashes.IMAGE_11_HEIC)]
    [TestCase(FileNames.IMAGE_11_90_DEG_HEIC, MD5Hashes.IMAGE_11_90_DEG_HEIC)]
    [TestCase(FileNames.IMAGE_11_180_DEG_HEIC, MD5Hashes.IMAGE_11_180_DEG_HEIC)]
    [TestCase(FileNames.IMAGE_11_270_DEG_HEIC, MD5Hashes.IMAGE_11_270_DEG_HEIC)]
    public void CalculateMD5Hash_MD5HashAndValidImageBytesAndFilePath_ReturnsCorrectMD5Hash(string fileName, string expectedHash)
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, false, true);
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService);

        string filePath = Path.Combine(_dataDirectory!, fileName);
        byte[] imageBytes = File.ReadAllBytes(filePath);

        string hash = assetHashCalculatorService.CalculateHash(imageBytes, filePath);

        Assert.That(string.IsNullOrWhiteSpace(hash), Is.False);
        Assert.That(hash, Has.Length.EqualTo(MD5Hashes.LENGTH));
        Assert.That(hash, Is.EqualTo(expectedHash));
    }

    [Test]
    public void CalculateMD5Hash_MD5HashAndEmptyImageBytes_ReturnsSameMD5Hash()
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, false, true);
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService);

        byte[] imageBytes = [];
        string? filePath = null;
        const string expectedHash = MD5Hashes.EMPTY_IMAGE;

        string hash = assetHashCalculatorService.CalculateHash(imageBytes, filePath!);

        Assert.That(string.IsNullOrWhiteSpace(hash), Is.False);
        Assert.That(hash, Has.Length.EqualTo(MD5Hashes.LENGTH));
        Assert.That(hash.ToLower(), Is.EqualTo(expectedHash));
    }

    [Test]
    public void CalculateMD5Hash_MD5HashAndNullImageBytes_ThrowsArgumentNullException()
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, false, true);
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService);

        byte[]? imageBytes = null;
        string? filePath = null;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => assetHashCalculatorService.CalculateHash(imageBytes!, filePath!));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'source')"));
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
    public void CalculateMD5Hash_MD5HashAndValidImageBytesAndNullFilePath_ReturnsCorrectMD5Hash(string fileName, string expectedHash)
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, false, true);
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService);

        string filePath = Path.Combine(_dataDirectory!, fileName);
        byte[] imageBytes = File.ReadAllBytes(filePath);

        string hash = assetHashCalculatorService.CalculateHash(imageBytes, filePath);

        Assert.That(string.IsNullOrWhiteSpace(hash), Is.False);
        Assert.That(hash, Has.Length.EqualTo(MD5Hashes.LENGTH));
        Assert.That(hash, Is.EqualTo(expectedHash));
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
    public void CalculateMD5Hash_MD5HashAndValidImageBytesAndInvalidFilePath_ReturnsCorrectMD5Hash(string fileName, string expectedHash)
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, false, true);
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService);

        string filePath = Path.Combine(_dataDirectory!, fileName);
        byte[] imageBytes = File.ReadAllBytes(filePath);

        string hash = assetHashCalculatorService.CalculateHash(imageBytes, filePath);

        Assert.That(string.IsNullOrWhiteSpace(hash), Is.False);
        Assert.That(hash, Has.Length.EqualTo(MD5Hashes.LENGTH));
        Assert.That(hash, Is.EqualTo(expectedHash));
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
    public void CalculateMD5Hash_MD5HashAndValidImageBytesAndDirectoryFilePath_ReturnsCorrectMD5Hash(string fileName, string expectedHash)
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, false, true);
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService);

        string filePath = Path.Combine(_dataDirectory!, fileName);
        byte[] imageBytes = File.ReadAllBytes(filePath);

        string hash = assetHashCalculatorService.CalculateHash(imageBytes, filePath);

        Assert.That(string.IsNullOrWhiteSpace(hash), Is.False);
        Assert.That(hash, Has.Length.EqualTo(MD5Hashes.LENGTH));
        Assert.That(hash, Is.EqualTo(expectedHash));
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
    public void CalculateHash_AllHashesEnabledAndValidImageBytesAndFilePath_ReturnsCorrectPHash(string fileName, string expectedHash)
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(true, true, true);
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService);

        string filePath = Path.Combine(_dataDirectory!, fileName);
        byte[] imageBytes = File.ReadAllBytes(filePath);

        string hash = assetHashCalculatorService.CalculateHash(imageBytes, filePath);

        Assert.That(string.IsNullOrWhiteSpace(hash), Is.False);
        Assert.That(hash, Has.Length.EqualTo(PHashes.LENGTH));
        Assert.That(hash.ToLower(), Is.EqualTo(expectedHash));
    }

    [Test]
    [TestCase(FileNames.IMAGE_1_JPG, DHashes.IMAGE_1_JPG)]
    [TestCase(FileNames.IMAGE_1_90_DEG_JPG, DHashes.IMAGE_1_90_DEG_JPG)]
    [TestCase(FileNames.IMAGE_1_180_DEG_JPG, DHashes.IMAGE_1_180_DEG_JPG)]
    [TestCase(FileNames.IMAGE_1_270_DEG_JPG, DHashes.IMAGE_1_270_DEG_JPG)]
    [TestCase(FileNames.IMAGE_8_JPEG, DHashes.IMAGE_8_JPEG)]
    [TestCase(FileNames.IMAGE_10_PORTRAIT_PNG, DHashes.IMAGE_10_PORTRAIT_PNG)]
    public void CalculateHash_DHashAndMd5HashEnabledAndValidImageBytesAndFilePath_ReturnsCorrectDHash(string fileName, string expectedHash)
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, true, true);
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService);

        string filePath = Path.Combine(_dataDirectory!, fileName);
        byte[] imageBytes = File.ReadAllBytes(filePath);

        string hash = assetHashCalculatorService.CalculateHash(imageBytes, filePath);

        Assert.That(string.IsNullOrWhiteSpace(hash), Is.False);
        Assert.That(hash, Has.Length.EqualTo(DHashes.LENGTH));
        Assert.That(hash, Is.EqualTo(expectedHash));
    }

    private static UserConfigurationService GetUserConfigurationService(bool usingPHash, bool usingDHash, bool usingMd5Hash)
    {
        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock
            .MockGetValue(UserConfigurationKeys.USING_PHASH, usingPHash.ToString())
            .MockGetValue(UserConfigurationKeys.USING_DHASH, usingDHash.ToString())
            .MockGetValue(UserConfigurationKeys.USING_MD5_HASH, usingMd5Hash.ToString());

        return new(configurationRootMock.Object);
    }
}
