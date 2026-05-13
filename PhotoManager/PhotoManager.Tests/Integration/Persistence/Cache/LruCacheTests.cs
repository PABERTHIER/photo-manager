using PhotoManager.Persistence.Cache;

namespace PhotoManager.Tests.Integration.Persistence.Cache;

[TestFixture]
public class LruCacheTests
{
    [Test]
    public void Set_LoadAndEvict_MaintainsCapacity()
    {
        LruCache<Guid, Dictionary<string, byte[]>> cache = new(3);

        Guid folder1 = Guid.NewGuid();
        Guid folder2 = Guid.NewGuid();
        Guid folder3 = Guid.NewGuid();
        Guid folder4 = Guid.NewGuid();

        Dictionary<string, byte[]> thumbnails1 = new()
        {
            ["img1.jpg"] = [0xFF, 0xD8, 0xFF, 0xE0],
            ["img2.jpg"] = [0x89, 0x50, 0x4E, 0x47]
        };

        Dictionary<string, byte[]> thumbnails2 = new()
        {
            ["photo.png"] = [0x89, 0x50, 0x4E, 0x47, 0x0D]
        };

        Dictionary<string, byte[]> thumbnails3 = new()
        {
            ["scan.tiff"] = [0x49, 0x49, 0x2A, 0x00]
        };

        Dictionary<string, byte[]> thumbnails4 = new()
        {
            ["new.jpg"] = [0xFF, 0xD8]
        };

        cache.Set(folder1, thumbnails1);
        cache.Set(folder2, thumbnails2);
        cache.Set(folder3, thumbnails3);

        Assert.That(cache.Count, Is.EqualTo(3));

        cache.Set(folder4, thumbnails4);

        Assert.That(cache.Count, Is.EqualTo(3));
        Assert.That(cache.TryGet(folder1, out _), Is.False);
        Assert.That(cache.TryGet(folder2, out _), Is.True);
        Assert.That(cache.TryGet(folder3, out _), Is.True);
        Assert.That(cache.TryGet(folder4, out _), Is.True);
    }

    [Test]
    public void Set_AccessPromotes_EvictsCorrectly()
    {
        LruCache<Guid, Dictionary<string, byte[]>> cache = new(2);

        Guid folder1 = Guid.NewGuid();
        Guid folder2 = Guid.NewGuid();
        Guid folder3 = Guid.NewGuid();

        cache.Set(folder1, new Dictionary<string, byte[]>
        {
            ["a.jpg"] = [1, 2, 3]
        });

        cache.Set(folder2, new Dictionary<string, byte[]>
        {
            ["b.jpg"] = [4, 5, 6]
        });

        cache.TryGet(folder1, out _);

        cache.Set(folder3, new Dictionary<string, byte[]>
        {
            ["c.jpg"] = [7, 8, 9]
        });

        Assert.That(cache.TryGet(folder1, out _), Is.True);
        Assert.That(cache.TryGet(folder2, out _), Is.False);
        Assert.That(cache.TryGet(folder3, out _), Is.True);
    }

    [Test]
    public void ConcurrentThumbnailAccess_DoesNotDeadlock()
    {
        LruCache<Guid, Dictionary<string, byte[]>> cache = new(10);
        Guid[] folderIds = [.. Enumerable.Range(0, 50).Select(_ => Guid.NewGuid())];

        Parallel.For(0, 8, _ =>
        {
            for (int i = 0; i < 500; i++)
            {
                Guid id = folderIds[i % folderIds.Length];

                cache.Set(id, new Dictionary<string, byte[]>
                {
                    [$"img_{i}.jpg"] = [(byte)(i % 256)]
                });

                cache.TryGet(id, out Dictionary<string, byte[]> _);

                if (i % 10 == 0)
                {
                    cache.Remove(id);
                }
            }
        });

        Assert.That(cache.Count, Is.LessThanOrEqualTo(cache.Capacity));
    }

    [Test]
    public void Set_LargeDataset_MaintainsPerformanceContract()
    {
        LruCache<int, byte[]> cache = new(100);

        for (int i = 0; i < 10_000; i++)
        {
            cache.Set(i, new byte[1024]);
        }

        Assert.That(cache.Count, Is.EqualTo(100));

        for (int i = 9_900; i < 10_000; i++)
        {
            Assert.That(cache.TryGet(i, out byte[] data), Is.True);
            Assert.That(data, Has.Length.EqualTo(1024));
        }

        Assert.That(cache.TryGet(0, out _), Is.False);
    }
}
