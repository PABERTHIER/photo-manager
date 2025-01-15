﻿using Reactive = System.Reactive;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryUpdateTargetPathToRecentTests
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

            List<string> recentTargetPaths = _assetRepository!.GetRecentTargetPaths();

            Assert.That(recentTargetPaths, Has.Count.EqualTo(3));
            Assert.That(recentTargetPaths[2], Is.EqualTo(folder1.Path));
            Assert.That(recentTargetPaths[0], Is.EqualTo(folder2.Path));
            Assert.That(recentTargetPaths[1], Is.EqualTo(folder3.Path));

            Assert.That(_assetRepository.HasChanges(), Is.True);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
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
                _assetRepository!.UpdateTargetPathToRecent(new() { Id = Guid.NewGuid(), Path = $"D:\\Workspace\\PhotoManager\\Folder{i}" });
            }

            List<string> recentTargetPaths = _assetRepository!.GetRecentTargetPaths();

            Assert.That(recentTargetPaths, Has.Count.EqualTo(20));
            Assert.That(recentTargetPaths[0], Is.EqualTo("D:\\Workspace\\PhotoManager\\Folder29"));
            Assert.That(recentTargetPaths[1], Is.EqualTo("D:\\Workspace\\PhotoManager\\Folder28"));
            Assert.That(recentTargetPaths[19], Is.EqualTo("D:\\Workspace\\PhotoManager\\Folder10"));

            Assert.That(_assetRepository.HasChanges(), Is.True);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void UpdateTargetPathToRecent_PathIsNull_UpdateRecentPathsAndSave()
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
            _assetRepository!.UpdateTargetPathToRecent(folder3);

            List<string> recentTargetPaths = _assetRepository!.GetRecentTargetPaths();

            Assert.That(recentTargetPaths, Has.Count.EqualTo(3));
            Assert.That(recentTargetPaths[2], Is.EqualTo(folder1.Path));
            Assert.That(recentTargetPaths[1], Is.EqualTo(folder2.Path));
            Assert.That(recentTargetPaths[0], Is.EqualTo(folder3.Path));

            Assert.That(_assetRepository.HasChanges(), Is.True);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
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

            List<string> recentTargetPaths = _assetRepository!.GetRecentTargetPaths();

            Assert.That(recentTargetPaths, Has.Count.EqualTo(3));
            Assert.That(recentTargetPaths[2], Is.EqualTo(folder1.Path));
            Assert.That(recentTargetPaths[1], Is.EqualTo(folder2.Path));
            Assert.That(recentTargetPaths[0], Is.EqualTo(folder3.Path));

            Assert.That(_assetRepository.HasChanges(), Is.True);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
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

            NullReferenceException? exception = Assert.Throws<NullReferenceException>(() => _assetRepository!.UpdateTargetPathToRecent(folder3!));

            Assert.That(exception?.Message, Is.EqualTo("Object reference not set to an instance of an object."));

            List<string> recentTargetPaths = _assetRepository!.GetRecentTargetPaths();

            Assert.That(recentTargetPaths, Has.Count.EqualTo(2));
            Assert.That(recentTargetPaths[1], Is.EqualTo(folder1.Path));
            Assert.That(recentTargetPaths[0], Is.EqualTo(folder2.Path));

            Assert.That(_assetRepository.HasChanges(), Is.True);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
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

            List<string> recentTargetPaths = _assetRepository!.GetRecentTargetPaths();

            Assert.That(recentTargetPaths, Has.Count.EqualTo(3));
            Assert.That(recentTargetPaths.Any(x => x == folder1.Path), Is.True);
            Assert.That(recentTargetPaths.Any(x => x == folder2.Path), Is.True);
            Assert.That(recentTargetPaths.Any(x => x == folder3.Path), Is.True);

            Assert.That(_assetRepository.HasChanges(), Is.True);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }
}
