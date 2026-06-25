using Directories = PhotoManager.Tests.Integration.Constants.Directories;
using Reactive = System.Reactive;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryUpdateTargetPathToRecentTests
{
    private string? _assetsDirectory;
    private string? _databaseDirectory;

    private AssetRepository? _assetRepository;
    private TestLogger<AssetRepository>? _testLogger;

    private IPathProviderService? _pathProviderServiceMock;
    private IConfigurationRoot? _configurationRootMock;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _assetsDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_assetsDirectory, Directories.DATABASE_TESTS);

        _configurationRootMock = Substitute.For<IConfigurationRoot>();
        _configurationRootMock.GetDefaultMockConfig();

        _pathProviderServiceMock = Substitute.For<IPathProviderService>();
        _pathProviderServiceMock.ResolveDatabaseDirectory().Returns(_databaseDirectory);
    }

    [SetUp]
    public void SetUp()
    {
        _testLogger = new();
        SqlitePersistenceContext sqlitePersistenceContext =
            PersistenceContextTestHelper.CreateInitializedContext(_pathProviderServiceMock!.ResolveDatabaseDirectory());
        UserConfigurationService userConfigurationService = _configurationRootMock!.CreateUserConfigurationService();
        ImageProcessingService imageProcessingService = new(new TestLogger<ImageProcessingService>());
        FileOperationsService fileOperationsService = new(userConfigurationService,
            new TestLogger<FileOperationsService>());
        ImageMetadataService imageMetadataService = new(fileOperationsService, new TestLogger<ImageMetadataService>());
        _assetRepository = new(imageProcessingService, imageMetadataService, userConfigurationService,
            sqlitePersistenceContext, _testLogger);
    }

    [TearDown]
    public void TearDown()
    {
        _assetRepository?.Dispose();
        TearDownHelper.DeleteTempDbDirectories(_databaseDirectory!);
        _testLogger!.LoggingAssertTearDown();
    }

    [Test]
    public void UpdateTargetPathToRecent_NewFolderAndExisting_UpdateRecentTargetPathsAndSave()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            Folder folder1 = new() { Id = Guid.NewGuid(), Path = "D:\\Workspace\\PhotoManager\\Toto" };
            Folder folder2 = new() { Id = Guid.NewGuid(), Path = "D:\\Workspace\\PhotoManager\\Tutu" };
            Folder folder3 = new() { Id = Guid.NewGuid(), Path = "D:\\Workspace\\PhotoManager\\Titi" };

            _assetRepository!.UpdateTargetPathToRecent(folder1);
            _assetRepository!.UpdateTargetPathToRecent(folder2);
            _assetRepository!.UpdateTargetPathToRecent(folder3);
            _assetRepository!.UpdateTargetPathToRecent(folder2);

            string[] recentTargetPaths = _assetRepository!.GetRecentTargetPaths();

            Assert.That(recentTargetPaths, Has.Length.EqualTo(3));
            Assert.That(recentTargetPaths[2], Is.EqualTo(folder1.Path));
            Assert.That(recentTargetPaths[0], Is.EqualTo(folder2.Path));
            Assert.That(recentTargetPaths[1], Is.EqualTo(folder3.Path));

            Assert.That(assetsUpdatedEvents, Is.Empty);

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void UpdateTargetPathToRecent_MaxCountHasBeenReached_UpdateRecentPathsAndSave()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            for (int i = 0; i < 30; i++)
            {
                _assetRepository!.UpdateTargetPathToRecent(new()
                {
                    Id = Guid.NewGuid(),
                    Path = $"D:\\Workspace\\PhotoManager\\Folder{i}"
                });
            }

            string[] recentTargetPaths = _assetRepository!.GetRecentTargetPaths();

            Assert.That(recentTargetPaths, Has.Length.EqualTo(20));
            Assert.That(recentTargetPaths[0], Is.EqualTo("D:\\Workspace\\PhotoManager\\Folder29"));
            Assert.That(recentTargetPaths[1], Is.EqualTo("D:\\Workspace\\PhotoManager\\Folder28"));
            Assert.That(recentTargetPaths[19], Is.EqualTo("D:\\Workspace\\PhotoManager\\Folder10"));

            Assert.That(assetsUpdatedEvents, Is.Empty);

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void UpdateTargetPathToRecent_PathIsNull_ThrowsInvalidOperationException()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string? nullPath = null;

            Folder folder1 = new() { Id = Guid.NewGuid(), Path = "D:\\Workspace\\PhotoManager\\Toto" };
            Folder folder2 = new() { Id = Guid.NewGuid(), Path = "D:\\Workspace\\PhotoManager\\Tutu" };
            Folder folder3 = new() { Id = Guid.NewGuid(), Path = nullPath! };

            _assetRepository!.UpdateTargetPathToRecent(folder1);
            _assetRepository!.UpdateTargetPathToRecent(folder2);

            InvalidOperationException? exception = Assert.Throws<InvalidOperationException>(
                () => _assetRepository!.UpdateTargetPathToRecent(folder3));

            Assert.That(exception?.Message, Is.EqualTo("Value must be set."));

            Assert.That(assetsUpdatedEvents, Is.Empty);

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void UpdateTargetPathToRecent_PathIsEmpty_UpdateRecentPathsAndSave()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string emptyPath = string.Empty;

            Folder folder1 = new() { Id = Guid.NewGuid(), Path = "D:\\Workspace\\PhotoManager\\Toto" };
            Folder folder2 = new() { Id = Guid.NewGuid(), Path = "D:\\Workspace\\PhotoManager\\Tutu" };
            Folder folder3 = new() { Id = Guid.NewGuid(), Path = emptyPath };

            _assetRepository!.UpdateTargetPathToRecent(folder1);
            _assetRepository!.UpdateTargetPathToRecent(folder2);
            _assetRepository!.UpdateTargetPathToRecent(folder3);

            string[] recentTargetPaths = _assetRepository!.GetRecentTargetPaths();

            Assert.That(recentTargetPaths, Has.Length.EqualTo(3));
            Assert.That(recentTargetPaths[2], Is.EqualTo(folder1.Path));
            Assert.That(recentTargetPaths[1], Is.EqualTo(folder2.Path));
            Assert.That(recentTargetPaths[0], Is.EqualTo(folder3.Path));

            Assert.That(assetsUpdatedEvents, Is.Empty);

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void UpdateTargetPathToRecent_FolderIsNull_ThrowsNullReferenceException()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            Folder folder1 = new() { Id = Guid.NewGuid(), Path = "D:\\Workspace\\PhotoManager\\Toto" };
            Folder folder2 = new() { Id = Guid.NewGuid(), Path = "D:\\Workspace\\PhotoManager\\Tutu" };
            Folder? folder3 = null;

            _assetRepository!.UpdateTargetPathToRecent(folder1);
            _assetRepository!.UpdateTargetPathToRecent(folder2);

            NullReferenceException? exception =
                Assert.Throws<NullReferenceException>(() => _assetRepository!.UpdateTargetPathToRecent(folder3!));

            Assert.That(exception?.Message, Is.EqualTo("Object reference not set to an instance of an object."));

            string[] recentTargetPaths = _assetRepository!.GetRecentTargetPaths();

            Assert.That(recentTargetPaths, Has.Length.EqualTo(2));
            Assert.That(recentTargetPaths[1], Is.EqualTo(folder1.Path));
            Assert.That(recentTargetPaths[0], Is.EqualTo(folder2.Path));

            Assert.That(assetsUpdatedEvents, Is.Empty);

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void UpdateTargetPathToRecent_ConcurrentAccess_RecentTargetPathsAreHandledSafely()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            Folder folder1 = new() { Id = Guid.NewGuid(), Path = "D:\\Workspace\\PhotoManager\\Toto" };
            Folder folder2 = new() { Id = Guid.NewGuid(), Path = "D:\\Workspace\\PhotoManager\\Tutu" };
            Folder folder3 = new() { Id = Guid.NewGuid(), Path = "D:\\Workspace\\PhotoManager\\Titi" };

            // Simulate concurrent access
            Parallel.Invoke(
                () => _assetRepository!.UpdateTargetPathToRecent(folder1),
                () => _assetRepository!.UpdateTargetPathToRecent(folder2),
                () => _assetRepository!.UpdateTargetPathToRecent(folder2),
                () => _assetRepository!.UpdateTargetPathToRecent(folder3)
            );

            string[] recentTargetPaths = _assetRepository!.GetRecentTargetPaths();

            Assert.That(recentTargetPaths, Has.Length.EqualTo(3));
            Assert.That(recentTargetPaths.Any(x => x == folder1.Path), Is.True);
            Assert.That(recentTargetPaths.Any(x => x == folder2.Path), Is.True);
            Assert.That(recentTargetPaths.Any(x => x == folder3.Path), Is.True);

            Assert.That(assetsUpdatedEvents, Is.Empty);

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }
}
