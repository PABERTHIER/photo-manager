using ImageMagick;

namespace PhotoManager.Tests.Integration.Infrastructure;

[TestFixture]
public class AssetHashCalculatorServiceTests
{
    private string? _dataDirectory;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
    }

    [Test]
    [TestCase("Image 1.jpg", "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9")]
    [TestCase("Image 1_90_deg.jpg", "0092081bd0a83e9253f4eb319b4cfab197a09ed3b28b5697c31f17bf2646638dec95bf28d4f454dda8ff3dcdaab33fa002774f13dfe6d7aee82f1a960e3e3aee")]
    [TestCase("Image 1_180_deg.jpg", "399383d82d4b7ed8233f714a1d14272b276b80505d7b3a2935b6ec962fd5cf280ccedb3e9380df1a11ee65e5f5fa66278dd2f407b7b6bdfa3093319164c446dd")]
    [TestCase("Image 1_270_deg.jpg", "43950e06cecd509509d868a2064a828aeb6ff6a9a30f296ec9a3621b269742e75dacb40300aa6927c4c8f0879268e1ab3d7eac228b38cade35cc9b7c6adf3449")]
    [TestCase("Image 8.jpeg", "156f81ceb3db1bfdf27618efe4b89575f17d4708053c1a9ce6c5dfb7f36a6b958a5c3f45147728de28cf538345336e7b81d01b3f5908d4677ce76e1661e97ac3")]
    [TestCase("Image 10 portrait.png", "7ad01e9fe639957a9e8eaddc7fd864068f4a03f9981fd480f310740a43a0a4f2b8fa7a80d9c83039c46fcfbb63a5e465adaf07d33191369590adcda1586b1c94")]
    [TestCase("Homer.gif", "c48b1f61f3a3a004f425d8493d30a50ae14408ed4c5354bf4d0ca40069f91951381a7df32ee7455a6edef0996c95571557a9993021331ff2dfbc3ccc7f0c8ff1")]
    [TestCase("Image_11.heic", "f52bd860f5ad7f81a92919e5fb5769d3e86778b2ade74832fbd3029435c85e59cb64b3c2ce425445a49917953e6e913c72b81e48976041a4439cb65e92baf18d")]
    [TestCase("Image_11_90.heic", "b75df4942de1efa7d4326fedd721e99cc43959a1c22098a8cfcf6861aea099741d9c98fa1615e54d53838c22b9c6ff3b328c4ee3c0875d2b4ac7f7dee8d61bcf")]
    [TestCase("Image_11_180.heic", "a2ad3ac9d51f74230b9f1d554172781d9cc214d01f8344354e2e56dc3daecb5a0021d4d2814eba0cde6fd681f3a8e8b8b55fbfad7184740e3518190e05dca77f")]
    [TestCase("Image_11_270.heic", "d4d7342a783f0f1b5e10ad9b493b71dd688dbc31b8ddd5534c9fff9c5fdbfa8bee32ef4e4258f7514a1b57f9a12b571454c8b66172b1a66bf1b53953b66227d5")]
    public void CalculateHash_DefaultHashAndValidImageBytesAndFilePath_ReturnsCorrectHash(string fileName, string expectedHash)
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, false, false);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);

        string filePath = Path.Combine(_dataDirectory!, fileName);
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
        const string expectedHash = "cf83e1357eefb8bdf1542850d66d8007d620e4050b5715dc83f4a921d36ce9ce47d0d13c5d85f2b0ff8318d2877eec2f63b931bd47417a81a538327af927da3e";

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
    [TestCase("Image_11_90.heic", "b75df4942de1efa7d4326fedd721e99cc43959a1c22098a8cfcf6861aea099741d9c98fa1615e54d53838c22b9c6ff3b328c4ee3c0875d2b4ac7f7dee8d61bcf")]
    public void CalculateHash_DefaultHashAndValidImageBytesAndNullFilePath_ReturnsCorrectHash(string fileName, string expectedHash)
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, false, false);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);

        string filePath = Path.Combine(_dataDirectory!, fileName);
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
    [TestCase("Image_11_90.heic", "b75df4942de1efa7d4326fedd721e99cc43959a1c22098a8cfcf6861aea099741d9c98fa1615e54d53838c22b9c6ff3b328c4ee3c0875d2b4ac7f7dee8d61bcf")]
    public void CalculateHash_DefaultHashAndValidImageBytesAndInvalidFilePath_ReturnsCorrectHash(string fileName, string expectedHash)
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, false, false);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);

        string filePath = Path.Combine(_dataDirectory!, fileName);
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
    [TestCase("Image_11_90.heic", "b75df4942de1efa7d4326fedd721e99cc43959a1c22098a8cfcf6861aea099741d9c98fa1615e54d53838c22b9c6ff3b328c4ee3c0875d2b4ac7f7dee8d61bcf")]
    public void CalculateHash_DefaultHashAndValidImageBytesAndDirectoryFilePath_ReturnsCorrectHash(string fileName, string expectedHash)
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, false, false);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);

        string filePath = Path.Combine(_dataDirectory!, fileName);
        byte[] imageBytes = File.ReadAllBytes(filePath);

        string hash = assetHashCalculatorService.CalculateHash(imageBytes, _dataDirectory!);

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
    [TestCase("Image_11.heic", "a928c84a96881408945461cb98d6366207162ee062ee062ee062ee062ee062ee062ee0a928c84a96881408945461cb98d6366207162ee062ee062ee062ee062ee062ee062ee0a928c84a96881408945461cb98d63662071a928c84a96881408945461cb98d63662071")]
    [TestCase("Image_11_90.heic", "a928f84a98881418945f61cbb8d6606206a62ee062ee062ee062ee062ee062ee062ee0a928f84a98881418945f61cbb8d6606206a62ee062ee062ee062ee062ee062ee062ee0a928f84a98881418945f61cbb8d6606206aa928f84a98881418945f61cbb8d6606206a")]
    [TestCase("Image_11_180.heic", "a928e84a97881448945561cb98d6386206f62ee062ee062ee062ee062ee062ee062ee0a928e84a97881448945561cb98d6386206f62ee062ee062ee062ee062ee062ee062ee0a928e84a97881448945561cb98d6386206fa928e84a97881448945561cb98d6386206f")]
    [TestCase("Image_11_270.heic", "a929184a99881458946061cbb8d6606206962ee062ee062ee062ee062ee062ee062ee0a929184a99881458946061cbb8d6606206962ee062ee062ee062ee062ee062ee062ee0a929184a99881458946061cbb8d66062069a929184a99881458946061cbb8d66062069")]
    public void CalculateHash_PHashAndValidImageBytesAndFilePath_ReturnsCorrectPHash(string fileName, string expectedHash)
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(true, false, false);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);

        string filePath = Path.Combine(_dataDirectory!, fileName);
        byte[] imageBytes = File.ReadAllBytes(filePath);

        string hash = assetHashCalculatorService.CalculateHash(imageBytes, filePath);

        Assert.IsFalse(string.IsNullOrWhiteSpace(hash));
        Assert.AreEqual(210, hash.Length); // The PHash is a 210-character hexadecimal string
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
    [TestCase("Image_11_90.heic", "a928f84a98881418945f61cbb8d6606206a62ee062ee062ee062ee062ee062ee062ee0a928f84a98881418945f61cbb8d6606206a62ee062ee062ee062ee062ee062ee062ee0a928f84a98881418945f61cbb8d6606206aa928f84a98881418945f61cbb8d6606206a")]
    public void CalculateHash_PHashAndEmptyImageBytesAndFilePath_ReturnsCorrectPHash(string fileName, string expectedHash)
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(true, false, false);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);

        byte[] imageBytes = [];
        string filePath = Path.Combine(_dataDirectory!, fileName);

        string hash = assetHashCalculatorService.CalculateHash(imageBytes, filePath);

        Assert.IsFalse(string.IsNullOrWhiteSpace(hash));
        Assert.AreEqual(210, hash.Length); // The PHash is a 210-character hexadecimal string
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
    [TestCase("Image_11_90.heic", "a928f84a98881418945f61cbb8d6606206a62ee062ee062ee062ee062ee062ee062ee0a928f84a98881418945f61cbb8d6606206a62ee062ee062ee062ee062ee062ee062ee0a928f84a98881418945f61cbb8d6606206aa928f84a98881418945f61cbb8d6606206a")]
    public void CalculateHash_PHashAndNullImageBytesAndFilePath_ReturnsCorrectPHash(string fileName, string expectedHash)
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(true, false, false);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);

        byte[]? imageBytes = null;
        string filePath = Path.Combine(_dataDirectory!, fileName);

        string hash = assetHashCalculatorService.CalculateHash(imageBytes!, filePath);

        Assert.IsFalse(string.IsNullOrWhiteSpace(hash));
        Assert.AreEqual(210, hash.Length); // The PHash is a 210-character hexadecimal string
        Assert.AreEqual(expectedHash, hash.ToLower());
    }

    [Test]
    public void CalculateHash_PHashAndImageDoesNotExist_ThrowsMagickBlobErrorException()
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(true, false, false);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);

        byte[]? imageBytes = null;
        string filePath = Path.Combine(_dataDirectory!, "ImageDoesNotExist.png");

        MagickBlobErrorException? exception = Assert.Throws<MagickBlobErrorException>(() => assetHashCalculatorService.CalculateHash(imageBytes!, filePath));

        Assert.AreEqual($"unable to open image '{filePath}': No such file or directory @ error/blob.c/OpenBlob/3571", exception?.Message);
    }

    [Test]
    public void CalculateHash_PHashAndImagePathIsInvalid_ThrowsMagickBlobErrorException()
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(true, false, false);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);

        byte[]? imageBytes = null;

        MagickBlobErrorException? exception = Assert.Throws<MagickBlobErrorException>(() => assetHashCalculatorService.CalculateHash(imageBytes!, _dataDirectory!));

        Assert.AreEqual($"unable to open image '{_dataDirectory!}': Permission denied @ error/blob.c/OpenBlob/3571", exception?.Message);
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
    [TestCase("Image_11_90.heic", "14766675108600273")]
    [TestCase("Image_11_180.heic", "53334510583489094")]
    public void CalculateHash_DHashAndValidImageBytesAndFilePath_ReturnsCorrectDHash(string fileName, string expectedHash)
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, true, false);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);

        string filePath = Path.Combine(_dataDirectory!, fileName);
        byte[] imageBytes = File.ReadAllBytes(filePath);

        string hash = assetHashCalculatorService.CalculateHash(imageBytes, filePath);

        Assert.IsFalse(string.IsNullOrWhiteSpace(hash));
        Assert.AreEqual(17, hash.Length); // The DHash is a 17-character number
        Assert.AreEqual(expectedHash, hash);
    }

    [Test]
    [TestCase("Homer.gif", "0")]
    [TestCase("Image_11.heic", "0")]
    [TestCase("Image_11_270.heic", "0")]
    public void CalculateHash_NonWorkingDHash_ReturnsCorrectDHash(string fileName, string expectedHash)
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, true, false);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);

        string filePath = Path.Combine(_dataDirectory!, fileName);
        byte[] imageBytes = File.ReadAllBytes(filePath);

        string hash = assetHashCalculatorService.CalculateHash(imageBytes, filePath);

        Assert.IsFalse(string.IsNullOrWhiteSpace(hash));
        Assert.AreEqual(1, hash.Length);
        Assert.AreEqual(expectedHash, hash);
    }

    [Test]
    [TestCase("Image 1.jpg", "23831507941108244")]
    [TestCase("Image 1_90_deg.jpg", "23831507941108244")]
    [TestCase("Image 1_180_deg.jpg", "23831507941108244")]
    [TestCase("Image 1_270_deg.jpg", "23831507941108244")]
    [TestCase("Image 8.jpeg", "23831507941108244")]
    [TestCase("Image 10 portrait.png", "36116814863094786")]
    [TestCase("Image_11_90.heic", "14766675108600273")]
    public void CalculateHash_DHashAndEmptyImageBytesAndFilePath_ReturnsCorrectDHash(string fileName, string expectedHash)
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, true, false);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);

        byte[] imageBytes = [];
        string filePath = Path.Combine(_dataDirectory!, fileName);

        string hash = assetHashCalculatorService.CalculateHash(imageBytes, filePath);

        Assert.IsFalse(string.IsNullOrWhiteSpace(hash));
        Assert.AreEqual(17, hash.Length); // The DHash is a 17-character number
        Assert.AreEqual(expectedHash, hash);
    }

    [Test]
    [TestCase("Homer.gif", "0")]
    [TestCase("Image_11.heic", "0")]
    public void CalculateHash_DHashAndEmptyImageBytesAndNonWorkingDHash_ReturnsCorrectDHash(string fileName, string expectedHash)
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, true, false);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);

        byte[] imageBytes = [];
        string filePath = Path.Combine(_dataDirectory!, fileName);

        string hash = assetHashCalculatorService.CalculateHash(imageBytes, filePath);

        Assert.IsFalse(string.IsNullOrWhiteSpace(hash));
        Assert.AreEqual(1, hash.Length);
        Assert.AreEqual(expectedHash, hash);
    }

    [Test]
    [TestCase("Image 1.jpg", "23831507941108244")]
    [TestCase("Image 1_90_deg.jpg", "23831507941108244")]
    [TestCase("Image 1_180_deg.jpg", "23831507941108244")]
    [TestCase("Image 1_270_deg.jpg", "23831507941108244")]
    [TestCase("Image 8.jpeg", "23831507941108244")]
    [TestCase("Image 10 portrait.png", "36116814863094786")]
    [TestCase("Image_11_90.heic", "14766675108600273")]
    public void CalculateHash_DHashAndNullImageBytesAndFilePath_ReturnsCorrectDHash(string fileName, string expectedHash)
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, true, false);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);

        byte[]? imageBytes = null;
        string filePath = Path.Combine(_dataDirectory!, fileName);

        string hash = assetHashCalculatorService.CalculateHash(imageBytes!, filePath);

        Assert.IsFalse(string.IsNullOrWhiteSpace(hash));
        Assert.AreEqual(17, hash.Length); // The DHash is a 17-character number
        Assert.AreEqual(expectedHash, hash);
    }

    [Test]
    [TestCase("Homer.gif", "0")]
    [TestCase("Image_11.heic", "0")]
    public void CalculateHash_DHashAndNullImageBytesAndNonWorkingDHash_ReturnsCorrectDHash(string fileName, string expectedHash)
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, true, false);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);

        byte[]? imageBytes = null;
        string filePath = Path.Combine(_dataDirectory!, fileName);

        string hash = assetHashCalculatorService.CalculateHash(imageBytes!, filePath);

        Assert.IsFalse(string.IsNullOrWhiteSpace(hash));
        Assert.AreEqual(1, hash.Length);
        Assert.AreEqual(expectedHash, hash);
    }

    [Test]
    public void CalculateHash_DHashAndImageDoesNotExist_ThrowsArgumentException()
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, true, false);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);

        byte[]? imageBytes = null;
        string filePath = Path.Combine(_dataDirectory!, "ImageDoesNotExist.png");

        ArgumentException? exception = Assert.Throws<ArgumentException>(() => assetHashCalculatorService.CalculateHash(imageBytes!, filePath));

        Assert.AreEqual("Parameter is not valid.", exception?.Message);
    }

    [Test]
    public void CalculateHash_DHashAndImagePathIsInvalid_ThrowsArgumentException()
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, true, false);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);

        byte[]? imageBytes = null;

        ArgumentException? exception = Assert.Throws<ArgumentException>(() => assetHashCalculatorService.CalculateHash(imageBytes!, _dataDirectory!));

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
    [TestCase("Image_11.heic", "d9aa281b55316e10c767877dee265732")]
    [TestCase("Image_11_90.heic", "e3aeca5fc781bd52470f9186653d3a3f")]
    [TestCase("Image_11_180.heic", "9bf6a1a37331ba6f391b885b6dcc67a1")]
    [TestCase("Image_11_270.heic", "35d24aaf7f6fc8b9872f069a8c28ab78")]
    public void CalculateMD5Hash_MD5HashAndValidImageBytesAndFilePath_ReturnsCorrectMD5Hash(string fileName, string expectedHash)
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, false, true);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);

        string filePath = Path.Combine(_dataDirectory!, fileName);
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
        const string expectedHash = "d41d8cd98f00b204e9800998ecf8427e";

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
    [TestCase("Image_11_90.heic", "e3aeca5fc781bd52470f9186653d3a3f")]
    public void CalculateMD5Hash_MD5HashAndValidImageBytesAndNullFilePath_ReturnsCorrectMD5Hash(string fileName, string expectedHash)
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, false, true);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);

        string filePath = Path.Combine(_dataDirectory!, fileName);
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
    [TestCase("Image_11_90.heic", "e3aeca5fc781bd52470f9186653d3a3f")]
    public void CalculateMD5Hash_MD5HashAndValidImageBytesAndInvalidFilePath_ReturnsCorrectMD5Hash(string fileName, string expectedHash)
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, false, true);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);

        string filePath = Path.Combine(_dataDirectory!, fileName);
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
    [TestCase("Image_11_90.heic", "e3aeca5fc781bd52470f9186653d3a3f")]
    public void CalculateMD5Hash_MD5HashAndValidImageBytesAndDirectoryFilePath_ReturnsCorrectMD5Hash(string fileName, string expectedHash)
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(false, false, true);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);

        string filePath = Path.Combine(_dataDirectory!, fileName);
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
    [TestCase("Image_11_90.heic", "a928f84a98881418945f61cbb8d6606206a62ee062ee062ee062ee062ee062ee062ee0a928f84a98881418945f61cbb8d6606206a62ee062ee062ee062ee062ee062ee062ee0a928f84a98881418945f61cbb8d6606206aa928f84a98881418945f61cbb8d6606206a")]
    public void CalculateHash_AllHashesEnabledAndValidImageBytesAndFilePath_ReturnsCorrectPHash(string fileName, string expectedHash)
    {
        UserConfigurationService userConfigurationService = GetUserConfigurationService(true, true, true);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);

        string filePath = Path.Combine(_dataDirectory!, fileName);
        byte[] imageBytes = File.ReadAllBytes(filePath);

        string hash = assetHashCalculatorService.CalculateHash(imageBytes, filePath);

        Assert.IsFalse(string.IsNullOrWhiteSpace(hash));
        Assert.AreEqual(210, hash.Length); // The PHash is a 210-character hexadecimal string
        Assert.AreEqual(expectedHash, hash.ToLower());
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

        string filePath = Path.Combine(_dataDirectory!, fileName);
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
