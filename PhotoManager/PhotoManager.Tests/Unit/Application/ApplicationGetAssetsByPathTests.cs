using FileNames = PhotoManager.Tests.Unit.Constants.FileNames;
using FileSize = PhotoManager.Tests.Unit.Constants.FileSize;
using Hashes = PhotoManager.Tests.Unit.Constants.Hashes;
using ModificationDate = PhotoManager.Tests.Unit.Constants.ModificationDate;
using PixelHeightAsset = PhotoManager.Tests.Unit.Constants.PixelHeightAsset;
using PixelWidthAsset = PhotoManager.Tests.Unit.Constants.PixelWidthAsset;
using ThumbnailHeightAsset = PhotoManager.Tests.Unit.Constants.ThumbnailHeightAsset;
using ThumbnailWidthAsset = PhotoManager.Tests.Unit.Constants.ThumbnailWidthAsset;

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
                    Thumbnail = new()
                    {
                        Width = ThumbnailWidthAsset.IMAGE_1_JPG,
                        Height = ThumbnailHeightAsset.IMAGE_1_JPG
                    }
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
                    Thumbnail = new()
                    {
                        Width = ThumbnailWidthAsset.IMAGE_2_JPG,
                        Height = ThumbnailHeightAsset.IMAGE_2_JPG
                    }
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

        IAssetRepository assetRepositoryMock = Substitute.For<IAssetRepository>();
        assetRepositoryMock.FolderExists(directory).Returns(true);
        assetRepositoryMock.GetAssetsByPath(directory).Returns(expectedResult);

        ISyncAssetsService syncAssetsServiceMock = Substitute.For<ISyncAssetsService>();
        ICatalogAssetsService catalogAssetsServiceMock = Substitute.For<ICatalogAssetsService>();
        IMoveAssetsService moveAssetsServiceMock = Substitute.For<IMoveAssetsService>();
        IFindDuplicatedAssetsService findDuplicatedAssetsServiceMock = Substitute.For<IFindDuplicatedAssetsService>();
        IUserConfigurationService userConfigurationServiceMock = Substitute.For<IUserConfigurationService>();
        IFileOperationsService fileOperationsServiceMock = Substitute.For<IFileOperationsService>();
        IImageProcessingService imageProcessingServiceMock = Substitute.For<IImageProcessingService>();

        PhotoManager.Application.Application application = new(
            assetRepositoryMock,
            syncAssetsServiceMock,
            catalogAssetsServiceMock,
            moveAssetsServiceMock,
            findDuplicatedAssetsServiceMock,
            userConfigurationServiceMock,
            fileOperationsServiceMock,
            imageProcessingServiceMock);

        Asset[] assets = application.GetAssetsByPath(directory);

        Assert.That(assets, Has.Length.EqualTo(2));
        Assert.That(assets, Is.EquivalentTo(expectedResult));

        assetRepositoryMock.Received(1).FolderExists(directory);
        assetRepositoryMock.DidNotReceive().AddFolder(directory);
        assetRepositoryMock.Received(1).GetAssetsByPath(directory);
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
                    Thumbnail = new()
                        { Width = ThumbnailWidthAsset.IMAGE_1_JPG, Height = ThumbnailHeightAsset.IMAGE_1_JPG }
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
                    Thumbnail = new()
                        { Width = ThumbnailWidthAsset.IMAGE_2_JPG, Height = ThumbnailHeightAsset.IMAGE_2_JPG }
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

        IAssetRepository assetRepositoryMock = Substitute.For<IAssetRepository>();
        assetRepositoryMock.FolderExists(directory).Returns(false);
        assetRepositoryMock.GetAssetsByPath(directory).Returns(expectedResult);

        ISyncAssetsService syncAssetsServiceMock = Substitute.For<ISyncAssetsService>();
        ICatalogAssetsService catalogAssetsServiceMock = Substitute.For<ICatalogAssetsService>();
        IMoveAssetsService moveAssetsServiceMock = Substitute.For<IMoveAssetsService>();
        IFindDuplicatedAssetsService findDuplicatedAssetsServiceMock = Substitute.For<IFindDuplicatedAssetsService>();
        IUserConfigurationService userConfigurationServiceMock = Substitute.For<IUserConfigurationService>();
        IFileOperationsService fileOperationsServiceMock = Substitute.For<IFileOperationsService>();
        IImageProcessingService imageProcessingServiceMock = Substitute.For<IImageProcessingService>();

        PhotoManager.Application.Application application = new(
            assetRepositoryMock,
            syncAssetsServiceMock,
            catalogAssetsServiceMock,
            moveAssetsServiceMock,
            findDuplicatedAssetsServiceMock,
            userConfigurationServiceMock,
            fileOperationsServiceMock,
            imageProcessingServiceMock);

        Asset[] assets = application.GetAssetsByPath(directory);

        Assert.That(assets, Has.Length.EqualTo(2));
        Assert.That(assets, Is.EquivalentTo(expectedResult));

        assetRepositoryMock.Received(1).FolderExists(directory);
        assetRepositoryMock.Received(1).AddFolder(directory);
        assetRepositoryMock.Received(1).GetAssetsByPath(directory);
    }

    [Test]
    [TestCase("", true)]
    [TestCase("", false)]
    [TestCase(" ", true)]
    [TestCase(" ", false)]
    [TestCase(null, true)]
    [TestCase(null, false)]
    public void GetAssetsByPath_DirectoryIsNullOrEmptyAndFolderExistsOrNot_ThrowsArgumentException(string? directory,
        bool folderExists)
    {
        Asset[] expectedResult = [];

        IAssetRepository assetRepositoryMock = Substitute.For<IAssetRepository>();
        assetRepositoryMock.FolderExists(directory!).Returns(folderExists);
        assetRepositoryMock.GetAssetsByPath(directory!).Returns(expectedResult);

        ISyncAssetsService syncAssetsServiceMock = Substitute.For<ISyncAssetsService>();
        ICatalogAssetsService catalogAssetsServiceMock = Substitute.For<ICatalogAssetsService>();
        IMoveAssetsService moveAssetsServiceMock = Substitute.For<IMoveAssetsService>();
        IFindDuplicatedAssetsService findDuplicatedAssetsServiceMock = Substitute.For<IFindDuplicatedAssetsService>();
        IUserConfigurationService userConfigurationServiceMock = Substitute.For<IUserConfigurationService>();
        IFileOperationsService fileOperationsServiceMock = Substitute.For<IFileOperationsService>();
        IImageProcessingService imageProcessingServiceMock = Substitute.For<IImageProcessingService>();

        PhotoManager.Application.Application application = new(
            assetRepositoryMock,
            syncAssetsServiceMock,
            catalogAssetsServiceMock,
            moveAssetsServiceMock,
            findDuplicatedAssetsServiceMock,
            userConfigurationServiceMock,
            fileOperationsServiceMock,
            imageProcessingServiceMock);

        ArgumentException? exception = Assert.Throws<ArgumentException>(() => application.GetAssetsByPath(directory!));

        Assert.That(exception?.Message, Is.EqualTo("Directory cannot be null or empty."));

        assetRepositoryMock.DidNotReceive().FolderExists(directory!);
        assetRepositoryMock.DidNotReceive().AddFolder(directory!);
        assetRepositoryMock.DidNotReceive().GetAssetsByPath(directory!);
    }
}
