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

        Assert.That(_syncAssetsConfiguration.Definitions, Has.Count.EqualTo(2));
        Assert.That(_syncAssetsConfiguration.Definitions[0].SourceDirectory, Is.EqualTo("C:\\Valid1\\Path"));
        Assert.That(_syncAssetsConfiguration.Definitions[0].DestinationDirectory, Is.EqualTo("C:\\Valid2\\Path"));
        Assert.That(_syncAssetsConfiguration.Definitions[0].IncludeSubFolders, Is.EqualTo(false));
        Assert.That(_syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource, Is.EqualTo(false));

        Assert.That(_syncAssetsConfiguration.Definitions[1].SourceDirectory, Is.EqualTo("C:\\Valid3\\Path"));
        Assert.That(_syncAssetsConfiguration.Definitions[1].DestinationDirectory, Is.EqualTo("C:\\Valid4\\Path"));
        Assert.That(_syncAssetsConfiguration.Definitions[1].IncludeSubFolders, Is.EqualTo(false));
        Assert.That(_syncAssetsConfiguration.Definitions[1].DeleteAssetsNotInSource, Is.EqualTo(false));
    }

    [Test]
    public void Validate_DefinitionsAreValidAndSourcesAreLocalPathAndDestinationAreRemotePath_RemovesNoneDefinitions()
    {
        _syncAssetsConfiguration!.Definitions.Add(new() { SourceDirectory = "C:\\Valid1\\Path", DestinationDirectory = "\\Server\\Valid1\\Path" });
        _syncAssetsConfiguration.Definitions.Add(new() { SourceDirectory = "C:\\Valid2\\Path", DestinationDirectory = "\\Server\\Valid2\\Path" });

        _syncAssetsConfiguration.Validate();

        Assert.That(_syncAssetsConfiguration.Definitions, Has.Count.EqualTo(2));
        Assert.That(_syncAssetsConfiguration.Definitions[0].SourceDirectory, Is.EqualTo("C:\\Valid1\\Path"));
        Assert.That(_syncAssetsConfiguration.Definitions[0].DestinationDirectory, Is.EqualTo("\\Server\\Valid1\\Path"));
        Assert.That(_syncAssetsConfiguration.Definitions[0].IncludeSubFolders, Is.EqualTo(false));
        Assert.That(_syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource, Is.EqualTo(false));

        Assert.That(_syncAssetsConfiguration.Definitions[1].SourceDirectory, Is.EqualTo("C:\\Valid2\\Path"));
        Assert.That(_syncAssetsConfiguration.Definitions[1].DestinationDirectory, Is.EqualTo("\\Server\\Valid2\\Path"));
        Assert.That(_syncAssetsConfiguration.Definitions[1].IncludeSubFolders, Is.EqualTo(false));
        Assert.That(_syncAssetsConfiguration.Definitions[1].DeleteAssetsNotInSource, Is.EqualTo(false));
    }

    [Test]
    public void Validate_DefinitionsAreValidAndSourcesAreRemotePathAndDestinationAreLocalPath_RemovesNoneDefinitions()
    {
        _syncAssetsConfiguration!.Definitions.Add(new() { SourceDirectory = "\\Server\\Valid1\\Path", DestinationDirectory = "C:\\Valid1\\Path" });
        _syncAssetsConfiguration.Definitions.Add(new() { SourceDirectory = "\\Server\\Valid2\\Path", DestinationDirectory = "C:\\Valid2\\Path" });

        _syncAssetsConfiguration.Validate();

        Assert.That(_syncAssetsConfiguration.Definitions, Has.Count.EqualTo(2));
        Assert.That(_syncAssetsConfiguration.Definitions[0].SourceDirectory, Is.EqualTo("\\Server\\Valid1\\Path"));
        Assert.That(_syncAssetsConfiguration.Definitions[0].DestinationDirectory, Is.EqualTo("C:\\Valid1\\Path"));
        Assert.That(_syncAssetsConfiguration.Definitions[0].IncludeSubFolders, Is.EqualTo(false));
        Assert.That(_syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource, Is.EqualTo(false));

        Assert.That(_syncAssetsConfiguration.Definitions[1].SourceDirectory, Is.EqualTo("\\Server\\Valid2\\Path"));
        Assert.That(_syncAssetsConfiguration.Definitions[1].DestinationDirectory, Is.EqualTo("C:\\Valid2\\Path"));
        Assert.That(_syncAssetsConfiguration.Definitions[1].IncludeSubFolders, Is.EqualTo(false));
        Assert.That(_syncAssetsConfiguration.Definitions[1].DeleteAssetsNotInSource, Is.EqualTo(false));
    }

    [Test]
    public void Validate_DefinitionsAreValidAndRemotePath_RemovesNoneDefinitions()
    {
        _syncAssetsConfiguration!.Definitions.Add(new() { SourceDirectory = "\\Server\\Valid1\\Path", DestinationDirectory = "\\Server\\Valid2\\Path" });
        _syncAssetsConfiguration.Definitions.Add(new() { SourceDirectory = "\\Server\\Valid3\\Path", DestinationDirectory = "\\Server\\Valid4\\Path" });

        _syncAssetsConfiguration.Validate();

        Assert.That(_syncAssetsConfiguration.Definitions, Has.Count.EqualTo(2));
        Assert.That(_syncAssetsConfiguration.Definitions[0].SourceDirectory, Is.EqualTo("\\Server\\Valid1\\Path"));
        Assert.That(_syncAssetsConfiguration.Definitions[0].DestinationDirectory, Is.EqualTo("\\Server\\Valid2\\Path"));
        Assert.That(_syncAssetsConfiguration.Definitions[0].IncludeSubFolders, Is.EqualTo(false));
        Assert.That(_syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource, Is.EqualTo(false));

        Assert.That(_syncAssetsConfiguration.Definitions[1].SourceDirectory, Is.EqualTo("\\Server\\Valid3\\Path"));
        Assert.That(_syncAssetsConfiguration.Definitions[1].DestinationDirectory, Is.EqualTo("\\Server\\Valid4\\Path"));
        Assert.That(_syncAssetsConfiguration.Definitions[1].IncludeSubFolders, Is.EqualTo(false));
        Assert.That(_syncAssetsConfiguration.Definitions[1].DeleteAssetsNotInSource, Is.EqualTo(false));
    }

    [Test]
    public void Validate_DefinitionsAreValidAndLocalPathOrRemotePath_RemovesNoneDefinitions()
    {
        _syncAssetsConfiguration!.Definitions.Add(new() { SourceDirectory = "C:\\Valid1\\Path", DestinationDirectory = "\\Server\\Valid1\\Path" });
        _syncAssetsConfiguration.Definitions.Add(new() { SourceDirectory = "\\Server\\Valid2\\Path", DestinationDirectory = "C:\\Valid2\\Path" });

        _syncAssetsConfiguration.Validate();

        Assert.That(_syncAssetsConfiguration.Definitions, Has.Count.EqualTo(2));
        Assert.That(_syncAssetsConfiguration.Definitions[0].SourceDirectory, Is.EqualTo("C:\\Valid1\\Path"));
        Assert.That(_syncAssetsConfiguration.Definitions[0].DestinationDirectory, Is.EqualTo("\\Server\\Valid1\\Path"));
        Assert.That(_syncAssetsConfiguration.Definitions[0].IncludeSubFolders, Is.EqualTo(false));
        Assert.That(_syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource, Is.EqualTo(false));

        Assert.That(_syncAssetsConfiguration.Definitions[1].SourceDirectory, Is.EqualTo("\\Server\\Valid2\\Path"));
        Assert.That(_syncAssetsConfiguration.Definitions[1].DestinationDirectory, Is.EqualTo("C:\\Valid2\\Path"));
        Assert.That(_syncAssetsConfiguration.Definitions[1].IncludeSubFolders, Is.EqualTo(false));
        Assert.That(_syncAssetsConfiguration.Definitions[1].DeleteAssetsNotInSource, Is.EqualTo(false));
    }

    [Test]
    public void Validate_DefinitionsAreValidAndSameSourceAndDestination_RemovesNoneDefinitions()
    {
        _syncAssetsConfiguration!.Definitions.Add(new() { SourceDirectory = "C:\\Valid1\\Path", DestinationDirectory = "C:\\Valid1\\Path" });

        _syncAssetsConfiguration.Validate();

        Assert.That(_syncAssetsConfiguration.Definitions, Has.Count.EqualTo(1));
        Assert.That(_syncAssetsConfiguration.Definitions[0].SourceDirectory, Is.EqualTo("C:\\Valid1\\Path"));
        Assert.That(_syncAssetsConfiguration.Definitions[0].DestinationDirectory, Is.EqualTo("C:\\Valid1\\Path"));
        Assert.That(_syncAssetsConfiguration.Definitions[0].IncludeSubFolders, Is.EqualTo(false));
        Assert.That(_syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource, Is.EqualTo(false));
    }

    [Test]
    public void Validate_DefinitionsAreEmpty_DefinitionsRemainEmpty()
    {
        _syncAssetsConfiguration!.Validate();

        Assert.That(_syncAssetsConfiguration.Definitions.Count, Is.EqualTo(0));
    }

    [Test]
    public void Validate_DefinitionsAreValidAndInvalid_RemovesInvalidDefinitions()
    {
        _syncAssetsConfiguration!.Definitions.Add(new() { SourceDirectory = "C:\\Valid1\\Path", DestinationDirectory = "\\Server\\Valid1\\Path" });
        _syncAssetsConfiguration.Definitions.Add(new() { SourceDirectory = "Invalid\\Path", DestinationDirectory = "\\Server\\Valid\\Path" });

        _syncAssetsConfiguration.Validate();

        Assert.That(_syncAssetsConfiguration.Definitions, Has.Count.EqualTo(1));
        Assert.That(_syncAssetsConfiguration.Definitions[0].SourceDirectory, Is.EqualTo("C:\\Valid1\\Path"));
        Assert.That(_syncAssetsConfiguration.Definitions[0].DestinationDirectory, Is.EqualTo("\\Server\\Valid1\\Path"));
        Assert.That(_syncAssetsConfiguration.Definitions[0].IncludeSubFolders, Is.EqualTo(false));
        Assert.That(_syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource, Is.EqualTo(false));
    }

    [Test]
    public void Validate_DefinitionsAreInvalid_RemovesAllDefinitions()
    {
        _syncAssetsConfiguration!.Definitions.Add(new() { SourceDirectory = "Invalid1\\Path", DestinationDirectory = "Invalid2\\Path" });
        _syncAssetsConfiguration.Definitions.Add(new() { SourceDirectory = "https://www.some-site.com", DestinationDirectory = "ftp://some-location.com" });
        _syncAssetsConfiguration.Definitions.Add(new() { SourceDirectory = "Invalid@Value.com", DestinationDirectory = "Invalid@Value.com" });
        _syncAssetsConfiguration.Definitions.Add(new() { SourceDirectory = "", DestinationDirectory = " " });
        _syncAssetsConfiguration.Definitions.Add(new() { SourceDirectory = " ", DestinationDirectory = "" });
        _syncAssetsConfiguration.Definitions.Add(new() { SourceDirectory = "", DestinationDirectory = null! });

        _syncAssetsConfiguration.Validate();

        Assert.That(_syncAssetsConfiguration.Definitions.Count, Is.EqualTo(0));
    }

    [Test]
    [TestCase(null!, "")]
    [TestCase(null!, null!)]
    public void Validate_DefinitionsAreInvalidSourceAndDestinationAreNull_ThrowsArgumentNullExceptionAndDoesNotRemoveDefinitions(string sourceDirectory, string destinationDirectory)
    {
        _syncAssetsConfiguration!.Definitions.Add(new() { SourceDirectory = sourceDirectory, DestinationDirectory = destinationDirectory });

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _syncAssetsConfiguration.Validate());

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'input')"));
        Assert.That(_syncAssetsConfiguration.Definitions, Has.Count.EqualTo(1));
        Assert.That(_syncAssetsConfiguration.Definitions[0].SourceDirectory, Is.EqualTo(sourceDirectory));
        Assert.That(_syncAssetsConfiguration.Definitions[0].DestinationDirectory, Is.EqualTo(destinationDirectory));
        Assert.That(_syncAssetsConfiguration.Definitions[0].IncludeSubFolders, Is.EqualTo(false));
        Assert.That(_syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource, Is.EqualTo(false));
    }

    [Test]
    public void Normalize_PathsAreAlreadyNormalized_PathsAreNotModified()
    {
        SyncAssetsDirectoriesDefinition syncAssetsDirectoriesDefinition1 = new() { SourceDirectory = "C:\\Valid1\\Path", DestinationDirectory = "\\Remote\\Valid1\\Path" };
        SyncAssetsDirectoriesDefinition syncAssetsDirectoriesDefinition2 = new() { SourceDirectory = "C:\\Valid2\\Path", DestinationDirectory = "\\Remote\\Valid2\\Path" };

        _syncAssetsConfiguration!.Definitions.Add(syncAssetsDirectoriesDefinition1);
        _syncAssetsConfiguration!.Definitions.Add(syncAssetsDirectoriesDefinition2);

        _syncAssetsConfiguration.Normalize();

        Assert.That(_syncAssetsConfiguration.Definitions[0].SourceDirectory, Is.EqualTo(syncAssetsDirectoriesDefinition1.SourceDirectory));
        Assert.That(_syncAssetsConfiguration.Definitions[0].DestinationDirectory, Is.EqualTo(syncAssetsDirectoriesDefinition1.DestinationDirectory));
        Assert.That(_syncAssetsConfiguration.Definitions[0].IncludeSubFolders, Is.EqualTo(syncAssetsDirectoriesDefinition1.IncludeSubFolders));
        Assert.That(_syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource, Is.EqualTo(syncAssetsDirectoriesDefinition1.DeleteAssetsNotInSource));

        Assert.That(_syncAssetsConfiguration.Definitions[1].SourceDirectory, Is.EqualTo(syncAssetsDirectoriesDefinition2.SourceDirectory));
        Assert.That(_syncAssetsConfiguration.Definitions[1].DestinationDirectory, Is.EqualTo(syncAssetsDirectoriesDefinition2.DestinationDirectory));
        Assert.That(_syncAssetsConfiguration.Definitions[1].IncludeSubFolders, Is.EqualTo(syncAssetsDirectoriesDefinition2.IncludeSubFolders));
        Assert.That(_syncAssetsConfiguration.Definitions[1].DeleteAssetsNotInSource, Is.EqualTo(syncAssetsDirectoriesDefinition2.DeleteAssetsNotInSource));

        _syncAssetsConfiguration.Validate();

        Assert.That(_syncAssetsConfiguration.Definitions, Has.Count.EqualTo(2));
        Assert.That(_syncAssetsConfiguration.Definitions[0].SourceDirectory, Is.EqualTo(syncAssetsDirectoriesDefinition1.SourceDirectory));
        Assert.That(_syncAssetsConfiguration.Definitions[0].DestinationDirectory, Is.EqualTo(syncAssetsDirectoriesDefinition1.DestinationDirectory));
        Assert.That(_syncAssetsConfiguration.Definitions[0].IncludeSubFolders, Is.EqualTo(syncAssetsDirectoriesDefinition1.IncludeSubFolders));
        Assert.That(_syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource, Is.EqualTo(syncAssetsDirectoriesDefinition1.DeleteAssetsNotInSource));
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

        Assert.That(syncAssetsDirectoriesDefinition.SourceDirectory, Is.EqualTo("C:\\Some\\Extra\\Backslashes"));
        Assert.That(syncAssetsDirectoriesDefinition.DestinationDirectory, Is.EqualTo("\\Remote\\With\\Extra\\Backslashes"));
        Assert.That(syncAssetsDirectoriesDefinition.IncludeSubFolders, Is.EqualTo(false));
        Assert.That(syncAssetsDirectoriesDefinition.DeleteAssetsNotInSource, Is.EqualTo(false));

        _syncAssetsConfiguration.Validate();

        Assert.That(_syncAssetsConfiguration.Definitions, Has.Count.EqualTo(1));
        Assert.That(_syncAssetsConfiguration.Definitions[0].SourceDirectory, Is.EqualTo("C:\\Some\\Extra\\Backslashes"));
        Assert.That(_syncAssetsConfiguration.Definitions[0].DestinationDirectory, Is.EqualTo("\\Remote\\With\\Extra\\Backslashes"));
        Assert.That(_syncAssetsConfiguration.Definitions[0].IncludeSubFolders, Is.EqualTo(false));
        Assert.That(_syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource, Is.EqualTo(false));
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

        Assert.That(syncAssetsDirectoriesDefinition1.SourceDirectory, Is.EqualTo("C:\\Local\\Path"));
        Assert.That(syncAssetsDirectoriesDefinition1.DestinationDirectory, Is.EqualTo("C:\\Another\\Local\\Path"));
        Assert.That(syncAssetsDirectoriesDefinition1.IncludeSubFolders, Is.EqualTo(false));
        Assert.That(syncAssetsDirectoriesDefinition1.DeleteAssetsNotInSource, Is.EqualTo(false));

        Assert.That(syncAssetsDirectoriesDefinition2.SourceDirectory, Is.EqualTo("\\Server\\Remote\\Path"));
        Assert.That(syncAssetsDirectoriesDefinition2.DestinationDirectory, Is.EqualTo("\\Another\\Server\\Remote\\Path"));
        Assert.That(syncAssetsDirectoriesDefinition2.IncludeSubFolders, Is.EqualTo(false));
        Assert.That(syncAssetsDirectoriesDefinition2.DeleteAssetsNotInSource, Is.EqualTo(false));

        Assert.That(syncAssetsDirectoriesDefinition3.SourceDirectory, Is.EqualTo("C:\\Local\\And\\Remote\\Path"));
        Assert.That(syncAssetsDirectoriesDefinition3.DestinationDirectory, Is.EqualTo("\\Server\\And\\Another\\Local\\Path"));
        Assert.That(syncAssetsDirectoriesDefinition3.IncludeSubFolders, Is.EqualTo(false));
        Assert.That(syncAssetsDirectoriesDefinition3.DeleteAssetsNotInSource, Is.EqualTo(false));

        Assert.That(syncAssetsDirectoriesDefinition4.SourceDirectory, Is.EqualTo("\\Server\\And\\Another\\Local\\Path"));
        Assert.That(syncAssetsDirectoriesDefinition4.DestinationDirectory, Is.EqualTo("C:\\Local\\And\\Remote\\Path"));
        Assert.That(syncAssetsDirectoriesDefinition4.IncludeSubFolders, Is.EqualTo(false));
        Assert.That(syncAssetsDirectoriesDefinition4.DeleteAssetsNotInSource, Is.EqualTo(false));

        _syncAssetsConfiguration.Validate();

        Assert.That(_syncAssetsConfiguration.Definitions, Has.Count.EqualTo(4));
        Assert.That(_syncAssetsConfiguration.Definitions[0].SourceDirectory, Is.EqualTo("C:\\Local\\Path"));
        Assert.That(_syncAssetsConfiguration.Definitions[0].DestinationDirectory, Is.EqualTo("C:\\Another\\Local\\Path"));
        Assert.That(_syncAssetsConfiguration.Definitions[0].IncludeSubFolders, Is.EqualTo(false));
        Assert.That(_syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource, Is.EqualTo(false));

        Assert.That(_syncAssetsConfiguration.Definitions[1].SourceDirectory, Is.EqualTo("\\Server\\Remote\\Path"));
        Assert.That(_syncAssetsConfiguration.Definitions[1].DestinationDirectory, Is.EqualTo("\\Another\\Server\\Remote\\Path"));
        Assert.That(_syncAssetsConfiguration.Definitions[1].IncludeSubFolders, Is.EqualTo(false));
        Assert.That(_syncAssetsConfiguration.Definitions[1].DeleteAssetsNotInSource, Is.EqualTo(false));

        Assert.That(_syncAssetsConfiguration.Definitions[2].SourceDirectory, Is.EqualTo("C:\\Local\\And\\Remote\\Path"));
        Assert.That(_syncAssetsConfiguration.Definitions[2].DestinationDirectory, Is.EqualTo("\\Server\\And\\Another\\Local\\Path"));
        Assert.That(_syncAssetsConfiguration.Definitions[2].IncludeSubFolders, Is.EqualTo(false));
        Assert.That(_syncAssetsConfiguration.Definitions[2].DeleteAssetsNotInSource, Is.EqualTo(false));

        Assert.That(_syncAssetsConfiguration.Definitions[3].SourceDirectory, Is.EqualTo("\\Server\\And\\Another\\Local\\Path"));
        Assert.That(_syncAssetsConfiguration.Definitions[3].DestinationDirectory, Is.EqualTo("C:\\Local\\And\\Remote\\Path"));
        Assert.That(_syncAssetsConfiguration.Definitions[3].IncludeSubFolders, Is.EqualTo(false));
        Assert.That(_syncAssetsConfiguration.Definitions[3].DeleteAssetsNotInSource, Is.EqualTo(false));
    }

    [Test]
    [TestCase("", null!)]
    [TestCase(null!, "")]
    [TestCase(null!, null!)]
    public void Normalize_PathsAreInvalid_ThrowsArgumentNullExceptionAndDoesNotRemoveDefinitions(string sourceDirectory, string destinationDirectory)
    {
        SyncAssetsDirectoriesDefinition syncAssetsDirectoriesDefinition = new() { SourceDirectory = sourceDirectory, DestinationDirectory = destinationDirectory };

        _syncAssetsConfiguration!.Definitions.Add(syncAssetsDirectoriesDefinition);

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _syncAssetsConfiguration.Normalize());

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'input')"));
        Assert.That(_syncAssetsConfiguration.Definitions, Has.Count.EqualTo(1));
        Assert.That(_syncAssetsConfiguration.Definitions[0].SourceDirectory, Is.EqualTo(sourceDirectory));
        Assert.That(_syncAssetsConfiguration.Definitions[0].DestinationDirectory, Is.EqualTo(destinationDirectory));
        Assert.That(_syncAssetsConfiguration.Definitions[0].IncludeSubFolders, Is.EqualTo(false));
        Assert.That(_syncAssetsConfiguration.Definitions[0].DeleteAssetsNotInSource, Is.EqualTo(false));
    }

    [Test]
    public void Normalize_DefinitionsAreEmpty_DoesNothing()
    {
        _syncAssetsConfiguration!.Normalize();

        Assert.That(_syncAssetsConfiguration.Definitions.Count, Is.EqualTo(0));
    }
}
