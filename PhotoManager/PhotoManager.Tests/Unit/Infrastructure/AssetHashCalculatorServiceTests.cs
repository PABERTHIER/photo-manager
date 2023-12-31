namespace PhotoManager.Tests.Unit.Infrastructure;

[TestFixture]
public class AssetHashCalculatorServiceTests
{
    private string? dataDirectory;

    private UserConfigurationService? _userConfigurationService;
    private AssetHashCalculatorService? _assetHashCalculatorService;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");

        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();

        _userConfigurationService = new(configurationRootMock.Object);
    }

    [SetUp]
    public void Setup()
    {
        _assetHashCalculatorService = new (_userConfigurationService!);
    }

    [Test]
    [TestCase("Image 1.jpg")]
    [TestCase("Image 1_90_deg.jpg")]
    [TestCase("Image 1_180_deg.jpg")]
    [TestCase("Image 1_270_deg.jpg")]
    [TestCase("Image 8.jpeg")]
    [TestCase("Image 10 portrait.png")]
    [TestCase("Homer.gif")]
    [TestCase("Image_11.heic")]
    public void CalculateHash_ValidImageBytesAndFilePath_ReturnsHash(string fileName)
    {
        string filePath = Path.Combine(dataDirectory!, fileName);
        byte[] imageBytes = File.ReadAllBytes(filePath);

        string hash = _assetHashCalculatorService!.CalculateHash(imageBytes, filePath);

        Assert.IsInstanceOf<string>(hash);
        Assert.IsFalse(string.IsNullOrWhiteSpace(hash));
    }

    // TODO: Add the cases for imageBytes null, empty or invalid and filePath null or invalid... when AssetHashCalculatorService would have been reworked
}
