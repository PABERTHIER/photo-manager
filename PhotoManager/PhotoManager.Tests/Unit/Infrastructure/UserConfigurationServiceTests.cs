using PhotoManager.UI;
using System.Reflection;
using System.Reflection.Emit;
using Directories = PhotoManager.Tests.Unit.Constants.Directories;

namespace PhotoManager.Tests.Unit.Infrastructure;

[TestFixture]
public class UserConfigurationServiceTests
{
    private UserConfigurationService? _userConfigurationService;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        IConfigurationRoot configurationRootMock = Substitute.For<IConfigurationRoot>();
        configurationRootMock.GetDefaultMockConfig();

        _userConfigurationService = new(configurationRootMock);
    }

    [Test]
    public void GetAboutInformation_WithValidAssembly_ReturnsAboutInformation()
    {
        AboutInformation aboutInformation = _userConfigurationService!.GetAboutInformation(typeof(App).Assembly);

        Assert.That(aboutInformation.Product, Is.EqualTo("PhotoManager"));
        Assert.That(aboutInformation.Author, Is.EqualTo("Toto"));
        Assert.That(string.IsNullOrWhiteSpace(aboutInformation.Version), Is.False);
        Assert.That(aboutInformation.Version, Does.StartWith("v"));
        Assert.That(aboutInformation.Version, Is.EqualTo(Constants.VERSION));
    }

    [Test]
    public void GetAboutInformation_WithDifferentAssembly_ReturnsDifferentProduct()
    {
        AboutInformation aboutInformation = _userConfigurationService!.GetAboutInformation(typeof(int).Assembly);

        Assert.That(aboutInformation.Product, Is.Not.EqualTo("PhotoManager"));
        Assert.That(aboutInformation.Product, Is.EqualTo("Microsoft® .NET"));
        Assert.That(aboutInformation.Author, Is.EqualTo("Toto"));
        Assert.That(aboutInformation.Version, Is.EqualTo(Constants.VERSION));
    }

    [Test]
    public void GetAboutInformation_WithAssemblyWithoutProductAttribute_ReturnsDefaultProduct()
    {
        AssemblyName assemblyName = new("TestAssemblyWithoutProductAttribute");
        AssemblyBuilder assemblyBuilder =
            AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);

        AboutInformation aboutInformation = _userConfigurationService!.GetAboutInformation(assemblyBuilder);

        Assert.That(aboutInformation.Product, Is.EqualTo("PhotoManager"));
        Assert.That(aboutInformation.Author, Is.EqualTo("Toto"));
        Assert.That(aboutInformation.Version, Is.EqualTo(Constants.VERSION));
    }

    [Test]
    public void AnalyseVideos_CorrectValue_ReturnsAnalyseVideosValue()
    {
        bool analyseVideos = _userConfigurationService!.AssetSettings.AnalyseVideos;

        Assert.That(analyseVideos, Is.False);
    }

    [Test]
    public void CorruptedMessage_CorrectValue_ReturnsCorruptedMessageValue()
    {
        string corruptedMessage = _userConfigurationService!.AssetSettings.CorruptedMessage;

        Assert.That(corruptedMessage, Is.Not.Null);
        Assert.That(corruptedMessage, Is.EqualTo("The asset is corrupted"));
    }

    [Test]
    public void RotatedMessage_CorrectValue_ReturnsRotatedMessageValue()
    {
        string rotatedMessage = _userConfigurationService!.AssetSettings.RotatedMessage;

        Assert.That(rotatedMessage, Is.Not.Null);
        Assert.That(rotatedMessage, Is.EqualTo("The asset has been rotated"));
    }

    [Test]
    public void CatalogBatchSize_CorrectValue_ReturnsCatalogBatchSizeValue()
    {
        int catalogBatchSize = _userConfigurationService!.AssetSettings.CatalogBatchSize;

        Assert.That(catalogBatchSize, Is.EqualTo(100));
    }

    [Test]
    public void CatalogCooldownMinutes_CorrectValue_ReturnsCatalogCooldownMinutesValue()
    {
        ushort catalogCooldownMinutes = _userConfigurationService!.AssetSettings.CatalogCooldownMinutes;

        Assert.That(catalogCooldownMinutes, Is.EqualTo(5));
    }

    [Test]
    public void CorruptedImageOrientation_CorrectValue_ReturnsCorruptedImageOrientationValue()
    {
        ushort corruptedImageOrientation = _userConfigurationService!.AssetSettings.CorruptedImageOrientation;

        Assert.That(corruptedImageOrientation, Is.EqualTo(10000));
    }

    [Test]
    public void DefaultExifOrientation_CorrectValue_ReturnsDefaultExifOrientationValue()
    {
        ushort defaultExifOrientation = _userConfigurationService!.AssetSettings.DefaultExifOrientation;

        Assert.That(defaultExifOrientation, Is.EqualTo(1));
    }

    [Test]
    public void DetectThumbnails_CorrectValue_ReturnsDetectThumbnailsValue()
    {
        bool detectThumbnails = _userConfigurationService!.AssetSettings.DetectThumbnails;

        Assert.That(detectThumbnails, Is.False);
    }

    [Test]
    public void SyncAssetsEveryXMinutes_CorrectValue_ReturnsSyncAssetsEveryXMinutesValue()
    {
        bool syncAssetsEveryXMinutes = _userConfigurationService!.AssetSettings.SyncAssetsEveryXMinutes;

        Assert.That(syncAssetsEveryXMinutes, Is.False);
    }

    [Test]
    public void ThumbnailMaxHeight_CorrectValue_ReturnsThumbnailMaxHeightValue()
    {
        int thumbnailMaxHeight = _userConfigurationService!.AssetSettings.ThumbnailMaxHeight;

        Assert.That(thumbnailMaxHeight, Is.EqualTo(150));
    }

    [Test]
    public void ThumbnailMaxWidth_CorrectValue_ReturnsThumbnailMaxWidthValue()
    {
        int thumbnailMaxWidth = _userConfigurationService!.AssetSettings.ThumbnailMaxWidth;

        Assert.That(thumbnailMaxWidth, Is.EqualTo(200));
    }

    [Test]
    public void PHashThreshold_CorrectValue_ReturnsPHashThresholdValue()
    {
        ushort pHashThreshold = _userConfigurationService!.HashSettings.PHashThreshold;

        Assert.That(pHashThreshold, Is.EqualTo(10));
    }

    [Test]
    public void UsingDHash_CorrectValue_ReturnsUsingDHashValue()
    {
        bool usingDHash = _userConfigurationService!.HashSettings.UsingDHash;

        Assert.That(usingDHash, Is.False);
    }

    [Test]
    public void UsingMD5Hash_CorrectValue_ReturnsUsingMD5HashValue()
    {
        bool usingMD5Hash = _userConfigurationService!.HashSettings.UsingMD5Hash;

        Assert.That(usingMD5Hash, Is.False);
    }

    [Test]
    public void UsingPHash_CorrectValue_ReturnsUsingPHashValue()
    {
        bool usingPHash = _userConfigurationService!.HashSettings.UsingPHash;

        Assert.That(usingPHash, Is.False);
    }

    [Test]
    public void AssetsDirectory_CorrectValue_ReturnsAssetsDirectoryValue()
    {
        string assetsDirectory = _userConfigurationService!.PathSettings.AssetsDirectory;

        Assert.That(assetsDirectory, Is.Not.Null);
        Assert.That(assetsDirectory,
            Is.EqualTo(PathHelper.ToPlatformAbsolutePath("E:\\Workspace\\PhotoManager\\TestAssets")));
    }

    [Test]
    public void ExemptedFolderPath_CorrectValue_ReturnsExemptedFolderPathValue()
    {
        string exemptedFolderPath = _userConfigurationService!.PathSettings.ExemptedFolderPath;

        Assert.That(exemptedFolderPath, Is.Not.Null);
        Assert.That(exemptedFolderPath,
            Is.EqualTo(PathHelper.ToPlatformAbsolutePath("E:\\Workspace\\PhotoManager\\TestAssets\\Exempted")));
    }

    [Test]
    public void FirstFrameVideosPath_CorrectValue_ReturnsFirstFrameVideosPathValue()
    {
        string assetsDirectory = _userConfigurationService!.PathSettings.AssetsDirectory;
        string firstFrameVideosPath = _userConfigurationService!.PathSettings.FirstFrameVideosPath;

        Assert.That(assetsDirectory, Is.Not.Null);
        Assert.That(firstFrameVideosPath, Is.Not.Null);

        Assert.That(firstFrameVideosPath,
            Is.EqualTo(Path.Combine(assetsDirectory, Directories.OUTPUT_VIDEO_FIRST_FRAME)));
        Assert.That(firstFrameVideosPath,
            Is.EqualTo(Path.Combine(PathHelper.ToPlatformAbsolutePath("E:\\Workspace\\PhotoManager\\TestAssets"),
                Directories.OUTPUT_VIDEO_FIRST_FRAME)));
    }

    [Test]
    public void PathSettings_PathsContainEnvironmentVariables_ReturnsExpandedPaths()
    {
        const string assetsVariableName = "PHOTOMANAGER_TEST_ASSETS_DIRECTORY";
        const string exemptedVariableName = "PHOTOMANAGER_TEST_EXEMPTED_FOLDER";
        string? previousAssetsDirectory = Environment.GetEnvironmentVariable(assetsVariableName);
        string? previousExemptedFolder = Environment.GetEnvironmentVariable(exemptedVariableName);
        string expandedAssetsDirectory = Path.GetFullPath(Path.Combine("C:\\PhotoManager", "Assets"));
        string expandedExemptedFolder = Path.GetFullPath(Path.Combine("C:\\PhotoManager", "Exempted"));

        try
        {
            Environment.SetEnvironmentVariable(assetsVariableName, expandedAssetsDirectory);
            Environment.SetEnvironmentVariable(exemptedVariableName, expandedExemptedFolder);

            IConfigurationRoot configurationRootMock = Substitute.For<IConfigurationRoot>();
            configurationRootMock.GetDefaultMockConfig();
            configurationRootMock
                .MockGetValue(UserConfigurationKeys.ASSETS_DIRECTORY, $"%{assetsVariableName}%")
                .MockGetValue(UserConfigurationKeys.EXEMPTED_FOLDER_PATH, "$" + "{" + exemptedVariableName + "}");

            UserConfigurationService userConfigurationService = new(configurationRootMock);

            Assert.That(userConfigurationService.PathSettings.AssetsDirectory, Is.EqualTo(expandedAssetsDirectory));
            Assert.That(userConfigurationService.PathSettings.ExemptedFolderPath, Is.EqualTo(expandedExemptedFolder));
            Assert.That(userConfigurationService.PathSettings.FirstFrameVideosPath,
                Is.EqualTo(Path.Combine(expandedAssetsDirectory, Directories.OUTPUT_VIDEO_FIRST_FRAME)));
        }
        finally
        {
            Environment.SetEnvironmentVariable(assetsVariableName, previousAssetsDirectory);
            Environment.SetEnvironmentVariable(exemptedVariableName, previousExemptedFolder);
        }
    }

    [Test]
    public void PathSettings_PathsContainHomeDirectory_ReturnsExpandedPaths()
    {
        IConfigurationRoot configurationRootMock = Substitute.For<IConfigurationRoot>();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock
            .MockGetValue(UserConfigurationKeys.ASSETS_DIRECTORY, "~/Pictures")
            .MockGetValue(UserConfigurationKeys.EXEMPTED_FOLDER_PATH, "~\\Exempted");

        UserConfigurationService userConfigurationService = new(configurationRootMock);
        string homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        Assert.That(userConfigurationService.PathSettings.AssetsDirectory,
            Is.EqualTo(Path.Combine(homeDirectory, "Pictures")));
        Assert.That(userConfigurationService.PathSettings.ExemptedFolderPath,
            Is.EqualTo(Path.Combine(homeDirectory, "Exempted")));
    }

    [Test]
    public void ProjectName_CorrectValue_ReturnsProjectNameValue()
    {
        string projectName = _userConfigurationService!.ProjectSettings.Name;

        Assert.That(projectName, Is.Not.Null);
        Assert.That(projectName, Is.EqualTo("PhotoManager"));
    }

    [Test]
    public void ProjectOwner_CorrectValue_ReturnsProjectOwnerValue()
    {
        string projectOwner = _userConfigurationService!.ProjectSettings.Owner;

        Assert.That(projectOwner, Is.Not.Null);
        Assert.That(projectOwner, Is.EqualTo("Toto"));
    }

    [Test]
    public void BackupsToKeep_CorrectValue_ReturnsBackupsToKeepValue()
    {
        ushort backupsToKeep = _userConfigurationService!.StorageSettings.BackupsToKeep;

        Assert.That(backupsToKeep, Is.EqualTo(2));
    }

    [Test]
    public void ThumbnailsDictionaryEntriesToKeep_CorrectValue_ReturnsThumbnailsDictionaryEntriesToKeepValue()
    {
        ushort thumbnailsDictionaryEntriesToKeep =
            _userConfigurationService!.StorageSettings.ThumbnailsDictionaryEntriesToKeep;

        Assert.That(thumbnailsDictionaryEntriesToKeep, Is.EqualTo(5));
    }

    [Test]
    public void GetRootCatalogFolderPaths_CorrectPath_ReturnsArrayOfPaths()
    {
        string[] paths = _userConfigurationService!.GetRootCatalogFolderPaths();

        Assert.That(paths, Has.Length.EqualTo(1));
        Assert.That(paths[0], Is.EqualTo(PathHelper.ToPlatformAbsolutePath("E:\\Workspace\\PhotoManager\\TestAssets")));
    }

    [Test]
    [TestCase("~/Pictures", "", "~/Pictures")]
    [TestCase("~/Pictures", " ", "~/Pictures")]
    [TestCase("~/Pictures", null, "~/Pictures")]
    [TestCase("~\\Documents", "", "~\\Documents")]
    [TestCase("~", "", "~")]
    [TestCase("~", null, "~")]
    public void ExpandHomeDirectory_HomeDirectoryIsNullOrWhiteSpace_ReturnsOriginalPath(
        string path, string? homeDirectory, string expectedResult)
    {
        string result = UserConfigurationService.ExpandHomeDirectory(path, homeDirectory!);

        Assert.That(result, Is.EqualTo(expectedResult));
    }

    [Test]
    [TestCase("~/Pictures", "C:\\Users\\Test", "Pictures")]
    [TestCase("~\\Documents", "C:\\Users\\Test", "Documents")]
    public void ExpandHomeDirectory_HomeRelativePath_JoinsHomeDirectoryAndSubPath(
        string path, string homeDirectory, string expectedSubPath)
    {
        string result = UserConfigurationService.ExpandHomeDirectory(path, homeDirectory);

        // The sub path is joined with the OS separator, so the expected value is built the same way
        Assert.That(result, Is.EqualTo(Path.Combine(homeDirectory, expectedSubPath)));
    }

    [Test]
    [TestCase("~", "C:\\Users\\Test", "C:\\Users\\Test")]
    [TestCase("/absolute/path", "C:\\Users\\Test", "/absolute/path")]
    [TestCase("C:\\absolute\\path", "C:\\Users\\Test", "C:\\absolute\\path")]
    [TestCase("relative/path", "C:\\Users\\Test", "relative/path")]
    public void ExpandHomeDirectory_VariousPaths_ReturnsExpectedResult(
        string path, string homeDirectory, string expectedResult)
    {
        string result = UserConfigurationService.ExpandHomeDirectory(path, homeDirectory);

        Assert.That(result, Is.EqualTo(expectedResult));
    }

    [Test]
    public void PathSettings_EmptyAssetsDirectory_ReturnsEmptyPath()
    {
        IConfigurationRoot configurationRootMock = Substitute.For<IConfigurationRoot>();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock
            .MockGetValue(UserConfigurationKeys.ASSETS_DIRECTORY, "")
            .MockGetValue(UserConfigurationKeys.EXEMPTED_FOLDER_PATH, "");

        UserConfigurationService userConfigurationService = new(configurationRootMock);

        Assert.That(userConfigurationService.PathSettings.AssetsDirectory, Is.EqualTo(""));
        Assert.That(userConfigurationService.PathSettings.ExemptedFolderPath, Is.EqualTo(""));
    }

    [Test]
    public void PathSettings_WhitespaceAssetsDirectory_ReturnsWhitespacePath()
    {
        IConfigurationRoot configurationRootMock = Substitute.For<IConfigurationRoot>();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock
            .MockGetValue(UserConfigurationKeys.ASSETS_DIRECTORY, "   ")
            .MockGetValue(UserConfigurationKeys.EXEMPTED_FOLDER_PATH, " ");

        UserConfigurationService userConfigurationService = new(configurationRootMock);

        Assert.That(userConfigurationService.PathSettings.AssetsDirectory, Is.EqualTo("   "));
        Assert.That(userConfigurationService.PathSettings.ExemptedFolderPath, Is.EqualTo(" "));
    }

    [Test]
    public void PathSettings_PathsContainBareUnixVariable_ReturnsExpandedPaths()
    {
        const string variableName = "PHOTOMANAGER_TEST_BARE_VAR";
        string? previousValue = Environment.GetEnvironmentVariable(variableName);
        string expandedValue = Path.GetFullPath("C:\\PhotoManager\\BareVarAssets");

        try
        {
            Environment.SetEnvironmentVariable(variableName, expandedValue);

            IConfigurationRoot configurationRootMock = Substitute.For<IConfigurationRoot>();
            configurationRootMock.GetDefaultMockConfig();
            configurationRootMock
                .MockGetValue(UserConfigurationKeys.ASSETS_DIRECTORY, "$" + variableName)
                .MockGetValue(UserConfigurationKeys.EXEMPTED_FOLDER_PATH, "$" + variableName + "\\Exempted");

            UserConfigurationService userConfigurationService = new(configurationRootMock);

            Assert.That(userConfigurationService.PathSettings.AssetsDirectory, Is.EqualTo(expandedValue));
            Assert.That(userConfigurationService.PathSettings.ExemptedFolderPath,
                Is.EqualTo(Path.GetFullPath(expandedValue + "\\Exempted")));
        }
        finally
        {
            Environment.SetEnvironmentVariable(variableName, previousValue);
        }
    }

    [Test]
    public void PathSettings_PathsContainUndefinedUnixVariable_KeepsOriginalVariableText()
    {
        const string variableName = "PHOTOMANAGER_UNDEFINED_VAR_12345";
        string? previousValue = Environment.GetEnvironmentVariable(variableName);

        try
        {
            Environment.SetEnvironmentVariable(variableName, null);

            IConfigurationRoot configurationRootMock = Substitute.For<IConfigurationRoot>();
            configurationRootMock.GetDefaultMockConfig();
            configurationRootMock
                .MockGetValue(UserConfigurationKeys.ASSETS_DIRECTORY, "${" + variableName + "}")
                .MockGetValue(UserConfigurationKeys.EXEMPTED_FOLDER_PATH, "$" + variableName);

            UserConfigurationService userConfigurationService = new(configurationRootMock);

            string expectedBracedPath = Path.GetFullPath("${" + variableName + "}");
            string expectedBarePath = Path.GetFullPath("$" + variableName);

            Assert.That(userConfigurationService.PathSettings.AssetsDirectory, Is.EqualTo(expectedBracedPath));
            Assert.That(userConfigurationService.PathSettings.ExemptedFolderPath, Is.EqualTo(expectedBarePath));
        }
        finally
        {
            Environment.SetEnvironmentVariable(variableName, previousValue);
        }
    }
}
