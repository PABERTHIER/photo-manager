using ImageMagick;

namespace PhotoManager.Tests.Unit.Common;

[TestFixture]
public class HashingHelperTests
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
    [TestCase("Image_11_90.heic", "b75df4942de1efa7d4326fedd721e99cc43959a1c22098a8cfcf6861aea099741d9c98fa1615e54d53838c22b9c6ff3b328c4ee3c0875d2b4ac7f7dee8d61bcf")]
    public void CalculateHash_ValidImageBytes_ReturnsCorrectHash(string fileName, string expectedHash)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);
        byte[] imageBytes = File.ReadAllBytes(filePath);

        string hash = HashingHelper.CalculateHash(imageBytes);

        Assert.IsFalse(string.IsNullOrWhiteSpace(hash));
        Assert.AreEqual(128, hash.Length); // SHA-512 generates a 128-character long hash in hexadecimal representation
        Assert.AreEqual(expectedHash, hash.ToLower());
    }

    [Test]
    public void CalculateHash_EmptyImageBytes_ReturnsSameHash()
    {
        byte[] imageBytes = [];
        const string expectedHash = "cf83e1357eefb8bdf1542850d66d8007d620e4050b5715dc83f4a921d36ce9ce47d0d13c5d85f2b0ff8318d2877eec2f63b931bd47417a81a538327af927da3e";

        string hash = HashingHelper.CalculateHash(imageBytes);

        Assert.IsFalse(string.IsNullOrWhiteSpace(hash));
        Assert.AreEqual(128, hash.Length);
        Assert.AreEqual(expectedHash, hash.ToLower());
    }

    [Test]
    public void CalculateHash_NullImageBytes_ThrowsArgumentNullException()
    {
        byte[]? imageBytes = null;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => HashingHelper.CalculateHash(imageBytes!));

        Assert.AreEqual("Value cannot be null. (Parameter 'source')", exception?.Message);
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
    public void CalculatePHash_ValidImagePath_ReturnsCorrectPHash(string fileName, string expectedHash)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);

        string? phash = HashingHelper.CalculatePHash(filePath);

        Assert.IsFalse(string.IsNullOrWhiteSpace(phash));
        Assert.AreEqual(210, phash?.Length); // The PHash is a 210-character hexadecimal string
        Assert.AreEqual(expectedHash, phash?.ToLower());
    }

    [Test]
    public void CalculatePHash_ImageDoesNotExist_ThrowsMagickBlobErrorException()
    {
        string filePath = Path.Combine(_dataDirectory!, "ImageDoesNotExist.png");

        MagickBlobErrorException? exception = Assert.Throws<MagickBlobErrorException>(() => HashingHelper.CalculatePHash(filePath));
        
        Assert.AreEqual($"unable to open image '{filePath}': No such file or directory @ error/blob.c/OpenBlob/3571", exception?.Message);
    }

    [Test]
    public void CalculatePHash_ImagePathIsInvalid_ThrowsMagickBlobErrorException()
    {
        MagickBlobErrorException? exception = Assert.Throws<MagickBlobErrorException>(() => HashingHelper.CalculatePHash(_dataDirectory!));
        
        Assert.AreEqual($"unable to open image '{_dataDirectory!}': Permission denied @ error/blob.c/OpenBlob/3571", exception?.Message);
    }

    [Test]
    public void CalculatePHash_ImagePathIsNull_ThrowsArgumentNullException()
    {
        string? filePath = null;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => HashingHelper.CalculatePHash(filePath!));
        
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
    public void CalculateDHash_ValidImagePath_ReturnsCorrectDHash(string fileName, string expectedHash)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);

        string dHash = HashingHelper.CalculateDHash(filePath);

        Assert.IsFalse(string.IsNullOrWhiteSpace(dHash));
        Assert.AreEqual(17, dHash.Length); // The DHash is a 17-character number
        Assert.AreEqual(expectedHash, dHash);
    }

    [Test]
    [TestCase("Homer.gif", "0")]
    [TestCase("Image_11.heic", "0")]
    public void CalculateDHash_NonWorkingDHash_ReturnsCorrectDHash(string fileName, string expectedHash)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);

        string dHash = HashingHelper.CalculateDHash(filePath);

        Assert.IsFalse(string.IsNullOrWhiteSpace(dHash));
        Assert.AreEqual(1, dHash.Length);
        Assert.AreEqual(expectedHash, dHash);
    }

    [Test]
    public void CalculateDHash_ImageDoesNotExist_ThrowsArgumentException()
    {
        string filePath = Path.Combine(_dataDirectory!, "ImageDoesNotExist.png");

        ArgumentException? exception = Assert.Throws<ArgumentException>(() => HashingHelper.CalculateDHash(filePath));
        
        Assert.AreEqual("Parameter is not valid.", exception?.Message);
    }

    [Test]
    public void CalculateDHash_ImagePathIsInvalid_ThrowsArgumentException()
    {
        ArgumentException? exception = Assert.Throws<ArgumentException>(() => HashingHelper.CalculateDHash(_dataDirectory!));
        
        Assert.AreEqual("Parameter is not valid.", exception?.Message);
    }

    [Test]
    public void CalculateDHash_ImagePathIsNull_ThrowsArgumentNullException()
    {
        string? filePath = null;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => HashingHelper.CalculateDHash(filePath!));
        
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
    [TestCase("Image_11_90.heic", "e3aeca5fc781bd52470f9186653d3a3f")]
    public void CalculateMD5Hash_ValidImageBytes_ReturnsCorrectMD5Hash(string fileName, string expectedHash)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);
        byte[] imageBytes = File.ReadAllBytes(filePath);

        string md5Hash = HashingHelper.CalculateMD5Hash(imageBytes);

        Assert.IsFalse(string.IsNullOrWhiteSpace(md5Hash));
        Assert.AreEqual(32, md5Hash.Length); // The MD5Hash is a 32-character hexadecimal string
        Assert.AreEqual(expectedHash, md5Hash);
    }

    [Test]
    public void CalculateMD5Hash_EmptyImageBytes_ReturnsSameMD5Hash()
    {
        byte[] imageBytes = [];
        const string expectedHash = "d41d8cd98f00b204e9800998ecf8427e";

        string hash = HashingHelper.CalculateMD5Hash(imageBytes);

        Assert.IsFalse(string.IsNullOrWhiteSpace(hash));
        Assert.AreEqual(32, hash.Length);
        Assert.AreEqual(expectedHash, hash.ToLower());
    }

    [Test]
    public void CalculateMD5Hash_NullImageBytes_ThrowsArgumentNullException()
    {
        byte[]? imageBytes = null;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => HashingHelper.CalculateMD5Hash(imageBytes!));

        Assert.AreEqual("Value cannot be null. (Parameter 'source')", exception?.Message);
    }

    [Test]
    [TestCase("1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9")] // SHA512
    [TestCase("afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee0afbaa849d28fc2b8dc1262d9e619b362ee0")] // PHash
    [TestCase("23831507941108244")] // DHash
    [TestCase("a886f237e1cf6da03fb793588fd79b64")] // MD5Hash
    public void CalculateHammingDistance_SameHashes_ReturnsZero(string hash)
    {
        int distance = HashingHelper.CalculateHammingDistance(hash, hash);

        Assert.AreEqual(0, distance);
    }

    [Test]
    [TestCase(
        "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9",
        "0092081bd0a83e9253f4eb319b4cfab197a09ed3b28b5697c31f17bf2646638dec95bf28d4f454dda8ff3dcdaab33fa002774f13dfe6d7aee82f1a960e3e3aee",
        118)] // SHA512
    [TestCase("afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee062ee062ee062ee062ee062ee062ee062ee0afbaa849d28fc2b8dc1262d9e619b362ee0afbaa849d28fc2b8dc1262d9e619b362ee0", "afaff849b08fd348dc1f62dae619b262ee062ee062ee062ee062ee062ee062ee062ee0afaff849b08fd348dc1f62dae619b262ee062ee062ee062ee062ee062ee062ee062ee0afaff849b08fd348dc1f62dae619b262ee0afaff849b08fd348dc1f62dae619b262ee0", 44)] // PHash
    [TestCase("23831507941108244", "36116814863094786", 17)] // DHash
    [TestCase("a886f237e1cf6da03fb793588fd79b64", "e740c5b99b0c87d86b9b1b3f322dec7e", 32)] // MD5Hash
    public void CalculateHammingDistance_DifferentHashes_ReturnsCorrectDistance(string hash1, string hash2, int expectedDistance)
    {
        int distance = HashingHelper.CalculateHammingDistance(hash1, hash2);

        Assert.AreEqual(expectedDistance, distance);
        Assert.GreaterOrEqual(expectedDistance, 0);
    }

    [Test]
    [TestCase("23831507941108244", "a886f237e1cf6da03fb793588fd79b64")]
    [TestCase("a886f237e1cf6da03fb793588fd79b64", "23831507941108244")]
    [TestCase("23831507941108244", null)]
    [TestCase(null, "23831507941108244")]
    [TestCase(null, null)]
    public void CalculateHammingDistance_IncorrectHashes_ThrowsArgumentException(string? hash1, string? hash2)
    {
        ArgumentException? exception = Assert.Throws<ArgumentException>(() => HashingHelper.CalculateHammingDistance(hash1!, hash2!));

        Assert.AreEqual("Invalid arguments for hamming distance calculation.", exception?.Message);
    }
}
