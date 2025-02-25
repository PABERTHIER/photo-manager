﻿using Reactive = System.Reactive;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryHasChangesTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;
    private const string DATABASE_END_PATH = "v1.0";

    private AssetRepository? _assetRepository;
    private Mock<IStorageService>? _storageServiceMock;
    private Mock<IConfigurationRoot>? _configurationRootMock;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        _databaseDirectory = Path.Combine(_dataDirectory, "DatabaseTests");
        _databasePath = Path.Combine(_databaseDirectory, DATABASE_END_PATH);

        _configurationRootMock = new Mock<IConfigurationRoot>();
        _configurationRootMock.GetDefaultMockConfig();

        _storageServiceMock = new Mock<IStorageService>();
        _storageServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_databasePath);
    }

    [SetUp]
    public void SetUp()
    {
        PhotoManager.Infrastructure.Database.Database database = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        UserConfigurationService userConfigurationService = new (_configurationRootMock!.Object);
        _assetRepository = new (database, _storageServiceMock!.Object, userConfigurationService);
    }

    [Test]
    public void HasChanges_Initialization_ReturnFalse()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            Assert.That(_assetRepository!.HasChanges(), Is.False);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void HasChanges_AfterChange_ReturnTrue()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            Assert.That(_assetRepository!.HasChanges(), Is.False);

            string folderPath1 = Path.Combine(_dataDirectory!, "TestFolder1");

            _assetRepository!.AddFolder(folderPath1);

            Assert.That(_assetRepository!.HasChanges(), Is.True);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void HasChanges_ConcurrentAccess_ChangesAreHandledSafely()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            Assert.That(_assetRepository!.HasChanges(), Is.False);

            string folderPath1 = Path.Combine(_dataDirectory!, "TestFolder1");

            _assetRepository!.AddFolder(folderPath1);

            bool hasChanges1 = false;
            bool hasChanges2 = false;
            bool hasChanges3 = false;

            // Simulate concurrent access
            Parallel.Invoke(
                () => hasChanges1 = _assetRepository!.HasChanges(),
                () => hasChanges2 = _assetRepository!.HasChanges(),
                () => hasChanges3 = _assetRepository!.HasChanges()
            );

            Assert.That(hasChanges1, Is.True);
            Assert.That(hasChanges2, Is.True);
            Assert.That(hasChanges3, Is.True);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }
}
