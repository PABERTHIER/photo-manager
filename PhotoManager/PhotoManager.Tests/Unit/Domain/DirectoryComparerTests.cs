﻿using Microsoft.Extensions.Configuration;

namespace PhotoManager.Tests.Unit.Domain;

[TestFixture]
public class DirectoryComparerTests
{
    private IStorageService? _storageService;
    private IDirectoryComparer? _directoryComparer;
    private Mock<IConfigurationRoot>? _configurationRootMock;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        _configurationRootMock = new Mock<IConfigurationRoot>();
        _configurationRootMock
            .MockGetValue("appsettings:CatalogBatchSize", "100")
            .MockGetValue("appsettings:CatalogCooldownMinutes", "5")
            .MockGetValue("appsettings:BackupsToKeep", "2")
            .MockGetValue("appsettings:ThumbnailsDictionaryEntriesToKeep", "5");

        UserConfigurationService userConfigurationService = new(_configurationRootMock!.Object);
        _storageService = new StorageService(userConfigurationService);
        _directoryComparer = new DirectoryComparer(_storageService);
    }

    [Test]
    public void GetNewFileNames_AllNewFiles_ReturnsArrayOfNewFileNames()
    {
        string[] fileNames = { "file1.jpg", "file2.png", "file3.mp4", "toto.txt", "tutu.bat" };
        List<Asset> cataloguedAssets = new()
        {
            new Asset { FileName = "file4.jpg" },
            new Asset { FileName = "file5.png" },
        };

        string[] newFileNames = _directoryComparer!.GetNewFileNames(fileNames, cataloguedAssets);

        Assert.IsNotEmpty(newFileNames);
        CollectionAssert.AreEquivalent(new string[] { "file1.jpg", "file2.png", "file3.mp4" }, newFileNames);
    }

    [Test]
    public void GetNewFileNames_SomeNewFiles_ReturnsArrayOfNewFileNames()
    {
        string[] fileNames = { "file1.jpg", "file2.png", "file3.mp4", "toto.txt", "tutu.bat" };
        List<Asset> cataloguedAssets = new()
        {
            new Asset { FileName = "file1.jpg" },
            new Asset { FileName = "file4.jpg" },
            new Asset { FileName = "file5.png" },
        };

        string[] newFileNames = _directoryComparer!.GetNewFileNames(fileNames, cataloguedAssets);

        Assert.IsNotEmpty(newFileNames);
        CollectionAssert.AreEquivalent(new string[] { "file2.png", "file3.mp4" }, newFileNames);
    }

    [Test]
    public void GetNewFileNames_NoNewFiles_ReturnsEmptyArray()
    {
        string[] fileNames = { "file1.jpg", "file2.png", "file3.mp4", "toto.txt", "tutu.bat" };
        List<Asset> cataloguedAssets = new()
        {
            new Asset { FileName = "file1.jpg" },
            new Asset { FileName = "file2.png" },
            new Asset { FileName = "file3.mp4" }
        };

        string[] newFileNames = _directoryComparer!.GetNewFileNames(fileNames, cataloguedAssets);

        Assert.IsEmpty(newFileNames);
    }

    [Test]
    public void GetNewFileNames_FileNamesIsEmpty_ReturnsEmptyArray()
    {
        string[] fileNames = Array.Empty<string>();
        List<Asset> cataloguedAssets = new()
        {
            new Asset { FileName = "file1.jpg" },
            new Asset { FileName = "file2.png" },
            new Asset { FileName = "file3.mp4" }
        };

        string[] newFileNames = _directoryComparer!.GetNewFileNames(fileNames, cataloguedAssets);

        Assert.IsEmpty(newFileNames);
    }

    [Test]
    public void GetNewFileNames_FileNamesIsNull_ThrowsArgumentNullException()
    {
        string[]? fileNames = null;
        List<Asset> cataloguedAssets = new()
        {
            new Asset { FileName = "file1.jpg" },
            new Asset { FileName = "file2.png" },
            new Asset { FileName = "file3.mp4" }
        };

        Assert.Throws<ArgumentNullException>(() => _directoryComparer!.GetNewFileNames(fileNames!, cataloguedAssets));
    }

    [Test]
    public void GetNewFileNames_CataloguedAssetsIsEmpty_ReturnsArrayOfNewFileNames()
    {
        string[] fileNames = { "file1.jpg", "file2.png", "file3.mp4", "toto.txt", "tutu.bat" };
        List<Asset> cataloguedAssets = new();

        string[] newFileNames = _directoryComparer!.GetNewFileNames(fileNames, cataloguedAssets);

        Assert.IsNotEmpty(newFileNames);
        CollectionAssert.AreEquivalent(new string[] { "file1.jpg", "file2.png", "file3.mp4" }, newFileNames);
    }

    [Test]
    public void GetNewFileNames_CataloguedAssetsIsNull_ThrowsArgumentNullException()
    {
        string[] fileNames = { "file1.jpg", "file2.png", "file3.mp4", "toto.txt", "tutu.bat" };
        List<Asset>? cataloguedAssets = null;

        Assert.Throws<ArgumentNullException>(() => _directoryComparer!.GetNewFileNames(fileNames, cataloguedAssets!));
    }

    [Test]
    public void GetNewFileNamesToSync_AllNewFiles_ReturnsArrayOfNewFileNames()
    {
        string[] sourceFileNames = { "file1.jpg", "file2.png", "file3.mp4", "toto.txt", "tutu.bat" };
        string[] destinationFileNames = { "file4.jpg", "file5.png" };

        string[] newFileNames = _directoryComparer!.GetNewFileNamesToSync(sourceFileNames, destinationFileNames);

        Assert.IsNotEmpty(newFileNames);
        CollectionAssert.AreEquivalent(new string[] { "file1.jpg", "file2.png", "file3.mp4" }, newFileNames);
    }

    [Test]
    public void GetNewFileNamesToSync_SomeNewFiles_ReturnsArrayOfNewFileNames()
    {
        string[] sourceFileNames = { "file1.jpg", "file2.png", "file3.mp4", "toto.txt", "tutu.bat" };
        string[] destinationFileNames = { "file1.jpg", "file4.jpg", "file5.png" };

        string[] newFileNames = _directoryComparer!.GetNewFileNamesToSync(sourceFileNames, destinationFileNames);

        Assert.IsNotEmpty(newFileNames);
        CollectionAssert.AreEquivalent(new string[] { "file2.png", "file3.mp4" }, newFileNames);
    }

    [Test]
    public void GetNewFileNamesToSync_NoNewFiles_ReturnsEmptyArray()
    {
        string[] sourceFileNames = { "file1.jpg", "file2.png", "file3.mp4", "toto.txt", "tutu.bat" };
        string[] destinationFileNames = { "file1.jpg", "file2.png", "file3.mp4" };

        string[] newFileNames = _directoryComparer!.GetNewFileNamesToSync(sourceFileNames, destinationFileNames);

        Assert.IsEmpty(newFileNames);
    }

    [Test]
    public void GetNewFileNamesToSync_SourceFileNamesIsEmpty_ReturnsEmptyArray()
    {
        string[] sourceFileNames = Array.Empty<string>();
        string[] destinationFileNames = { "file1.jpg", "file2.png", "file3.mp4" };

        string[] newFileNames = _directoryComparer!.GetNewFileNamesToSync(sourceFileNames, destinationFileNames);

        Assert.IsEmpty(newFileNames);
    }

    [Test]
    public void GetNewFileNamesToSync_SourceFileNamesIsNull_ThrowsArgumentNullException()
    {
        string[]? sourceFileNames = null;
        string[] destinationFileNames = { "file1.jpg", "file2.png", "file3.mp4" };

        Assert.Throws<ArgumentNullException>(() => _directoryComparer!.GetNewFileNamesToSync(sourceFileNames!, destinationFileNames));
    }

    [Test]
    public void GetNewFileNamesToSync_DestinationFileNamesIsEmpty_ReturnsArrayOfNewFileNames()
    {
        string[] sourceFileNames = { "file1.jpg", "file2.png", "file3.mp4", "toto.txt", "tutu.bat" };
        string[] destinationFileNames = Array.Empty<string>();

        string[] newFileNames = _directoryComparer!.GetNewFileNamesToSync(sourceFileNames, destinationFileNames);

        Assert.IsNotEmpty(newFileNames);
        CollectionAssert.AreEquivalent(new string[] { "file1.jpg", "file2.png", "file3.mp4" }, newFileNames);
    }

    [Test]
    public void GetNewFileNamesToSync_DestinationFileNamesIsNull_ThrowsArgumentNullException()
    {
        string[] sourceFileNames = { "file1.jpg", "file2.png", "file3.mp4", "toto.txt", "tutu.bat" };
        string[]? destinationFileNames = null;

        Assert.Throws<ArgumentNullException>(() => _directoryComparer!.GetNewFileNamesToSync(sourceFileNames, destinationFileNames!));
    }

    [Test]
    [TestCase(new string[] { "image1.jpg", "video1.mp4", "file2.txt", "image2.png" }, new string[] { "image1.jpg", "image2.png" }, new string[] { "video1.mp4" })]
    [TestCase(new string[] { "file2.txt", "image2.png", "image1.jpg", "video1.mp4" }, new string[] { "image1.jpg", "image2.png" }, new string[] { "video1.mp4" })]
    [TestCase(new string[] { "image1.jpg", "video1.mp4", "image2.png" }, new string[] { "image1.jpg", "image2.png" }, new string[] { "video1.mp4" })]
    [TestCase(new string[] { "image1.jpg", "image2.png" }, new string[] { "image1.jpg", "image2.png" }, new string[] { })]
    [TestCase(new string[] { "video1.mp4", "video2.mov" }, new string[] { }, new string[] { "video1.mp4", "video2.mov" })]
    [TestCase(new string[] { "file1.txt", "file2.doc", "file3.pdf" }, new string[] { }, new string[] { })]
    [TestCase(new string[] { }, new string[] { }, new string[] { })]
    public void GetImageAndVideoNames_ReturnsImageAndVideoNamesArray(string[] fileNames, string[] expectedImageNames, string[] expectedVideoNames)
    {
        (string[] imageNames, string[] videoNames) = _directoryComparer!.GetImageAndVideoNames(fileNames);

        CollectionAssert.AreEquivalent(expectedImageNames, imageNames);
        CollectionAssert.AreEquivalent(expectedVideoNames, videoNames);
    }

    [Test]
    public void GetImageAndVideoNames_FileNamesIsNull_ThrowsNullReferenceException()
    {
        string[]? fileNames = null;

        Assert.Throws<NullReferenceException>(() => _directoryComparer!.GetImageAndVideoNames(fileNames!));
    }

    [Test]
    public void GetDeletedFileNames_AllDeletedFiles_ReturnsArrayOfDeletedFileNames()
    {
        string[] fileNames = { "file1.jpg", "file2.png", "file3.mp4", "toto.txt", "tutu.bat" };
        List<Asset> cataloguedAssets = new()
        {
            new Asset { FileName = "file4.jpg" },
            new Asset { FileName = "file5.png" },
        };

        string[] deletedFileNames = _directoryComparer!.GetDeletedFileNames(fileNames, cataloguedAssets);

        Assert.IsNotEmpty(deletedFileNames);
        CollectionAssert.AreEquivalent(new string[] { "file4.jpg", "file5.png" }, deletedFileNames);
    }

    [Test]
    public void GetDeletedFileNames_SomeDeletedFiles_ReturnsArrayOfDeletedFileNames()
    {
        string[] fileNames = { "file1.jpg", "file2.png", "file3.mp4", "toto.txt", "tutu.bat" };
        List<Asset> cataloguedAssets = new()
        {
            new Asset { FileName = "file1.jpg" },
            new Asset { FileName = "file4.jpg" },
            new Asset { FileName = "file5.png" },
        };

        string[] deletedFileNames = _directoryComparer!.GetDeletedFileNames(fileNames, cataloguedAssets);

        Assert.IsNotEmpty(deletedFileNames);
        CollectionAssert.AreEquivalent(new string[] { "file4.jpg", "file5.png" }, deletedFileNames);
    }

    [Test]
    public void GetDeletedFileNames_NoDeletedFiles_ReturnsEmptyArray()
    {
        string[] fileNames = { "file1.jpg", "file2.png", "file3.mp4", "toto.txt", "tutu.bat" };
        List<Asset> cataloguedAssets = new()
        {
            new Asset { FileName = "file1.jpg" },
            new Asset { FileName = "file2.png" },
            new Asset { FileName = "file3.mp4" }
        };

        string[] deletedFileNames = _directoryComparer!.GetDeletedFileNames(fileNames, cataloguedAssets);

        Assert.IsEmpty(deletedFileNames);
    }

    [Test]
    public void GetDeletedFileNames_FileNamesIsEmpty_ReturnsArrayOfDeletedFileNames()
    {
        string[] fileNames = Array.Empty<string>();
        List<Asset> cataloguedAssets = new()
        {
            new Asset { FileName = "file1.jpg" },
            new Asset { FileName = "file2.png" },
            new Asset { FileName = "file3.mp4" }
        };

        string[] deletedFileNames = _directoryComparer!.GetDeletedFileNames(fileNames, cataloguedAssets);

        Assert.IsNotEmpty(deletedFileNames);
        CollectionAssert.AreEquivalent(new string[] { "file1.jpg", "file2.png", "file3.mp4" }, deletedFileNames);
    }

    [Test]
    public void GetDeletedFileNames_FileNamesIsNull_ThrowsArgumentNullException()
    {
        string[]? fileNames = null;
        List<Asset> cataloguedAssets = new()
        {
            new Asset { FileName = "file1.jpg" },
            new Asset { FileName = "file2.png" },
            new Asset { FileName = "file3.mp4" }
        };

        Assert.Throws<ArgumentNullException>(() => _directoryComparer!.GetDeletedFileNames(fileNames!, cataloguedAssets));
    }

    [Test]
    public void GetDeletedFileNames_CataloguedAssetsIsEmpty_ReturnsEmptyArray()
    {
        string[] fileNames = { "file1.jpg", "file2.png", "file3.mp4", "toto.txt", "tutu.bat" };
        List<Asset> cataloguedAssets = new();

        string[] deletedFileNames = _directoryComparer!.GetDeletedFileNames(fileNames, cataloguedAssets);

        Assert.IsEmpty(deletedFileNames);
    }

    [Test]
    public void GetDeletedFileNames_CataloguedAssetsIsNull_ThrowsArgumentNullException()
    {
        string[] fileNames = { "file1.jpg", "file2.png", "file3.mp4", "toto.txt", "tutu.bat" };
        List<Asset>? cataloguedAssets = null;

        Assert.Throws<ArgumentNullException>(() => _directoryComparer!.GetDeletedFileNames(fileNames, cataloguedAssets!));
    }

    [Test]
    public void GetDeletedFileNamesToSync_AllDeletedFiles_ReturnsArrayOfDeletedFileNames()
    {
        string[] sourceFileNames = { "file1.jpg", "file2.png", "file3.mp4", "toto.txt", "tutu.bat" };
        string[] destinationFileNames = { "file4.jpg", "file5.png" };

        string[] deletedFileNames = _directoryComparer!.GetDeletedFileNamesToSync(sourceFileNames, destinationFileNames);

        Assert.IsNotEmpty(deletedFileNames);
        CollectionAssert.AreEquivalent(new string[] { "file4.jpg", "file5.png" }, deletedFileNames);
    }

    [Test]
    public void GetDeletedFileNamesToSync_SomeDeletedFiles_ReturnsArrayOfDeletedFileNames()
    {
        string[] sourceFileNames = { "file1.jpg", "file2.png", "file3.mp4", "toto.txt", "tutu.bat" };
        string[] destinationFileNames = { "file1.jpg", "file4.jpg", "file5.png" };

        string[] deletedFileNames = _directoryComparer!.GetDeletedFileNamesToSync(sourceFileNames, destinationFileNames);

        Assert.IsNotEmpty(deletedFileNames);
        CollectionAssert.AreEquivalent(new string[] { "file4.jpg", "file5.png" }, deletedFileNames);
    }

    [Test]
    public void GetDeletedFileNamesToSync_NoDeletedFiles_ReturnsEmptyArray()
    {
        string[] sourceFileNames = { "file1.jpg", "file2.png", "file3.mp4", "toto.txt", "tutu.bat" };
        string[] destinationFileNames = { "file1.jpg", "file2.png", "file3.mp4" };

        string[] deletedFileNames = _directoryComparer!.GetDeletedFileNamesToSync(sourceFileNames, destinationFileNames);

        Assert.IsEmpty(deletedFileNames);
    }

    [Test]
    public void GetDeletedFileNamesToSync_SourceFileNamesIsEmpty_ReturnsArrayOfDeletedFileNames()
    {
        string[] sourceFileNames = Array.Empty<string>();
        string[] destinationFileNames = { "file1.jpg", "file2.png", "file3.mp4" };

        string[] deletedFileNames = _directoryComparer!.GetDeletedFileNamesToSync(sourceFileNames, destinationFileNames);

        Assert.IsNotEmpty(deletedFileNames);
        CollectionAssert.AreEquivalent(new string[] { "file1.jpg", "file2.png", "file3.mp4" }, deletedFileNames);
    }

    [Test]
    public void GetDeletedFileNamesToSync_SourceFileNamesIsNull_ThrowsArgumentNullException()
    {
        string[]? sourceFileNames = null;
        string[] destinationFileNames = { "file1.jpg", "file2.png", "file3.mp4" };

        Assert.Throws<ArgumentNullException>(() => _directoryComparer!.GetDeletedFileNamesToSync(sourceFileNames!, destinationFileNames));
    }

    [Test]
    public void GetDeletedFileNamesToSync_DestinationFileNamesIsEmpty_ReturnsEmptyArray()
    {
        string[] sourceFileNames = { "file1.jpg", "file2.png", "file3.mp4", "toto.txt", "tutu.bat" };
        string[] destinationFileNames = Array.Empty<string>();

        string[] deletedFileNames = _directoryComparer!.GetDeletedFileNamesToSync(sourceFileNames, destinationFileNames);

        Assert.IsEmpty(deletedFileNames);
    }

    [Test]
    public void GetDeletedFileNamesToSync_DestinationFileNamesIsNull_ThrowsArgumentNullException()
    {
        string[] sourceFileNames = { "file1.jpg", "file2.png", "file3.mp4", "toto.txt", "tutu.bat" };
        string[]? destinationFileNames = null;

        Assert.Throws<ArgumentNullException>(() => _directoryComparer!.GetDeletedFileNamesToSync(sourceFileNames, destinationFileNames!));
    }
}