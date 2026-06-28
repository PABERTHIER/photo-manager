using PhotoManager.Domain.Comparers;
using Directories = PhotoManager.Tests.Unit.Constants.Directories;

namespace PhotoManager.Tests.Unit.Domain.Comparers;

[TestFixture]
public class DateTimeAssetComparerTests
{
    private string? _assetsDirectory;

    private Asset? _asset1;
    private Asset? _asset2;
    private Asset? _asset3;
    private Asset? _asset4;
    private Asset? _asset5;
    private Asset? _asset6;
    private Asset? _asset7;
    private Asset? _asset8;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _assetsDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);

        Guid folderId = Guid.NewGuid();
        DateTime actualDate = DateTime.Now;

        Folder folder = new() { Id = folderId, Path = _assetsDirectory };

        _asset1 = AssetBuilder.Create()
            .WithFolder(folder)
            .WithFileName("Image 1.jpg")
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(1280, 720, 200, 112)
            .WithFileProperties(29000, actualDate, actualDate)
            .WithThumbnailCreationDateTime(actualDate)
            .WithHash(
                "d83f8c94118726480bb48ad2cbeb62b1e4081cae0248a98546edd40e25c0b115402dbf046d8a187c91e1e2f238cad5d41793ec892774f21c98eba8423c442bba")
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        _asset2 = AssetBuilder.Create()
            .WithFolder(folder)
            .WithFileName("image 1.jpg")
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(1280, 720, 200, 112)
            .WithFileProperties(30000, actualDate.AddDays(-1), actualDate.AddDays(-1))
            .WithThumbnailCreationDateTime(actualDate.AddDays(-1))
            .WithHash(
                "2b7ead7db283e3df1a04260143aa4e6528afe2ab17d528d382eaeb482f4e2ce55681a0a94735e6e35f54001c6a6c85299f1ad327a15215b8faed7f6e2d098532")
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        _asset3 = AssetBuilder.Create()
            .WithFolder(folder)
            .WithFileName("Image_1.jpg")
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(1280, 720, 200, 112)
            .WithFileProperties(31000, actualDate.AddDays(-2), actualDate.AddDays(-2))
            .WithThumbnailCreationDateTime(actualDate.AddDays(-2))
            .WithHash(
                "a36c0a32e17e40e43f80232d068231ff74be137e723cc12150442eac5acfaa0a88c5ebe25160230d80a1455f6dbc5ac90b1ecf3b527bed3a687379503c202005")
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        _asset4 = AssetBuilder.Create()
            .WithFolder(folder)
            .WithFileName("IMAGE 1.jpg")
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(1280, 720, 200, 112)
            .WithFileProperties(32000, actualDate.AddDays(-3), actualDate.AddDays(-3))
            .WithThumbnailCreationDateTime(actualDate.AddDays(-3))
            .WithHash(
                "fd64c9cd647ce85a5e3061594ccae50d9c11ff272af4593c0a3b5124099932cb4146484ce5c37b7a95c3046725179fc1d0bd40c363beacddec238c8dde48a676")
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        _asset5 = AssetBuilder.Create()
            .WithFolder(folder)
            .WithFileName("Image1.jpg")
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(1280, 720, 200, 112)
            .WithFileProperties(33000, actualDate.AddDays(-4), actualDate.AddDays(-4))
            .WithThumbnailCreationDateTime(actualDate.AddDays(-4))
            .WithHash(
                "fe11ba801fa93829ddd6201eab5218f870b106051b4eec7aa007551f70aa2a044ee986257fe030cd5138cdb0f3ef7f43930712e0ac5d2868af84d1b817bc64d9")
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        _asset6 = AssetBuilder.Create()
            .WithFolder(folder)
            .WithFileName("Image 1_duplicate.jpg")
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(1280, 720, 200, 112)
            .WithFileProperties(29000, actualDate, actualDate)
            .WithThumbnailCreationDateTime(actualDate)
            .WithHash(
                "d9399eea541dcc90af870995587be118b0e615f71317248c48c8ac9389b920af4e070f3b9c1965c8801a1c5e5f489df72c475070cd7a58770224b41a82b6dfe7")
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        _asset7 = AssetBuilder.Create()
            .WithFolder(folder)
            .WithFileName("Image 10.jpg")
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(1280, 720, 200, 112)
            .WithFileProperties(35000, new(2020, 12, 31), new(2020, 12, 31))
            .WithThumbnailCreationDateTime(new(2020, 12, 31))
            .WithHash(
                "e2ab4b48e17e3e8cca3a349514f49251e377075c756a0687f7db7ccee6aa4f658dcb7341a859a728ee20b9bd1c3ed44ecba352753770ce5c0738c4d851920335")
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        _asset8 = AssetBuilder.Create()
            .WithFolder(folder)
            .WithFileName("picture.png")
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(1280, 720, 200, 112)
            .WithFileProperties(36000, new(2005, 02, 15), new(2005, 02, 15))
            .WithThumbnailCreationDateTime(new(2005, 02, 15))
            .WithHash(
                "ff51412ee4982849d4db953b794e20ce6335277b7c8426543e955b9ae448c94541322e0f59aa88612abb3db166129cfe60aaa6cc20daff6aac2ae9ad896fcc01")
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
    }

    [Test]
    public void Compare_AscendingIsTrueAndThumbnailCreationDateTime_ReturnsExpectedResults()
    {
        DateTimeAssetComparer comparer = new(true, asset => asset.ThumbnailCreationDateTime);

        CompareAssets(_asset1!, _asset1!, comparer, 0);
        CompareAssets(_asset1!, _asset2!, comparer, 1);
        CompareAssets(_asset1!, _asset3!, comparer, 1);
        CompareAssets(_asset1!, _asset4!, comparer, 1);
        CompareAssets(_asset1!, _asset5!, comparer, 1);
        CompareAssets(_asset1!, _asset6!, comparer, -1);
        CompareAssets(_asset1!, _asset7!, comparer, 1);
        CompareAssets(_asset1!, _asset8!, comparer, 1);

        CompareAssets(_asset2!, _asset1!, comparer, -1);
        CompareAssets(_asset2!, _asset2!, comparer, 0);
        CompareAssets(_asset2!, _asset3!, comparer, 1);
        CompareAssets(_asset2!, _asset4!, comparer, 1);
        CompareAssets(_asset2!, _asset5!, comparer, 1);
        CompareAssets(_asset2!, _asset6!, comparer, -1);
        CompareAssets(_asset2!, _asset7!, comparer, 1);
        CompareAssets(_asset2!, _asset8!, comparer, 1);

        CompareAssets(_asset3!, _asset1!, comparer, -1);
        CompareAssets(_asset3!, _asset2!, comparer, -1);
        CompareAssets(_asset3!, _asset3!, comparer, 0);
        CompareAssets(_asset3!, _asset4!, comparer, 1);
        CompareAssets(_asset3!, _asset5!, comparer, 1);
        CompareAssets(_asset3!, _asset6!, comparer, -1);
        CompareAssets(_asset3!, _asset7!, comparer, 1);
        CompareAssets(_asset3!, _asset8!, comparer, 1);

        CompareAssets(_asset4!, _asset1!, comparer, -1);
        CompareAssets(_asset4!, _asset2!, comparer, -1);
        CompareAssets(_asset4!, _asset3!, comparer, -1);
        CompareAssets(_asset4!, _asset4!, comparer, 0);
        CompareAssets(_asset4!, _asset5!, comparer, 1);
        CompareAssets(_asset4!, _asset6!, comparer, -1);
        CompareAssets(_asset4!, _asset7!, comparer, 1);
        CompareAssets(_asset4!, _asset8!, comparer, 1);

        CompareAssets(_asset5!, _asset1!, comparer, -1);
        CompareAssets(_asset5!, _asset2!, comparer, -1);
        CompareAssets(_asset5!, _asset3!, comparer, -1);
        CompareAssets(_asset5!, _asset4!, comparer, -1);
        CompareAssets(_asset5!, _asset5!, comparer, 0);
        CompareAssets(_asset5!, _asset6!, comparer, -1);
        CompareAssets(_asset5!, _asset7!, comparer, 1);
        CompareAssets(_asset5!, _asset8!, comparer, 1);

        CompareAssets(_asset6!, _asset1!, comparer, 1);
        CompareAssets(_asset6!, _asset2!, comparer, 1);
        CompareAssets(_asset6!, _asset3!, comparer, 1);
        CompareAssets(_asset6!, _asset4!, comparer, 1);
        CompareAssets(_asset6!, _asset5!, comparer, 1);
        CompareAssets(_asset6!, _asset6!, comparer, 0);
        CompareAssets(_asset6!, _asset7!, comparer, 1);
        CompareAssets(_asset6!, _asset8!, comparer, 1);

        CompareAssets(_asset7!, _asset1!, comparer, -1);
        CompareAssets(_asset7!, _asset2!, comparer, -1);
        CompareAssets(_asset7!, _asset3!, comparer, -1);
        CompareAssets(_asset7!, _asset4!, comparer, -1);
        CompareAssets(_asset7!, _asset5!, comparer, -1);
        CompareAssets(_asset7!, _asset6!, comparer, -1);
        CompareAssets(_asset7!, _asset7!, comparer, 0);
        CompareAssets(_asset7!, _asset8!, comparer, 1);

        CompareAssets(_asset8!, _asset1!, comparer, -1);
        CompareAssets(_asset8!, _asset2!, comparer, -1);
        CompareAssets(_asset8!, _asset3!, comparer, -1);
        CompareAssets(_asset8!, _asset4!, comparer, -1);
        CompareAssets(_asset8!, _asset5!, comparer, -1);
        CompareAssets(_asset8!, _asset6!, comparer, -1);
        CompareAssets(_asset8!, _asset7!, comparer, -1);
        CompareAssets(_asset8!, _asset8!, comparer, 0);
    }

    [Test]
    public void Compare_AscendingIsFalseAndThumbnailCreationDateTime_ReturnsExpectedResults()
    {
        DateTimeAssetComparer comparer = new(false, asset => asset.ThumbnailCreationDateTime);

        CompareAssets(_asset1!, _asset1!, comparer, 0);
        CompareAssets(_asset1!, _asset2!, comparer, -1);
        CompareAssets(_asset1!, _asset3!, comparer, -1);
        CompareAssets(_asset1!, _asset4!, comparer, -1);
        CompareAssets(_asset1!, _asset5!, comparer, -1);
        CompareAssets(_asset1!, _asset6!, comparer, 1);
        CompareAssets(_asset1!, _asset7!, comparer, -1);
        CompareAssets(_asset1!, _asset8!, comparer, -1);

        CompareAssets(_asset2!, _asset1!, comparer, 1);
        CompareAssets(_asset2!, _asset2!, comparer, 0);
        CompareAssets(_asset2!, _asset3!, comparer, -1);
        CompareAssets(_asset2!, _asset4!, comparer, -1);
        CompareAssets(_asset2!, _asset5!, comparer, -1);
        CompareAssets(_asset2!, _asset6!, comparer, 1);
        CompareAssets(_asset2!, _asset7!, comparer, -1);
        CompareAssets(_asset2!, _asset8!, comparer, -1);

        CompareAssets(_asset3!, _asset1!, comparer, 1);
        CompareAssets(_asset3!, _asset2!, comparer, 1);
        CompareAssets(_asset3!, _asset3!, comparer, 0);
        CompareAssets(_asset3!, _asset4!, comparer, -1);
        CompareAssets(_asset3!, _asset5!, comparer, -1);
        CompareAssets(_asset3!, _asset6!, comparer, 1);
        CompareAssets(_asset3!, _asset7!, comparer, -1);
        CompareAssets(_asset3!, _asset8!, comparer, -1);

        CompareAssets(_asset4!, _asset1!, comparer, 1);
        CompareAssets(_asset4!, _asset2!, comparer, 1);
        CompareAssets(_asset4!, _asset3!, comparer, 1);
        CompareAssets(_asset4!, _asset4!, comparer, 0);
        CompareAssets(_asset4!, _asset5!, comparer, -1);
        CompareAssets(_asset4!, _asset6!, comparer, 1);
        CompareAssets(_asset4!, _asset7!, comparer, -1);
        CompareAssets(_asset4!, _asset8!, comparer, -1);

        CompareAssets(_asset5!, _asset1!, comparer, 1);
        CompareAssets(_asset5!, _asset2!, comparer, 1);
        CompareAssets(_asset5!, _asset3!, comparer, 1);
        CompareAssets(_asset5!, _asset4!, comparer, 1);
        CompareAssets(_asset5!, _asset5!, comparer, 0);
        CompareAssets(_asset5!, _asset6!, comparer, 1);
        CompareAssets(_asset5!, _asset7!, comparer, -1);
        CompareAssets(_asset5!, _asset8!, comparer, -1);

        CompareAssets(_asset6!, _asset1!, comparer, -1);
        CompareAssets(_asset6!, _asset2!, comparer, -1);
        CompareAssets(_asset6!, _asset3!, comparer, -1);
        CompareAssets(_asset6!, _asset4!, comparer, -1);
        CompareAssets(_asset6!, _asset5!, comparer, -1);
        CompareAssets(_asset6!, _asset6!, comparer, 0);
        CompareAssets(_asset6!, _asset7!, comparer, -1);
        CompareAssets(_asset6!, _asset8!, comparer, -1);

        CompareAssets(_asset7!, _asset1!, comparer, 1);
        CompareAssets(_asset7!, _asset2!, comparer, 1);
        CompareAssets(_asset7!, _asset3!, comparer, 1);
        CompareAssets(_asset7!, _asset4!, comparer, 1);
        CompareAssets(_asset7!, _asset5!, comparer, 1);
        CompareAssets(_asset7!, _asset6!, comparer, 1);
        CompareAssets(_asset7!, _asset7!, comparer, 0);
        CompareAssets(_asset7!, _asset8!, comparer, -1);

        CompareAssets(_asset8!, _asset1!, comparer, 1);
        CompareAssets(_asset8!, _asset2!, comparer, 1);
        CompareAssets(_asset8!, _asset3!, comparer, 1);
        CompareAssets(_asset8!, _asset4!, comparer, 1);
        CompareAssets(_asset8!, _asset5!, comparer, 1);
        CompareAssets(_asset8!, _asset6!, comparer, 1);
        CompareAssets(_asset8!, _asset7!, comparer, 1);
        CompareAssets(_asset8!, _asset8!, comparer, 0);
    }

    [Test]
    public void Compare_AscendingIsTrueAndFilePropertiesCreation_ReturnsExpectedResults()
    {
        DateTimeAssetComparer comparer = new(true, asset => asset.FileProperties.Creation);

        CompareAssets(_asset1!, _asset1!, comparer, 0);
        CompareAssets(_asset1!, _asset2!, comparer, 1);
        CompareAssets(_asset1!, _asset3!, comparer, 1);
        CompareAssets(_asset1!, _asset4!, comparer, 1);
        CompareAssets(_asset1!, _asset5!, comparer, 1);
        CompareAssets(_asset1!, _asset6!, comparer, -1);
        CompareAssets(_asset1!, _asset7!, comparer, 1);
        CompareAssets(_asset1!, _asset8!, comparer, 1);

        CompareAssets(_asset2!, _asset1!, comparer, -1);
        CompareAssets(_asset2!, _asset2!, comparer, 0);
        CompareAssets(_asset2!, _asset3!, comparer, 1);
        CompareAssets(_asset2!, _asset4!, comparer, 1);
        CompareAssets(_asset2!, _asset5!, comparer, 1);
        CompareAssets(_asset2!, _asset6!, comparer, -1);
        CompareAssets(_asset2!, _asset7!, comparer, 1);
        CompareAssets(_asset2!, _asset8!, comparer, 1);

        CompareAssets(_asset3!, _asset1!, comparer, -1);
        CompareAssets(_asset3!, _asset2!, comparer, -1);
        CompareAssets(_asset3!, _asset3!, comparer, 0);
        CompareAssets(_asset3!, _asset4!, comparer, 1);
        CompareAssets(_asset3!, _asset5!, comparer, 1);
        CompareAssets(_asset3!, _asset6!, comparer, -1);
        CompareAssets(_asset3!, _asset7!, comparer, 1);
        CompareAssets(_asset3!, _asset8!, comparer, 1);

        CompareAssets(_asset4!, _asset1!, comparer, -1);
        CompareAssets(_asset4!, _asset2!, comparer, -1);
        CompareAssets(_asset4!, _asset3!, comparer, -1);
        CompareAssets(_asset4!, _asset4!, comparer, 0);
        CompareAssets(_asset4!, _asset5!, comparer, 1);
        CompareAssets(_asset4!, _asset6!, comparer, -1);
        CompareAssets(_asset4!, _asset7!, comparer, 1);
        CompareAssets(_asset4!, _asset8!, comparer, 1);

        CompareAssets(_asset5!, _asset1!, comparer, -1);
        CompareAssets(_asset5!, _asset2!, comparer, -1);
        CompareAssets(_asset5!, _asset3!, comparer, -1);
        CompareAssets(_asset5!, _asset4!, comparer, -1);
        CompareAssets(_asset5!, _asset5!, comparer, 0);
        CompareAssets(_asset5!, _asset6!, comparer, -1);
        CompareAssets(_asset5!, _asset7!, comparer, 1);
        CompareAssets(_asset5!, _asset8!, comparer, 1);

        CompareAssets(_asset6!, _asset1!, comparer, 1);
        CompareAssets(_asset6!, _asset2!, comparer, 1);
        CompareAssets(_asset6!, _asset3!, comparer, 1);
        CompareAssets(_asset6!, _asset4!, comparer, 1);
        CompareAssets(_asset6!, _asset5!, comparer, 1);
        CompareAssets(_asset6!, _asset6!, comparer, 0);
        CompareAssets(_asset6!, _asset7!, comparer, 1);
        CompareAssets(_asset6!, _asset8!, comparer, 1);

        CompareAssets(_asset7!, _asset1!, comparer, -1);
        CompareAssets(_asset7!, _asset2!, comparer, -1);
        CompareAssets(_asset7!, _asset3!, comparer, -1);
        CompareAssets(_asset7!, _asset4!, comparer, -1);
        CompareAssets(_asset7!, _asset5!, comparer, -1);
        CompareAssets(_asset7!, _asset6!, comparer, -1);
        CompareAssets(_asset7!, _asset7!, comparer, 0);
        CompareAssets(_asset7!, _asset8!, comparer, 1);

        CompareAssets(_asset8!, _asset1!, comparer, -1);
        CompareAssets(_asset8!, _asset2!, comparer, -1);
        CompareAssets(_asset8!, _asset3!, comparer, -1);
        CompareAssets(_asset8!, _asset4!, comparer, -1);
        CompareAssets(_asset8!, _asset5!, comparer, -1);
        CompareAssets(_asset8!, _asset6!, comparer, -1);
        CompareAssets(_asset8!, _asset7!, comparer, -1);
        CompareAssets(_asset8!, _asset8!, comparer, 0);
    }

    [Test]
    public void Compare_AscendingIsFalseAndFilePropertiesCreation_ReturnsExpectedResults()
    {
        DateTimeAssetComparer comparer = new(false, asset => asset.FileProperties.Creation);

        CompareAssets(_asset1!, _asset1!, comparer, 0);
        CompareAssets(_asset1!, _asset2!, comparer, -1);
        CompareAssets(_asset1!, _asset3!, comparer, -1);
        CompareAssets(_asset1!, _asset4!, comparer, -1);
        CompareAssets(_asset1!, _asset5!, comparer, -1);
        CompareAssets(_asset1!, _asset6!, comparer, 1);
        CompareAssets(_asset1!, _asset7!, comparer, -1);
        CompareAssets(_asset1!, _asset8!, comparer, -1);

        CompareAssets(_asset2!, _asset1!, comparer, 1);
        CompareAssets(_asset2!, _asset2!, comparer, 0);
        CompareAssets(_asset2!, _asset3!, comparer, -1);
        CompareAssets(_asset2!, _asset4!, comparer, -1);
        CompareAssets(_asset2!, _asset5!, comparer, -1);
        CompareAssets(_asset2!, _asset6!, comparer, 1);
        CompareAssets(_asset2!, _asset7!, comparer, -1);
        CompareAssets(_asset2!, _asset8!, comparer, -1);

        CompareAssets(_asset3!, _asset1!, comparer, 1);
        CompareAssets(_asset3!, _asset2!, comparer, 1);
        CompareAssets(_asset3!, _asset3!, comparer, 0);
        CompareAssets(_asset3!, _asset4!, comparer, -1);
        CompareAssets(_asset3!, _asset5!, comparer, -1);
        CompareAssets(_asset3!, _asset6!, comparer, 1);
        CompareAssets(_asset3!, _asset7!, comparer, -1);
        CompareAssets(_asset3!, _asset8!, comparer, -1);

        CompareAssets(_asset4!, _asset1!, comparer, 1);
        CompareAssets(_asset4!, _asset2!, comparer, 1);
        CompareAssets(_asset4!, _asset3!, comparer, 1);
        CompareAssets(_asset4!, _asset4!, comparer, 0);
        CompareAssets(_asset4!, _asset5!, comparer, -1);
        CompareAssets(_asset4!, _asset6!, comparer, 1);
        CompareAssets(_asset4!, _asset7!, comparer, -1);
        CompareAssets(_asset4!, _asset8!, comparer, -1);

        CompareAssets(_asset5!, _asset1!, comparer, 1);
        CompareAssets(_asset5!, _asset2!, comparer, 1);
        CompareAssets(_asset5!, _asset3!, comparer, 1);
        CompareAssets(_asset5!, _asset4!, comparer, 1);
        CompareAssets(_asset5!, _asset5!, comparer, 0);
        CompareAssets(_asset5!, _asset6!, comparer, 1);
        CompareAssets(_asset5!, _asset7!, comparer, -1);
        CompareAssets(_asset5!, _asset8!, comparer, -1);

        CompareAssets(_asset6!, _asset1!, comparer, -1);
        CompareAssets(_asset6!, _asset2!, comparer, -1);
        CompareAssets(_asset6!, _asset3!, comparer, -1);
        CompareAssets(_asset6!, _asset4!, comparer, -1);
        CompareAssets(_asset6!, _asset5!, comparer, -1);
        CompareAssets(_asset6!, _asset6!, comparer, 0);
        CompareAssets(_asset6!, _asset7!, comparer, -1);
        CompareAssets(_asset6!, _asset8!, comparer, -1);

        CompareAssets(_asset7!, _asset1!, comparer, 1);
        CompareAssets(_asset7!, _asset2!, comparer, 1);
        CompareAssets(_asset7!, _asset3!, comparer, 1);
        CompareAssets(_asset7!, _asset4!, comparer, 1);
        CompareAssets(_asset7!, _asset5!, comparer, 1);
        CompareAssets(_asset7!, _asset6!, comparer, 1);
        CompareAssets(_asset7!, _asset7!, comparer, 0);
        CompareAssets(_asset7!, _asset8!, comparer, -1);

        CompareAssets(_asset8!, _asset1!, comparer, 1);
        CompareAssets(_asset8!, _asset2!, comparer, 1);
        CompareAssets(_asset8!, _asset3!, comparer, 1);
        CompareAssets(_asset8!, _asset4!, comparer, 1);
        CompareAssets(_asset8!, _asset5!, comparer, 1);
        CompareAssets(_asset8!, _asset6!, comparer, 1);
        CompareAssets(_asset8!, _asset7!, comparer, 1);
        CompareAssets(_asset8!, _asset8!, comparer, 0);
    }

    [Test]
    public void Compare_AscendingIsTrueAndFilePropertiesModification_ReturnsExpectedResults()
    {
        DateTimeAssetComparer comparer = new(true, asset => asset.FileProperties.Modification);

        CompareAssets(_asset1!, _asset1!, comparer, 0);
        CompareAssets(_asset1!, _asset2!, comparer, 1);
        CompareAssets(_asset1!, _asset3!, comparer, 1);
        CompareAssets(_asset1!, _asset4!, comparer, 1);
        CompareAssets(_asset1!, _asset5!, comparer, 1);
        CompareAssets(_asset1!, _asset6!, comparer, -1);
        CompareAssets(_asset1!, _asset7!, comparer, 1);
        CompareAssets(_asset1!, _asset8!, comparer, 1);

        CompareAssets(_asset2!, _asset1!, comparer, -1);
        CompareAssets(_asset2!, _asset2!, comparer, 0);
        CompareAssets(_asset2!, _asset3!, comparer, 1);
        CompareAssets(_asset2!, _asset4!, comparer, 1);
        CompareAssets(_asset2!, _asset5!, comparer, 1);
        CompareAssets(_asset2!, _asset6!, comparer, -1);
        CompareAssets(_asset2!, _asset7!, comparer, 1);
        CompareAssets(_asset2!, _asset8!, comparer, 1);

        CompareAssets(_asset3!, _asset1!, comparer, -1);
        CompareAssets(_asset3!, _asset2!, comparer, -1);
        CompareAssets(_asset3!, _asset3!, comparer, 0);
        CompareAssets(_asset3!, _asset4!, comparer, 1);
        CompareAssets(_asset3!, _asset5!, comparer, 1);
        CompareAssets(_asset3!, _asset6!, comparer, -1);
        CompareAssets(_asset3!, _asset7!, comparer, 1);
        CompareAssets(_asset3!, _asset8!, comparer, 1);

        CompareAssets(_asset4!, _asset1!, comparer, -1);
        CompareAssets(_asset4!, _asset2!, comparer, -1);
        CompareAssets(_asset4!, _asset3!, comparer, -1);
        CompareAssets(_asset4!, _asset4!, comparer, 0);
        CompareAssets(_asset4!, _asset5!, comparer, 1);
        CompareAssets(_asset4!, _asset6!, comparer, -1);
        CompareAssets(_asset4!, _asset7!, comparer, 1);
        CompareAssets(_asset4!, _asset8!, comparer, 1);

        CompareAssets(_asset5!, _asset1!, comparer, -1);
        CompareAssets(_asset5!, _asset2!, comparer, -1);
        CompareAssets(_asset5!, _asset3!, comparer, -1);
        CompareAssets(_asset5!, _asset4!, comparer, -1);
        CompareAssets(_asset5!, _asset5!, comparer, 0);
        CompareAssets(_asset5!, _asset6!, comparer, -1);
        CompareAssets(_asset5!, _asset7!, comparer, 1);
        CompareAssets(_asset5!, _asset8!, comparer, 1);

        CompareAssets(_asset6!, _asset1!, comparer, 1);
        CompareAssets(_asset6!, _asset2!, comparer, 1);
        CompareAssets(_asset6!, _asset3!, comparer, 1);
        CompareAssets(_asset6!, _asset4!, comparer, 1);
        CompareAssets(_asset6!, _asset5!, comparer, 1);
        CompareAssets(_asset6!, _asset6!, comparer, 0);
        CompareAssets(_asset6!, _asset7!, comparer, 1);
        CompareAssets(_asset6!, _asset8!, comparer, 1);

        CompareAssets(_asset7!, _asset1!, comparer, -1);
        CompareAssets(_asset7!, _asset2!, comparer, -1);
        CompareAssets(_asset7!, _asset3!, comparer, -1);
        CompareAssets(_asset7!, _asset4!, comparer, -1);
        CompareAssets(_asset7!, _asset5!, comparer, -1);
        CompareAssets(_asset7!, _asset6!, comparer, -1);
        CompareAssets(_asset7!, _asset7!, comparer, 0);
        CompareAssets(_asset7!, _asset8!, comparer, 1);

        CompareAssets(_asset8!, _asset1!, comparer, -1);
        CompareAssets(_asset8!, _asset2!, comparer, -1);
        CompareAssets(_asset8!, _asset3!, comparer, -1);
        CompareAssets(_asset8!, _asset4!, comparer, -1);
        CompareAssets(_asset8!, _asset5!, comparer, -1);
        CompareAssets(_asset8!, _asset6!, comparer, -1);
        CompareAssets(_asset8!, _asset7!, comparer, -1);
        CompareAssets(_asset8!, _asset8!, comparer, 0);
    }

    [Test]
    public void Compare_AscendingIsFalseAndFilePropertiesModification_ReturnsExpectedResults()
    {
        DateTimeAssetComparer comparer = new(false, asset => asset.FileProperties.Modification);

        CompareAssets(_asset1!, _asset1!, comparer, 0);
        CompareAssets(_asset1!, _asset2!, comparer, -1);
        CompareAssets(_asset1!, _asset3!, comparer, -1);
        CompareAssets(_asset1!, _asset4!, comparer, -1);
        CompareAssets(_asset1!, _asset5!, comparer, -1);
        CompareAssets(_asset1!, _asset6!, comparer, 1);
        CompareAssets(_asset1!, _asset7!, comparer, -1);
        CompareAssets(_asset1!, _asset8!, comparer, -1);

        CompareAssets(_asset2!, _asset1!, comparer, 1);
        CompareAssets(_asset2!, _asset2!, comparer, 0);
        CompareAssets(_asset2!, _asset3!, comparer, -1);
        CompareAssets(_asset2!, _asset4!, comparer, -1);
        CompareAssets(_asset2!, _asset5!, comparer, -1);
        CompareAssets(_asset2!, _asset6!, comparer, 1);
        CompareAssets(_asset2!, _asset7!, comparer, -1);
        CompareAssets(_asset2!, _asset8!, comparer, -1);

        CompareAssets(_asset3!, _asset1!, comparer, 1);
        CompareAssets(_asset3!, _asset2!, comparer, 1);
        CompareAssets(_asset3!, _asset3!, comparer, 0);
        CompareAssets(_asset3!, _asset4!, comparer, -1);
        CompareAssets(_asset3!, _asset5!, comparer, -1);
        CompareAssets(_asset3!, _asset6!, comparer, 1);
        CompareAssets(_asset3!, _asset7!, comparer, -1);
        CompareAssets(_asset3!, _asset8!, comparer, -1);

        CompareAssets(_asset4!, _asset1!, comparer, 1);
        CompareAssets(_asset4!, _asset2!, comparer, 1);
        CompareAssets(_asset4!, _asset3!, comparer, 1);
        CompareAssets(_asset4!, _asset4!, comparer, 0);
        CompareAssets(_asset4!, _asset5!, comparer, -1);
        CompareAssets(_asset4!, _asset6!, comparer, 1);
        CompareAssets(_asset4!, _asset7!, comparer, -1);
        CompareAssets(_asset4!, _asset8!, comparer, -1);

        CompareAssets(_asset5!, _asset1!, comparer, 1);
        CompareAssets(_asset5!, _asset2!, comparer, 1);
        CompareAssets(_asset5!, _asset3!, comparer, 1);
        CompareAssets(_asset5!, _asset4!, comparer, 1);
        CompareAssets(_asset5!, _asset5!, comparer, 0);
        CompareAssets(_asset5!, _asset6!, comparer, 1);
        CompareAssets(_asset5!, _asset7!, comparer, -1);
        CompareAssets(_asset5!, _asset8!, comparer, -1);

        CompareAssets(_asset6!, _asset1!, comparer, -1);
        CompareAssets(_asset6!, _asset2!, comparer, -1);
        CompareAssets(_asset6!, _asset3!, comparer, -1);
        CompareAssets(_asset6!, _asset4!, comparer, -1);
        CompareAssets(_asset6!, _asset5!, comparer, -1);
        CompareAssets(_asset6!, _asset6!, comparer, 0);
        CompareAssets(_asset6!, _asset7!, comparer, -1);
        CompareAssets(_asset6!, _asset8!, comparer, -1);

        CompareAssets(_asset7!, _asset1!, comparer, 1);
        CompareAssets(_asset7!, _asset2!, comparer, 1);
        CompareAssets(_asset7!, _asset3!, comparer, 1);
        CompareAssets(_asset7!, _asset4!, comparer, 1);
        CompareAssets(_asset7!, _asset5!, comparer, 1);
        CompareAssets(_asset7!, _asset6!, comparer, 1);
        CompareAssets(_asset7!, _asset7!, comparer, 0);
        CompareAssets(_asset7!, _asset8!, comparer, -1);

        CompareAssets(_asset8!, _asset1!, comparer, 1);
        CompareAssets(_asset8!, _asset2!, comparer, 1);
        CompareAssets(_asset8!, _asset3!, comparer, 1);
        CompareAssets(_asset8!, _asset4!, comparer, 1);
        CompareAssets(_asset8!, _asset5!, comparer, 1);
        CompareAssets(_asset8!, _asset6!, comparer, 1);
        CompareAssets(_asset8!, _asset7!, comparer, 1);
        CompareAssets(_asset8!, _asset8!, comparer, 0);
    }

    [Test]
    public void Compare_AscendingIsTrueAndFirstDateTimeIsMoreRecent_ReturnsPositiveNumber()
    {
        DateTimeAssetComparer comparer = new(true, asset => asset.ThumbnailCreationDateTime);

        int result = comparer.Compare(_asset1, _asset2);

        Assert.That(result, Is.GreaterThan(0));
    }

    [Test]
    public void Compare_AscendingIsTrueAndFirstDateTimeIsLessRecent_ReturnsNegativeNumber()
    {
        DateTimeAssetComparer comparer = new(true, asset => asset.ThumbnailCreationDateTime);

        int result = comparer.Compare(_asset2, _asset1);

        Assert.That(result, Is.LessThan(0));
    }

    [Test]
    public void Compare_AscendingIsTrueAndDateTimesAreEqualButStringsAreNotEqual_ReturnsNegativeNumber()
    {
        DateTimeAssetComparer comparer = new(true, asset => asset.ThumbnailCreationDateTime);

        int result = comparer.Compare(_asset1, _asset6);

        Assert.That(result, Is.LessThan(0));
    }

    [Test]
    public void Compare_AscendingIsFalseAndFirstDateTimeIsMoreRecent_ReturnsNegativeNumber()
    {
        DateTimeAssetComparer comparer = new(false, asset => asset.ThumbnailCreationDateTime);

        int result = comparer.Compare(_asset1, _asset2);

        Assert.That(result, Is.LessThan(0));
    }

    [Test]
    public void Compare_AscendingIsFalseAndFirstDateTimeIsLessRecent_ReturnsPositiveNumber()
    {
        DateTimeAssetComparer comparer = new(false, asset => asset.ThumbnailCreationDateTime);

        int result = comparer.Compare(_asset2, _asset1);

        Assert.That(result, Is.GreaterThan(0));
    }

    [Test]
    public void Compare_AscendingIsFalseAndDateTimesAreEqualButStringsAreNotEqual_ReturnsPositiveNumber()
    {
        DateTimeAssetComparer comparer = new(false, asset => asset.ThumbnailCreationDateTime);

        int result = comparer.Compare(_asset1, _asset6);

        Assert.That(result, Is.GreaterThan(0));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void Compare_FirstAssetIsNull_ThrowsArgumentNullException(bool ascending)
    {
        DateTimeAssetComparer comparer = new(ascending, asset => asset.ThumbnailCreationDateTime);

        Asset? asset1 = null;

        Assert.Throws<ArgumentNullException>(() => _ = comparer.Compare(asset1, _asset2));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void Compare_SecondAssetIsNull_ThrowsArgumentNullException(bool ascending)
    {
        DateTimeAssetComparer comparer = new(ascending, asset => asset.ThumbnailCreationDateTime);

        Asset? asset2 = null;

        Assert.Throws<ArgumentNullException>(() => _ = comparer.Compare(_asset1, asset2));
    }

    private static void CompareAssets(Asset asset1, Asset asset2, DateTimeAssetComparer comparer, int expectedResult)
    {
        int result = comparer.Compare(asset1, asset2);
        Assert.That(Math.Sign(result), Is.EqualTo(expectedResult));
    }
}
