﻿namespace PhotoManager.Tests.Unit.Infrastructure.Database.DatabaseTests;

[TestFixture]
public class DatabaseReadBlobTests
{
    private string? dataDirectory;

    private PhotoManager.Infrastructure.Database.Database? _database;
    private UserConfigurationService? _userConfigurationService;

    private readonly char semicolonSeparator = ';';

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");

        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();

        _userConfigurationService = new (configurationRootMock.Object);
    }

    [SetUp]
    public void Setup()
    {
        _database = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
    }

    [Test]
    public void ReadBlob_FileExistsAndSemicolonSeparator_ReturnsBlob()
    {
        string blobName = Guid.NewGuid() + ".bin"; // The blobName is always like this FolderId + ".bin"
        string directoryPath = Path.Combine(dataDirectory!, "DatabaseTests");
        string blobFilePath = Path.Combine(directoryPath, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, blobName);
        Dictionary<string, byte[]> blobToWrite = new()
        {
            { "Image1.jpg", new byte[] { 1, 2, 3 } },
            { "Image2.png", new byte[] { 4, 5, 6 } }
        };

        try
        {
            _database!.Initialize(
                directoryPath,
                semicolonSeparator,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);

            _database!.WriteBlob(blobToWrite, blobName);
            Assert.IsTrue(File.Exists(blobFilePath));

            Dictionary<string, byte[]>? blob = _database!.ReadBlob(blobName);

            Assert.IsNotNull(blob);
            Assert.IsInstanceOf<Dictionary<string, byte[]>?>(blob);
            Assert.AreEqual(blobFilePath, _database!.Diagnostics.LastReadFilePath);
            Assert.AreEqual(blobToWrite.Count, blob!.Count);
            Assert.IsTrue(blob.ContainsKey("Image1.jpg"));
            CollectionAssert.AreEqual(blobToWrite["Image1.jpg"], blob["Image1.jpg"]);
            CollectionAssert.AreEqual(blobToWrite["Image2.png"], blob["Image2.png"]);
            Assert.That(blobToWrite, Is.EquivalentTo(blob));
            Assert.AreEqual(blobToWrite, blob);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests_Backups"), true);
        }
    }

    [Test]
    public void ReadBlob_FileExistsAndPipeSeparator_ReturnsBlob()
    {
        string blobName = Guid.NewGuid() + ".bin"; // The blobName is always like this FolderId + ".bin"
        string directoryPath = Path.Combine(dataDirectory!, "DatabaseTests");
        string blobFilePath = Path.Combine(directoryPath, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, blobName);
        Dictionary<string, byte[]> blobToWrite = new()
        {
            { "Image1.jpg", new byte[] { 1, 2, 3 } },
            { "Image2.png", new byte[] { 4, 5, 6 } }
        };

        try
        {
            _database!.Initialize(
                directoryPath,
                _userConfigurationService!.StorageSettings.Separator,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);

            _database!.WriteBlob(blobToWrite, blobName);
            Assert.IsTrue(File.Exists(blobFilePath));

            Dictionary<string, byte[]>? blob = _database!.ReadBlob(blobName);

            Assert.IsNotNull(blob);
            Assert.IsInstanceOf<Dictionary<string, byte[]>?>(blob);
            Assert.AreEqual(blobFilePath, _database!.Diagnostics.LastReadFilePath);
            Assert.AreEqual(blobToWrite.Count, blob!.Count);
            Assert.IsTrue(blob.ContainsKey("Image1.jpg"));
            CollectionAssert.AreEqual(blobToWrite["Image1.jpg"], blob["Image1.jpg"]);
            CollectionAssert.AreEqual(blobToWrite["Image2.png"], blob["Image2.png"]);
            Assert.That(blobToWrite, Is.EquivalentTo(blob));
            Assert.AreEqual(blobToWrite, blob);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests_Backups"), true);
        }
    }

    [Test]
    public void ReadBlob_FileDoesNotExistsAndPipeSeparator_ReturnsNull()
    {
        string blobName = Guid.NewGuid() + ".bin"; // The blobName is always like this FolderId + ".bin"
        string directoryPath = Path.Combine(dataDirectory!, "DatabaseTests");
        string blobFilePath = Path.Combine(directoryPath, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, blobName);

        try
        {
            _database!.Initialize(
                directoryPath,
                _userConfigurationService!.StorageSettings.Separator,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);

            Dictionary<string, byte[]>? blob = _database!.ReadBlob(blobName);

            Assert.IsNull(blob);
            Assert.AreEqual(blobFilePath, _database!.Diagnostics.LastReadFilePath);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests_Backups"), true);
        }
    }
}
