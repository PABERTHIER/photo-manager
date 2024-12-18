﻿using Reactive = System.Reactive;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositorySaveSyncAssetsConfigurationTests
{
    private string? _dataDirectory;
    private string? _backupPath;
    private const string BACKUP_END_PATH = "DatabaseTests\\v1.0";

    private AssetRepository? _assetRepository;
    private Mock<IStorageService>? _storageServiceMock;
    private Mock<IConfigurationRoot>? _configurationRootMock;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        _backupPath = Path.Combine(_dataDirectory, BACKUP_END_PATH);

        _configurationRootMock = new Mock<IConfigurationRoot>();
        _configurationRootMock.GetDefaultMockConfig();

        _storageServiceMock = new Mock<IStorageService>();
        _storageServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_backupPath);
    }

    [SetUp]
    public void SetUp()
    {
        PhotoManager.Infrastructure.Database.Database database = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        UserConfigurationService userConfigurationService = new (_configurationRootMock!.Object);
        _assetRepository = new (database, _storageServiceMock!.Object, userConfigurationService);
    }

    [Test]
    public void SaveSyncAssetsConfiguration_SyncAssetsConfiguration_SaveConfiguration()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            SyncAssetsConfiguration syncAssetsConfigurationToSave = new();
            syncAssetsConfigurationToSave.Definitions.Add(
                new SyncAssetsDirectoriesDefinition
                {
                    SourceDirectory = "C:\\Toto\\Screenshots",
                    DestinationDirectory = "C:\\Images\\Toto",
                    IncludeSubFolders = false,
                    DeleteAssetsNotInSource = false
                });
            syncAssetsConfigurationToSave.Definitions.Add(
                new SyncAssetsDirectoriesDefinition
                {
                    SourceDirectory = "C:\\Tutu\\Screenshots",
                    DestinationDirectory = "C:\\Images\\Tutu",
                    IncludeSubFolders = false,
                    DeleteAssetsNotInSource = false
                });

            _assetRepository!.SaveSyncAssetsConfiguration(syncAssetsConfigurationToSave);
            SyncAssetsConfiguration syncAssetsConfiguration = _assetRepository.GetSyncAssetsConfiguration();

            Assert.AreEqual(2, syncAssetsConfiguration.Definitions.Count);
            Assert.AreEqual("C:\\Toto\\Screenshots", syncAssetsConfiguration.Definitions[0].SourceDirectory);
            Assert.AreEqual("C:\\Images\\Toto", syncAssetsConfiguration.Definitions[0].DestinationDirectory);
            Assert.AreEqual("C:\\Tutu\\Screenshots", syncAssetsConfiguration.Definitions[1].SourceDirectory);
            Assert.AreEqual("C:\\Images\\Tutu", syncAssetsConfiguration.Definitions[1].DestinationDirectory);

            Assert.IsTrue(_assetRepository.HasChanges());

            _assetRepository!.SaveSyncAssetsConfiguration(new SyncAssetsConfiguration());
            syncAssetsConfiguration = _assetRepository.GetSyncAssetsConfiguration();

            Assert.AreEqual(0, syncAssetsConfiguration.Definitions.Count);

            Assert.IsTrue(_assetRepository.HasChanges());

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void SaveSyncAssetsConfiguration_ConcurrentAccess_SyncAssetsConfigurationIsHandledSafely()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            SyncAssetsConfiguration syncAssetsConfigurationToSave = new();
            syncAssetsConfigurationToSave.Definitions.Add(
                new SyncAssetsDirectoriesDefinition
                {
                    SourceDirectory = "C:\\Toto\\Screenshots",
                    DestinationDirectory = "C:\\Images\\Toto",
                    IncludeSubFolders = false,
                    DeleteAssetsNotInSource = false
                });
            syncAssetsConfigurationToSave.Definitions.Add(
                new SyncAssetsDirectoriesDefinition
                {
                    SourceDirectory = "C:\\Tutu\\Screenshots",
                    DestinationDirectory = "C:\\Images\\Tutu",
                    IncludeSubFolders = false,
                    DeleteAssetsNotInSource = false
                });

            // Simulate concurrent access
            Parallel.Invoke(
                () => _assetRepository!.SaveSyncAssetsConfiguration(syncAssetsConfigurationToSave),
                () => _assetRepository!.SaveSyncAssetsConfiguration(syncAssetsConfigurationToSave),
                () => _assetRepository!.SaveSyncAssetsConfiguration(syncAssetsConfigurationToSave)
            );

            SyncAssetsConfiguration syncAssetsConfiguration = _assetRepository!.GetSyncAssetsConfiguration();

            Assert.AreEqual(2, syncAssetsConfiguration.Definitions.Count);
            Assert.AreEqual("C:\\Toto\\Screenshots", syncAssetsConfiguration.Definitions[0].SourceDirectory);
            Assert.AreEqual("C:\\Images\\Toto", syncAssetsConfiguration.Definitions[0].DestinationDirectory);
            Assert.AreEqual("C:\\Tutu\\Screenshots", syncAssetsConfiguration.Definitions[1].SourceDirectory);
            Assert.AreEqual("C:\\Images\\Tutu", syncAssetsConfiguration.Definitions[1].DestinationDirectory);

            Assert.IsTrue(_assetRepository.HasChanges());

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }
}
