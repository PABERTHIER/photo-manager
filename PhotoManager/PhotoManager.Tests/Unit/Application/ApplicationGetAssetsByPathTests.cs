using FileNames = PhotoManager.Tests.Unit.Constants.FileNames;
using FileSize = PhotoManager.Tests.Unit.Constants.FileSize;
using Hashes = PhotoManager.Tests.Unit.Constants.Hashes;
using ModificationDate = PhotoManager.Tests.Unit.Constants.ModificationDate;
using PixelWidthAsset = PhotoManager.Tests.Unit.Constants.PixelWidthAsset;
using PixelHeightAsset = PhotoManager.Tests.Unit.Constants.PixelHeightAsset;
using ThumbnailWidthAsset = PhotoManager.Tests.Unit.Constants.ThumbnailWidthAsset;
using ThumbnailHeightAsset = PhotoManager.Tests.Unit.Constants.ThumbnailHeightAsset;

namespace PhotoManager.Tests.Unit.Application;

[TestFixture]
public class ApplicationGetAssetsByPathTests
{
    [Test]
    public void GetAssetsByPath_ValidDirectoryAndFolderExists_ReturnsAssetsArray()
    {
        const string directory = "D:\\Pictures";

        Guid folderId = Guid.NewGuid();
        Folder folder = new() { Id = folderId, Path = directory };

        Asset[] expectedResult =
        [
            new()
            {
                FolderId = folderId,
                Folder = folder,
                FileName = FileNames.IMAGE_1_JPG,
                Pixel = new()
                {
                    Asset = new() { Width = PixelWidthAsset.IMAGE_1_JPG, Height = PixelHeightAsset.IMAGE_1_JPG },
                    Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_1_JPG, Height = ThumbnailHeightAsset.IMAGE_1_JPG }
                },
                FileProperties = new()
                {
                    Size = FileSize.IMAGE_1_JPG,
                    Creation = DateTime.Now,
                    Modification = ModificationDate.Default
                },
                ThumbnailCreationDateTime = DateTime.Now,
                ImageRotation = Rotation.Rotate0,
                Hash = Hashes.IMAGE_1_JPG,
                Metadata = new()
                {
                    Corrupted = new() { IsTrue = false, Message = null },
                    Rotated = new() { IsTrue = false, Message = null }
                }
            },
            new()
            {
                FolderId = folderId,
                Folder = folder,
                FileName = FileNames.IMAGE_2_JPG,
                Pixel = new()
                {
                    Asset = new() { Width = PixelWidthAsset.IMAGE_2_JPG, Height = PixelHeightAsset.IMAGE_2_JPG },
                    Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_2_JPG, Height = ThumbnailHeightAsset.IMAGE_2_JPG }
                },
                FileProperties = new()
                {
                    Size = FileSize.IMAGE_2_JPG,
                    Creation = DateTime.Now,
                    Modification = ModificationDate.Default
                },
                ThumbnailCreationDateTime = DateTime.Now,
                ImageRotation = Rotation.Rotate0,
                Hash = Hashes.IMAGE_2_JPG,
                Metadata = new()
                {
                    Corrupted = new() { IsTrue = false, Message = null },
                    Rotated = new() { IsTrue = false, Message = null }
                }
            }
        ];

        Mock<IAssetRepository> assetRepositoryMock = new();
        assetRepositoryMock.Setup(x => x.FolderExists(directory)).Returns(true);
        assetRepositoryMock.Setup(x => x.GetAssetsByPath(directory)).Returns(expectedResult);

        Mock<ISyncAssetsService> syncAssetsServiceMock = new();
        Mock<ICatalogAssetsService> catalogAssetsServiceMock = new();
        Mock<IMoveAssetsService> moveAssetsServiceMock = new();
        Mock<IFindDuplicatedAssetsService> findDuplicatedAssetsServiceMock = new();
        Mock<IUserConfigurationService> userConfigurationServiceMock = new();
        Mock<IStorageService> storageServiceMock = new();

        PhotoManager.Application.Application application = new (
            assetRepositoryMock.Object,
            syncAssetsServiceMock.Object,
            catalogAssetsServiceMock.Object,
            moveAssetsServiceMock.Object,
            findDuplicatedAssetsServiceMock.Object,
            userConfigurationServiceMock.Object,
            storageServiceMock.Object);

        Asset[] assets = application.GetAssetsByPath(directory);

        Assert.That(assets, Has.Length.EqualTo(2));
        Assert.That(assets, Is.EquivalentTo(expectedResult));

        assetRepositoryMock.Verify(x => x.FolderExists(directory), Times.Once);
        assetRepositoryMock.Verify(x => x.AddFolder(directory), Times.Never);
        assetRepositoryMock.Verify(x => x.GetAssetsByPath(directory), Times.Once);
    }

    [Test]
    public void GetAssetsByPath_ValidDirectoryAndFolderDoesNotExist_ReturnsAssetsArray()
    {
        const string directory = "D:\\Pictures";

        Guid folderId = Guid.NewGuid();
        Folder folder = new() { Id = folderId, Path = directory };

        Asset[] expectedResult =
        [
            new()
            {
                FolderId = folderId,
                Folder = folder,
                FileName = FileNames.IMAGE_1_JPG,
                Pixel = new()
                {
                    Asset = new() { Width = PixelWidthAsset.IMAGE_1_JPG, Height = PixelHeightAsset.IMAGE_1_JPG },
                    Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_1_JPG, Height = ThumbnailHeightAsset.IMAGE_1_JPG }
                },
                FileProperties = new()
                {
                    Size = FileSize.IMAGE_1_JPG,
                    Creation = DateTime.Now,
                    Modification = ModificationDate.Default
                },
                ThumbnailCreationDateTime = DateTime.Now,
                ImageRotation = Rotation.Rotate0,
                Hash = Hashes.IMAGE_1_JPG,
                Metadata = new()
                {
                    Corrupted = new() { IsTrue = false, Message = null },
                    Rotated = new() { IsTrue = false, Message = null }
                }
            },
            new()
            {
                FolderId = folderId,
                Folder = folder,
                FileName = FileNames.IMAGE_2_JPG,
                Pixel = new()
                {
                    Asset = new() { Width = PixelWidthAsset.IMAGE_2_JPG, Height = PixelHeightAsset.IMAGE_2_JPG },
                    Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_2_JPG, Height = ThumbnailHeightAsset.IMAGE_2_JPG }
                },
                FileProperties = new()
                {
                    Size = FileSize.IMAGE_2_JPG,
                    Creation = DateTime.Now,
                    Modification = ModificationDate.Default
                },
                ThumbnailCreationDateTime = DateTime.Now,
                ImageRotation = Rotation.Rotate0,
                Hash = Hashes.IMAGE_2_JPG,
                Metadata = new()
                {
                    Corrupted = new() { IsTrue = false, Message = null },
                    Rotated = new() { IsTrue = false, Message = null }
                }
            }
        ];

        Mock<IAssetRepository> assetRepositoryMock = new();
        assetRepositoryMock.Setup(x => x.FolderExists(directory)).Returns(false);
        assetRepositoryMock.Setup(x => x.GetAssetsByPath(directory)).Returns(expectedResult);

        Mock<ISyncAssetsService> syncAssetsServiceMock = new();
        Mock<ICatalogAssetsService> catalogAssetsServiceMock = new();
        Mock<IMoveAssetsService> moveAssetsServiceMock = new();
        Mock<IFindDuplicatedAssetsService> findDuplicatedAssetsServiceMock = new();
        Mock<IUserConfigurationService> userConfigurationServiceMock = new();
        Mock<IStorageService> storageServiceMock = new();

        PhotoManager.Application.Application application = new (
            assetRepositoryMock.Object,
            syncAssetsServiceMock.Object,
            catalogAssetsServiceMock.Object,
            moveAssetsServiceMock.Object,
            findDuplicatedAssetsServiceMock.Object,
            userConfigurationServiceMock.Object,
            storageServiceMock.Object);

        Asset[] assets = application.GetAssetsByPath(directory);

        Assert.That(assets, Has.Length.EqualTo(2));
        Assert.That(assets, Is.EquivalentTo(expectedResult));

        assetRepositoryMock.Verify(x => x.FolderExists(directory), Times.Once);
        assetRepositoryMock.Verify(x => x.AddFolder(directory), Times.Once);
        assetRepositoryMock.Verify(x => x.GetAssetsByPath(directory), Times.Once);
    }

    [Test]
    [TestCase("", true)]
    [TestCase("", false)]
    [TestCase(" ", true)]
    [TestCase(" ", false)]
    [TestCase(null, true)]
    [TestCase(null, false)]
    public void GetAssetsByPath_DirectoryIsNullOrEmptyAndFolderExistsOrNot_ThrowsArgumentException(string? directory, bool folderExists)
    {
        Asset[] expectedResult = [];

        Mock<IAssetRepository> assetRepositoryMock = new();
        assetRepositoryMock.Setup(x => x.FolderExists(directory!)).Returns(folderExists);
        assetRepositoryMock.Setup(x => x.GetAssetsByPath(directory!)).Returns(expectedResult);

        Mock<ISyncAssetsService> syncAssetsServiceMock = new();
        Mock<ICatalogAssetsService> catalogAssetsServiceMock = new();
        Mock<IMoveAssetsService> moveAssetsServiceMock = new();
        Mock<IFindDuplicatedAssetsService> findDuplicatedAssetsServiceMock = new();
        Mock<IUserConfigurationService> userConfigurationServiceMock = new();
        Mock<IStorageService> storageServiceMock = new();

        PhotoManager.Application.Application application = new (
            assetRepositoryMock.Object,
            syncAssetsServiceMock.Object,
            catalogAssetsServiceMock.Object,
            moveAssetsServiceMock.Object,
            findDuplicatedAssetsServiceMock.Object,
            userConfigurationServiceMock.Object,
            storageServiceMock.Object);

        ArgumentException? exception = Assert.Throws<ArgumentException>(() => application.GetAssetsByPath(directory!));

        Assert.That(exception?.Message, Is.EqualTo("Directory cannot be null or empty."));

        assetRepositoryMock.Verify(x => x.FolderExists(directory!), Times.Never);
        assetRepositoryMock.Verify(x => x.AddFolder(directory!), Times.Never);
        assetRepositoryMock.Verify(x => x.GetAssetsByPath(directory!), Times.Never);
    }
}
