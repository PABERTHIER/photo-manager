using PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

namespace PhotoManager.Tests.Integration.Domain;

[TestFixture]
public class AssetCreationServiceTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;
    private const string DATABASE_END_PATH = "v1.0";
    private const string FFMPEG_PATH = "E:\\ffmpeg\\bin\\ffmpeg.exe"; // TODO: Will be removed when the dll of Ffmpeg would have been generated and stored in the project

    private AssetCreationService? _assetCreationService;
    private Database? _database;
    private UserConfigurationService? _userConfigurationService;
    private TestableAssetRepository? _testableAssetRepository;
    private Mock<IStorageService>? _storageServiceMock;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        _databaseDirectory = Path.Combine(_dataDirectory, "DatabaseTests");
        _databasePath = Path.Combine(_databaseDirectory, DATABASE_END_PATH);

        _storageServiceMock = new Mock<IStorageService>();
        _storageServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<double>())).Returns(_databasePath);

        _database = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
    }

    private void ConfigureAssetCreationService(int thumbnailMaxWidth, int thumbnailMaxHeight, bool usingDHash, bool usingMD5Hash, bool usingPHash, bool analyseVideos)
    {
        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock.MockGetValue(UserConfigurationKeys.ASSETS_DIRECTORY, _dataDirectory!); // Only needed for videos
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
        _assetCreationService = new (_testableAssetRepository, storageService, assetHashCalculatorService, _userConfigurationService);
    }

    [Test]
    [TestCase("Image 1.jpg", 29857, 720, 1280, 200, 112, Rotation.Rotate0, "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9", false, null, false, null, 2097, "")]
    [TestCase("Image 1_90_deg.jpg", 31535, 1280, 720, 84, 150, Rotation.Rotate90, "0092081bd0a83e9253f4eb319b4cfab197a09ed3b28b5697c31f17bf2646638dec95bf28d4f454dda8ff3dcdaab33fa002774f13dfe6d7aee82f1a960e3e3aee", false, null, true, "The asset has been rotated", 1728, "")]
    [TestCase("Image 1_180_deg.jpg", 31535, 720, 1280, 200, 112, Rotation.Rotate180, "399383d82d4b7ed8233f714a1d14272b276b80505d7b3a2935b6ec962fd5cf280ccedb3e9380df1a11ee65e5f5fa66278dd2f407b7b6bdfa3093319164c446dd", false, null, true, "The asset has been rotated", 2098, "")]
    [TestCase("Image 1_270_deg.jpg", 31535, 1280, 720, 84, 150, Rotation.Rotate270, "43950e06cecd509509d868a2064a828aeb6ff6a9a30f296ec9a3621b269742e75dacb40300aa6927c4c8f0879268e1ab3d7eac228b38cade35cc9b7c6adf3449", false, null, true, "The asset has been rotated", 1709, "")]
    [TestCase("Image 2.jpg", 30197, 720, 1280, 200, 112, Rotation.Rotate0, "0b6d010f85544871c307bb3a96028402f55fa29094908cdd0f74a8ec8d3fc3d4fbec995d98b89aafef3dcf5581c018fbb50481e33c7e45aef552d66c922f4078", false, null, false, null, 2106, "")]
    [TestCase("Image 2 duplicated.jpg", 30197, 720, 1280, 200, 112, Rotation.Rotate0, "0b6d010f85544871c307bb3a96028402f55fa29094908cdd0f74a8ec8d3fc3d4fbec995d98b89aafef3dcf5581c018fbb50481e33c7e45aef552d66c922f4078", false, null, false, null, 2106, "")]
    [TestCase("Image 8.jpeg", 30514, 720, 1280, 200, 112, Rotation.Rotate0, "156f81ceb3db1bfdf27618efe4b89575f17d4708053c1a9ce6c5dfb7f36a6b958a5c3f45147728de28cf538345336e7b81d01b3f5908d4677ce76e1661e97ac3", false, null, false, null, 2112, "")]
    [TestCase("Image 9.png", 126277, 720, 1280, 200, 112, Rotation.Rotate0, "bcc994c14aa314dbc2dfbf48ffd34fa628dadcd86cdb8efda113b94a9035f15956cf039f5858b74cd7f404e98f7e84d9821b39aaa6cbbdc73228fa74ad2a5c20", false, null, false, null, 11002, "")]
    [TestCase("Image 10 portrait.png", 153318, 1280, 720, 84, 150, Rotation.Rotate0, "7ad01e9fe639957a9e8eaddc7fd864068f4a03f9981fd480f310740a43a0a4f2b8fa7a80d9c83039c46fcfbb63a5e465adaf07d33191369590adcda1586b1c94", false, null, false, null, 6076, "")]
    [TestCase("Image_11.heic", 1411940, 4032, 3024, 112, 150, Rotation.Rotate0, "f52bd860f5ad7f81a92919e5fb5769d3e86778b2ade74832fbd3029435c85e59cb64b3c2ce425445a49917953e6e913c72b81e48976041a4439cb65e92baf18d", false, null, false, null, 5831, "")]
    [TestCase("Image_11_90.heic", 1411940, 3024, 4032, 200, 150, Rotation.Rotate90, "b75df4942de1efa7d4326fedd721e99cc43959a1c22098a8cfcf6861aea099741d9c98fa1615e54d53838c22b9c6ff3b328c4ee3c0875d2b4ac7f7dee8d61bcf", false, null, true, "The asset has been rotated", 5868, "")]
    [TestCase("Image_11_180.heic", 1423616, 4032, 3024, 112, 150, Rotation.Rotate180, "a2ad3ac9d51f74230b9f1d554172781d9cc214d01f8344354e2e56dc3daecb5a0021d4d2814eba0cde6fd681f3a8e8b8b55fbfad7184740e3518190e05dca77f", false, null, true, "The asset has been rotated", 5694, "")]
    [TestCase("Image_11_270.heic", 1423616, 3024, 4032, 200, 150, Rotation.Rotate270, "d4d7342a783f0f1b5e10ad9b493b71dd688dbc31b8ddd5534c9fff9c5fdbfa8bee32ef4e4258f7514a1b57f9a12b571454c8b66172b1a66bf1b53953b66227d5", false, null, true, "The asset has been rotated", 6038, "")]
    [TestCase("IMAGE_WITH_UPPERCASE_NAME.JPG", 40937, 720, 1280, 200, 112, Rotation.Rotate0, "0465fc2af396e003a3f45bf71e951b905ca7d19c21c36d47cd7b2524190862e38cb4c0da6fdd1dbd89b5b7050d837be664bc45693657ec7681e4d9b648044c91", false, null, false, null, 3013, "")]
    [TestCase("Homer.gif", 64123, 320, 320, 150, 150, Rotation.Rotate0, "c48b1f61f3a3a004f425d8493d30a50ae14408ed4c5354bf4d0ca40069f91951381a7df32ee7455a6edef0996c95571557a9993021331ff2dfbc3ccc7f0c8ff1", false, null, false, null, 8594, "")]
    [TestCase("1336_BottomLeftPart.JPG", 1119873, 1512, 2016, 200, 150, Rotation.Rotate0, "c50e17cee9a52c0235afb410c361d094daf7330c20e4b3f3cb3e3e18683793355e903f84d49bf1db4af7e094980204cc005aa3c503829d14c9e6cd19826e284f", false, null, false, null, 13124, "Duplicates\\Part")]
    [TestCase("1336_BottomPart.JPG", 2172866, 1512, 4032, 200, 75, Rotation.Rotate0, "aef59f88b3c54a5fd1bfc098acac1d662bb3799e83220049ee7c3ae5def487a1464112cce7b7384428ee9b69813448d509413a1fc1abcddc272ec2e044c5f876", false, null, false, null, 7556, "Duplicates\\Part")]
    [TestCase("1336_BottomRightPart.JPG", 1061673, 1512, 2016, 200, 150, Rotation.Rotate0, "966370f38d7864499453b99934bb51f1654e567033e637b68ded1ec12c338eaa03d42bb4dc74621551c5dfd922180a291f7c58980bf6c94ae114e4d296a484e4", false, null, false, null, 13256, "Duplicates\\Part")]
    [TestCase("1336_LeftPart.JPG", 1475892, 3024, 2016, 100, 150, Rotation.Rotate0, "8afa3f673b047d5096ed2a2528b37c7222ab5b44cf780a7920cea35228e93323709b09b1a2ffe1bd2f789061be9bef8ebcb642fc47a2f3b6573a68a8cd5045ee", false, null, false, null, 5807, "Duplicates\\Part")]
    [TestCase("1336_Original.JPG", 4526710, 3024, 4032, 200, 150, Rotation.Rotate0, "fb0bb3887c28d022113c5c52a65f9da14eab0806842f902b618806dedeb7c53da1ed26ae5bcaf9a358e81efead71c4807ea38054e44f426857f8bf2c1fd1c7aa", false, null, false, null, 10736, "Duplicates\\Part")]
    [TestCase("1336_RightPart.JPG", 1437903, 3024, 2016, 100, 150, Rotation.Rotate0, "0e0e12ea70894ad521c8d3da14b92bbd4e7c7c03010502d779f2e485ea30e917ad6a9bc0435520f70c3448fd96cd9ccf88cf95d2a75a6da669b0f41731aabbe7", false, null, false, null, 5911, "Duplicates\\Part")]
    [TestCase("1336_TopLeftPart.JPG", 362012, 1512, 2016, 200, 150, Rotation.Rotate0, "f59b3913bdd53b6cb485a2c04770d1153350df2caf6dabf38b8b8311fb3fb583becbf7d368a1e0987d3584251a64bca72042d334b6e6c25edb7dfaead2db383d", false, null, false, null, 5677, "Duplicates\\Part")]
    [TestCase("1336_TopPart.JPG", 737269, 1512, 4032, 200, 75, Rotation.Rotate0, "93d6ea1575ea8fd4f540b88ae3ce727d28c657a11388c9b6cf927572abffdcc467d3e9b43e622b34158c18ed680c841d43e17a80c7b544261f106d887b51d2d2", false, null, false, null, 4148, "Duplicates\\Part")]
    [TestCase("1336_TopRightPart.JPG", 383508, 1512, 2016, 200, 150, Rotation.Rotate0, "3fde05f0e85d71a909fa38351411f3d45305857ea7a920ad22bcffaf4cf50136b4a55acb128bdc9cd9d7db600332f83e4157926a511939b0b57f2e50c020e3d7", false, null, false, null, 5998, "Duplicates\\Part")]
    [TestCase("1336_1K.JPG", 311067, 768, 1024, 200, 150, Rotation.Rotate0, "4dcbbf63585dc9fff056a0370ee33efc3d7c57b556bce917f89c5012fbacbc1ee2929f7d3a11ee2f8af426b183212d67167d87e48b80707ab5a1fb4e766d2544", false, null, false, null, 10288, "Duplicates\\Resolution")]
    [TestCase("1336_2K.JPG", 1154116, 1536, 2048, 200, 150, Rotation.Rotate0, "76a8f0b611fdd32b73bff92a186449a7e4bf0b84d618fe8b9666bbf2b022ff6bf259a4a98be42db7d9043ff3c3ff20ae720c5067966c40d16ad3fc67769f514b", false, null, false, null, 10193, "Duplicates\\Resolution")]
    [TestCase("1336_3K.JPG", 2098339, 2160, 2880, 200, 150, Rotation.Rotate0, "98044e6674187219ab629c901dfdf5a72f2535104d83cd167d1bdd9039ded91b7a585b74c5c89860f2e0fab85af9e2181265cba48b908028a42b8ec042265f03", false, null, false, null, 10541, "Duplicates\\Resolution")]
    [TestCase("1336_4K_Original.JPG", 4526710, 3024, 4032, 200, 150, Rotation.Rotate0, "fb0bb3887c28d022113c5c52a65f9da14eab0806842f902b618806dedeb7c53da1ed26ae5bcaf9a358e81efead71c4807ea38054e44f426857f8bf2c1fd1c7aa", false, null, false, null, 10736, "Duplicates\\Resolution")]
    [TestCase("1336_8K.JPG", 8855421, 5760, 7680, 200, 150, Rotation.Rotate0, "18289324fb22b8b337b1f4a7d627c605056e04fd308699bc831af0579cbc2d6a5959811dbecce09b26f82c341ea82d02d5c99c1580f50c24106d19c88bd6eba2", false, null, false, null, 10801, "Duplicates\\Resolution")]
    [TestCase("1336_Thumbnail.JPG", 19472, 152, 202, 200, 150, Rotation.Rotate0, "0bbabdb2fe28d4ef53b78a917eb89938fe564d758ebb9cb011e5a675dbee0a8e92bc38b8fe0762e279d48e95e7d17f1bb392fd96e8db7006e9e0bb60c5d2ed31", false, null, false, null, 10846, "Duplicates\\Resolution")]
    [TestCase("Image_1336_Mini.JPG", 1586, 32, 43, 200, 149, Rotation.Rotate0, "768d0fd37e280f1ef8d2a17702626ca1b290521167c22b0aecadd3cc2f858a88ed59a72f7a4b36c123f487da69842e55e4407de0dd248076947ce7df53e92be4", false, null, false, null, 4731, "Duplicates\\Thumbnail")]
    [TestCase("Image_1336_Original.JPG", 4526710, 3024, 4032, 200, 150, Rotation.Rotate0, "fb0bb3887c28d022113c5c52a65f9da14eab0806842f902b618806dedeb7c53da1ed26ae5bcaf9a358e81efead71c4807ea38054e44f426857f8bf2c1fd1c7aa", false, null, false, null, 10736, "Duplicates\\Thumbnail")]
    [TestCase("Image_1336_ShitQuality.JPG", 370819, 3024, 4032, 200, 150, Rotation.Rotate0, "0861602a84f5bf56e5f4acf49c1d98da8129542e871208a9c9f25eb497cdc65a75ccf4941874c2be7f4e36309053fe34d61a42b4d1b266910918a0b68f19cfc2", false, null, false, null, 11264, "Duplicates\\Thumbnail")]
    [TestCase("Image_1336_Small.JPG", 53227, 443, 591, 200, 150, Rotation.Rotate0, "787146674307ab1e867c0961968dfea28aafc7d45e6cfd83f1562fbd201d029812289cd692c36a4f537c576b25af03c9f6f3cc996a8651f3c69f8240ecb3d5f4", false, null, false, null, 10566, "Duplicates\\Thumbnail")]
    public void CreateAsset_PictureAndBasicHashType_ReturnsAsset(
        string fileName,
        long fileSize,
        int pixelHeight,
        int pixelWidth,
        int thumbnailPixelWidth,
        int thumbnailPixelHeight,
        Rotation imageRotation,
        string hash,
        bool isAssetCorrupted,
        string assetCorruptedMessage,
        bool isAssetRotated,
        string assetRotatedMessage,
        int imageByteSize,
        string additionalPath)
    {
        ConfigureAssetCreationService(200, 150, false, false, false, false);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, additionalPath);
            Folder folder = _testableAssetRepository!.AddFolder(folderPath); // Set above, not in this method

            string imagePath = Path.Combine(folderPath, fileName);
            Assert.IsTrue(File.Exists(imagePath));

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            Asset? asset = _assetCreationService!.CreateAsset(folderPath, fileName);

            Assert.IsNotNull(asset);

            AssertAssetPropertyValidity(
                asset!,
                fileName,
                imagePath,
                folderPath,
                folder,
                fileSize,
                pixelHeight,
                pixelWidth,
                thumbnailPixelWidth,
                thumbnailPixelHeight,
                imageRotation,
                hash,
                isAssetCorrupted,
                assetCorruptedMessage,
                isAssetRotated,
                assetRotatedMessage);

            AssertCataloguedAssetAndThumbnailValidity(asset!, folder, thumbnails, imageByteSize);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase("Image 1.jpg", 29857, 720, 1280, 200, 112, Rotation.Rotate0, "afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee0afbaa849d28fc2b8dc1262d9e619b362ee0", false, null, false, null, 2097, "")]
    [TestCase("Image 1_90_deg.jpg", 31535, 1280, 720, 84, 150, Rotation.Rotate90, "afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee0afbaa849d28fc2b8dc1262d9e619b362ee0", false, null, true, "The asset has been rotated", 1728, "")]
    [TestCase("Image 1_180_deg.jpg", 31535, 720, 1280, 200, 112, Rotation.Rotate180, "afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee0afbaa849d28fc2b8dc1262d9e619b362ee0", false, null, true, "The asset has been rotated", 2098, "")]
    [TestCase("Image 1_270_deg.jpg", 31535, 1280, 720, 84, 150, Rotation.Rotate270, "afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee0afbaa849d28fc2b8dc1262d9e619b362ee0", false, null, true, "The asset has been rotated", 1709, "")]
    [TestCase("Image 2.jpg", 30197, 720, 1280, 200, 112, Rotation.Rotate0, "afba2849d18fc388dc1662da0619b362ee062ee062ee062ee062ee062ee062ee062ee0afba2849d18fc388dc1662da0619b362ee062ee062ee062ee062ee062ee062ee062ee0afba2849d18fc388dc1662da0619b362ee0afba2849d18fc388dc1662da0619b362ee0", false, null, false, null, 2106, "")]
    [TestCase("Image 2 duplicated.jpg", 30197, 720, 1280, 200, 112, Rotation.Rotate0, "afba2849d18fc388dc1662da0619b362ee062ee062ee062ee062ee062ee062ee062ee0afba2849d18fc388dc1662da0619b362ee062ee062ee062ee062ee062ee062ee062ee0afba2849d18fc388dc1662da0619b362ee0afba2849d18fc388dc1662da0619b362ee0", false, null, false, null, 2106, "")]
    [TestCase("Image 8.jpeg", 30514, 720, 1280, 200, 112, Rotation.Rotate0, "afb8a849cc8fc608dc2362da4619b462ee062ee062ee062ee062ee062ee062ee062ee0afb8a849cc8fc608dc2362da4619b462ee062ee062ee062ee062ee062ee062ee062ee0afb8a849cc8fc608dc2362da4619b462ee0afb8a849cc8fc608dc2362da4619b462ee0", false, null, false, null, 2112, "")]
    [TestCase("Image 9.png", 126277, 720, 1280, 200, 112, Rotation.Rotate0, "afb2d849b98fd038dc1062da9619b162ee062ee062ee062ee062ee062ee062ee062ee0afb2d849b98fd038dc1062da9619b162ee062ee062ee062ee062ee062ee062ee062ee0afb2d849b98fd038dc1062da9619b162ee0afb2d849b98fd038dc1062da9619b162ee0", false, null, false, null, 11002, "")]
    [TestCase("Image 10 portrait.png", 153318, 1280, 720, 84, 150, Rotation.Rotate0, "afaff849b08fd348dc1f62dae619b262ee062ee062ee062ee062ee062ee062ee062ee0afaff849b08fd348dc1f62dae619b262ee062ee062ee062ee062ee062ee062ee062ee0afaff849b08fd348dc1f62dae619b262ee0afaff849b08fd348dc1f62dae619b262ee0", false, null, false, null, 6076, "")]
    [TestCase("Image_11.heic", 1411940, 4032, 3024, 112, 150, Rotation.Rotate0, "a928c84a96881408945461cb98d6366207162ee062ee062ee062ee062ee062ee062ee0a928c84a96881408945461cb98d6366207162ee062ee062ee062ee062ee062ee062ee0a928c84a96881408945461cb98d63662071a928c84a96881408945461cb98d63662071", false, null, false, null, 5831, "")]
    [TestCase("Image_11_90.heic", 1411940, 3024, 4032, 200, 150, Rotation.Rotate90, "a928f84a98881418945f61cbb8d6606206a62ee062ee062ee062ee062ee062ee062ee0a928f84a98881418945f61cbb8d6606206a62ee062ee062ee062ee062ee062ee062ee0a928f84a98881418945f61cbb8d6606206aa928f84a98881418945f61cbb8d6606206a", false, null, true, "The asset has been rotated", 5868, "")]
    [TestCase("Image_11_180.heic", 1423616, 4032, 3024, 112, 150, Rotation.Rotate180, "a928e84a97881448945561cb98d6386206f62ee062ee062ee062ee062ee062ee062ee0a928e84a97881448945561cb98d6386206f62ee062ee062ee062ee062ee062ee062ee0a928e84a97881448945561cb98d6386206fa928e84a97881448945561cb98d6386206f", false, null, true, "The asset has been rotated", 5694, "")]
    [TestCase("Image_11_270.heic", 1423616, 3024, 4032, 200, 150, Rotation.Rotate270, "a929184a99881458946061cbb8d6606206962ee062ee062ee062ee062ee062ee062ee0a929184a99881458946061cbb8d6606206962ee062ee062ee062ee062ee062ee062ee0a929184a99881458946061cbb8d66062069a929184a99881458946061cbb8d66062069", false, null, true, "The asset has been rotated", 6038, "")]
    [TestCase("IMAGE_WITH_UPPERCASE_NAME.JPG", 40937, 720, 1280, 200, 112, Rotation.Rotate0, "af946849858fab58dc6b62d9a619b962ee062ee062ee062ee062ee062ee062ee062ee0af946849858fab58dc6b62d9a619b962ee062ee062ee062ee062ee062ee062ee062ee0af946849858fab58dc6b62d9a619b962ee0af946849858fab58dc6b62d9a619b962ee0", false, null, false, null, 3013, "")]
    [TestCase("Homer.gif", 64123, 320, 320, 150, 150, Rotation.Rotate0, "af783881028a11d8ad696231b8ee326225d62ee062ee062ee062ee062ee062ee062ee0af783881028a11d8ad696231b8ee326225d62ee062ee062ee062ee062ee062ee062ee0af783881028a11d8ad696231b8ee326225daf783881028a11d8ad696231b8ee326225d", false, null, false, null, 8594, "")]
    [TestCase("1336_BottomLeftPart.JPG", 1119873, 1512, 2016, 200, 150, Rotation.Rotate0, "a939484b35885058815f61a1f8a70361c4562ee062ee062ee062ee062ee062ee062ee0a939484b35885058815f61a1f8a70361c4562ee062ee062ee062ee062ee062ee062ee0a939484b35885058815f61a1f8a70361c45a939484b35885058815f61a1f8a70361c45", false, null, false, null, 13124, "Duplicates\\Part")]
    [TestCase("1336_BottomPart.JPG", 2172866, 1512, 4032, 200, 75, Rotation.Rotate0, "a3cb9ad75a86e60869ba8da5c876de8dd2f62ee062ee062ee062ee062ee062ee062ee0a3cb9ad75a86e60869ba8da5c876de8dd2f62ee062ee062ee062ee062ee062ee062ee0a3cb9ad75a86e60869ba8da5c876de8dd2fa3cb9ad75a86e60869ba8da5c876de8dd2f", false, null, false, null, 7556, "Duplicates\\Part")]
    [TestCase("1336_BottomRightPart.JPG", 1061673, 1512, 2016, 200, 150, Rotation.Rotate0, "a757a847f2873ca86abd8dc848a10a8e58f62ee062ee062ee062ee062ee062ee062ee0a757a847f2873ca86abd8dc848a10a8e58f62ee062ee062ee062ee062ee062ee062ee0a757a847f2873ca86abd8dc848a10a8e58fa757a847f2873ca86abd8dc848a10a8e58f", false, null, false, null, 13256, "Duplicates\\Part")]
    [TestCase("1336_LeftPart.JPG", 1475892, 3024, 2016, 100, 150, Rotation.Rotate0, "ac9c684f6c8797587bc88f6668a37d6207262ee062ee062ee062ee062ee062ee062ee0ac9c684f6c8797587bc88f6668a37d6207262ee062ee062ee062ee062ee062ee062ee0ac9c684f6c8797587bc88f6668a37d62072ac9c684f6c8797587bc88f6668a37d62072", false, null, false, null, 5807, "Duplicates\\Part")]
    [TestCase("1336_Original.JPG", 4526710, 3024, 4032, 200, 150, Rotation.Rotate0, "abf9e847058af2e892da61f508b70361f7162ee062ee062ee062ee062ee062ee062ee0abf9e847058af2e892da61f508b70361f7162ee062ee062ee062ee062ee062ee062ee0abf9e847058af2e892da61f508b70361f71abf9e847058af2e892da61f508b70361f71", false, null, false, null, 10736, "Duplicates\\Part")]
    [TestCase("1336_RightPart.JPG", 1437903, 3024, 2016, 100, 150, Rotation.Rotate0, "ac49784f3886c9286e8b8dcb9896648f28662ee062ee062ee062ee062ee062ee062ee0ac49784f3886c9286e8b8dcb9896648f28662ee062ee062ee062ee062ee062ee062ee0ac49784f3886c9286e8b8dcb9896648f286ac49784f3886c9286e8b8dcb9896648f286", false, null, false, null, 5911, "Duplicates\\Part")]
    [TestCase("1336_TopLeftPart.JPG", 362012, 1512, 2016, 200, 150, Rotation.Rotate0, "ade578542a8bf548bb8b6261a8e6c36268562ee062ee062ee062ee062ee062ee062ee0ade578542a8bf548bb8b6261a8e6c36268562ee062ee062ee062ee062ee062ee062ee0ade578542a8bf548bb8b6261a8e6c362685ade578542a8bf548bb8b6261a8e6c362685", false, null, false, null, 5677, "Duplicates\\Part")]
    [TestCase("1336_TopPart.JPG", 737269, 1512, 4032, 200, 75, Rotation.Rotate0, "a9d838286c8b5668b5f3625668cbfa624a862ee062ee062ee062ee062ee062ee062ee0a9d838286c8b5668b5f3625668cbfa624a862ee062ee062ee062ee062ee062ee062ee0a9d838286c8b5668b5f3625668cbfa624a8a9d838286c8b5668b5f3625668cbfa624a8", false, null, false, null, 4148, "Duplicates\\Part")]
    [TestCase("1336_TopRightPart.JPG", 383508, 1512, 2016, 200, 150, Rotation.Rotate0, "ae3968548a8c4158c2f562b528f01e6270e62ee062ee062ee062ee062ee062ee062ee0ae3968548a8c4158c2f562b528f01e6270e62ee062ee062ee062ee062ee062ee062ee0ae3968548a8c4158c2f562b528f01e6270eae3968548a8c4158c2f562b528f01e6270e", false, null, false, null, 5998, "Duplicates\\Part")]
    [TestCase("1336_1K.JPG", 311067, 768, 1024, 200, 150, Rotation.Rotate0, "abf0a846dd8aef48929a61f458b6af61f6262ee062ee062ee062ee062ee062ee062ee0abf0a846dd8aef48929a61f458b6af61f6262ee062ee062ee062ee062ee062ee062ee0abf0a846dd8aef48929a61f458b6af61f62abf0a846dd8aef48929a61f458b6af61f62", false, null, false, null, 10288, "Duplicates\\Resolution")]
    [TestCase("1336_2K.JPG", 1154116, 1536, 2048, 200, 150, Rotation.Rotate0, "abf0b846dd8aef18929961f458b6af61f6262ee062ee062ee062ee062ee062ee062ee0abf0b846dd8aef18929961f458b6af61f6262ee062ee062ee062ee062ee062ee062ee0abf0b846dd8aef18929961f458b6af61f62abf0b846dd8aef18929961f458b6af61f62", false, null, false, null, 10193, "Duplicates\\Resolution")]
    [TestCase("1336_3K.JPG", 2098339, 2160, 2880, 200, 150, Rotation.Rotate0, "abe7a846b68aeb78925961f3a8b65a61f5462ee062ee062ee062ee062ee062ee062ee0abe7a846b68aeb78925961f3a8b65a61f5462ee062ee062ee062ee062ee062ee062ee0abe7a846b68aeb78925961f3a8b65a61f54abe7a846b68aeb78925961f3a8b65a61f54", false, null, false, null, 10541, "Duplicates\\Resolution")]
    [TestCase("1336_4K_Original.JPG", 4526710, 3024, 4032, 200, 150, Rotation.Rotate0, "abf9e847058af2e892da61f508b70361f7162ee062ee062ee062ee062ee062ee062ee0abf9e847058af2e892da61f508b70361f7162ee062ee062ee062ee062ee062ee062ee0abf9e847058af2e892da61f508b70361f71abf9e847058af2e892da61f508b70361f71", false, null, false, null, 10736, "Duplicates\\Resolution")]
    [TestCase("1336_8K.JPG", 8855421, 5760, 7680, 200, 150, Rotation.Rotate0, "abf6a846f88af0c892c361f4d8b6e661f6a62ee062ee062ee062ee062ee062ee062ee0abf6a846f88af0c892c361f4d8b6e661f6a62ee062ee062ee062ee062ee062ee062ee0abf6a846f88af0c892c361f4d8b6e661f6aabf6a846f88af0c892c361f4d8b6e661f6a", false, null, false, null, 10801, "Duplicates\\Resolution")]
    [TestCase("1336_Thumbnail.JPG", 19472, 152, 202, 200, 150, Rotation.Rotate0, "abf0f846de8aeee8929b61f478b6b061f6062ee062ee062ee062ee062ee062ee062ee0abf0f846de8aeee8929b61f478b6b061f6062ee062ee062ee062ee062ee062ee062ee0abf0f846de8aeee8929b61f478b6b061f60abf0f846de8aeee8929b61f478b6b061f60", false, null, false, null, 10846, "Duplicates\\Resolution")]
    [TestCase("Image_1336_Mini.JPG", 1586, 32, 43, 200, 149, Rotation.Rotate0, "abd4f8464c8af0d8926261f378b61d61f6362ee062ee062ee062ee062ee062ee062ee0abd4f8464c8af0d8926261f378b61d61f6362ee062ee062ee062ee062ee062ee062ee0abd4f8464c8af0d8926261f378b61d61f63abd4f8464c8af0d8926261f378b61d61f63", false, null, false, null, 4731, "Duplicates\\Thumbnail")]
    [TestCase("Image_1336_Original.JPG", 4526710, 3024, 4032, 200, 150, Rotation.Rotate0, "abf9e847058af2e892da61f508b70361f7162ee062ee062ee062ee062ee062ee062ee0abf9e847058af2e892da61f508b70361f7162ee062ee062ee062ee062ee062ee062ee0abf9e847058af2e892da61f508b70361f71abf9e847058af2e892da61f508b70361f71", false, null, false, null, 10736, "Duplicates\\Thumbnail")]
    [TestCase("Image_1336_ShitQuality.JPG", 370819, 3024, 4032, 200, 150, Rotation.Rotate0, "abd4c846458af188927261f388b63261f6962ee062ee062ee062ee062ee062ee062ee0abd4c846458af188927261f388b63261f6962ee062ee062ee062ee062ee062ee062ee0abd4c846458af188927261f388b63261f69abd4c846458af188927261f388b63261f69", false, null, false, null, 11264, "Duplicates\\Thumbnail")]
    [TestCase("Image_1336_Small.JPG", 53227, 443, 591, 200, 150, Rotation.Rotate0, "abd60846478ae848929b61f458b66761f5862ee062ee062ee062ee062ee062ee062ee0abd60846478ae848929b61f458b66761f5862ee062ee062ee062ee062ee062ee062ee0abd60846478ae848929b61f458b66761f58abd60846478ae848929b61f458b66761f58", false, null, false, null, 10566, "Duplicates\\Thumbnail")]
    public void CreateAsset_PictureAndPHashType_ReturnsAsset(
        string fileName,
        long fileSize,
        int pixelHeight,
        int pixelWidth,
        int thumbnailPixelWidth,
        int thumbnailPixelHeight,
        Rotation imageRotation,
        string hash,
        bool isAssetCorrupted,
        string assetCorruptedMessage,
        bool isAssetRotated,
        string assetRotatedMessage,
        int imageByteSize,
        string additionalPath)
    {
        ConfigureAssetCreationService(200, 150, false, false, true, false);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, additionalPath);
            Folder folder = _testableAssetRepository!.AddFolder(folderPath); // Set above, not in this method

            string imagePath = Path.Combine(folderPath, fileName);
            Assert.IsTrue(File.Exists(imagePath));

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            Asset? asset = _assetCreationService!.CreateAsset(folderPath, fileName);

            Assert.IsNotNull(asset);

            AssertAssetPropertyValidity(
                asset!,
                fileName,
                imagePath,
                folderPath,
                folder,
                fileSize,
                pixelHeight,
                pixelWidth,
                thumbnailPixelWidth,
                thumbnailPixelHeight,
                imageRotation,
                hash,
                isAssetCorrupted,
                assetCorruptedMessage,
                isAssetRotated,
                assetRotatedMessage);

            AssertCataloguedAssetAndThumbnailValidity(asset!, folder, thumbnails, imageByteSize);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase("Image 1.jpg", 29857, 720, 1280, 200, 112, Rotation.Rotate0, "23831507941108244", false, null, false, null, 2097, "")]
    [TestCase("Image 1_90_deg.jpg", 31535, 1280, 720, 84, 150, Rotation.Rotate90, "23831507941108244", false, null, true, "The asset has been rotated", 1728, "")]
    [TestCase("Image 1_180_deg.jpg", 31535, 720, 1280, 200, 112, Rotation.Rotate180, "23831507941108244", false, null, true, "The asset has been rotated", 2098, "")]
    [TestCase("Image 1_270_deg.jpg", 31535, 1280, 720, 84, 150, Rotation.Rotate270, "23831507941108244", false, null, true, "The asset has been rotated", 1709, "")]
    [TestCase("Image 2.jpg", 30197, 720, 1280, 200, 112, Rotation.Rotate0, "23831507941108244", false, null, false, null, 2106, "")]
    [TestCase("Image 2 duplicated.jpg", 30197, 720, 1280, 200, 112, Rotation.Rotate0, "23831507941108244", false, null, false, null, 2106, "")]
    [TestCase("Image 8.jpeg", 30514, 720, 1280, 200, 112, Rotation.Rotate0, "23831507941108244", false, null, false, null, 2112, "")]
    [TestCase("Image 9.png", 126277, 720, 1280, 200, 112, Rotation.Rotate0, "9346989754624", false, null, false, null, 11002, "")]
    [TestCase("Image 10 portrait.png", 153318, 1280, 720, 84, 150, Rotation.Rotate0, "36116814863094786", false, null, false, null, 6076, "")]
    [TestCase("Image_11.heic", 1411940, 4032, 3024, 112, 150, Rotation.Rotate0, "0", false, null, false, null, 5831, "")]
    [TestCase("Image_11_90.heic", 1411940, 3024, 4032, 200, 150, Rotation.Rotate90, "14766675108600273", false, null, true, "The asset has been rotated", 5868, "")]
    [TestCase("Image_11_180.heic", 1423616, 4032, 3024, 112, 150, Rotation.Rotate180, "53334510583489094", false, null, true, "The asset has been rotated", 5694, "")]
    [TestCase("Image_11_270.heic", 1423616, 3024, 4032, 200, 150, Rotation.Rotate270, "0", false, null, true, "The asset has been rotated", 6038, "")]
    [TestCase("IMAGE_WITH_UPPERCASE_NAME.JPG", 40937, 720, 1280, 200, 112, Rotation.Rotate0, "23831507941108244", false, null, false, null, 3013, "")]
    [TestCase("Homer.gif", 64123, 320, 320, 150, 150, Rotation.Rotate0, "0", false, null, false, null, 8594, "")]
    [TestCase("1336_BottomLeftPart.JPG", 1119873, 1512, 2016, 200, 150, Rotation.Rotate0, "27162885008392837", false, null, false, null, 13124, "Duplicates\\Part")]
    [TestCase("1336_BottomPart.JPG", 2172866, 1512, 4032, 200, 75, Rotation.Rotate0, "27162885008392837", false, null, false, null, 7556, "Duplicates\\Part")]
    [TestCase("1336_BottomRightPart.JPG", 1061673, 1512, 2016, 200, 150, Rotation.Rotate0, "62467497692214892", false, null, false, null, 13256, "Duplicates\\Part")]
    [TestCase("1336_LeftPart.JPG", 1475892, 3024, 2016, 100, 150, Rotation.Rotate0, "9079767575972116", false, null, false, null, 5807, "Duplicates\\Part")]
    [TestCase("1336_Original.JPG", 4526710, 3024, 4032, 200, 150, Rotation.Rotate0, "1090783378", false, null, false, null, 10736, "Duplicates\\Part")]
    [TestCase("1336_RightPart.JPG", 1437903, 3024, 2016, 100, 150, Rotation.Rotate0, "6879371593265376", false, null, false, null, 5911, "Duplicates\\Part")]
    [TestCase("1336_TopLeftPart.JPG", 362012, 1512, 2016, 200, 150, Rotation.Rotate0, "4574570757752993", false, null, false, null, 5677, "Duplicates\\Part")]
    [TestCase("1336_TopPart.JPG", 737269, 1512, 4032, 200, 75, Rotation.Rotate0, "9079767575972116", false, null, false, null, 4148, "Duplicates\\Part")]
    [TestCase("1336_TopRightPart.JPG", 383508, 1512, 2016, 200, 150, Rotation.Rotate0, "6879371593265376", false, null, false, null, 5998, "Duplicates\\Part")]
    [TestCase("1336_1K.JPG", 311067, 768, 1024, 200, 150, Rotation.Rotate0, "1144041865478144", false, null, false, null, 10288, "Duplicates\\Resolution")]
    [TestCase("1336_2K.JPG", 1154116, 1536, 2048, 200, 150, Rotation.Rotate0, "3951120804249760", false, null, false, null, 10193, "Duplicates\\Resolution")]
    [TestCase("1336_3K.JPG", 2098339, 2160, 2880, 200, 150, Rotation.Rotate0, "54473113164973580", false, null, false, null, 10541, "Duplicates\\Resolution")]
    [TestCase("1336_4K_Original.JPG", 4526710, 3024, 4032, 200, 150, Rotation.Rotate0, "1090783378", false, null, false, null, 10736, "Duplicates\\Resolution")]
    [TestCase("1336_8K.JPG", 8855421, 5760, 7680, 200, 150, Rotation.Rotate0, "276890633", false, null, false, null, 10801, "Duplicates\\Resolution")]
    [TestCase("1336_Thumbnail.JPG", 19472, 152, 202, 200, 150, Rotation.Rotate0, "8421922", false, null, false, null, 10846, "Duplicates\\Resolution")]
    [TestCase("Image_1336_Mini.JPG", 1586, 32, 43, 200, 149, Rotation.Rotate0, "61348232627230616", false, null, false, null, 4731, "Duplicates\\Thumbnail")]
    [TestCase("Image_1336_Original.JPG", 4526710, 3024, 4032, 200, 150, Rotation.Rotate0, "1090783378", false, null, false, null, 10736, "Duplicates\\Thumbnail")]
    [TestCase("Image_1336_ShitQuality.JPG", 370819, 3024, 4032, 200, 150, Rotation.Rotate0, "0", false, null, false, null, 11264, "Duplicates\\Thumbnail")]
    [TestCase("Image_1336_Small.JPG", 53227, 443, 591, 200, 150, Rotation.Rotate0, "0", false, null, false, null, 10566, "Duplicates\\Thumbnail")]
    public void CreateAsset_PictureAndDHashType_ReturnsAsset(
        string fileName,
        long fileSize,
        int pixelHeight,
        int pixelWidth,
        int thumbnailPixelWidth,
        int thumbnailPixelHeight,
        Rotation imageRotation,
        string hash,
        bool isAssetCorrupted,
        string assetCorruptedMessage,
        bool isAssetRotated,
        string assetRotatedMessage,
        int imageByteSize,
        string additionalPath)
    {
        ConfigureAssetCreationService(200, 150, true, false, false, false);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, additionalPath);
            Folder folder = _testableAssetRepository!.AddFolder(folderPath); // Set above, not in this method

            string imagePath = Path.Combine(folderPath, fileName);
            Assert.IsTrue(File.Exists(imagePath));

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            Asset? asset = _assetCreationService!.CreateAsset(folderPath, fileName);

            Assert.IsNotNull(asset);

            AssertAssetPropertyValidity(
                asset!,
                fileName,
                imagePath,
                folderPath,
                folder,
                fileSize,
                pixelHeight,
                pixelWidth,
                thumbnailPixelWidth,
                thumbnailPixelHeight,
                imageRotation,
                hash,
                isAssetCorrupted,
                assetCorruptedMessage,
                isAssetRotated,
                assetRotatedMessage);

            AssertCataloguedAssetAndThumbnailValidity(asset!, folder, thumbnails, imageByteSize);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase("Image 1.jpg", 29857, 720, 1280, 200, 112, Rotation.Rotate0, "a886f237e1cf6da03fb793588fd79b64", false, null, false, null, 2097, "")]
    [TestCase("Image 1_90_deg.jpg", 31535, 1280, 720, 84, 150, Rotation.Rotate90, "e740c5b99b0c87d86b9b1b3f322dec7e", false, null, true, "The asset has been rotated", 1728, "")]
    [TestCase("Image 1_180_deg.jpg", 31535, 720, 1280, 200, 112, Rotation.Rotate180, "bc43ad1ebd118a2f9443b3199d4127c8", false, null, true, "The asset has been rotated", 2098, "")]
    [TestCase("Image 1_270_deg.jpg", 31535, 1280, 720, 84, 150, Rotation.Rotate270, "f8f724518021547c26ef9262f29f6b9a", false, null, true, "The asset has been rotated", 1709, "")]
    [TestCase("Image 2.jpg", 30197, 720, 1280, 200, 112, Rotation.Rotate0, "27007861db67443498b6aa0d00808b41", false, null, false, null, 2106, "")]
    [TestCase("Image 2 duplicated.jpg", 30197, 720, 1280, 200, 112, Rotation.Rotate0, "27007861db67443498b6aa0d00808b41", false, null, false, null, 2106, "")]
    [TestCase("Image 8.jpeg", 30514, 720, 1280, 200, 112, Rotation.Rotate0, "4194e88c247b5bbc7a1c6294cc795466", false, null, false, null, 2112, "")]
    [TestCase("Image 9.png", 126277, 720, 1280, 200, 112, Rotation.Rotate0, "05d29088572bd5a5970fc370c0ca8751", false, null, false, null, 11002, "")]
    [TestCase("Image 10 portrait.png", 153318, 1280, 720, 84, 150, Rotation.Rotate0, "02b09a63d382bc1a1f88afa125f3adb3", false, null, false, null, 6076, "")]
    [TestCase("Image_11.heic", 1411940, 4032, 3024, 112, 150, Rotation.Rotate0, "d9aa281b55316e10c767877dee265732", false, null, false, null, 5831, "")]
    [TestCase("Image_11_90.heic", 1411940, 3024, 4032, 200, 150, Rotation.Rotate90, "e3aeca5fc781bd52470f9186653d3a3f", false, null, true, "The asset has been rotated", 5868, "")]
    [TestCase("Image_11_180.heic", 1423616, 4032, 3024, 112, 150, Rotation.Rotate180, "9bf6a1a37331ba6f391b885b6dcc67a1", false, null, true, "The asset has been rotated", 5694, "")]
    [TestCase("Image_11_270.heic", 1423616, 3024, 4032, 200, 150, Rotation.Rotate270, "35d24aaf7f6fc8b9872f069a8c28ab78", false, null, true, "The asset has been rotated", 6038, "")]
    [TestCase("IMAGE_WITH_UPPERCASE_NAME.JPG", 40937, 720, 1280, 200, 112, Rotation.Rotate0, "b70782c592e3e88f16291acf31d1b133", false, null, false, null, 3013, "")]
    [TestCase("Homer.gif", 64123, 320, 320, 150, 150, Rotation.Rotate0, "a409ce713de9334117791b15a586dd0e", false, null, false, null, 8594, "")]
    [TestCase("1336_BottomLeftPart.JPG", 1119873, 1512, 2016, 200, 150, Rotation.Rotate0, "1691eb898f26c38b8b5d66b5d5ed71c6", false, null, false, null, 13124, "Duplicates\\Part")]
    [TestCase("1336_BottomPart.JPG", 2172866, 1512, 4032, 200, 75, Rotation.Rotate0, "bac5ead516448cc2a657ea75251ae88d", false, null, false, null, 7556, "Duplicates\\Part")]
    [TestCase("1336_BottomRightPart.JPG", 1061673, 1512, 2016, 200, 150, Rotation.Rotate0, "25830c1fb500800d426c8832d7fabb9e", false, null, false, null, 13256, "Duplicates\\Part")]
    [TestCase("1336_LeftPart.JPG", 1475892, 3024, 2016, 100, 150, Rotation.Rotate0, "55370d738498beff0963693512d2d94f", false, null, false, null, 5807, "Duplicates\\Part")]
    [TestCase("1336_Original.JPG", 4526710, 3024, 4032, 200, 150, Rotation.Rotate0, "bed3d05d409001724e5f6095c2acaa95", false, null, false, null, 10736, "Duplicates\\Part")]
    [TestCase("1336_RightPart.JPG", 1437903, 3024, 2016, 100, 150, Rotation.Rotate0, "01b35414fd99b583a5261ff83260b698", false, null, false, null, 5911, "Duplicates\\Part")]
    [TestCase("1336_TopLeftPart.JPG", 362012, 1512, 2016, 200, 150, Rotation.Rotate0, "58695d954982417e68b68975beff02c4", false, null, false, null, 5677, "Duplicates\\Part")]
    [TestCase("1336_TopPart.JPG", 737269, 1512, 4032, 200, 75, Rotation.Rotate0, "216eaa3b37a658bd0d288d858022c7e1", false, null, false, null, 4148, "Duplicates\\Part")]
    [TestCase("1336_TopRightPart.JPG", 383508, 1512, 2016, 200, 150, Rotation.Rotate0, "1ec7dfdec1b6e9aa8ad5f50aae638e0e", false, null, false, null, 5998, "Duplicates\\Part")]
    [TestCase("1336_1K.JPG", 311067, 768, 1024, 200, 150, Rotation.Rotate0, "80bbaf9483ef57ae043863fa954615c6", false, null, false, null, 10288, "Duplicates\\Resolution")]
    [TestCase("1336_2K.JPG", 1154116, 1536, 2048, 200, 150, Rotation.Rotate0, "83355feec0abe189e3b3f1bcff375151", false, null, false, null, 10193, "Duplicates\\Resolution")]
    [TestCase("1336_3K.JPG", 2098339, 2160, 2880, 200, 150, Rotation.Rotate0, "02433546c8d0b8a9b76f861077924f85", false, null, false, null, 10541, "Duplicates\\Resolution")]
    [TestCase("1336_4K_Original.JPG", 4526710, 3024, 4032, 200, 150, Rotation.Rotate0, "bed3d05d409001724e5f6095c2acaa95", false, null, false, null, 10736, "Duplicates\\Resolution")]
    [TestCase("1336_8K.JPG", 8855421, 5760, 7680, 200, 150, Rotation.Rotate0, "961b08d1e4b7d9ed07fd9c5b83d23edb", false, null, false, null, 10801, "Duplicates\\Resolution")]
    [TestCase("1336_Thumbnail.JPG", 19472, 152, 202, 200, 150, Rotation.Rotate0, "dd33165a8c3329683ca1ce2bb90ed1b2", false, null, false, null, 10846, "Duplicates\\Resolution")]
    [TestCase("Image_1336_Mini.JPG", 1586, 32, 43, 200, 149, Rotation.Rotate0, "051a784180ba8c0fce93ce0b81add76c", false, null, false, null, 4731, "Duplicates\\Thumbnail")]
    [TestCase("Image_1336_Original.JPG", 4526710, 3024, 4032, 200, 150, Rotation.Rotate0, "bed3d05d409001724e5f6095c2acaa95", false, null, false, null, 10736, "Duplicates\\Thumbnail")]
    [TestCase("Image_1336_ShitQuality.JPG", 370819, 3024, 4032, 200, 150, Rotation.Rotate0, "5ec22ad598e3518bf7a5b78344ee8543", false, null, false, null, 11264, "Duplicates\\Thumbnail")]
    [TestCase("Image_1336_Small.JPG", 53227, 443, 591, 200, 150, Rotation.Rotate0, "1f060a67916d271bd37c366d5e1287e8", false, null, false, null, 10566, "Duplicates\\Thumbnail")]
    public void CreateAsset_PictureAndMD5HashType_ReturnsAsset(
        string fileName,
        long fileSize,
        int pixelHeight,
        int pixelWidth,
        int thumbnailPixelWidth,
        int thumbnailPixelHeight,
        Rotation imageRotation,
        string hash,
        bool isAssetCorrupted,
        string assetCorruptedMessage,
        bool isAssetRotated,
        string assetRotatedMessage,
        int imageByteSize,
        string additionalPath)
    {
        ConfigureAssetCreationService(200, 150, false, true, false, false);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, additionalPath);
            Folder folder = _testableAssetRepository!.AddFolder(folderPath); // Set above, not in this method

            string imagePath = Path.Combine(folderPath, fileName);
            Assert.IsTrue(File.Exists(imagePath));

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            Asset? asset = _assetCreationService!.CreateAsset(folderPath, fileName);

            Assert.IsNotNull(asset);

            AssertAssetPropertyValidity(
                asset!,
                fileName,
                imagePath,
                folderPath,
                folder,
                fileSize,
                pixelHeight,
                pixelWidth,
                thumbnailPixelWidth,
                thumbnailPixelHeight,
                imageRotation,
                hash,
                isAssetCorrupted,
                assetCorruptedMessage,
                isAssetRotated,
                assetRotatedMessage);

            AssertCataloguedAssetAndThumbnailValidity(asset!, folder, thumbnails, imageByteSize);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [TestCase("1415.JPG", 2264917, 4032, 3024, 112, 150, Rotation.Rotate270, "5f122c429a160e0eb1df1a6b77cea6ee4c13b731ebff02c79646314d5bf57070d4fae13a1a8821927d7954f83ae706c937affb452ed32a38dc800e00ef0f6176", false, null, true, "The asset has been rotated", 5022, "Duplicates\\NotDuplicate\\Sample3")]
    public void CreateAsset_PreviouslyRotatedPictureAndBasicHashType_ReturnsAsset(
        string fileName,
        long fileSize,
        int pixelHeight,
        int pixelWidth,
        int thumbnailPixelWidth,
        int thumbnailPixelHeight,
        Rotation imageRotation,
        string hash,
        bool isAssetCorrupted,
        string assetCorruptedMessage,
        bool isAssetRotated,
        string assetRotatedMessage,
        int imageByteSize,
        string additionalPath)
    {
        ConfigureAssetCreationService(200, 150, false, false, false, false);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, additionalPath);
            Folder folder = _testableAssetRepository!.AddFolder(folderPath); // Set above, not in this method

            string imagePath = Path.Combine(folderPath, fileName);
            Assert.IsTrue(File.Exists(imagePath));

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            Asset? asset = _assetCreationService!.CreateAsset(folderPath, fileName);

            Assert.IsNotNull(asset);

            AssertAssetPropertyValidity(
                asset!,
                fileName,
                imagePath,
                folderPath,
                folder,
                fileSize,
                pixelHeight,
                pixelWidth,
                thumbnailPixelWidth,
                thumbnailPixelHeight,
                imageRotation,
                hash,
                isAssetCorrupted,
                assetCorruptedMessage,
                isAssetRotated,
                assetRotatedMessage);

            AssertCataloguedAssetAndThumbnailValidity(asset!, folder, thumbnails, imageByteSize);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase(150, 150, 150, 84, 1627)]
    [TestCase(200, 150, 200, 112, 2097)]
    [TestCase(150, 200, 150, 84, 1627)]
    [TestCase(0, 0, 0, 0, 29708)]
    [TestCase(1000, 10, 1000, 562, 21109)]
    [TestCase(10, 10000, 10, 6, 663)]
    [TestCase(150, 10000000, 150, 84, 1627)]
    [TestCase(-100, -100, -100, -56, 1223)]
    public void CreateAsset_PictureAndBasicHashTypeAndDifferentThumbnailSettings_ReturnsAsset(
        int thumbnailMaxWidth,
        int thumbnailMaxHeight,
        int expectedThumbnailPixelWidth,
        int expectedThumbnailPixelHeight,
        int expectedImageByteSize)
    {
        ConfigureAssetCreationService(thumbnailMaxWidth, thumbnailMaxHeight, false, false, false, false);

        Asset expectedAsset = new()
        {
            FileName = "Image 1.jpg",
            FileSize = 29857,
            PixelHeight = 720,
            PixelWidth = 1280,
            ThumbnailPixelWidth = expectedThumbnailPixelWidth,
            ThumbnailPixelHeight = expectedThumbnailPixelHeight,
            ImageRotation = Rotation.Rotate0,
            Hash = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9",
            IsAssetCorrupted = false,
            AssetCorruptedMessage = null,
            IsAssetRotated = false,
            AssetRotatedMessage = null
        };

        try
        {
            Folder folder = _testableAssetRepository!.AddFolder(_dataDirectory!); // Set above, not in this method

            string imagePath = Path.Combine(_dataDirectory!, expectedAsset.FileName);
            Assert.IsTrue(File.Exists(imagePath));

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            Asset? asset = _assetCreationService!.CreateAsset(_dataDirectory!, expectedAsset.FileName);

            Assert.IsNotNull(asset);

            AssertAssetPropertyValidity(
                asset!,
                expectedAsset.FileName,
                imagePath,
                _dataDirectory!,
                folder,
                expectedAsset.FileSize,
                expectedAsset.PixelHeight,
                expectedAsset.PixelWidth,
                expectedAsset.ThumbnailPixelWidth,
                expectedAsset.ThumbnailPixelHeight,
                expectedAsset.ImageRotation,
                expectedAsset.Hash,
                expectedAsset.IsAssetCorrupted,
                expectedAsset.AssetCorruptedMessage,
                expectedAsset.IsAssetRotated,
                expectedAsset.AssetRotatedMessage);

            AssertCataloguedAssetAndThumbnailValidity(asset!, folder, thumbnails, expectedImageByteSize);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase(150, 150, 84, 150, 6076)]
    [TestCase(200, 150, 84, 150, 6076)]
    [TestCase(150, 200, 112, 200, 11612)]
    [TestCase(0, 0, 0, 0, 153318)]
    [TestCase(1000, 10, 6, 10, 264)]
    [TestCase(10, 10000, 5625, 10000, 16119861)]
    [TestCase(10000000, 100, 56, 100, 2543)]
    [TestCase(-100, -100, -56, -100, 2564)]
    public void CreateAsset_PortraitPictureAndBasicHashTypeAndDifferentThumbnailSettings_ReturnsAsset(
        int thumbnailMaxWidth,
        int thumbnailMaxHeight,
        int expectedThumbnailPixelWidth,
        int expectedThumbnailPixelHeight,
        int expectedImageByteSize)
    {
        ConfigureAssetCreationService(thumbnailMaxWidth, thumbnailMaxHeight, false, false, false, false);

        Asset expectedAsset = new()
        {
            FileName = "Image 10 portrait.png",
            FileSize = 153318,
            PixelHeight = 1280,
            PixelWidth = 720,
            ThumbnailPixelWidth = expectedThumbnailPixelWidth,
            ThumbnailPixelHeight = expectedThumbnailPixelHeight,
            ImageRotation = Rotation.Rotate0,
            Hash = "7ad01e9fe639957a9e8eaddc7fd864068f4a03f9981fd480f310740a43a0a4f2b8fa7a80d9c83039c46fcfbb63a5e465adaf07d33191369590adcda1586b1c94",
            IsAssetCorrupted = false,
            AssetCorruptedMessage = null,
            IsAssetRotated = false,
            AssetRotatedMessage = null
        };

        try
        {
            Folder folder = _testableAssetRepository!.AddFolder(_dataDirectory!); // Set above, not in this method

            string imagePath = Path.Combine(_dataDirectory!, expectedAsset.FileName);
            Assert.IsTrue(File.Exists(imagePath));

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            Asset? asset = _assetCreationService!.CreateAsset(_dataDirectory!, expectedAsset.FileName);

            Assert.IsNotNull(asset);

            AssertAssetPropertyValidity(
                asset!,
                expectedAsset.FileName,
                imagePath,
                _dataDirectory!,
                folder,
                expectedAsset.FileSize,
                expectedAsset.PixelHeight,
                expectedAsset.PixelWidth,
                expectedAsset.ThumbnailPixelWidth,
                expectedAsset.ThumbnailPixelHeight,
                expectedAsset.ImageRotation,
                expectedAsset.Hash,
                expectedAsset.IsAssetCorrupted,
                expectedAsset.AssetCorruptedMessage,
                expectedAsset.IsAssetRotated,
                expectedAsset.AssetRotatedMessage);

            AssertCataloguedAssetAndThumbnailValidity(asset!, folder, thumbnails, expectedImageByteSize);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase(150, 150, 112, 150, 5831)]
    [TestCase(200, 150, 112, 150, 5831)]
    [TestCase(150, 200, 150, 200, 9535)]
    [TestCase(0, 0, 0, 0, 634)]
    [TestCase(1000, 10, 8, 10, 704)]
    [TestCase(10, 10000, 7500, 10000, 7144043)]
    [TestCase(100000, 100, 75, 100, 3173)]
    public void CreateAsset_HeicPictureAndBasicHashTypeAndDifferentThumbnailSettings_ReturnsAsset(
        int thumbnailMaxWidth,
        int thumbnailMaxHeight,
        int expectedThumbnailPixelWidth,
        int expectedThumbnailPixelHeight,
        int expectedImageByteSize)
    {
        ConfigureAssetCreationService(thumbnailMaxWidth, thumbnailMaxHeight, false, false, false, false);

        Asset expectedAsset = new()
        {
            FileName = "Image_11.heic",
            FileSize = 1411940,
            PixelHeight = 4032,
            PixelWidth = 3024,
            ThumbnailPixelWidth = expectedThumbnailPixelWidth,
            ThumbnailPixelHeight = expectedThumbnailPixelHeight,
            ImageRotation = Rotation.Rotate0,
            Hash = "f52bd860f5ad7f81a92919e5fb5769d3e86778b2ade74832fbd3029435c85e59cb64b3c2ce425445a49917953e6e913c72b81e48976041a4439cb65e92baf18d",
            IsAssetCorrupted = false,
            AssetCorruptedMessage = null,
            IsAssetRotated = false,
            AssetRotatedMessage = null
        };

        try
        {
            Folder folder = _testableAssetRepository!.AddFolder(_dataDirectory!); // Set above, not in this method

            string imagePath = Path.Combine(_dataDirectory!, expectedAsset.FileName);
            Assert.IsTrue(File.Exists(imagePath));

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            Asset? asset = _assetCreationService!.CreateAsset(_dataDirectory!, expectedAsset.FileName);

            Assert.IsNotNull(asset);

            AssertAssetPropertyValidity(
                asset!,
                expectedAsset.FileName,
                imagePath,
                _dataDirectory!,
                folder,
                expectedAsset.FileSize,
                expectedAsset.PixelHeight,
                expectedAsset.PixelWidth,
                expectedAsset.ThumbnailPixelWidth,
                expectedAsset.ThumbnailPixelHeight,
                expectedAsset.ImageRotation,
                expectedAsset.Hash,
                expectedAsset.IsAssetCorrupted,
                expectedAsset.AssetCorruptedMessage,
                expectedAsset.IsAssetRotated,
                expectedAsset.AssetRotatedMessage);

            AssertCataloguedAssetAndThumbnailValidity(asset!, folder, thumbnails, expectedImageByteSize);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void CreateAsset_PictureAndBasicHashTypeAndCreatingTwiceSameImageInDifferentLocations_ReturnsAssetAndCreatesBothAssets()
    {
        ConfigureAssetCreationService(200, 150, false, false, false, false);

        Asset expectedAsset = new()
        {
            FileName = "Image 1.jpg",
            FileSize = 29857,
            PixelHeight = 720,
            PixelWidth = 1280,
            ThumbnailPixelWidth = 200,
            ThumbnailPixelHeight = 112,
            ImageRotation = Rotation.Rotate0,
            Hash = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9",
            IsAssetCorrupted = false,
            AssetCorruptedMessage = null,
            IsAssetRotated = false,
            AssetRotatedMessage = null
        };

        const int imageByteSize = 2097;

        try
        {
            Folder folder = _testableAssetRepository!.AddFolder(_dataDirectory!); // Set above, not in this method

            string imagePath = Path.Combine(_dataDirectory!, expectedAsset.FileName);
            Assert.IsTrue(File.Exists(imagePath));

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            Asset? asset = _assetCreationService!.CreateAsset(_dataDirectory!, expectedAsset.FileName);

            Assert.IsNotNull(asset);

            AssertAssetPropertyValidity(
                asset!,
                expectedAsset.FileName,
                imagePath,
                _dataDirectory!,
                folder,
                expectedAsset.FileSize,
                expectedAsset.PixelHeight,
                expectedAsset.PixelWidth,
                expectedAsset.ThumbnailPixelWidth,
                expectedAsset.ThumbnailPixelHeight,
                expectedAsset.ImageRotation,
                expectedAsset.Hash,
                expectedAsset.IsAssetCorrupted,
                expectedAsset.AssetCorruptedMessage,
                expectedAsset.IsAssetRotated,
                expectedAsset.AssetRotatedMessage);

            string newSameAssetFolderPath = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder1");
            Folder newSameAssetFolder = _testableAssetRepository!.AddFolder(newSameAssetFolderPath); // Set above, not in this method

            string newSameImagePath = Path.Combine(newSameAssetFolderPath, expectedAsset.FileName);
            Assert.IsTrue(File.Exists(imagePath));

            Asset? newSameAsset = _assetCreationService!.CreateAsset(newSameAssetFolderPath, expectedAsset.FileName);

            Assert.IsNotNull(newSameAsset);

            AssertAssetPropertyValidity(
                newSameAsset!,
                expectedAsset.FileName,
                newSameImagePath,
                newSameAssetFolderPath,
                newSameAssetFolder,
                expectedAsset.FileSize,
                expectedAsset.PixelHeight,
                expectedAsset.PixelWidth,
                expectedAsset.ThumbnailPixelWidth,
                expectedAsset.ThumbnailPixelHeight,
                expectedAsset.ImageRotation,
                expectedAsset.Hash,
                expectedAsset.IsAssetCorrupted,
                expectedAsset.AssetCorruptedMessage,
                expectedAsset.IsAssetRotated,
                expectedAsset.AssetRotatedMessage);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(2, assetsFromRepository.Count);

            Asset firstAssetFromRepository = assetsFromRepository[0];

            Assert.AreEqual(asset!.FileName, firstAssetFromRepository.FileName);
            Assert.AreEqual(asset.FolderId, firstAssetFromRepository.FolderId);
            Assert.AreEqual(asset.Hash, firstAssetFromRepository.Hash);
            Assert.IsNull(firstAssetFromRepository.ImageData);

            Asset secondAssetFromRepository = assetsFromRepository[1];

            Assert.AreEqual(newSameAsset!.FileName, secondAssetFromRepository.FileName);
            Assert.AreEqual(newSameAsset.FolderId, secondAssetFromRepository.FolderId);
            Assert.AreEqual(newSameAsset.Hash, secondAssetFromRepository.Hash);
            Assert.IsNull(secondAssetFromRepository.ImageData);

            Assert.AreEqual(2, thumbnails.Count);
            Assert.IsTrue(thumbnails.ContainsKey(asset.Folder.Path));
            Assert.IsTrue(thumbnails.ContainsKey(newSameAsset.Folder.Path));

            Assert.AreEqual(1, thumbnails[folder.Path].Count);
            Assert.IsTrue(thumbnails[folder.Path].ContainsKey(asset.FileName));
            Assert.IsNotNull(thumbnails[folder.Path][asset.FileName]);
            Assert.AreEqual(imageByteSize, thumbnails[folder.Path][asset.FileName].Length);

            Assert.AreEqual(1, thumbnails[newSameAssetFolder.Path].Count);
            Assert.IsTrue(thumbnails[newSameAssetFolder.Path].ContainsKey(newSameAsset.FileName));
            Assert.IsNotNull(thumbnails[newSameAssetFolder.Path][newSameAsset.FileName]);
            Assert.AreEqual(imageByteSize, thumbnails[newSameAssetFolder.Path][newSameAsset.FileName].Length);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void CreateAsset_PictureAndBasicHashTypeAndCreatingTwiceSameImage_DoesNotAddTheSecondOneAndReturnsNull()
    {
        ConfigureAssetCreationService(200, 150, false, false, false, false);

        Asset expectedAsset = new()
        {
            FileName = "Image 1.jpg",
            FileSize = 29857,
            PixelHeight = 720,
            PixelWidth = 1280,
            ThumbnailPixelWidth = 200,
            ThumbnailPixelHeight = 112,
            ImageRotation = Rotation.Rotate0,
            Hash = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9",
            IsAssetCorrupted = false,
            AssetCorruptedMessage = null,
            IsAssetRotated = false,
            AssetRotatedMessage = null
        };

        const int imageByteSize = 2097;

        try
        {
            Folder folder = _testableAssetRepository!.AddFolder(_dataDirectory!); // Set above, not in this method

            string imagePath = Path.Combine(_dataDirectory!, expectedAsset.FileName);
            Assert.IsTrue(File.Exists(imagePath));

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            Asset? asset = _assetCreationService!.CreateAsset(_dataDirectory!, expectedAsset.FileName);

            Assert.IsNotNull(asset);

            AssertAssetPropertyValidity(
                asset!,
                expectedAsset.FileName,
                imagePath,
                _dataDirectory!,
                folder,
                expectedAsset.FileSize,
                expectedAsset.PixelHeight,
                expectedAsset.PixelWidth,
                expectedAsset.ThumbnailPixelWidth,
                expectedAsset.ThumbnailPixelHeight,
                expectedAsset.ImageRotation,
                expectedAsset.Hash,
                expectedAsset.IsAssetCorrupted,
                expectedAsset.AssetCorruptedMessage,
                expectedAsset.IsAssetRotated,
                expectedAsset.AssetRotatedMessage);

            AssertCataloguedAssetAndThumbnailValidity(asset!, folder, thumbnails, imageByteSize);

            Asset? newSameAsset = _assetCreationService!.CreateAsset(_dataDirectory!, expectedAsset.FileName);

            Assert.IsNull(newSameAsset);

            AssertCataloguedAssetAndThumbnailValidity(asset!, folder, thumbnails, imageByteSize);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase("Image 1.jpg", "Invalid_Image_1.jpg")]
    [TestCase("Homer.gif", "Invalid_Homer.gif")]
    [TestCase("Image 9.png", "Invalid_Image_9.png")]
    [TestCase("Image_11.heic", "Invalid_Image_11.heic")]
    public void CreateAsset_InvalidPicture_ReturnsNullAndDoesNotCreateAsset(string fileName, string invalidFileName)
    {
        ConfigureAssetCreationService(200, 150, false, false, false, false);
        string tempDirectory = Path.Combine(_dataDirectory!, "InvalidImage");

        try
        {
            string imagePath = Path.Combine(_dataDirectory!, fileName);
            Assert.IsTrue(File.Exists(imagePath));

            Directory.CreateDirectory(tempDirectory);
            string invalidImagePath = Path.Combine(tempDirectory, invalidFileName);
            Assert.IsFalse(File.Exists(invalidImagePath));

            ImageHelpers.CreateInvalidImage(imagePath, invalidImagePath);

            Assert.IsTrue(File.Exists(invalidImagePath));

            _testableAssetRepository!.AddFolder(tempDirectory); // Set above, not in this method

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            Asset? asset = _assetCreationService!.CreateAsset(tempDirectory, invalidFileName);

            Assert.IsNull(asset);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Assert.IsEmpty(thumbnails);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(tempDirectory, true);
        }
    }

    [Test]
    [TestCase("Image 9.png", "Corrupted_Image_1.jpg", 126277, 720, 1280, 200, 112, Rotation.Rotate0, "bcc994c14aa314dbc2dfbf48ffd34fa628dadcd86cdb8efda113b94a9035f15956cf039f5858b74cd7f404e98f7e84d9821b39aaa6cbbdc73228fa74ad2a5c20", 2148)]
    [TestCase("Image 1.jpg", "Corrupted_Image_1.heic", 29857, 720, 1280, 200, 112, Rotation.Rotate0, "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9", 2122)]
    public void CreateAsset_CorruptedPictureBasicHashType_ReturnsAssetWithCorruptedMessage(
        string initialFileName,
        string corruptedAssetFileName,
        long fileSize,
        int pixelHeight,
        int pixelWidth,
        int thumbnailPixelWidth,
        int thumbnailPixelHeight,
        Rotation imageRotation,
        string hash,
        int imageByteSizeExpected)
    {
        ConfigureAssetCreationService(200, 150, false, false, false, false);
        string tempDirectory = Path.Combine(_dataDirectory!, "CorruptedImage");

        try
        {
            Asset expectedAsset = new()
            {
                FileName = corruptedAssetFileName,
                FileSize = fileSize,
                PixelHeight = pixelHeight,
                PixelWidth = pixelWidth,
                ThumbnailPixelWidth = thumbnailPixelWidth,
                ThumbnailPixelHeight = thumbnailPixelHeight,
                ImageRotation = imageRotation,
                Hash = hash,
                IsAssetCorrupted = true,
                AssetCorruptedMessage = _userConfigurationService!.AssetSettings.AssetCorruptedMessage,
                IsAssetRotated = false,
                AssetRotatedMessage = null
            };

            string imagePath = Path.Combine(_dataDirectory!, initialFileName);
            Assert.IsTrue(File.Exists(imagePath));

            Directory.CreateDirectory(tempDirectory);
            string corruptedImagePath = Path.Combine(tempDirectory, corruptedAssetFileName);
            Assert.IsFalse(File.Exists(corruptedImagePath));

            File.Copy(imagePath, corruptedImagePath, overwrite: true);

            Assert.IsTrue(File.Exists(corruptedImagePath));

            Folder folder = _testableAssetRepository!.AddFolder(tempDirectory); // Set above, not in this method

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            Asset? asset = _assetCreationService!.CreateAsset(tempDirectory, corruptedAssetFileName);

            Assert.IsNotNull(asset);

            AssertAssetPropertyValidity(
                asset!,
                expectedAsset.FileName,
                corruptedImagePath,
                tempDirectory,
                folder,
                expectedAsset.FileSize,
                expectedAsset.PixelHeight,
                expectedAsset.PixelWidth,
                expectedAsset.ThumbnailPixelWidth,
                expectedAsset.ThumbnailPixelHeight,
                expectedAsset.ImageRotation,
                expectedAsset.Hash,
                expectedAsset.IsAssetCorrupted,
                expectedAsset.AssetCorruptedMessage,
                expectedAsset.IsAssetRotated,
                expectedAsset.AssetRotatedMessage);

            AssertCataloguedAssetAndThumbnailValidity(asset!, folder, thumbnails, imageByteSizeExpected);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(tempDirectory, true);
        }
    }

    [Test]
    public void CreateAsset_PictureAndBasicHashTypeAndFolderNotAdded_ThrowsNullReferenceExceptionAndAssetIsNotCreated()
    {
        ConfigureAssetCreationService(200, 150, false, false, false, false);

        try
        {
            const string assetName = "Image 1.jpg";
            string imagePath = Path.Combine(_dataDirectory!, assetName);

            Assert.IsTrue(File.Exists(imagePath));

            List<Asset> assetsFromRepository = _testableAssetRepository!.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            NullReferenceException? exception = Assert.Throws<NullReferenceException>(() => _assetCreationService!.CreateAsset(_dataDirectory!, assetName));

            Assert.AreEqual("Object reference not set to an instance of an object.", exception?.Message);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);
            Assert.IsEmpty(thumbnails);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase("Image 1.jpg", 100000, 100)] // Width > Height
    [TestCase("Image 10 portrait.png", 100, 100000)] // Height > Width
    [TestCase("Homer.gif", 100, 100000)] // Width == Height
    public void CreateAsset_PictureAndBasicHashTypeAndThumbnailSettingsTooBig_ThrowsOverflowExceptionAndAssetIsNotCreated(string assetName, int thumbnailMaxWidth, int thumbnailMaxHeight)
    {
        ConfigureAssetCreationService(thumbnailMaxWidth, thumbnailMaxHeight, false, false, false, false);

        try
        {
            _testableAssetRepository!.AddFolder(_dataDirectory!); // Set above, not in this method

            string imagePath = Path.Combine(_dataDirectory!, assetName);
            Assert.IsTrue(File.Exists(imagePath));

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            OverflowException? exception = Assert.Throws<OverflowException>(() => _assetCreationService!.CreateAsset(_dataDirectory!, assetName));

            Assert.AreEqual("The image data generated an overflow during processing.", exception?.Message);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);
            Assert.IsEmpty(thumbnails);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void CreateAsset_HeicPictureAndBasicHashTypeAndThumbnailSettingsTooBig_ThrowsInvalidOperationExceptionAndAssetIsNotCreated()
    {
        const string assetName = "Image_11.heic";
        const int thumbnailMaxWidth = 150;
        const int thumbnailMaxHeight = 10000000;

        ConfigureAssetCreationService(thumbnailMaxWidth, thumbnailMaxHeight, false, false, false, false);

        try
        {
            _testableAssetRepository!.AddFolder(_dataDirectory!); // Set above, not in this method

            string imagePath = Path.Combine(_dataDirectory!, assetName);
            Assert.IsTrue(File.Exists(imagePath));

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            InvalidOperationException? exception = Assert.Throws<InvalidOperationException>(() => _assetCreationService!.CreateAsset(_dataDirectory!, assetName));

            Assert.AreEqual("Operation is not valid due to the current state of the object.", exception?.Message);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);
            Assert.IsEmpty(thumbnails);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void CreateAsset_HeicPictureAndBasicHashTypeAndNegativeThumbnailSettings_ThrowsArgumentExceptionAndAssetIsNotCreated()
    {
        const string assetName = "Image_11.heic";
        const int thumbnailMaxWidth = -100;
        const int thumbnailMaxHeight = -100;

        ConfigureAssetCreationService(thumbnailMaxWidth, thumbnailMaxHeight, false, false, false, false);

        try
        {
            _testableAssetRepository!.AddFolder(_dataDirectory!); // Set above, not in this method

            string imagePath = Path.Combine(_dataDirectory!, assetName);
            Assert.IsTrue(File.Exists(imagePath));

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            ArgumentException? exception = Assert.Throws<ArgumentException>(() => _assetCreationService!.CreateAsset(_dataDirectory!, assetName));

            Assert.AreEqual("Value should not be negative. (Parameter 'width')", exception?.Message);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);
            Assert.IsEmpty(thumbnails);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void CreateAsset_PictureAndBasicHashTypeAndDirectoryNameIsNull_ThrowsArgumentNullExceptionAndAssetIsNotCreated()
    {
        ConfigureAssetCreationService(200, 150, false, false, false, false);

        try
        {
            const string assetName = "Image 1.jpg";
            _testableAssetRepository!.AddFolder(_dataDirectory!); // Set above, not in this method

            string imagePath = Path.Combine(_dataDirectory!, assetName);
            Assert.IsTrue(File.Exists(imagePath));

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _assetCreationService!.CreateAsset(null!, assetName));

            Assert.AreEqual("Value cannot be null. (Parameter 'path1')", exception?.Message);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);
            Assert.IsEmpty(thumbnails);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void CreateAsset_PictureAndBasicHashTypeAndDirectoryNameIsEmpty_LogsErrorAndReturnsNullAndAssetIsNotCreated()
    {
        ConfigureAssetCreationService(200, 150, false, false, false, false);
        LoggingAssertsService loggingAssertsService = new();

        try
        {
            const string assetName = "Image 1.jpg";
            string directoryName = Path.Combine(_dataDirectory!, "TestFolder\\TestHiddenSubFolder");

            _testableAssetRepository!.AddFolder(_dataDirectory!); // Set above, not in this method

            string imagePath = Path.Combine(_dataDirectory!, assetName);
            Assert.IsTrue(File.Exists(imagePath));

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            Asset? asset = _assetCreationService!.CreateAsset(directoryName, assetName);

            Assert.IsNull(asset);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);
            Assert.IsEmpty(thumbnails);

            FileNotFoundException fileNotFoundException = new ($"The file {Path.Combine(directoryName, assetName)} does not exist.");
            Exception[] expectedExceptions = [fileNotFoundException];
            Type typeOfService = typeof(AssetCreationService);

            loggingAssertsService.AssertLogs(expectedExceptions, typeOfService);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            loggingAssertsService.LoggingAssertTearDown();
        }
    }

    [Test]
    public void CreateAsset_PictureAndBasicHashTypeAndDirectoryNamePointingToAFile_LogsErrorAndReturnsNullAndAssetIsNotCreated()
    {
        ConfigureAssetCreationService(200, 150, false, false, false, false);
        LoggingAssertsService loggingAssertsService = new();

        try
        {
            const string assetName = "Image 1.jpg";
            string directoryName = Path.Combine(_dataDirectory!, assetName);

            _testableAssetRepository!.AddFolder(_dataDirectory!); // Set above, not in this method

            string imagePath = Path.Combine(_dataDirectory!, assetName);
            Assert.IsTrue(File.Exists(imagePath));

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            Asset? asset = _assetCreationService!.CreateAsset(directoryName, assetName);

            Assert.IsNull(asset);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);
            Assert.IsEmpty(thumbnails);

            FileNotFoundException fileNotFoundException = new ($"The file {Path.Combine(directoryName, assetName)} does not exist.");
            Exception[] expectedExceptions = [fileNotFoundException];
            Type typeOfService = typeof(AssetCreationService);

            loggingAssertsService.AssertLogs(expectedExceptions, typeOfService);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            loggingAssertsService.LoggingAssertTearDown();
        }
    }

    [Test]
    public void CreateAsset_PictureAndBasicHashTypeAndFileNameIsNull_ThrowsArgumentNullExceptionAndAssetIsNotCreated()
    {
        ConfigureAssetCreationService(200, 150, false, false, false, false);

        try
        {
            string assetName = null!;
            _testableAssetRepository!.AddFolder(_dataDirectory!); // Set above, not in this method

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _assetCreationService!.CreateAsset(_dataDirectory!, assetName));

            Assert.AreEqual("Value cannot be null. (Parameter 'path2')", exception?.Message);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);
            Assert.IsEmpty(thumbnails);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void CreateAsset_PictureAndBasicHashTypeAndFileNameDoesNotExist_LogsErrorAndReturnsNullAndAssetIsNotCreated()
    {
        ConfigureAssetCreationService(200, 150, false, false, false, false);
        LoggingAssertsService loggingAssertsService = new();

        try
        {
            const string assetName = "Toto.jpg";

            _testableAssetRepository!.AddFolder(_dataDirectory!); // Set above, not in this method

            string imagePath = Path.Combine(_dataDirectory!, assetName);
            Assert.IsFalse(File.Exists(imagePath));

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            Asset? asset = _assetCreationService!.CreateAsset(_dataDirectory!, assetName);

            Assert.IsNull(asset);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);
            Assert.IsEmpty(thumbnails);

            FileNotFoundException fileNotFoundException = new ($"The file {imagePath} does not exist.");
            Exception[] expectedExceptions = [fileNotFoundException];
            Type typeOfService = typeof(AssetCreationService);

            loggingAssertsService.AssertLogs(expectedExceptions, typeOfService);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            loggingAssertsService.LoggingAssertTearDown();
        }
    }

    [Test]
    public void CreateAsset_PictureAndBasicHashTypeAndFileNameIsADirectory_LogsErrorAndReturnsNullAndAssetIsNotCreated()
    {
        ConfigureAssetCreationService(200, 150, false, false, false, false);
        LoggingAssertsService loggingAssertsService = new();

        try
        {
            const string assetName = "Image 1.jpg";

            _testableAssetRepository!.AddFolder(_dataDirectory!); // Set above, not in this method

            string imagePath = Path.Combine(_dataDirectory!, assetName);
            Assert.IsTrue(File.Exists(imagePath));

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            Asset? asset = _assetCreationService!.CreateAsset(_dataDirectory!, _dataDirectory!);

            Assert.IsNull(asset);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);
            Assert.IsEmpty(thumbnails);

            FileNotFoundException fileNotFoundException = new ($"The file {_dataDirectory} does not exist.");
            Exception[] expectedExceptions = [fileNotFoundException];
            Type typeOfService = typeof(AssetCreationService);

            loggingAssertsService.AssertLogs(expectedExceptions, typeOfService);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            loggingAssertsService.LoggingAssertTearDown();
        }
    }

    [TestCase("Homer.mp4", "Homer.jpg", 6599, 180, 320, 200, 112, Rotation.Rotate0, "59742f2cd31c0997be96f9e758799d975f5918f7732f351d66280a708681ea74ccbfa1b61a327835a3f1dbb5ea5f9989484764a10f56f7dd6f32f7b24e286d66", false, null, false, null, 4779)]
    public void CreateAsset_VideoAndBasicHashType_ReturnsAsset(
        string fileName,
        string firstFrameFileName,
        long fileSize,
        int pixelHeight,
        int pixelWidth,
        int thumbnailPixelWidth,
        int thumbnailPixelHeight,
        Rotation imageRotation,
        string hash,
        bool isAssetCorrupted,
        string assetCorruptedMessage,
        bool isAssetRotated,
        string assetRotatedMessage,
        int imageByteSize)
    {
        ConfigureAssetCreationService(200, 150, false, false, false, true);

        string firstFrameVideosPath = _userConfigurationService!.PathSettings.FirstFrameVideosPath;

        try
        {
            string videoPath = Path.Combine(_dataDirectory!, fileName);
            Assert.IsTrue(File.Exists(videoPath));

            string imagePath = Path.Combine(firstFrameVideosPath, firstFrameFileName);
            Assert.IsFalse(File.Exists(imagePath));

            _testableAssetRepository!.AddFolder(_dataDirectory!); // Set above, not in this method

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            Asset? asset = _assetCreationService!.CreateAsset(_dataDirectory!, fileName, true);

            Assert.IsNull(asset);

            Assert.IsTrue(File.Exists(imagePath));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Assert.IsEmpty(thumbnails);

            Folder folder = _testableAssetRepository.AddFolder(firstFrameVideosPath); // Set above, not in this method

            asset = _assetCreationService!.CreateAsset(firstFrameVideosPath, firstFrameFileName);

            Assert.IsNotNull(asset);

            Assert.IsTrue(File.Exists(videoPath));
            Assert.IsTrue(File.Exists(imagePath));

            AssertAssetPropertyValidity(
                asset!,
                firstFrameFileName,
                imagePath,
                firstFrameVideosPath,
                folder,
                fileSize,
                pixelHeight,
                pixelWidth,
                thumbnailPixelWidth,
                thumbnailPixelHeight,
                imageRotation,
                hash,
                isAssetCorrupted,
                assetCorruptedMessage,
                isAssetRotated,
                assetRotatedMessage);

            AssertCataloguedAssetAndThumbnailValidity(asset!, folder, thumbnails, imageByteSize);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(firstFrameVideosPath, true);
        }
    }

    // TODO: Evolve this test when the rework of video handling has been made
    [TestCase("Homer.mp4", "Homer.jpg", 6599, 180, 320, 200, 112, Rotation.Rotate0, "59742f2cd31c0997be96f9e758799d975f5918f7732f351d66280a708681ea74ccbfa1b61a327835a3f1dbb5ea5f9989484764a10f56f7dd6f32f7b24e286d66", false, null, false, null, 4779)]
    public void CreateAsset_VideoAndBasicHashTypeAndCreateFirstFrameInSameLocationAsVideo_ReturnsAsset(
        string fileName,
        string firstFrameFileName,
        long fileSize,
        int pixelHeight,
        int pixelWidth,
        int thumbnailPixelWidth,
        int thumbnailPixelHeight,
        Rotation imageRotation,
        string hash,
        bool isAssetCorrupted,
        string assetCorruptedMessage,
        bool isAssetRotated,
        string assetRotatedMessage,
        int imageByteSize)
    {
        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock.MockGetValue(UserConfigurationKeys.ASSETS_DIRECTORY, _dataDirectory!); // Only needed for videos
        configurationRootMock.MockGetValue(UserConfigurationKeys.THUMBNAIL_MAX_WIDTH, "200");
        configurationRootMock.MockGetValue(UserConfigurationKeys.THUMBNAIL_MAX_HEIGHT, "150");
        configurationRootMock.MockGetValue(UserConfigurationKeys.USING_DHASH, "false");
        configurationRootMock.MockGetValue(UserConfigurationKeys.USING_MD5_HASH, "false");
        configurationRootMock.MockGetValue(UserConfigurationKeys.USING_PHASH, "false");
        configurationRootMock.MockGetValue(UserConfigurationKeys.ANALYSE_VIDEOS, "true");
        configurationRootMock.MockGetValue(UserConfigurationKeys.FFMPEG_PATH, FFMPEG_PATH);
        configurationRootMock.MockGetValue(UserConfigurationKeys.FIRST_FRAME_VIDEOS_FOLDER_NAME, "TempForVideo");

        _userConfigurationService = new (configurationRootMock.Object);
        _testableAssetRepository = new (_database!, _storageServiceMock!.Object, _userConfigurationService);
        StorageService storageService = new (_userConfigurationService);
        AssetHashCalculatorService assetHashCalculatorService = new (_userConfigurationService);
        _assetCreationService = new (_testableAssetRepository, storageService, assetHashCalculatorService, _userConfigurationService);

        string firstFrameVideosPath = _userConfigurationService!.PathSettings.FirstFrameVideosPath;

        Directory.CreateDirectory(firstFrameVideosPath);

        try
        {
            string videoPath = Path.Combine(_dataDirectory!, fileName);
            Assert.IsTrue(File.Exists(videoPath));

            string newVideoPath = Path.Combine(firstFrameVideosPath, fileName);
            File.Copy(videoPath, newVideoPath);
            Assert.IsTrue(File.Exists(newVideoPath));
            Assert.IsTrue(File.Exists(videoPath));

            string imagePath = Path.Combine(firstFrameVideosPath, firstFrameFileName);
            Assert.IsFalse(File.Exists(imagePath));

            // Folder folder = _testableAssetRepository!.AddFolder(firstFrameVideosPath); // Set above, not in this method
            _testableAssetRepository!.AddFolder(firstFrameVideosPath); // Set above, not in this method

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            Asset? asset = _assetCreationService!.CreateAsset(firstFrameVideosPath, fileName, true);

            // Assert.IsNotNull(asset);
            Assert.IsNull(asset);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Assert.IsEmpty(thumbnails);

            Assert.IsTrue(File.Exists(newVideoPath));
            Assert.IsTrue(File.Exists(imagePath));

            // AssertAssetPropertyValidity(
            //     asset!,
            //     firstFrameFileName,
            //     imagePath,
            //     firstFrameVideosPath,
            //     folder,
            //     fileSize,
            //     pixelHeight,
            //     pixelWidth,
            //     thumbnailPixelWidth,
            //     thumbnailPixelHeight,
            //     imageRotation,
            //     hash,
            //     isAssetCorrupted,
            //     assetCorruptedMessage,
            //     isAssetRotated,
            //     assetRotatedMessage);

            // AssertCataloguedAssetAndThumbnailValidity(asset!, folder, thumbnails, imageByteSize);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(firstFrameVideosPath, true);
        }
    }

    [TestCase("Homer1s.mp4", "Homer1s.jpg")]
    public void CreateAsset_VideoTooShortAndBasicHashType_ReturnsNullAndDoesNotCreateFirstFrameAndDoesNotCreateAsset(string fileName, string firstFrameFileName)
    {
        ConfigureAssetCreationService(200, 150, false, false, false, true);

        string firstFrameVideosPath = _userConfigurationService!.PathSettings.FirstFrameVideosPath;

        try
        {
            string videoPath = Path.Combine(_dataDirectory!, fileName);
            Assert.IsTrue(File.Exists(videoPath));

            string imagePath = Path.Combine(firstFrameVideosPath, firstFrameFileName);
            Assert.IsFalse(File.Exists(imagePath));

            _testableAssetRepository!.AddFolder(_dataDirectory!); // Set above, not in this method

            List<Asset> assetsFromRepository = _testableAssetRepository!.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            Asset? asset = _assetCreationService!.CreateAsset(_dataDirectory!, fileName, true);

            Assert.IsNull(asset);

            Assert.IsFalse(File.Exists(imagePath));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Assert.IsEmpty(thumbnails);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(firstFrameVideosPath, true);
        }
    }

    [TestCase("Homer.mp4", "Homer.jpg", 6599, 180, 320, 200, 112, Rotation.Rotate0, "59742f2cd31c0997be96f9e758799d975f5918f7732f351d66280a708681ea74ccbfa1b61a327835a3f1dbb5ea5f9989484764a10f56f7dd6f32f7b24e286d66", false, null, false, null, 4779)]
    public void CreateAsset_VideoAndBasicHashTypeAndAddSameVideoTwice_ReturnsAsset(
        string fileName,
        string firstFrameFileName,
        long fileSize,
        int pixelHeight,
        int pixelWidth,
        int thumbnailPixelWidth,
        int thumbnailPixelHeight,
        Rotation imageRotation,
        string hash,
        bool isAssetCorrupted,
        string assetCorruptedMessage,
        bool isAssetRotated,
        string assetRotatedMessage,
        int imageByteSize)
    {
        ConfigureAssetCreationService(200, 150, false, false, false, true);

        string firstFrameVideosPath = _userConfigurationService!.PathSettings.FirstFrameVideosPath;

        try
        {
            string videoPath = Path.Combine(_dataDirectory!, fileName);
            Assert.IsTrue(File.Exists(videoPath));

            string imagePath = Path.Combine(firstFrameVideosPath, firstFrameFileName);
            Assert.IsFalse(File.Exists(imagePath));

            _testableAssetRepository!.AddFolder(_dataDirectory!); // Set above, not in this method

            List<Asset> assetsFromRepository = _testableAssetRepository!.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            Asset? asset1 = _assetCreationService!.CreateAsset(_dataDirectory!, fileName, true);

            Assert.IsNull(asset1);

            Assert.IsTrue(File.Exists(imagePath));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Assert.IsEmpty(thumbnails);

            Folder folder = _testableAssetRepository.AddFolder(firstFrameVideosPath); // Set above, not in this method

            Asset? asset2 = _assetCreationService!.CreateAsset(firstFrameVideosPath, firstFrameFileName);

            Assert.IsNotNull(asset2);

            Assert.IsTrue(File.Exists(videoPath));
            Assert.IsTrue(File.Exists(imagePath));

            AssertAssetPropertyValidity(
                asset2!,
                firstFrameFileName,
                imagePath,
                firstFrameVideosPath,
                folder,
                fileSize,
                pixelHeight,
                pixelWidth,
                thumbnailPixelWidth,
                thumbnailPixelHeight,
                imageRotation,
                hash,
                isAssetCorrupted,
                assetCorruptedMessage,
                isAssetRotated,
                assetRotatedMessage);

            AssertCataloguedAssetAndThumbnailValidity(asset2!, folder, thumbnails, imageByteSize);

            Asset? asset3 = _assetCreationService!.CreateAsset(_dataDirectory!, fileName, true);

            Assert.IsNull(asset3);

            Assert.IsTrue(File.Exists(videoPath));
            Assert.IsTrue(File.Exists(imagePath));

            AssertCataloguedAssetAndThumbnailValidity(asset2!, folder, thumbnails, imageByteSize);

            folder = _testableAssetRepository.AddFolder(firstFrameVideosPath); // Set above, not in this method

            Asset? asset4 = _assetCreationService!.CreateAsset(firstFrameVideosPath, firstFrameFileName);

            Assert.IsNull(asset4);

            Assert.IsTrue(File.Exists(videoPath));
            Assert.IsTrue(File.Exists(imagePath));

            Assert.AreEqual(firstFrameFileName, asset2!.FileName);

            AssertCataloguedAssetAndThumbnailValidity(asset2, folder, thumbnails, imageByteSize);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(firstFrameVideosPath, true);
        }
    }

    [TestCase("Homer.mp4", "Homer.jpg")]
    public void CreateAsset_VideoAndBasicHashTypeButAnotherImageAlreadyExistsInTheOutputDirectory_DoesNotCreateFirstFrame(string fileName, string firstFrameFileName)
    {
        ConfigureAssetCreationService(200, 150, false, false, false, true);

        string firstFrameVideosPath = _userConfigurationService!.PathSettings.FirstFrameVideosPath;

        try
        {
            Directory.CreateDirectory(firstFrameVideosPath);

            string sourceImagePath = Path.Combine(_dataDirectory!, "Image 1.jpg");
            string imagePath = Path.Combine(firstFrameVideosPath, firstFrameFileName);
            File.Copy(sourceImagePath, imagePath);
            Assert.IsTrue(File.Exists(imagePath));

            string videoPath = Path.Combine(_dataDirectory!, fileName);
            Assert.IsTrue(File.Exists(videoPath));

            _testableAssetRepository!.AddFolder(_dataDirectory!); // Set above, not in this method

            List<Asset> assetsFromRepository = _testableAssetRepository!.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            Asset? asset1 = _assetCreationService!.CreateAsset(_dataDirectory!, fileName, true);

            Assert.IsNull(asset1);

            Assert.IsTrue(File.Exists(imagePath));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Assert.IsEmpty(thumbnails);

            Folder folder = _testableAssetRepository.AddFolder(firstFrameVideosPath); // Set above, not in this method

            Asset? asset2 = _assetCreationService!.CreateAsset(firstFrameVideosPath, firstFrameFileName);

            Assert.IsNotNull(asset2);

            Assert.IsTrue(File.Exists(videoPath));
            Assert.IsTrue(File.Exists(imagePath));

            Asset expectedAsset = new()
            {
                FileName = firstFrameFileName,
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

            const int expectedImageByteSize = 2097;

            AssertAssetPropertyValidity(
                asset2!,
                firstFrameFileName,
                imagePath,
                firstFrameVideosPath,
                folder,
                expectedAsset.FileSize,
                expectedAsset.PixelHeight,
                expectedAsset.PixelWidth,
                expectedAsset.ThumbnailPixelWidth,
                expectedAsset.ThumbnailPixelHeight,
                expectedAsset.ImageRotation,
                expectedAsset.Hash,
                expectedAsset.IsAssetCorrupted,
                expectedAsset.AssetCorruptedMessage,
                expectedAsset.IsAssetRotated,
                expectedAsset.AssetRotatedMessage);

            AssertCataloguedAssetAndThumbnailValidity(asset2!, folder, thumbnails, expectedImageByteSize);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(firstFrameVideosPath, true);
        }
    }

    [Test]
    [TestCase("Homer.mp4", "Homer.jpg", false, false)]
    [TestCase("Homer.mp4", "Homer.jpg", false, true)]
    [TestCase("Homer.mp4", "Homer.jpg", true, false)]
    public void CreateAsset_VideoAndIsVideoAndAnalyseVideos_ReturnsNullAndDoesNotCreateFirstFrameAndDoesNotCreateAsset(string fileName, string firstFrameFileName, bool analyseVideos, bool isVideo)
    {
        ConfigureAssetCreationService(200, 150, false, false, false, analyseVideos);

        try
        {
            string filePath = Path.Combine(_dataDirectory!, fileName);
            Assert.IsTrue(File.Exists(filePath));

            _testableAssetRepository!.AddFolder(_dataDirectory!); // Set above, not in this method

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            Asset? asset = _assetCreationService!.CreateAsset(_dataDirectory!, fileName, isVideo);

            Assert.IsNull(asset);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Assert.IsEmpty(thumbnails);

            string firstFrameVideosPath = _userConfigurationService!.PathSettings.FirstFrameVideosPath;

            string imagePath = Path.Combine(firstFrameVideosPath, firstFrameFileName);
            Assert.IsFalse(File.Exists(imagePath));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase("Image 1.jpg", 29857, 720, 1280, 200, 112, Rotation.Rotate0, "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9", false, null, false, null, 2097, false)]
    public void CreateAsset_PictureAndIsVideoIsTrueAndAnalyseVideosIsFalse_ReturnsAsset(
        string fileName,
        long fileSize,
        int pixelHeight,
        int pixelWidth,
        int thumbnailPixelWidth,
        int thumbnailPixelHeight,
        Rotation imageRotation,
        string hash,
        bool isAssetCorrupted,
        string assetCorruptedMessage,
        bool isAssetRotated,
        string assetRotatedMessage,
        int imageByteSize,
        bool analyseVideos)
    {
        ConfigureAssetCreationService(200, 150, false, false, false, analyseVideos);

        string firstFrameVideosPath = _userConfigurationService!.PathSettings.FirstFrameVideosPath;

        try
        {
            string filePath = Path.Combine(_dataDirectory!, fileName);
            Assert.IsTrue(File.Exists(filePath));

            Folder folder = _testableAssetRepository!.AddFolder(_dataDirectory!); // Set above, not in this method

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            Asset? asset = _assetCreationService!.CreateAsset(_dataDirectory!, fileName, true);

            Assert.IsNotNull(asset);

            AssertAssetPropertyValidity(
                asset!,
                fileName,
                filePath,
                _dataDirectory!,
                folder,
                fileSize,
                pixelHeight,
                pixelWidth,
                thumbnailPixelWidth,
                thumbnailPixelHeight,
                imageRotation,
                hash,
                isAssetCorrupted,
                assetCorruptedMessage,
                isAssetRotated,
                assetRotatedMessage);

            AssertCataloguedAssetAndThumbnailValidity(asset!, folder, thumbnails, imageByteSize);

            string imagePath = Path.Combine(firstFrameVideosPath, fileName);
            Assert.IsFalse(File.Exists(imagePath));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            if (analyseVideos)
            {
                Directory.Delete(firstFrameVideosPath, true);
            }
        }
    }

    [Test]
    [TestCase("Image 1.jpg", 29857, 720, 1280, 200, 112, Rotation.Rotate0, "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9", false, null, false, null, 2097, true)]
    public void CreateAsset_PictureAndIsVideoIsTrueAndAnalyseVideosIsTrue_ReturnsNullAndDoesNotCreateFirstFrameAndDoesNotCreateAsset(
        string fileName,
        long fileSize,
        int pixelHeight,
        int pixelWidth,
        int thumbnailPixelWidth,
        int thumbnailPixelHeight,
        Rotation imageRotation,
        string hash,
        bool isAssetCorrupted,
        string assetCorruptedMessage,
        bool isAssetRotated,
        string assetRotatedMessage,
        int imageByteSize,
        bool analyseVideos)
    {
        ConfigureAssetCreationService(200, 150, false, false, false, analyseVideos);

        string firstFrameVideosPath = _userConfigurationService!.PathSettings.FirstFrameVideosPath;

        try
        {
            string filePath = Path.Combine(_dataDirectory!, fileName);
            Assert.IsTrue(File.Exists(filePath));

            _testableAssetRepository!.AddFolder(_dataDirectory!); // Set above, not in this method

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            Asset? asset = _assetCreationService!.CreateAsset(_dataDirectory!, fileName, true);

            Assert.IsNull(asset);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Assert.IsEmpty(thumbnails);

            string imagePath = Path.Combine(firstFrameVideosPath, fileName);
            Assert.IsFalse(File.Exists(imagePath));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            if (analyseVideos)
            {
                Directory.Delete(firstFrameVideosPath, true);
            }
        }
    }

    [Test]
    public void CreateAsset_VideoAndBasicHashTypeAndDirectoryNameIsNull_ThrowsArgumentNullExceptionAndDoesNotCreateFirstFrameAndDoesNotCreateAsset()
    {
        ConfigureAssetCreationService(200, 150, false, false, false, true);

        try
        {
            const string assetName = "Homer.mp4";
            _testableAssetRepository!.AddFolder(_dataDirectory!); // Set above, not in this method

            string imagePath = Path.Combine(_dataDirectory!, assetName);
            Assert.IsTrue(File.Exists(imagePath));

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _assetCreationService!.CreateAsset(null!, assetName, true));

            Assert.AreEqual("Value cannot be null. (Parameter 'path1')", exception?.Message);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Assert.IsEmpty(thumbnails);

            string firstFrameVideosPath = _userConfigurationService!.PathSettings.FirstFrameVideosPath;
            string firstFrameGeneratedPath = Path.Combine(firstFrameVideosPath, "Homer.jpg");
            Assert.IsFalse(File.Exists(firstFrameGeneratedPath));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void CreateAsset_VideoAndBasicHashTypeAndDirectoryNameIsEmpty_ReturnsNullAndDoesNotCreateFirstFrameAndDoesNotCreateAsset()
    {
        ConfigureAssetCreationService(200, 150, false, false, false, true);

        try
        {
            const string assetName = "Homer.mp4";
            string directoryName = Path.Combine(_dataDirectory!, "TestFolder\\TestHiddenSubFolder");

            _testableAssetRepository!.AddFolder(_dataDirectory!); // Set above, not in this method

            string imagePath = Path.Combine(_dataDirectory!, assetName);
            Assert.IsTrue(File.Exists(imagePath));

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            Asset? asset = _assetCreationService!.CreateAsset(directoryName, assetName, true);

            Assert.IsNull(asset);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Assert.IsEmpty(thumbnails);

            string firstFrameVideosPath = _userConfigurationService!.PathSettings.FirstFrameVideosPath;

            string firstFrameGeneratedPath = Path.Combine(firstFrameVideosPath, "Homer.jpg");
            Assert.IsFalse(File.Exists(firstFrameGeneratedPath));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(_userConfigurationService!.PathSettings.FirstFrameVideosPath, true);
        }
    }

    [Test]
    public void CreateAsset_VideoAndBasicHashTypeAndDirectoryNamePointingToAFile_ReturnsNullAndDoesNotCreateFirstFrameAndDoesNotCreateAsset()
    {
        ConfigureAssetCreationService(200, 150, false, false, false, true);

        try
        {
            const string assetName = "Homer.mp4";
            string directoryName = Path.Combine(_dataDirectory!, assetName);

            _testableAssetRepository!.AddFolder(_dataDirectory!); // Set above, not in this method

            string imagePath = Path.Combine(_dataDirectory!, assetName);
            Assert.IsTrue(File.Exists(imagePath));

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            Asset? asset = _assetCreationService!.CreateAsset(directoryName, assetName, true);

            Assert.IsNull(asset);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Assert.IsEmpty(thumbnails);

            string firstFrameVideosPath = _userConfigurationService!.PathSettings.FirstFrameVideosPath;

            string firstFrameGeneratedPath = Path.Combine(firstFrameVideosPath, "Homer.jpg");
            Assert.IsFalse(File.Exists(firstFrameGeneratedPath));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(_userConfigurationService!.PathSettings.FirstFrameVideosPath, true);
        }
    }

    [Test]
    public void CreateAsset_VideoAndBasicHashTypeAndFileNameIsNull_ThrowsArgumentNullExceptionAndDoesNotCreateFirstFrameAndDoesNotCreateAsset()
    {
        ConfigureAssetCreationService(200, 150, false, false, false, true);

        try
        {
            string assetName = null!;
            _testableAssetRepository!.AddFolder(_dataDirectory!); // Set above, not in this method

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _assetCreationService!.CreateAsset(_dataDirectory!, assetName, true));

            Assert.AreEqual("Value cannot be null. (Parameter 'path2')", exception?.Message);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Assert.IsEmpty(thumbnails);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void CreateAsset_VideoAndBasicHashTypeAndFileNameDoesNotExist_ReturnsNullAndDoesNotCreateFirstFrameAndDoesNotCreateAsset()
    {
        ConfigureAssetCreationService(200, 150, false, false, false, true);

        try
        {
            const string assetName = "Toto.mp4";

            _testableAssetRepository!.AddFolder(_dataDirectory!); // Set above, not in this method

            string imagePath = Path.Combine(_dataDirectory!, assetName);
            Assert.IsFalse(File.Exists(imagePath));

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            Asset? asset = _assetCreationService!.CreateAsset(_dataDirectory!, assetName, true);

            Assert.IsNull(asset);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);
            Assert.IsEmpty(thumbnails);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(_userConfigurationService!.PathSettings.FirstFrameVideosPath, true);
        }
    }

    [Test]
    public void CreateAsset_VideoAndBasicHashTypeAndFileNameIsADirectory_ReturnsNullAndDoesNotCreateFirstFrameAndDoesNotCreateAsset()
    {
        ConfigureAssetCreationService(200, 150, false, false, false, true);

        try
        {
            const string assetName = "Homer.mp4";

            _testableAssetRepository!.AddFolder(_dataDirectory!); // Set above, not in this method

            string imagePath = Path.Combine(_dataDirectory!, assetName);
            Assert.IsTrue(File.Exists(imagePath));

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            Asset? asset = _assetCreationService!.CreateAsset(_dataDirectory!, _dataDirectory!, true);

            Assert.IsNull(asset);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);
            Assert.IsEmpty(thumbnails);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(_userConfigurationService!.PathSettings.FirstFrameVideosPath, true);
        }
    }

    private static void AssertAssetPropertyValidity(
        Asset asset,
        string fileName,
        string imagePath,
        string folderPath,
        Folder folder,
        long fileSize,
        int pixelHeight,
        int pixelWidth,
        int thumbnailPixelWidth,
        int thumbnailPixelHeight,
        Rotation imageRotation,
        string hash,
        bool isAssetCorrupted,
        string? assetCorruptedMessage,
        bool isAssetRotated,
        string? assetRotatedMessage)
    {
        DateTime actualDate = DateTime.Now.Date;
        DateTime minDate = DateTime.MinValue.Date;

        Assert.AreEqual(fileName, asset.FileName);
        Assert.AreEqual(folder.FolderId, asset.FolderId);
        Assert.AreEqual(folder, asset.Folder);
        Assert.AreEqual(fileSize, asset.FileSize);
        Assert.AreEqual(pixelWidth, asset.PixelWidth);
        Assert.AreEqual(pixelHeight, asset.PixelHeight);
        Assert.AreEqual(thumbnailPixelWidth, asset.ThumbnailPixelWidth);
        Assert.AreEqual(thumbnailPixelHeight, asset.ThumbnailPixelHeight);
        Assert.AreEqual(imageRotation, asset.ImageRotation);
        Assert.AreEqual(actualDate, asset.ThumbnailCreationDateTime.Date);
        Assert.AreEqual(hash, asset.Hash);
        Assert.AreEqual(isAssetCorrupted, asset.IsAssetCorrupted);
        Assert.AreEqual(assetCorruptedMessage, asset.AssetCorruptedMessage);
        Assert.AreEqual(isAssetRotated, asset.IsAssetRotated);
        Assert.AreEqual(assetRotatedMessage, asset.AssetRotatedMessage);
        Assert.AreEqual(imagePath, asset.FullPath);
        Assert.AreEqual(folderPath, asset.Folder.Path);
        Assert.IsNull(asset.ImageData); // Set above, not in this method
        Assert.AreEqual(minDate, asset.FileCreationDateTime); // Set above, not in this method
        Assert.AreEqual(minDate, asset.FileModificationDateTime); // Set above, not in this method
    }

    private void AssertCataloguedAssetAndThumbnailValidity(Asset asset, Folder folder, IReadOnlyDictionary<string, Dictionary<string, byte[]>> thumbnails, int imageByteSize)
    {
        List<Asset> assetsFromRepository = _testableAssetRepository!.GetCataloguedAssets();
        Assert.AreEqual(1, assetsFromRepository.Count);

        Asset assetFromRepository = assetsFromRepository[0];

        Assert.AreEqual(asset.FileName, assetFromRepository.FileName);
        Assert.AreEqual(asset.FolderId, assetFromRepository.FolderId);
        Assert.AreEqual(asset.Hash, assetFromRepository.Hash);
        Assert.IsNull(assetFromRepository.ImageData);

        Assert.AreEqual(1, thumbnails.Count);
        Assert.IsTrue(thumbnails.ContainsKey(asset.Folder.Path));
        Assert.AreEqual(1, thumbnails[folder.Path].Count);
        Assert.IsTrue(thumbnails[folder.Path].ContainsKey(asset.FileName));
        Assert.IsNotNull(thumbnails[folder.Path][asset.FileName]);
        Assert.AreEqual(imageByteSize, thumbnails[folder.Path][asset.FileName].Length);
    }
}
