using Microsoft.Extensions.Configuration;
using PhotoManager.UI;

namespace PhotoManager.Tests.Unit.Infrastructure;

[TestFixture]
public class UserConfigurationServiceTests
{
    private string? dataDirectory;
    private IUserConfigurationService? _userConfigurationService;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");

        Mock<IConfigurationRoot> configurationMock = new();
        configurationMock
            .MockGetValue("appsettings:CatalogBatchSize", "100")
            .MockGetValue("appsettings:CatalogCooldownMinutes", "5")
            .MockGetValue("appsettings:BackupsToKeep", "2")
            .MockGetValue("appsettings:ThumbnailsDictionaryEntriesToKeep", "5");

        _userConfigurationService = new UserConfigurationService(configurationMock.Object);
    }

    [Test]
    [TestCase(WallpaperStyle.Fill, "10", "0")]
    [TestCase(WallpaperStyle.Fit, "6", "0")]
    [TestCase(WallpaperStyle.Stretch, "2", "0")]
    [TestCase(WallpaperStyle.Tile, "0", "1")]
    [TestCase(WallpaperStyle.Center, "0", "0")]
    [TestCase(WallpaperStyle.Span, "22", "0")]
    public void SetAsWallpaper_ValidStyleAndTile_RegistersExpectedValues(WallpaperStyle style, string expectedStyleValue, string expectedTileValue)
    {
        Folder folder = new() { Path = dataDirectory! };
        Asset asset = new() { Folder = folder, FileName = "NonExistentFile.jpg" }; // Not giving an existing file to prevent the wallpaper to be changed

        // Set up a StringWriter to capture console output
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        _userConfigurationService!.SetAsWallpaper(asset, style);

        // Get the captured console output
        var consoleOutput = stringWriter.ToString();

        // Assert that the expected error message was printed to the console
        Assert.IsTrue(consoleOutput.Contains($"Wallpaper set for style {expectedStyleValue} and tile {expectedTileValue}"));
    }

    [Test]
    public void GetAboutInformation_WithValidAssembly_ReturnsAboutInformation()
    {
        AboutInformation aboutInformation = _userConfigurationService!.GetAboutInformation(typeof(App).Assembly);

        Assert.AreEqual("PhotoManager", aboutInformation.Product);
        Assert.IsFalse(string.IsNullOrWhiteSpace(aboutInformation.Version));
        Assert.That(aboutInformation.Version, Does.StartWith("v"));
        Assert.AreEqual("v1.0.0", aboutInformation.Version);
    }

    [Test]
    public void GetAboutInformation_WithInvalidAssembly_ReturnsDefaultProduct()
    {
        AboutInformation aboutInformation = _userConfigurationService!.GetAboutInformation(typeof(int).Assembly);

        Assert.AreNotEqual("PhotoManager", aboutInformation.Product);
        Assert.AreEqual("v1.0.0", aboutInformation.Version);
    }

    [Test]
    public void GetPicturesDirectory_ValidPath_ReturnsPicturesDirectory()
    {
        string picturesDirectory = _userConfigurationService!.GetPicturesDirectory();

        Assert.IsFalse(string.IsNullOrWhiteSpace(picturesDirectory));
        Assert.AreEqual(PathConstants.PathLocation, picturesDirectory);
    }

    [Test]
    public void GetApplicationBackupFolder_ValidPath_ReturnsBackupFolderPath()
    {
        string backupFolderPath = _userConfigurationService!.GetApplicationBackupFolderPath();

        Assert.AreEqual(PathConstants.PathBackup, backupFolderPath);
    }

    [Test]
    public void GetCatalogBatchSize_CorrectSize_ReturnsBatchSize()
    {
        int batchSize = _userConfigurationService!.GetCatalogBatchSize();

        Assert.IsNotNull(batchSize);
        Assert.AreEqual(100, batchSize);
    }

    [Test]
    public void GetCatalogCooldownMinutes_CorrectValue_ReturnsCatalogCooldownMinutes()
    {
        int catalogCooldownMinutes = _userConfigurationService!.GetCatalogCooldownMinutes();

        Assert.IsNotNull(catalogCooldownMinutes);
        Assert.AreEqual(5, catalogCooldownMinutes);
    }

    [Test]
    public void GetBackupsToKeep_CorrectValue_ReturnsBackupsToKeep()
    {
        int backupsToKeep = _userConfigurationService!.GetBackupsToKeep();

        Assert.IsNotNull(backupsToKeep);
        Assert.AreEqual(2, backupsToKeep);
    }

    [Test]
    public void GetThumbnailsDictionaryEntriesToKeep_CorrectValue_ReturnsThumbnailsDictionaryEntriesToKeep()
    {
        int thumbnailsDictionaryEntriesToKeep = _userConfigurationService!.GetThumbnailsDictionaryEntriesToKeep();

        Assert.IsNotNull(thumbnailsDictionaryEntriesToKeep);
        Assert.AreEqual(5, thumbnailsDictionaryEntriesToKeep);
    }

    [Test]
    public void GetRootCatalogFolderPaths_CorrectPath_ReturnsArrayOfPaths()
    {
        string[] paths = _userConfigurationService!.GetRootCatalogFolderPaths();

        Assert.IsNotNull(paths);
        Assert.AreEqual(1, paths.Length);
        Assert.AreEqual(PathConstants.PathLocation, paths[0]);
    }
}
