using PhotoManager.UI.ViewModels.Enums;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using Directories = PhotoManager.Tests.Unit.Constants.Directories;

namespace PhotoManager.Tests.Unit.UI.ViewModels;

[TestFixture]
public class SyncAssetsViewModelTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;

    private SyncAssetsViewModel? _syncAssetsViewModel;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_dataDirectory, Directories.DATABASE_TESTS);
        _databasePath = Path.Combine(_databaseDirectory, Constants.DATABASE_END_PATH);
    }

    private void ConfigureSyncAssetsViewModel(int catalogBatchSize, string assetsDirectory, int thumbnailMaxWidth, int thumbnailMaxHeight, bool usingDHash, bool usingMD5Hash, bool usingPHash, bool analyseVideos)
    {
        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock.MockGetValue(UserConfigurationKeys.CATALOG_BATCH_SIZE, catalogBatchSize.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.ASSETS_DIRECTORY, assetsDirectory);
        configurationRootMock.MockGetValue(UserConfigurationKeys.THUMBNAIL_MAX_WIDTH, thumbnailMaxWidth.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.THUMBNAIL_MAX_HEIGHT, thumbnailMaxHeight.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.USING_DHASH, usingDHash.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.USING_MD5_HASH, usingMD5Hash.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.USING_PHASH, usingPHash.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.ANALYSE_VIDEOS, analyseVideos.ToString());

        UserConfigurationService userConfigurationService = new(configurationRootMock.Object);

        Mock<IStorageService> storageServiceMock = new();
        storageServiceMock.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_databasePath!);
        storageServiceMock.Setup(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new BitmapImage());

        Database database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        AssetRepository assetRepository = new(database, storageServiceMock.Object, userConfigurationService);
        StorageService storageService = new(userConfigurationService);
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService);
        AssetCreationService assetCreationService = new(assetRepository, storageService, assetHashCalculatorService, userConfigurationService);
        AssetsComparator assetsComparator = new();
        CatalogAssetsService catalogAssetsService = new(assetRepository, storageService, assetCreationService, userConfigurationService, assetsComparator);
        MoveAssetsService moveAssetsService = new(assetRepository, storageService, assetCreationService);
        SyncAssetsService syncAssetsService = new(assetRepository, storageService, assetsComparator, moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService = new(assetRepository, storageService, userConfigurationService);
        PhotoManager.Application.Application application = new(assetRepository, syncAssetsService, catalogAssetsService, moveAssetsService, findDuplicatedAssetsService, userConfigurationService, storageService);
        _syncAssetsViewModel = new(application);
    }

    [Test]
    public void Description_CorrectValue_ReturnsDescription()
    {
        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            const string description = """
                                       This process allows to sync new assets to the catalog.
                                       You can configure one or multiple sync operations by entering a source path and a destination path.
                                       You can specify if the sync operation should also include sub-folders.
                                       There is also the option to delete from the destination path the assets not present in the source path.
                                       """;

            string expectedDescription = description.Replace(Environment.NewLine, " ");

            Assert.That(_syncAssetsViewModel!.Description, Is.EqualTo(expectedDescription));

            CheckAfterChanges(_syncAssetsViewModel, []);

            Assert.That(notifyPropertyChangedEvents, Is.Empty);

            CheckInstance(syncAssetsViewModelInstances, []);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void Definitions_HasDefinitions_NotifiesChangesAndReturnsDefinitions()
    {
        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            List<SyncAssetsDirectoriesDefinition> definitions =
            [
                new() { SourceDirectory = @"C:\Toto\Screenshots", DestinationDirectory = @"C:\Images\Screenshots" },
                new() { SourceDirectory = @"\\MyServer\Images", DestinationDirectory = @"C:\Images" },
                new()
                {
                    SourceDirectory = @"C:\Folder\Screenshots",
                    DestinationDirectory = @"C:\Images\Folder3",
                    IncludeSubFolders = true,
                    DeleteAssetsNotInSource = true
                }
            ];

            _syncAssetsViewModel!.Definitions = [.. definitions];

            Assert.That(_syncAssetsViewModel!.Definitions, Has.Count.EqualTo(3));

            Assert.That(_syncAssetsViewModel!.Definitions[0].SourceDirectory, Is.EqualTo(@"C:\Toto\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[0].DestinationDirectory, Is.EqualTo(@"C:\Images\Screenshots"));
            Assert.That(_syncAssetsViewModel.Definitions[0].IncludeSubFolders, Is.False);
            Assert.That(_syncAssetsViewModel.Definitions[0].DeleteAssetsNotInSource, Is.False);

            Assert.That(_syncAssetsViewModel!.Definitions[1].SourceDirectory, Is.EqualTo(@"\\MyServer\Images"));
            Assert.That(_syncAssetsViewModel!.Definitions[1].DestinationDirectory, Is.EqualTo(@"C:\Images"));
            Assert.That(_syncAssetsViewModel.Definitions[1].IncludeSubFolders, Is.False);
            Assert.That(_syncAssetsViewModel.Definitions[1].DeleteAssetsNotInSource, Is.False);

            Assert.That(_syncAssetsViewModel!.Definitions[2].SourceDirectory, Is.EqualTo(@"C:\Folder\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[2].DestinationDirectory, Is.EqualTo(@"C:\Images\Folder3"));
            Assert.That(_syncAssetsViewModel.Definitions[2].IncludeSubFolders, Is.True);
            Assert.That(_syncAssetsViewModel.Definitions[2].DeleteAssetsNotInSource, Is.True);

            CheckAfterChanges(_syncAssetsViewModel, definitions);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(1));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Definitions"));

            CheckInstance(syncAssetsViewModelInstances, definitions);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void Definitions_SameDefinitions_NotifiesChangesAndReturnsDefinitions()
    {
        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            List<SyncAssetsDirectoriesDefinition> definitions =
            [
                new() { SourceDirectory = @"C:\Toto\Screenshots", DestinationDirectory = @"C:\Images\Screenshots" },
                new() { SourceDirectory = @"C:\Toto\Screenshots", DestinationDirectory = @"C:\Images\Screenshots" },
                new()
                {
                    SourceDirectory = @"C:\Folder\Screenshots",
                    DestinationDirectory = @"C:\Images\Folder3",
                    IncludeSubFolders = true,
                    DeleteAssetsNotInSource = true
                }
            ];

            _syncAssetsViewModel!.Definitions = [.. definitions];

            Assert.That(_syncAssetsViewModel!.Definitions, Has.Count.EqualTo(3));

            Assert.That(_syncAssetsViewModel!.Definitions[0].SourceDirectory, Is.EqualTo(@"C:\Toto\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[0].DestinationDirectory, Is.EqualTo(@"C:\Images\Screenshots"));
            Assert.That(_syncAssetsViewModel.Definitions[0].IncludeSubFolders, Is.False);
            Assert.That(_syncAssetsViewModel.Definitions[0].DeleteAssetsNotInSource, Is.False);

            Assert.That(_syncAssetsViewModel!.Definitions[1].SourceDirectory, Is.EqualTo(@"C:\Toto\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[1].DestinationDirectory, Is.EqualTo(@"C:\Images\Screenshots"));
            Assert.That(_syncAssetsViewModel.Definitions[1].IncludeSubFolders, Is.False);
            Assert.That(_syncAssetsViewModel.Definitions[1].DeleteAssetsNotInSource, Is.False);

            Assert.That(_syncAssetsViewModel!.Definitions[2].SourceDirectory, Is.EqualTo(@"C:\Folder\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[2].DestinationDirectory, Is.EqualTo(@"C:\Images\Folder3"));
            Assert.That(_syncAssetsViewModel.Definitions[2].IncludeSubFolders, Is.True);
            Assert.That(_syncAssetsViewModel.Definitions[2].DeleteAssetsNotInSource, Is.True);

            CheckAfterChanges(_syncAssetsViewModel, definitions);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(1));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Definitions"));

            CheckInstance(syncAssetsViewModelInstances, definitions);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void Definitions_EmptyDefinitions_NotifiesChangesAndReturnsEmptyCollection()
    {
        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            List<SyncAssetsDirectoriesDefinition> definitions = [];

            _syncAssetsViewModel!.Definitions = [.. definitions];

            Assert.That(_syncAssetsViewModel!.Definitions, Is.Empty);

            CheckAfterChanges(_syncAssetsViewModel, []);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(1));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Definitions"));

            CheckInstance(syncAssetsViewModelInstances, []);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void Definitions_NoDefinitions_DoesNotNotifyChangesAndReturnsEmptyCollection()
    {
        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Assert.That(_syncAssetsViewModel!.Definitions, Is.Empty);

            CheckAfterChanges(_syncAssetsViewModel, []);

            Assert.That(notifyPropertyChangedEvents, Is.Empty);

            CheckInstance(syncAssetsViewModelInstances, []);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void Definitions_DefinitionsIsNull_NotifiesChangesAndReturnsNull()
    {
        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> _) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            ObservableCollection<SyncAssetsDirectoriesDefinition> definitions = null!;

            _syncAssetsViewModel!.Definitions = definitions;

            Assert.That(_syncAssetsViewModel!.Definitions, Is.Null);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(1));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Definitions"));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void DeleteDefinition_HasDefinitions_NotifiesChangesAndDeletesDefinition()
    {
        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            List<SyncAssetsDirectoriesDefinition> definitions =
            [
                new() { SourceDirectory = @"C:\Toto\Screenshots", DestinationDirectory = @"C:\Images\Screenshots" },
                new() { SourceDirectory = @"C:\Tutu\Screenshots", DestinationDirectory = @"C:\Images\Tutu" },
                new()
                {
                    SourceDirectory = @"C:\Folder\Screenshots",
                    DestinationDirectory = @"C:\Images\Folder3",
                    IncludeSubFolders = true,
                    DeleteAssetsNotInSource = true
                }
            ];

            List<SyncAssetsDirectoriesDefinition> definitionsAfterFirstDeletion =
            [
                new() { SourceDirectory = @"C:\Toto\Screenshots", DestinationDirectory = @"C:\Images\Screenshots" },
                new()
                {
                    SourceDirectory = @"C:\Folder\Screenshots",
                    DestinationDirectory = @"C:\Images\Folder3",
                    IncludeSubFolders = true,
                    DeleteAssetsNotInSource = true
                }
            ];

            List<SyncAssetsDirectoriesDefinition> definitionsAfterSecondDeletion =
            [
                new() { SourceDirectory = @"C:\Toto\Screenshots", DestinationDirectory = @"C:\Images\Screenshots" }
            ];

            List<SyncAssetsDirectoriesDefinition> definitionsAfterThirdDeletion = [];

            _syncAssetsViewModel!.Definitions = [.. definitions];

            Assert.That(_syncAssetsViewModel.Definitions, Has.Count.EqualTo(3));

            // First deletion
            _syncAssetsViewModel.DeleteDefinition(_syncAssetsViewModel.Definitions[1]);

            Assert.That(_syncAssetsViewModel.Definitions, Has.Count.EqualTo(2));

            Assert.That(_syncAssetsViewModel.Definitions[0].SourceDirectory, Is.EqualTo(@"C:\Toto\Screenshots"));
            Assert.That(_syncAssetsViewModel.Definitions[0].DestinationDirectory, Is.EqualTo(@"C:\Images\Screenshots"));
            Assert.That(_syncAssetsViewModel.Definitions[0].IncludeSubFolders, Is.False);
            Assert.That(_syncAssetsViewModel.Definitions[0].DeleteAssetsNotInSource, Is.False);

            Assert.That(_syncAssetsViewModel.Definitions[1].SourceDirectory, Is.EqualTo(@"C:\Folder\Screenshots"));
            Assert.That(_syncAssetsViewModel.Definitions[1].DestinationDirectory, Is.EqualTo(@"C:\Images\Folder3"));
            Assert.That(_syncAssetsViewModel.Definitions[1].IncludeSubFolders, Is.True);
            Assert.That(_syncAssetsViewModel.Definitions[1].DeleteAssetsNotInSource, Is.True);

            CheckAfterChanges(_syncAssetsViewModel, definitionsAfterFirstDeletion);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(2));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Definitions"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("Definitions"));

            CheckInstance(syncAssetsViewModelInstances, definitionsAfterFirstDeletion);

            // Second deletion
            _syncAssetsViewModel.DeleteDefinition(_syncAssetsViewModel.Definitions[1]);

            Assert.That(_syncAssetsViewModel.Definitions, Has.Count.EqualTo(1));

            Assert.That(_syncAssetsViewModel.Definitions[0].SourceDirectory, Is.EqualTo(@"C:\Toto\Screenshots"));
            Assert.That(_syncAssetsViewModel.Definitions[0].DestinationDirectory, Is.EqualTo(@"C:\Images\Screenshots"));
            Assert.That(_syncAssetsViewModel.Definitions[0].IncludeSubFolders, Is.False);
            Assert.That(_syncAssetsViewModel.Definitions[0].DeleteAssetsNotInSource, Is.False);

            CheckAfterChanges(_syncAssetsViewModel, definitionsAfterSecondDeletion);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(3));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Definitions"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("Definitions"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("Definitions"));

            CheckInstance(syncAssetsViewModelInstances, definitionsAfterSecondDeletion);

            // Third deletion
            _syncAssetsViewModel.DeleteDefinition(_syncAssetsViewModel.Definitions[0]);

            Assert.That(_syncAssetsViewModel.Definitions, Is.Empty);

            CheckAfterChanges(_syncAssetsViewModel, definitionsAfterThirdDeletion);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(4));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Definitions"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("Definitions"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("Definitions"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("Definitions"));

            CheckInstance(syncAssetsViewModelInstances, definitionsAfterThirdDeletion);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void DeleteDefinition_DuplicateDefinitionsInTheCollection_NotifiesChangesAndDeletesGivenDefinition()
    {
        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            List<SyncAssetsDirectoriesDefinition> definitions =
            [
                new() { SourceDirectory = @"C:\Toto\Screenshots", DestinationDirectory = @"C:\Images\Screenshots" },
                new()
                {
                    SourceDirectory = @"C:\Folder\Screenshots",
                    DestinationDirectory = @"C:\Images\Folder3",
                    IncludeSubFolders = true,
                    DeleteAssetsNotInSource = true
                },
                new() { SourceDirectory = @"C:\Toto\Screenshots", DestinationDirectory = @"C:\Images\Screenshots" }
            ];

            List<SyncAssetsDirectoriesDefinition> definitionsAfterFirstDeletion =
            [
                new() { SourceDirectory = @"C:\Toto\Screenshots", DestinationDirectory = @"C:\Images\Screenshots" },
                new()
                {
                    SourceDirectory = @"C:\Folder\Screenshots",
                    DestinationDirectory = @"C:\Images\Folder3",
                    IncludeSubFolders = true,
                    DeleteAssetsNotInSource = true
                }
            ];

            _syncAssetsViewModel!.Definitions = [.. definitions];

            Assert.That(_syncAssetsViewModel.Definitions, Has.Count.EqualTo(3));

            _syncAssetsViewModel.DeleteDefinition(_syncAssetsViewModel.Definitions[2]);

            Assert.That(_syncAssetsViewModel.Definitions, Has.Count.EqualTo(2));

            Assert.That(_syncAssetsViewModel.Definitions[0].SourceDirectory, Is.EqualTo(@"C:\Toto\Screenshots"));
            Assert.That(_syncAssetsViewModel.Definitions[0].DestinationDirectory, Is.EqualTo(@"C:\Images\Screenshots"));
            Assert.That(_syncAssetsViewModel.Definitions[0].IncludeSubFolders, Is.False);
            Assert.That(_syncAssetsViewModel.Definitions[0].DeleteAssetsNotInSource, Is.False);

            Assert.That(_syncAssetsViewModel.Definitions[1].SourceDirectory, Is.EqualTo(@"C:\Folder\Screenshots"));
            Assert.That(_syncAssetsViewModel.Definitions[1].DestinationDirectory, Is.EqualTo(@"C:\Images\Folder3"));
            Assert.That(_syncAssetsViewModel.Definitions[1].IncludeSubFolders, Is.True);
            Assert.That(_syncAssetsViewModel.Definitions[1].DeleteAssetsNotInSource, Is.True);

            CheckAfterChanges(_syncAssetsViewModel, definitionsAfterFirstDeletion);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(2));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Definitions"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("Definitions"));

            CheckInstance(syncAssetsViewModelInstances, definitionsAfterFirstDeletion);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void DeleteDefinition_NoDefinitions_DoesNothing()
    {
        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Assert.That(_syncAssetsViewModel!.Definitions, Is.Empty);

            _syncAssetsViewModel.DeleteDefinition(new() { SourceDirectory = @"C:\Toto\Screenshots", DestinationDirectory = @"C:\Images\Screenshots" });

            Assert.That(_syncAssetsViewModel!.Definitions, Is.Empty);

            CheckAfterChanges(_syncAssetsViewModel, []);

            Assert.That(notifyPropertyChangedEvents, Is.Empty);

            CheckInstance(syncAssetsViewModelInstances, []);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void DeleteDefinition_DefinitionIsNotFound_DoesNothing()
    {
        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            List<SyncAssetsDirectoriesDefinition> definitions =
            [
                new() { SourceDirectory = @"C:\Tutu\Screenshots", DestinationDirectory = @"C:\Images\Tutu" },
                new()
                {
                    SourceDirectory = @"C:\Folder\Screenshots",
                    DestinationDirectory = @"C:\Images\Folder3",
                    IncludeSubFolders = true,
                    DeleteAssetsNotInSource = true
                }
            ];

            _syncAssetsViewModel!.Definitions = [.. definitions];

            Assert.That(_syncAssetsViewModel.Definitions, Has.Count.EqualTo(2));

            _syncAssetsViewModel.DeleteDefinition(new() { SourceDirectory = @"C:\Toto\Screenshots", DestinationDirectory = @"C:\Images\Screenshots" });

            Assert.That(_syncAssetsViewModel.Definitions, Has.Count.EqualTo(2));

            Assert.That(_syncAssetsViewModel!.Definitions[0].SourceDirectory, Is.EqualTo(@"C:\Tutu\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[0].DestinationDirectory, Is.EqualTo(@"C:\Images\Tutu"));
            Assert.That(_syncAssetsViewModel!.Definitions[0].IncludeSubFolders, Is.False);
            Assert.That(_syncAssetsViewModel!.Definitions[0].DeleteAssetsNotInSource, Is.False);

            Assert.That(_syncAssetsViewModel!.Definitions[1].SourceDirectory, Is.EqualTo(@"C:\Folder\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[1].DestinationDirectory, Is.EqualTo(@"C:\Images\Folder3"));
            Assert.That(_syncAssetsViewModel!.Definitions[1].IncludeSubFolders, Is.True);
            Assert.That(_syncAssetsViewModel!.Definitions[1].DeleteAssetsNotInSource, Is.True);

            CheckAfterChanges(_syncAssetsViewModel, definitions);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(1));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Definitions"));

            CheckInstance(syncAssetsViewModelInstances, definitions);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void DeleteDefinition_DefinitionIsNull_DoesNothing()
    {
        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            List<SyncAssetsDirectoriesDefinition> definitions =
            [
                new() { SourceDirectory = @"C:\Toto\Screenshots", DestinationDirectory = @"C:\Images\Screenshots" },
                new() { SourceDirectory = @"C:\Tutu\Screenshots", DestinationDirectory = @"C:\Images\Tutu" },
                new()
                {
                    SourceDirectory = @"C:\Folder\Screenshots",
                    DestinationDirectory = @"C:\Images\Folder3",
                    IncludeSubFolders = true,
                    DeleteAssetsNotInSource = true
                }
            ];

            _syncAssetsViewModel!.Definitions = [.. definitions];

            Assert.That(_syncAssetsViewModel.Definitions, Has.Count.EqualTo(3));

            _syncAssetsViewModel.DeleteDefinition(null!);

            Assert.That(_syncAssetsViewModel.Definitions, Has.Count.EqualTo(3));

            Assert.That(_syncAssetsViewModel!.Definitions[0].SourceDirectory, Is.EqualTo(@"C:\Toto\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[0].DestinationDirectory, Is.EqualTo(@"C:\Images\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[0].IncludeSubFolders, Is.False);
            Assert.That(_syncAssetsViewModel!.Definitions[0].DeleteAssetsNotInSource, Is.False);

            Assert.That(_syncAssetsViewModel!.Definitions[1].SourceDirectory, Is.EqualTo(@"C:\Tutu\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[1].DestinationDirectory, Is.EqualTo(@"C:\Images\Tutu"));
            Assert.That(_syncAssetsViewModel!.Definitions[1].IncludeSubFolders, Is.False);
            Assert.That(_syncAssetsViewModel!.Definitions[1].DeleteAssetsNotInSource, Is.False);

            Assert.That(_syncAssetsViewModel!.Definitions[2].SourceDirectory, Is.EqualTo(@"C:\Folder\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[2].DestinationDirectory, Is.EqualTo(@"C:\Images\Folder3"));
            Assert.That(_syncAssetsViewModel!.Definitions[2].IncludeSubFolders, Is.True);
            Assert.That(_syncAssetsViewModel!.Definitions[2].DeleteAssetsNotInSource, Is.True);

            CheckAfterChanges(_syncAssetsViewModel, definitions);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(1));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Definitions"));

            CheckInstance(syncAssetsViewModelInstances, definitions);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void MoveUpAndMoveDown_HasDefinitions_UpdatesDefinitions()
    {
        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            List<SyncAssetsDirectoriesDefinition> definitions =
            [
                new() { SourceDirectory = @"C:\Toto\Screenshots", DestinationDirectory = @"C:\Images\Screenshots" },
                new() { SourceDirectory = @"C:\Tutu\Screenshots", DestinationDirectory = @"C:\Images\Tutu" },
                new()
                {
                    SourceDirectory = @"C:\Folder\Screenshots",
                    DestinationDirectory = @"C:\Images\Folder3",
                    IncludeSubFolders = true,
                    DeleteAssetsNotInSource = true
                }
            ];

            List<SyncAssetsDirectoriesDefinition> definitionsAfterMovedUpAndDown =
            [
                new() { SourceDirectory = @"C:\Tutu\Screenshots", DestinationDirectory = @"C:\Images\Tutu" },
                new()
                {
                    SourceDirectory = @"C:\Folder\Screenshots",
                    DestinationDirectory = @"C:\Images\Folder3",
                    IncludeSubFolders = true,
                    DeleteAssetsNotInSource = true
                },
                new() { SourceDirectory = @"C:\Toto\Screenshots", DestinationDirectory = @"C:\Images\Screenshots" }
            ];

            _syncAssetsViewModel!.Definitions = [.. definitions];

            Assert.That(_syncAssetsViewModel.Definitions, Has.Count.EqualTo(3));

            _syncAssetsViewModel!.MoveUpDefinition(_syncAssetsViewModel.Definitions[1]);

            Assert.That(_syncAssetsViewModel.Definitions, Has.Count.EqualTo(3));

            Assert.That(_syncAssetsViewModel!.Definitions[0].SourceDirectory, Is.EqualTo(@"C:\Tutu\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[0].DestinationDirectory, Is.EqualTo(@"C:\Images\Tutu"));
            Assert.That(_syncAssetsViewModel!.Definitions[0].IncludeSubFolders, Is.False);
            Assert.That(_syncAssetsViewModel!.Definitions[0].DeleteAssetsNotInSource, Is.False);

            Assert.That(_syncAssetsViewModel!.Definitions[1].SourceDirectory, Is.EqualTo(@"C:\Toto\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[1].DestinationDirectory, Is.EqualTo(@"C:\Images\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[1].IncludeSubFolders, Is.False);
            Assert.That(_syncAssetsViewModel!.Definitions[1].DeleteAssetsNotInSource, Is.False);

            Assert.That(_syncAssetsViewModel!.Definitions[2].SourceDirectory, Is.EqualTo(@"C:\Folder\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[2].DestinationDirectory, Is.EqualTo(@"C:\Images\Folder3"));
            Assert.That(_syncAssetsViewModel!.Definitions[2].IncludeSubFolders, Is.True);
            Assert.That(_syncAssetsViewModel!.Definitions[2].DeleteAssetsNotInSource, Is.True);

            _syncAssetsViewModel!.MoveDownDefinition(_syncAssetsViewModel.Definitions[1]);

            Assert.That(_syncAssetsViewModel.Definitions, Has.Count.EqualTo(3));

            Assert.That(_syncAssetsViewModel!.Definitions[0].SourceDirectory, Is.EqualTo(@"C:\Tutu\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[0].DestinationDirectory, Is.EqualTo(@"C:\Images\Tutu"));
            Assert.That(_syncAssetsViewModel!.Definitions[0].IncludeSubFolders, Is.False);
            Assert.That(_syncAssetsViewModel!.Definitions[0].DeleteAssetsNotInSource, Is.False);

            Assert.That(_syncAssetsViewModel!.Definitions[1].SourceDirectory, Is.EqualTo(@"C:\Folder\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[1].DestinationDirectory, Is.EqualTo(@"C:\Images\Folder3"));
            Assert.That(_syncAssetsViewModel!.Definitions[1].IncludeSubFolders, Is.True);
            Assert.That(_syncAssetsViewModel!.Definitions[1].DeleteAssetsNotInSource, Is.True);

            Assert.That(_syncAssetsViewModel!.Definitions[2].SourceDirectory, Is.EqualTo(@"C:\Toto\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[2].DestinationDirectory, Is.EqualTo(@"C:\Images\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[2].IncludeSubFolders, Is.False);
            Assert.That(_syncAssetsViewModel!.Definitions[2].DeleteAssetsNotInSource, Is.False);

            CheckAfterChanges(_syncAssetsViewModel, definitionsAfterMovedUpAndDown);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(1));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Definitions"));

            CheckInstance(syncAssetsViewModelInstances, definitionsAfterMovedUpAndDown);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void MoveUp_FirstDefinition_DoesNotMoveUpDefinition()
    {
        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            List<SyncAssetsDirectoriesDefinition> definitions =
            [
                new() { SourceDirectory = @"C:\Toto\Screenshots", DestinationDirectory = @"C:\Images\Screenshots" },
                new() { SourceDirectory = @"C:\Tutu\Screenshots", DestinationDirectory = @"C:\Images\Tutu" },
                new()
                {
                    SourceDirectory = @"C:\Folder\Screenshots",
                    DestinationDirectory = @"C:\Images\Folder3",
                    IncludeSubFolders = true,
                    DeleteAssetsNotInSource = true
                }
            ];

            _syncAssetsViewModel!.Definitions = [.. definitions];

            Assert.That(_syncAssetsViewModel.Definitions, Has.Count.EqualTo(3));

            _syncAssetsViewModel!.MoveUpDefinition(_syncAssetsViewModel.Definitions[0]);

            Assert.That(_syncAssetsViewModel.Definitions, Has.Count.EqualTo(3));

            Assert.That(_syncAssetsViewModel!.Definitions[0].SourceDirectory, Is.EqualTo(@"C:\Toto\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[0].DestinationDirectory, Is.EqualTo(@"C:\Images\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[0].IncludeSubFolders, Is.False);
            Assert.That(_syncAssetsViewModel!.Definitions[0].DeleteAssetsNotInSource, Is.False);

            Assert.That(_syncAssetsViewModel!.Definitions[1].SourceDirectory, Is.EqualTo(@"C:\Tutu\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[1].DestinationDirectory, Is.EqualTo(@"C:\Images\Tutu"));
            Assert.That(_syncAssetsViewModel!.Definitions[1].IncludeSubFolders, Is.False);
            Assert.That(_syncAssetsViewModel!.Definitions[1].DeleteAssetsNotInSource, Is.False);

            Assert.That(_syncAssetsViewModel!.Definitions[2].SourceDirectory, Is.EqualTo(@"C:\Folder\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[2].DestinationDirectory, Is.EqualTo(@"C:\Images\Folder3"));
            Assert.That(_syncAssetsViewModel!.Definitions[2].IncludeSubFolders, Is.True);
            Assert.That(_syncAssetsViewModel!.Definitions[2].DeleteAssetsNotInSource, Is.True);

            CheckAfterChanges(_syncAssetsViewModel, definitions);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(1));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Definitions"));

            CheckInstance(syncAssetsViewModelInstances, definitions);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void MoveUp_NotFirstAndNotLastDefinition_MovesUpDefinition()
    {
        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            List<SyncAssetsDirectoriesDefinition> definitions =
            [
                new() { SourceDirectory = @"C:\Toto\Screenshots", DestinationDirectory = @"C:\Images\Screenshots" },
                new() { SourceDirectory = @"C:\Tutu\Screenshots", DestinationDirectory = @"C:\Images\Tutu" },
                new()
                {
                    SourceDirectory = @"C:\Folder\Screenshots",
                    DestinationDirectory = @"C:\Images\Folder3",
                    IncludeSubFolders = true,
                    DeleteAssetsNotInSource = true
                }
            ];

            List<SyncAssetsDirectoriesDefinition> definitionsAfterMovedUp =
            [
                new() { SourceDirectory = @"C:\Tutu\Screenshots", DestinationDirectory = @"C:\Images\Tutu" },
                new() { SourceDirectory = @"C:\Toto\Screenshots", DestinationDirectory = @"C:\Images\Screenshots" },
                new()
                {
                    SourceDirectory = @"C:\Folder\Screenshots",
                    DestinationDirectory = @"C:\Images\Folder3",
                    IncludeSubFolders = true,
                    DeleteAssetsNotInSource = true
                }
            ];

            _syncAssetsViewModel!.Definitions = [.. definitions];

            Assert.That(_syncAssetsViewModel.Definitions, Has.Count.EqualTo(3));

            _syncAssetsViewModel!.MoveUpDefinition(_syncAssetsViewModel.Definitions[1]);

            Assert.That(_syncAssetsViewModel.Definitions, Has.Count.EqualTo(3));

            Assert.That(_syncAssetsViewModel!.Definitions[0].SourceDirectory, Is.EqualTo(@"C:\Tutu\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[0].DestinationDirectory, Is.EqualTo(@"C:\Images\Tutu"));
            Assert.That(_syncAssetsViewModel!.Definitions[0].IncludeSubFolders, Is.False);
            Assert.That(_syncAssetsViewModel!.Definitions[0].DeleteAssetsNotInSource, Is.False);

            Assert.That(_syncAssetsViewModel!.Definitions[1].SourceDirectory, Is.EqualTo(@"C:\Toto\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[1].DestinationDirectory, Is.EqualTo(@"C:\Images\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[1].IncludeSubFolders, Is.False);
            Assert.That(_syncAssetsViewModel!.Definitions[1].DeleteAssetsNotInSource, Is.False);

            Assert.That(_syncAssetsViewModel!.Definitions[2].SourceDirectory, Is.EqualTo(@"C:\Folder\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[2].DestinationDirectory, Is.EqualTo(@"C:\Images\Folder3"));
            Assert.That(_syncAssetsViewModel!.Definitions[2].IncludeSubFolders, Is.True);
            Assert.That(_syncAssetsViewModel!.Definitions[2].DeleteAssetsNotInSource, Is.True);

            CheckAfterChanges(_syncAssetsViewModel, definitionsAfterMovedUp);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(1));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Definitions"));

            CheckInstance(syncAssetsViewModelInstances, definitionsAfterMovedUp);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void MoveUp_LastDefinition_MovesUpDefinition()
    {
        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            List<SyncAssetsDirectoriesDefinition> definitions =
            [
                new() { SourceDirectory = @"C:\Toto\Screenshots", DestinationDirectory = @"C:\Images\Screenshots" },
                new() { SourceDirectory = @"C:\Tutu\Screenshots", DestinationDirectory = @"C:\Images\Tutu" },
                new()
                {
                    SourceDirectory = @"C:\Folder\Screenshots",
                    DestinationDirectory = @"C:\Images\Folder3",
                    IncludeSubFolders = true,
                    DeleteAssetsNotInSource = true
                }
            ];

            List<SyncAssetsDirectoriesDefinition> definitionsAfterMovedUp =
            [
                new() { SourceDirectory = @"C:\Toto\Screenshots", DestinationDirectory = @"C:\Images\Screenshots" },
                new()
                {
                    SourceDirectory = @"C:\Folder\Screenshots",
                    DestinationDirectory = @"C:\Images\Folder3",
                    IncludeSubFolders = true,
                    DeleteAssetsNotInSource = true
                },
                new() { SourceDirectory = @"C:\Tutu\Screenshots", DestinationDirectory = @"C:\Images\Tutu" }
            ];

            _syncAssetsViewModel!.Definitions = [.. definitions];

            Assert.That(_syncAssetsViewModel.Definitions, Has.Count.EqualTo(3));

            _syncAssetsViewModel!.MoveUpDefinition(_syncAssetsViewModel.Definitions[2]);

            Assert.That(_syncAssetsViewModel.Definitions, Has.Count.EqualTo(3));

            Assert.That(_syncAssetsViewModel!.Definitions[0].SourceDirectory, Is.EqualTo(@"C:\Toto\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[0].DestinationDirectory, Is.EqualTo(@"C:\Images\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[0].IncludeSubFolders, Is.False);
            Assert.That(_syncAssetsViewModel!.Definitions[0].DeleteAssetsNotInSource, Is.False);

            Assert.That(_syncAssetsViewModel!.Definitions[1].SourceDirectory, Is.EqualTo(@"C:\Folder\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[1].DestinationDirectory, Is.EqualTo(@"C:\Images\Folder3"));
            Assert.That(_syncAssetsViewModel!.Definitions[1].IncludeSubFolders, Is.True);
            Assert.That(_syncAssetsViewModel!.Definitions[1].DeleteAssetsNotInSource, Is.True);

            Assert.That(_syncAssetsViewModel!.Definitions[2].SourceDirectory, Is.EqualTo(@"C:\Tutu\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[2].DestinationDirectory, Is.EqualTo(@"C:\Images\Tutu"));
            Assert.That(_syncAssetsViewModel!.Definitions[2].IncludeSubFolders, Is.False);
            Assert.That(_syncAssetsViewModel!.Definitions[2].DeleteAssetsNotInSource, Is.False);

            CheckAfterChanges(_syncAssetsViewModel, definitionsAfterMovedUp);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(1));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Definitions"));

            CheckInstance(syncAssetsViewModelInstances, definitionsAfterMovedUp);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void MoveUp_DuplicateDefinitionsInTheCollection_MovesUpGivenDefinition()
    {
        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            List<SyncAssetsDirectoriesDefinition> definitions =
            [
                new() { SourceDirectory = @"C:\Toto\Screenshots", DestinationDirectory = @"C:\Images\Screenshots" },
                new()
                {
                    SourceDirectory = @"C:\Folder\Screenshots",
                    DestinationDirectory = @"C:\Images\Folder3",
                    IncludeSubFolders = true,
                    DeleteAssetsNotInSource = true
                },
                new() { SourceDirectory = @"C:\Toto\Screenshots", DestinationDirectory = @"C:\Images\Screenshots" }
            ];

            List<SyncAssetsDirectoriesDefinition> definitionsAfterMovedUp =
            [
                new() { SourceDirectory = @"C:\Toto\Screenshots", DestinationDirectory = @"C:\Images\Screenshots" },
                new() { SourceDirectory = @"C:\Toto\Screenshots", DestinationDirectory = @"C:\Images\Screenshots" },
                new()
                {
                    SourceDirectory = @"C:\Folder\Screenshots",
                    DestinationDirectory = @"C:\Images\Folder3",
                    IncludeSubFolders = true,
                    DeleteAssetsNotInSource = true
                }
            ];

            _syncAssetsViewModel!.Definitions = [.. definitions];

            Assert.That(_syncAssetsViewModel.Definitions, Has.Count.EqualTo(3));

            _syncAssetsViewModel!.MoveUpDefinition(_syncAssetsViewModel.Definitions[2]);

            Assert.That(_syncAssetsViewModel.Definitions, Has.Count.EqualTo(3));

            Assert.That(_syncAssetsViewModel!.Definitions[0].SourceDirectory, Is.EqualTo(@"C:\Toto\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[0].DestinationDirectory, Is.EqualTo(@"C:\Images\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[0].IncludeSubFolders, Is.False);
            Assert.That(_syncAssetsViewModel!.Definitions[0].DeleteAssetsNotInSource, Is.False);

            Assert.That(_syncAssetsViewModel!.Definitions[1].SourceDirectory, Is.EqualTo(@"C:\Toto\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[1].DestinationDirectory, Is.EqualTo(@"C:\Images\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[1].IncludeSubFolders, Is.False);
            Assert.That(_syncAssetsViewModel!.Definitions[1].DeleteAssetsNotInSource, Is.False);

            Assert.That(_syncAssetsViewModel!.Definitions[2].SourceDirectory, Is.EqualTo(@"C:\Folder\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[2].DestinationDirectory, Is.EqualTo(@"C:\Images\Folder3"));
            Assert.That(_syncAssetsViewModel!.Definitions[2].IncludeSubFolders, Is.True);
            Assert.That(_syncAssetsViewModel!.Definitions[2].DeleteAssetsNotInSource, Is.True);

            CheckAfterChanges(_syncAssetsViewModel, definitionsAfterMovedUp);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(1));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Definitions"));

            CheckInstance(syncAssetsViewModelInstances, definitionsAfterMovedUp);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void MoveUp_OnlyOneDefinition_DoesNothing()
    {
        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            List<SyncAssetsDirectoriesDefinition> definitions =
            [
                new() { SourceDirectory = @"C:\Toto\Screenshots", DestinationDirectory = @"C:\Images\Screenshots" }
            ];

            _syncAssetsViewModel!.Definitions = [.. definitions];

            Assert.That(_syncAssetsViewModel.Definitions, Has.Count.EqualTo(1));

            _syncAssetsViewModel!.MoveUpDefinition(_syncAssetsViewModel.Definitions[0]);

            Assert.That(_syncAssetsViewModel.Definitions, Has.Count.EqualTo(1));

            Assert.That(_syncAssetsViewModel!.Definitions[0].SourceDirectory, Is.EqualTo(@"C:\Toto\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[0].DestinationDirectory, Is.EqualTo(@"C:\Images\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[0].IncludeSubFolders, Is.False);
            Assert.That(_syncAssetsViewModel!.Definitions[0].DeleteAssetsNotInSource, Is.False);

            CheckAfterChanges(_syncAssetsViewModel, definitions);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(1));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Definitions"));

            CheckInstance(syncAssetsViewModelInstances, definitions);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void MoveUp_DefinitionIsNotFound_DoesNothing()
    {
        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            List<SyncAssetsDirectoriesDefinition> definitions =
            [
                new() { SourceDirectory = @"C:\Toto\Screenshots", DestinationDirectory = @"C:\Images\Screenshots" },
                new() { SourceDirectory = @"C:\Tutu\Screenshots", DestinationDirectory = @"C:\Images\Tutu" },
                new()
                {
                    SourceDirectory = @"C:\Folder\Screenshots",
                    DestinationDirectory = @"C:\Images\Folder3",
                    IncludeSubFolders = true,
                    DeleteAssetsNotInSource = true
                }
            ];

            _syncAssetsViewModel!.Definitions = [.. definitions];

            Assert.That(_syncAssetsViewModel.Definitions, Has.Count.EqualTo(3));

            _syncAssetsViewModel!.MoveUpDefinition(new() { SourceDirectory = @"C:\Toto\Screenshots1", DestinationDirectory = @"C:\Toto\Screenshots2" });

            Assert.That(_syncAssetsViewModel.Definitions, Has.Count.EqualTo(3));

            Assert.That(_syncAssetsViewModel!.Definitions[0].SourceDirectory, Is.EqualTo(@"C:\Toto\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[0].DestinationDirectory, Is.EqualTo(@"C:\Images\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[0].IncludeSubFolders, Is.False);
            Assert.That(_syncAssetsViewModel!.Definitions[0].DeleteAssetsNotInSource, Is.False);

            Assert.That(_syncAssetsViewModel!.Definitions[1].SourceDirectory, Is.EqualTo(@"C:\Tutu\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[1].DestinationDirectory, Is.EqualTo(@"C:\Images\Tutu"));
            Assert.That(_syncAssetsViewModel!.Definitions[1].IncludeSubFolders, Is.False);
            Assert.That(_syncAssetsViewModel!.Definitions[1].DeleteAssetsNotInSource, Is.False);

            Assert.That(_syncAssetsViewModel!.Definitions[2].SourceDirectory, Is.EqualTo(@"C:\Folder\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[2].DestinationDirectory, Is.EqualTo(@"C:\Images\Folder3"));
            Assert.That(_syncAssetsViewModel!.Definitions[2].IncludeSubFolders, Is.True);
            Assert.That(_syncAssetsViewModel!.Definitions[2].DeleteAssetsNotInSource, Is.True);

            CheckAfterChanges(_syncAssetsViewModel, definitions);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(1));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Definitions"));

            CheckInstance(syncAssetsViewModelInstances, definitions);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void MoveUp_NoDefinitions_DoesNothing()
    {
        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            List<SyncAssetsDirectoriesDefinition> definitions = [];

            _syncAssetsViewModel!.Definitions = [.. definitions];

            Assert.That(_syncAssetsViewModel.Definitions, Is.Empty);

            _syncAssetsViewModel!.MoveUpDefinition(new() { SourceDirectory = @"C:\Toto\Screenshots1", DestinationDirectory = @"C:\Toto\Screenshots2" });

            Assert.That(_syncAssetsViewModel.Definitions, Is.Empty);

            CheckAfterChanges(_syncAssetsViewModel, definitions);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(1));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Definitions"));

            CheckInstance(syncAssetsViewModelInstances, definitions);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void MoveUp_DefinitionsIsNull_ThrowsNullReferenceException()
    {
        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> _) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            ObservableCollection<SyncAssetsDirectoriesDefinition> definitions = null!;

            _syncAssetsViewModel!.Definitions = definitions;

            Assert.That(_syncAssetsViewModel!.Definitions, Is.Null);

            NullReferenceException? exception = Assert.Throws<NullReferenceException>(() =>
            {
                _syncAssetsViewModel!.MoveUpDefinition(
                    new() { SourceDirectory = @"C:\Toto\Screenshots1", DestinationDirectory = @"C:\Toto\Screenshots2" }
                );
            });

            Assert.That(exception?.Message, Is.EqualTo("Object reference not set to an instance of an object."));

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(1));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Definitions"));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void MoveUp_DefinitionIsNull_DoesNothing()
    {
        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            List<SyncAssetsDirectoriesDefinition> definitions =
            [
                new() { SourceDirectory = @"C:\Toto\Screenshots", DestinationDirectory = @"C:\Images\Screenshots" },
                new() { SourceDirectory = @"C:\Tutu\Screenshots", DestinationDirectory = @"C:\Images\Tutu" },
                new()
                {
                    SourceDirectory = @"C:\Folder\Screenshots",
                    DestinationDirectory = @"C:\Images\Folder3",
                    IncludeSubFolders = true,
                    DeleteAssetsNotInSource = true
                }
            ];

            _syncAssetsViewModel!.Definitions = [.. definitions];

            Assert.That(_syncAssetsViewModel.Definitions, Has.Count.EqualTo(3));

            _syncAssetsViewModel!.MoveUpDefinition(null!);

            Assert.That(_syncAssetsViewModel.Definitions, Has.Count.EqualTo(3));

            Assert.That(_syncAssetsViewModel!.Definitions[0].SourceDirectory, Is.EqualTo(@"C:\Toto\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[0].DestinationDirectory, Is.EqualTo(@"C:\Images\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[0].IncludeSubFolders, Is.False);
            Assert.That(_syncAssetsViewModel!.Definitions[0].DeleteAssetsNotInSource, Is.False);

            Assert.That(_syncAssetsViewModel!.Definitions[1].SourceDirectory, Is.EqualTo(@"C:\Tutu\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[1].DestinationDirectory, Is.EqualTo(@"C:\Images\Tutu"));
            Assert.That(_syncAssetsViewModel!.Definitions[1].IncludeSubFolders, Is.False);
            Assert.That(_syncAssetsViewModel!.Definitions[1].DeleteAssetsNotInSource, Is.False);

            Assert.That(_syncAssetsViewModel!.Definitions[2].SourceDirectory, Is.EqualTo(@"C:\Folder\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[2].DestinationDirectory, Is.EqualTo(@"C:\Images\Folder3"));
            Assert.That(_syncAssetsViewModel!.Definitions[2].IncludeSubFolders, Is.True);
            Assert.That(_syncAssetsViewModel!.Definitions[2].DeleteAssetsNotInSource, Is.True);

            CheckAfterChanges(_syncAssetsViewModel, definitions);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(1));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Definitions"));

            CheckInstance(syncAssetsViewModelInstances, definitions);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void MoveDown_FirstAndNotLastDefinition_MovesDownDefinition()
    {
        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            List<SyncAssetsDirectoriesDefinition> definitions =
            [
                new() { SourceDirectory = @"C:\Toto\Screenshots", DestinationDirectory = @"C:\Images\Screenshots" },
                new() { SourceDirectory = @"C:\Tutu\Screenshots", DestinationDirectory = @"C:\Images\Tutu" },
                new()
                {
                    SourceDirectory = @"C:\Folder\Screenshots",
                    DestinationDirectory = @"C:\Images\Folder3",
                    IncludeSubFolders = true,
                    DeleteAssetsNotInSource = true
                }
            ];

            List<SyncAssetsDirectoriesDefinition> definitionsAfterMovedDown =
            [
                new() { SourceDirectory = @"C:\Tutu\Screenshots", DestinationDirectory = @"C:\Images\Tutu" },
                new() { SourceDirectory = @"C:\Toto\Screenshots", DestinationDirectory = @"C:\Images\Screenshots" },
                new()
                {
                    SourceDirectory = @"C:\Folder\Screenshots",
                    DestinationDirectory = @"C:\Images\Folder3",
                    IncludeSubFolders = true,
                    DeleteAssetsNotInSource = true
                }
            ];

            _syncAssetsViewModel!.Definitions = [.. definitions];

            Assert.That(_syncAssetsViewModel.Definitions, Has.Count.EqualTo(3));

            _syncAssetsViewModel!.MoveDownDefinition(_syncAssetsViewModel.Definitions[0]);

            Assert.That(_syncAssetsViewModel.Definitions, Has.Count.EqualTo(3));

            Assert.That(_syncAssetsViewModel!.Definitions[0].SourceDirectory, Is.EqualTo(@"C:\Tutu\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[0].DestinationDirectory, Is.EqualTo(@"C:\Images\Tutu"));
            Assert.That(_syncAssetsViewModel!.Definitions[0].IncludeSubFolders, Is.False);
            Assert.That(_syncAssetsViewModel!.Definitions[0].DeleteAssetsNotInSource, Is.False);

            Assert.That(_syncAssetsViewModel!.Definitions[1].SourceDirectory, Is.EqualTo(@"C:\Toto\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[1].DestinationDirectory, Is.EqualTo(@"C:\Images\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[1].IncludeSubFolders, Is.False);
            Assert.That(_syncAssetsViewModel!.Definitions[1].DeleteAssetsNotInSource, Is.False);

            Assert.That(_syncAssetsViewModel!.Definitions[2].SourceDirectory, Is.EqualTo(@"C:\Folder\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[2].DestinationDirectory, Is.EqualTo(@"C:\Images\Folder3"));
            Assert.That(_syncAssetsViewModel!.Definitions[2].IncludeSubFolders, Is.True);
            Assert.That(_syncAssetsViewModel!.Definitions[2].DeleteAssetsNotInSource, Is.True);

            CheckAfterChanges(_syncAssetsViewModel, definitionsAfterMovedDown);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(1));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Definitions"));

            CheckInstance(syncAssetsViewModelInstances, definitionsAfterMovedDown);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void MoveDown_NotFirstAndNotLastDefinition_MovesDownDefinition()
    {
        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            List<SyncAssetsDirectoriesDefinition> definitions =
            [
                new() { SourceDirectory = @"C:\Toto\Screenshots", DestinationDirectory = @"C:\Images\Screenshots" },
                new() { SourceDirectory = @"C:\Tutu\Screenshots", DestinationDirectory = @"C:\Images\Tutu" },
                new()
                {
                    SourceDirectory = @"C:\Folder\Screenshots",
                    DestinationDirectory = @"C:\Images\Folder3",
                    IncludeSubFolders = true,
                    DeleteAssetsNotInSource = true
                }
            ];

            List<SyncAssetsDirectoriesDefinition> definitionsAfterMovedDown =
            [
                new() { SourceDirectory = @"C:\Toto\Screenshots", DestinationDirectory = @"C:\Images\Screenshots" },
                new()
                {
                    SourceDirectory = @"C:\Folder\Screenshots",
                    DestinationDirectory = @"C:\Images\Folder3",
                    IncludeSubFolders = true,
                    DeleteAssetsNotInSource = true
                },
                new() { SourceDirectory = @"C:\Tutu\Screenshots", DestinationDirectory = @"C:\Images\Tutu" }
            ];

            _syncAssetsViewModel!.Definitions = [.. definitions];

            Assert.That(_syncAssetsViewModel.Definitions, Has.Count.EqualTo(3));

            _syncAssetsViewModel!.MoveDownDefinition(_syncAssetsViewModel.Definitions[1]);

            Assert.That(_syncAssetsViewModel.Definitions, Has.Count.EqualTo(3));

            Assert.That(_syncAssetsViewModel!.Definitions[0].SourceDirectory, Is.EqualTo(@"C:\Toto\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[0].DestinationDirectory, Is.EqualTo(@"C:\Images\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[0].IncludeSubFolders, Is.False);
            Assert.That(_syncAssetsViewModel!.Definitions[0].DeleteAssetsNotInSource, Is.False);

            Assert.That(_syncAssetsViewModel!.Definitions[1].SourceDirectory, Is.EqualTo(@"C:\Folder\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[1].DestinationDirectory, Is.EqualTo(@"C:\Images\Folder3"));
            Assert.That(_syncAssetsViewModel!.Definitions[1].IncludeSubFolders, Is.True);
            Assert.That(_syncAssetsViewModel!.Definitions[1].DeleteAssetsNotInSource, Is.True);

            Assert.That(_syncAssetsViewModel!.Definitions[2].SourceDirectory, Is.EqualTo(@"C:\Tutu\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[2].DestinationDirectory, Is.EqualTo(@"C:\Images\Tutu"));
            Assert.That(_syncAssetsViewModel!.Definitions[2].IncludeSubFolders, Is.False);
            Assert.That(_syncAssetsViewModel!.Definitions[2].DeleteAssetsNotInSource, Is.False);

            CheckAfterChanges(_syncAssetsViewModel, definitionsAfterMovedDown);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(1));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Definitions"));

            CheckInstance(syncAssetsViewModelInstances, definitionsAfterMovedDown);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void MoveDown_LastDefinition_DoesNotMoveDownDefinition()
    {
        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            List<SyncAssetsDirectoriesDefinition> definitions =
            [
                new() { SourceDirectory = @"C:\Toto\Screenshots", DestinationDirectory = @"C:\Images\Screenshots" },
                new() { SourceDirectory = @"C:\Tutu\Screenshots", DestinationDirectory = @"C:\Images\Tutu" },
                new()
                {
                    SourceDirectory = @"C:\Folder\Screenshots",
                    DestinationDirectory = @"C:\Images\Folder3",
                    IncludeSubFolders = true,
                    DeleteAssetsNotInSource = true
                }
            ];

            List<SyncAssetsDirectoriesDefinition> definitionsAfterMovedDown =
            [
                new() { SourceDirectory = @"C:\Toto\Screenshots", DestinationDirectory = @"C:\Images\Screenshots" },
                new() { SourceDirectory = @"C:\Tutu\Screenshots", DestinationDirectory = @"C:\Images\Tutu" },
                new()
                {
                    SourceDirectory = @"C:\Folder\Screenshots",
                    DestinationDirectory = @"C:\Images\Folder3",
                    IncludeSubFolders = true,
                    DeleteAssetsNotInSource = true
                }
            ];

            _syncAssetsViewModel!.Definitions = [.. definitions];

            Assert.That(_syncAssetsViewModel.Definitions, Has.Count.EqualTo(3));

            _syncAssetsViewModel!.MoveDownDefinition(_syncAssetsViewModel.Definitions[2]);

            Assert.That(_syncAssetsViewModel.Definitions, Has.Count.EqualTo(3));

            Assert.That(_syncAssetsViewModel!.Definitions[0].SourceDirectory, Is.EqualTo(@"C:\Toto\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[0].DestinationDirectory, Is.EqualTo(@"C:\Images\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[0].IncludeSubFolders, Is.False);
            Assert.That(_syncAssetsViewModel!.Definitions[0].DeleteAssetsNotInSource, Is.False);

            Assert.That(_syncAssetsViewModel!.Definitions[1].SourceDirectory, Is.EqualTo(@"C:\Tutu\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[1].DestinationDirectory, Is.EqualTo(@"C:\Images\Tutu"));
            Assert.That(_syncAssetsViewModel!.Definitions[1].IncludeSubFolders, Is.False);
            Assert.That(_syncAssetsViewModel!.Definitions[1].DeleteAssetsNotInSource, Is.False);

            Assert.That(_syncAssetsViewModel!.Definitions[2].SourceDirectory, Is.EqualTo(@"C:\Folder\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[2].DestinationDirectory, Is.EqualTo(@"C:\Images\Folder3"));
            Assert.That(_syncAssetsViewModel!.Definitions[2].IncludeSubFolders, Is.True);
            Assert.That(_syncAssetsViewModel!.Definitions[2].DeleteAssetsNotInSource, Is.True);

            CheckAfterChanges(_syncAssetsViewModel, definitionsAfterMovedDown);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(1));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Definitions"));

            CheckInstance(syncAssetsViewModelInstances, definitionsAfterMovedDown);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void MoveDown_DuplicateDefinitionsInTheCollection_MovesDownGivenDefinition()
    {
        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            List<SyncAssetsDirectoriesDefinition> definitions =
            [
                new() { SourceDirectory = @"C:\Toto\Screenshots", DestinationDirectory = @"C:\Images\Screenshots" },
                new() { SourceDirectory = @"C:\Toto\Screenshots", DestinationDirectory = @"C:\Images\Screenshots" },
                new()
                {
                    SourceDirectory = @"C:\Folder\Screenshots",
                    DestinationDirectory = @"C:\Images\Folder3",
                    IncludeSubFolders = true,
                    DeleteAssetsNotInSource = true
                }
            ];

            List<SyncAssetsDirectoriesDefinition> definitionsAfterMovedDown =
            [
                new() { SourceDirectory = @"C:\Toto\Screenshots", DestinationDirectory = @"C:\Images\Screenshots" },
                new()
                {
                    SourceDirectory = @"C:\Folder\Screenshots",
                    DestinationDirectory = @"C:\Images\Folder3",
                    IncludeSubFolders = true,
                    DeleteAssetsNotInSource = true
                },
                new() { SourceDirectory = @"C:\Toto\Screenshots", DestinationDirectory = @"C:\Images\Screenshots" },
            ];

            _syncAssetsViewModel!.Definitions = [.. definitions];

            Assert.That(_syncAssetsViewModel.Definitions, Has.Count.EqualTo(3));

            _syncAssetsViewModel!.MoveDownDefinition(_syncAssetsViewModel.Definitions[1]);

            Assert.That(_syncAssetsViewModel.Definitions, Has.Count.EqualTo(3));

            Assert.That(_syncAssetsViewModel!.Definitions[0].SourceDirectory, Is.EqualTo(@"C:\Toto\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[0].DestinationDirectory, Is.EqualTo(@"C:\Images\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[0].IncludeSubFolders, Is.False);
            Assert.That(_syncAssetsViewModel!.Definitions[0].DeleteAssetsNotInSource, Is.False);

            Assert.That(_syncAssetsViewModel!.Definitions[1].SourceDirectory, Is.EqualTo(@"C:\Folder\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[1].DestinationDirectory, Is.EqualTo(@"C:\Images\Folder3"));
            Assert.That(_syncAssetsViewModel!.Definitions[1].IncludeSubFolders, Is.True);
            Assert.That(_syncAssetsViewModel!.Definitions[1].DeleteAssetsNotInSource, Is.True);

            Assert.That(_syncAssetsViewModel!.Definitions[2].SourceDirectory, Is.EqualTo(@"C:\Toto\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[2].DestinationDirectory, Is.EqualTo(@"C:\Images\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[2].IncludeSubFolders, Is.False);
            Assert.That(_syncAssetsViewModel!.Definitions[2].DeleteAssetsNotInSource, Is.False);

            CheckAfterChanges(_syncAssetsViewModel, definitionsAfterMovedDown);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(1));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Definitions"));

            CheckInstance(syncAssetsViewModelInstances, definitionsAfterMovedDown);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void MoveDown_OnlyOneDefinition_DoesNothing()
    {
        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            List<SyncAssetsDirectoriesDefinition> definitions =
            [
                new() { SourceDirectory = @"C:\Toto\Screenshots", DestinationDirectory = @"C:\Images\Screenshots" }
            ];

            _syncAssetsViewModel!.Definitions = [.. definitions];

            Assert.That(_syncAssetsViewModel.Definitions, Has.Count.EqualTo(1));

            _syncAssetsViewModel!.MoveDownDefinition(_syncAssetsViewModel.Definitions[0]);

            Assert.That(_syncAssetsViewModel.Definitions, Has.Count.EqualTo(1));

            Assert.That(_syncAssetsViewModel!.Definitions[0].SourceDirectory, Is.EqualTo(@"C:\Toto\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[0].DestinationDirectory, Is.EqualTo(@"C:\Images\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[0].IncludeSubFolders, Is.False);
            Assert.That(_syncAssetsViewModel!.Definitions[0].DeleteAssetsNotInSource, Is.False);

            CheckAfterChanges(_syncAssetsViewModel, definitions);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(1));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Definitions"));

            CheckInstance(syncAssetsViewModelInstances, definitions);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void MoveDown_DefinitionIsNotFound_DoesNothing()
    {
        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            List<SyncAssetsDirectoriesDefinition> definitions =
            [
                new() { SourceDirectory = @"C:\Toto\Screenshots", DestinationDirectory = @"C:\Images\Screenshots" },
                new() { SourceDirectory = @"C:\Tutu\Screenshots", DestinationDirectory = @"C:\Images\Tutu" },
                new()
                {
                    SourceDirectory = @"C:\Folder\Screenshots",
                    DestinationDirectory = @"C:\Images\Folder3",
                    IncludeSubFolders = true,
                    DeleteAssetsNotInSource = true
                }
            ];

            _syncAssetsViewModel!.Definitions = [.. definitions];

            Assert.That(_syncAssetsViewModel.Definitions, Has.Count.EqualTo(3));

            _syncAssetsViewModel!.MoveDownDefinition(new() { SourceDirectory = @"C:\Toto\Screenshots1", DestinationDirectory = @"C:\Toto\Screenshots2" });

            Assert.That(_syncAssetsViewModel.Definitions, Has.Count.EqualTo(3));

            Assert.That(_syncAssetsViewModel!.Definitions[0].SourceDirectory, Is.EqualTo(@"C:\Toto\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[0].DestinationDirectory, Is.EqualTo(@"C:\Images\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[0].IncludeSubFolders, Is.False);
            Assert.That(_syncAssetsViewModel!.Definitions[0].DeleteAssetsNotInSource, Is.False);

            Assert.That(_syncAssetsViewModel!.Definitions[1].SourceDirectory, Is.EqualTo(@"C:\Tutu\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[1].DestinationDirectory, Is.EqualTo(@"C:\Images\Tutu"));
            Assert.That(_syncAssetsViewModel!.Definitions[1].IncludeSubFolders, Is.False);
            Assert.That(_syncAssetsViewModel!.Definitions[1].DeleteAssetsNotInSource, Is.False);

            Assert.That(_syncAssetsViewModel!.Definitions[2].SourceDirectory, Is.EqualTo(@"C:\Folder\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[2].DestinationDirectory, Is.EqualTo(@"C:\Images\Folder3"));
            Assert.That(_syncAssetsViewModel!.Definitions[2].IncludeSubFolders, Is.True);
            Assert.That(_syncAssetsViewModel!.Definitions[2].DeleteAssetsNotInSource, Is.True);

            CheckAfterChanges(_syncAssetsViewModel, definitions);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(1));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Definitions"));

            CheckInstance(syncAssetsViewModelInstances, definitions);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void MoveDown_NoDefinitions_DoesNothing()
    {
        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            List<SyncAssetsDirectoriesDefinition> definitions = [];

            _syncAssetsViewModel!.Definitions = [.. definitions];

            Assert.That(_syncAssetsViewModel.Definitions, Is.Empty);

            _syncAssetsViewModel!.MoveDownDefinition(new() { SourceDirectory = @"C:\Toto\Screenshots1", DestinationDirectory = @"C:\Toto\Screenshots2" });

            Assert.That(_syncAssetsViewModel.Definitions, Is.Empty);

            CheckAfterChanges(_syncAssetsViewModel, definitions);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(1));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Definitions"));

            CheckInstance(syncAssetsViewModelInstances, definitions);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void MoveDown_DefinitionsIsNull_ThrowsNullReferenceException()
    {
        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> _) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            ObservableCollection<SyncAssetsDirectoriesDefinition> definitions = null!;

            _syncAssetsViewModel!.Definitions = definitions;

            Assert.That(_syncAssetsViewModel!.Definitions, Is.Null);

            NullReferenceException? exception = Assert.Throws<NullReferenceException>(() =>
            {
                _syncAssetsViewModel!.MoveDownDefinition(
                    new() { SourceDirectory = @"C:\Toto\Screenshots1", DestinationDirectory = @"C:\Toto\Screenshots2" }
                );
            });

            Assert.That(exception?.Message, Is.EqualTo("Object reference not set to an instance of an object."));

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(1));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Definitions"));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void MoveDown_DefinitionIsNull_DoesNothing()
    {
        ConfigureSyncAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            List<SyncAssetsDirectoriesDefinition> definitions =
            [
                new() { SourceDirectory = @"C:\Toto\Screenshots", DestinationDirectory = @"C:\Images\Screenshots" },
                new() { SourceDirectory = @"C:\Tutu\Screenshots", DestinationDirectory = @"C:\Images\Tutu" },
                new()
                {
                    SourceDirectory = @"C:\Folder\Screenshots",
                    DestinationDirectory = @"C:\Images\Folder3",
                    IncludeSubFolders = true,
                    DeleteAssetsNotInSource = true
                }
            ];

            _syncAssetsViewModel!.Definitions = [.. definitions];

            Assert.That(_syncAssetsViewModel.Definitions, Has.Count.EqualTo(3));

            _syncAssetsViewModel!.MoveDownDefinition(null!);

            Assert.That(_syncAssetsViewModel.Definitions, Has.Count.EqualTo(3));

            Assert.That(_syncAssetsViewModel!.Definitions[0].SourceDirectory, Is.EqualTo(@"C:\Toto\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[0].DestinationDirectory, Is.EqualTo(@"C:\Images\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[0].IncludeSubFolders, Is.False);
            Assert.That(_syncAssetsViewModel!.Definitions[0].DeleteAssetsNotInSource, Is.False);

            Assert.That(_syncAssetsViewModel!.Definitions[1].SourceDirectory, Is.EqualTo(@"C:\Tutu\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[1].DestinationDirectory, Is.EqualTo(@"C:\Images\Tutu"));
            Assert.That(_syncAssetsViewModel!.Definitions[1].IncludeSubFolders, Is.False);
            Assert.That(_syncAssetsViewModel!.Definitions[1].DeleteAssetsNotInSource, Is.False);

            Assert.That(_syncAssetsViewModel!.Definitions[2].SourceDirectory, Is.EqualTo(@"C:\Folder\Screenshots"));
            Assert.That(_syncAssetsViewModel!.Definitions[2].DestinationDirectory, Is.EqualTo(@"C:\Images\Folder3"));
            Assert.That(_syncAssetsViewModel!.Definitions[2].IncludeSubFolders, Is.True);
            Assert.That(_syncAssetsViewModel!.Definitions[2].DeleteAssetsNotInSource, Is.True);

            CheckAfterChanges(_syncAssetsViewModel, definitions);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(1));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("Definitions"));

            CheckInstance(syncAssetsViewModelInstances, definitions);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    private (List<string> notifyPropertyChangedEvents, List<SyncAssetsViewModel> syncAssetsViewModelInstances) NotifyPropertyChangedEvents()
    {
        List<string> notifyPropertyChangedEvents = [];
        List<SyncAssetsViewModel> syncAssetsViewModelInstances = [];

        _syncAssetsViewModel!.PropertyChanged += delegate (object? sender, PropertyChangedEventArgs e)
        {
            notifyPropertyChangedEvents.Add(e.PropertyName!);
            syncAssetsViewModelInstances.Add((SyncAssetsViewModel)sender!);
        };

        return (notifyPropertyChangedEvents, syncAssetsViewModelInstances);
    }

    private void CheckBeforeChanges()
    {
        // From SyncAssetsViewModel
        Assert.That(_syncAssetsViewModel!.Definitions, Is.Empty);

        // From BaseProcessViewModel
        Assert.That(_syncAssetsViewModel!.ProcessStatusMessages, Is.Empty);
        Assert.That(_syncAssetsViewModel!.Results, Is.Empty);
        Assert.That(_syncAssetsViewModel!.Step, Is.EqualTo(ProcessStep.ViewDescription));
        Assert.That(_syncAssetsViewModel!.DescriptionVisible, Is.EqualTo(Visibility.Visible));
        Assert.That(_syncAssetsViewModel!.ConfigurationVisible, Is.EqualTo(Visibility.Hidden));
        Assert.That(_syncAssetsViewModel!.RunVisible, Is.EqualTo(Visibility.Hidden));
        Assert.That(_syncAssetsViewModel!.ResultsVisible, Is.EqualTo(Visibility.Hidden));
        Assert.That(_syncAssetsViewModel!.CanViewDescription, Is.True);
        Assert.That(_syncAssetsViewModel!.CanConfigure, Is.False);
        Assert.That(_syncAssetsViewModel!.CanViewResults, Is.False);
    }

    private static void CheckAfterChanges(
        SyncAssetsViewModel syncAssetsViewModelInstance,
        List<SyncAssetsDirectoriesDefinition> definitions)
    {
        // From SyncAssetsViewModel
        Assert.That(syncAssetsViewModelInstance.Definitions, Has.Count.EqualTo(definitions.Count));

        for (int i = 0; i < definitions.Count; i++)
        {
            Assert.That(syncAssetsViewModelInstance.Definitions[i].SourceDirectory, Is.EqualTo(definitions[i].SourceDirectory));
            Assert.That(syncAssetsViewModelInstance.Definitions[i].DestinationDirectory, Is.EqualTo(definitions[i].DestinationDirectory));
            Assert.That(syncAssetsViewModelInstance.Definitions[i].IncludeSubFolders, Is.EqualTo(definitions[i].IncludeSubFolders));
            Assert.That(syncAssetsViewModelInstance.Definitions[i].DeleteAssetsNotInSource, Is.EqualTo(definitions[i].DeleteAssetsNotInSource));
        }

        // From BaseProcessViewModel
        Assert.That(syncAssetsViewModelInstance.ProcessStatusMessages, Is.Empty);
        Assert.That(syncAssetsViewModelInstance.Results, Is.Empty);
        Assert.That(syncAssetsViewModelInstance.Step, Is.EqualTo(ProcessStep.ViewDescription));
        Assert.That(syncAssetsViewModelInstance.DescriptionVisible, Is.EqualTo(Visibility.Visible));
        Assert.That(syncAssetsViewModelInstance.ConfigurationVisible, Is.EqualTo(Visibility.Hidden));
        Assert.That(syncAssetsViewModelInstance.RunVisible, Is.EqualTo(Visibility.Hidden));
        Assert.That(syncAssetsViewModelInstance.ResultsVisible, Is.EqualTo(Visibility.Hidden));
        Assert.That(syncAssetsViewModelInstance.CanViewDescription, Is.True);
        Assert.That(syncAssetsViewModelInstance.CanConfigure, Is.False);
        Assert.That(syncAssetsViewModelInstance.CanViewResults, Is.False);
    }

    private static void CheckInstance(
        List<SyncAssetsViewModel> syncAssetsViewModelInstances,
        List<SyncAssetsDirectoriesDefinition> definitions)
    {
        int syncAssetsViewModelInstancesCount = syncAssetsViewModelInstances.Count;

        if (syncAssetsViewModelInstancesCount > 1)
        {
            Assert.That(syncAssetsViewModelInstances[syncAssetsViewModelInstancesCount - 2],
                Is.EqualTo(syncAssetsViewModelInstances[0]));
            // No need to go deeper, same instance because ref updated each time
            Assert.That(syncAssetsViewModelInstances[syncAssetsViewModelInstancesCount - 1],
                Is.EqualTo(syncAssetsViewModelInstances[syncAssetsViewModelInstancesCount - 2]));
        }

        if (syncAssetsViewModelInstancesCount > 0)
        {
            CheckAfterChanges(syncAssetsViewModelInstances[0], definitions);
        }
    }
}
