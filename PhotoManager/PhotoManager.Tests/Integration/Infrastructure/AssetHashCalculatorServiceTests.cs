using ImageMagick;

namespace PhotoManager.Tests.Integration.Infrastructure;

[TestFixture]
public class AssetHashCalculatorServiceTests
{
    private string? dataDirectory;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
    }

    [Test]
    [TestCase("Image 1.jpg", "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9")]
    [TestCase("Image 1_90_deg.jpg", "0092081bd0a83e9253f4eb319b4cfab197a09ed3b28b5697c31f17bf2646638dec95bf28d4f454dda8ff3dcdaab33fa002774f13dfe6d7aee82f1a960e3e3aee")]
    [TestCase("Image 1_180_deg.jpg", "399383d82d4b7ed8233f714a1d14272b276b80505d7b3a2935b6ec962fd5cf280ccedb3e9380df1a11ee65e5f5fa66278dd2f407b7b6bdfa3093319164c446dd")]
    [TestCase("Image 1_270_deg.jpg", "43950e06cecd509509d868a2064a828aeb6ff6a9a30f296ec9a3621b269742e75dacb40300aa6927c4c8f0879268e1ab3d7eac228b38cade35cc9b7c6adf3449")]
    [TestCase("Image 8.jpeg", "156f81ceb3db1bfdf27618efe4b89575f17d4708053c1a9ce6c5dfb7f36a6b958a5c3f45147728de28cf538345336e7b81d01b3f5908d4677ce76e1661e97ac3")]
    [TestCase("Image 10 portrait.png", "7ad01e9fe639957a9e8eaddc7fd864068f4a03f9981fd480f310740a43a0a4f2b8fa7a80d9c83039c46fcfbb63a5e465adaf07d33191369590adcda1586b1c94")]
    [TestCase("Homer.gif", "c48b1f61f3a3a004f425d8493d30a50ae14408ed4c5354bf4d0ca40069f91951381a7df32ee7455a6edef0996c95571557a9993021331ff2dfbc3ccc7f0c8ff1")]
    [TestCase("Image_11.heic", "df43c6dffbb9bd64c7b3b609f656e57a63e50e8d1784c72bb09c7b1df362fe239a68ddb85822f5afb7d7ef35a60ffa58904c2b32dd8a3a1fe7cae23030f00931")]
    public void CalculateHash_DefaultHashAndValidImageBytesAndFilePath_ReturnsCorrectHash(string fileName, string expectedHash)
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, false, false);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);

        string filePath = Path.Combine(dataDirectory!, fileName);
        byte[] imageBytes = File.ReadAllBytes(filePath);

        string hash = assetHashCalculatorService.CalculateHash(imageBytes, filePath);

        Assert.IsFalse(string.IsNullOrWhiteSpace(hash));
        Assert.AreEqual(128, hash.Length); // SHA-512 generates a 128-character long hash in hexadecimal representation
        Assert.AreEqual(expectedHash, hash.ToLower());
    }

    [Test]
    public void CalculateHash_DefaultHashAndEmptyImageBytes_ReturnsSameHash()
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, false, false);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);

        byte[] imageBytes = [];
        string? filePath = null;
        string expectedHash = "cf83e1357eefb8bdf1542850d66d8007d620e4050b5715dc83f4a921d36ce9ce47d0d13c5d85f2b0ff8318d2877eec2f63b931bd47417a81a538327af927da3e";

        string hash = assetHashCalculatorService.CalculateHash(imageBytes, filePath!);

        Assert.IsFalse(string.IsNullOrWhiteSpace(hash));
        Assert.AreEqual(128, hash.Length);
        Assert.AreEqual(expectedHash, hash.ToLower());
    }

    [Test]
    public void CalculateHash_DefaultHashAndNullImageBytes_ThrowsArgumentNullException()
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, false, false);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);

        byte[]? imageBytes = null;
        string? filePath = null;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => assetHashCalculatorService.CalculateHash(imageBytes!, filePath!));

        Assert.AreEqual("Value cannot be null. (Parameter 'source')", exception?.Message);
    }

    [Test]
    [TestCase("Image 1.jpg", "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9")]
    [TestCase("Image 1_90_deg.jpg", "0092081bd0a83e9253f4eb319b4cfab197a09ed3b28b5697c31f17bf2646638dec95bf28d4f454dda8ff3dcdaab33fa002774f13dfe6d7aee82f1a960e3e3aee")]
    [TestCase("Image 1_180_deg.jpg", "399383d82d4b7ed8233f714a1d14272b276b80505d7b3a2935b6ec962fd5cf280ccedb3e9380df1a11ee65e5f5fa66278dd2f407b7b6bdfa3093319164c446dd")]
    [TestCase("Image 1_270_deg.jpg", "43950e06cecd509509d868a2064a828aeb6ff6a9a30f296ec9a3621b269742e75dacb40300aa6927c4c8f0879268e1ab3d7eac228b38cade35cc9b7c6adf3449")]
    [TestCase("Image 8.jpeg", "156f81ceb3db1bfdf27618efe4b89575f17d4708053c1a9ce6c5dfb7f36a6b958a5c3f45147728de28cf538345336e7b81d01b3f5908d4677ce76e1661e97ac3")]
    [TestCase("Image 10 portrait.png", "7ad01e9fe639957a9e8eaddc7fd864068f4a03f9981fd480f310740a43a0a4f2b8fa7a80d9c83039c46fcfbb63a5e465adaf07d33191369590adcda1586b1c94")]
    [TestCase("Homer.gif", "c48b1f61f3a3a004f425d8493d30a50ae14408ed4c5354bf4d0ca40069f91951381a7df32ee7455a6edef0996c95571557a9993021331ff2dfbc3ccc7f0c8ff1")]
    [TestCase("Image_11.heic", "df43c6dffbb9bd64c7b3b609f656e57a63e50e8d1784c72bb09c7b1df362fe239a68ddb85822f5afb7d7ef35a60ffa58904c2b32dd8a3a1fe7cae23030f00931")]
    public void CalculateHash_DefaultHashAndValidImageBytesAndNullFilePath_ReturnsCorrectHash(string fileName, string expectedHash)
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, false, false);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);

        string filePath = Path.Combine(dataDirectory!, fileName);
        byte[] imageBytes = File.ReadAllBytes(filePath);

        string hash = assetHashCalculatorService.CalculateHash(imageBytes, null!);

        Assert.IsFalse(string.IsNullOrWhiteSpace(hash));
        Assert.AreEqual(128, hash.Length); // SHA-512 generates a 128-character long hash in hexadecimal representation
        Assert.AreEqual(expectedHash, hash.ToLower());
    }

    [Test]
    [TestCase("Image 1.jpg", "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9")]
    [TestCase("Image 1_90_deg.jpg", "0092081bd0a83e9253f4eb319b4cfab197a09ed3b28b5697c31f17bf2646638dec95bf28d4f454dda8ff3dcdaab33fa002774f13dfe6d7aee82f1a960e3e3aee")]
    [TestCase("Image 1_180_deg.jpg", "399383d82d4b7ed8233f714a1d14272b276b80505d7b3a2935b6ec962fd5cf280ccedb3e9380df1a11ee65e5f5fa66278dd2f407b7b6bdfa3093319164c446dd")]
    [TestCase("Image 1_270_deg.jpg", "43950e06cecd509509d868a2064a828aeb6ff6a9a30f296ec9a3621b269742e75dacb40300aa6927c4c8f0879268e1ab3d7eac228b38cade35cc9b7c6adf3449")]
    [TestCase("Image 8.jpeg", "156f81ceb3db1bfdf27618efe4b89575f17d4708053c1a9ce6c5dfb7f36a6b958a5c3f45147728de28cf538345336e7b81d01b3f5908d4677ce76e1661e97ac3")]
    [TestCase("Image 10 portrait.png", "7ad01e9fe639957a9e8eaddc7fd864068f4a03f9981fd480f310740a43a0a4f2b8fa7a80d9c83039c46fcfbb63a5e465adaf07d33191369590adcda1586b1c94")]
    [TestCase("Homer.gif", "c48b1f61f3a3a004f425d8493d30a50ae14408ed4c5354bf4d0ca40069f91951381a7df32ee7455a6edef0996c95571557a9993021331ff2dfbc3ccc7f0c8ff1")]
    [TestCase("Image_11.heic", "df43c6dffbb9bd64c7b3b609f656e57a63e50e8d1784c72bb09c7b1df362fe239a68ddb85822f5afb7d7ef35a60ffa58904c2b32dd8a3a1fe7cae23030f00931")]
    public void CalculateHash_DefaultHashAndValidImageBytesAndInvalidFilePath_ReturnsCorrectHash(string fileName, string expectedHash)
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, false, false);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);

        string filePath = Path.Combine(dataDirectory!, fileName);
        byte[] imageBytes = File.ReadAllBytes(filePath);

        string hash = assetHashCalculatorService.CalculateHash(imageBytes, "invalid_path/nonexistent.jpg");

        Assert.IsFalse(string.IsNullOrWhiteSpace(hash));
        Assert.AreEqual(128, hash.Length); // SHA-512 generates a 128-character long hash in hexadecimal representation
        Assert.AreEqual(expectedHash, hash.ToLower());
    }

    [Test]
    [TestCase("Image 1.jpg", "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9")]
    [TestCase("Image 1_90_deg.jpg", "0092081bd0a83e9253f4eb319b4cfab197a09ed3b28b5697c31f17bf2646638dec95bf28d4f454dda8ff3dcdaab33fa002774f13dfe6d7aee82f1a960e3e3aee")]
    [TestCase("Image 1_180_deg.jpg", "399383d82d4b7ed8233f714a1d14272b276b80505d7b3a2935b6ec962fd5cf280ccedb3e9380df1a11ee65e5f5fa66278dd2f407b7b6bdfa3093319164c446dd")]
    [TestCase("Image 1_270_deg.jpg", "43950e06cecd509509d868a2064a828aeb6ff6a9a30f296ec9a3621b269742e75dacb40300aa6927c4c8f0879268e1ab3d7eac228b38cade35cc9b7c6adf3449")]
    [TestCase("Image 8.jpeg", "156f81ceb3db1bfdf27618efe4b89575f17d4708053c1a9ce6c5dfb7f36a6b958a5c3f45147728de28cf538345336e7b81d01b3f5908d4677ce76e1661e97ac3")]
    [TestCase("Image 10 portrait.png", "7ad01e9fe639957a9e8eaddc7fd864068f4a03f9981fd480f310740a43a0a4f2b8fa7a80d9c83039c46fcfbb63a5e465adaf07d33191369590adcda1586b1c94")]
    [TestCase("Homer.gif", "c48b1f61f3a3a004f425d8493d30a50ae14408ed4c5354bf4d0ca40069f91951381a7df32ee7455a6edef0996c95571557a9993021331ff2dfbc3ccc7f0c8ff1")]
    [TestCase("Image_11.heic", "df43c6dffbb9bd64c7b3b609f656e57a63e50e8d1784c72bb09c7b1df362fe239a68ddb85822f5afb7d7ef35a60ffa58904c2b32dd8a3a1fe7cae23030f00931")]
    public void CalculateHash_DefaultHashAndValidImageBytesAndDirectoryFilePath_ReturnsCorrectHash(string fileName, string expectedHash)
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, false, false);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);

        string filePath = Path.Combine(dataDirectory!, fileName);
        byte[] imageBytes = File.ReadAllBytes(filePath);

        string hash = assetHashCalculatorService.CalculateHash(imageBytes, dataDirectory!);

        Assert.IsFalse(string.IsNullOrWhiteSpace(hash));
        Assert.AreEqual(128, hash.Length); // SHA-512 generates a 128-character long hash in hexadecimal representation
        Assert.AreEqual(expectedHash, hash.ToLower());
    }

    [Test]
    [TestCase("Image 1.jpg", "afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee0afbaa849d28fc2b8dc1262d9e619b362ee0")]
    [TestCase("Image 1_90_deg.jpg", "afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee0afbaa849d28fc2b8dc1262d9e619b362ee0")]
    [TestCase("Image 1_180_deg.jpg", "afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee0afbaa849d28fc2b8dc1262d9e619b362ee0")]
    [TestCase("Image 1_270_deg.jpg", "afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee0afbaa849d28fc2b8dc1262d9e619b362ee0")]
    [TestCase("Image 8.jpeg", "afb8a849cc8fc608dc2362da4619b462ee062ee062ee062ee062ee062ee062ee062ee0afb8a849cc8fc608dc2362da4619b462ee062ee062ee062ee062ee062ee062ee062ee0afb8a849cc8fc608dc2362da4619b462ee0afb8a849cc8fc608dc2362da4619b462ee0")]
    [TestCase("Image 10 portrait.png", "afaff849b08fd348dc1f62dae619b262ee062ee062ee062ee062ee062ee062ee062ee0afaff849b08fd348dc1f62dae619b262ee062ee062ee062ee062ee062ee062ee062ee0afaff849b08fd348dc1f62dae619b262ee0afaff849b08fd348dc1f62dae619b262ee0")]
    [TestCase("Homer.gif", "af783881028a11d8ad696231b8ee326225d62ee062ee062ee062ee062ee062ee062ee0af783881028a11d8ad696231b8ee326225d62ee062ee062ee062ee062ee062ee062ee0af783881028a11d8ad696231b8ee326225daf783881028a11d8ad696231b8ee326225d")]
    [TestCase("Image_11.heic", "a926f84a9188106894a161cc28d7cf6205662ee062ee062ee062ee062ee062ee062ee0a926f84a9188106894a161cc28d7cf6205662ee062ee062ee062ee062ee062ee062ee0a926f84a9188106894a161cc28d7cf62056a926f84a9188106894a161cc28d7cf62056")]
    public void CalculateHash_PHashAndValidImageBytesAndFilePath_ReturnsCorrectPHash(string fileName, string expectedHash)
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(true, false, false);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);

        string filePath = Path.Combine(dataDirectory!, fileName);
        byte[] imageBytes = File.ReadAllBytes(filePath);

        string? hash = assetHashCalculatorService.CalculateHash(imageBytes, filePath);

        Assert.IsFalse(string.IsNullOrWhiteSpace(hash));
        Assert.AreEqual(210, hash?.Length); // The PHash is a 210-character hexadecimal string
        Assert.AreEqual(expectedHash, hash?.ToLower());
    }

    [Test]
    [TestCase("Image 1.jpg", "afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee0afbaa849d28fc2b8dc1262d9e619b362ee0")]
    [TestCase("Image 1_90_deg.jpg", "afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee0afbaa849d28fc2b8dc1262d9e619b362ee0")]
    [TestCase("Image 1_180_deg.jpg", "afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee0afbaa849d28fc2b8dc1262d9e619b362ee0")]
    [TestCase("Image 1_270_deg.jpg", "afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee0afbaa849d28fc2b8dc1262d9e619b362ee0")]
    [TestCase("Image 8.jpeg", "afb8a849cc8fc608dc2362da4619b462ee062ee062ee062ee062ee062ee062ee062ee0afb8a849cc8fc608dc2362da4619b462ee062ee062ee062ee062ee062ee062ee062ee0afb8a849cc8fc608dc2362da4619b462ee0afb8a849cc8fc608dc2362da4619b462ee0")]
    [TestCase("Image 10 portrait.png", "afaff849b08fd348dc1f62dae619b262ee062ee062ee062ee062ee062ee062ee062ee0afaff849b08fd348dc1f62dae619b262ee062ee062ee062ee062ee062ee062ee062ee0afaff849b08fd348dc1f62dae619b262ee0afaff849b08fd348dc1f62dae619b262ee0")]
    [TestCase("Homer.gif", "af783881028a11d8ad696231b8ee326225d62ee062ee062ee062ee062ee062ee062ee0af783881028a11d8ad696231b8ee326225d62ee062ee062ee062ee062ee062ee062ee0af783881028a11d8ad696231b8ee326225daf783881028a11d8ad696231b8ee326225d")]
    [TestCase("Image_11.heic", "a926f84a9188106894a161cc28d7cf6205662ee062ee062ee062ee062ee062ee062ee0a926f84a9188106894a161cc28d7cf6205662ee062ee062ee062ee062ee062ee062ee0a926f84a9188106894a161cc28d7cf62056a926f84a9188106894a161cc28d7cf62056")]
    public void CalculateHash_PHashAndEmptyImageBytesAndFilePath_ReturnsCorrectPHash(string fileName, string expectedHash)
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(true, false, false);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);

        byte[] imageBytes = [];
        string filePath = Path.Combine(dataDirectory!, fileName);

        string? hash = assetHashCalculatorService.CalculateHash(imageBytes, filePath);

        Assert.IsFalse(string.IsNullOrWhiteSpace(hash));
        Assert.AreEqual(210, hash?.Length); // The PHash is a 210-character hexadecimal string
        Assert.AreEqual(expectedHash, hash?.ToLower());
    }

    [Test]
    [TestCase("Image 1.jpg", "afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee0afbaa849d28fc2b8dc1262d9e619b362ee0")]
    [TestCase("Image 1_90_deg.jpg", "afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee0afbaa849d28fc2b8dc1262d9e619b362ee0")]
    [TestCase("Image 1_180_deg.jpg", "afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee0afbaa849d28fc2b8dc1262d9e619b362ee0")]
    [TestCase("Image 1_270_deg.jpg", "afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee0afbaa849d28fc2b8dc1262d9e619b362ee0")]
    [TestCase("Image 8.jpeg", "afb8a849cc8fc608dc2362da4619b462ee062ee062ee062ee062ee062ee062ee062ee0afb8a849cc8fc608dc2362da4619b462ee062ee062ee062ee062ee062ee062ee062ee0afb8a849cc8fc608dc2362da4619b462ee0afb8a849cc8fc608dc2362da4619b462ee0")]
    [TestCase("Image 10 portrait.png", "afaff849b08fd348dc1f62dae619b262ee062ee062ee062ee062ee062ee062ee062ee0afaff849b08fd348dc1f62dae619b262ee062ee062ee062ee062ee062ee062ee062ee0afaff849b08fd348dc1f62dae619b262ee0afaff849b08fd348dc1f62dae619b262ee0")]
    [TestCase("Homer.gif", "af783881028a11d8ad696231b8ee326225d62ee062ee062ee062ee062ee062ee062ee0af783881028a11d8ad696231b8ee326225d62ee062ee062ee062ee062ee062ee062ee0af783881028a11d8ad696231b8ee326225daf783881028a11d8ad696231b8ee326225d")]
    [TestCase("Image_11.heic", "a926f84a9188106894a161cc28d7cf6205662ee062ee062ee062ee062ee062ee062ee0a926f84a9188106894a161cc28d7cf6205662ee062ee062ee062ee062ee062ee062ee0a926f84a9188106894a161cc28d7cf62056a926f84a9188106894a161cc28d7cf62056")]
    public void CalculateHash_PHashAndNullImageBytesAndFilePath_ReturnsCorrectPHash(string fileName, string expectedHash)
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(true, false, false);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);

        byte[]? imageBytes = null;
        string filePath = Path.Combine(dataDirectory!, fileName);

        string? hash = assetHashCalculatorService.CalculateHash(imageBytes!, filePath);

        Assert.IsFalse(string.IsNullOrWhiteSpace(hash));
        Assert.AreEqual(210, hash?.Length); // The PHash is a 210-character hexadecimal string
        Assert.AreEqual(expectedHash, hash?.ToLower());
    }

    [Test]
    public void CalculateHash_PHashAndImageDoesNotExist_ThrowsMagickBlobErrorException()
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(true, false, false);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);

        byte[]? imageBytes = null;
        string filePath = Path.Combine(dataDirectory!, "ImageDoesNotExist.png");

        MagickBlobErrorException? exception = Assert.Throws<MagickBlobErrorException>(() => assetHashCalculatorService.CalculateHash(imageBytes!, filePath));

        Assert.AreEqual($"unable to open image '{filePath}': No such file or directory @ error/blob.c/OpenBlob/3573", exception?.Message);
    }

    [Test]
    public void CalculateHash_PHashAndImagePathIsInvalid_ThrowsMagickBlobErrorException()
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(true, false, false);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);

        byte[]? imageBytes = null;

        MagickBlobErrorException? exception = Assert.Throws<MagickBlobErrorException>(() => assetHashCalculatorService.CalculateHash(imageBytes!, dataDirectory!));

        Assert.AreEqual($"unable to open image '{dataDirectory!}': Permission denied @ error/blob.c/OpenBlob/3573", exception?.Message);
    }

    [Test]
    public void CalculateHash_PHashAndImagePathIsNull_ThrowsArgumentNullException()
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(true, false, false);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);

        byte[]? imageBytes = null;
        string? filePath = null;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => assetHashCalculatorService.CalculateHash(imageBytes!, filePath!));

        Assert.AreEqual("Value cannot be null or empty. (Parameter 'fileName')", exception?.Message);
    }

    [Test]
    [TestCase("Image 1.jpg", "23831507941108244")]
    [TestCase("Image 1_90_deg.jpg", "23831507941108244")]
    [TestCase("Image 1_180_deg.jpg", "23831507941108244")]
    [TestCase("Image 1_270_deg.jpg", "23831507941108244")]
    [TestCase("Image 8.jpeg", "23831507941108244")]
    [TestCase("Image 10 portrait.png", "36116814863094786")]
    public void CalculateHash_DHashAndValidImageBytesAndFilePath_ReturnsCorrectDHash(string fileName, string expectedHash)
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, true, false);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);

        string filePath = Path.Combine(dataDirectory!, fileName);
        byte[] imageBytes = File.ReadAllBytes(filePath);

        string hash = assetHashCalculatorService.CalculateHash(imageBytes, filePath);

        Assert.IsFalse(string.IsNullOrWhiteSpace(hash));
        Assert.AreEqual(17, hash.Length); // The DHash is a 17-character number
        Assert.AreEqual(expectedHash, hash);
    }

    [Test]
    [TestCase("Homer.gif", "0")]
    public void CalculateHash_DHashAndGifImagePath_ReturnsCorrectDHash(string fileName, string expectedHash)
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, true, false);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);

        string filePath = Path.Combine(dataDirectory!, fileName);
        byte[] imageBytes = File.ReadAllBytes(filePath);

        string hash = assetHashCalculatorService.CalculateHash(imageBytes, filePath);

        Assert.IsFalse(string.IsNullOrWhiteSpace(hash));
        Assert.AreEqual(1, hash.Length);
        Assert.AreEqual(expectedHash, hash);
    }

    [Test]
    [TestCase("Image_11.heic", "9077567998918656")]
    public void CalculateHash_DHashAndHeicImagePath_ReturnsCorrectDHash(string fileName, string expectedHash)
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, true, false);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);

        string filePath = Path.Combine(dataDirectory!, fileName);
        byte[] imageBytes = File.ReadAllBytes(filePath);

        string hash = assetHashCalculatorService.CalculateHash(imageBytes, filePath);

        Assert.IsFalse(string.IsNullOrWhiteSpace(hash));
        Assert.AreEqual(16, hash.Length); // The DHash is a 16-character number for heic file
        Assert.AreEqual(expectedHash, hash);
    }

    [Test]
    [TestCase("Image 1.jpg", "23831507941108244")]
    [TestCase("Image 1_90_deg.jpg", "23831507941108244")]
    [TestCase("Image 1_180_deg.jpg", "23831507941108244")]
    [TestCase("Image 1_270_deg.jpg", "23831507941108244")]
    [TestCase("Image 8.jpeg", "23831507941108244")]
    [TestCase("Image 10 portrait.png", "36116814863094786")]
    public void CalculateHash_DHashAndEmptyImageBytesAndFilePath_ReturnsCorrectDHash(string fileName, string expectedHash)
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, true, false);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);

        byte[] imageBytes = [];
        string filePath = Path.Combine(dataDirectory!, fileName);

        string hash = assetHashCalculatorService.CalculateHash(imageBytes, filePath);

        Assert.IsFalse(string.IsNullOrWhiteSpace(hash));
        Assert.AreEqual(17, hash.Length); // The DHash is a 17-character number
        Assert.AreEqual(expectedHash, hash);
    }

    [Test]
    [TestCase("Homer.gif", "0")]
    public void CalculateHash_DHashAndEmptyImageBytesAndGifImagePath_ReturnsCorrectDHash(string fileName, string expectedHash)
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, true, false);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);

        byte[] imageBytes = [];
        string filePath = Path.Combine(dataDirectory!, fileName);

        string hash = assetHashCalculatorService.CalculateHash(imageBytes, filePath);

        Assert.IsFalse(string.IsNullOrWhiteSpace(hash));
        Assert.AreEqual(1, hash.Length);
        Assert.AreEqual(expectedHash, hash);
    }

    [Test]
    [TestCase("Image_11.heic", "9077567998918656")]
    public void CalculateHash_DHashAndEmptyImageBytesAndHeicImagePath_ReturnsCorrectDHash(string fileName, string expectedHash)
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, true, false);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);

        byte[] imageBytes = [];
        string filePath = Path.Combine(dataDirectory!, fileName);

        string hash = assetHashCalculatorService.CalculateHash(imageBytes, filePath);

        Assert.IsFalse(string.IsNullOrWhiteSpace(hash));
        Assert.AreEqual(16, hash.Length); // The DHash is a 16-character number for heic file
        Assert.AreEqual(expectedHash, hash);
    }

    [Test]
    [TestCase("Image 1.jpg", "23831507941108244")]
    [TestCase("Image 1_90_deg.jpg", "23831507941108244")]
    [TestCase("Image 1_180_deg.jpg", "23831507941108244")]
    [TestCase("Image 1_270_deg.jpg", "23831507941108244")]
    [TestCase("Image 8.jpeg", "23831507941108244")]
    [TestCase("Image 10 portrait.png", "36116814863094786")]
    public void CalculateHash_DHashAndNullImageBytesAndFilePath_ReturnsCorrectDHash(string fileName, string expectedHash)
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, true, false);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);

        byte[]? imageBytes = null;
        string filePath = Path.Combine(dataDirectory!, fileName);

        string hash = assetHashCalculatorService.CalculateHash(imageBytes!, filePath);

        Assert.IsFalse(string.IsNullOrWhiteSpace(hash));
        Assert.AreEqual(17, hash.Length); // The DHash is a 17-character number
        Assert.AreEqual(expectedHash, hash);
    }

    [Test]
    [TestCase("Homer.gif", "0")]
    public void CalculateHash_DHashAndNullImageBytesAndGifImagePath_ReturnsCorrectDHash(string fileName, string expectedHash)
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, true, false);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);

        byte[]? imageBytes = null;
        string filePath = Path.Combine(dataDirectory!, fileName);

        string hash = assetHashCalculatorService.CalculateHash(imageBytes!, filePath);

        Assert.IsFalse(string.IsNullOrWhiteSpace(hash));
        Assert.AreEqual(1, hash.Length);
        Assert.AreEqual(expectedHash, hash);
    }

    [Test]
    [TestCase("Image_11.heic", "9077567998918656")]
    public void CalculateHash_DHashAndNullImageBytesAndHeicImagePath_ReturnsCorrectDHash(string fileName, string expectedHash)
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, true, false);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);

        byte[]? imageBytes = null;
        string filePath = Path.Combine(dataDirectory!, fileName);

        string hash = assetHashCalculatorService.CalculateHash(imageBytes!, filePath);

        Assert.IsFalse(string.IsNullOrWhiteSpace(hash));
        Assert.AreEqual(16, hash.Length); // The DHash is a 16-character number for heic file
        Assert.AreEqual(expectedHash, hash);
    }

    [Test]
    public void CalculateHash_DHashAndImageDoesNotExist_ThrowsArgumentException()
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, true, false);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);

        byte[]? imageBytes = null;
        string filePath = Path.Combine(dataDirectory!, "ImageDoesNotExist.png");

        ArgumentException? exception = Assert.Throws<ArgumentException>(() => assetHashCalculatorService.CalculateHash(imageBytes!, filePath));

        Assert.AreEqual("Parameter is not valid.", exception?.Message);
    }

    [Test]
    public void CalculateHash_DHashAndImagePathIsInvalid_ThrowsArgumentException()
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, true, false);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);

        byte[]? imageBytes = null;

        ArgumentException? exception = Assert.Throws<ArgumentException>(() => assetHashCalculatorService.CalculateHash(imageBytes!, dataDirectory!));

        Assert.AreEqual("Parameter is not valid.", exception?.Message);
    }

    [Test]
    public void CalculateHash_DHashAndImagePathIsNull_ThrowsArgumentNullException()
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, true, false);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);

        byte[]? imageBytes = null;
        string? filePath = null;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => assetHashCalculatorService.CalculateHash(imageBytes!, filePath!));

        Assert.AreEqual("Value cannot be null. (Parameter 'path')", exception?.Message);
    }

    [Test]
    [TestCase("Image 1.jpg", "a886f237e1cf6da03fb793588fd79b64")]
    [TestCase("Image 1_90_deg.jpg", "e740c5b99b0c87d86b9b1b3f322dec7e")]
    [TestCase("Image 1_180_deg.jpg", "bc43ad1ebd118a2f9443b3199d4127c8")]
    [TestCase("Image 1_270_deg.jpg", "f8f724518021547c26ef9262f29f6b9a")]
    [TestCase("Image 8.jpeg", "4194e88c247b5bbc7a1c6294cc795466")]
    [TestCase("Image 10 portrait.png", "02b09a63d382bc1a1f88afa125f3adb3")]
    [TestCase("Homer.gif", "a409ce713de9334117791b15a586dd0e")]
    [TestCase("Image_11.heic", "24cee7da517186279bafac45072fe622")]
    public void CalculateMD5Hash_MD5HashAndValidImageBytesAndFilePath_ReturnsCorrectMD5Hash(string fileName, string expectedHash)
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, false, true);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);

        string filePath = Path.Combine(dataDirectory!, fileName);
        byte[] imageBytes = File.ReadAllBytes(filePath);

        string hash = assetHashCalculatorService.CalculateHash(imageBytes, filePath);

        Assert.IsFalse(string.IsNullOrWhiteSpace(hash));
        Assert.AreEqual(32, hash.Length); // The MD5Hash is a 32-character hexadecimal string
        Assert.AreEqual(expectedHash, hash);
    }

    [Test]
    public void CalculateMD5Hash_MD5HashAndEmptyImageBytes_ReturnsSameMD5Hash()
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, false, true);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);

        byte[] imageBytes = [];
        string? filePath = null;
        string expectedHash = "d41d8cd98f00b204e9800998ecf8427e";

        string hash = assetHashCalculatorService.CalculateHash(imageBytes, filePath!);

        Assert.IsFalse(string.IsNullOrWhiteSpace(hash));
        Assert.AreEqual(32, hash.Length);
        Assert.AreEqual(expectedHash, hash.ToLower());
    }

    [Test]
    public void CalculateMD5Hash_MD5HashAndNullImageBytes_ThrowsArgumentNullException()
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, false, true);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);

        byte[]? imageBytes = null;
        string? filePath = null;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => assetHashCalculatorService.CalculateHash(imageBytes!, filePath!));

        Assert.AreEqual("Value cannot be null. (Parameter 'source')", exception?.Message);
    }

    [Test]
    [TestCase("Image 1.jpg", "a886f237e1cf6da03fb793588fd79b64")]
    [TestCase("Image 1_90_deg.jpg", "e740c5b99b0c87d86b9b1b3f322dec7e")]
    [TestCase("Image 1_180_deg.jpg", "bc43ad1ebd118a2f9443b3199d4127c8")]
    [TestCase("Image 1_270_deg.jpg", "f8f724518021547c26ef9262f29f6b9a")]
    [TestCase("Image 8.jpeg", "4194e88c247b5bbc7a1c6294cc795466")]
    [TestCase("Image 10 portrait.png", "02b09a63d382bc1a1f88afa125f3adb3")]
    [TestCase("Homer.gif", "a409ce713de9334117791b15a586dd0e")]
    [TestCase("Image_11.heic", "24cee7da517186279bafac45072fe622")]
    public void CalculateMD5Hash_MD5HashAndValidImageBytesAndNullFilePath_ReturnsCorrectMD5Hash(string fileName, string expectedHash)
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, false, true);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);

        string filePath = Path.Combine(dataDirectory!, fileName);
        byte[] imageBytes = File.ReadAllBytes(filePath);

        string hash = assetHashCalculatorService.CalculateHash(imageBytes, filePath);

        Assert.IsFalse(string.IsNullOrWhiteSpace(hash));
        Assert.AreEqual(32, hash.Length); // The MD5Hash is a 32-character hexadecimal string
        Assert.AreEqual(expectedHash, hash);
    }

    [Test]
    [TestCase("Image 1.jpg", "a886f237e1cf6da03fb793588fd79b64")]
    [TestCase("Image 1_90_deg.jpg", "e740c5b99b0c87d86b9b1b3f322dec7e")]
    [TestCase("Image 1_180_deg.jpg", "bc43ad1ebd118a2f9443b3199d4127c8")]
    [TestCase("Image 1_270_deg.jpg", "f8f724518021547c26ef9262f29f6b9a")]
    [TestCase("Image 8.jpeg", "4194e88c247b5bbc7a1c6294cc795466")]
    [TestCase("Image 10 portrait.png", "02b09a63d382bc1a1f88afa125f3adb3")]
    [TestCase("Homer.gif", "a409ce713de9334117791b15a586dd0e")]
    [TestCase("Image_11.heic", "24cee7da517186279bafac45072fe622")]
    public void CalculateMD5Hash_MD5HashAndValidImageBytesAndInvalidFilePath_ReturnsCorrectMD5Hash(string fileName, string expectedHash)
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, false, true);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);

        string filePath = Path.Combine(dataDirectory!, fileName);
        byte[] imageBytes = File.ReadAllBytes(filePath);

        string hash = assetHashCalculatorService.CalculateHash(imageBytes, filePath);

        Assert.IsFalse(string.IsNullOrWhiteSpace(hash));
        Assert.AreEqual(32, hash.Length); // The MD5Hash is a 32-character hexadecimal string
        Assert.AreEqual(expectedHash, hash);
    }

    [Test]
    [TestCase("Image 1.jpg", "a886f237e1cf6da03fb793588fd79b64")]
    [TestCase("Image 1_90_deg.jpg", "e740c5b99b0c87d86b9b1b3f322dec7e")]
    [TestCase("Image 1_180_deg.jpg", "bc43ad1ebd118a2f9443b3199d4127c8")]
    [TestCase("Image 1_270_deg.jpg", "f8f724518021547c26ef9262f29f6b9a")]
    [TestCase("Image 8.jpeg", "4194e88c247b5bbc7a1c6294cc795466")]
    [TestCase("Image 10 portrait.png", "02b09a63d382bc1a1f88afa125f3adb3")]
    [TestCase("Homer.gif", "a409ce713de9334117791b15a586dd0e")]
    [TestCase("Image_11.heic", "24cee7da517186279bafac45072fe622")]
    public void CalculateMD5Hash_MD5HashAndValidImageBytesAndDirectoryFilePath_ReturnsCorrectMD5Hash(string fileName, string expectedHash)
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, false, true);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);

        string filePath = Path.Combine(dataDirectory!, fileName);
        byte[] imageBytes = File.ReadAllBytes(filePath);

        string hash = assetHashCalculatorService.CalculateHash(imageBytes, filePath);

        Assert.IsFalse(string.IsNullOrWhiteSpace(hash));
        Assert.AreEqual(32, hash.Length); // The MD5Hash is a 32-character hexadecimal string
        Assert.AreEqual(expectedHash, hash);
    }

    [Test]
    [TestCase("Image 1.jpg", "afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee0afbaa849d28fc2b8dc1262d9e619b362ee0")]
    [TestCase("Image 1_90_deg.jpg", "afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee0afbaa849d28fc2b8dc1262d9e619b362ee0")]
    [TestCase("Image 1_180_deg.jpg", "afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee0afbaa849d28fc2b8dc1262d9e619b362ee0")]
    [TestCase("Image 1_270_deg.jpg", "afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee0afbaa849d28fc2b8dc1262d9e619b362ee0")]
    [TestCase("Image 8.jpeg", "afb8a849cc8fc608dc2362da4619b462ee062ee062ee062ee062ee062ee062ee062ee0afb8a849cc8fc608dc2362da4619b462ee062ee062ee062ee062ee062ee062ee062ee0afb8a849cc8fc608dc2362da4619b462ee0afb8a849cc8fc608dc2362da4619b462ee0")]
    [TestCase("Image 10 portrait.png", "afaff849b08fd348dc1f62dae619b262ee062ee062ee062ee062ee062ee062ee062ee0afaff849b08fd348dc1f62dae619b262ee062ee062ee062ee062ee062ee062ee062ee0afaff849b08fd348dc1f62dae619b262ee0afaff849b08fd348dc1f62dae619b262ee0")]
    [TestCase("Homer.gif", "af783881028a11d8ad696231b8ee326225d62ee062ee062ee062ee062ee062ee062ee0af783881028a11d8ad696231b8ee326225d62ee062ee062ee062ee062ee062ee062ee0af783881028a11d8ad696231b8ee326225daf783881028a11d8ad696231b8ee326225d")]
    [TestCase("Image_11.heic", "a926f84a9188106894a161cc28d7cf6205662ee062ee062ee062ee062ee062ee062ee0a926f84a9188106894a161cc28d7cf6205662ee062ee062ee062ee062ee062ee062ee0a926f84a9188106894a161cc28d7cf62056a926f84a9188106894a161cc28d7cf62056")]
    public void CalculateHash_AllHashesEnabledAndValidImageBytesAndFilePath_ReturnsCorrectPHash(string fileName, string expectedHash)
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(true, true, true);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);

        string filePath = Path.Combine(dataDirectory!, fileName);
        byte[] imageBytes = File.ReadAllBytes(filePath);

        string? hash = assetHashCalculatorService.CalculateHash(imageBytes, filePath);

        Assert.IsFalse(string.IsNullOrWhiteSpace(hash));
        Assert.AreEqual(210, hash?.Length); // The PHash is a 210-character hexadecimal string
        Assert.AreEqual(expectedHash, hash?.ToLower());
    }

    [Test]
    [TestCase("Image 1.jpg", "23831507941108244")]
    [TestCase("Image 1_90_deg.jpg", "23831507941108244")]
    [TestCase("Image 1_180_deg.jpg", "23831507941108244")]
    [TestCase("Image 1_270_deg.jpg", "23831507941108244")]
    [TestCase("Image 8.jpeg", "23831507941108244")]
    [TestCase("Image 10 portrait.png", "36116814863094786")]
    public void CalculateHash_DHashAndMd5HashEnabledAndValidImageBytesAndFilePath_ReturnsCorrectDHash(string fileName, string expectedHash)
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, true, true);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);

        string filePath = Path.Combine(dataDirectory!, fileName);
        byte[] imageBytes = File.ReadAllBytes(filePath);

        string hash = assetHashCalculatorService.CalculateHash(imageBytes, filePath);

        Assert.IsFalse(string.IsNullOrWhiteSpace(hash));
        Assert.AreEqual(17, hash.Length); // The DHash is a 17-character number
        Assert.AreEqual(expectedHash, hash);
    }

    private static UserConfigurationService GetUserConfigurationService(bool usingPHash, bool usingDHash, bool usingMd5Hash)
    {
        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock
            .MockGetValue(UserConfigurationKeys.USING_PHASH, usingPHash.ToString())
            .MockGetValue(UserConfigurationKeys.USING_DHASH, usingDHash.ToString())
            .MockGetValue(UserConfigurationKeys.USING_MD5_HASH, usingMd5Hash.ToString());

        return new(configurationRootMock.Object);
    }
}
