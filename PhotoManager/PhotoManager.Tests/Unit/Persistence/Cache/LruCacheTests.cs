using PhotoManager.Persistence.Cache;

namespace PhotoManager.Tests.Unit.Persistence.Cache;

[TestFixture]
public class LruCacheTests
{
    [Test]
    public void Constructor_InvalidCapacity_ThrowsArgumentOutOfRangeException([Values(0, -1, -10)] int capacity)
    {
        ArgumentOutOfRangeException ex = Assert.Throws<ArgumentOutOfRangeException>(
            () => _ = new LruCache<string, int>(capacity))!;

        Assert.That(ex.ParamName, Is.EqualTo("capacity"));
    }

    [Test]
    public void Set_NewKey_AddsToCache()
    {
        LruCache<string, int> cache = new(3);

        cache.Set("a", 1);

        Assert.That(cache.Count, Is.EqualTo(1));
        Assert.That(cache.TryGet("a", out int value), Is.True);
        Assert.That(value, Is.EqualTo(1));
    }

    [Test]
    public void Set_ExistingKey_UpdatesValueAndPromotes()
    {
        LruCache<string, int> cache = new(2);

        cache.Set("a", 1);
        cache.Set("b", 2);
        cache.Set("a", 99); // updates and becomes MRU
        cache.Set("c", 3); // evicts "b" (LRU), not "a"

        Assert.That(cache.TryGet("b", out _), Is.False);
        Assert.That(cache.TryGet("a", out int aValue), Is.True);
        Assert.That(aValue, Is.EqualTo(99));
        Assert.That(cache.TryGet("c", out _), Is.True);
    }

    [Test]
    public void Set_OverCapacity_EvictsLeastRecentlyUsed()
    {
        LruCache<string, int> cache = new(2);

        cache.Set("a", 1);
        cache.Set("b", 2);
        cache.Set("c", 3); // should evict "a"

        Assert.That(cache.Count, Is.EqualTo(2));
        Assert.That(cache.TryGet("a", out _), Is.False);
        Assert.That(cache.TryGet("b", out _), Is.True);
        Assert.That(cache.TryGet("c", out _), Is.True);
    }

    [Test]
    public void TryGet_PromotesEntryToMostRecent()
    {
        LruCache<string, int> cache = new(2);
        cache.Set("a", 1);
        cache.Set("b", 2);

        Assert.That(cache.TryGet("a", out _), Is.True); // promote a

        cache.Set("c", 3); // should evict "b" now, not "a"

        Assert.That(cache.TryGet("a", out _), Is.True);
        Assert.That(cache.TryGet("b", out _), Is.False);
        Assert.That(cache.TryGet("c", out _), Is.True);
    }

    [Test]
    public void TryGet_MissingKey_ReturnsFalseAndDefault()
    {
        LruCache<string, string> cache = new(2);

        Assert.That(cache.TryGet("missing", out string value), Is.False);
        Assert.That(value, Is.Null);
    }

    [Test]
    public void Remove_ExistingKey_ReturnsTrueAndDecrementsCount()
    {
        LruCache<string, int> cache = new(3);
        cache.Set("a", 1);
        cache.Set("b", 2);

        Assert.That(cache.Remove("a"), Is.True);
        Assert.That(cache.Count, Is.EqualTo(1));
        Assert.That(cache.TryGet("a", out _), Is.False);
    }

    [Test]
    public void Remove_MissingKey_ReturnsFalse()
    {
        LruCache<string, int> cache = new(3);
        cache.Set("a", 1);

        Assert.That(cache.Remove("z"), Is.False);
        Assert.That(cache.Count, Is.EqualTo(1));
    }

    [Test]
    public void Remove_HeadAndTail_RewiresLinks()
    {
        LruCache<string, int> cache = new(3);
        cache.Set("a", 1);
        cache.Set("b", 2);
        cache.Set("c", 3); // order tail->head : a, b, c

        Assert.That(cache.Remove("c"), Is.True); // remove head
        Assert.That(cache.Remove("a"), Is.True); // remove tail

        Assert.That(cache.Count, Is.EqualTo(1));
        Assert.That(cache.TryGet("b", out int bValue), Is.True);
        Assert.That(bValue, Is.EqualTo(2));
    }

    [Test]
    public void Clear_ResetsState()
    {
        LruCache<string, int> cache = new(3);
        cache.Set("a", 1);
        cache.Set("b", 2);

        cache.Clear();

        Assert.That(cache.Count, Is.Zero);
        Assert.That(cache.TryGet("a", out _), Is.False);

        cache.Set("c", 3);
        Assert.That(cache.Count, Is.EqualTo(1));
    }

    [Test]
    public void CapacityProperty_ReturnsConfiguredCapacity()
    {
        LruCache<string, int> cache = new(7);
        Assert.That(cache.Capacity, Is.EqualTo(7));
    }

    [Test]
    public void ConcurrentAccess_DoesNotCorruptState()
    {
        LruCache<int, int> cache = new(64);
        const int writers = 4;
        const int writesPerWriter = 5_000;

        Parallel.For(0, writers, w =>
        {
            for (int i = 0; i < writesPerWriter; i++)
            {
                int key = ((w * writesPerWriter) + i) % 256;
                cache.Set(key, key);
                cache.TryGet(key, out _);
            }
        });

        Assert.That(cache.Count, Is.LessThanOrEqualTo(cache.Capacity));
    }
}
