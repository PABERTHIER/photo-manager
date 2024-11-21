namespace PhotoManager.Tests.Unit.Domain;

[TestFixture]
public class SyncAssetsConfigurationTests
{
    private SyncAssetsConfiguration? _syncAssetsConfiguration;

    [SetUp]
    public void SetUp()
    {
        _syncAssetsConfiguration = new SyncAssetsConfiguration();
    }

    [Test]
    public void Validate_DefinitionsAreValidAndLocalPath_RemovesNoneDefinitions()
    {
        _syncAssetsConfiguration!.Definitions.Add(new() { SourceDirectory = "C:\\Valid1\\Path", DestinationDirectory = "C:\\Valid2\\Path" });
        _syncAssetsConfiguration.Definitions.Add(new() { SourceDirectory = "C:\\Valid3\\Path", DestinationDirectory = "C:\\Valid4\\Path" });

        _syncAssetsConfiguration.Validate();

        Assert.AreEqual(2, _syncAssetsConfiguration.Definitions.Count);
        Assert.AreEqual("C:\\Valid1\\Path", _syncAssetsConfiguration.Definitions[0].SourceDirectory);
        Assert.AreEqual("C:\\Valid2\\Path", _syncAssetsConfiguration.Definitions[0].DestinationDirectory);
        Assert.AreEqual(false, _syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource);
        Assert.AreEqual(false, _syncAssetsConfiguration.Definitions[0].IncludeSubFolders);

        Assert.AreEqual("C:\\Valid3\\Path", _syncAssetsConfiguration.Definitions[1].SourceDirectory);
        Assert.AreEqual("C:\\Valid4\\Path", _syncAssetsConfiguration.Definitions[1].DestinationDirectory);
        Assert.AreEqual(false, _syncAssetsConfiguration.Definitions[1].DeleteAssetsNotInSource);
        Assert.AreEqual(false, _syncAssetsConfiguration.Definitions[1].IncludeSubFolders);
    }

    [Test]
    public void Validate_DefinitionsAreValidAndSourceAreLocalPathAndDestinationAreRemotePath_RemovesNoneDefinitions()
    {
        _syncAssetsConfiguration!.Definitions.Add(new() { SourceDirectory = "C:\\Valid1\\Path", DestinationDirectory = "\\Server\\Valid1\\Path" });
        _syncAssetsConfiguration.Definitions.Add(new() { SourceDirectory = "C:\\Valid2\\Path", DestinationDirectory = "\\Server\\Valid2\\Path" });

        _syncAssetsConfiguration.Validate();

        Assert.AreEqual(2, _syncAssetsConfiguration.Definitions.Count);
        Assert.AreEqual("C:\\Valid1\\Path", _syncAssetsConfiguration.Definitions[0].SourceDirectory);
        Assert.AreEqual("\\Server\\Valid1\\Path", _syncAssetsConfiguration.Definitions[0].DestinationDirectory);
        Assert.AreEqual(false, _syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource);
        Assert.AreEqual(false, _syncAssetsConfiguration.Definitions[0].IncludeSubFolders);

        Assert.AreEqual("C:\\Valid2\\Path", _syncAssetsConfiguration.Definitions[1].SourceDirectory);
        Assert.AreEqual("\\Server\\Valid2\\Path", _syncAssetsConfiguration.Definitions[1].DestinationDirectory);
        Assert.AreEqual(false, _syncAssetsConfiguration.Definitions[1].DeleteAssetsNotInSource);
        Assert.AreEqual(false, _syncAssetsConfiguration.Definitions[1].IncludeSubFolders);
    }

    [Test]
    public void Validate_DefinitionsAreValidAndSourceAreRemotePathAndDestinationAreLocalPath_RemovesNoneDefinitions()
    {
        _syncAssetsConfiguration!.Definitions.Add(new() { SourceDirectory = "\\Server\\Valid1\\Path", DestinationDirectory = "C:\\Valid1\\Path" });
        _syncAssetsConfiguration.Definitions.Add(new() { SourceDirectory = "\\Server\\Valid2\\Path", DestinationDirectory = "C:\\Valid2\\Path" });

        _syncAssetsConfiguration.Validate();

        Assert.AreEqual(2, _syncAssetsConfiguration.Definitions.Count);
        Assert.AreEqual("\\Server\\Valid1\\Path", _syncAssetsConfiguration.Definitions[0].SourceDirectory);
        Assert.AreEqual("C:\\Valid1\\Path", _syncAssetsConfiguration.Definitions[0].DestinationDirectory);
        Assert.AreEqual(false, _syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource);
        Assert.AreEqual(false, _syncAssetsConfiguration.Definitions[0].IncludeSubFolders);

        Assert.AreEqual("\\Server\\Valid2\\Path", _syncAssetsConfiguration.Definitions[1].SourceDirectory);
        Assert.AreEqual("C:\\Valid2\\Path", _syncAssetsConfiguration.Definitions[1].DestinationDirectory);
        Assert.AreEqual(false, _syncAssetsConfiguration.Definitions[1].DeleteAssetsNotInSource);
        Assert.AreEqual(false, _syncAssetsConfiguration.Definitions[1].IncludeSubFolders);
    }

    [Test]
    public void Validate_DefinitionsAreValidAndRemotePath_RemovesNoneDefinitions()
    {
        _syncAssetsConfiguration!.Definitions.Add(new() { SourceDirectory = "\\Server\\Valid1\\Path", DestinationDirectory = "\\Server\\Valid2\\Path" });
        _syncAssetsConfiguration.Definitions.Add(new() { SourceDirectory = "\\Server\\Valid3\\Path", DestinationDirectory = "\\Server\\Valid4\\Path" });

        _syncAssetsConfiguration.Validate();

        Assert.AreEqual(2, _syncAssetsConfiguration.Definitions.Count);
        Assert.AreEqual("\\Server\\Valid1\\Path", _syncAssetsConfiguration.Definitions[0].SourceDirectory);
        Assert.AreEqual("\\Server\\Valid2\\Path", _syncAssetsConfiguration.Definitions[0].DestinationDirectory);
        Assert.AreEqual(false, _syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource);
        Assert.AreEqual(false, _syncAssetsConfiguration.Definitions[0].IncludeSubFolders);

        Assert.AreEqual("\\Server\\Valid3\\Path", _syncAssetsConfiguration.Definitions[1].SourceDirectory);
        Assert.AreEqual("\\Server\\Valid4\\Path", _syncAssetsConfiguration.Definitions[1].DestinationDirectory);
        Assert.AreEqual(false, _syncAssetsConfiguration.Definitions[1].DeleteAssetsNotInSource);
        Assert.AreEqual(false, _syncAssetsConfiguration.Definitions[1].IncludeSubFolders);
    }

    [Test]
    public void Validate_DefinitionsAreValidAndLocalPathOrRemotePath_RemovesNoneDefinitions()
    {
        _syncAssetsConfiguration!.Definitions.Add(new() { SourceDirectory = "C:\\Valid1\\Path", DestinationDirectory = "\\Server\\Valid1\\Path" });
        _syncAssetsConfiguration.Definitions.Add(new() { SourceDirectory = "\\Server\\Valid2\\Path", DestinationDirectory = "C:\\Valid2\\Path" });

        _syncAssetsConfiguration.Validate();

        Assert.AreEqual(2, _syncAssetsConfiguration.Definitions.Count);
        Assert.AreEqual("C:\\Valid1\\Path", _syncAssetsConfiguration.Definitions[0].SourceDirectory);
        Assert.AreEqual("\\Server\\Valid1\\Path", _syncAssetsConfiguration.Definitions[0].DestinationDirectory);
        Assert.AreEqual(false, _syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource);
        Assert.AreEqual(false, _syncAssetsConfiguration.Definitions[0].IncludeSubFolders);

        Assert.AreEqual("\\Server\\Valid2\\Path", _syncAssetsConfiguration.Definitions[1].SourceDirectory);
        Assert.AreEqual("C:\\Valid2\\Path", _syncAssetsConfiguration.Definitions[1].DestinationDirectory);
        Assert.AreEqual(false, _syncAssetsConfiguration.Definitions[1].DeleteAssetsNotInSource);
        Assert.AreEqual(false, _syncAssetsConfiguration.Definitions[1].IncludeSubFolders);
    }

    [Test]
    public void Validate_DefinitionsAreValidAndSameSourceAndDestination_RemovesNoneDefinitions()
    {
        _syncAssetsConfiguration!.Definitions.Add(new() { SourceDirectory = "C:\\Valid1\\Path", DestinationDirectory = "C:\\Valid1\\Path" });

        _syncAssetsConfiguration.Validate();

        Assert.AreEqual(1, _syncAssetsConfiguration.Definitions.Count);
        Assert.AreEqual("C:\\Valid1\\Path", _syncAssetsConfiguration.Definitions[0].SourceDirectory);
        Assert.AreEqual("C:\\Valid1\\Path", _syncAssetsConfiguration.Definitions[0].DestinationDirectory);
        Assert.AreEqual(false, _syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource);
        Assert.AreEqual(false, _syncAssetsConfiguration.Definitions[0].IncludeSubFolders);
    }

    [Test]
    public void Validate_DefinitionsAreEmpty_DefinitionsRemainEmpty()
    {
        _syncAssetsConfiguration!.Validate();

        Assert.AreEqual(0, _syncAssetsConfiguration.Definitions.Count);
    }

    [Test]
    public void Validate_DefinitionsAreValidAndInvalid_RemovesInvalidDefinitions()
    {
        _syncAssetsConfiguration!.Definitions.Add(new() { SourceDirectory = "C:\\Valid1\\Path", DestinationDirectory = "\\Server\\Valid1\\Path" });
        _syncAssetsConfiguration.Definitions.Add(new() { SourceDirectory = "Invalid\\Path", DestinationDirectory = "\\Server\\Valid\\Path" });

        _syncAssetsConfiguration.Validate();

        Assert.AreEqual(1, _syncAssetsConfiguration.Definitions.Count);
        Assert.AreEqual("C:\\Valid1\\Path", _syncAssetsConfiguration.Definitions[0].SourceDirectory);
        Assert.AreEqual("\\Server\\Valid1\\Path", _syncAssetsConfiguration.Definitions[0].DestinationDirectory);
        Assert.AreEqual(false, _syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource);
        Assert.AreEqual(false, _syncAssetsConfiguration.Definitions[0].IncludeSubFolders);
    }

    [Test]
    public void Validate_DefinitionsAreInvalid_RemovesAllDefinitions()
    {
        _syncAssetsConfiguration!.Definitions.Add(new() { SourceDirectory = "Invalid1\\Path", DestinationDirectory = "Invalid2\\Path" });
        _syncAssetsConfiguration.Definitions.Add(new() { SourceDirectory = "https://www.some-site.com", DestinationDirectory = "ftp://some-location.com" });
        _syncAssetsConfiguration.Definitions.Add(new() { SourceDirectory = "Invalid@Value.com", DestinationDirectory = "Invalid@Value.com" });
        _syncAssetsConfiguration.Definitions.Add(new() { SourceDirectory = "", DestinationDirectory = "" });
        _syncAssetsConfiguration.Definitions.Add(new() { SourceDirectory = "", DestinationDirectory = null! });

        _syncAssetsConfiguration.Validate();

        Assert.AreEqual(0, _syncAssetsConfiguration.Definitions.Count);
    }

    [Test]
    [TestCase(null, "")]
    [TestCase(null, null)]
    public void Validate_DefinitionsAreInvalidSourceAndDestinationAreNull_ThrowsArgumentNullExceptionAndDoesNotRemoveDefinitions(string sourceDirectory, string destinationDirectory)
    {
        _syncAssetsConfiguration!.Definitions.Add(new() { SourceDirectory = sourceDirectory, DestinationDirectory = destinationDirectory });

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _syncAssetsConfiguration.Validate());

        Assert.AreEqual("Value cannot be null. (Parameter 'input')", exception?.Message);
        Assert.AreEqual(1, _syncAssetsConfiguration.Definitions.Count);
        Assert.AreEqual(sourceDirectory, _syncAssetsConfiguration.Definitions[0].SourceDirectory);
        Assert.AreEqual(destinationDirectory, _syncAssetsConfiguration.Definitions[0].DestinationDirectory);
        Assert.AreEqual(false, _syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource);
        Assert.AreEqual(false, _syncAssetsConfiguration.Definitions[0].IncludeSubFolders);
    }

    [Test]
    public void Normalize_PathsAreAlreadyNormalized_PathsAreNotModified()
    {
        SyncAssetsDirectoriesDefinition syncAssetsDirectoriesDefinition1 = new() { SourceDirectory = "C:\\Valid1\\Path", DestinationDirectory = "\\Remote\\Valid1\\Path" };
        SyncAssetsDirectoriesDefinition syncAssetsDirectoriesDefinition2 = new() { SourceDirectory = "C:\\Valid2\\Path", DestinationDirectory = "\\Remote\\Valid2\\Path" };

        _syncAssetsConfiguration!.Definitions.Add(syncAssetsDirectoriesDefinition1);
        _syncAssetsConfiguration!.Definitions.Add(syncAssetsDirectoriesDefinition2);

        _syncAssetsConfiguration.Normalize();

        Assert.AreEqual(syncAssetsDirectoriesDefinition1.SourceDirectory, _syncAssetsConfiguration.Definitions[0].SourceDirectory);
        Assert.AreEqual(syncAssetsDirectoriesDefinition1.DestinationDirectory, _syncAssetsConfiguration.Definitions[0].DestinationDirectory);
        Assert.AreEqual(syncAssetsDirectoriesDefinition1.IncludeSubFolders, _syncAssetsConfiguration.Definitions[0].IncludeSubFolders);
        Assert.AreEqual(syncAssetsDirectoriesDefinition1.DeleteAssetsNotInSource, _syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource);

        Assert.AreEqual(syncAssetsDirectoriesDefinition2.SourceDirectory, _syncAssetsConfiguration.Definitions[1].SourceDirectory);
        Assert.AreEqual(syncAssetsDirectoriesDefinition2.DestinationDirectory, _syncAssetsConfiguration.Definitions[1].DestinationDirectory);
        Assert.AreEqual(syncAssetsDirectoriesDefinition2.IncludeSubFolders, _syncAssetsConfiguration.Definitions[1].IncludeSubFolders);
        Assert.AreEqual(syncAssetsDirectoriesDefinition2.DeleteAssetsNotInSource, _syncAssetsConfiguration.Definitions[1].DeleteAssetsNotInSource);

        _syncAssetsConfiguration.Validate();

        Assert.AreEqual(2, _syncAssetsConfiguration.Definitions.Count);
        Assert.AreEqual(syncAssetsDirectoriesDefinition1.SourceDirectory, _syncAssetsConfiguration.Definitions[0].SourceDirectory);
        Assert.AreEqual(syncAssetsDirectoriesDefinition1.DestinationDirectory, _syncAssetsConfiguration.Definitions[0].DestinationDirectory);
        Assert.AreEqual(syncAssetsDirectoriesDefinition1.DeleteAssetsNotInSource, _syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource);
        Assert.AreEqual(syncAssetsDirectoriesDefinition1.IncludeSubFolders, _syncAssetsConfiguration.Definitions[0].IncludeSubFolders);
    }

    [Test]
    public void Normalize_PathsContainExtraBackslashes_RemoveBackslashes()
    {
        SyncAssetsDirectoriesDefinition syncAssetsDirectoriesDefinition = new()
        {
            SourceDirectory = @"C:\Some\\Extra\Backslashes",
            DestinationDirectory = @"\\Remote\With\\\\Extra\Backslashes"
        };

        _syncAssetsConfiguration!.Definitions.Add(syncAssetsDirectoriesDefinition);

        _syncAssetsConfiguration.Normalize();

        Assert.AreEqual("C:\\Some\\Extra\\Backslashes", syncAssetsDirectoriesDefinition.SourceDirectory);
        Assert.AreEqual("\\Remote\\With\\Extra\\Backslashes", syncAssetsDirectoriesDefinition.DestinationDirectory);
        Assert.AreEqual(false, syncAssetsDirectoriesDefinition.DeleteAssetsNotInSource);
        Assert.AreEqual(false, syncAssetsDirectoriesDefinition.IncludeSubFolders);

        _syncAssetsConfiguration.Validate();

        Assert.AreEqual(1, _syncAssetsConfiguration.Definitions.Count);
        Assert.AreEqual("C:\\Some\\Extra\\Backslashes", _syncAssetsConfiguration.Definitions[0].SourceDirectory);
        Assert.AreEqual("\\Remote\\With\\Extra\\Backslashes", _syncAssetsConfiguration.Definitions[0].DestinationDirectory);
        Assert.AreEqual(false, _syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource);
        Assert.AreEqual(false, _syncAssetsConfiguration.Definitions[0].IncludeSubFolders);
    }

    [Test]
    public void Normalize_PathAreLocalAndRemotePaths_PathsAreNormalized()
    {
        SyncAssetsDirectoriesDefinition syncAssetsDirectoriesDefinition1 = new()
        {
            SourceDirectory = @"C:\Local\Path",
            DestinationDirectory = @"C:\Another\Local\Path"
        };

        SyncAssetsDirectoriesDefinition syncAssetsDirectoriesDefinition2 = new()
        {
            SourceDirectory = @"\\\\Server\Remote\Path",
            DestinationDirectory = @"\\Another\Server\Remote\Path"
        };

        SyncAssetsDirectoriesDefinition syncAssetsDirectoriesDefinition3 = new()
        {
            SourceDirectory = @"C:\Local\And\\Remote\Path",
            DestinationDirectory = @"\\Server\And\Another\\Local\Path"
        };

        SyncAssetsDirectoriesDefinition syncAssetsDirectoriesDefinition4 = new()
        {
            SourceDirectory = @"\\Server\And\Another\\Local\Path",
            DestinationDirectory = @"C:\Local\And\\Remote\Path"
        };

        _syncAssetsConfiguration!.Definitions.Add(syncAssetsDirectoriesDefinition1);
        _syncAssetsConfiguration!.Definitions.Add(syncAssetsDirectoriesDefinition2);
        _syncAssetsConfiguration!.Definitions.Add(syncAssetsDirectoriesDefinition3);
        _syncAssetsConfiguration!.Definitions.Add(syncAssetsDirectoriesDefinition4);

        _syncAssetsConfiguration.Normalize();

        Assert.AreEqual("C:\\Local\\Path", syncAssetsDirectoriesDefinition1.SourceDirectory);
        Assert.AreEqual("C:\\Another\\Local\\Path", syncAssetsDirectoriesDefinition1.DestinationDirectory);
        Assert.AreEqual(false, syncAssetsDirectoriesDefinition1.DeleteAssetsNotInSource);
        Assert.AreEqual(false, syncAssetsDirectoriesDefinition1.IncludeSubFolders);

        Assert.AreEqual("\\Server\\Remote\\Path", syncAssetsDirectoriesDefinition2.SourceDirectory);
        Assert.AreEqual("\\Another\\Server\\Remote\\Path", syncAssetsDirectoriesDefinition2.DestinationDirectory);
        Assert.AreEqual(false, syncAssetsDirectoriesDefinition2.DeleteAssetsNotInSource);
        Assert.AreEqual(false, syncAssetsDirectoriesDefinition2.IncludeSubFolders);

        Assert.AreEqual("C:\\Local\\And\\Remote\\Path", syncAssetsDirectoriesDefinition3.SourceDirectory);
        Assert.AreEqual("\\Server\\And\\Another\\Local\\Path", syncAssetsDirectoriesDefinition3.DestinationDirectory);
        Assert.AreEqual(false, syncAssetsDirectoriesDefinition3.DeleteAssetsNotInSource);
        Assert.AreEqual(false, syncAssetsDirectoriesDefinition3.IncludeSubFolders);

        Assert.AreEqual("\\Server\\And\\Another\\Local\\Path", syncAssetsDirectoriesDefinition4.SourceDirectory);
        Assert.AreEqual("C:\\Local\\And\\Remote\\Path", syncAssetsDirectoriesDefinition4.DestinationDirectory);
        Assert.AreEqual(false, syncAssetsDirectoriesDefinition4.DeleteAssetsNotInSource);
        Assert.AreEqual(false, syncAssetsDirectoriesDefinition4.IncludeSubFolders);

        _syncAssetsConfiguration.Validate();

        Assert.AreEqual(4, _syncAssetsConfiguration.Definitions.Count);
        Assert.AreEqual("C:\\Local\\Path", _syncAssetsConfiguration.Definitions[0].SourceDirectory);
        Assert.AreEqual("C:\\Another\\Local\\Path", _syncAssetsConfiguration.Definitions[0].DestinationDirectory);
        Assert.AreEqual(false, _syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource);
        Assert.AreEqual(false, _syncAssetsConfiguration.Definitions[0].IncludeSubFolders);

        Assert.AreEqual("\\Server\\Remote\\Path", _syncAssetsConfiguration.Definitions[1].SourceDirectory);
        Assert.AreEqual("\\Another\\Server\\Remote\\Path", _syncAssetsConfiguration.Definitions[1].DestinationDirectory);
        Assert.AreEqual(false, _syncAssetsConfiguration.Definitions[1].DeleteAssetsNotInSource);
        Assert.AreEqual(false, _syncAssetsConfiguration.Definitions[1].IncludeSubFolders);

        Assert.AreEqual("C:\\Local\\And\\Remote\\Path", _syncAssetsConfiguration.Definitions[2].SourceDirectory);
        Assert.AreEqual("\\Server\\And\\Another\\Local\\Path", _syncAssetsConfiguration.Definitions[2].DestinationDirectory);
        Assert.AreEqual(false, _syncAssetsConfiguration.Definitions[2].DeleteAssetsNotInSource);
        Assert.AreEqual(false, _syncAssetsConfiguration.Definitions[2].IncludeSubFolders);

        Assert.AreEqual("\\Server\\And\\Another\\Local\\Path", _syncAssetsConfiguration.Definitions[3].SourceDirectory);
        Assert.AreEqual("C:\\Local\\And\\Remote\\Path", _syncAssetsConfiguration.Definitions[3].DestinationDirectory);
        Assert.AreEqual(false, _syncAssetsConfiguration.Definitions[3].DeleteAssetsNotInSource);
        Assert.AreEqual(false, _syncAssetsConfiguration.Definitions[3].IncludeSubFolders);
    }

    [Test]
    [TestCase("", null)]
    [TestCase(null, "")]
    [TestCase(null, null)]
    public void Normalize_PathsAreInvalid_ThrowsArgumentNullExceptionAndDoesNotRemoveDefinitions(string sourceDirectory, string destinationDirectory)
    {
        SyncAssetsDirectoriesDefinition syncAssetsDirectoriesDefinition = new() { SourceDirectory = sourceDirectory, DestinationDirectory = destinationDirectory };

        _syncAssetsConfiguration!.Definitions.Add(syncAssetsDirectoriesDefinition);

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _syncAssetsConfiguration.Normalize());

        Assert.AreEqual("Value cannot be null. (Parameter 'input')", exception?.Message);
        Assert.AreEqual(1, _syncAssetsConfiguration.Definitions.Count);
        Assert.AreEqual(sourceDirectory, _syncAssetsConfiguration.Definitions[0].SourceDirectory);
        Assert.AreEqual(destinationDirectory, _syncAssetsConfiguration.Definitions[0].DestinationDirectory);
        Assert.AreEqual(false, _syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource);
        Assert.AreEqual(false, _syncAssetsConfiguration.Definitions[0].IncludeSubFolders);
    }

    [Test]
    public void Normalize_DefinitionsAreEmpty_DoesNothing()
    {
        _syncAssetsConfiguration!.Normalize();

        Assert.AreEqual(0, _syncAssetsConfiguration.Definitions.Count);
    }
}
