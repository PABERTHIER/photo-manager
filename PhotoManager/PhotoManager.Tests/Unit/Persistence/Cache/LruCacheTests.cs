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
    public void Set_CapacityOne_EvictsOnSecondInsert()
    {
        LruCache<string, int> cache = new(1);

        cache.Set("a", 1);
        cache.Set("b", 2);

        Assert.That(cache.Count, Is.EqualTo(1));
        Assert.That(cache.TryGet("a", out _), Is.False);
        Assert.That(cache.TryGet("b", out int bValue), Is.True);
        Assert.That(bValue, Is.EqualTo(2));
    }

    [Test]
    public void Set_ExistingKey_WhenAlreadyHead_UpdatesValue()
    {
        LruCache<string, int> cache = new(3);

        cache.Set("a", 1);
        cache.Set("a", 42);

        Assert.That(cache.Count, Is.EqualTo(1));
        Assert.That(cache.TryGet("a", out int value), Is.True);
        Assert.That(value, Is.EqualTo(42));
    }

    [Test]
    public void Remove_OnlyItem_LeavesEmptyCacheAndRemainsUsable()
    {
        LruCache<string, int> cache = new(3);
        cache.Set("a", 1);

        Assert.That(cache.Remove("a"), Is.True);
        Assert.That(cache.Count, Is.Zero);
        Assert.That(cache.TryGet("a", out _), Is.False);

        cache.Set("b", 2);
        Assert.That(cache.Count, Is.EqualTo(1));
        Assert.That(cache.TryGet("b", out int bValue), Is.True);
        Assert.That(bValue, Is.EqualTo(2));
    }

    [Test]
    public void Remove_MiddleNode_RewiresLinks()
    {
        LruCache<string, int> cache = new(5);
        cache.Set("a", 1);
        cache.Set("b", 2);
        cache.Set("c", 3);

        Assert.That(cache.Remove("b"), Is.True);
        Assert.That(cache.Count, Is.EqualTo(2));
        Assert.That(cache.TryGet("a", out int aValue), Is.True);
        Assert.That(aValue, Is.EqualTo(1));
        Assert.That(cache.TryGet("c", out int cValue), Is.True);
        Assert.That(cValue, Is.EqualTo(3));
    }

    [Test]
    public void Clear_EmptyCache_DoesNotThrow()
    {
        LruCache<string, int> cache = new(3);

        Assert.DoesNotThrow(cache.Clear);
        Assert.That(cache.Count, Is.Zero);
    }

    [Test]
    public void Remove_AllItems_OneByOne_LeavesEmptyCache()
    {
        LruCache<string, int> cache = new(3);
        cache.Set("a", 1);
        cache.Set("b", 2);
        cache.Set("c", 3);

        Assert.That(cache.Remove("b"), Is.True);
        Assert.That(cache.Remove("c"), Is.True);
        Assert.That(cache.Remove("a"), Is.True);
        Assert.That(cache.Count, Is.Zero);

        cache.Set("x", 10);
        Assert.That(cache.TryGet("x", out int xValue), Is.True);
        Assert.That(xValue, Is.EqualTo(10));
    }

    [Test]
    public void Set_AfterRemoveAll_WorksCorrectly()
    {
        LruCache<string, int> cache = new(2);
        cache.Set("a", 1);
        cache.Set("b", 2);
        cache.Remove("a");
        cache.Remove("b");

        cache.Set("c", 3);
        cache.Set("d", 4);
        cache.Set("e", 5);

        Assert.That(cache.Count, Is.EqualTo(2));
        Assert.That(cache.TryGet("c", out _), Is.False);
        Assert.That(cache.TryGet("d", out _), Is.True);
        Assert.That(cache.TryGet("e", out _), Is.True);
    }

    [Test]
    public void TryGet_OnlyItem_PromotesAndKeepsHead()
    {
        LruCache<string, int> cache = new(2);
        cache.Set("a", 1);

        Assert.That(cache.TryGet("a", out int value), Is.True);
        Assert.That(value, Is.EqualTo(1));

        cache.Set("b", 2);
        cache.Set("c", 3);

        Assert.That(cache.TryGet("a", out _), Is.False);
        Assert.That(cache.TryGet("b", out _), Is.True);
        Assert.That(cache.TryGet("c", out _), Is.True);
    }

    [Test]
    public void Set_NullValue_IsAllowed()
    {
        LruCache<string, string?> cache = new(2);

        cache.Set("a", null);

        Assert.That(cache.TryGet("a", out string? value), Is.True);
        Assert.That(value, Is.Null);
        Assert.That(cache.Count, Is.EqualTo(1));
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

    [Test]
    public void ConcurrentAccess_SetAndRemove_DoesNotCorruptState()
    {
        LruCache<int, int> cache = new(32);
        const int threads = 4;
        const int operationsPerThread = 2_000;

        Parallel.For(0, threads, t =>
        {
            for (int i = 0; i < operationsPerThread; i++)
            {
                int key = ((t * operationsPerThread) + i) % 64;
                cache.Set(key, key);
                cache.Remove(key);
            }
        });

        Assert.That(cache.Count, Is.LessThanOrEqualTo(cache.Capacity));
    }

    [Test]
    public void ConcurrentAccess_SetClearAndTryGet_DoesNotCorruptState()
    {
        LruCache<int, int> cache = new(16);

        Parallel.For(0, 4, t =>
        {
            for (int i = 0; i < 1_000; i++)
            {
                cache.Set(i, i);
                cache.TryGet(i, out _);

                if (i % 100 == 0)
                {
                    cache.Clear();
                }
            }
        });

        Assert.That(cache.Count, Is.LessThanOrEqualTo(cache.Capacity));
    }
}
