namespace PhotoManager.Tests.Unit.Domain;

[TestFixture]
public class BkTreeTests
{
    private TestLogger<BkTree>? _testLogger;

    private Asset? _asset1;
    private Asset? _asset2;
    private Asset? _asset3;

    [SetUp]
    public void SetUp()
    {
        _testLogger = new();

        _asset1 = CreateAsset("Image 1.jpg");
        _asset2 = CreateAsset("Image 2.jpg");
        _asset3 = CreateAsset("Image 3.jpg");
    }

    [TearDown]
    public void TearDown()
    {
        _testLogger!.LoggingAssertTearDown();
    }

    // ── Insert ────────────────────────────────────────────────────────────────

    [Test]
    public void Insert_EmptyTree_CreatesRootGroup()
    {
        BkTree bkTree = new(_testLogger!);

        bkTree.Insert("aaaa", _asset1!);

        IReadOnlyList<List<Asset>> groups = bkTree.GetAllGroups();

        Assert.That(groups, Has.Count.EqualTo(1));
        Assert.That(groups[0], Has.Count.EqualTo(1));
        Assert.That(groups[0][0], Is.EqualTo(_asset1));

        _testLogger!.AssertLogExceptions([], typeof(BkTree));
    }

    [Test]
    public void Insert_SameHash_AppendsToExistingGroup()
    {
        BkTree bkTree = new(_testLogger!);

        bkTree.Insert("aaaa", _asset1!);
        bkTree.Insert("aaaa", _asset2!);

        IReadOnlyList<List<Asset>> groups = bkTree.GetAllGroups();

        Assert.That(groups, Has.Count.EqualTo(1));
        Assert.That(groups[0], Has.Count.EqualTo(2));
        Assert.That(groups[0], Does.Contain(_asset1));
        Assert.That(groups[0], Does.Contain(_asset2));

        _testLogger!.AssertLogExceptions([], typeof(BkTree));
    }

    [Test]
    public void Insert_NewHash_CreatesNewChildGroup()
    {
        BkTree bkTree = new(_testLogger!);

        // distance("aaaa", "bbbb") = 4
        bkTree.Insert("aaaa", _asset1!);
        bkTree.Insert("bbbb", _asset2!);

        IReadOnlyList<List<Asset>> groups = bkTree.GetAllGroups();

        Assert.That(groups, Has.Count.EqualTo(2));
        Assert.That(groups[0][0], Is.EqualTo(_asset1));
        Assert.That(groups[1][0], Is.EqualTo(_asset2));

        _testLogger!.AssertLogExceptions([], typeof(BkTree));
    }

    [Test]
    public void Insert_SameDistanceAsExistingChild_TraversesDeeper()
    {
        // distance("aaaa","bbbb") = 4, distance("aaaa","cccc") = 4
        // So "cccc" traverses into the "bbbb" node, then creates a child there
        BkTree bkTree = new(_testLogger!);

        bkTree.Insert("aaaa", _asset1!);
        bkTree.Insert("bbbb", _asset2!);
        bkTree.Insert("cccc", _asset3!);

        IReadOnlyList<List<Asset>> groups = bkTree.GetAllGroups();

        Assert.That(groups, Has.Count.EqualTo(3));
        Assert.That(groups[0][0], Is.EqualTo(_asset1));
        Assert.That(groups[1][0], Is.EqualTo(_asset2));
        Assert.That(groups[2][0], Is.EqualTo(_asset3));

        // Verify "cccc" is reachable during traversal by confirming TryAddToExistingGroups finds it
        Asset asset4 = CreateAsset("Image 4.jpg");
        bool found = bkTree.TryAddToExistingGroups("cccc", asset4, 0);

        Assert.That(found, Is.True);
        Assert.That(groups[2], Does.Contain(asset4));

        _testLogger!.AssertLogExceptions([], typeof(BkTree));
    }

    // ── GetAllGroups ──────────────────────────────────────────────────────────

    [Test]
    public void GetAllGroups_EmptyTree_ReturnsEmpty()
    {
        BkTree bkTree = new(_testLogger!);

        IReadOnlyList<List<Asset>> groups = bkTree.GetAllGroups();

        Assert.That(groups, Is.Empty);

        _testLogger!.AssertLogExceptions([], typeof(BkTree));
    }

    [Test]
    public void GetAllGroups_SingleNode_ReturnsSingleGroup()
    {
        BkTree bkTree = new(_testLogger!);
        bkTree.Insert("aaaa", _asset1!);

        IReadOnlyList<List<Asset>> groups = bkTree.GetAllGroups();

        Assert.That(groups, Has.Count.EqualTo(1));
        Assert.That(groups[0][0], Is.EqualTo(_asset1));

        _testLogger!.AssertLogExceptions([], typeof(BkTree));
    }

    [Test]
    public void GetAllGroups_MultipleNodes_ReturnsGroupsInInsertionOrder()
    {
        BkTree bkTree = new(_testLogger!);

        // Insert A, B, C in order — GetAllGroups must preserve insertion order
        bkTree.Insert("aaaa", _asset1!);
        bkTree.Insert("bbbb", _asset2!);
        bkTree.Insert("cccc", _asset3!);

        IReadOnlyList<List<Asset>> groups = bkTree.GetAllGroups();

        Assert.That(groups, Has.Count.EqualTo(3));
        Assert.That(groups[0][0], Is.EqualTo(_asset1));
        Assert.That(groups[1][0], Is.EqualTo(_asset2));
        Assert.That(groups[2][0], Is.EqualTo(_asset3));

        _testLogger!.AssertLogExceptions([], typeof(BkTree));
    }

    // ── TryAddToExistingGroups ────────────────────────────────────────────────

    [Test]
    public void TryAddToExistingGroups_EmptyTree_ReturnsFalse()
    {
        BkTree bkTree = new(_testLogger!);

        bool result = bkTree.TryAddToExistingGroups("aaaa", _asset1!, 2);

        Assert.That(result, Is.False);
        Assert.That(bkTree.GetAllGroups(), Is.Empty);

        _testLogger!.AssertLogExceptions([], typeof(BkTree));
    }

    [Test]
    public void TryAddToExistingGroups_NoMatch_ReturnsFalse()
    {
        // distance("aaaa","bbbb") = 4, threshold = 2 → no match
        BkTree bkTree = new(_testLogger!);
        bkTree.Insert("aaaa", _asset1!);

        bool result = bkTree.TryAddToExistingGroups("bbbb", _asset2!, 2);

        Assert.That(result, Is.False);
        Assert.That(bkTree.GetAllGroups()[0], Has.Count.EqualTo(1));

        _testLogger!.AssertLogExceptions([], typeof(BkTree));
    }

    [Test]
    public void TryAddToExistingGroups_ExactMatch_ReturnsTrueAndAddsAsset()
    {
        BkTree bkTree = new(_testLogger!);
        bkTree.Insert("aaaa", _asset1!);

        bool result = bkTree.TryAddToExistingGroups("aaaa", _asset2!, 0);

        Assert.That(result, Is.True);
        Assert.That(bkTree.GetAllGroups(), Has.Count.EqualTo(1));
        Assert.That(bkTree.GetAllGroups()[0], Does.Contain(_asset2));

        _testLogger!.AssertLogExceptions([], typeof(BkTree));
    }

    [Test]
    public void TryAddToExistingGroups_WithinThreshold_ReturnsTrueAndAddsAsset()
    {
        // distance("aaaa","aaab") = 1, threshold = 2 → match
        BkTree bkTree = new(_testLogger!);
        bkTree.Insert("aaaa", _asset1!);

        bool result = bkTree.TryAddToExistingGroups("aaab", _asset2!, 2);

        Assert.That(result, Is.True);
        Assert.That(bkTree.GetAllGroups()[0], Does.Contain(_asset2));

        _testLogger!.AssertLogExceptions([], typeof(BkTree));
    }

    [Test]
    public void TryAddToExistingGroups_MultipleMatches_AddsToAllMatchingGroups()
    {
        // distance("aaaa","aabb") = 2 → match; distance("bbbb","aabb") = 2 → match
        // threshold = 2 → both groups match
        BkTree bkTree = new(_testLogger!);
        bkTree.Insert("aaaa", _asset1!);
        bkTree.Insert("bbbb", _asset2!);

        Asset asset4 = CreateAsset("Image 4.jpg");
        bool result = bkTree.TryAddToExistingGroups("aabb", asset4, 2);

        Assert.That(result, Is.True);
        Assert.That(bkTree.GetAllGroups()[0], Does.Contain(asset4));
        Assert.That(bkTree.GetAllGroups()[1], Does.Contain(asset4));

        _testLogger!.AssertLogExceptions([], typeof(BkTree));
    }

    [Test]
    public void TryAddToExistingGroups_ThresholdZero_NoMatchOutsideExact()
    {
        // distance("aaaa","aaab") = 1, threshold = 0 → no match
        BkTree bkTree = new(_testLogger!);
        bkTree.Insert("aaaa", _asset1!);

        bool result = bkTree.TryAddToExistingGroups("aaab", _asset2!, 0);

        Assert.That(result, Is.False);
        Assert.That(bkTree.GetAllGroups()[0], Has.Count.EqualTo(1));

        _testLogger!.AssertLogExceptions([], typeof(BkTree));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static Asset CreateAsset(string fileName) => new()
    {
        FolderId = Guid.NewGuid(),
        Folder = new() { Id = Guid.Empty, Path = "" },
        FileName = fileName,
        ImageRotation = Rotation.Rotate0,
        Pixel = new()
        {
            Asset = new() { Width = 1920, Height = 1080 },
            Thumbnail = new() { Width = 200, Height = 112 }
        },
        FileProperties = new() { Size = 1024 },
        ThumbnailCreationDateTime = DateTime.Now,
        Hash = "0000",
        Metadata = new()
        {
            Corrupted = new() { IsTrue = false, Message = null },
            Rotated = new() { IsTrue = false, Message = null }
        }
    };
}
