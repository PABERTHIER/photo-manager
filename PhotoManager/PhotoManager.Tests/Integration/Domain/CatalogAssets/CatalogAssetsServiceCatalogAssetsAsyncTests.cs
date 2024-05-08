using PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;
using System.IO.Compression;

namespace PhotoManager.Tests.Integration.Domain.CatalogAssets;

[TestFixture]
public class CatalogAssetsServiceCatalogAssetsAsyncTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;
    private const string DATABASE_END_PATH = "v1.0";
    private string? _databaseBackupPath;
    private const string DATABASE_BACKUP_END_PATH = "v1.0_Backups";
    private string? _defaultAssetsDirectory;
    private const string FFMPEG_PATH = "E:\\ffmpeg\\bin\\ffmpeg.exe"; // TODO: Will be removed when the dll of Ffmpeg would have been generated and stored in the project

    private CatalogAssetsService? _catalogAssetsService;
    private BlobStorage? _blobStorage;
    private Database? _database;
    private UserConfigurationService? _userConfigurationService;
    private TestableAssetRepository? _testableAssetRepository;
    private Mock<IStorageService>? _storageServiceMock;

    private Asset? _asset1;
    private Asset? _asset2;
    private Asset? _asset3;
    private Asset? _asset4;

    private Asset? _asset1Temp;
    private Asset? _asset2Temp;
    private Asset? _asset3Temp;
    private Asset? _asset4Temp;
    private Asset? _asset5Temp;

    private const int ASSET1_IMAGE_BYTE_SIZE = 2097;
    private const int ASSET2_IMAGE_BYTE_SIZE = 11002;
    private const int ASSET3_IMAGE_BYTE_SIZE = 11002;
    private const int ASSET4_IMAGE_BYTE_SIZE = 5831;

    private const int ASSET1_TEMP_IMAGE_BYTE_SIZE = 2097;
    private const int ASSET2_TEMP_IMAGE_BYTE_SIZE = 2097;
    private const int ASSET3_TEMP_IMAGE_BYTE_SIZE = 8594;
    private const int ASSET4_TEMP_IMAGE_BYTE_SIZE = 4779;
    private const int ASSET5_TEMP_IMAGE_BYTE_SIZE = 4779;

    private const string CREATING_BACKUP_MESSAGE = "Creating catalog backup...";
    private const string UPDATING_BACKUP_MESSAGE = "Updating catalog backup...";

    // private Asset? asset8;
    // private Asset? asset9;
    // private Asset? asset10;
    // private Asset? asset11;
    // private Asset? asset12;
    // private Asset? asset13;
    // private Asset? asset14;
    // private Asset? asset15;
    // private Asset? asset16;
    // private Asset? asset17;
    // private Asset? asset18;
    // private Asset? asset19;
    // private Asset? asset20;
    // private Asset? asset21;
    // private Asset? asset22;
    // private Asset? asset23;
    // private Asset? asset24;
    // private Asset? asset25;
    // private Asset? asset26;
    // private Asset? asset27;
    // private Asset? asset28;
    // private Asset? asset29;
    // private Asset? asset30;
    // private Asset? asset31;
    // private Asset? asset32;
    // private Asset? asset33;
    // private Asset? asset34;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        _databaseDirectory = Path.Combine(_dataDirectory, "DatabaseTests");
        _databasePath = Path.Combine(_databaseDirectory, DATABASE_END_PATH);
        _databaseBackupPath = Path.Combine(_databaseDirectory, DATABASE_BACKUP_END_PATH);
        _defaultAssetsDirectory = Path.Combine(_dataDirectory, "Path");

        _storageServiceMock = new Mock<IStorageService>();
        _storageServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<double>())).Returns(_databasePath);
        _storageServiceMock.Setup(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new BitmapImage());

        _blobStorage = new();
        _database = new (new ObjectListStorage(), _blobStorage, new BackupStorage());
    }

    [SetUp]
    public void Setup()
    {
        _asset1 = new()
        {
            FileName = "Image 1_duplicate.jpg",
            FileSize = 29857,
            PixelHeight = 720,
            PixelWidth = 1280,
            ThumbnailPixelWidth = 200,
            ThumbnailPixelHeight = 112,
            ThumbnailCreationDateTime = new DateTime(2023, 01, 07, 00, 00, 00),
            ImageRotation = Rotation.Rotate0,
            Hash = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9",
            IsAssetCorrupted = false,
            AssetCorruptedMessage = null,
            IsAssetRotated = false,
            AssetRotatedMessage = null
        };
        _asset2 = new()
        {
            FileName = "Image 9.png",
            FileSize = 126277,
            PixelHeight = 720,
            PixelWidth = 1280,
            ThumbnailPixelWidth = 200,
            ThumbnailPixelHeight = 112,
            ThumbnailCreationDateTime = new DateTime(2023, 01, 07, 00, 00, 00),
            ImageRotation = Rotation.Rotate0,
            Hash = "bcc994c14aa314dbc2dfbf48ffd34fa628dadcd86cdb8efda113b94a9035f15956cf039f5858b74cd7f404e98f7e84d9821b39aaa6cbbdc73228fa74ad2a5c20",
            IsAssetCorrupted = false,
            AssetCorruptedMessage = null,
            IsAssetRotated = false,
            AssetRotatedMessage = null
        };
        _asset3 = new()
        {
            FileName = "Image 9_duplicate.png",
            FileSize = 126277,
            PixelHeight = 720,
            PixelWidth = 1280,
            ThumbnailPixelWidth = 200,
            ThumbnailPixelHeight = 112,
            ThumbnailCreationDateTime = new DateTime(2023, 01, 07, 00, 00, 00),
            ImageRotation = Rotation.Rotate0,
            Hash = "bcc994c14aa314dbc2dfbf48ffd34fa628dadcd86cdb8efda113b94a9035f15956cf039f5858b74cd7f404e98f7e84d9821b39aaa6cbbdc73228fa74ad2a5c20",
            IsAssetCorrupted = false,
            AssetCorruptedMessage = null,
            IsAssetRotated = false,
            AssetRotatedMessage = null
        };
        _asset4 = new()
        {
            FileName = "Image_11.heic",
            FileSize = 1411940,
            PixelHeight = 4032,
            PixelWidth = 3024,
            ThumbnailPixelWidth = 112,
            ThumbnailPixelHeight = 150,
            ThumbnailCreationDateTime = new DateTime(2024, 03, 24, 00, 00, 00),
            ImageRotation = Rotation.Rotate0,
            Hash = "f52bd860f5ad7f81a92919e5fb5769d3e86778b2ade74832fbd3029435c85e59cb64b3c2ce425445a49917953e6e913c72b81e48976041a4439cb65e92baf18d",
            IsAssetCorrupted = false,
            AssetCorruptedMessage = null,
            IsAssetRotated = false,
            AssetRotatedMessage = null
        };
        // TODO: Add ThumbnailCreationDateTime Property if needed below
        // asset2 = new()
        // {
        //     FileName = "Image 1_90_deg.jpg",
        //     FileSize = 31535,
        //     PixelHeight = 1280,
        //     PixelWidth = 720,
        //     ThumbnailPixelWidth = 84,
        //     ThumbnailPixelHeight = 150,
        //     ImageRotation = Rotation.Rotate90,
        //     Hash = "0092081bd0a83e9253f4eb319b4cfab197a09ed3b28b5697c31f17bf2646638dec95bf28d4f454dda8ff3dcdaab33fa002774f13dfe6d7aee82f1a960e3e3aee",
        //     IsAssetCorrupted = false,
        //     AssetCorruptedMessage = null,
        //     IsAssetRotated = true,
        //     AssetRotatedMessage = "The asset has been rotated"
        // };
        //
        // asset3 = new()
        // {
        //     FileName = "Image 1_180_deg.jpg",
        //     FileSize = 31535,
        //     PixelHeight = 720,
        //     PixelWidth = 1280,
        //     ThumbnailPixelWidth = 200,
        //     ThumbnailPixelHeight = 112,
        //     ImageRotation = Rotation.Rotate180,
        //     Hash = "399383d82d4b7ed8233f714a1d14272b276b80505d7b3a2935b6ec962fd5cf280ccedb3e9380df1a11ee65e5f5fa66278dd2f407b7b6bdfa3093319164c446dd",
        //     IsAssetCorrupted = false,
        //     AssetCorruptedMessage = null,
        //     IsAssetRotated = true,
        //     AssetRotatedMessage = "The asset has been rotated"
        // };
        //
        // asset4 = new()
        // {
        //     FileName = "Image 1_270_deg.jpg",
        //     FileSize = 31535,
        //     PixelHeight = 1280,
        //     PixelWidth = 720,
        //     ThumbnailPixelWidth = 84,
        //     ThumbnailPixelHeight = 150,
        //     ImageRotation = Rotation.Rotate270,
        //     Hash = "43950e06cecd509509d868a2064a828aeb6ff6a9a30f296ec9a3621b269742e75dacb40300aa6927c4c8f0879268e1ab3d7eac228b38cade35cc9b7c6adf3449",
        //     IsAssetCorrupted = false,
        //     AssetCorruptedMessage = null,
        //     IsAssetRotated = true,
        //     AssetRotatedMessage = "The asset has been rotated"
        // };
        //
        // asset5 = new()
        // {
        //     FileName = "Image 2.jpg",
        //     FileSize = 30197,
        //     PixelHeight = 720,
        //     PixelWidth = 1280,
        //     ThumbnailPixelWidth = 200,
        //     ThumbnailPixelHeight = 112,
        //     ImageRotation = Rotation.Rotate0,
        //     Hash = "0b6d010f85544871c307bb3a96028402f55fa29094908cdd0f74a8ec8d3fc3d4fbec995d98b89aafef3dcf5581c018fbb50481e33c7e45aef552d66c922f4078",
        //     IsAssetCorrupted = false,
        //     AssetCorruptedMessage = null,
        //     IsAssetRotated = false,
        //     AssetRotatedMessage = null
        // };
        //
        // asset6 = new()
        // {
        //     FileName = "Image 2 duplicated.jpg",
        //     FileSize = 30197,
        //     PixelHeight = 720,
        //     PixelWidth = 1280,
        //     ThumbnailPixelWidth = 200,
        //     ThumbnailPixelHeight = 112,
        //     ImageRotation = Rotation.Rotate0,
        //     Hash = "0b6d010f85544871c307bb3a96028402f55fa29094908cdd0f74a8ec8d3fc3d4fbec995d98b89aafef3dcf5581c018fbb50481e33c7e45aef552d66c922f4078",
        //     IsAssetCorrupted = false,
        //     AssetCorruptedMessage = null,
        //     IsAssetRotated = false,
        //     AssetRotatedMessage = null
        // };
        //
        // asset7 = new()
        // {
        //     FileName = "Image 8.jpeg",
        //     FileSize = 30514,
        //     PixelHeight = 720,
        //     PixelWidth = 1280,
        //     ThumbnailPixelWidth = 200,
        //     ThumbnailPixelHeight = 112,
        //     ImageRotation = Rotation.Rotate0,
        //     Hash = "156f81ceb3db1bfdf27618efe4b89575f17d4708053c1a9ce6c5dfb7f36a6b958a5c3f45147728de28cf538345336e7b81d01b3f5908d4677ce76e1661e97ac3",
        //     IsAssetCorrupted = false,
        //     AssetCorruptedMessage = null,
        //     IsAssetRotated = false,
        //     AssetRotatedMessage = null
        // };
        //
        // asset8 = new()
        // {
        //     FileName = "Image 9.png",
        //     FileSize = 126277,
        //     PixelHeight = 720,
        //     PixelWidth = 1280,
        //     ThumbnailPixelWidth = 200,
        //     ThumbnailPixelHeight = 112,
        //     ImageRotation = Rotation.Rotate0,
        //     Hash = "bcc994c14aa314dbc2dfbf48ffd34fa628dadcd86cdb8efda113b94a9035f15956cf039f5858b74cd7f404e98f7e84d9821b39aaa6cbbdc73228fa74ad2a5c20",
        //     IsAssetCorrupted = false,
        //     AssetCorruptedMessage = null,
        //     IsAssetRotated = false,
        //     AssetRotatedMessage = null
        // };
        //
        // asset9 = new()
        // {
        //     FileName = "Image 10 portrait.png",
        //     FileSize = 153318,
        //     PixelHeight = 1280,
        //     PixelWidth = 720,
        //     ThumbnailPixelWidth = 84,
        //     ThumbnailPixelHeight = 150,
        //     ImageRotation = Rotation.Rotate0,
        //     Hash = "7ad01e9fe639957a9e8eaddc7fd864068f4a03f9981fd480f310740a43a0a4f2b8fa7a80d9c83039c46fcfbb63a5e465adaf07d33191369590adcda1586b1c94",
        //     IsAssetCorrupted = false,
        //     AssetCorruptedMessage = null,
        //     IsAssetRotated = false,
        //     AssetRotatedMessage = null
        // };
        //
        // asset10 = new()
        // {
        //     FileName = "Image_11.heic",
        //     FileSize = 1411940,
        //     PixelHeight = 4032,
        //     PixelWidth = 3024,
        //     ThumbnailPixelWidth = 112,
        //     ThumbnailPixelHeight = 150,
        //     ImageRotation = Rotation.Rotate0,
        //     Hash = "f52bd860f5ad7f81a92919e5fb5769d3e86778b2ade74832fbd3029435c85e59cb64b3c2ce425445a49917953e6e913c72b81e48976041a4439cb65e92baf18d",
        //     IsAssetCorrupted = false,
        //     AssetCorruptedMessage = null,
        //     IsAssetRotated = false,
        //     AssetRotatedMessage = null
        // };
        //
        // asset11 = new()
        // {
        //     FileName = "Image_11_90.heic",
        //     FileSize = 1411940,
        //     PixelHeight = 3024,
        //     PixelWidth = 4032,
        //     ThumbnailPixelWidth = 200,
        //     ThumbnailPixelHeight = 150,
        //     ImageRotation = Rotation.Rotate90,
        //     Hash = "b75df4942de1efa7d4326fedd721e99cc43959a1c22098a8cfcf6861aea099741d9c98fa1615e54d53838c22b9c6ff3b328c4ee3c0875d2b4ac7f7dee8d61bcf",
        //     IsAssetCorrupted = false,
        //     AssetCorruptedMessage = null,
        //     IsAssetRotated = true,
        //     AssetRotatedMessage = "The asset has been rotated"
        // };
        //
        // asset12 = new()
        // {
        //     FileName = "Image_11_180.heic",
        //     FileSize = 1423616,
        //     PixelHeight = 4032,
        //     PixelWidth = 3024,
        //     ThumbnailPixelWidth = 112,
        //     ThumbnailPixelHeight = 150,
        //     ImageRotation = Rotation.Rotate180,
        //     Hash = "a2ad3ac9d51f74230b9f1d554172781d9cc214d01f8344354e2e56dc3daecb5a0021d4d2814eba0cde6fd681f3a8e8b8b55fbfad7184740e3518190e05dca77f",
        //     IsAssetCorrupted = false,
        //     AssetCorruptedMessage = null,
        //     IsAssetRotated = true,
        //     AssetRotatedMessage = "The asset has been rotated"
        // };
        //
        // asset13 = new()
        // {
        //     FileName = "Image_11_270.heic",
        //     FileSize = 1423616,
        //     PixelHeight = 3024,
        //     PixelWidth = 4032,
        //     ThumbnailPixelWidth = 200,
        //     ThumbnailPixelHeight = 150,
        //     ImageRotation = Rotation.Rotate270,
        //     Hash = "d4d7342a783f0f1b5e10ad9b493b71dd688dbc31b8ddd5534c9fff9c5fdbfa8bee32ef4e4258f7514a1b57f9a12b571454c8b66172b1a66bf1b53953b66227d5",
        //     IsAssetCorrupted = false,
        //     AssetCorruptedMessage = null,
        //     IsAssetRotated = true,
        //     AssetRotatedMessage = "The asset has been rotated"
        // };
        //
        // asset14 = new()
        // {
        //     FileName = "IMAGE_WITH_UPPERCASE_NAME.JPG",
        //     FileSize = 40937,
        //     PixelHeight = 720,
        //     PixelWidth = 1280,
        //     ThumbnailPixelWidth = 200,
        //     ThumbnailPixelHeight = 112,
        //     ImageRotation = Rotation.Rotate0,
        //     Hash = "0465fc2af396e003a3f45bf71e951b905ca7d19c21c36d47cd7b2524190862e38cb4c0da6fdd1dbd89b5b7050d837be664bc45693657ec7681e4d9b648044c91",
        //     IsAssetCorrupted = false,
        //     AssetCorruptedMessage = null,
        //     IsAssetRotated = false,
        //     AssetRotatedMessage = null
        // };
        //
        // asset16 = new()
        // {
        //     FileName = "1336_BottomLeftPart.JPG",
        //     FileSize = 1119873,
        //     PixelHeight = 1512,
        //     PixelWidth = 2016,
        //     ThumbnailPixelWidth = 200,
        //     ThumbnailPixelHeight = 150,
        //     ImageRotation = Rotation.Rotate0,
        //     Hash = "c50e17cee9a52c0235afb410c361d094daf7330c20e4b3f3cb3e3e18683793355e903f84d49bf1db4af7e094980204cc005aa3c503829d14c9e6cd19826e284f",
        //     IsAssetCorrupted = false,
        //     AssetCorruptedMessage = null,
        //     IsAssetRotated = false,
        //     AssetRotatedMessage = null
        // };
        //
        // asset17 = new()
        // {
        //     FileName = "1336_BottomPart.JPG",
        //     FileSize = 2172866,
        //     PixelHeight = 1512,
        //     PixelWidth = 4032,
        //     ThumbnailPixelWidth = 200,
        //     ThumbnailPixelHeight = 75,
        //     ImageRotation = Rotation.Rotate0,
        //     Hash = "aef59f88b3c54a5fd1bfc098acac1d662bb3799e83220049ee7c3ae5def487a1464112cce7b7384428ee9b69813448d509413a1fc1abcddc272ec2e044c5f876",
        //     IsAssetCorrupted = false,
        //     AssetCorruptedMessage = null,
        //     IsAssetRotated = false,
        //     AssetRotatedMessage = null
        // };
        //
        // asset18 = new()
        // {
        //     FileName = "1336_BottomRightPart.JPG",
        //     FileSize = 1061673,
        //     PixelHeight = 1512,
        //     PixelWidth = 2016,
        //     ThumbnailPixelWidth = 200,
        //     ThumbnailPixelHeight = 150,
        //     ImageRotation = Rotation.Rotate0,
        //     Hash = "966370f38d7864499453b99934bb51f1654e567033e637b68ded1ec12c338eaa03d42bb4dc74621551c5dfd922180a291f7c58980bf6c94ae114e4d296a484e4",
        //     IsAssetCorrupted = false,
        //     AssetCorruptedMessage = null,
        //     IsAssetRotated = false,
        //     AssetRotatedMessage = null
        // };
        //
        // asset19 = new()
        // {
        //     FileName = "1336_LeftPart.JPG",
        //     FileSize = 1475892,
        //     PixelHeight = 3024,
        //     PixelWidth = 2016,
        //     ThumbnailPixelWidth = 100,
        //     ThumbnailPixelHeight = 150,
        //     ImageRotation = Rotation.Rotate0,
        //     Hash = "8afa3f673b047d5096ed2a2528b37c7222ab5b44cf780a7920cea35228e93323709b09b1a2ffe1bd2f789061be9bef8ebcb642fc47a2f3b6573a68a8cd5045ee",
        //     IsAssetCorrupted = false,
        //     AssetCorruptedMessage = null,
        //     IsAssetRotated = false,
        //     AssetRotatedMessage = null
        // };
        //
        // asset20 = new()
        // {
        //     FileName = "1336_Original.JPG",
        //     FileSize = 4526710,
        //     PixelHeight = 3024,
        //     PixelWidth = 4032,
        //     ThumbnailPixelWidth = 200,
        //     ThumbnailPixelHeight = 150,
        //     ImageRotation = Rotation.Rotate0,
        //     Hash = "fb0bb3887c28d022113c5c52a65f9da14eab0806842f902b618806dedeb7c53da1ed26ae5bcaf9a358e81efead71c4807ea38054e44f426857f8bf2c1fd1c7aa",
        //     IsAssetCorrupted = false,
        //     AssetCorruptedMessage = null,
        //     IsAssetRotated = false,
        //     AssetRotatedMessage = null
        // };
        //
        // asset21 = new()
        // {
        //     FileName = "1336_RightPart.JPG",
        //     FileSize = 1437903,
        //     PixelHeight = 3024,
        //     PixelWidth = 2016,
        //     ThumbnailPixelWidth = 100,
        //     ThumbnailPixelHeight = 150,
        //     ImageRotation = Rotation.Rotate0,
        //     Hash = "0e0e12ea70894ad521c8d3da14b92bbd4e7c7c03010502d779f2e485ea30e917ad6a9bc0435520f70c3448fd96cd9ccf88cf95d2a75a6da669b0f41731aabbe7",
        //     IsAssetCorrupted = false,
        //     AssetCorruptedMessage = null,
        //     IsAssetRotated = false,
        //     AssetRotatedMessage = null
        // };
        //
        // asset22 = new()
        // {
        //     FileName = "1336_TopLeftPart.JPG",
        //     FileSize = 362012,
        //     PixelHeight = 1512,
        //     PixelWidth = 2016,
        //     ThumbnailPixelWidth = 200,
        //     ThumbnailPixelHeight = 150,
        //     ImageRotation = Rotation.Rotate0,
        //     Hash = "f59b3913bdd53b6cb485a2c04770d1153350df2caf6dabf38b8b8311fb3fb583becbf7d368a1e0987d3584251a64bca72042d334b6e6c25edb7dfaead2db383d",
        //     IsAssetCorrupted = false,
        //     AssetCorruptedMessage = null,
        //     IsAssetRotated = false,
        //     AssetRotatedMessage = null
        // };
        //
        // asset23 = new()
        // {
        //     FileName = "1336_TopPart.JPG",
        //     FileSize = 737269,
        //     PixelHeight = 1512,
        //     PixelWidth = 4032,
        //     ThumbnailPixelWidth = 200,
        //     ThumbnailPixelHeight = 75,
        //     ImageRotation = Rotation.Rotate0,
        //     Hash = "93d6ea1575ea8fd4f540b88ae3ce727d28c657a11388c9b6cf927572abffdcc467d3e9b43e622b34158c18ed680c841d43e17a80c7b544261f106d887b51d2d2",
        //     IsAssetCorrupted = false,
        //     AssetCorruptedMessage = null,
        //     IsAssetRotated = false,
        //     AssetRotatedMessage = null
        // };
        //
        // asset24 = new()
        // {
        //     FileName = "1336_TopRightPart.JPG",
        //     FileSize = 383508,
        //     PixelHeight = 1512,
        //     PixelWidth = 2016,
        //     ThumbnailPixelWidth = 200,
        //     ThumbnailPixelHeight = 150,
        //     ImageRotation = Rotation.Rotate0,
        //     Hash = "3fde05f0e85d71a909fa38351411f3d45305857ea7a920ad22bcffaf4cf50136b4a55acb128bdc9cd9d7db600332f83e4157926a511939b0b57f2e50c020e3d7",
        //     IsAssetCorrupted = false,
        //     AssetCorruptedMessage = null,
        //     IsAssetRotated = false,
        //     AssetRotatedMessage = null
        // };
        //
        // asset25 = new()
        // {
        //     FileName = "1336_1K.JPG",
        //     FileSize = 311067,
        //     PixelHeight = 768,
        //     PixelWidth = 1024,
        //     ThumbnailPixelWidth = 200,
        //     ThumbnailPixelHeight = 150,
        //     ImageRotation = Rotation.Rotate0,
        //     Hash = "4dcbbf63585dc9fff056a0370ee33efc3d7c57b556bce917f89c5012fbacbc1ee2929f7d3a11ee2f8af426b183212d67167d87e48b80707ab5a1fb4e766d2544",
        //     IsAssetCorrupted = false,
        //     AssetCorruptedMessage = null,
        //     IsAssetRotated = false,
        //     AssetRotatedMessage = null
        // };
        //
        // asset26 = new()
        // {
        //     FileName = "1336_2K.JPG",
        //     FileSize = 1154116,
        //     PixelHeight = 1536,
        //     PixelWidth = 2048,
        //     ThumbnailPixelWidth = 200,
        //     ThumbnailPixelHeight = 150,
        //     ImageRotation = Rotation.Rotate0,
        //     Hash = "76a8f0b611fdd32b73bff92a186449a7e4bf0b84d618fe8b9666bbf2b022ff6bf259a4a98be42db7d9043ff3c3ff20ae720c5067966c40d16ad3fc67769f514b",
        //     IsAssetCorrupted = false,
        //     AssetCorruptedMessage = null,
        //     IsAssetRotated = false,
        //     AssetRotatedMessage = null
        // };
        //
        // asset27 = new()
        // {
        //     FileName = "1336_3K.JPG",
        //     FileSize = 2098339,
        //     PixelHeight = 2160,
        //     PixelWidth = 2880,
        //     ThumbnailPixelWidth = 200,
        //     ThumbnailPixelHeight = 150,
        //     ImageRotation = Rotation.Rotate0,
        //     Hash = "98044e6674187219ab629c901dfdf5a72f2535104d83cd167d1bdd9039ded91b7a585b74c5c89860f2e0fab85af9e2181265cba48b908028a42b8ec042265f03",
        //     IsAssetCorrupted = false,
        //     AssetCorruptedMessage = null,
        //     IsAssetRotated = false,
        //     AssetRotatedMessage = null
        // };
        //
        // asset28 = new()
        // {
        //     FileName = "1336_4K_Original.JPG",
        //     FileSize = 4526710,
        //     PixelHeight = 3024,
        //     PixelWidth = 4032,
        //     ThumbnailPixelWidth = 200,
        //     ThumbnailPixelHeight = 150,
        //     ImageRotation = Rotation.Rotate0,
        //     Hash = "fb0bb3887c28d022113c5c52a65f9da14eab0806842f902b618806dedeb7c53da1ed26ae5bcaf9a358e81efead71c4807ea38054e44f426857f8bf2c1fd1c7aa",
        //     IsAssetCorrupted = false,
        //     AssetCorruptedMessage = null,
        //     IsAssetRotated = false,
        //     AssetRotatedMessage = null
        // };
        //
        // asset29 = new()
        // {
        //     FileName = "1336_8K.JPG",
        //     FileSize = 8855421,
        //     PixelHeight = 5760,
        //     PixelWidth = 7680,
        //     ThumbnailPixelWidth = 200,
        //     ThumbnailPixelHeight = 150,
        //     ImageRotation = Rotation.Rotate0,
        //     Hash = "18289324fb22b8b337b1f4a7d627c605056e04fd308699bc831af0579cbc2d6a5959811dbecce09b26f82c341ea82d02d5c99c1580f50c24106d19c88bd6eba2",
        //     IsAssetCorrupted = false,
        //     AssetCorruptedMessage = null,
        //     IsAssetRotated = false,
        //     AssetRotatedMessage = null
        // };
        //
        // asset30 = new()
        // {
        //     FileName = "1336_Thumbnail.JPG",
        //     FileSize = 19472,
        //     PixelHeight = 152,
        //     PixelWidth = 202,
        //     ThumbnailPixelWidth = 200,
        //     ThumbnailPixelHeight = 150,
        //     ImageRotation = Rotation.Rotate0,
        //     Hash = "0bbabdb2fe28d4ef53b78a917eb89938fe564d758ebb9cb011e5a675dbee0a8e92bc38b8fe0762e279d48e95e7d17f1bb392fd96e8db7006e9e0bb60c5d2ed31",
        //     IsAssetCorrupted = false,
        //     AssetCorruptedMessage = null,
        //     IsAssetRotated = false,
        //     AssetRotatedMessage = null
        // };
        //
        // asset31 = new()
        // {
        //     FileName = "Image_1336_Mini.JPG",
        //     FileSize = 1586,
        //     PixelHeight = 32,
        //     PixelWidth = 43,
        //     ThumbnailPixelWidth = 200,
        //     ThumbnailPixelHeight = 149,
        //     ImageRotation = Rotation.Rotate0,
        //     Hash = "768d0fd37e280f1ef8d2a17702626ca1b290521167c22b0aecadd3cc2f858a88ed59a72f7a4b36c123f487da69842e55e4407de0dd248076947ce7df53e92be4",
        //     IsAssetCorrupted = false,
        //     AssetCorruptedMessage = null,
        //     IsAssetRotated = false,
        //     AssetRotatedMessage = null
        // };
        //
        // asset32 = new()
        // {
        //     FileName = "Image_1336_Original.JPG",
        //     FileSize = 4526710,
        //     PixelHeight = 3024,
        //     PixelWidth = 4032,
        //     ThumbnailPixelWidth = 200,
        //     ThumbnailPixelHeight = 150,
        //     ImageRotation = Rotation.Rotate0,
        //     Hash = "fb0bb3887c28d022113c5c52a65f9da14eab0806842f902b618806dedeb7c53da1ed26ae5bcaf9a358e81efead71c4807ea38054e44f426857f8bf2c1fd1c7aa",
        //     IsAssetCorrupted = false,
        //     AssetCorruptedMessage = null,
        //     IsAssetRotated = false,
        //     AssetRotatedMessage = null
        // };
        //
        // asset33 = new()
        // {
        //     FileName = "Image_1336_ShitQuality.JPG",
        //     FileSize = 370819,
        //     PixelHeight = 3024,
        //     PixelWidth = 4032,
        //     ThumbnailPixelWidth = 200,
        //     ThumbnailPixelHeight = 150,
        //     ImageRotation = Rotation.Rotate0,
        //     Hash = "0861602a84f5bf56e5f4acf49c1d98da8129542e871208a9c9f25eb497cdc65a75ccf4941874c2be7f4e36309053fe34d61a42b4d1b266910918a0b68f19cfc2",
        //     IsAssetCorrupted = false,
        //     AssetCorruptedMessage = null,
        //     IsAssetRotated = false,
        //     AssetRotatedMessage = null
        // };
        //
        // asset34 = new()
        // {
        //     FileName = "Image_1336_Small.JPG",
        //     FileSize = 53227,
        //     PixelHeight = 443,
        //     PixelWidth = 591,
        //     ThumbnailPixelWidth = 200,
        //     ThumbnailPixelHeight = 150,
        //     ImageRotation = Rotation.Rotate0,
        //     Hash = "787146674307ab1e867c0961968dfea28aafc7d45e6cfd83f1562fbd201d029812289cd692c36a4f537c576b25af03c9f6f3cc996a8651f3c69f8240ecb3d5f4",
        //     IsAssetCorrupted = false,
        //     AssetCorruptedMessage = null,
        //     IsAssetRotated = false,
        //     AssetRotatedMessage = null
        // };
        _asset1Temp = new()
        {
            FileName = "Image 1_duplicate_copied.jpg",
            FileSize = 29857,
            PixelHeight = 720,
            PixelWidth = 1280,
            ThumbnailPixelWidth = 200,
            ThumbnailPixelHeight = 112,
            ThumbnailCreationDateTime = new DateTime(2023, 01, 07, 00, 00, 00),
            ImageRotation = Rotation.Rotate0,
            Hash = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9",
            IsAssetCorrupted = false,
            AssetCorruptedMessage = null,
            IsAssetRotated = false,
            AssetRotatedMessage = null
        };
        _asset2Temp = new()
        {
            FileName = "Image 1.jpg",
            FileSize = 29857,
            PixelHeight = 720,
            PixelWidth = 1280,
            ThumbnailPixelWidth = 200,
            ThumbnailPixelHeight = 112,
            ThumbnailCreationDateTime = new DateTime(2023, 01, 07, 00, 00, 00),
            ImageRotation = Rotation.Rotate0,
            Hash = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9",
            IsAssetCorrupted = false,
            AssetCorruptedMessage = null,
            IsAssetRotated = false,
            AssetRotatedMessage = null
        };
        _asset3Temp = new()
        {
            FileName = "Homer.gif",
            FileSize = 64123,
            PixelHeight = 320,
            PixelWidth = 320,
            ThumbnailPixelWidth = 150,
            ThumbnailPixelHeight = 150,
            ThumbnailCreationDateTime = new DateTime(2023, 08, 05, 00, 00, 00),
            ImageRotation = Rotation.Rotate0,
            Hash = "c48b1f61f3a3a004f425d8493d30a50ae14408ed4c5354bf4d0ca40069f91951381a7df32ee7455a6edef0996c95571557a9993021331ff2dfbc3ccc7f0c8ff1",
            IsAssetCorrupted = false,
            AssetCorruptedMessage = null,
            IsAssetRotated = false,
            AssetRotatedMessage = null
        };
        _asset4Temp = new()
        {
            FileName = "Homer.jpg",
            FileSize = 6599,
            PixelHeight = 180,
            PixelWidth = 320,
            ThumbnailPixelWidth = 200,
            ThumbnailPixelHeight = 112,
            ThumbnailCreationDateTime = DateTime.Now,
            ImageRotation = Rotation.Rotate0,
            Hash = "59742f2cd31c0997be96f9e758799d975f5918f7732f351d66280a708681ea74ccbfa1b61a327835a3f1dbb5ea5f9989484764a10f56f7dd6f32f7b24e286d66",
            IsAssetCorrupted = false,
            AssetCorruptedMessage = null,
            IsAssetRotated = false,
            AssetRotatedMessage = null
        };
        _asset5Temp = new()
        {
            FileName = "HomerDuplicated.jpg",
            FileSize = 6599,
            PixelHeight = 180,
            PixelWidth = 320,
            ThumbnailPixelWidth = 200,
            ThumbnailPixelHeight = 112,
            ThumbnailCreationDateTime = DateTime.Now,
            ImageRotation = Rotation.Rotate0,
            Hash = "59742f2cd31c0997be96f9e758799d975f5918f7732f351d66280a708681ea74ccbfa1b61a327835a3f1dbb5ea5f9989484764a10f56f7dd6f32f7b24e286d66",
            IsAssetCorrupted = false,
            AssetCorruptedMessage = null,
            IsAssetRotated = false,
            AssetRotatedMessage = null
        };
    }

    private void ConfigureCatalogAssetService(int catalogBatchSize, string assetsDirectory, int thumbnailMaxWidth, int thumbnailMaxHeight, bool usingDHash, bool usingMD5Hash, bool usingPHash, bool analyseVideos)
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
        configurationRootMock.MockGetValue(UserConfigurationKeys.FFMPEG_PATH, FFMPEG_PATH);

        _userConfigurationService = new (configurationRootMock.Object);
        _testableAssetRepository = new (_database!, _storageServiceMock!.Object, _userConfigurationService);
        StorageService storageService = new (_userConfigurationService);
        AssetHashCalculatorService assetHashCalculatorService = new (_userConfigurationService);
        DirectoryComparer directoryComparer = new (storageService);
        _catalogAssetsService = new (_testableAssetRepository, assetHashCalculatorService, storageService, _userConfigurationService, directoryComparer);
    }

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public async Task CatalogAssetsAsync_AssetsAndRootCatalogFolderExists_SyncTheAssets(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder2");

        ConfigureCatalogAssetService(100, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        try
        {
            string imagePath1 = Path.Combine(assetsDirectory, "Image 1_duplicate.jpg");
            string imagePath2 = Path.Combine(assetsDirectory, "Image 9.png");
            string imagePath3 = Path.Combine(assetsDirectory, "Image 9_duplicate.png");
            string imagePath4 = Path.Combine(assetsDirectory, "Image_11.heic");

            List<string> assetPaths = [imagePath1, imagePath2, imagePath3, imagePath4];
            List<Asset> expectedAssets = [_asset1!, _asset2!, _asset3!, _asset4!];
            List<int> assetsImageByteSize = [ASSET1_IMAGE_BYTE_SIZE, ASSET2_IMAGE_BYTE_SIZE, ASSET3_IMAGE_BYTE_SIZE, ASSET4_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(4, assetsInDirectory.Length);

            foreach (string assetPath in assetPaths)
            {
                Assert.IsTrue(File.Exists(assetPath));
            }

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNull(folder);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CheckBackupBefore(backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _catalogAssetsService!.CatalogAssetsAsync(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            _asset1!.Folder = folder!;
            _asset1!.FolderId = folder!.FolderId;
            _asset2!.Folder = folder;
            _asset2!.FolderId = folder.FolderId;
            _asset3!.Folder = folder;
            _asset3!.FolderId = folder.FolderId;
            _asset4!.Folder = folder;
            _asset4!.FolderId = folder.FolderId;

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(4, assetsFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(4, assetsFromRepository.Count);

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssets[i], assetPaths[i], assetsDirectory, folder);
            }

            Dictionary<Folder, List<Asset>> folderToAssetsMapping = new() { { folder, expectedAssets } };
            Dictionary<string, int> assetNameToByteSizeMapping = new()
            {
                { _asset1!.FileName, ASSET1_IMAGE_BYTE_SIZE },
                { _asset2!.FileName, ASSET2_IMAGE_BYTE_SIZE },
                { _asset3!.FileName, ASSET3_IMAGE_BYTE_SIZE },
                { _asset4!.FileName, ASSET4_IMAGE_BYTE_SIZE }
            };

            AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMapping, [folder], thumbnails, assetsImageByteSize);
            CheckBlobsAndTablesAfterSaveCatalog(
                blobsPath,
                tablesPath,
                [folder],
                [folder],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            CheckBackupAfter(
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder],
                [folder],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.AreEqual(9, catalogChanges.Count);

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            foreach (Asset expectedAsset in folderToAssetsMapping[folder])
            {
                CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder],
                    expectedAsset,
                    folder,
                    ref increment);
            }

            CheckCatalogChangesBackup(catalogChanges, CREATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task CatalogAssetsAsync_AssetsImageAndVideosAndRootCatalogFolderExists_SyncTheAssets()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "TempAssetsDirectory");

        ConfigureCatalogAssetService(100, assetsDirectory, 200, 150, false, false, false, true);

        try
        {
            Directory.CreateDirectory(assetsDirectory);

            string imagePath1 = Path.Combine(_dataDirectory!, "Image 1.jpg");
            string imagePath2 = Path.Combine(_dataDirectory!, "Homer.gif");
            string videoPath1 = Path.Combine(_dataDirectory!, "Homer.mp4");
            string videoPath2 = Path.Combine(_dataDirectory!, "Homer1s.mp4");

            string imagePath1ToCopy = Path.Combine(assetsDirectory, "Image 1.jpg");
            string imagePath2ToCopy = Path.Combine(assetsDirectory, "Homer.gif");
            string videoPath1ToCopy = Path.Combine(assetsDirectory, "Homer.mp4");
            string videoPath2ToCopy = Path.Combine(assetsDirectory, "Homer1s.mp4");

            string firstFrameVideosDirectory = _userConfigurationService!.PathSettings.FirstFrameVideosPath;
            string firstFramePath1 = Path.Combine(firstFrameVideosDirectory, "Homer.jpg");

            File.Copy(imagePath1, imagePath1ToCopy);
            File.Copy(imagePath2, imagePath2ToCopy);
            File.Copy(videoPath1, videoPath1ToCopy);
            File.Copy(videoPath2, videoPath2ToCopy);

            List<string> assetPaths = [imagePath1ToCopy, imagePath2ToCopy, videoPath1ToCopy, videoPath2ToCopy];
            List<string> assetPathsAfterSync = [imagePath2ToCopy, imagePath1ToCopy, firstFramePath1];
            List<Asset> expectedAssets = [_asset3Temp!, _asset2Temp!, _asset4Temp!];
            List<int> assetsImageByteSize = [ASSET3_TEMP_IMAGE_BYTE_SIZE, ASSET2_TEMP_IMAGE_BYTE_SIZE, ASSET4_TEMP_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(4, assetsInDirectory.Length);

            foreach (string assetPath in assetPaths)
            {
                Assert.IsTrue(File.Exists(assetPath));
            }

            Assert.IsFalse(File.Exists(firstFramePath1));

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNull(folder);

            Folder? videoFirstFramefolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.IsNull(videoFirstFramefolder);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CheckBackupBefore(backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            List<Asset> videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.IsEmpty(videoFirstFramesFromRepositoryByPath);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _catalogAssetsService!.CatalogAssetsAsync(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            videoFirstFramefolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.IsNotNull(videoFirstFramefolder);

            assetsInDirectory = Directory.GetFiles(firstFrameVideosDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);
            Assert.IsTrue(File.Exists(firstFramePath1));

            _asset3Temp!.Folder = folder!;
            _asset3Temp!.FolderId = folder!.FolderId;
            _asset2Temp!.Folder = folder;
            _asset2Temp!.FolderId = folder.FolderId;
            _asset4Temp!.Folder = videoFirstFramefolder!;
            _asset4Temp!.FolderId = videoFirstFramefolder!.FolderId;

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(2, assetsFromRepositoryByPath.Count);

            videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.AreEqual(1, videoFirstFramesFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(3, assetsFromRepository.Count);

            List<Folder> expectedFolders = [folder, folder, videoFirstFramefolder];
            List<string> expectedDirectories = [assetsDirectory, assetsDirectory, firstFrameVideosDirectory];

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssets[i], assetPathsAfterSync[i], expectedDirectories[i], expectedFolders[i]);
            }

            List<Folder> folders = [folder, videoFirstFramefolder];
            Dictionary<Folder, List<Asset>> folderToAssetsMapping = new()
            {
                { folder, [_asset3Temp!, _asset2Temp!]},
                { videoFirstFramefolder, [_asset4Temp!]}
            };
            Dictionary<string, int> assetNameToByteSizeMapping = new()
            {
                { _asset3Temp!.FileName, ASSET3_TEMP_IMAGE_BYTE_SIZE },
                { _asset2Temp!.FileName, ASSET2_TEMP_IMAGE_BYTE_SIZE },
                { _asset4Temp!.FileName, ASSET4_TEMP_IMAGE_BYTE_SIZE }
            };

            AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMapping, folders, thumbnails, assetsImageByteSize);
            CheckBlobsAndTablesAfterSaveCatalog(
                blobsPath,
                tablesPath,
                folders,
                folders,
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            CheckBackupAfter(
                backupFilePath,
                blobsPath,
                tablesPath,
                folders,
                folders,
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.AreEqual(10, catalogChanges.Count);

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);

            foreach (Asset expectedAsset in folderToAssetsMapping[folder])
            {
                CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder],
                    expectedAsset,
                    folder,
                    ref increment);
            }

            CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            CheckCatalogChangesAssetAdded(
                catalogChanges,
                firstFrameVideosDirectory,
                folderToAssetsMapping[videoFirstFramefolder],
                _asset4Temp!,
                videoFirstFramefolder,
                ref increment);
            CheckCatalogChangesBackup(catalogChanges, CREATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(assetsDirectory, true);
        }
    }

    [Test]
    public async Task CatalogAssetsAsync_AssetsImageAndVideosAndAnalyseVideosIsFalseAndRootCatalogFolderExists_SyncTheAssetsButNotTheVideo()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "TempAssetsDirectory");

        ConfigureCatalogAssetService(100, assetsDirectory, 200, 150, false, false, false, false);

        try
        {
            Directory.CreateDirectory(assetsDirectory);

            string imagePath1 = Path.Combine(_dataDirectory!, "Image 1.jpg");
            string imagePath2 = Path.Combine(_dataDirectory!, "Homer.gif");
            string videoPath1 = Path.Combine(_dataDirectory!, "Homer.mp4");
            string videoPath2 = Path.Combine(_dataDirectory!, "Homer1s.mp4");

            string imagePath1ToCopy = Path.Combine(assetsDirectory, "Image 1.jpg");
            string imagePath2ToCopy = Path.Combine(assetsDirectory, "Homer.gif");
            string videoPath1ToCopy = Path.Combine(assetsDirectory, "Homer.mp4");
            string videoPath2ToCopy = Path.Combine(assetsDirectory, "Homer1s.mp4");

            string firstFrameVideosDirectory = _userConfigurationService!.PathSettings.FirstFrameVideosPath;
            string firstFramePath1 = Path.Combine(firstFrameVideosDirectory, "Homer.jpg");

            File.Copy(imagePath1, imagePath1ToCopy);
            File.Copy(imagePath2, imagePath2ToCopy);
            File.Copy(videoPath1, videoPath1ToCopy);
            File.Copy(videoPath2, videoPath2ToCopy);

            List<string> assetPaths = [imagePath1ToCopy, imagePath2ToCopy, videoPath1ToCopy, videoPath2ToCopy];
            List<string> assetPathsAfterSync = [imagePath2ToCopy, imagePath1ToCopy];
            List<Asset> expectedAssets = [_asset3Temp!, _asset2Temp!];
            List<int> assetsImageByteSize = [ASSET3_TEMP_IMAGE_BYTE_SIZE, ASSET2_TEMP_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(4, assetsInDirectory.Length);

            foreach (string assetPath in assetPaths)
            {
                Assert.IsTrue(File.Exists(assetPath));
            }

            Assert.IsFalse(File.Exists(firstFramePath1));

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNull(folder);

            Folder? videoFirstFramefolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.IsNull(videoFirstFramefolder);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CheckBackupBefore(backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            List<Asset> videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.IsEmpty(videoFirstFramesFromRepositoryByPath);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _catalogAssetsService!.CatalogAssetsAsync(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            videoFirstFramefolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.IsNull(videoFirstFramefolder);

            Assert.IsFalse(File.Exists(firstFramePath1));

            _asset3Temp!.Folder = folder!;
            _asset3Temp!.FolderId = folder!.FolderId;
            _asset2Temp!.Folder = folder;
            _asset2Temp!.FolderId = folder.FolderId;

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(2, assetsFromRepositoryByPath.Count);

            videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.IsEmpty(videoFirstFramesFromRepositoryByPath);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(2, assetsFromRepository.Count);

            List<Folder> expectedFolders = [folder, folder];
            List<string> expectedDirectories = [assetsDirectory, assetsDirectory];

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssets[i], assetPathsAfterSync[i], expectedDirectories[i], expectedFolders[i]);
            }

            Dictionary<Folder, List<Asset>> folderToAssetsMapping = new() { { folder, [_asset3Temp!, _asset2Temp!]} };
            Dictionary<string, int> assetNameToByteSizeMapping = new()
            {
                { _asset3Temp!.FileName, ASSET3_TEMP_IMAGE_BYTE_SIZE },
                { _asset2Temp!.FileName, ASSET2_TEMP_IMAGE_BYTE_SIZE }
            };

            AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMapping, [folder], thumbnails, assetsImageByteSize);
            CheckBlobsAndTablesAfterSaveCatalog(
                blobsPath,
                tablesPath,
                [folder],
                [folder],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            CheckBackupAfter(
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder],
                [folder],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.AreEqual(7, catalogChanges.Count);

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            foreach (Asset expectedAsset in folderToAssetsMapping[folder])
            {
                CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder],
                    expectedAsset,
                    folder,
                    ref increment);
            }

            CheckCatalogChangesBackup(catalogChanges, CREATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(assetsDirectory, true);
        }
    }

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public async Task CatalogAssetsAsync_AssetsAndRootCatalogFolderExistsAndCatalogBatchSizeIsSmaller_SyncTheFirstAssets(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder2");

        ConfigureCatalogAssetService(2, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        try
        {
            string imagePath1 = Path.Combine(assetsDirectory, "Image 1_duplicate.jpg");
            string imagePath2 = Path.Combine(assetsDirectory, "Image 9.png");

            List<string> assetPaths = [imagePath1, imagePath2];
            List<Asset> expectedAssets = [_asset1!, _asset2!];
            List<int> assetsImageByteSize = [ASSET1_IMAGE_BYTE_SIZE, ASSET2_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(4, assetsInDirectory.Length);

            foreach (string assetPath in assetPaths)
            {
                Assert.IsTrue(File.Exists(assetPath));
            }

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNull(folder);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CheckBackupBefore(backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _catalogAssetsService!.CatalogAssetsAsync(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            _asset1!.Folder = folder!;
            _asset1!.FolderId = folder!.FolderId;
            _asset2!.Folder = folder;
            _asset2!.FolderId = folder.FolderId;

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(2, assetsFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(2, assetsFromRepository.Count);

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssets[i], assetPaths[i], assetsDirectory, folder);
            }

            Dictionary<Folder, List<Asset>> folderToAssetsMapping = new() { { folder, expectedAssets } };
            Dictionary<string, int> assetNameToByteSizeMapping = new()
            {
                { _asset1!.FileName, ASSET1_IMAGE_BYTE_SIZE },
                { _asset2!.FileName, ASSET2_IMAGE_BYTE_SIZE }
            };

            AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMapping, [folder], thumbnails, assetsImageByteSize);
            CheckBlobsAndTablesAfterSaveCatalog(
                blobsPath,
                tablesPath,
                [folder],
                [folder],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            CheckBackupAfter(
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder],
                [folder],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.AreEqual(7, catalogChanges.Count);

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            foreach (Asset expectedAsset in folderToAssetsMapping[folder])
            {
                CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder],
                    expectedAsset,
                    folder,
                    ref increment);
            }

            CheckCatalogChangesBackup(catalogChanges, CREATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public async Task CatalogAssetsAsync_AssetsAndRootCatalogFolderExistsAndOneCorruptedImage_SyncTheAssetsButNotTheCorruptedImage(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "TempAssetsDirectory");

        ConfigureCatalogAssetService(100, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        try
        {
            Directory.CreateDirectory(assetsDirectory);

            string imagePath1 = Path.Combine(_dataDirectory!, "Image 1.jpg");
            string imagePath2 = Path.Combine(_dataDirectory!, "Homer.gif");

            string imagePath1ToCopy = Path.Combine(assetsDirectory, "Image 1.jpg");
            string imagePath2ToCopy = Path.Combine(assetsDirectory, "Homer.gif");

            File.Copy(imagePath1, imagePath1ToCopy);
            File.Copy(imagePath2, imagePath2ToCopy);

            List<string> assetPaths = [imagePath2ToCopy];
            List<Asset> expectedAssets = [_asset3Temp!];
            List<int> assetsImageByteSize = [ASSET3_TEMP_IMAGE_BYTE_SIZE];

            // Corrupt image
            string imagePath1ToCopyTemp = Path.Combine(assetsDirectory, "Image 1_Temp.jpg");
            File.Copy(imagePath1ToCopy, imagePath1ToCopyTemp);
            ImageHelpers.CreateInvalidImage(imagePath1ToCopyTemp, imagePath1ToCopy);
            File.Delete(imagePath1ToCopyTemp);
            Assert.IsTrue(File.Exists(imagePath1ToCopy));

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(2, assetsInDirectory.Length);

            foreach (string assetPath in assetPaths)
            {
                Assert.IsTrue(File.Exists(assetPath));
            }

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNull(folder);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CheckBackupBefore(backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _catalogAssetsService!.CatalogAssetsAsync(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            _asset3Temp!.Folder = folder!;
            _asset3Temp!.FolderId = folder!.FolderId;

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(1, assetsFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(1, assetsFromRepository.Count);

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssets[i], assetPaths[i], assetsDirectory, folder);
            }

            Dictionary<Folder, List<Asset>> folderToAssetsMapping = new() { { folder, expectedAssets } };
            Dictionary<string, int> assetNameToByteSizeMapping = new() { { _asset3Temp!.FileName, ASSET3_TEMP_IMAGE_BYTE_SIZE } };

            AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMapping, [folder], thumbnails, assetsImageByteSize);
            CheckBlobsAndTablesAfterSaveCatalog(
                blobsPath,
                tablesPath,
                [folder],
                [folder],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            CheckBackupAfter(
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder],
                [folder],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.AreEqual(6, catalogChanges.Count);

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            foreach (Asset expectedAsset in folderToAssetsMapping[folder])
            {
                CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder],
                    expectedAsset,
                    folder,
                    ref increment);
            }

            CheckCatalogChangesBackup(catalogChanges, CREATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(assetsDirectory, true);
        }
    }

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public async Task CatalogAssetsAsync_AssetsAndRootCatalogFolderExistsAndCallbackIsNull_SyncTheAssetsWithoutEvent(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder2");

        ConfigureCatalogAssetService(100, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        try
        {
            string imagePath1 = Path.Combine(assetsDirectory, "Image 1_duplicate.jpg");
            string imagePath2 = Path.Combine(assetsDirectory, "Image 9.png");
            string imagePath3 = Path.Combine(assetsDirectory, "Image 9_duplicate.png");
            string imagePath4 = Path.Combine(assetsDirectory, "Image_11.heic");

            List<string> assetPaths = [imagePath1, imagePath2, imagePath3, imagePath4];
            List<Asset> expectedAssets = [_asset1!, _asset2!, _asset3!, _asset4!];
            List<int> assetsImageByteSize = [ASSET1_IMAGE_BYTE_SIZE, ASSET2_IMAGE_BYTE_SIZE, ASSET3_IMAGE_BYTE_SIZE, ASSET4_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(4, assetsInDirectory.Length);

            foreach (string assetPath in assetPaths)
            {
                Assert.IsTrue(File.Exists(assetPath));
            }

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNull(folder);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CheckBackupBefore(backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            CatalogChangeCallback? catalogChangeCallback = null;

            await _catalogAssetsService!.CatalogAssetsAsync(catalogChangeCallback!);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            _asset1!.Folder = folder!;
            _asset1!.FolderId = folder!.FolderId;
            _asset2!.Folder = folder;
            _asset2!.FolderId = folder.FolderId;
            _asset3!.Folder = folder;
            _asset3!.FolderId = folder.FolderId;
            _asset4!.Folder = folder;
            _asset4!.FolderId = folder.FolderId;

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(4, assetsFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(4, assetsFromRepository.Count);

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssets[i], assetPaths[i], assetsDirectory, folder);
            }

            Dictionary<Folder, List<Asset>> folderToAssetsMapping = new() { { folder, expectedAssets } };
            Dictionary<string, int> assetNameToByteSizeMapping = new()
            {
                { _asset1!.FileName, ASSET1_IMAGE_BYTE_SIZE },
                { _asset2!.FileName, ASSET2_IMAGE_BYTE_SIZE },
                { _asset3!.FileName, ASSET3_IMAGE_BYTE_SIZE },
                { _asset4!.FileName, ASSET4_IMAGE_BYTE_SIZE }
            };

            AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMapping, [folder], thumbnails, assetsImageByteSize);
            CheckBlobsAndTablesAfterSaveCatalog(
                blobsPath,
                tablesPath,
                [folder],
                [folder],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            CheckBackupAfter(
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder],
                [folder],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.IsNull(catalogChangeCallback);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase(true, 0)]
    [TestCase(true, 2)]
    [TestCase(true, 100)]
    [TestCase(false, 0)]
    public async Task CatalogAssetsAsync_AssetsAndRootCatalogFolderExistsAndIsCancellationRequestedOrCatalogBatchSizeIsEqualTo0_StopsTheSync(bool canceled, int catalogBatchSize)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder2");

        ConfigureCatalogAssetService(catalogBatchSize, assetsDirectory, 200, 150, false, false, false, false);

        try
        {
            string imagePath1 = Path.Combine(assetsDirectory, "Image 1_duplicate.jpg");
            string imagePath2 = Path.Combine(assetsDirectory, "Image 9.png");
            string imagePath3 = Path.Combine(assetsDirectory, "Image 9_duplicate.png");
            string imagePath4 = Path.Combine(assetsDirectory, "Image_11.heic");

            List<string> assetPaths = [imagePath1, imagePath2, imagePath3, imagePath4];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(4, assetsInDirectory.Length);

            foreach (string assetPath in assetPaths)
            {
                Assert.IsTrue(File.Exists(assetPath));
            }

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNull(folder);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CheckBackupBefore(backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];
            CancellationToken cancellationToken = new (canceled);

            await _catalogAssetsService!.CatalogAssetsAsync(catalogChanges.Add, cancellationToken);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            CheckBlobsAndTablesAfterSaveCatalogEmpty(blobsPath, tablesPath, false, false, folder!);

            Assert.IsTrue(_testableAssetRepository.HasChanges()); // SaveCatalog has not been done due to the Cancellation

            CheckDefaultEmptyBackup(backupFilePath, blobsPath, tablesPath, false, false, folder!);

            Assert.AreEqual(4, catalogChanges.Count);

            int increment = 0;

            CheckCatalogChangesBackup(catalogChanges, CREATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    // --------------------------------------------------------------------------------------------------------------------------------------------
    // UpdatedAssets
    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public async Task CatalogAssetsAsync_AssetsAndRootCatalogFolderExistsAndOneImageIsUpdated_SyncTheAssets(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder2");
        string destinationFilePathToCopy = Path.Combine(assetsDirectory, _asset1Temp!.FileName);

        ConfigureCatalogAssetService(100, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        try
        {
            string imagePath1 = Path.Combine(assetsDirectory, "Image 1_duplicate.jpg");
            string imagePath2 = Path.Combine(assetsDirectory, "Image 9.png");
            string imagePath3 = Path.Combine(assetsDirectory, "Image 9_duplicate.png");
            string imagePath4 = Path.Combine(assetsDirectory, "Image_11.heic");
            File.Copy(imagePath1, destinationFilePathToCopy);

            List<string> assetPaths = [imagePath1, destinationFilePathToCopy, imagePath2, imagePath3, imagePath4];
            List<Asset> expectedAssets = [_asset1!, _asset1Temp!, _asset2!, _asset3!, _asset4!];
            List<int> assetsImageByteSize = [ASSET1_IMAGE_BYTE_SIZE, ASSET1_TEMP_IMAGE_BYTE_SIZE, ASSET2_IMAGE_BYTE_SIZE, ASSET3_IMAGE_BYTE_SIZE, ASSET4_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(5, assetsInDirectory.Length);

            foreach (string assetPath in assetPaths)
            {
                Assert.IsTrue(File.Exists(assetPath));
            }

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNull(folder);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CheckBackupBefore(backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _catalogAssetsService!.CatalogAssetsAsync(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            _asset1!.Folder = folder!;
            _asset1!.FolderId = folder!.FolderId;
            _asset1Temp!.Folder = folder;
            _asset1Temp!.FolderId = folder.FolderId;
            _asset2!.Folder = folder;
            _asset2!.FolderId = folder.FolderId;
            _asset3!.Folder = folder;
            _asset3!.FolderId = folder.FolderId;
            _asset4!.Folder = folder;
            _asset4!.FolderId = folder.FolderId;

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(5, assetsFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(5, assetsFromRepository.Count);

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssets[i], assetPaths[i], assetsDirectory, folder);
            }

            Dictionary<Folder, List<Asset>> folderToAssetsMapping = new() { { folder, expectedAssets } };
            Dictionary<string, int> assetNameToByteSizeMapping = new()
            {
                { _asset1!.FileName, ASSET1_IMAGE_BYTE_SIZE },
                { _asset1Temp!.FileName, ASSET1_TEMP_IMAGE_BYTE_SIZE },
                { _asset2!.FileName, ASSET2_IMAGE_BYTE_SIZE },
                { _asset3!.FileName, ASSET3_IMAGE_BYTE_SIZE },
                { _asset4!.FileName, ASSET4_IMAGE_BYTE_SIZE }
            };

            AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMapping, [folder], thumbnails, assetsImageByteSize);
            CheckBlobsAndTablesAfterSaveCatalog(
                blobsPath,
                tablesPath,
                [folder],
                [folder],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            CheckBackupAfter(
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder],
                [folder],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.AreEqual(10, catalogChanges.Count);

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            foreach (Asset expectedAsset in folderToAssetsMapping[folder])
            {
                CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder],
                    expectedAsset,
                    folder,
                    ref increment);
            }

            CheckCatalogChangesBackup(catalogChanges, CREATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);

            // Second sync

            _asset1Temp.ThumbnailCreationDateTime = DateTime.Now.AddDays(10);
            File.SetLastWriteTime(destinationFilePathToCopy, _asset1Temp.ThumbnailCreationDateTime);

            List<string> assetPathsUpdated = [];
            assetPaths.ForEach(assetPathsUpdated.Add);
            assetPathsUpdated.Remove(destinationFilePathToCopy);
            assetPathsUpdated.Add(destinationFilePathToCopy);

            List<Asset> expectedAssetsUpdated = [];
            expectedAssets.ForEach(expectedAssetsUpdated.Add);
            expectedAssetsUpdated.Remove(_asset1Temp);
            expectedAssetsUpdated.Add(_asset1Temp);

            List<int> assetsImageByteSizeUpdated = [];
            assetsImageByteSize.ForEach(assetsImageByteSizeUpdated.Add);
            assetsImageByteSizeUpdated.Remove(ASSET1_TEMP_IMAGE_BYTE_SIZE);
            assetsImageByteSizeUpdated.Add(ASSET1_TEMP_IMAGE_BYTE_SIZE);

            assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(5, assetsInDirectory.Length);

            await _catalogAssetsService!.CatalogAssetsAsync(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(5, assetsFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(5, assetsFromRepository.Count);

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssetsUpdated[i], assetPathsUpdated[i], assetsDirectory, folder!);
            }

            Dictionary<Folder, List<Asset>> folderToAssetsMappingUpdated = new() { { folder!, expectedAssetsUpdated} };
            Dictionary<string, int> assetNameToByteSizeMappingUpdated = new()
            {
                { _asset1!.FileName, ASSET1_IMAGE_BYTE_SIZE },
                { _asset2!.FileName, ASSET2_IMAGE_BYTE_SIZE },
                { _asset3!.FileName, ASSET3_IMAGE_BYTE_SIZE },
                { _asset4!.FileName, ASSET4_IMAGE_BYTE_SIZE },
                { _asset1Temp!.FileName, ASSET1_TEMP_IMAGE_BYTE_SIZE }
            };

            AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMappingUpdated, [folder!], thumbnails, assetsImageByteSizeUpdated);
            CheckBlobsAndTablesAfterSaveCatalog(
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMappingUpdated,
                assetNameToByteSizeMappingUpdated);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            CheckBackupAfter(
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMappingUpdated,
                assetNameToByteSizeMappingUpdated);

            Assert.AreEqual(16, catalogChanges.Count);

            increment = 0;

            foldersInRepository = _testableAssetRepository!.GetFolders();

            CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            foreach (Asset expectedAsset in folderToAssetsMapping[folder!])
            {
                CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder!],
                    expectedAsset,
                    folder!,
                    ref increment);
            }

            CheckCatalogChangesBackup(catalogChanges, CREATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);

            CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            CheckCatalogChangesAssetUpdated(catalogChanges, assetsDirectory, expectedAssetsUpdated, _asset1Temp, folder!, ref increment);
            CheckCatalogChangesBackup(catalogChanges, UPDATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            File.Delete(destinationFilePathToCopy);
        }
    }

    // TODO: It is not able to detect if a video has been updated
    [Test]
    public async Task CatalogAssetsAsync_AssetsAndRootCatalogFolderExistsAndOneVideoIsUpdated_SyncTheAssets()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "TempAssetsDirectory");

        ConfigureCatalogAssetService(100, assetsDirectory, 200, 150, false, false, false, true);

        try
        {
            Directory.CreateDirectory(assetsDirectory);

            string imagePath1 = Path.Combine(_dataDirectory!, "Image 1.jpg");
            string imagePath2 = Path.Combine(_dataDirectory!, "Homer.gif");
            string videoPath1 = Path.Combine(_dataDirectory!, "Homer.mp4");

            string imagePath1ToCopy = Path.Combine(assetsDirectory, "Image 1.jpg");
            string imagePath2ToCopy = Path.Combine(assetsDirectory, "Homer.gif");
            string videoPath1ToCopy = Path.Combine(assetsDirectory, "Homer.mp4");

            string firstFrameVideosDirectory = _userConfigurationService!.PathSettings.FirstFrameVideosPath;
            string firstFramePath1 = Path.Combine(firstFrameVideosDirectory, "Homer.jpg");

            File.Copy(imagePath1, imagePath1ToCopy);
            File.Copy(imagePath2, imagePath2ToCopy);
            File.Copy(videoPath1, videoPath1ToCopy);

            List<string> assetPaths = [imagePath1ToCopy, imagePath2ToCopy, videoPath1ToCopy];
            List<string> assetPathsAfterSync = [imagePath2ToCopy, imagePath1ToCopy, firstFramePath1];
            List<Asset> expectedAssets = [_asset3Temp!, _asset2Temp!, _asset4Temp!];
            List<int> assetsImageByteSize = [ASSET3_TEMP_IMAGE_BYTE_SIZE, ASSET2_TEMP_IMAGE_BYTE_SIZE, ASSET4_TEMP_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(3, assetsInDirectory.Length);

            foreach (string assetPath in assetPaths)
            {
                Assert.IsTrue(File.Exists(assetPath));
            }

            Assert.IsFalse(File.Exists(firstFramePath1));

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNull(folder);

            Folder? videoFirstFramefolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.IsNull(videoFirstFramefolder);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CheckBackupBefore(backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            List<Asset> videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.IsEmpty(videoFirstFramesFromRepositoryByPath);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _catalogAssetsService!.CatalogAssetsAsync(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            videoFirstFramefolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.IsNotNull(videoFirstFramefolder);

            assetsInDirectory = Directory.GetFiles(firstFrameVideosDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);
            Assert.IsTrue(File.Exists(firstFramePath1));

            _asset3Temp!.Folder = folder!;
            _asset3Temp!.FolderId = folder!.FolderId;
            _asset2Temp!.Folder = folder;
            _asset2Temp!.FolderId = folder.FolderId;
            _asset4Temp!.Folder = videoFirstFramefolder!;
            _asset4Temp!.FolderId = videoFirstFramefolder!.FolderId;

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(2, assetsFromRepositoryByPath.Count);

            videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.AreEqual(1, videoFirstFramesFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(3, assetsFromRepository.Count);

            List<Folder> expectedFolders = [folder, folder, videoFirstFramefolder];
            List<string> expectedDirectories = [assetsDirectory, assetsDirectory, firstFrameVideosDirectory];

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssets[i], assetPathsAfterSync[i], expectedDirectories[i], expectedFolders[i]);
            }

            List<Folder> folders = [folder, videoFirstFramefolder];
            Dictionary<Folder, List<Asset>> folderToAssetsMapping = new()
            {
                { folder, [_asset3Temp!, _asset2Temp!]},
                { videoFirstFramefolder, [_asset4Temp!]}
            };
            Dictionary<string, int> assetNameToByteSizeMapping = new()
            {
                { _asset3Temp!.FileName, ASSET3_TEMP_IMAGE_BYTE_SIZE },
                { _asset2Temp!.FileName, ASSET2_TEMP_IMAGE_BYTE_SIZE },
                { _asset4Temp!.FileName, ASSET4_TEMP_IMAGE_BYTE_SIZE }
            };

            AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMapping, folders, thumbnails, assetsImageByteSize);
            CheckBlobsAndTablesAfterSaveCatalog(
                blobsPath,
                tablesPath,
                folders,
                folders,
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            CheckBackupAfter(
                backupFilePath,
                blobsPath,
                tablesPath,
                folders,
                folders,
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.AreEqual(10, catalogChanges.Count);

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);

            foreach (Asset expectedAsset in folderToAssetsMapping[folder])
            {
                CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder],
                    expectedAsset,
                    folder,
                    ref increment);
            }

            CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            CheckCatalogChangesAssetAdded(
                catalogChanges,
                firstFrameVideosDirectory,
                folderToAssetsMapping[videoFirstFramefolder],
                _asset4Temp!,
                videoFirstFramefolder,
                ref increment);
            CheckCatalogChangesBackup(catalogChanges, CREATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);

            // Second sync

            File.SetLastWriteTime(videoPath1ToCopy, _asset4Temp.ThumbnailCreationDateTime);

            assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(3, assetsInDirectory.Length);

            assetsInDirectory = Directory.GetFiles(firstFrameVideosDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);

            await _catalogAssetsService!.CatalogAssetsAsync(catalogChanges.Add);

            assetsInDirectory = Directory.GetFiles(firstFrameVideosDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);
            Assert.IsTrue(File.Exists(firstFramePath1));

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            videoFirstFramefolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.IsNotNull(videoFirstFramefolder);

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(2, assetsFromRepositoryByPath.Count);

            videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.AreEqual(1, videoFirstFramesFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(3, assetsFromRepository.Count);

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssets[i], assetPathsAfterSync[i], expectedDirectories[i], expectedFolders[i]);
            }

            AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMapping, folders, thumbnails, assetsImageByteSize);
            CheckBlobsAndTablesAfterSaveCatalog(
                blobsPath,
                tablesPath,
                folders,
                folders,
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            CheckBackupAfter(
                backupFilePath,
                blobsPath,
                tablesPath,
                folders,
                folders,
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.AreEqual(14, catalogChanges.Count);

            increment = 0;

            foldersInRepository = _testableAssetRepository!.GetFolders();

            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);

            foreach (Asset expectedAsset in folderToAssetsMapping[folder!])
            {
                CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder!],
                    expectedAsset,
                    folder!,
                    ref increment);
            }

            CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            CheckCatalogChangesAssetAdded(
                catalogChanges,
                firstFrameVideosDirectory,
                folderToAssetsMapping[videoFirstFramefolder!],
                _asset4Temp!,
                videoFirstFramefolder!,
                ref increment);

            CheckCatalogChangesBackup(catalogChanges, CREATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);

            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(assetsDirectory, true);
        }
    }

    [Test]
    public async Task CatalogAssetsAsync_AssetsAndRootCatalogFolderExistsAndOneVideoIsUpdatedAndAnalyseVideosIsFalse_SyncTheAssets()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "TempAssetsDirectory");

        ConfigureCatalogAssetService(100, assetsDirectory, 200, 150, false, false, false, false);

        try
        {
            Directory.CreateDirectory(assetsDirectory);

            string imagePath1 = Path.Combine(_dataDirectory!, "Image 1.jpg");
            string imagePath2 = Path.Combine(_dataDirectory!, "Homer.gif");
            string videoPath1 = Path.Combine(_dataDirectory!, "Homer.mp4");

            string imagePath1ToCopy = Path.Combine(assetsDirectory, "Image 1.jpg");
            string imagePath2ToCopy = Path.Combine(assetsDirectory, "Homer.gif");
            string videoPath1ToCopy = Path.Combine(assetsDirectory, "Homer.mp4");

            string firstFrameVideosDirectory = _userConfigurationService!.PathSettings.FirstFrameVideosPath;
            string firstFramePath1 = Path.Combine(firstFrameVideosDirectory, "Homer.jpg");

            File.Copy(imagePath1, imagePath1ToCopy);
            File.Copy(imagePath2, imagePath2ToCopy);
            File.Copy(videoPath1, videoPath1ToCopy);

            List<string> assetPaths = [imagePath1ToCopy, imagePath2ToCopy, videoPath1ToCopy];
            List<string> assetPathsAfterSync = [imagePath2ToCopy, imagePath1ToCopy];
            List<Asset> expectedAssets = [_asset3Temp!, _asset2Temp!];
            List<int> assetsImageByteSize = [ASSET3_TEMP_IMAGE_BYTE_SIZE, ASSET2_TEMP_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(3, assetsInDirectory.Length);

            foreach (string assetPath in assetPaths)
            {
                Assert.IsTrue(File.Exists(assetPath));
            }

            Assert.IsFalse(File.Exists(firstFramePath1));

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNull(folder);

            Folder? videoFirstFramefolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.IsNull(videoFirstFramefolder);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CheckBackupBefore(backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            List<Asset> videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.IsEmpty(videoFirstFramesFromRepositoryByPath);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _catalogAssetsService!.CatalogAssetsAsync(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            videoFirstFramefolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.IsNull(videoFirstFramefolder);

            Assert.IsFalse(File.Exists(firstFramePath1));

            _asset3Temp!.Folder = folder!;
            _asset3Temp!.FolderId = folder!.FolderId;
            _asset2Temp!.Folder = folder;
            _asset2Temp!.FolderId = folder.FolderId;

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(2, assetsFromRepositoryByPath.Count);

            videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.IsEmpty(videoFirstFramesFromRepositoryByPath);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(2, assetsFromRepository.Count);

            List<Folder> expectedFolders = [folder, folder];
            List<string> expectedDirectories = [assetsDirectory, assetsDirectory];

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssets[i], assetPathsAfterSync[i], expectedDirectories[i], expectedFolders[i]);
            }

            Dictionary<Folder, List<Asset>> folderToAssetsMapping = new() { { folder, [_asset3Temp!, _asset2Temp!]} };
            Dictionary<string, int> assetNameToByteSizeMapping = new()
            {
                { _asset3Temp!.FileName, ASSET3_TEMP_IMAGE_BYTE_SIZE },
                { _asset2Temp!.FileName, ASSET2_TEMP_IMAGE_BYTE_SIZE }
            };

            AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMapping, [folder], thumbnails, assetsImageByteSize);
            CheckBlobsAndTablesAfterSaveCatalog(
                blobsPath,
                tablesPath,
                [folder],
                [folder],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            CheckBackupAfter(
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder],
                [folder],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.AreEqual(7, catalogChanges.Count);

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            foreach (Asset expectedAsset in folderToAssetsMapping[folder])
            {
                CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder],
                    expectedAsset,
                    folder,
                    ref increment);
            }

            CheckCatalogChangesBackup(catalogChanges, CREATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);

            // Second sync

            _asset4Temp!.ThumbnailCreationDateTime = DateTime.Now.AddDays(10);
            File.SetLastWriteTime(videoPath1ToCopy, _asset4Temp.ThumbnailCreationDateTime);

            assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(3, assetsInDirectory.Length);

            await _catalogAssetsService!.CatalogAssetsAsync(catalogChanges.Add);

            Assert.IsFalse(File.Exists(firstFramePath1));

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            videoFirstFramefolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.IsNull(videoFirstFramefolder);

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(2, assetsFromRepositoryByPath.Count);

            videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.IsEmpty(videoFirstFramesFromRepositoryByPath);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(2, assetsFromRepository.Count);

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssets[i], assetPathsAfterSync[i], expectedDirectories[i], expectedFolders[i]);
            }

            AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMapping, [folder!], thumbnails, assetsImageByteSize);
            CheckBlobsAndTablesAfterSaveCatalog(
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            CheckBackupAfter(
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.AreEqual(10, catalogChanges.Count);

            increment = 0;

            foldersInRepository = _testableAssetRepository!.GetFolders();

            CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            foreach (Asset expectedAsset in folderToAssetsMapping[folder!])
            {
                CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder!],
                    expectedAsset,
                    folder!,
                    ref increment);
            }

            CheckCatalogChangesBackup(catalogChanges, CREATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);

            CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(assetsDirectory, true);
        }
    }

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public async Task CatalogAssetsAsync_AssetsAndRootCatalogFolderExistsAndOneImageIsUpdatedAndCatalogBatchSizeIsSmaller_SyncTheAssets(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "TempAssetsDirectory");

        ConfigureCatalogAssetService(1, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        try
        {
            Directory.CreateDirectory(assetsDirectory);

            string imagePath1 = Path.Combine(_dataDirectory!, "Image 1.jpg");
            string imagePath2 = Path.Combine(_dataDirectory!, "Homer.gif");

            string imagePath1ToCopy = Path.Combine(assetsDirectory, "Image 1.jpg");
            string imagePath2ToCopy = Path.Combine(assetsDirectory, "Homer.gif");

            File.Copy(imagePath1, imagePath1ToCopy);

            List<string> assetPaths = [imagePath1ToCopy];
            List<Asset> expectedAssets = [_asset2Temp!];
            List<int> assetsImageByteSize = [ASSET2_TEMP_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);

            foreach (string assetPath in assetPaths)
            {
                Assert.IsTrue(File.Exists(assetPath));
            }

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNull(folder);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CheckBackupBefore(backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _catalogAssetsService!.CatalogAssetsAsync(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            _asset2Temp!.Folder = folder!;
            _asset2Temp!.FolderId = folder!.FolderId;

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(1, assetsFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(1, assetsFromRepository.Count);

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssets[i], assetPaths[i], assetsDirectory, folder);
            }

            Dictionary<Folder, List<Asset>> folderToAssetsMapping = new() { { folder, expectedAssets } };
            Dictionary<string, int> assetNameToByteSizeMapping = new() { { _asset2Temp!.FileName, ASSET2_TEMP_IMAGE_BYTE_SIZE } };

            AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMapping, [folder], thumbnails, assetsImageByteSize);
            CheckBlobsAndTablesAfterSaveCatalog(
                blobsPath,
                tablesPath,
                [folder],
                [folder],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            CheckBackupAfter(
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder],
                [folder],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.AreEqual(6, catalogChanges.Count);

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            foreach (Asset expectedAsset in folderToAssetsMapping[folder])
            {
                CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder],
                    expectedAsset,
                    folder,
                    ref increment);
            }

            CheckCatalogChangesBackup(catalogChanges, CREATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);

            // Second sync

            File.Copy(imagePath2, imagePath2ToCopy);

            List<string> assetPathsUpdated = [imagePath1ToCopy, imagePath2ToCopy];
            List<Asset> expectedAssetsUpdated = [_asset2Temp!, _asset3Temp!];
            List<int> assetsImageByteSizeUpdated = [ASSET2_TEMP_IMAGE_BYTE_SIZE, ASSET3_TEMP_IMAGE_BYTE_SIZE];

            Dictionary<Folder, List<Asset>> folderToAssetsMappingUpdated = new()
            {
                { folder, [_asset3Temp!, _asset2Temp!]},
            };
            Dictionary<string, int> assetNameToByteSizeMappingUpdated = new()
            {
                { _asset2Temp!.FileName, ASSET2_TEMP_IMAGE_BYTE_SIZE },
                { _asset3Temp!.FileName, ASSET3_TEMP_IMAGE_BYTE_SIZE }
            };

            _asset2Temp.ThumbnailCreationDateTime = DateTime.Now.AddDays(10);
            File.SetLastWriteTime(imagePath1ToCopy, _asset2Temp.ThumbnailCreationDateTime);

            assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(2, assetsInDirectory.Length);

            await _catalogAssetsService!.CatalogAssetsAsync(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            _asset3Temp!.Folder = folder!;
            _asset3Temp!.FolderId = folder!.FolderId;

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(2, assetsFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(2, assetsFromRepository.Count);

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssetsUpdated[i], assetPathsUpdated[i], assetsDirectory, folder);
            }

            AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMappingUpdated, [folder], thumbnails, assetsImageByteSizeUpdated);
            CheckBlobsAndTablesAfterSaveCatalog(
                blobsPath,
                tablesPath,
                [folder],
                [folder],
                assetsFromRepository,
                folderToAssetsMappingUpdated,
                assetNameToByteSizeMappingUpdated);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            CheckBackupAfter(
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder],
                [folder],
                assetsFromRepository,
                folderToAssetsMappingUpdated,
                assetNameToByteSizeMappingUpdated);

            Assert.AreEqual(12, catalogChanges.Count);

            increment = 0;

            foldersInRepository = _testableAssetRepository!.GetFolders();

            CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            foreach (Asset expectedAsset in folderToAssetsMapping[folder])
            {
                CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder],
                    expectedAsset,
                    folder,
                    ref increment);
            }

            CheckCatalogChangesBackup(catalogChanges, CREATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);

            CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);
            CheckCatalogChangesAssetAdded(
                catalogChanges,
                assetsDirectory,
                folderToAssetsMappingUpdated[folder],
                _asset3Temp,
                folder,
                ref increment);
            CheckCatalogChangesBackup(catalogChanges, UPDATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(assetsDirectory, true);
        }
    }

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public async Task CatalogAssetsAsync_AssetsAndRootCatalogFolderExistsAndOneCorruptedImageIsUpdated_SyncTheAssetsAndRemovesTheCorruptedImage(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "TempAssetsDirectory");

        ConfigureCatalogAssetService(100, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        try
        {
            Directory.CreateDirectory(assetsDirectory);

            string imagePath1 = Path.Combine(_dataDirectory!, "Image 1.jpg");
            string imagePath2 = Path.Combine(_dataDirectory!, "Homer.gif");

            string imagePath1ToCopy = Path.Combine(assetsDirectory, "Image 1.jpg");
            string imagePath2ToCopy = Path.Combine(assetsDirectory, "Homer.gif");

            File.Copy(imagePath1, imagePath1ToCopy);
            File.Copy(imagePath2, imagePath2ToCopy);

            List<string> assetPaths = [imagePath2ToCopy, imagePath1ToCopy];
            List<Asset> expectedAssets = [_asset3Temp!, _asset2Temp!];
            List<int> assetsImageByteSize = [ASSET3_TEMP_IMAGE_BYTE_SIZE, ASSET2_TEMP_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(2, assetsInDirectory.Length);

            foreach (string assetPath in assetPaths)
            {
                Assert.IsTrue(File.Exists(assetPath));
            }

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNull(folder);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CheckBackupBefore(backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _catalogAssetsService!.CatalogAssetsAsync(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            _asset2Temp!.Folder = folder!;
            _asset2Temp!.FolderId = folder!.FolderId;
            _asset3Temp!.Folder = folder;
            _asset3Temp!.FolderId = folder.FolderId;

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(2, assetsFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(2, assetsFromRepository.Count);

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssets[i], assetPaths[i], assetsDirectory, folder);
            }

            Dictionary<Folder, List<Asset>> folderToAssetsMapping = new() { { folder, expectedAssets } };
            Dictionary<string, int> assetNameToByteSizeMapping = new()
            {
                { _asset3Temp!.FileName, ASSET3_TEMP_IMAGE_BYTE_SIZE },
                { _asset2Temp!.FileName, ASSET2_TEMP_IMAGE_BYTE_SIZE }
            };

            AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMapping, [folder], thumbnails, assetsImageByteSize);
            CheckBlobsAndTablesAfterSaveCatalog(
                blobsPath,
                tablesPath,
                [folder],
                [folder],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            CheckBackupAfter(
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder],
                [folder],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.AreEqual(7, catalogChanges.Count);

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            foreach (Asset expectedAsset in folderToAssetsMapping[folder])
            {
                CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder],
                    expectedAsset,
                    folder,
                    ref increment);
            }

            CheckCatalogChangesBackup(catalogChanges, CREATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);

            // Second sync

            _asset2Temp.ThumbnailCreationDateTime = DateTime.Now; // Because recreated with CreateInvalidImage()
            File.SetLastWriteTime(imagePath1ToCopy, DateTime.Now.AddDays(10));

            // Corrupt image
            string imagePath1ToCopyTemp = Path.Combine(assetsDirectory, "Image 1_Temp.jpg");
            File.Copy(imagePath1ToCopy, imagePath1ToCopyTemp);
            ImageHelpers.CreateInvalidImage(imagePath1ToCopyTemp, imagePath1ToCopy);
            File.Delete(imagePath1ToCopyTemp);
            Assert.IsTrue(File.Exists(imagePath1ToCopy));

            List<string> assetPathsUpdated = [];
            assetPaths.ForEach(assetPathsUpdated.Add);
            assetPathsUpdated.Remove(imagePath1ToCopy);

            List<Asset> expectedAssetsUpdated = [];
            expectedAssets.ForEach(expectedAssetsUpdated.Add);
            expectedAssetsUpdated.Remove(_asset2Temp);

            List<int> assetsImageByteSizeUpdated = [];
            assetsImageByteSize.ForEach(assetsImageByteSizeUpdated.Add);
            assetsImageByteSizeUpdated.Remove(ASSET2_TEMP_IMAGE_BYTE_SIZE);

            assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(2, assetsInDirectory.Length);

            await _catalogAssetsService!.CatalogAssetsAsync(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(1, assetsFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(1, assetsFromRepository.Count);

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssetsUpdated[i], assetPathsUpdated[i], assetsDirectory, folder!);
            }

            Dictionary<Folder, List<Asset>> folderToAssetsMappingUpdated = new() { { folder!, expectedAssetsUpdated} };
            Dictionary<string, int> assetNameToByteSizeMappingUpdated = new() { { _asset3Temp!.FileName, ASSET3_TEMP_IMAGE_BYTE_SIZE } };

            AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMappingUpdated, [folder!], thumbnails, assetsImageByteSize);
            CheckBlobsAndTablesAfterSaveCatalog(
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMappingUpdated,
                assetNameToByteSizeMappingUpdated);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            CheckBackupAfter(
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMappingUpdated,
                assetNameToByteSizeMappingUpdated);

            Assert.AreEqual(12, catalogChanges.Count);

            increment = 0;

            foldersInRepository = _testableAssetRepository!.GetFolders();

            CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            foreach (Asset expectedAsset in folderToAssetsMapping[folder!])
            {
                CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder!],
                    expectedAsset,
                    folder!,
                    ref increment);
            }

            CheckCatalogChangesBackup(catalogChanges, CREATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);

            CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);
            CheckCatalogChangesBackup(catalogChanges, UPDATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(assetsDirectory, true);
        }
    }

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public async Task CatalogAssetsAsync_AssetsAndRootCatalogFolderExistsAndOneImageIsUpdatedAndBackupIsDeleted_SyncTheAssets(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder2");
        string destinationFilePathToCopy = Path.Combine(assetsDirectory, _asset1Temp!.FileName);

        ConfigureCatalogAssetService(100, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        try
        {
            string imagePath1 = Path.Combine(assetsDirectory, "Image 1_duplicate.jpg");
            string imagePath2 = Path.Combine(assetsDirectory, "Image 9.png");
            string imagePath3 = Path.Combine(assetsDirectory, "Image 9_duplicate.png");
            string imagePath4 = Path.Combine(assetsDirectory, "Image_11.heic");
            File.Copy(imagePath1, destinationFilePathToCopy);

            List<string> assetPaths = [imagePath1, destinationFilePathToCopy, imagePath2, imagePath3, imagePath4];
            List<Asset> expectedAssets = [_asset1!, _asset1Temp!, _asset2!, _asset3!, _asset4!];
            List<int> assetsImageByteSize = [ASSET1_IMAGE_BYTE_SIZE, ASSET1_TEMP_IMAGE_BYTE_SIZE, ASSET2_IMAGE_BYTE_SIZE, ASSET3_IMAGE_BYTE_SIZE, ASSET4_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(5, assetsInDirectory.Length);

            foreach (string assetPath in assetPaths)
            {
                Assert.IsTrue(File.Exists(assetPath));
            }

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNull(folder);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CheckBackupBefore(backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _catalogAssetsService!.CatalogAssetsAsync(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            _asset1!.Folder = folder!;
            _asset1!.FolderId = folder!.FolderId;
            _asset1Temp!.Folder = folder;
            _asset1Temp!.FolderId = folder.FolderId;
            _asset2!.Folder = folder;
            _asset2!.FolderId = folder.FolderId;
            _asset3!.Folder = folder;
            _asset3!.FolderId = folder.FolderId;
            _asset4!.Folder = folder;
            _asset4!.FolderId = folder.FolderId;

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(5, assetsFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(5, assetsFromRepository.Count);

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssets[i], assetPaths[i], assetsDirectory, folder);
            }

            Dictionary<Folder, List<Asset>> folderToAssetsMapping = new() { { folder, expectedAssets } };
            Dictionary<string, int> assetNameToByteSizeMapping = new()
            {
                { _asset1!.FileName, ASSET1_IMAGE_BYTE_SIZE },
                { _asset1Temp!.FileName, ASSET1_TEMP_IMAGE_BYTE_SIZE },
                { _asset2!.FileName, ASSET2_IMAGE_BYTE_SIZE },
                { _asset3!.FileName, ASSET3_IMAGE_BYTE_SIZE },
                { _asset4!.FileName, ASSET4_IMAGE_BYTE_SIZE }
            };

            AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMapping, [folder], thumbnails, assetsImageByteSize);
            CheckBlobsAndTablesAfterSaveCatalog(
                blobsPath,
                tablesPath,
                [folder],
                [folder],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            CheckBackupAfter(
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder],
                [folder],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.AreEqual(10, catalogChanges.Count);

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            foreach (Asset expectedAsset in folderToAssetsMapping[folder])
            {
                CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder],
                    expectedAsset,
                    folder,
                    ref increment);
            }

            CheckCatalogChangesBackup(catalogChanges, CREATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);

            // Second sync

            RemoveDatabaseBackup([folder], blobsPath, tablesPath, backupFilePath);

            _asset1Temp.ThumbnailCreationDateTime = DateTime.Now.AddDays(10);
            File.SetLastWriteTime(destinationFilePathToCopy, _asset1Temp.ThumbnailCreationDateTime);

            List<string> assetPathsUpdated = [];
            assetPaths.ForEach(assetPathsUpdated.Add);
            assetPathsUpdated.Remove(destinationFilePathToCopy);
            assetPathsUpdated.Add(destinationFilePathToCopy);

            List<Asset> expectedAssetsUpdated = [];
            expectedAssets.ForEach(expectedAssetsUpdated.Add);
            expectedAssetsUpdated.Remove(_asset1Temp);
            expectedAssetsUpdated.Add(_asset1Temp);

            List<int> assetsImageByteSizeUpdated = [];
            assetsImageByteSize.ForEach(assetsImageByteSizeUpdated.Add);
            assetsImageByteSizeUpdated.Remove(ASSET1_TEMP_IMAGE_BYTE_SIZE);
            assetsImageByteSizeUpdated.Add(ASSET1_TEMP_IMAGE_BYTE_SIZE);

            assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(5, assetsInDirectory.Length);

            await _catalogAssetsService!.CatalogAssetsAsync(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(5, assetsFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(5, assetsFromRepository.Count);

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssetsUpdated[i], assetPathsUpdated[i], assetsDirectory, folder!);
            }

            Dictionary<Folder, List<Asset>> folderToAssetsMappingUpdated = new() { { folder!, expectedAssetsUpdated} };
            Dictionary<string, int> assetNameToByteSizeMappingUpdated = new()
            {
                { _asset1!.FileName, ASSET1_IMAGE_BYTE_SIZE },
                { _asset2!.FileName, ASSET2_IMAGE_BYTE_SIZE },
                { _asset3!.FileName, ASSET3_IMAGE_BYTE_SIZE },
                { _asset4!.FileName, ASSET4_IMAGE_BYTE_SIZE },
                { _asset1Temp!.FileName, ASSET1_TEMP_IMAGE_BYTE_SIZE }
            };

            AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMappingUpdated, [folder!], thumbnails, assetsImageByteSizeUpdated);
            CheckBlobsAndTablesAfterSaveCatalog(
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMappingUpdated,
                assetNameToByteSizeMappingUpdated);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            CheckBackupAfter(
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMappingUpdated,
                assetNameToByteSizeMappingUpdated);

            Assert.AreEqual(16, catalogChanges.Count);

            increment = 0;

            foldersInRepository = _testableAssetRepository!.GetFolders();

            CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            foreach (Asset expectedAsset in folderToAssetsMapping[folder!])
            {
                CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder!],
                    expectedAsset,
                    folder!,
                    ref increment);
            }

            CheckCatalogChangesBackup(catalogChanges, CREATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);

            CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            CheckCatalogChangesAssetUpdated(catalogChanges, assetsDirectory, expectedAssetsUpdated, _asset1Temp, folder!, ref increment);
            CheckCatalogChangesBackup(catalogChanges, CREATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            File.Delete(destinationFilePathToCopy);
        }
    }

    // --------------------------------------------------------------------------------------------------------------------------------------------
    // DeletedAssets
    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public async Task CatalogAssetsAsync_AssetsAndRootCatalogFolderExistsAndOneImageIsDeleted_SyncTheAssetsAndRemovesDeletedOne(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder2");
        string destinationFilePathToCopy = Path.Combine(assetsDirectory, _asset1Temp!.FileName);

        ConfigureCatalogAssetService(100, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        try
        {
            string imagePath1 = Path.Combine(assetsDirectory, "Image 1_duplicate.jpg");
            string imagePath2 = Path.Combine(assetsDirectory, "Image 9.png");
            string imagePath3 = Path.Combine(assetsDirectory, "Image 9_duplicate.png");
            string imagePath4 = Path.Combine(assetsDirectory, "Image_11.heic");
            File.Copy(imagePath1, destinationFilePathToCopy);

            List<string> assetPaths = [imagePath1, destinationFilePathToCopy, imagePath2, imagePath3, imagePath4];
            List<Asset> expectedAssets = [_asset1!, _asset1Temp!, _asset2!, _asset3!, _asset4!];
            List<int> assetsImageByteSize = [ASSET1_IMAGE_BYTE_SIZE, ASSET1_TEMP_IMAGE_BYTE_SIZE, ASSET2_IMAGE_BYTE_SIZE, ASSET3_IMAGE_BYTE_SIZE, ASSET4_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(5, assetsInDirectory.Length);

            foreach (string assetPath in assetPaths)
            {
                Assert.IsTrue(File.Exists(assetPath));
            }

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNull(folder);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CheckBackupBefore(backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _catalogAssetsService!.CatalogAssetsAsync(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            _asset1!.Folder = folder!;
            _asset1!.FolderId = folder!.FolderId;
            _asset1Temp!.Folder = folder;
            _asset1Temp!.FolderId = folder.FolderId;
            _asset2!.Folder = folder;
            _asset2!.FolderId = folder.FolderId;
            _asset3!.Folder = folder;
            _asset3!.FolderId = folder.FolderId;
            _asset4!.Folder = folder;
            _asset4!.FolderId = folder.FolderId;

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(5, assetsFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(5, assetsFromRepository.Count);

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssets[i], assetPaths[i], assetsDirectory, folder);
            }

            Dictionary<Folder, List<Asset>> folderToAssetsMapping = new() { { folder, expectedAssets } };
            Dictionary<string, int> assetNameToByteSizeMapping = new()
            {
                { _asset1!.FileName, ASSET1_IMAGE_BYTE_SIZE },
                { _asset1Temp!.FileName, ASSET1_TEMP_IMAGE_BYTE_SIZE },
                { _asset2!.FileName, ASSET2_IMAGE_BYTE_SIZE },
                { _asset3!.FileName, ASSET3_IMAGE_BYTE_SIZE },
                { _asset4!.FileName, ASSET4_IMAGE_BYTE_SIZE }
            };

            AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMapping, [folder], thumbnails, assetsImageByteSize);
            CheckBlobsAndTablesAfterSaveCatalog(
                blobsPath,
                tablesPath,
                [folder],
                [folder],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            CheckBackupAfter(
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder],
                [folder],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.AreEqual(10, catalogChanges.Count);

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            foreach (Asset expectedAsset in folderToAssetsMapping[folder])
            {
                CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder],
                    expectedAsset,
                    folder,
                    ref increment);
            }

            CheckCatalogChangesBackup(catalogChanges, CREATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);

            // Second sync

            File.Delete(destinationFilePathToCopy);

            List<string> assetPathsUpdated = [];
            assetPaths.ForEach(assetPathsUpdated.Add);
            assetPathsUpdated.Remove(destinationFilePathToCopy);

            List<Asset> expectedAssetsUpdated = [];
            expectedAssets.ForEach(expectedAssetsUpdated.Add);
            expectedAssetsUpdated.Remove(_asset1Temp);

            List<int> assetsImageByteSizeUpdated = [];
            assetsImageByteSize.ForEach(assetsImageByteSizeUpdated.Add);
            assetsImageByteSizeUpdated.Remove(ASSET1_TEMP_IMAGE_BYTE_SIZE);

            assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(4, assetsInDirectory.Length);

            Assert.IsFalse(File.Exists(destinationFilePathToCopy));

            await _catalogAssetsService!.CatalogAssetsAsync(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(4, assetsFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(4, assetsFromRepository.Count);

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssetsUpdated[i], assetPathsUpdated[i], assetsDirectory, folder!);
            }

            Dictionary<Folder, List<Asset>> folderToAssetsMappingUpdated = new() { { folder!, expectedAssetsUpdated} };
            Dictionary<string, int> assetNameToByteSizeMappingUpdated = new()
            {
                { _asset1!.FileName, ASSET1_IMAGE_BYTE_SIZE },
                { _asset2!.FileName, ASSET2_IMAGE_BYTE_SIZE },
                { _asset3!.FileName, ASSET3_IMAGE_BYTE_SIZE },
                { _asset4!.FileName, ASSET4_IMAGE_BYTE_SIZE }
            };

            AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMappingUpdated, [folder!], thumbnails, assetsImageByteSizeUpdated);
            CheckBlobsAndTablesAfterSaveCatalog(
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMappingUpdated,
                assetNameToByteSizeMappingUpdated);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            CheckBackupAfter(
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMappingUpdated,
                assetNameToByteSizeMappingUpdated);

            Assert.AreEqual(16, catalogChanges.Count);

            increment = 0;

            foldersInRepository = _testableAssetRepository!.GetFolders();

            CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            foreach (Asset expectedAsset in folderToAssetsMapping[folder!])
            {
                CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder!],
                    expectedAsset,
                    folder!,
                    ref increment);
            }

            CheckCatalogChangesBackup(catalogChanges, CREATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);

            CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            CheckCatalogChangesAssetDeleted(catalogChanges, assetsDirectory, expectedAssetsUpdated, _asset1Temp, folder!, ref increment);
            CheckCatalogChangesBackup(catalogChanges, UPDATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task CatalogAssetsAsync_AssetsAndRootCatalogFolderExistsAndOneVideoIsDeleted_SyncTheAssets()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "TempAssetsDirectory");

        ConfigureCatalogAssetService(100, assetsDirectory, 200, 150, false, false, false, true);

        try
        {
            Directory.CreateDirectory(assetsDirectory);

            string imagePath1 = Path.Combine(_dataDirectory!, "Image 1.jpg");
            string imagePath2 = Path.Combine(_dataDirectory!, "Homer.gif");
            string videoPath1 = Path.Combine(_dataDirectory!, "Homer.mp4");

            string imagePath1ToCopy = Path.Combine(assetsDirectory, "Image 1.jpg");
            string imagePath2ToCopy = Path.Combine(assetsDirectory, "Homer.gif");
            string videoPath1ToCopy = Path.Combine(assetsDirectory, "Homer.mp4");

            string firstFrameVideosDirectory = _userConfigurationService!.PathSettings.FirstFrameVideosPath;
            string firstFramePath1 = Path.Combine(firstFrameVideosDirectory, "Homer.jpg");

            File.Copy(imagePath1, imagePath1ToCopy);
            File.Copy(imagePath2, imagePath2ToCopy);
            File.Copy(videoPath1, videoPath1ToCopy);

            List<string> assetPaths = [imagePath1ToCopy, imagePath2ToCopy, videoPath1ToCopy];
            List<string> assetPathsAfterSync = [imagePath2ToCopy, imagePath1ToCopy, firstFramePath1];
            List<Asset> expectedAssets = [_asset3Temp!, _asset2Temp!, _asset4Temp!];
            List<int> assetsImageByteSize = [ASSET3_TEMP_IMAGE_BYTE_SIZE, ASSET2_TEMP_IMAGE_BYTE_SIZE, ASSET4_TEMP_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(3, assetsInDirectory.Length);

            foreach (string assetPath in assetPaths)
            {
                Assert.IsTrue(File.Exists(assetPath));
            }

            Assert.IsFalse(File.Exists(firstFramePath1));

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNull(folder);

            Folder? videoFirstFramefolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.IsNull(videoFirstFramefolder);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CheckBackupBefore(backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            List<Asset> videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.IsEmpty(videoFirstFramesFromRepositoryByPath);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _catalogAssetsService!.CatalogAssetsAsync(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            videoFirstFramefolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.IsNotNull(videoFirstFramefolder);

            assetsInDirectory = Directory.GetFiles(firstFrameVideosDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);
            Assert.IsTrue(File.Exists(firstFramePath1));

            _asset3Temp!.Folder = folder!;
            _asset3Temp!.FolderId = folder!.FolderId;
            _asset2Temp!.Folder = folder;
            _asset2Temp!.FolderId = folder.FolderId;
            _asset4Temp!.Folder = videoFirstFramefolder!;
            _asset4Temp!.FolderId = videoFirstFramefolder!.FolderId;

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(2, assetsFromRepositoryByPath.Count);

            videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.AreEqual(1, videoFirstFramesFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(3, assetsFromRepository.Count);

            List<Folder> expectedFolders = [folder, folder, videoFirstFramefolder];
            List<string> expectedDirectories = [assetsDirectory, assetsDirectory, firstFrameVideosDirectory];

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssets[i], assetPathsAfterSync[i], expectedDirectories[i], expectedFolders[i]);
            }

            List<Folder> folders = [folder, videoFirstFramefolder];
            Dictionary<Folder, List<Asset>> folderToAssetsMapping = new()
            {
                { folder, [_asset3Temp!, _asset2Temp!]},
                { videoFirstFramefolder, [_asset4Temp!]}
            };
            Dictionary<string, int> assetNameToByteSizeMapping = new()
            {
                { _asset3Temp!.FileName, ASSET3_TEMP_IMAGE_BYTE_SIZE },
                { _asset2Temp!.FileName, ASSET2_TEMP_IMAGE_BYTE_SIZE },
                { _asset4Temp!.FileName, ASSET4_TEMP_IMAGE_BYTE_SIZE }
            };

            AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMapping, folders, thumbnails, assetsImageByteSize);
            CheckBlobsAndTablesAfterSaveCatalog(
                blobsPath,
                tablesPath,
                folders,
                folders,
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            CheckBackupAfter(
                backupFilePath,
                blobsPath,
                tablesPath,
                folders,
                folders,
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.AreEqual(10, catalogChanges.Count);

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);

            foreach (Asset expectedAsset in folderToAssetsMapping[folder])
            {
                CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder],
                    expectedAsset,
                    folder,
                    ref increment);
            }

            CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            CheckCatalogChangesAssetAdded(
                catalogChanges,
                firstFrameVideosDirectory,
                folderToAssetsMapping[videoFirstFramefolder],
                _asset4Temp!,
                videoFirstFramefolder,
                ref increment);
            CheckCatalogChangesBackup(catalogChanges, CREATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);

            // Second sync

            File.Delete(videoPath1ToCopy);

            assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(2, assetsInDirectory.Length);

            await _catalogAssetsService!.CatalogAssetsAsync(catalogChanges.Add);

            assetsInDirectory = Directory.GetFiles(firstFrameVideosDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);
            Assert.IsTrue(File.Exists(firstFramePath1));

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            videoFirstFramefolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.IsNotNull(videoFirstFramefolder);

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(2, assetsFromRepositoryByPath.Count);

            videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.AreEqual(1, videoFirstFramesFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(3, assetsFromRepository.Count);

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssets[i], assetPathsAfterSync[i], expectedDirectories[i], expectedFolders[i]);
            }

            AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMapping, folders, thumbnails, assetsImageByteSize);
            CheckBlobsAndTablesAfterSaveCatalog(
                blobsPath,
                tablesPath,
                folders,
                folders,
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            CheckBackupAfter(
                backupFilePath,
                blobsPath,
                tablesPath,
                folders,
                folders,
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.AreEqual(14, catalogChanges.Count);

            increment = 0;

            foldersInRepository = _testableAssetRepository!.GetFolders();

            CheckCatalogChangesInspectingFolder(catalogChanges,folders.Count, foldersInRepository, assetsDirectory, ref increment);

            foreach (Asset expectedAsset in folderToAssetsMapping[folder!])
            {
                CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder!],
                    expectedAsset,
                    folder!,
                    ref increment);
            }

            CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            CheckCatalogChangesAssetAdded(
                catalogChanges,
                firstFrameVideosDirectory,
                folderToAssetsMapping[videoFirstFramefolder!],
                _asset4Temp!,
                videoFirstFramefolder!,
                ref increment);

            CheckCatalogChangesBackup(catalogChanges, CREATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);

            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(assetsDirectory, true);
        }
    }

    [Test]
    public async Task CatalogAssetsAsync_AssetsAndRootCatalogFolderExistsAndOneVideoIsDeletedAndAnalyseVideosIsFalse_SyncTheAssets()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "TempAssetsDirectory");

        ConfigureCatalogAssetService(100, assetsDirectory, 200, 150, false, false, false, false);

        try
        {
            Directory.CreateDirectory(assetsDirectory);

            string imagePath1 = Path.Combine(_dataDirectory!, "Image 1.jpg");
            string imagePath2 = Path.Combine(_dataDirectory!, "Homer.gif");
            string videoPath1 = Path.Combine(_dataDirectory!, "Homer.mp4");

            string imagePath1ToCopy = Path.Combine(assetsDirectory, "Image 1.jpg");
            string imagePath2ToCopy = Path.Combine(assetsDirectory, "Homer.gif");
            string videoPath1ToCopy = Path.Combine(assetsDirectory, "Homer.mp4");

            string firstFrameVideosDirectory = _userConfigurationService!.PathSettings.FirstFrameVideosPath;
            string firstFramePath1 = Path.Combine(firstFrameVideosDirectory, "Homer.jpg");

            File.Copy(imagePath1, imagePath1ToCopy);
            File.Copy(imagePath2, imagePath2ToCopy);
            File.Copy(videoPath1, videoPath1ToCopy);

            List<string> assetPaths = [imagePath1ToCopy, imagePath2ToCopy, videoPath1ToCopy];
            List<string> assetPathsAfterSync = [imagePath2ToCopy, imagePath1ToCopy];
            List<Asset> expectedAssets = [_asset3Temp!, _asset2Temp!];
            List<int> assetsImageByteSize = [ASSET3_TEMP_IMAGE_BYTE_SIZE, ASSET2_TEMP_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(3, assetsInDirectory.Length);

            foreach (string assetPath in assetPaths)
            {
                Assert.IsTrue(File.Exists(assetPath));
            }

            Assert.IsFalse(File.Exists(firstFramePath1));

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNull(folder);

            Folder? videoFirstFramefolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.IsNull(videoFirstFramefolder);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CheckBackupBefore(backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            List<Asset> videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.IsEmpty(videoFirstFramesFromRepositoryByPath);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _catalogAssetsService!.CatalogAssetsAsync(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            videoFirstFramefolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.IsNull(videoFirstFramefolder);

            Assert.IsFalse(File.Exists(firstFramePath1));

            _asset3Temp!.Folder = folder!;
            _asset3Temp!.FolderId = folder!.FolderId;
            _asset2Temp!.Folder = folder;
            _asset2Temp!.FolderId = folder.FolderId;

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(2, assetsFromRepositoryByPath.Count);

            videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.IsEmpty(videoFirstFramesFromRepositoryByPath);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(2, assetsFromRepository.Count);

            List<Folder> expectedFolders = [folder, folder];
            List<string> expectedDirectories = [assetsDirectory, assetsDirectory];

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssets[i], assetPathsAfterSync[i], expectedDirectories[i], expectedFolders[i]);
            }

            Dictionary<Folder, List<Asset>> folderToAssetsMapping = new() { { folder, [_asset3Temp!, _asset2Temp!]} };
            Dictionary<string, int> assetNameToByteSizeMapping = new()
            {
                { _asset3Temp!.FileName, ASSET3_TEMP_IMAGE_BYTE_SIZE },
                { _asset2Temp!.FileName, ASSET2_TEMP_IMAGE_BYTE_SIZE }
            };

            AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMapping, [folder], thumbnails, assetsImageByteSize);
            CheckBlobsAndTablesAfterSaveCatalog(
                blobsPath,
                tablesPath,
                [folder],
                [folder],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            CheckBackupAfter(
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder],
                [folder],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.AreEqual(7, catalogChanges.Count);

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            foreach (Asset expectedAsset in folderToAssetsMapping[folder])
            {
                CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder],
                    expectedAsset,
                    folder,
                    ref increment);
            }

            CheckCatalogChangesBackup(catalogChanges, CREATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);

            // Second sync

            File.Delete(videoPath1ToCopy);

            assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(2, assetsInDirectory.Length);

            await _catalogAssetsService!.CatalogAssetsAsync(catalogChanges.Add);

            Assert.IsFalse(File.Exists(firstFramePath1));

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            videoFirstFramefolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.IsNull(videoFirstFramefolder);

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(2, assetsFromRepositoryByPath.Count);

            videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.IsEmpty(videoFirstFramesFromRepositoryByPath);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(2, assetsFromRepository.Count);

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssets[i], assetPathsAfterSync[i], expectedDirectories[i], expectedFolders[i]);
            }

            AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMapping, [folder!], thumbnails, assetsImageByteSize);
            CheckBlobsAndTablesAfterSaveCatalog(
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            CheckBackupAfter(
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.AreEqual(10, catalogChanges.Count);

            increment = 0;

            foldersInRepository = _testableAssetRepository!.GetFolders();

            CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            foreach (Asset expectedAsset in folderToAssetsMapping[folder!])
            {
                CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder!],
                    expectedAsset,
                    folder!,
                    ref increment);
            }

            CheckCatalogChangesBackup(catalogChanges, CREATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);

            CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(assetsDirectory, true);
        }
    }

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public async Task CatalogAssetsAsync_AssetsAndRootCatalogFolderExistsAndOneImageIsDeletedAndCatalogBatchSizeIsSmaller_SyncTheAssets(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "TempAssetsDirectory");

        ConfigureCatalogAssetService(1, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        try
        {
            Directory.CreateDirectory(assetsDirectory);

            string imagePath1 = Path.Combine(_dataDirectory!, "Image 1.jpg");
            string imagePath2 = Path.Combine(_dataDirectory!, "Homer.gif");

            string imagePath1ToCopy = Path.Combine(assetsDirectory, "Image 1.jpg");
            string imagePath2ToCopy = Path.Combine(assetsDirectory, "Homer.gif");

            File.Copy(imagePath1, imagePath1ToCopy);

            List<string> assetPaths = [imagePath1ToCopy];
            List<Asset> expectedAssets = [_asset2Temp!];
            List<int> assetsImageByteSize = [ASSET2_TEMP_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);

            foreach (string assetPath in assetPaths)
            {
                Assert.IsTrue(File.Exists(assetPath));
            }

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNull(folder);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CheckBackupBefore(backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _catalogAssetsService!.CatalogAssetsAsync(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            _asset2Temp!.Folder = folder!;
            _asset2Temp!.FolderId = folder!.FolderId;

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(1, assetsFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(1, assetsFromRepository.Count);

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssets[i], assetPaths[i], assetsDirectory, folder);
            }

            Dictionary<Folder, List<Asset>> folderToAssetsMapping = new() { { folder, expectedAssets } };
            Dictionary<string, int> assetNameToByteSizeMapping = new() { { _asset2Temp!.FileName, ASSET2_TEMP_IMAGE_BYTE_SIZE } };

            AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMapping, [folder], thumbnails, assetsImageByteSize);
            CheckBlobsAndTablesAfterSaveCatalog(
                blobsPath,
                tablesPath,
                [folder],
                [folder],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            CheckBackupAfter(
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder],
                [folder],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.AreEqual(6, catalogChanges.Count);

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            foreach (Asset expectedAsset in folderToAssetsMapping[folder])
            {
                CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder],
                    expectedAsset,
                    folder,
                    ref increment);
            }

            CheckCatalogChangesBackup(catalogChanges, CREATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);

            // Second sync

            List<string> assetPathsUpdated = [imagePath1ToCopy, imagePath2ToCopy];
            List<Asset> expectedAssetsUpdated = [_asset2Temp!, _asset3Temp!];
            List<int> assetsImageByteSizeUpdated = [ASSET2_TEMP_IMAGE_BYTE_SIZE, ASSET3_TEMP_IMAGE_BYTE_SIZE];

            Dictionary<Folder, List<Asset>> folderToAssetsMappingUpdated = new()
            {
                { folder, [_asset2Temp!, _asset3Temp!] }
            };
            Dictionary<string, int> assetNameToByteSizeMappingUpdated = new()
            {
                { _asset2Temp!.FileName, ASSET2_TEMP_IMAGE_BYTE_SIZE },
                { _asset3Temp!.FileName, ASSET3_TEMP_IMAGE_BYTE_SIZE }
            };

            File.Delete(imagePath1ToCopy);
            File.Copy(imagePath2, imagePath2ToCopy);

            assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);

            await _catalogAssetsService!.CatalogAssetsAsync(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            _asset3Temp!.Folder = folder!;
            _asset3Temp!.FolderId = folder!.FolderId;

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(2, assetsFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(2, assetsFromRepository.Count);

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssetsUpdated[i], assetPathsUpdated[i], assetsDirectory, folder);
            }

            AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMappingUpdated, [folder], thumbnails, assetsImageByteSizeUpdated);
            CheckBlobsAndTablesAfterSaveCatalog(
                blobsPath,
                tablesPath,
                [folder],
                [folder],
                assetsFromRepository,
                folderToAssetsMappingUpdated,
                assetNameToByteSizeMappingUpdated);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            CheckBackupAfter(
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder],
                [folder],
                assetsFromRepository,
                folderToAssetsMappingUpdated,
                assetNameToByteSizeMappingUpdated);

            Assert.AreEqual(12, catalogChanges.Count);

            increment = 0;

            foldersInRepository = _testableAssetRepository!.GetFolders();

            CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            foreach (Asset expectedAsset in folderToAssetsMapping[folder])
            {
                CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder],
                    expectedAsset,
                    folder,
                    ref increment);
            }

            CheckCatalogChangesBackup(catalogChanges, CREATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);

            CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);
            CheckCatalogChangesAssetAdded(
                catalogChanges,
                assetsDirectory,
                folderToAssetsMappingUpdated[folder],
                _asset3Temp,
                folder,
                ref increment);
            CheckCatalogChangesBackup(catalogChanges, UPDATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(assetsDirectory, true);
        }
    }

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public async Task CatalogAssetsAsync_AssetsAndRootCatalogFolderExistsAndOneImageIsDeletedAndBackupIsDeleted_SyncTheAssetsAndRemovesDeletedOne(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder2");
        string destinationFilePathToCopy = Path.Combine(assetsDirectory, _asset1Temp!.FileName);

        ConfigureCatalogAssetService(100, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        try
        {
            string imagePath1 = Path.Combine(assetsDirectory, "Image 1_duplicate.jpg");
            string imagePath2 = Path.Combine(assetsDirectory, "Image 9.png");
            string imagePath3 = Path.Combine(assetsDirectory, "Image 9_duplicate.png");
            string imagePath4 = Path.Combine(assetsDirectory, "Image_11.heic");
            File.Copy(imagePath1, destinationFilePathToCopy);

            List<string> assetPaths = [imagePath1, destinationFilePathToCopy, imagePath2, imagePath3, imagePath4];
            List<Asset> expectedAssets = [_asset1!, _asset1Temp!, _asset2!, _asset3!, _asset4!];
            List<int> assetsImageByteSize = [ASSET1_IMAGE_BYTE_SIZE, ASSET1_TEMP_IMAGE_BYTE_SIZE, ASSET2_IMAGE_BYTE_SIZE, ASSET3_IMAGE_BYTE_SIZE, ASSET4_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(5, assetsInDirectory.Length);

            foreach (string assetPath in assetPaths)
            {
                Assert.IsTrue(File.Exists(assetPath));
            }

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNull(folder);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CheckBackupBefore(backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _catalogAssetsService!.CatalogAssetsAsync(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            _asset1!.Folder = folder!;
            _asset1!.FolderId = folder!.FolderId;
            _asset1Temp!.Folder = folder;
            _asset1Temp!.FolderId = folder.FolderId;
            _asset2!.Folder = folder;
            _asset2!.FolderId = folder.FolderId;
            _asset3!.Folder = folder;
            _asset3!.FolderId = folder.FolderId;
            _asset4!.Folder = folder;
            _asset4!.FolderId = folder.FolderId;

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(5, assetsFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(5, assetsFromRepository.Count);

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssets[i], assetPaths[i], assetsDirectory, folder);
            }

            Dictionary<Folder, List<Asset>> folderToAssetsMapping = new() { { folder, expectedAssets } };
            Dictionary<string, int> assetNameToByteSizeMapping = new()
            {
                { _asset1!.FileName, ASSET1_IMAGE_BYTE_SIZE },
                { _asset1Temp!.FileName, ASSET1_TEMP_IMAGE_BYTE_SIZE },
                { _asset2!.FileName, ASSET2_IMAGE_BYTE_SIZE },
                { _asset3!.FileName, ASSET3_IMAGE_BYTE_SIZE },
                { _asset4!.FileName, ASSET4_IMAGE_BYTE_SIZE }
            };

            AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMapping, [folder], thumbnails, assetsImageByteSize);
            CheckBlobsAndTablesAfterSaveCatalog(
                blobsPath,
                tablesPath,
                [folder],
                [folder],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            CheckBackupAfter(
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder],
                [folder],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.AreEqual(10, catalogChanges.Count);

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            foreach (Asset expectedAsset in folderToAssetsMapping[folder])
            {
                CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder],
                    expectedAsset,
                    folder,
                    ref increment);
            }

            CheckCatalogChangesBackup(catalogChanges, CREATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);

            // Second sync

            RemoveDatabaseBackup([folder], blobsPath, tablesPath, backupFilePath);

            File.Delete(destinationFilePathToCopy);

            List<string> assetPathsUpdated = [];
            assetPaths.ForEach(assetPathsUpdated.Add);
            assetPathsUpdated.Remove(destinationFilePathToCopy);

            List<Asset> expectedAssetsUpdated = [];
            expectedAssets.ForEach(expectedAssetsUpdated.Add);
            expectedAssetsUpdated.Remove(_asset1Temp);

            List<int> assetsImageByteSizeUpdated = [];
            assetsImageByteSize.ForEach(assetsImageByteSizeUpdated.Add);
            assetsImageByteSizeUpdated.Remove(ASSET1_TEMP_IMAGE_BYTE_SIZE);

            assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(4, assetsInDirectory.Length);

            Assert.IsFalse(File.Exists(destinationFilePathToCopy));

            await _catalogAssetsService!.CatalogAssetsAsync(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(4, assetsFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(4, assetsFromRepository.Count);

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssetsUpdated[i], assetPathsUpdated[i], assetsDirectory, folder!);
            }

            Dictionary<Folder, List<Asset>> folderToAssetsMappingUpdated = new() { { folder!, expectedAssetsUpdated} };
            Dictionary<string, int> assetNameToByteSizeMappingUpdated = new()
            {
                { _asset1!.FileName, ASSET1_IMAGE_BYTE_SIZE },
                { _asset2!.FileName, ASSET2_IMAGE_BYTE_SIZE },
                { _asset3!.FileName, ASSET3_IMAGE_BYTE_SIZE },
                { _asset4!.FileName, ASSET4_IMAGE_BYTE_SIZE }
            };

            AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMappingUpdated, [folder!], thumbnails, assetsImageByteSizeUpdated);
            CheckBlobsAndTablesAfterSaveCatalog(
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMappingUpdated,
                assetNameToByteSizeMappingUpdated);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            CheckBackupAfter(
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMappingUpdated,
                assetNameToByteSizeMappingUpdated);

            Assert.AreEqual(16, catalogChanges.Count);

            increment = 0;

            foldersInRepository = _testableAssetRepository!.GetFolders();

            CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            foreach (Asset expectedAsset in folderToAssetsMapping[folder!])
            {
                CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder!],
                    expectedAsset,
                    folder!,
                    ref increment);
            }

            CheckCatalogChangesBackup(catalogChanges, CREATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);

            CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            CheckCatalogChangesAssetDeleted(catalogChanges, assetsDirectory, expectedAssetsUpdated, _asset1Temp, folder!, ref increment);
            CheckCatalogChangesBackup(catalogChanges, CREATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    // TODO: Rework the CancellationRequested
    [Test]
    [Ignore("Need to rework the CancellationRequested")]
    [TestCase(false)]
    [TestCase(true)]
    public async Task CatalogAssetsAsync_AssetsAndRootCatalogFolderExistsAndOneImageDeletedAndIsCancellationRequested_SyncTheAssetsAndRemovesDeletedOne(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder2");
        string destinationFilePathToCopy = Path.Combine(assetsDirectory, _asset1Temp!.FileName);

        ConfigureCatalogAssetService(100, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        try
        {
            string imagePath1 = Path.Combine(assetsDirectory, "Image 1_duplicate.jpg");
            string imagePath2 = Path.Combine(assetsDirectory, "Image 9.png");
            string imagePath3 = Path.Combine(assetsDirectory, "Image 9_duplicate.png");
            string imagePath4 = Path.Combine(assetsDirectory, "Image_11.heic");
            File.Copy(imagePath1, destinationFilePathToCopy);

            List<string> assetPaths = [imagePath1, destinationFilePathToCopy, imagePath2, imagePath3, imagePath4];
            List<Asset> expectedAssets = [_asset1!, _asset1Temp!, _asset2!, _asset3!, _asset4!];
            List<int> assetsImageByteSize = [ASSET1_IMAGE_BYTE_SIZE, ASSET1_TEMP_IMAGE_BYTE_SIZE, ASSET2_IMAGE_BYTE_SIZE, ASSET3_IMAGE_BYTE_SIZE, ASSET4_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(5, assetsInDirectory.Length);

            foreach (string assetPath in assetPaths)
            {
                Assert.IsTrue(File.Exists(assetPath));
            }

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNull(folder);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CheckBackupBefore(backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _catalogAssetsService!.CatalogAssetsAsync(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            _asset1!.Folder = folder!;
            _asset1!.FolderId = folder!.FolderId;
            _asset1Temp!.Folder = folder;
            _asset1Temp!.FolderId = folder.FolderId;
            _asset2!.Folder = folder;
            _asset2!.FolderId = folder.FolderId;
            _asset3!.Folder = folder;
            _asset3!.FolderId = folder.FolderId;
            _asset4!.Folder = folder;
            _asset4!.FolderId = folder.FolderId;

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(5, assetsFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(5, assetsFromRepository.Count);

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssets[i], assetPaths[i], assetsDirectory, folder);
            }

            Dictionary<Folder, List<Asset>> folderToAssetsMapping = new() { { folder, expectedAssets } };
            Dictionary<string, int> assetNameToByteSizeMapping = new()
            {
                { _asset1!.FileName, ASSET1_IMAGE_BYTE_SIZE },
                { _asset1Temp!.FileName, ASSET1_TEMP_IMAGE_BYTE_SIZE },
                { _asset2!.FileName, ASSET2_IMAGE_BYTE_SIZE },
                { _asset3!.FileName, ASSET3_IMAGE_BYTE_SIZE },
                { _asset4!.FileName, ASSET4_IMAGE_BYTE_SIZE }
            };

            AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMapping, [folder], thumbnails, assetsImageByteSize);
            CheckBlobsAndTablesAfterSaveCatalog(
                blobsPath,
                tablesPath,
                [folder],
                [folder],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            CheckBackupAfter(
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder],
                [folder],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.AreEqual(10, catalogChanges.Count);

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            foreach (Asset expectedAsset in folderToAssetsMapping[folder])
            {
                CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder],
                    expectedAsset,
                    folder,
                    ref increment);
            }

            CheckCatalogChangesBackup(catalogChanges, CREATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);

            // Second sync

            File.Delete(destinationFilePathToCopy);

            List<string> assetPathsUpdated = [];
            assetPaths.ForEach(assetPathsUpdated.Add);
            assetPathsUpdated.Remove(destinationFilePathToCopy);

            List<Asset> expectedAssetsUpdated = [];
            expectedAssets.ForEach(expectedAssetsUpdated.Add);
            expectedAssetsUpdated.Remove(_asset1Temp);

            List<int> assetsImageByteSizeUpdated = [];
            assetsImageByteSize.ForEach(assetsImageByteSizeUpdated.Add);
            assetsImageByteSizeUpdated.Remove(ASSET1_TEMP_IMAGE_BYTE_SIZE);

            assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(4, assetsInDirectory.Length);

            Assert.IsFalse(File.Exists(destinationFilePathToCopy));

            CancellationToken cancellationToken = new (true);
            await _catalogAssetsService!.CatalogAssetsAsync(catalogChanges.Add, cancellationToken);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(4, assetsFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(4, assetsFromRepository.Count);

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssetsUpdated[i], assetPathsUpdated[i], assetsDirectory, folder!);
            }

            Dictionary<Folder, List<Asset>> folderToAssetsMappingUpdated = new() { { folder!, expectedAssetsUpdated} };
            Dictionary<string, int> assetNameToByteSizeMappingUpdated = new()
            {
                { _asset1!.FileName, ASSET1_IMAGE_BYTE_SIZE },
                { _asset2!.FileName, ASSET2_IMAGE_BYTE_SIZE },
                { _asset3!.FileName, ASSET3_IMAGE_BYTE_SIZE },
                { _asset4!.FileName, ASSET4_IMAGE_BYTE_SIZE },
                { _asset1Temp!.FileName, ASSET1_TEMP_IMAGE_BYTE_SIZE }
            };

            AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMappingUpdated, [folder!], thumbnails, assetsImageByteSizeUpdated);
            CheckBlobsAndTablesAfterSaveCatalog(
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMappingUpdated,
                assetNameToByteSizeMappingUpdated);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            CheckBackupAfter(
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMappingUpdated,
                assetNameToByteSizeMappingUpdated);

            Assert.AreEqual(16, catalogChanges.Count);

            increment = 0;

            foldersInRepository = _testableAssetRepository!.GetFolders();

            CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            foreach (Asset expectedAsset in folderToAssetsMapping[folder!])
            {
                CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder!],
                    expectedAsset,
                    folder!,
                    ref increment);
            }

            CheckCatalogChangesBackup(catalogChanges, CREATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);

            CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            CheckCatalogChangesAssetDeleted(catalogChanges, assetsDirectory, expectedAssetsUpdated, _asset1Temp, folder!, ref increment);
            CheckCatalogChangesBackup(catalogChanges, UPDATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public async Task CatalogAssetsAsync_AssetsAndRootCatalogFolderExistsAndOneFolderIsDeleted_SyncTheAssetsAndRemovesDeletedOnes(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder2");
        string tempDirectory = Path.Combine(assetsDirectory, "TempFolder");
        string destinationFilePathToCopy = Path.Combine(tempDirectory, _asset1Temp!.FileName);

        ConfigureCatalogAssetService(100, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        try
        {
            Directory.CreateDirectory(tempDirectory);

            string imagePath1 = Path.Combine(assetsDirectory, "Image 1_duplicate.jpg");
            string imagePath2 = Path.Combine(assetsDirectory, "Image 9.png");
            string imagePath3 = Path.Combine(assetsDirectory, "Image 9_duplicate.png");
            string imagePath4 = Path.Combine(assetsDirectory, "Image_11.heic");
            File.Copy(imagePath1, destinationFilePathToCopy);

            List<string> assetPaths = [imagePath1, imagePath2, imagePath3, imagePath4, destinationFilePathToCopy];
            List<Asset> expectedAssets = [_asset1!, _asset2!, _asset3!, _asset4!, _asset1Temp!];

            List<int> assetsImageByteSizeFirstSync = [ASSET1_IMAGE_BYTE_SIZE, ASSET2_IMAGE_BYTE_SIZE, ASSET3_IMAGE_BYTE_SIZE, ASSET4_IMAGE_BYTE_SIZE, ASSET1_TEMP_IMAGE_BYTE_SIZE];
            List<int> assetsImageByteSizeSecondSync = [ASSET1_IMAGE_BYTE_SIZE, ASSET2_IMAGE_BYTE_SIZE, ASSET3_IMAGE_BYTE_SIZE, ASSET4_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory1 = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(4, assetsInDirectory1.Length);

            string[] assetsInDirectory2 = Directory.GetFiles(tempDirectory);
            Assert.AreEqual(1, assetsInDirectory2.Length);

            foreach (string assetPath in assetPaths)
            {
                Assert.IsTrue(File.Exists(assetPath));
            }

            Folder? folder1 = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNull(folder1);

            Folder? folder2 = _testableAssetRepository!.GetFolderByPath(tempDirectory);
            Assert.IsNull(folder2);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CheckBackupBefore(backupFilePath);

            List<Asset> assetsFromRepositoryByPath1 = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath1);

            List<Asset> assetsFromRepositoryByPath2 = _testableAssetRepository.GetCataloguedAssetsByPath(tempDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath2);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _catalogAssetsService!.CatalogAssetsAsync(catalogChanges.Add);

            folder1 = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder1);

            folder2 = _testableAssetRepository!.GetFolderByPath(tempDirectory);
            Assert.IsNotNull(folder2);

            _asset1!.Folder = folder1!;
            _asset1!.FolderId = folder1!.FolderId;
            _asset2!.Folder = folder1;
            _asset2!.FolderId = folder1.FolderId;
            _asset3!.Folder = folder1;
            _asset3!.FolderId = folder1.FolderId;
            _asset4!.Folder = folder1;
            _asset4!.FolderId = folder1.FolderId;

            _asset1Temp!.Folder = folder2!;
            _asset1Temp!.FolderId = folder2!.FolderId;

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath1 = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(4, assetsFromRepositoryByPath1.Count);

            assetsFromRepositoryByPath2 = _testableAssetRepository.GetCataloguedAssetsByPath(tempDirectory);
            Assert.AreEqual(1, assetsFromRepositoryByPath2.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(5, assetsFromRepository.Count);

            List<string> expectedDirectories = [assetsDirectory, assetsDirectory, assetsDirectory, assetsDirectory, tempDirectory];
            List<Folder> expectedFolders = [folder1, folder1, folder1, folder1, folder2];

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssets[i], assetPaths[i], expectedDirectories[i], expectedFolders[i]);
            }

            List<Folder> folders = [folder1, folder2];
            Dictionary<Folder, List<Asset>> folderToAssetsMapping = new() { { folder1, [_asset1, _asset2, _asset3, _asset4] }, { folder2, [_asset1Temp] } };
            Dictionary<string, int> assetNameToByteSizeMapping = new()
            {
                { _asset1!.FileName, ASSET1_IMAGE_BYTE_SIZE },
                { _asset2!.FileName, ASSET2_IMAGE_BYTE_SIZE },
                { _asset3!.FileName, ASSET3_IMAGE_BYTE_SIZE },
                { _asset4!.FileName, ASSET4_IMAGE_BYTE_SIZE },
                { _asset1Temp!.FileName, ASSET1_TEMP_IMAGE_BYTE_SIZE }
            };

            AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMapping, folders, thumbnails, assetsImageByteSizeFirstSync);
            CheckBlobsAndTablesAfterSaveCatalog(
                blobsPath,
                tablesPath,
                folders,
                folders,
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            CheckBackupAfter(
                backupFilePath,
                blobsPath,
                tablesPath,
                folders,
                folders,
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.AreEqual(12, catalogChanges.Count);

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);

            foreach (Asset expectedAsset in folderToAssetsMapping[folder1])
            {
                CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder1],
                    expectedAsset,
                    folder1,
                    ref increment);
            }

            CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, tempDirectory, ref increment);
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, tempDirectory, ref increment);

            foreach (Asset expectedAsset in folderToAssetsMapping[folder2])
            {
                CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    tempDirectory,
                    folderToAssetsMapping[folder2],
                    expectedAsset,
                    folder2,
                    ref increment);
            }

            CheckCatalogChangesBackup(catalogChanges, CREATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);

            // Second sync

            Directory.Delete(tempDirectory, true);

            List<string> assetPathsUpdated = [];
            assetPaths.ForEach(assetPathsUpdated.Add);
            assetPathsUpdated.Remove(destinationFilePathToCopy);

            List<Asset> expectedAssetsUpdated = [];
            expectedAssets.ForEach(expectedAssetsUpdated.Add);
            expectedAssetsUpdated.Remove(_asset1Temp);

            assetsInDirectory1 = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(4, assetsInDirectory1.Length);

            Assert.IsFalse(File.Exists(destinationFilePathToCopy));

            await _catalogAssetsService!.CatalogAssetsAsync(catalogChanges.Add);

            folder1 = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder1);

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath1 = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(4, assetsFromRepositoryByPath1.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(4, assetsFromRepository.Count);

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssetsUpdated[i], assetPathsUpdated[i], assetsDirectory, folder1!);
            }

            Dictionary<Folder, List<Asset>> folderToAssetsMappingUpdated = new() { { folder1!, expectedAssetsUpdated} };
            Dictionary<string, int> assetNameToByteSizeMappingUpdated = new()
            {
                { _asset1!.FileName, ASSET1_IMAGE_BYTE_SIZE },
                { _asset2!.FileName, ASSET2_IMAGE_BYTE_SIZE },
                { _asset3!.FileName, ASSET3_IMAGE_BYTE_SIZE },
                { _asset4!.FileName, ASSET4_IMAGE_BYTE_SIZE }
            };

            AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMappingUpdated, [folder1!], thumbnails, assetsImageByteSizeSecondSync);
            CheckBlobsAndTablesAfterSaveCatalog(
                blobsPath,
                tablesPath,
                [folder1!],
                [folder1!],
                assetsFromRepository,
                folderToAssetsMappingUpdated,
                assetNameToByteSizeMappingUpdated);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            CheckBackupAfter(
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder1!],
                [folder1!],
                assetsFromRepository,
                folderToAssetsMappingUpdated,
                assetNameToByteSizeMappingUpdated);

            Assert.AreEqual(19, catalogChanges.Count);

            increment = 0;

            foldersInRepository = _testableAssetRepository!.GetFolders();

            CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            foreach (Asset expectedAsset in folderToAssetsMapping[folder1!])
            {
                CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder1!],
                    expectedAsset,
                    folder1!,
                    ref increment);
            }

            CheckCatalogChangesFolderAdded(catalogChanges, 1, [folder2], tempDirectory, ref increment); // Not foldersInRepository because this folder has been deleted
            CheckCatalogChangesInspectingFolder(catalogChanges, 1, [folder2], tempDirectory, ref increment); // Not foldersInRepository because this folder has been deleted

            foreach (Asset expectedAsset in folderToAssetsMapping[folder2])
            {
                CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    tempDirectory,
                    folderToAssetsMapping[folder2],
                    expectedAsset,
                    folder2,
                    ref increment);
            }

            CheckCatalogChangesBackup(catalogChanges, CREATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);

            CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);
            CheckCatalogChangesAssetDeleted(catalogChanges, tempDirectory, assetsFromRepositoryByPath2, _asset1Temp, folder2, ref increment);
            CheckCatalogChangesFolderDeleted(catalogChanges, 1, foldersInRepository.Length, tempDirectory, ref increment);

            CheckCatalogChangesBackup(catalogChanges, UPDATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            // If failing, just in case
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, true);
            }
        }
    }

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public async Task CatalogAssetsAsync_AssetsAndRootCatalogFolderExistsAndOneFolderIsDeletedAndAndCatalogBatchSizeIsSmaller_SyncTheAssetsAndDoesNotRemoveAllDeletedOnes(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "TempAssetsDirectory");
        string tempDirectory = Path.Combine(assetsDirectory, "FolderToDelete");

        ConfigureCatalogAssetService(1, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        try
        {
            Directory.CreateDirectory(assetsDirectory);
            Directory.CreateDirectory(tempDirectory);

            string imagePath1 = Path.Combine(_dataDirectory!, "Image 1.jpg");
            string imagePath2 = Path.Combine(_dataDirectory!, "Image 9.png");
            string destinationFilePathToCopy1 = Path.Combine(tempDirectory, _asset2Temp!.FileName);
            string destinationFilePathToCopy2 = Path.Combine(tempDirectory, _asset2!.FileName);
            File.Copy(imagePath1, destinationFilePathToCopy1);
            File.Copy(imagePath2, destinationFilePathToCopy2);

            List<string> assetPaths = [destinationFilePathToCopy1, destinationFilePathToCopy2];

            List<int> assetsImageByteSizeFirstSync = [ASSET2_TEMP_IMAGE_BYTE_SIZE];
            List<int> assetsImageByteSizeSecondSync = [ASSET2_TEMP_IMAGE_BYTE_SIZE, ASSET2_IMAGE_BYTE_SIZE];
            List<int> assetsImageByteSizeThirdSync = [ASSET2_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory1 = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(0, assetsInDirectory1.Length);

            string[] assetsInDirectory2 = Directory.GetFiles(tempDirectory);
            Assert.AreEqual(2, assetsInDirectory2.Length);

            foreach (string assetPath in assetPaths)
            {
                Assert.IsTrue(File.Exists(assetPath));
            }

            Folder? folder1 = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNull(folder1);

            Folder? folder2 = _testableAssetRepository!.GetFolderByPath(tempDirectory);
            Assert.IsNull(folder2);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CheckBackupBefore(backupFilePath);

            List<Asset> assetsFromRepositoryByPath1 = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath1);

            List<Asset> assetsFromRepositoryByPath2 = _testableAssetRepository.GetCataloguedAssetsByPath(tempDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath2);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _catalogAssetsService!.CatalogAssetsAsync(catalogChanges.Add);

            folder1 = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder1);

            folder2 = _testableAssetRepository!.GetFolderByPath(tempDirectory);
            Assert.IsNotNull(folder2);

            _asset2Temp!.Folder = folder2!;
            _asset2Temp!.FolderId = folder2!.FolderId;

            List<Folder> folders = [folder1!, folder2];
            Dictionary<Folder, List<Asset>> folderToAssetsMappingFirstSync = new() { { folder2, [_asset2Temp] } };
            Dictionary<string, int> assetNameToByteSizeMappingFirstSync = new() { { _asset2Temp!.FileName, ASSET2_TEMP_IMAGE_BYTE_SIZE } };

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath1 = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(0, assetsFromRepositoryByPath1.Count);

            assetsFromRepositoryByPath2 = _testableAssetRepository.GetCataloguedAssetsByPath(tempDirectory);
            Assert.AreEqual(1, assetsFromRepositoryByPath2.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(1, assetsFromRepository.Count);

            AssertAssetPropertyValidity(assetsFromRepository[0], _asset2Temp, destinationFilePathToCopy1, tempDirectory, folder2);
            AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMappingFirstSync, [folder2], thumbnails, assetsImageByteSizeFirstSync); // Only folder2 contains assets
            CheckBlobsAndTablesAfterSaveCatalog(
                blobsPath,
                tablesPath,
                folders,
                [folder2],
                assetsFromRepository,
                folderToAssetsMappingFirstSync,
                assetNameToByteSizeMappingFirstSync);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            CheckBackupAfter(
                backupFilePath,
                blobsPath,
                tablesPath,
                folders,
                [folder2],
                assetsFromRepository,
                folderToAssetsMappingFirstSync,
                assetNameToByteSizeMappingFirstSync);

            Assert.AreEqual(8, catalogChanges.Count);

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);
            CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, tempDirectory, ref increment);
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, tempDirectory, ref increment);
            CheckCatalogChangesAssetAdded(catalogChanges, tempDirectory, folderToAssetsMappingFirstSync[folder2], _asset2Temp, folder2, ref increment);
            CheckCatalogChangesBackup(catalogChanges, CREATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);

            // Second sync

            await _catalogAssetsService!.CatalogAssetsAsync(catalogChanges.Add);

            folder1 = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder1);

            folder2 = _testableAssetRepository!.GetFolderByPath(tempDirectory);
            Assert.IsNotNull(folder2);

            _asset2!.Folder = folder2!;
            _asset2!.FolderId = folder2!.FolderId;

            Dictionary<Folder, List<Asset>> folderToAssetsMappingSecondSync = new() { { folder2, [_asset2Temp, _asset2] } };
            Dictionary<string, int> assetNameToByteSizeMappingSecondSync = new()
            {
                { _asset2Temp!.FileName, ASSET2_TEMP_IMAGE_BYTE_SIZE },
                { _asset2!.FileName, ASSET2_IMAGE_BYTE_SIZE }
            };

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath1 = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(0, assetsFromRepositoryByPath1.Count);

            assetsFromRepositoryByPath2 = _testableAssetRepository.GetCataloguedAssetsByPath(tempDirectory);
            Assert.AreEqual(2, assetsFromRepositoryByPath2.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(2, assetsFromRepository.Count);

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                AssertAssetPropertyValidity(assetsFromRepository[i], folderToAssetsMappingSecondSync[folder2][i], assetPaths[i], tempDirectory, folder2);
            }

            AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMappingSecondSync, [folder2], thumbnails, assetsImageByteSizeSecondSync); // Only folder2 contains assets
            CheckBlobsAndTablesAfterSaveCatalog(
                blobsPath,
                tablesPath,
                folders,
                [folder2],
                assetsFromRepository,
                folderToAssetsMappingSecondSync,
                assetNameToByteSizeMappingSecondSync);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            CheckBackupAfter(
                backupFilePath,
                blobsPath,
                tablesPath,
                folders,
                [folder2],
                assetsFromRepository,
                folderToAssetsMappingSecondSync,
                assetNameToByteSizeMappingSecondSync);

            Assert.AreEqual(15, catalogChanges.Count);

            increment = 0;

            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);
            CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, tempDirectory, ref increment);
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, tempDirectory, ref increment);
            CheckCatalogChangesAssetAdded(catalogChanges, tempDirectory, folderToAssetsMappingFirstSync[folder2], _asset2Temp, folder2, ref increment);
            CheckCatalogChangesBackup(catalogChanges, CREATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);

            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, tempDirectory, ref increment);
            CheckCatalogChangesAssetAdded(catalogChanges, tempDirectory, folderToAssetsMappingSecondSync[folder2], _asset2, folder2, ref increment);
            CheckCatalogChangesBackup(catalogChanges, UPDATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);

            // Third sync

            Directory.Delete(tempDirectory, true);

            Assert.IsFalse(File.Exists(destinationFilePathToCopy1));
            Assert.IsFalse(File.Exists(destinationFilePathToCopy2));

            await _catalogAssetsService!.CatalogAssetsAsync(catalogChanges.Add);

            folder1 = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder1);

            folder2 = _testableAssetRepository!.GetFolderByPath(tempDirectory);
            Assert.IsNotNull(folder2);

            Dictionary<Folder, List<Asset>> folderToAssetsMappingThirdSync = new() { { folder2!, [_asset2] } };
            Dictionary<string, int> assetNameToByteSizeMappingThirdSync = new() { { _asset2!.FileName, ASSET2_IMAGE_BYTE_SIZE } };

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath1 = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(0, assetsFromRepositoryByPath1.Count);

            assetsFromRepositoryByPath2 = _testableAssetRepository.GetCataloguedAssetsByPath(tempDirectory);
            Assert.AreEqual(1, assetsFromRepositoryByPath2.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(1, assetsFromRepository.Count);

            AssertAssetPropertyValidity(assetsFromRepository[0], _asset2, destinationFilePathToCopy2, tempDirectory, folder2!);
            AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMappingThirdSync, [folder2!], thumbnails, assetsImageByteSizeThirdSync); // Only folder2 contains assets
            CheckBlobsAndTablesAfterSaveCatalog(
                blobsPath,
                tablesPath,
                folders,
                [folder2!],
                assetsFromRepository,
                folderToAssetsMappingThirdSync,
                assetNameToByteSizeMappingThirdSync);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            CheckBackupAfter(
                backupFilePath,
                blobsPath,
                tablesPath,
                folders,
                [folder2!],
                assetsFromRepository,
                folderToAssetsMappingThirdSync,
                assetNameToByteSizeMappingThirdSync);

            Assert.AreEqual(21, catalogChanges.Count);

            increment = 0;

            foldersInRepository = _testableAssetRepository!.GetFolders();

            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);
            CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, tempDirectory, ref increment);
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, tempDirectory, ref increment);
            CheckCatalogChangesAssetAdded(catalogChanges, tempDirectory, folderToAssetsMappingFirstSync[folder2!], _asset2Temp, folder2!, ref increment);
            CheckCatalogChangesBackup(catalogChanges, CREATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);

            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, tempDirectory, ref increment);
            CheckCatalogChangesAssetAdded(catalogChanges, tempDirectory, folderToAssetsMappingSecondSync[folder2!], _asset2, folder2!, ref increment);
            CheckCatalogChangesBackup(catalogChanges, UPDATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);

            CheckCatalogChangesInspectingFolder(catalogChanges, 2, foldersInRepository, assetsDirectory, ref increment);
            CheckCatalogChangesAssetDeleted(catalogChanges, tempDirectory, folderToAssetsMappingSecondSync[folder2!], _asset2Temp, folder2!, ref increment);
            CheckCatalogChangesBackup(catalogChanges, UPDATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);

            // Fourth sync

            assetsInDirectory1 = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(0, assetsInDirectory1.Length);

            await _catalogAssetsService!.CatalogAssetsAsync(catalogChanges.Add);

            folder1 = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder1);

            Folder? folder2Updated = _testableAssetRepository!.GetFolderByPath(tempDirectory);
            Assert.IsNull(folder2Updated);

            Dictionary<Folder, List<Asset>> folderToAssetsMappingFourthSync = new();
            Dictionary<string, int> assetNameToByteSizeMappingFourthSync = new();

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath1 = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(0, assetsFromRepositoryByPath1.Count);

            assetsFromRepositoryByPath2 = _testableAssetRepository.GetCataloguedAssetsByPath(tempDirectory);
            Assert.AreEqual(0, assetsFromRepositoryByPath2.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(0, assetsFromRepository.Count);

            AssertThumbnailsValidity(assetsFromRepository, [], [], thumbnails, []); // No Folders and assets anymore
            CheckBlobsAndTablesAfterSaveCatalog(
                blobsPath,
                tablesPath,
                [folder1!],
                [],
                assetsFromRepository,
                folderToAssetsMappingFourthSync,
                assetNameToByteSizeMappingFourthSync);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            CheckBackupAfter(
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder1!],
                [],
                assetsFromRepository,
                folderToAssetsMappingFourthSync,
                assetNameToByteSizeMappingFourthSync);

            Assert.AreEqual(28, catalogChanges.Count);

            increment = 0;

            foldersInRepository = _testableAssetRepository!.GetFolders();

            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, folders, assetsDirectory, ref increment);  // Not foldersInRepository because this folder has been deleted
            CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, folders, tempDirectory, ref increment);
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, folders, tempDirectory, ref increment);
            CheckCatalogChangesAssetAdded(catalogChanges, tempDirectory, folderToAssetsMappingFirstSync[folder2!], _asset2Temp, folder2!, ref increment);
            CheckCatalogChangesBackup(catalogChanges, CREATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);

            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, folders, assetsDirectory, ref increment);
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, folders, tempDirectory, ref increment);
            CheckCatalogChangesAssetAdded(catalogChanges, tempDirectory, folderToAssetsMappingSecondSync[folder2!], _asset2, folder2!, ref increment);
            CheckCatalogChangesBackup(catalogChanges, UPDATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);

            CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);
            CheckCatalogChangesAssetDeleted(catalogChanges, tempDirectory, folderToAssetsMappingSecondSync[folder2!], _asset2Temp, folder2!, ref increment);
            CheckCatalogChangesBackup(catalogChanges, UPDATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);

            CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);
            CheckCatalogChangesAssetDeleted(catalogChanges, tempDirectory, folderToAssetsMappingThirdSync[folder2!], _asset2, folder2!, ref increment);
            CheckCatalogChangesFolderDeleted(catalogChanges, 1, foldersInRepository.Length, tempDirectory, ref increment);
            CheckCatalogChangesBackup(catalogChanges, UPDATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(assetsDirectory, true);
        }
    }

    // --------------------------------------------------------------------------------------------------------------------------------------------
    // Full scenario test
    [Test]
    public async Task CatalogAssetsAsync_AssetsImageAndVideosAndRootCatalogFolderExistsAndSubDirAndUpdateAndDelete_SyncTheAssets()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "TempAssetsDirectory");

        ConfigureCatalogAssetService(100, assetsDirectory, 200, 150, false, false, false, true);

        try
        {
            Directory.CreateDirectory(assetsDirectory);

            string imageDeletedDirectory = Path.Combine(assetsDirectory, "FolderImageDeleted");
            string imageUpdatedDirectory = Path.Combine(assetsDirectory, "FolderImageUpdated");
            string subDirDirectory = Path.Combine(assetsDirectory, "FolderSubDir");
            string subSubDirDirectory = Path.Combine(subDirDirectory, "FolderSubSubDir");

            Directory.CreateDirectory(imageDeletedDirectory);
            Directory.CreateDirectory(imageUpdatedDirectory);
            Directory.CreateDirectory(subDirDirectory);
            Directory.CreateDirectory(subSubDirDirectory);

            string imagePath1 = Path.Combine(_dataDirectory!, "Image_11.heic");
            string imagePath2 = Path.Combine(_dataDirectory!, "Image 9.png");
            string imagePath3 = Path.Combine(_dataDirectory!, "Image 1.jpg");
            string imagePath4 = Path.Combine(_dataDirectory!, "Homer.gif");
            string videoPath1 = Path.Combine(_dataDirectory!, "Homer.mp4");

            string imagePath1ToCopy = Path.Combine(assetsDirectory, "Image_11.heic");
            string imagePath2ToCopy = Path.Combine(imageDeletedDirectory, "Image 9.png");
            string imagePath3ToCopy = Path.Combine(imageUpdatedDirectory, "Image 1.jpg");
            string imagePath4ToCopy = Path.Combine(subDirDirectory, "Homer.gif");
            string videoPath1ToCopy = Path.Combine(assetsDirectory, "Homer.mp4");
            string videoPath2ToCopy = Path.Combine(subSubDirDirectory, "HomerDuplicated.mp4");

            string firstFrameVideosDirectory = _userConfigurationService!.PathSettings.FirstFrameVideosPath;
            string firstFramePath1 = Path.Combine(firstFrameVideosDirectory, "Homer.jpg");
            string firstFramePath2 = Path.Combine(firstFrameVideosDirectory, "HomerDuplicated.jpg");

            File.Copy(imagePath1, imagePath1ToCopy);
            File.Copy(imagePath2, imagePath2ToCopy);
            File.Copy(imagePath3, imagePath3ToCopy);
            File.Copy(imagePath4, imagePath4ToCopy);
            File.Copy(videoPath1, videoPath1ToCopy);
            File.Copy(videoPath1, videoPath2ToCopy);

            List<string> assetPaths = [imagePath1ToCopy, imagePath2ToCopy, imagePath3ToCopy, imagePath4ToCopy, videoPath1ToCopy, videoPath2ToCopy];
            List<string> assetPathsAfterSync = [imagePath1ToCopy, imagePath2ToCopy, imagePath3ToCopy, imagePath4ToCopy, firstFramePath1, firstFramePath2];

            List<Asset> expectedAssetsFirstSync = [_asset4!, _asset2!, _asset2Temp!, _asset3Temp!, _asset4Temp!, _asset5Temp!];
            List<Asset> expectedAssetsSecondSync = [_asset4!, _asset3Temp!, _asset4Temp!, _asset5Temp!, _asset2Temp!];

            List<int> assetsImageByteSizeFirstSync = [
                ASSET4_IMAGE_BYTE_SIZE,
                ASSET2_IMAGE_BYTE_SIZE,
                ASSET2_TEMP_IMAGE_BYTE_SIZE,
                ASSET3_TEMP_IMAGE_BYTE_SIZE,
                ASSET4_TEMP_IMAGE_BYTE_SIZE,
                ASSET5_TEMP_IMAGE_BYTE_SIZE];
            List<int> assetsImageByteSizeSecondSync = [
                ASSET4_IMAGE_BYTE_SIZE,
                ASSET3_TEMP_IMAGE_BYTE_SIZE,
                ASSET4_TEMP_IMAGE_BYTE_SIZE,
                ASSET5_TEMP_IMAGE_BYTE_SIZE,
                ASSET2_TEMP_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(2, assetsInDirectory.Length);
            assetsInDirectory = Directory.GetFiles(imageDeletedDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);
            assetsInDirectory = Directory.GetFiles(imageUpdatedDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);
            assetsInDirectory = Directory.GetFiles(subDirDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);
            assetsInDirectory = Directory.GetFiles(subSubDirDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);

            foreach (string assetPath in assetPaths)
            {
                Assert.IsTrue(File.Exists(assetPath));
            }

            Assert.IsFalse(File.Exists(firstFramePath1));
            Assert.IsFalse(File.Exists(firstFramePath2));

            Folder? rootFolder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNull(rootFolder);

            Folder? imageDeletedFolder = _testableAssetRepository!.GetFolderByPath(imageDeletedDirectory);
            Assert.IsNull(imageDeletedFolder);

            Folder? imageUpdatedFolder = _testableAssetRepository!.GetFolderByPath(imageUpdatedDirectory);
            Assert.IsNull(imageUpdatedFolder);

            Folder? subDirFolder = _testableAssetRepository!.GetFolderByPath(subDirDirectory);
            Assert.IsNull(subDirFolder);

            Folder? subSubDirFolder = _testableAssetRepository!.GetFolderByPath(subSubDirDirectory);
            Assert.IsNull(subSubDirFolder);

            Folder? videoFirstFramefolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.IsNull(videoFirstFramefolder);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CheckBackupBefore(backupFilePath);

            List<Asset> assetsInRootFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsInRootFromRepositoryByPath);

            List<Asset> assetsInImageDeletedFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(imageDeletedDirectory);
            Assert.IsEmpty(assetsInImageDeletedFolderFromRepositoryByPath);

            List<Asset> assetsInImageUpdatedFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(imageUpdatedDirectory);
            Assert.IsEmpty(assetsInImageUpdatedFolderFromRepositoryByPath);

            List<Asset> assetsInSubDirFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(subDirDirectory);
            Assert.IsEmpty(assetsInSubDirFolderFromRepositoryByPath);

            List<Asset> assetsInSubSubDirFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(subSubDirDirectory);
            Assert.IsEmpty(assetsInSubSubDirFolderFromRepositoryByPath);

            List<Asset> videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.IsEmpty(videoFirstFramesFromRepositoryByPath);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _catalogAssetsService!.CatalogAssetsAsync(catalogChanges.Add);

            rootFolder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(rootFolder);

            imageDeletedFolder = _testableAssetRepository!.GetFolderByPath(imageDeletedDirectory);
            Assert.IsNotNull(imageDeletedFolder);

            imageUpdatedFolder = _testableAssetRepository!.GetFolderByPath(imageUpdatedDirectory);
            Assert.IsNotNull(imageUpdatedFolder);

            subDirFolder = _testableAssetRepository!.GetFolderByPath(subDirDirectory);
            Assert.IsNotNull(subDirFolder);

            subSubDirFolder = _testableAssetRepository!.GetFolderByPath(subSubDirDirectory);
            Assert.IsNotNull(subSubDirFolder);

            videoFirstFramefolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.IsNotNull(videoFirstFramefolder);

            assetsInDirectory = Directory.GetFiles(firstFrameVideosDirectory);
            Assert.AreEqual(2, assetsInDirectory.Length);
            Assert.IsTrue(File.Exists(firstFramePath1));
            Assert.IsTrue(File.Exists(firstFramePath2));

            _asset4!.Folder = rootFolder!;
            _asset4!.FolderId = rootFolder!.FolderId;
            _asset2!.Folder = imageDeletedFolder!;
            _asset2!.FolderId = imageDeletedFolder!.FolderId;
            _asset2Temp!.Folder = imageUpdatedFolder!;
            _asset2Temp!.FolderId = imageUpdatedFolder!.FolderId;
            _asset3Temp!.Folder = subDirFolder!;
            _asset3Temp!.FolderId = subDirFolder!.FolderId;
            _asset4Temp!.Folder = videoFirstFramefolder!;
            _asset4Temp!.FolderId = videoFirstFramefolder!.FolderId;
            _asset5Temp!.Folder = videoFirstFramefolder;
            _asset5Temp!.FolderId = videoFirstFramefolder.FolderId;

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsInRootFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(1, assetsInRootFromRepositoryByPath.Count);

            assetsInImageDeletedFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(imageDeletedDirectory);
            Assert.AreEqual(1, assetsInImageDeletedFolderFromRepositoryByPath.Count);

            assetsInImageUpdatedFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(imageUpdatedDirectory);
            Assert.AreEqual(1, assetsInImageUpdatedFolderFromRepositoryByPath.Count);

            assetsInSubDirFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(subDirDirectory);
            Assert.AreEqual(1, assetsInSubDirFolderFromRepositoryByPath.Count);

            assetsInSubSubDirFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(subSubDirDirectory);
            Assert.AreEqual(0, assetsInSubSubDirFolderFromRepositoryByPath.Count);

            videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.AreEqual(2, videoFirstFramesFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(6, assetsFromRepository.Count);

            List<Folder> expectedFolders = [rootFolder, imageDeletedFolder, imageUpdatedFolder, subDirFolder, videoFirstFramefolder, videoFirstFramefolder];
            List<string> expectedDirectories = [assetsDirectory, imageDeletedDirectory, imageUpdatedDirectory, subDirDirectory, firstFrameVideosDirectory, firstFrameVideosDirectory];

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssetsFirstSync[i], assetPathsAfterSync[i], expectedDirectories[i], expectedFolders[i]);
            }

            List<Folder> folders = [rootFolder, imageDeletedFolder, imageUpdatedFolder, subDirFolder, subSubDirFolder!, videoFirstFramefolder];
            List<Folder> foldersContainingAssets = [rootFolder, imageDeletedFolder, imageUpdatedFolder, subDirFolder, videoFirstFramefolder];
            Dictionary<Folder, List<Asset>> folderToAssetsMappingFirstSync = new()
            {
                { rootFolder, [_asset4!]},
                { imageDeletedFolder, [_asset2!]},
                { imageUpdatedFolder, [_asset2Temp!]},
                { subDirFolder, [_asset3Temp!]},
                { videoFirstFramefolder, [_asset4Temp!, _asset5Temp!]}
            };
            Dictionary<string, int> assetNameToByteSizeMappingFirstSync = new()
            {
                { _asset4!.FileName, ASSET4_IMAGE_BYTE_SIZE },
                { _asset2!.FileName, ASSET2_IMAGE_BYTE_SIZE },
                { _asset2Temp!.FileName, ASSET2_TEMP_IMAGE_BYTE_SIZE },
                { _asset3Temp!.FileName, ASSET3_TEMP_IMAGE_BYTE_SIZE },
                { _asset4Temp!.FileName, ASSET4_TEMP_IMAGE_BYTE_SIZE },
                { _asset5Temp!.FileName, ASSET5_TEMP_IMAGE_BYTE_SIZE }
            };

            AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMappingFirstSync, foldersContainingAssets, thumbnails, assetsImageByteSizeFirstSync);
            CheckBlobsAndTablesAfterSaveCatalog(
                blobsPath,
                tablesPath,
                folders,
                foldersContainingAssets,
                assetsFromRepository,
                folderToAssetsMappingFirstSync,
                assetNameToByteSizeMappingFirstSync);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            CheckBackupAfter(
                backupFilePath,
                blobsPath,
                tablesPath,
                folders,
                foldersContainingAssets,
                assetsFromRepository,
                folderToAssetsMappingFirstSync,
                assetNameToByteSizeMappingFirstSync);

            Assert.AreEqual(21, catalogChanges.Count);

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);
            CheckCatalogChangesAssetAdded(catalogChanges, assetsDirectory, folderToAssetsMappingFirstSync[rootFolder], _asset4!, rootFolder, ref increment);

            CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, imageDeletedDirectory, ref increment);
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, imageDeletedDirectory, ref increment);
            CheckCatalogChangesAssetAdded(catalogChanges, imageDeletedDirectory, folderToAssetsMappingFirstSync[imageDeletedFolder], _asset2!, imageDeletedFolder, ref increment);

            CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, imageUpdatedDirectory, ref increment);
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, imageUpdatedDirectory, ref increment);
            CheckCatalogChangesAssetAdded(catalogChanges, imageUpdatedDirectory, folderToAssetsMappingFirstSync[imageUpdatedFolder], _asset2Temp!, imageUpdatedFolder, ref increment);

            CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, subDirDirectory, ref increment);
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, subDirDirectory, ref increment);
            CheckCatalogChangesAssetAdded(catalogChanges, subDirDirectory, folderToAssetsMappingFirstSync[subDirFolder], _asset3Temp!, subDirFolder, ref increment);

            CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, subSubDirDirectory, ref increment);
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, subSubDirDirectory, ref increment);

            CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);

            foreach (Asset expectedAsset in folderToAssetsMappingFirstSync[videoFirstFramefolder])
            {
                CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    firstFrameVideosDirectory,
                    folderToAssetsMappingFirstSync[videoFirstFramefolder],
                    expectedAsset,
                    videoFirstFramefolder,
                    ref increment);
            }

            CheckCatalogChangesBackup(catalogChanges, CREATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);
            
            // Second Sync

            File.Delete(imagePath2ToCopy);

            _asset2Temp!.ThumbnailCreationDateTime = DateTime.Now.AddDays(10);
            File.SetLastWriteTime(imagePath3ToCopy, _asset2Temp.ThumbnailCreationDateTime);

            Dictionary<Folder, List<Asset>> folderToAssetsMappingSecondSync = new()
            {
                { rootFolder, [_asset4!]},
                { subDirFolder, [_asset3Temp!]},
                { videoFirstFramefolder, [_asset4Temp!, _asset5Temp!]},
                { imageUpdatedFolder, [_asset2Temp!]}
            };
            Dictionary<string, int> assetNameToByteSizeMappingSecondSync = new()
            {
                { _asset4!.FileName, ASSET4_IMAGE_BYTE_SIZE },
                { _asset3Temp!.FileName, ASSET3_TEMP_IMAGE_BYTE_SIZE },
                { _asset4Temp!.FileName, ASSET4_TEMP_IMAGE_BYTE_SIZE },
                { _asset5Temp!.FileName, ASSET5_TEMP_IMAGE_BYTE_SIZE },
                { _asset2Temp!.FileName, ASSET2_TEMP_IMAGE_BYTE_SIZE }
            };

            assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(2, assetsInDirectory.Length);
            assetsInDirectory = Directory.GetFiles(imageDeletedDirectory);
            Assert.AreEqual(0, assetsInDirectory.Length);
            assetsInDirectory = Directory.GetFiles(imageUpdatedDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);
            assetsInDirectory = Directory.GetFiles(subDirDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);
            assetsInDirectory = Directory.GetFiles(subSubDirDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);
            assetsInDirectory = Directory.GetFiles(firstFrameVideosDirectory);
            Assert.AreEqual(2, assetsInDirectory.Length);

            await _catalogAssetsService!.CatalogAssetsAsync(catalogChanges.Add);

            rootFolder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(rootFolder);

            imageDeletedFolder = _testableAssetRepository!.GetFolderByPath(imageDeletedDirectory);
            Assert.IsNotNull(imageDeletedFolder);

            imageUpdatedFolder = _testableAssetRepository!.GetFolderByPath(imageUpdatedDirectory);
            Assert.IsNotNull(imageUpdatedFolder);

            subDirFolder = _testableAssetRepository!.GetFolderByPath(subDirDirectory);
            Assert.IsNotNull(subDirFolder);

            subSubDirFolder = _testableAssetRepository!.GetFolderByPath(subSubDirDirectory);
            Assert.IsNotNull(subSubDirFolder);

            videoFirstFramefolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.IsNotNull(videoFirstFramefolder);

            assetsInDirectory = Directory.GetFiles(firstFrameVideosDirectory);
            Assert.AreEqual(2, assetsInDirectory.Length);
            Assert.IsTrue(File.Exists(firstFramePath1));
            Assert.IsTrue(File.Exists(firstFramePath2));

            _asset4!.Folder = rootFolder!;
            _asset4!.FolderId = rootFolder!.FolderId;
            _asset2Temp!.Folder = imageUpdatedFolder!;
            _asset2Temp!.FolderId = imageUpdatedFolder!.FolderId;
            _asset3Temp!.Folder = subDirFolder!;
            _asset3Temp!.FolderId = subDirFolder!.FolderId;
            _asset4Temp!.Folder = videoFirstFramefolder!;
            _asset4Temp!.FolderId = videoFirstFramefolder!.FolderId;
            _asset5Temp!.Folder = videoFirstFramefolder;
            _asset5Temp!.FolderId = videoFirstFramefolder.FolderId;

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsInRootFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(1, assetsInRootFromRepositoryByPath.Count);

            assetsInImageDeletedFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(imageDeletedDirectory);
            Assert.IsEmpty(assetsInImageDeletedFolderFromRepositoryByPath);

            assetsInImageUpdatedFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(imageUpdatedDirectory);
            Assert.AreEqual(1, assetsInImageUpdatedFolderFromRepositoryByPath.Count);

            assetsInSubDirFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(subDirDirectory);
            Assert.AreEqual(1, assetsInSubDirFolderFromRepositoryByPath.Count);

            assetsInSubSubDirFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(subSubDirDirectory);
            Assert.AreEqual(0, assetsInSubSubDirFolderFromRepositoryByPath.Count);

            videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.AreEqual(2, videoFirstFramesFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(5, assetsFromRepository.Count);

            assetPathsAfterSync = [imagePath1ToCopy, imagePath4ToCopy, firstFramePath1, firstFramePath2, imagePath3ToCopy];
            expectedFolders = [rootFolder, subDirFolder, videoFirstFramefolder, videoFirstFramefolder, imageUpdatedFolder];
            expectedDirectories = [assetsDirectory, subDirDirectory, firstFrameVideosDirectory, firstFrameVideosDirectory, imageUpdatedDirectory];

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssetsSecondSync[i], assetPathsAfterSync[i], expectedDirectories[i], expectedFolders[i]);
            }

            AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMappingSecondSync, foldersContainingAssets, thumbnails, assetsImageByteSizeSecondSync);
            CheckBlobsAndTablesAfterSaveCatalog(
                blobsPath,
                tablesPath,
                folders,
                foldersContainingAssets,
                assetsFromRepository,
                folderToAssetsMappingSecondSync,
                assetNameToByteSizeMappingSecondSync);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            CheckBackupAfter(
                backupFilePath,
                blobsPath,
                tablesPath,
                folders,
                foldersContainingAssets,
                assetsFromRepository,
                folderToAssetsMappingSecondSync,
                assetNameToByteSizeMappingSecondSync);

            Assert.AreEqual(33, catalogChanges.Count);

            increment = 0;

            foldersInRepository = _testableAssetRepository!.GetFolders();

            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);
            CheckCatalogChangesAssetAdded(catalogChanges, assetsDirectory, folderToAssetsMappingFirstSync[rootFolder], _asset4!, rootFolder, ref increment);

            CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, imageDeletedDirectory, ref increment);
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, imageDeletedDirectory, ref increment);
            CheckCatalogChangesAssetAdded(catalogChanges, imageDeletedDirectory, folderToAssetsMappingFirstSync[imageDeletedFolder!], _asset2!, imageDeletedFolder!, ref increment);

            CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, imageUpdatedDirectory, ref increment);
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, imageUpdatedDirectory, ref increment);
            CheckCatalogChangesAssetAdded(catalogChanges, imageUpdatedDirectory, folderToAssetsMappingFirstSync[imageUpdatedFolder], _asset2Temp!, imageUpdatedFolder, ref increment);

            CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, subDirDirectory, ref increment);
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, subDirDirectory, ref increment);
            CheckCatalogChangesAssetAdded(catalogChanges, subDirDirectory, folderToAssetsMappingFirstSync[subDirFolder], _asset3Temp!, subDirFolder, ref increment);

            CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, subSubDirDirectory, ref increment);
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, subSubDirDirectory, ref increment);

            CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);

            foreach (Asset expectedAsset in folderToAssetsMappingFirstSync[videoFirstFramefolder])
            {
                CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    firstFrameVideosDirectory,
                    folderToAssetsMappingFirstSync[videoFirstFramefolder],
                    expectedAsset,
                    videoFirstFramefolder,
                    ref increment);
            }

            CheckCatalogChangesBackup(catalogChanges, CREATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);

            // Second part (second sync)
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, imageDeletedDirectory, ref increment);
            CheckCatalogChangesAssetDeleted(catalogChanges, imageDeletedDirectory, [], _asset2!, imageDeletedFolder!, ref increment);
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, imageUpdatedDirectory, ref increment);
            CheckCatalogChangesAssetUpdated(catalogChanges, imageUpdatedDirectory, folderToAssetsMappingSecondSync[imageUpdatedFolder], _asset2Temp!, imageUpdatedFolder, ref increment);
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, subDirDirectory, ref increment);
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, subSubDirDirectory, ref increment);
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            CheckCatalogChangesBackup(catalogChanges, UPDATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(assetsDirectory, true);
        }
    }

    // TODO: Actually, video with same name are considered the same, need to evolve this
    [Test]
    public async Task CatalogAssetsAsync_AssetsImageAndSameVideosAndRootCatalogFolderExistsAndSubDirAndUpdateAndDelete_SyncTheAssetsButNotTheVideoInSubdirectory()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "TempAssetsDirectory");

        ConfigureCatalogAssetService(100, assetsDirectory, 200, 150, false, false, false, true);

        try
        {
            Directory.CreateDirectory(assetsDirectory);

            string imageDeletedDirectory = Path.Combine(assetsDirectory, "FolderImageDeleted");
            string imageUpdatedDirectory = Path.Combine(assetsDirectory, "FolderImageUpdated");
            string subDirDirectory = Path.Combine(assetsDirectory, "FolderSubDir");
            string subSubDirDirectory = Path.Combine(subDirDirectory, "FolderSubSubDir");

            Directory.CreateDirectory(imageDeletedDirectory);
            Directory.CreateDirectory(imageUpdatedDirectory);
            Directory.CreateDirectory(subDirDirectory);
            Directory.CreateDirectory(subSubDirDirectory);

            string imagePath1 = Path.Combine(_dataDirectory!, "Image_11.heic");
            string imagePath2 = Path.Combine(_dataDirectory!, "Image 9.png");
            string imagePath3 = Path.Combine(_dataDirectory!, "Image 1.jpg");
            string imagePath4 = Path.Combine(_dataDirectory!, "Homer.gif");
            string videoPath1 = Path.Combine(_dataDirectory!, "Homer.mp4");

            string imagePath1ToCopy = Path.Combine(assetsDirectory, "Image_11.heic");
            string imagePath2ToCopy = Path.Combine(imageDeletedDirectory, "Image 9.png");
            string imagePath3ToCopy = Path.Combine(imageUpdatedDirectory, "Image 1.jpg");
            string imagePath4ToCopy = Path.Combine(subDirDirectory, "Homer.gif");
            string videoPath1ToCopy = Path.Combine(assetsDirectory, "Homer.mp4");
            string videoPath2ToCopy = Path.Combine(subSubDirDirectory, "Homer.mp4");

            string firstFrameVideosDirectory = _userConfigurationService!.PathSettings.FirstFrameVideosPath;
            string firstFramePath1 = Path.Combine(firstFrameVideosDirectory, "Homer.jpg");

            File.Copy(imagePath1, imagePath1ToCopy);
            File.Copy(imagePath2, imagePath2ToCopy);
            File.Copy(imagePath3, imagePath3ToCopy);
            File.Copy(imagePath4, imagePath4ToCopy);
            File.Copy(videoPath1, videoPath1ToCopy);
            File.Copy(videoPath1, videoPath2ToCopy);

            List<string> assetPaths = [imagePath1ToCopy, imagePath2ToCopy, imagePath3ToCopy, imagePath4ToCopy, videoPath1ToCopy, videoPath2ToCopy];
            List<string> assetPathsAfterSync = [imagePath1ToCopy, imagePath2ToCopy, imagePath3ToCopy, imagePath4ToCopy, firstFramePath1];

            List<Asset> expectedAssetsFirstSync = [_asset4!, _asset2!, _asset2Temp!, _asset3Temp!, _asset4Temp!];
            List<Asset> expectedAssetsSecondSync = [_asset4!, _asset3Temp!, _asset4Temp!, _asset2Temp!];

            List<int> assetsImageByteSizeFirstSync = [
                ASSET4_IMAGE_BYTE_SIZE,
                ASSET2_IMAGE_BYTE_SIZE,
                ASSET2_TEMP_IMAGE_BYTE_SIZE,
                ASSET3_TEMP_IMAGE_BYTE_SIZE,
                ASSET4_TEMP_IMAGE_BYTE_SIZE];
            List<int> assetsImageByteSizeSecondSync = [
                ASSET4_IMAGE_BYTE_SIZE,
                ASSET3_TEMP_IMAGE_BYTE_SIZE,
                ASSET4_TEMP_IMAGE_BYTE_SIZE,
                ASSET2_TEMP_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(2, assetsInDirectory.Length);
            assetsInDirectory = Directory.GetFiles(imageDeletedDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);
            assetsInDirectory = Directory.GetFiles(imageUpdatedDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);
            assetsInDirectory = Directory.GetFiles(subDirDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);
            assetsInDirectory = Directory.GetFiles(subSubDirDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);

            foreach (string assetPath in assetPaths)
            {
                Assert.IsTrue(File.Exists(assetPath));
            }

            Assert.IsFalse(File.Exists(firstFramePath1));

            Folder? rootFolder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNull(rootFolder);

            Folder? imageDeletedFolder = _testableAssetRepository!.GetFolderByPath(imageDeletedDirectory);
            Assert.IsNull(imageDeletedFolder);

            Folder? imageUpdatedFolder = _testableAssetRepository!.GetFolderByPath(imageUpdatedDirectory);
            Assert.IsNull(imageUpdatedFolder);

            Folder? subDirFolder = _testableAssetRepository!.GetFolderByPath(subDirDirectory);
            Assert.IsNull(subDirFolder);

            Folder? subSubDirFolder = _testableAssetRepository!.GetFolderByPath(subSubDirDirectory);
            Assert.IsNull(subSubDirFolder);

            Folder? videoFirstFramefolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.IsNull(videoFirstFramefolder);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CheckBackupBefore(backupFilePath);

            List<Asset> assetsInRootFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsInRootFromRepositoryByPath);

            List<Asset> assetsInImageDeletedFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(imageDeletedDirectory);
            Assert.IsEmpty(assetsInImageDeletedFolderFromRepositoryByPath);

            List<Asset> assetsInImageUpdatedFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(imageUpdatedDirectory);
            Assert.IsEmpty(assetsInImageUpdatedFolderFromRepositoryByPath);

            List<Asset> assetsInSubDirFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(subDirDirectory);
            Assert.IsEmpty(assetsInSubDirFolderFromRepositoryByPath);

            List<Asset> assetsInSubSubDirFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(subSubDirDirectory);
            Assert.IsEmpty(assetsInSubSubDirFolderFromRepositoryByPath);

            List<Asset> videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.IsEmpty(videoFirstFramesFromRepositoryByPath);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _catalogAssetsService!.CatalogAssetsAsync(catalogChanges.Add);

            rootFolder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(rootFolder);

            imageDeletedFolder = _testableAssetRepository!.GetFolderByPath(imageDeletedDirectory);
            Assert.IsNotNull(imageDeletedFolder);

            imageUpdatedFolder = _testableAssetRepository!.GetFolderByPath(imageUpdatedDirectory);
            Assert.IsNotNull(imageUpdatedFolder);

            subDirFolder = _testableAssetRepository!.GetFolderByPath(subDirDirectory);
            Assert.IsNotNull(subDirFolder);

            subSubDirFolder = _testableAssetRepository!.GetFolderByPath(subSubDirDirectory);
            Assert.IsNotNull(subSubDirFolder);

            videoFirstFramefolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.IsNotNull(videoFirstFramefolder);

            assetsInDirectory = Directory.GetFiles(firstFrameVideosDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);
            Assert.IsTrue(File.Exists(firstFramePath1));

            _asset4!.Folder = rootFolder!;
            _asset4!.FolderId = rootFolder!.FolderId;
            _asset2!.Folder = imageDeletedFolder!;
            _asset2!.FolderId = imageDeletedFolder!.FolderId;
            _asset2Temp!.Folder = imageUpdatedFolder!;
            _asset2Temp!.FolderId = imageUpdatedFolder!.FolderId;
            _asset3Temp!.Folder = subDirFolder!;
            _asset3Temp!.FolderId = subDirFolder!.FolderId;
            _asset4Temp!.Folder = videoFirstFramefolder!;
            _asset4Temp!.FolderId = videoFirstFramefolder!.FolderId;

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsInRootFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(1, assetsInRootFromRepositoryByPath.Count);

            assetsInImageDeletedFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(imageDeletedDirectory);
            Assert.AreEqual(1, assetsInImageDeletedFolderFromRepositoryByPath.Count);

            assetsInImageUpdatedFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(imageUpdatedDirectory);
            Assert.AreEqual(1, assetsInImageUpdatedFolderFromRepositoryByPath.Count);

            assetsInSubDirFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(subDirDirectory);
            Assert.AreEqual(1, assetsInSubDirFolderFromRepositoryByPath.Count);

            assetsInSubSubDirFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(subSubDirDirectory);
            Assert.AreEqual(0, assetsInSubSubDirFolderFromRepositoryByPath.Count);

            videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.AreEqual(1, videoFirstFramesFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(5, assetsFromRepository.Count);

            List<Folder> expectedFolders = [rootFolder, imageDeletedFolder, imageUpdatedFolder, subDirFolder, videoFirstFramefolder];
            List<string> expectedDirectories = [assetsDirectory, imageDeletedDirectory, imageUpdatedDirectory, subDirDirectory, firstFrameVideosDirectory];

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssetsFirstSync[i], assetPathsAfterSync[i], expectedDirectories[i], expectedFolders[i]);
            }

            List<Folder> folders = [rootFolder, imageDeletedFolder, imageUpdatedFolder, subDirFolder, subSubDirFolder!, videoFirstFramefolder];
            List<Folder> foldersContainingAssets = [rootFolder, imageDeletedFolder, imageUpdatedFolder, subDirFolder, videoFirstFramefolder];
            Dictionary<Folder, List<Asset>> folderToAssetsMappingFirstSync = new()
            {
                { rootFolder, [_asset4!]},
                { imageDeletedFolder, [_asset2!]},
                { imageUpdatedFolder, [_asset2Temp!]},
                { subDirFolder, [_asset3Temp!]},
                { videoFirstFramefolder, [_asset4Temp!]}
            };
            Dictionary<string, int> assetNameToByteSizeMappingFirstSync = new()
            {
                { _asset4!.FileName, ASSET4_IMAGE_BYTE_SIZE },
                { _asset2!.FileName, ASSET2_IMAGE_BYTE_SIZE },
                { _asset2Temp!.FileName, ASSET2_TEMP_IMAGE_BYTE_SIZE },
                { _asset3Temp!.FileName, ASSET3_TEMP_IMAGE_BYTE_SIZE },
                { _asset4Temp!.FileName, ASSET4_TEMP_IMAGE_BYTE_SIZE }
            };

            AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMappingFirstSync, foldersContainingAssets, thumbnails, assetsImageByteSizeFirstSync);
            CheckBlobsAndTablesAfterSaveCatalog(
                blobsPath,
                tablesPath,
                folders,
                foldersContainingAssets,
                assetsFromRepository,
                folderToAssetsMappingFirstSync,
                assetNameToByteSizeMappingFirstSync);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            CheckBackupAfter(
                backupFilePath,
                blobsPath,
                tablesPath,
                folders,
                foldersContainingAssets,
                assetsFromRepository,
                folderToAssetsMappingFirstSync,
                assetNameToByteSizeMappingFirstSync);

            Assert.AreEqual(20, catalogChanges.Count);

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);
            CheckCatalogChangesAssetAdded(catalogChanges, assetsDirectory, folderToAssetsMappingFirstSync[rootFolder], _asset4!, rootFolder, ref increment);

            CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, imageDeletedDirectory, ref increment);
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, imageDeletedDirectory, ref increment);
            CheckCatalogChangesAssetAdded(catalogChanges, imageDeletedDirectory, folderToAssetsMappingFirstSync[imageDeletedFolder], _asset2!, imageDeletedFolder, ref increment);

            CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, imageUpdatedDirectory, ref increment);
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, imageUpdatedDirectory, ref increment);
            CheckCatalogChangesAssetAdded(catalogChanges, imageUpdatedDirectory, folderToAssetsMappingFirstSync[imageUpdatedFolder], _asset2Temp!, imageUpdatedFolder, ref increment);

            CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, subDirDirectory, ref increment);
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, subDirDirectory, ref increment);
            CheckCatalogChangesAssetAdded(catalogChanges, subDirDirectory, folderToAssetsMappingFirstSync[subDirFolder], _asset3Temp!, subDirFolder, ref increment);

            CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, subSubDirDirectory, ref increment);
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, subSubDirDirectory, ref increment);

            CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            CheckCatalogChangesAssetAdded(
                catalogChanges,
                firstFrameVideosDirectory,
                folderToAssetsMappingFirstSync[videoFirstFramefolder],
                _asset4Temp!,
                videoFirstFramefolder,
                ref increment);

            CheckCatalogChangesBackup(catalogChanges, CREATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);
            
            // Second Sync

            File.Delete(imagePath2ToCopy);

            _asset2Temp!.ThumbnailCreationDateTime = DateTime.Now.AddDays(10);
            File.SetLastWriteTime(imagePath3ToCopy, _asset2Temp.ThumbnailCreationDateTime);

            Dictionary<Folder, List<Asset>> folderToAssetsMappingSecondSync = new()
            {
                { rootFolder, [_asset4!]},
                { subDirFolder, [_asset3Temp!]},
                { videoFirstFramefolder, [_asset4Temp!]},
                { imageUpdatedFolder, [_asset2Temp!]}
            };
            Dictionary<string, int> assetNameToByteSizeMappingSecondSync = new()
            {
                { _asset4!.FileName, ASSET4_IMAGE_BYTE_SIZE },
                { _asset3Temp!.FileName, ASSET3_TEMP_IMAGE_BYTE_SIZE },
                { _asset4Temp!.FileName, ASSET4_TEMP_IMAGE_BYTE_SIZE },
                { _asset2Temp!.FileName, ASSET2_TEMP_IMAGE_BYTE_SIZE }
            };

            assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(2, assetsInDirectory.Length);
            assetsInDirectory = Directory.GetFiles(imageDeletedDirectory);
            Assert.AreEqual(0, assetsInDirectory.Length);
            assetsInDirectory = Directory.GetFiles(imageUpdatedDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);
            assetsInDirectory = Directory.GetFiles(subDirDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);
            assetsInDirectory = Directory.GetFiles(subSubDirDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);
            assetsInDirectory = Directory.GetFiles(firstFrameVideosDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);

            await _catalogAssetsService!.CatalogAssetsAsync(catalogChanges.Add);

            rootFolder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(rootFolder);

            imageDeletedFolder = _testableAssetRepository!.GetFolderByPath(imageDeletedDirectory);
            Assert.IsNotNull(imageDeletedFolder);

            imageUpdatedFolder = _testableAssetRepository!.GetFolderByPath(imageUpdatedDirectory);
            Assert.IsNotNull(imageUpdatedFolder);

            subDirFolder = _testableAssetRepository!.GetFolderByPath(subDirDirectory);
            Assert.IsNotNull(subDirFolder);

            subSubDirFolder = _testableAssetRepository!.GetFolderByPath(subSubDirDirectory);
            Assert.IsNotNull(subSubDirFolder);

            videoFirstFramefolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.IsNotNull(videoFirstFramefolder);

            assetsInDirectory = Directory.GetFiles(firstFrameVideosDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);
            Assert.IsTrue(File.Exists(firstFramePath1));

            _asset4!.Folder = rootFolder!;
            _asset4!.FolderId = rootFolder!.FolderId;
            _asset2Temp!.Folder = imageUpdatedFolder!;
            _asset2Temp!.FolderId = imageUpdatedFolder!.FolderId;
            _asset3Temp!.Folder = subDirFolder!;
            _asset3Temp!.FolderId = subDirFolder!.FolderId;
            _asset4Temp!.Folder = videoFirstFramefolder!;
            _asset4Temp!.FolderId = videoFirstFramefolder!.FolderId;

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsInRootFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(1, assetsInRootFromRepositoryByPath.Count);

            assetsInImageDeletedFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(imageDeletedDirectory);
            Assert.IsEmpty(assetsInImageDeletedFolderFromRepositoryByPath);

            assetsInImageUpdatedFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(imageUpdatedDirectory);
            Assert.AreEqual(1, assetsInImageUpdatedFolderFromRepositoryByPath.Count);

            assetsInSubDirFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(subDirDirectory);
            Assert.AreEqual(1, assetsInSubDirFolderFromRepositoryByPath.Count);

            assetsInSubSubDirFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(subSubDirDirectory);
            Assert.AreEqual(0, assetsInSubSubDirFolderFromRepositoryByPath.Count);

            videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.AreEqual(1, videoFirstFramesFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(4, assetsFromRepository.Count);

            assetPathsAfterSync = [imagePath1ToCopy, imagePath4ToCopy, firstFramePath1, imagePath3ToCopy];
            expectedFolders = [rootFolder, subDirFolder, videoFirstFramefolder, imageUpdatedFolder];
            expectedDirectories = [assetsDirectory, subDirDirectory, firstFrameVideosDirectory, imageUpdatedDirectory];

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssetsSecondSync[i], assetPathsAfterSync[i], expectedDirectories[i], expectedFolders[i]);
            }

            AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMappingSecondSync, foldersContainingAssets, thumbnails, assetsImageByteSizeSecondSync);
            CheckBlobsAndTablesAfterSaveCatalog(
                blobsPath,
                tablesPath,
                folders,
                foldersContainingAssets,
                assetsFromRepository,
                folderToAssetsMappingSecondSync,
                assetNameToByteSizeMappingSecondSync);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            CheckBackupAfter(
                backupFilePath,
                blobsPath,
                tablesPath,
                folders,
                foldersContainingAssets,
                assetsFromRepository,
                folderToAssetsMappingSecondSync,
                assetNameToByteSizeMappingSecondSync);

            Assert.AreEqual(32, catalogChanges.Count);

            increment = 0;

            foldersInRepository = _testableAssetRepository!.GetFolders();

            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);
            CheckCatalogChangesAssetAdded(catalogChanges, assetsDirectory, folderToAssetsMappingFirstSync[rootFolder], _asset4!, rootFolder, ref increment);

            CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, imageDeletedDirectory, ref increment);
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, imageDeletedDirectory, ref increment);
            CheckCatalogChangesAssetAdded(catalogChanges, imageDeletedDirectory, folderToAssetsMappingFirstSync[imageDeletedFolder!], _asset2!, imageDeletedFolder!, ref increment);

            CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, imageUpdatedDirectory, ref increment);
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, imageUpdatedDirectory, ref increment);
            CheckCatalogChangesAssetAdded(catalogChanges, imageUpdatedDirectory, folderToAssetsMappingFirstSync[imageUpdatedFolder], _asset2Temp!, imageUpdatedFolder, ref increment);

            CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, subDirDirectory, ref increment);
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, subDirDirectory, ref increment);
            CheckCatalogChangesAssetAdded(catalogChanges, subDirDirectory, folderToAssetsMappingFirstSync[subDirFolder], _asset3Temp!, subDirFolder, ref increment);

            CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, subSubDirDirectory, ref increment);
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, subSubDirDirectory, ref increment);

            CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            CheckCatalogChangesAssetAdded(
                catalogChanges,
                firstFrameVideosDirectory,
                folderToAssetsMappingFirstSync[videoFirstFramefolder],
                _asset4Temp!,
                videoFirstFramefolder,
                ref increment);

            CheckCatalogChangesBackup(catalogChanges, CREATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);

            // Second part (second sync)
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, imageDeletedDirectory, ref increment);
            CheckCatalogChangesAssetDeleted(catalogChanges, imageDeletedDirectory, [], _asset2!, imageDeletedFolder!, ref increment);
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, imageUpdatedDirectory, ref increment);
            CheckCatalogChangesAssetUpdated(catalogChanges, imageUpdatedDirectory, folderToAssetsMappingSecondSync[imageUpdatedFolder], _asset2Temp!, imageUpdatedFolder, ref increment);
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, subDirDirectory, ref increment);
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, subSubDirDirectory, ref increment);
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            CheckCatalogChangesBackup(catalogChanges, UPDATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(assetsDirectory, true);
        }
    }

    // TODO: Case where FirstFrameFolder is before the other one (like folder is Named zFolder)
    // --------------------------------------------------------------------------------------------------------------------------------------------

    [Test]
    [TestCase(2)]
    [TestCase(100)]
    public async Task CatalogAssetsAsync_NoAssetsAndRootCatalogFolderExists_DoesNothing(int catalogBatchSize)
    {
        ConfigureCatalogAssetService(catalogBatchSize, _defaultAssetsDirectory!, 200, 150, false, false, false, false);

        try
        {
            Directory.CreateDirectory(_defaultAssetsDirectory!);

            string[] assetsInDirectory = Directory.GetFiles(_defaultAssetsDirectory!);
            Assert.IsEmpty(assetsInDirectory);

            Folder? folder = _testableAssetRepository!.GetFolderByPath(_defaultAssetsDirectory!);
            Assert.IsNull(folder);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CheckBackupBefore(backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(_defaultAssetsDirectory!);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _catalogAssetsService!.CatalogAssetsAsync(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(_defaultAssetsDirectory!);
            Assert.IsNotNull(folder);

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(_defaultAssetsDirectory!);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            CheckBlobsAndTablesAfterSaveCatalogEmpty(blobsPath, tablesPath, true, true, folder!);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            CheckDefaultEmptyBackup(backupFilePath, blobsPath, tablesPath, true, true, folder!);

            Assert.AreEqual(5, catalogChanges.Count);

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, _defaultAssetsDirectory!, ref increment);
            CheckCatalogChangesBackup(catalogChanges, CREATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(_defaultAssetsDirectory!, true);
        }
    }

    [Test]
    [TestCase(2)]
    [TestCase(100)]
    public async Task CatalogAssetsAsync_NoAssetsAndRootCatalogFolderDoesNotExist_DoesNothing(int catalogBatchSize)
    {
        ConfigureCatalogAssetService(catalogBatchSize, _defaultAssetsDirectory!, 200, 150, false, false, false, false);

        try
        {
            Folder? folder = _testableAssetRepository!.GetFolderByPath(_defaultAssetsDirectory!);
            Assert.IsNull(folder);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CheckBackupBefore(backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository!.GetCataloguedAssetsByPath(_defaultAssetsDirectory!);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _catalogAssetsService!.CatalogAssetsAsync(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(_defaultAssetsDirectory!);
            Assert.IsNull(folder);

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(_defaultAssetsDirectory!);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            CheckBlobsAndTablesAfterSaveCatalogEmpty(blobsPath, tablesPath, true, false, folder!);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            CheckDefaultEmptyBackup(backupFilePath, blobsPath, tablesPath, true, false, folder!);

            Assert.AreEqual(5, catalogChanges.Count);

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            CheckCatalogChangesFolderDeleted(catalogChanges, 0, foldersInRepository.Length, _defaultAssetsDirectory!, ref increment);
            CheckCatalogChangesBackup(catalogChanges, CREATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public async Task CatalogAssetsAsync_RootCatalogFolderPointsToAFile_DoesNotSyncTheAssets(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Image 1.jpg");

        ConfigureCatalogAssetService(100, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        try
        {
            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNull(folder);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CheckBackupBefore(backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository!.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _catalogAssetsService!.CatalogAssetsAsync(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNull(folder);

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            CheckBlobsAndTablesAfterSaveCatalogEmpty(blobsPath, tablesPath, true, false, folder!);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            CheckDefaultEmptyBackup(backupFilePath, blobsPath, tablesPath, true, false, folder!);

            Assert.AreEqual(5, catalogChanges.Count);

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            CheckCatalogChangesFolderDeleted(catalogChanges, 0, foldersInRepository.Length, assetsDirectory, ref increment);
            CheckCatalogChangesBackup(catalogChanges, CREATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase(0, true)]
    [TestCase(0, false)]
    [TestCase(2, true)]
    [TestCase(2, false)]
    [TestCase(100, true)]
    [TestCase(100, false)]
    public async Task CatalogAssetsAsync_NoAssetsAndRootCatalogExistAndFolderAndIsCancellationRequested_StopsTheSync(int catalogBatchSize, bool folderExists)
    {
        ConfigureCatalogAssetService(catalogBatchSize, _defaultAssetsDirectory!, 200, 150, false, false, false, false);

        try
        {
            if (folderExists)
            {
                Directory.CreateDirectory(_defaultAssetsDirectory!);

                string[] assetsInDirectory = Directory.GetFiles(_defaultAssetsDirectory!);
                Assert.IsEmpty(assetsInDirectory);
            }

            Folder? folder = _testableAssetRepository!.GetFolderByPath(_defaultAssetsDirectory!);
            Assert.IsNull(folder);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CheckBackupBefore(backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(_defaultAssetsDirectory!);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];
            CancellationToken cancellationToken = new (true);

            await _catalogAssetsService!.CatalogAssetsAsync(catalogChanges.Add, cancellationToken);

            folder = _testableAssetRepository!.GetFolderByPath(_defaultAssetsDirectory!);
            Assert.IsNotNull(folder);

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(_defaultAssetsDirectory!);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            CheckBlobsAndTablesAfterSaveCatalogEmpty(blobsPath, tablesPath, false, false, folder!);

            Assert.IsTrue(_testableAssetRepository.HasChanges()); // SaveCatalog has not been done due to the Cancellation

            CheckDefaultEmptyBackup(backupFilePath, blobsPath, tablesPath, false, false, folder!);

            Assert.AreEqual(4, catalogChanges.Count);

            int increment = 0;

            CheckCatalogChangesBackup(catalogChanges, CREATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);

            if (folderExists)
            {
                Directory.Delete(_defaultAssetsDirectory!, true);
            }
        }
    }

    // BACKUP SECTION ------------------------------------------------------------------------------------------------
    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public async Task CatalogAssetsAsync_AssetsAndRootCatalogFolderExistsAndBackupExistsAndSameContent_SyncTheAssets(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder2");

        ConfigureCatalogAssetService(100, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        try
        {
            string imagePath1 = Path.Combine(assetsDirectory, "Image 1_duplicate.jpg");
            string imagePath2 = Path.Combine(assetsDirectory, "Image 9.png");
            string imagePath3 = Path.Combine(assetsDirectory, "Image 9_duplicate.png");
            string imagePath4 = Path.Combine(assetsDirectory, "Image_11.heic");

            List<string> assetPaths = [imagePath1, imagePath2, imagePath3, imagePath4];
            List<Asset> expectedAssets = [_asset1!, _asset2!, _asset3!, _asset4!];
            List<int> assetsImageByteSize = [ASSET1_IMAGE_BYTE_SIZE, ASSET2_IMAGE_BYTE_SIZE, ASSET3_IMAGE_BYTE_SIZE, ASSET4_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(4, assetsInDirectory.Length);

            foreach (string assetPath in assetPaths)
            {
                Assert.IsTrue(File.Exists(assetPath));
            }

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNull(folder);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CheckBackupBefore(backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _catalogAssetsService!.CatalogAssetsAsync(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            _asset1!.Folder = folder!;
            _asset1!.FolderId = folder!.FolderId;
            _asset2!.Folder = folder;
            _asset2!.FolderId = folder.FolderId;
            _asset3!.Folder = folder;
            _asset3!.FolderId = folder.FolderId;
            _asset4!.Folder = folder;
            _asset4!.FolderId = folder.FolderId;

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(4, assetsFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(4, assetsFromRepository.Count);

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssets[i], assetPaths[i], assetsDirectory, folder);
            }

            Dictionary<Folder, List<Asset>> folderToAssetsMapping = new() { { folder, expectedAssets } };
            Dictionary<string, int> assetNameToByteSizeMapping = new()
            {
                { _asset1!.FileName, ASSET1_IMAGE_BYTE_SIZE },
                { _asset2!.FileName, ASSET2_IMAGE_BYTE_SIZE },
                { _asset3!.FileName, ASSET3_IMAGE_BYTE_SIZE },
                { _asset4!.FileName, ASSET4_IMAGE_BYTE_SIZE }
            };

            AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMapping, [folder], thumbnails, assetsImageByteSize);
            CheckBlobsAndTablesAfterSaveCatalog(
                blobsPath,
                tablesPath,
                [folder],
                [folder],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            CheckBackupAfter(
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder],
                [folder],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.AreEqual(9, catalogChanges.Count);

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            foreach (Asset expectedAsset in folderToAssetsMapping[folder])
            {
                CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder],
                    expectedAsset,
                    folder,
                    ref increment);
            }

            CheckCatalogChangesBackup(catalogChanges, CREATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);

            // Second sync

            await _catalogAssetsService!.CatalogAssetsAsync(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(4, assetsFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(4, assetsFromRepository.Count);

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssets[i], assetPaths[i], assetsDirectory, folder!);
            }

            AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMapping, [folder!], thumbnails, assetsImageByteSize);
            CheckBlobsAndTablesAfterSaveCatalog(
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            CheckBackupAfter(
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.AreEqual(12, catalogChanges.Count);

            increment = 0;

            foldersInRepository = _testableAssetRepository!.GetFolders();

            CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            foreach (Asset expectedAsset in folderToAssetsMapping[folder!])
            {
                CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder!],
                    expectedAsset,
                    folder!,
                    ref increment);
            }

            CheckCatalogChangesBackup(catalogChanges, CREATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);

            CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            CheckCatalogChangesEnd(catalogChanges, ref increment);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public async Task CatalogAssetsAsync_AssetsAndRootCatalogFolderExistsAndBackupExistsAndOneNewAsset_SyncTheAssets(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder2");
        string destinationFilePathToCopy = Path.Combine(assetsDirectory, _asset1Temp!.FileName);

        ConfigureCatalogAssetService(100, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        try
        {
            string imagePath1 = Path.Combine(assetsDirectory, "Image 1_duplicate.jpg");
            string imagePath2 = Path.Combine(assetsDirectory, "Image 9.png");
            string imagePath3 = Path.Combine(assetsDirectory, "Image 9_duplicate.png");
            string imagePath4 = Path.Combine(assetsDirectory, "Image_11.heic");

            List<string> assetPaths = [imagePath1, imagePath2, imagePath3, imagePath4];
            List<Asset> expectedAssets = [_asset1!, _asset2!, _asset3!, _asset4!];
            List<int> assetsImageByteSize = [ASSET1_IMAGE_BYTE_SIZE, ASSET2_IMAGE_BYTE_SIZE, ASSET3_IMAGE_BYTE_SIZE, ASSET4_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(4, assetsInDirectory.Length);

            foreach (string assetPath in assetPaths)
            {
                Assert.IsTrue(File.Exists(assetPath));
            }

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNull(folder);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CheckBackupBefore(backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _catalogAssetsService!.CatalogAssetsAsync(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            _asset1!.Folder = folder!;
            _asset1!.FolderId = folder!.FolderId;
            _asset2!.Folder = folder;
            _asset2!.FolderId = folder.FolderId;
            _asset3!.Folder = folder;
            _asset3!.FolderId = folder.FolderId;
            _asset4!.Folder = folder;
            _asset4!.FolderId = folder.FolderId;

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(4, assetsFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(4, assetsFromRepository.Count);

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssets[i], assetPaths[i], assetsDirectory, folder);
            }

            Dictionary<Folder, List<Asset>> folderToAssetsMapping = new() { { folder, expectedAssets } };
            Dictionary<string, int> assetNameToByteSizeMapping = new()
            {
                { _asset1!.FileName, ASSET1_IMAGE_BYTE_SIZE },
                { _asset2!.FileName, ASSET2_IMAGE_BYTE_SIZE },
                { _asset3!.FileName, ASSET3_IMAGE_BYTE_SIZE },
                { _asset4!.FileName, ASSET4_IMAGE_BYTE_SIZE }
            };

            AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMapping, [folder], thumbnails, assetsImageByteSize);
            CheckBlobsAndTablesAfterSaveCatalog(
                blobsPath,
                tablesPath,
                [folder],
                [folder],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            CheckBackupAfter(
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder],
                [folder],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.AreEqual(9, catalogChanges.Count);

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            foreach (Asset expectedAsset in folderToAssetsMapping[folder])
            {
                CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder],
                    expectedAsset,
                    folder,
                    ref increment);
            }

            CheckCatalogChangesBackup(catalogChanges, CREATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);

            // Second sync

            File.Copy(imagePath1, destinationFilePathToCopy);

            List<string> assetPathsUpdated = [];
            assetPaths.ForEach(assetPathsUpdated.Add);
            assetPathsUpdated.Add(destinationFilePathToCopy);

            List<Asset> expectedAssetsUpdated = [];
            expectedAssets.ForEach(expectedAssetsUpdated.Add);
            expectedAssetsUpdated.Add(_asset1Temp);

            List<int> assetsImageByteSizeUpdated = [];
            assetsImageByteSize.ForEach(assetsImageByteSizeUpdated.Add);
            assetsImageByteSizeUpdated.Add(ASSET1_TEMP_IMAGE_BYTE_SIZE);

            assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(5, assetsInDirectory.Length);
            Assert.IsTrue(File.Exists(destinationFilePathToCopy));

            await _catalogAssetsService!.CatalogAssetsAsync(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            _asset1Temp!.Folder = folder!;
            _asset1Temp!.FolderId = folder!.FolderId;

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(5, assetsFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(5, assetsFromRepository.Count);

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssetsUpdated[i], assetPathsUpdated[i], assetsDirectory, folder);
            }

            Dictionary<Folder, List<Asset>> folderToAssetsMappingUpdated = new() { { folder, expectedAssetsUpdated} };
            Dictionary<string, int> assetNameToByteSizeMappingUpdated = new()
            {
                { _asset1!.FileName, ASSET1_IMAGE_BYTE_SIZE },
                { _asset2!.FileName, ASSET2_IMAGE_BYTE_SIZE },
                { _asset3!.FileName, ASSET3_IMAGE_BYTE_SIZE },
                { _asset4!.FileName, ASSET4_IMAGE_BYTE_SIZE },
                { _asset1Temp!.FileName, ASSET1_TEMP_IMAGE_BYTE_SIZE }
            };

            AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMappingUpdated, [folder], thumbnails, assetsImageByteSizeUpdated);
            CheckBlobsAndTablesAfterSaveCatalog(
                blobsPath,
                tablesPath,
                [folder],
                [folder],
                assetsFromRepository,
                folderToAssetsMappingUpdated,
                assetNameToByteSizeMappingUpdated);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            CheckBackupAfter(
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder],
                [folder],
                assetsFromRepository,
                folderToAssetsMappingUpdated,
                assetNameToByteSizeMappingUpdated);

            Assert.AreEqual(15, catalogChanges.Count);

            increment = 0;

            foldersInRepository = _testableAssetRepository!.GetFolders();

            CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            foreach (Asset expectedAsset in folderToAssetsMapping[folder])
            {
                CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder],
                    expectedAsset,
                    folder,
                    ref increment);
            }

            CheckCatalogChangesBackup(catalogChanges, CREATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);

            CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            CheckCatalogChangesAssetAdded(catalogChanges, assetsDirectory, expectedAssetsUpdated, _asset1Temp, folder, ref increment);
            CheckCatalogChangesBackup(catalogChanges, UPDATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            File.Delete(destinationFilePathToCopy);
        }
    }

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public async Task CatalogAssetsAsync_AssetsAndRootCatalogFolderExistsAndBackupExistsOnDifferentDateAndOneNewAsset_SyncTheAssets(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder2");
        string destinationFilePathToCopy = Path.Combine(assetsDirectory, _asset1Temp!.FileName);

        ConfigureCatalogAssetService(100, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        try
        {
            string imagePath1 = Path.Combine(assetsDirectory, "Image 1_duplicate.jpg");
            string imagePath2 = Path.Combine(assetsDirectory, "Image 9.png");
            string imagePath3 = Path.Combine(assetsDirectory, "Image 9_duplicate.png");
            string imagePath4 = Path.Combine(assetsDirectory, "Image_11.heic");

            List<string> assetPaths = [imagePath1, imagePath2, imagePath3, imagePath4];
            List<Asset> expectedAssets = [_asset1!, _asset2!, _asset3!, _asset4!];
            List<int> assetsImageByteSize = [ASSET1_IMAGE_BYTE_SIZE, ASSET2_IMAGE_BYTE_SIZE, ASSET3_IMAGE_BYTE_SIZE, ASSET4_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(4, assetsInDirectory.Length);

            foreach (string assetPath in assetPaths)
            {
                Assert.IsTrue(File.Exists(assetPath));
            }

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNull(folder);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CheckBackupBefore(backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _catalogAssetsService!.CatalogAssetsAsync(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            _asset1!.Folder = folder!;
            _asset1!.FolderId = folder!.FolderId;
            _asset2!.Folder = folder;
            _asset2!.FolderId = folder.FolderId;
            _asset3!.Folder = folder;
            _asset3!.FolderId = folder.FolderId;
            _asset4!.Folder = folder;
            _asset4!.FolderId = folder.FolderId;

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(4, assetsFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(4, assetsFromRepository.Count);

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssets[i], assetPaths[i], assetsDirectory, folder);
            }

            Dictionary<Folder, List<Asset>> folderToAssetsMapping = new() { { folder, expectedAssets } };
            Dictionary<string, int> assetNameToByteSizeMapping = new()
            {
                { _asset1!.FileName, ASSET1_IMAGE_BYTE_SIZE },
                { _asset2!.FileName, ASSET2_IMAGE_BYTE_SIZE },
                { _asset3!.FileName, ASSET3_IMAGE_BYTE_SIZE },
                { _asset4!.FileName, ASSET4_IMAGE_BYTE_SIZE }
            };

            AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMapping, [folder], thumbnails, assetsImageByteSize);
            CheckBlobsAndTablesAfterSaveCatalog(
                blobsPath,
                tablesPath,
                [folder],
                [folder],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            CheckBackupAfter(
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder],
                [folder],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.AreEqual(9, catalogChanges.Count);

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            foreach (Asset expectedAsset in folderToAssetsMapping[folder])
            {
                CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder],
                    expectedAsset,
                    folder,
                    ref increment);
            }

            CheckCatalogChangesBackup(catalogChanges, CREATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);

            // Second sync

            string oldBackupFileName = DateTime.Now.Date.ToString("20240110") + ".zip";
            string oldBackupFilePath = Path.Combine(_databaseBackupPath!, oldBackupFileName);
            File.Copy(backupFilePath, oldBackupFilePath);
            File.Delete(backupFilePath);

            CheckBackupBefore(backupFilePath);

            File.Copy(imagePath1, destinationFilePathToCopy);

            List<string> assetPathsUpdated = [];
            assetPaths.ForEach(assetPathsUpdated.Add);
            assetPathsUpdated.Add(destinationFilePathToCopy);

            List<Asset> expectedAssetsUpdated = [];
            expectedAssets.ForEach(expectedAssetsUpdated.Add);
            expectedAssetsUpdated.Add(_asset1Temp);

            List<int> assetsImageByteSizeUpdated = [];
            assetsImageByteSize.ForEach(assetsImageByteSizeUpdated.Add);
            assetsImageByteSizeUpdated.Add(ASSET1_TEMP_IMAGE_BYTE_SIZE);

            assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(5, assetsInDirectory.Length);
            Assert.IsTrue(File.Exists(destinationFilePathToCopy));

            await _catalogAssetsService!.CatalogAssetsAsync(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            _asset1Temp!.Folder = folder!;
            _asset1Temp!.FolderId = folder!.FolderId;

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(5, assetsFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(5, assetsFromRepository.Count);

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssetsUpdated[i], assetPathsUpdated[i], assetsDirectory, folder);
            }

            Dictionary<Folder, List<Asset>> folderToAssetsMappingUpdated = new() { { folder, expectedAssetsUpdated} };
            Dictionary<string, int> assetNameToByteSizeMappingUpdated = new()
            {
                { _asset1!.FileName, ASSET1_IMAGE_BYTE_SIZE },
                { _asset2!.FileName, ASSET2_IMAGE_BYTE_SIZE },
                { _asset3!.FileName, ASSET3_IMAGE_BYTE_SIZE },
                { _asset4!.FileName, ASSET4_IMAGE_BYTE_SIZE },
                { _asset1Temp!.FileName, ASSET1_TEMP_IMAGE_BYTE_SIZE }
            };

            AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMappingUpdated, [folder], thumbnails, assetsImageByteSizeUpdated);
            CheckBlobsAndTablesAfterSaveCatalog(
                blobsPath,
                tablesPath,
                [folder],
                [folder],
                assetsFromRepository,
                folderToAssetsMappingUpdated,
                assetNameToByteSizeMappingUpdated);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            CheckBackupAfter(
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder],
                [folder],
                assetsFromRepository,
                folderToAssetsMappingUpdated,
                assetNameToByteSizeMappingUpdated);

            Assert.AreEqual(15, catalogChanges.Count);

            increment = 0;

            foldersInRepository = _testableAssetRepository!.GetFolders();

            CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            foreach (Asset expectedAsset in folderToAssetsMapping[folder])
            {
                CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder],
                    expectedAsset,
                    folder,
                    ref increment);
            }

            CheckCatalogChangesBackup(catalogChanges, CREATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);

            CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            CheckCatalogChangesAssetAdded(catalogChanges, assetsDirectory, expectedAssetsUpdated, _asset1Temp, folder, ref increment);
            CheckCatalogChangesBackup(catalogChanges, CREATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);

            Assert.IsTrue(File.Exists(oldBackupFilePath));
            Assert.IsTrue(File.Exists(backupFilePath));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            File.Delete(destinationFilePathToCopy);
        }
    }

    // BACKUP SECTION ------------------------------------------------------------------------------------------------
    
    // TODO: Delete -> Add, then delete, then add
    // TODO: Clean up
    // TODO: Add testCase for AnalyseVideo for each tests
    // TODO: Verify each ReasonEnum and fix them
    // TODO: RAF (+ cases in Notion)
    // TODO: Cases for exception handling
    // TODO: Test to Cancel the token for each method (testcase)

    // TODO: Tests the remaining catch in the method to have 100% of coverage.
    [Test]
    [Ignore("")]
    public void CatalogAssetsAsync_NoAssetsAndTokenIsCancelled_Throws()
    {
        // ConfigureCatalogAssetService(defaultAssetsDirectory!);
        //
        // try
        // {
        //     CancellationTokenSource cancellationTokenSource = new();
        //
        //     // Start the task but don't wait for it
        //     Task task = _catalogAssetsService!.CatalogAssetsAsync(null!, cancellationTokenSource.Token);
        //
        //     // Simulate cancellation after a short delay
        //     cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(1));
        //
        //     Assert.ThrowsAsync<OperationCanceledException>(async () => await task);
        // }
        // finally
        // {
        //     Directory.Delete(databaseDirectory!, true);
        // }
    }

    [Test]
    [Ignore("")]
    public async Task CatalogAssetsAsync_LogOnExceptionTest_()
    {
        // string appDataFolder = Path.Combine(dataDirectory, "ApplicationData", Guid.NewGuid().ToString());
        //
        // using var mock = AutoMock.GetLoose(
        //     cfg =>
        //     {
        //         cfg.RegisterDatabaseTypes();
        //         cfg.RegisterType<AssetHashCalculatorService>().As<IAssetHashCalculatorService>().SingleInstance();
        //         cfg.RegisterType<DirectoryComparer>().As<IDirectoryComparer>().SingleInstance();
        //         cfg.RegisterType<AssetRepository>().As<IAssetRepository>().SingleInstance();
        //         cfg.RegisterType<CatalogAssetsService>().As<ICatalogAssetsService>().SingleInstance();
        //         cfg.RegisterType<MoveAssetsService>().As<IMoveAssetsService>().SingleInstance();
        //     });
        // mock.Mock<IUserConfigurationService>().Setup(conf => conf.GetApplicationBackupFolderPath()).Returns(appDataFolder);
        // mock.Mock<IUserConfigurationService>().Setup(conf => conf.GetPicturesDirectory()).Returns(dataDirectory);
        // mock.Mock<IUserConfigurationService>().Setup(conf => conf.GetCatalogBatchSize()).Returns(1000);
        // mock.Mock<IUserConfigurationService>().Setup(conf => conf.GetRootCatalogFolderPaths()).Returns(new string[] { dataDirectory });
        //
        // mock.Mock<IStorageService>().Setup(s => s.FolderExists(It.IsAny<string>())).Returns(true);
        // mock.Mock<IStorageService>().Setup(s => s.GetFileNames(It.IsAny<string>())).Throws(new IOException());
        // mock.Mock<IStorageService>().Setup(s => s.ResolveDataDirectory(It.IsAny<double>())).Returns(appDataFolder);
        //
        // var repository = mock.Container.Resolve<IAssetRepository>();
        // var catalogAssetsService = mock.Container.Resolve<ICatalogAssetsService>();
        // var moveAssetsService = mock.Container.Resolve<IMoveAssetsService>();
        //
        // string[] fileList = Directory.GetFiles(dataDirectory, "*.jp*g") // jpg and jpeg files
        // .Select(f => Path.GetFileName(f))
        // .ToArray();
        //
        // var statusChanges = new List<CatalogChangeCallbackEventArgs>();
        //
        // await catalogAssetsService.CatalogAssetsAsync(e => statusChanges.Add(e));
        //
        // var processedAssets = statusChanges.Where(s => s.Asset != null).Select(s => s.Asset).ToList();
        // var exceptions = statusChanges.Where(s => s.Exception != null).Select(s => s.Exception).ToList();
        //
        // var repositoryAssets = repository.GetAssetsByPath(dataDirectory);
        // processedAssets.Should().BeEmpty();
        // repositoryAssets.Should().BeEmpty();
        // exceptions.Should().ContainSingle();
    }

    [Test]
    [Ignore("")]
    public async Task CatalogAssetsAsync_SaveCatalogOnOperationCanceledExceptionTest_()
    {
        // using var mock = AutoMock.GetLoose(
        //     cfg =>
        //     {
        //         cfg.RegisterType<Database>().As<IDatabase>().SingleInstance();
        //         cfg.RegisterType<AssetHashCalculatorService>().As<IAssetHashCalculatorService>().SingleInstance();
        //         cfg.RegisterType<DirectoryComparer>().As<IDirectoryComparer>().SingleInstance();
        //         cfg.RegisterType<CatalogAssetsService>().As<ICatalogAssetsService>().SingleInstance();
        //         cfg.RegisterType<MoveAssetsService>().As<IMoveAssetsService>().SingleInstance();
        //     });
        // mock.Mock<IUserConfigurationService>().Setup(conf => conf.GetApplicationBackupFolderPath()).Returns(Path.Combine(dataDirectory, "ApplicationData", Guid.NewGuid().ToString()));
        // mock.Mock<IUserConfigurationService>().Setup(conf => conf.GetPicturesDirectory()).Returns(dataDirectory);
        // mock.Mock<IUserConfigurationService>().Setup(conf => conf.GetCatalogBatchSize()).Returns(1000);
        // mock.Mock<IUserConfigurationService>().Setup(conf => conf.GetRootCatalogFolderPaths()).Returns(new string[] { dataDirectory });
        //
        // mock.Mock<IStorageService>().Setup(s => s.FolderExists(It.IsAny<string>())).Returns(true);
        // mock.Mock<IStorageService>().Setup(s => s.GetFileNames(It.IsAny<string>())).Throws(new OperationCanceledException());
        // mock.Mock<IAssetRepository>().Setup(a => a.GetFolders()).Returns(new Folder[] { new Folder { Path = dataDirectory } });
        //
        // var repository = mock.Container.Resolve<IAssetRepository>();
        // var catalogAssetsService = mock.Container.Resolve<ICatalogAssetsService>();
        // var moveAssetsService = mock.Container.Resolve<IMoveAssetsService>();
        //
        // var statusChanges = new List<CatalogChangeCallbackEventArgs>();
        //
        // Func<Task> func = async () => await catalogAssetsService.CatalogAssetsAsync(e => statusChanges.Add(e));
        // await func.Should().ThrowAsync<OperationCanceledException>();
        // mock.Mock<IAssetRepository>().Verify(r => r.SaveCatalog(It.IsAny<Folder>()), Times.Once);
    }

    private static void CheckBlobsAndTablesBeforeSaveCatalog(string blobsPath, string tablesPath)
    {
        string[] blobFiles = Directory.GetFiles(blobsPath);
        string[] tableFiles = Directory.GetFiles(tablesPath);

        Assert.IsEmpty(blobFiles);
        Assert.IsEmpty(tableFiles);

        Assert.IsFalse(File.Exists(Path.Combine(tablesPath, "assets.db")));
        Assert.IsFalse(File.Exists(Path.Combine(tablesPath, "folders.db")));
        Assert.IsFalse(File.Exists(Path.Combine(tablesPath, "syncassetsdirectoriesdefinitions.db")));
        Assert.IsFalse(File.Exists(Path.Combine(tablesPath, "recenttargetpaths.db")));
    }

    private void CheckBlobsAndTablesAfterSaveCatalog(
        string blobsPath,
        string tablesPath,
        IReadOnlyCollection<Folder> folders,
        IReadOnlyCollection<Folder> foldersContainingAssets,
        IReadOnlyCollection<Asset> assetsFromRepository,
        Dictionary<Folder, List<Asset>> folderToAssetsMapping,
        Dictionary<string, int> assetNameToByteSizeMapping)
    {
        string[] blobFiles = Directory.GetFiles(blobsPath);
        string[] tableFiles = Directory.GetFiles(tablesPath);

        Assert.AreEqual(foldersContainingAssets.Count, blobFiles.Length);

        foreach (Folder folder in foldersContainingAssets)
        {
            string blobFileName = $"{folder.FolderId}.bin";
            string blobFilePath = Path.Combine(blobsPath, blobFileName);

            Assert.IsTrue(File.Exists(blobFilePath));

            List<Asset> assetsFromRepositoryByFolder = assetsFromRepository.Where(x => x.FolderId == folder.FolderId).ToList();

            Dictionary<string, byte[]>? dataRead = _blobStorage!.ReadFromBinaryFile(blobFilePath);
            Assert.IsNotNull(dataRead);
            Assert.AreEqual(assetsFromRepositoryByFolder.Count, dataRead!.Count);

            for (int j = 0; j < dataRead.Count; j++)
            {
                Assert.IsTrue(dataRead.ContainsKey(assetsFromRepositoryByFolder[j].FileName));
                Assert.IsTrue(assetNameToByteSizeMapping.ContainsKey(assetsFromRepositoryByFolder[j].FileName));
                Assert.AreEqual(assetNameToByteSizeMapping[assetsFromRepositoryByFolder[j].FileName], dataRead[assetsFromRepositoryByFolder[j].FileName].Length);
            }
        }

        Assert.AreEqual(4, tableFiles.Length);
        Assert.IsTrue(File.Exists(Path.Combine(tablesPath, "assets.db")));
        Assert.IsTrue(File.Exists(Path.Combine(tablesPath, "folders.db")));
        Assert.IsTrue(File.Exists(Path.Combine(tablesPath, "syncassetsdirectoriesdefinitions.db")));
        Assert.IsTrue(File.Exists(Path.Combine(tablesPath, "recenttargetpaths.db")));

        List<Asset> assetsFromDatabase = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
        List<Folder> foldersFromDatabase = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.FoldersTableName, FolderConfigs.ReadFunc);
        List<SyncAssetsDirectoriesDefinition> syncAssetsDirectoriesDefinitionsFromDatabase =
            _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.SyncAssetsDirectoriesDefinitionsTableName, SyncAssetsDirectoriesDefinitionConfigs.ReadFunc);
        List<string> recentTargetPathsFromDatabase = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.RecentTargetPathsTableName, RecentPathsConfigs.ReadFunc);

        Assert.AreEqual(assetsFromRepository.Count, assetsFromDatabase.Count);

        Dictionary<Asset, Folder> assetToFolderMapping = folderToAssetsMapping.SelectMany(kv =>
            kv.Value.Select(a => (Asset: a, Folder: kv.Key))).ToDictionary(x => x.Asset, x => x.Folder);

        foreach (Asset assetFromDatabase in assetsFromDatabase)
        {
            Asset expectedAsset = assetToFolderMapping.Keys.First(a => a.FileName == assetFromDatabase.FileName && a.FolderId == assetFromDatabase.FolderId);
            Folder expectedFolder = assetToFolderMapping[expectedAsset];

            AssertAssetFromDatabaseValidity(assetFromDatabase, expectedAsset, expectedFolder.FolderId);
        }

        Assert.AreEqual(folders.Count, foldersFromDatabase.Count);

        Dictionary<Guid, Folder> foldersById = folders.ToDictionary(f => f.FolderId, f => f);
        Dictionary<Guid, Folder> foldersFromDatabaseById = foldersFromDatabase.ToDictionary(f => f.FolderId, f => f);

        foreach ((Guid folderId, Folder? expectedFolder) in foldersById)
        {
            Folder actualFolder = foldersFromDatabaseById[folderId];

            Assert.AreEqual(expectedFolder.FolderId, actualFolder.FolderId);
            Assert.AreEqual(expectedFolder.Path, actualFolder.Path);
        }

        Assert.IsEmpty(syncAssetsDirectoriesDefinitionsFromDatabase);
        Assert.IsEmpty(recentTargetPathsFromDatabase);
    }

    private void CheckBlobsAndTablesAfterSaveCatalogEmpty(string blobsPath, string tablesPath, bool hasEmptyTables, bool hasOneFolder, Folder folder)
    {
        string[] blobFiles = Directory.GetFiles(blobsPath);
        string[] tableFiles = Directory.GetFiles(tablesPath);

        Assert.IsEmpty(blobFiles);

        if (hasEmptyTables)
        {
            Assert.AreEqual(4, tableFiles.Length);
            Assert.IsTrue(File.Exists(Path.Combine(tablesPath, "assets.db")));
            Assert.IsTrue(File.Exists(Path.Combine(tablesPath, "folders.db")));
            Assert.IsTrue(File.Exists(Path.Combine(tablesPath, "syncassetsdirectoriesdefinitions.db")));
            Assert.IsTrue(File.Exists(Path.Combine(tablesPath, "recenttargetpaths.db")));
        }
        else
        {
            Assert.IsEmpty(tableFiles);
        }

        List<Asset> assetsFromDatabase = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
        List<Folder> foldersFromDatabase = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.FoldersTableName, FolderConfigs.ReadFunc);
        List<SyncAssetsDirectoriesDefinition> syncAssetsDirectoriesDefinitionsFromDatabase =
            _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.SyncAssetsDirectoriesDefinitionsTableName, SyncAssetsDirectoriesDefinitionConfigs.ReadFunc);
        List<string> recentTargetPathsFromDatabase = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.RecentTargetPathsTableName, RecentPathsConfigs.ReadFunc);

        Assert.IsEmpty(assetsFromDatabase);

        if (hasOneFolder)
        {
            Assert.AreEqual(1, foldersFromDatabase.Count);
            Assert.AreEqual(folder.FolderId, foldersFromDatabase[0].FolderId);
            Assert.AreEqual(folder.Path, foldersFromDatabase[0].Path);
        }
        else
        {
            Assert.IsEmpty(foldersFromDatabase);
        }

        Assert.IsEmpty(syncAssetsDirectoriesDefinitionsFromDatabase);
        Assert.IsEmpty(recentTargetPathsFromDatabase);
    }

    private void CheckBackupBefore(string backupFilePath)
    {
        Assert.IsFalse(File.Exists(backupFilePath));
        Assert.IsFalse(_testableAssetRepository!.BackupExists());
    }

    private void CheckBackupAfter(
        string backupFilePath,
        string blobsPath,
        string tablesPath,
        IReadOnlyCollection<Folder> folders,
        IReadOnlyCollection<Folder> foldersContainingAssets,
        IReadOnlyCollection<Asset> assetsFromRepository,
        Dictionary<Folder, List<Asset>> folderToAssetsMapping,
        Dictionary<string, int> assetNameToByteSizeMapping)
    {
        string backupBlobsDirectory = Path.Combine(_databaseBackupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
        string backupTablesDirectory = Path.Combine(_databaseBackupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

        Assert.IsFalse(Directory.Exists(backupBlobsDirectory));
        Assert.IsFalse(Directory.Exists(backupTablesDirectory));

        ZipFile.ExtractToDirectory(backupFilePath, _databaseBackupPath!);
        Assert.IsTrue(File.Exists(backupFilePath));

        Assert.IsTrue(Directory.Exists(backupBlobsDirectory));
        Assert.IsTrue(Directory.Exists(backupTablesDirectory));

        string[] sourceDirectories = Directory.GetDirectories(_databasePath!);
        string[] backupDirectories = Directory.GetDirectories(_databaseBackupPath!);

        Assert.AreEqual(sourceDirectories.Length, backupDirectories.Length);

        Assert.AreEqual(sourceDirectories[0], blobsPath);
        string[] blobs = Directory.GetFiles(blobsPath);
        Assert.AreEqual(foldersContainingAssets.Count, blobs.Length);

        Assert.AreEqual(sourceDirectories[1], tablesPath);
        string[] tables = Directory.GetFiles(tablesPath);
        Assert.AreEqual(4, tables.Length);

        Assert.AreEqual(backupDirectories[0], backupBlobsDirectory);
        string[] blobsBackup = Directory.GetFiles(backupBlobsDirectory);
        Assert.AreEqual(foldersContainingAssets.Count, blobsBackup.Length);

        Assert.AreEqual(backupDirectories[1], backupTablesDirectory);
        string[] tablesBackup = Directory.GetFiles(backupTablesDirectory);
        Assert.AreEqual(4, tablesBackup.Length);

        CheckBlobsAndTablesAfterSaveCatalog(
            backupBlobsDirectory,
            backupTablesDirectory,
            folders,
            foldersContainingAssets,
            assetsFromRepository,
            folderToAssetsMapping,
            assetNameToByteSizeMapping);

        Directory.Delete(backupBlobsDirectory, true);
        Directory.Delete(backupTablesDirectory, true);

        Assert.IsFalse(Directory.Exists(backupBlobsDirectory));
        Assert.IsFalse(Directory.Exists(backupTablesDirectory));
    }

    private void CheckDefaultEmptyBackup(string backupFilePath, string blobsPath, string tablesPath, bool hasEmptyTables, bool hasOneFolder, Folder folder)
    {
        int expectedTablesCount = hasEmptyTables ? 4 : 0;

        string backupBlobsDirectory = Path.Combine(_databaseBackupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
        string backupTablesDirectory = Path.Combine(_databaseBackupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

        Assert.IsFalse(Directory.Exists(backupBlobsDirectory));
        Assert.IsFalse(Directory.Exists(backupTablesDirectory));

        ZipFile.ExtractToDirectory(backupFilePath, _databaseBackupPath!);
        File.Delete(backupFilePath);
        Assert.IsFalse(File.Exists(backupFilePath));

        Assert.IsTrue(Directory.Exists(backupBlobsDirectory));
        Assert.IsTrue(Directory.Exists(backupTablesDirectory));

        string[] sourceDirectories = Directory.GetDirectories(_databasePath!);
        string[] backupDirectories = Directory.GetDirectories(_databaseBackupPath!);

        Assert.AreEqual(sourceDirectories.Length, backupDirectories.Length);

        Assert.AreEqual(sourceDirectories[0], blobsPath);
        string[] blobs = Directory.GetFiles(blobsPath);
        Assert.AreEqual(0, blobs.Length);

        Assert.AreEqual(sourceDirectories[1], tablesPath);
        string[] tables = Directory.GetFiles(tablesPath);
        Assert.AreEqual(expectedTablesCount, tables.Length);

        Assert.AreEqual(backupDirectories[0], backupBlobsDirectory);
        string[] blobsBackup = Directory.GetFiles(backupBlobsDirectory);
        Assert.AreEqual(0, blobsBackup.Length);

        Assert.AreEqual(backupDirectories[1], backupTablesDirectory);
        string[] tablesBackup = Directory.GetFiles(backupTablesDirectory);
        Assert.AreEqual(expectedTablesCount, tablesBackup.Length);

        CheckBlobsAndTablesAfterSaveCatalogEmpty(backupBlobsDirectory, backupTablesDirectory, hasEmptyTables, hasOneFolder, folder);
    }

    private static void AssertAssetPropertyValidity(Asset asset, Asset expectedAsset, string assetPath, string folderPath, Folder folder)
    {
        DateTime actualDate = DateTime.Now.Date;

        Assert.AreEqual(expectedAsset.FileName, asset.FileName);
        Assert.AreEqual(folder.FolderId, asset.FolderId);
        Assert.AreEqual(folder, asset.Folder);
        Assert.AreEqual(expectedAsset.FileSize, asset.FileSize);
        Assert.AreEqual(expectedAsset.PixelWidth, asset.PixelWidth);
        Assert.AreEqual(expectedAsset.PixelHeight, asset.PixelHeight);
        Assert.AreEqual(expectedAsset.ThumbnailPixelWidth, asset.ThumbnailPixelWidth);
        Assert.AreEqual(expectedAsset.ThumbnailPixelHeight, asset.ThumbnailPixelHeight);
        Assert.AreEqual(expectedAsset.ImageRotation, asset.ImageRotation);
        Assert.AreEqual(actualDate, asset.ThumbnailCreationDateTime.Date);
        Assert.AreEqual(expectedAsset.Hash, asset.Hash);
        Assert.AreEqual(expectedAsset.IsAssetCorrupted, asset.IsAssetCorrupted);
        Assert.AreEqual(expectedAsset.AssetCorruptedMessage, asset.AssetCorruptedMessage);
        Assert.AreEqual(expectedAsset.IsAssetRotated, asset.IsAssetRotated);
        Assert.AreEqual(expectedAsset.AssetRotatedMessage, asset.AssetRotatedMessage);
        Assert.AreEqual(assetPath, asset.FullPath);
        Assert.AreEqual(folderPath, asset.Folder.Path);
        Assert.IsNotNull(asset.ImageData); // Unlike in CatalogAssetsServiceCreateAssetTests it is set here
        Assert.AreEqual(actualDate, asset.FileCreationDateTime.Date); // Because files are generated by tests (thumbnailCreationDateTime is then the FileModificationDateTime value)  
        Assert.AreEqual(expectedAsset.ThumbnailCreationDateTime.Date, asset.FileModificationDateTime.Date);  // Unlike in CatalogAssetsServiceCreateAssetTests it is set here
    }

    private static void AssertThumbnailsValidity(
        IReadOnlyList<Asset> assetsFromRepository,
        Dictionary<Folder, List<Asset>> folderToAssetsMapping,
        IReadOnlyList<Folder> folders,
        Dictionary<string, Dictionary<string, byte[]>> thumbnails,
        IReadOnlyList<int> assetsImageByteSize)
    {
        Assert.AreEqual(folders.Count, thumbnails.Count);

        int thumbnailsTotalCount = 0;

        for (int i = 0; i < thumbnails.Count; i++)
        {
            Assert.IsTrue(thumbnails.ContainsKey(folders[i].Path));
            thumbnailsTotalCount += thumbnails[folders[i].Path].Count;
        }

        Assert.AreEqual(assetsFromRepository.Count, thumbnailsTotalCount);

        Dictionary<Asset, Folder> assetToFolderMapping = folderToAssetsMapping.SelectMany(kv =>
            kv.Value.Select(a => (Asset: a, Folder: kv.Key))).ToDictionary(x => x.Asset, x => x.Folder);

        for (int i = 0; i < assetsFromRepository.Count; i++)
        {
            Asset currentAsset = assetsFromRepository[i];

            Asset expectedAsset = assetToFolderMapping.Keys.First(a => a.FileName == currentAsset.FileName && a.FolderId == currentAsset.FolderId);
            Folder expectedFolder = assetToFolderMapping[expectedAsset];

            Assert.IsTrue(thumbnails[expectedFolder.Path].ContainsKey(currentAsset.FileName));

            byte[] assetImageByteSize = thumbnails[expectedFolder.Path][currentAsset.FileName];

            Assert.IsNotNull(assetImageByteSize);
            Assert.AreEqual(assetsImageByteSize[i], assetImageByteSize.Length);
        }
    }

    private static void AssertAssetFromDatabaseValidity(Asset assetFromDatabase, Asset expectedAsset, Guid folderId)
    {
        DateTime actualDate = DateTime.Now.Date;
        DateTime minDate = DateTime.MinValue.Date;

        Assert.AreEqual(expectedAsset.FileName, assetFromDatabase.FileName);
        Assert.AreEqual(folderId, assetFromDatabase.FolderId);
        Assert.IsNull(assetFromDatabase.Folder);  // Not saved in Db, loaded at the runtime
        Assert.AreEqual(expectedAsset.FileSize, assetFromDatabase.FileSize);
        Assert.AreEqual(expectedAsset.PixelWidth, assetFromDatabase.PixelWidth);
        Assert.AreEqual(expectedAsset.PixelHeight, assetFromDatabase.PixelHeight);
        Assert.AreEqual(expectedAsset.ThumbnailPixelWidth, assetFromDatabase.ThumbnailPixelWidth);
        Assert.AreEqual(expectedAsset.ThumbnailPixelHeight, assetFromDatabase.ThumbnailPixelHeight);
        Assert.AreEqual(expectedAsset.ImageRotation, assetFromDatabase.ImageRotation);
        Assert.AreEqual(actualDate, assetFromDatabase.ThumbnailCreationDateTime.Date);
        Assert.AreEqual(expectedAsset.Hash, assetFromDatabase.Hash);
        Assert.AreEqual(expectedAsset.IsAssetCorrupted, assetFromDatabase.IsAssetCorrupted);
        Assert.AreEqual(string.Empty, assetFromDatabase.AssetCorruptedMessage);
        Assert.AreEqual(expectedAsset.IsAssetRotated, assetFromDatabase.IsAssetRotated);
        Assert.AreEqual(string.Empty, assetFromDatabase.AssetRotatedMessage);
        Assert.AreEqual(expectedAsset.FileName, assetFromDatabase.FullPath); // Folder is not saved in Db, loaded at the runtime
        Assert.IsNull(assetFromDatabase.ImageData); // Not saved in Db, loaded at the runtime
        Assert.AreEqual(minDate, assetFromDatabase.FileCreationDateTime.Date); // Not saved in Db, loaded at the runtime
        Assert.AreEqual(minDate, assetFromDatabase.FileModificationDateTime.Date); // Not saved in Db, loaded at the runtime
    }

    private static void CheckCatalogChangesBackup(IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges, string expectedMessage, ref int increment)
    {
        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        Assert.IsNull(catalogChange.Asset);
        Assert.IsNull(catalogChange.Folder);
        Assert.IsEmpty(catalogChange.CataloguedAssetsByPath);
        Assert.AreEqual(ReasonEnum.AssetCreated, catalogChange.Reason);
        Assert.AreEqual(expectedMessage, catalogChange.Message);
        Assert.IsNull(catalogChange.Exception);
        increment++;

        catalogChange = catalogChanges[increment];
        Assert.IsNull(catalogChange.Asset);
        Assert.IsNull(catalogChange.Folder);
        Assert.IsEmpty(catalogChange.CataloguedAssetsByPath);
        Assert.AreEqual(ReasonEnum.AssetCreated, catalogChange.Reason);
        Assert.AreEqual(string.Empty, catalogChange.Message);
        Assert.IsNull(catalogChange.Exception);
        increment++;
    }

    private static void CheckCatalogChangesInspectingFolder(IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges, int expectedFoldersCount, IReadOnlyCollection<Folder> folders, string assetsDirectory, ref int increment)
    {
        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        Assert.IsNull(catalogChange.Asset);
        Assert.AreEqual(expectedFoldersCount, folders.Count);
        Assert.IsNotNull(catalogChange.Folder);
        Assert.AreEqual(folders.First(x => x.FolderId == catalogChange.Folder!.FolderId), catalogChange.Folder);
        Assert.AreEqual(assetsDirectory, catalogChange.Folder!.Path);
        Assert.AreEqual(0, catalogChange.CataloguedAssetsByPath.Count);
        Assert.AreEqual(ReasonEnum.AssetCreated, catalogChange.Reason); // TODO: Bad reason
        Assert.AreEqual($"Inspecting folder {assetsDirectory}.", catalogChange.Message);
        Assert.IsNull(catalogChange.Exception);
        increment++;
    }

    private static void CheckCatalogChangesFolderAdded(IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges, int expectedFoldersCount, IReadOnlyCollection<Folder> folders, string assetsDirectory, ref int increment)
    {
        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        Assert.IsNull(catalogChange.Asset);
        Assert.AreEqual(expectedFoldersCount, folders.Count);
        Assert.IsNotNull(catalogChange.Folder);
        Assert.AreEqual(folders.First(x => x.FolderId == catalogChange.Folder!.FolderId), catalogChange.Folder);
        Assert.AreEqual(assetsDirectory, catalogChange.Folder!.Path);
        Assert.AreEqual(0, catalogChange.CataloguedAssetsByPath.Count);
        Assert.AreEqual(ReasonEnum.FolderCreated, catalogChange.Reason);
        Assert.AreEqual($"Folder {assetsDirectory} added to catalog.", catalogChange.Message);
        Assert.IsNull(catalogChange.Exception);
        increment++;
    }

    private static void CheckCatalogChangesFolderDeleted(IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges, int expectedFoldersCount, int foldersCount, string assetsDirectory, ref int increment)
    {
        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        Assert.IsNull(catalogChange.Asset);
        Assert.AreEqual(expectedFoldersCount, foldersCount);
        Assert.IsNotNull(catalogChange.Folder);
        Assert.AreEqual(assetsDirectory, catalogChange.Folder!.Path);
        Assert.AreEqual(0, catalogChange.CataloguedAssetsByPath.Count);
        Assert.AreEqual(ReasonEnum.FolderDeleted, catalogChange.Reason);
        Assert.AreEqual($"Folder {assetsDirectory} deleted from catalog.", catalogChange.Message);
        Assert.IsNull(catalogChange.Exception);
        increment++;
    }

    private static void CheckCatalogChangesAssetAdded(
        IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges,
        string assetsDirectory,
        IReadOnlyCollection<Asset> expectedAssets,
        Asset expectedAsset,
        Folder folder,
        ref int increment)
    {
        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        Assert.IsNotNull(catalogChange.Asset);
        AssertAssetPropertyValidity(catalogChange.Asset!, expectedAsset, expectedAsset.FullPath, assetsDirectory, folder);
        Assert.IsNull(catalogChange.Folder);
        Assert.AreEqual(expectedAssets.Count, catalogChange.CataloguedAssetsByPath.Count);
        Assert.AreEqual(ReasonEnum.AssetCreated, catalogChange.Reason);
        Assert.AreEqual($"Image {expectedAsset.FullPath} added to catalog.", catalogChange.Message);
        Assert.IsNull(catalogChange.Exception);
        increment++;
    }

    private static void CheckCatalogChangesAssetUpdated(
        IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges,
        string assetsDirectory,
        IReadOnlyCollection<Asset> expectedAssets,
        Asset expectedAsset,
        Folder folder,
        ref int increment)
    {
        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        Assert.IsNotNull(catalogChange.Asset);
        AssertAssetPropertyValidity(catalogChange.Asset!, expectedAsset, expectedAsset.FullPath, assetsDirectory, folder);
        Assert.IsNull(catalogChange.Folder);
        Assert.AreEqual(expectedAssets.Count, catalogChange.CataloguedAssetsByPath.Count);
        Assert.AreEqual(ReasonEnum.AssetUpdated, catalogChange.Reason);
        Assert.AreEqual($"Image {expectedAsset.FullPath} updated in catalog.", catalogChange.Message);
        Assert.IsNull(catalogChange.Exception);
        increment++;
    }

    private static void CheckCatalogChangesAssetDeleted(
        IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges,
        string assetsDirectory,
        IReadOnlyCollection<Asset> expectedAssets,
        Asset expectedAsset,
        Folder folder,
        ref int increment)
    {
        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        Assert.IsNotNull(catalogChange.Asset);
        AssertAssetPropertyValidity(catalogChange.Asset!, expectedAsset, expectedAsset.FullPath, assetsDirectory, folder);
        Assert.IsNull(catalogChange.Folder);
        Assert.AreEqual(expectedAssets.Count, catalogChange.CataloguedAssetsByPath.Count);
        Assert.AreEqual(ReasonEnum.AssetDeleted, catalogChange.Reason);
        Assert.AreEqual($"Image {expectedAsset.FullPath} deleted from catalog.", catalogChange.Message);
        Assert.IsNull(catalogChange.Exception);
        increment++;
    }

    private static void CheckCatalogChangesEnd(IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges, ref int increment)
    {
        int baseIncrement = increment;
        for (int i = increment; i < baseIncrement + 2; i++)
        {
            CatalogChangeCallbackEventArgs catalogChange = catalogChanges[i];
            Assert.IsNull(catalogChange.Asset);
            Assert.IsNull(catalogChange.Folder);
            Assert.AreEqual(0, catalogChange.CataloguedAssetsByPath.Count);
            Assert.AreEqual(ReasonEnum.AssetCreated, catalogChange.Reason);
            Assert.AreEqual(string.Empty, catalogChange.Message);
            Assert.IsNull(catalogChange.Exception);
            increment++;
        }
    }

    private static void RemoveDatabaseBackup(
        List<Folder> folders,
        string blobsPath,
        string tablesPath,
        string backupFilePath)
    {

        // Delete all blobs in Blobs directory
        foreach (Folder folder in folders)
        {
            string blobFileName = $"{folder.FolderId}.bin";
            string blobFilePath = Path.Combine(blobsPath, blobFileName);

            File.Delete(blobFilePath);
            Assert.IsFalse(File.Exists(blobFilePath));
        }

        // Delete all tables in Tables directory
        string assetsTablePath = Path.Combine(tablesPath, "assets.db");
        string foldersTablePath = Path.Combine(tablesPath, "folders.db");
        string syncAssetsDirectoriesDefinitionsTablePath = Path.Combine(tablesPath, "syncassetsdirectoriesdefinitions.db");
        string recentTargetPathsTablePath = Path.Combine(tablesPath, "recenttargetpaths.db");

        File.Delete(assetsTablePath);
        File.Delete(foldersTablePath);
        File.Delete(syncAssetsDirectoriesDefinitionsTablePath);
        File.Delete(recentTargetPathsTablePath);

        Assert.IsFalse(File.Exists(assetsTablePath));
        Assert.IsFalse(File.Exists(foldersTablePath));
        Assert.IsFalse(File.Exists(syncAssetsDirectoriesDefinitionsTablePath));
        Assert.IsFalse(File.Exists(recentTargetPathsTablePath));

        // Delete ZIP file in backup
        File.Delete(backupFilePath);
        Assert.IsFalse(File.Exists(backupFilePath));
    }

    // private void CheckCatalogChanges(List<CatalogChangeCallbackEventArgs> catalogChanges)
    // {
    //     List<Asset> processedAssets = catalogChanges.Where(s => s.Asset != null).Select(s => s.Asset!).ToList();
    //     // TODO: beautify this code
    //     Dictionary<string, List<Asset>> cataloguedAssets = [];
    //
    //     foreach (CatalogChangeCallbackEventArgs catalogChange in catalogChanges)
    //     {
    //         if (catalogChange.CataloguedAssets.Count > 0)
    //         {
    //             cataloguedAssets.Add(catalogChange.Asset?.FileName ?? string.Empty, catalogChange.CataloguedAssets);
    //         }
    //     }
    //
    //     // TODO: Check the content of cataloguedAssets when in working cases with synced assets
    //
    //     List<Exception> exceptions = catalogChanges.Where(s => s.Exception != null).Select(s => s.Exception!).ToList();
    //     Asset[] repositoryAssets = _testableAssetRepository!.GetAssetsByPath(_dataDirectory!);
    //
    //     Assert.AreEqual(repositoryAssets.Length, processedAssets.Count); // TODO: Make a fileList ? -> OneTimeSetup fileList.AddRange(jpegFiles); ... 
    //     Assert.IsEmpty(exceptions);
    // }
}
