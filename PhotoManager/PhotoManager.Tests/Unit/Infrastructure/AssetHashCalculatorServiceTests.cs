namespace PhotoManager.Tests.Unit.Infrastructure;

[TestFixture]
public class AssetHashCalculatorServiceTests
{
    private string? dataDirectory;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
    }

    // The cases for imageBytes null, empty or invalid and filePath null or invalid are already handled in the HashingHelperTests
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
        var filePath = Path.Combine(dataDirectory!, fileName);
        byte[] imageBytes = File.ReadAllBytes(filePath);
        AssetHashCalculatorService assetHashCalculatorService = new();

        string hash = assetHashCalculatorService.CalculateHash(imageBytes, filePath);

        Assert.IsInstanceOf<string>(hash);
        Assert.False(string.IsNullOrWhiteSpace(hash));
    }
}
