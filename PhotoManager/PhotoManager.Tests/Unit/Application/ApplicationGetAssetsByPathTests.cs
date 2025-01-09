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
                FileName = "Image 1.jpg",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                FileProperties = new()
                {
                    Size = 29857,
                    Creation = DateTime.Now,
                    Modification = new (2024, 06, 07, 08, 54, 37)
                },
                ThumbnailCreationDateTime = DateTime.Now,
                ImageRotation = Rotation.Rotate0,
                Hash = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9",
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
                FileName = "Image 2.jpg",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                FileProperties = new()
                {
                    Size = 29857,
                    Creation = DateTime.Now,
                    Modification = new (2024, 06, 07, 08, 54, 37)
                },
                ThumbnailCreationDateTime = DateTime.Now,
                ImageRotation = Rotation.Rotate0,
                Hash = "0b6d010f85544871c307bb3a96028402f55fa29094908cdd0f74a8ec8d3fc3d4fbec995d98b89aafef3dcf5581c018fbb50481e33c7e45aef552d66c922f4078",
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
                FileName = "Image 1.jpg",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                FileProperties = new()
                {
                    Size = 29857,
                    Creation = DateTime.Now,
                    Modification = new (2024, 06, 07, 08, 54, 37)
                },
                ThumbnailCreationDateTime = DateTime.Now,
                ImageRotation = Rotation.Rotate0,
                Hash = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9",
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
                FileName = "Image 2.jpg",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                FileProperties = new()
                {
                    Size = 29857,
                    Creation = DateTime.Now,
                    Modification = new (2024, 06, 07, 08, 54, 37)
                },
                ThumbnailCreationDateTime = DateTime.Now,
                ImageRotation = Rotation.Rotate0,
                Hash = "0b6d010f85544871c307bb3a96028402f55fa29094908cdd0f74a8ec8d3fc3d4fbec995d98b89aafef3dcf5581c018fbb50481e33c7e45aef552d66c922f4078",
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
