using PhotoManager.Persistence.Cache;
using Directories = PhotoManager.Tests.Integration.Constants.Directories;
using FileNames = PhotoManager.Tests.Integration.Constants.FileNames;

namespace PhotoManager.Tests.Integration.Persistence.Cache;

[TestFixture]
public class LruCacheTests
{
    private string? _assetsDirectory;

    private byte[]? _thumbnailData1;
    private byte[]? _thumbnailData2;
    private byte[]? _thumbnailData3;
    private byte[]? _thumbnailData4;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _assetsDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);

        _thumbnailData1 = File.ReadAllBytes(Path.Combine(_assetsDirectory, FileNames.IMAGE_1_JPG));
        _thumbnailData2 = File.ReadAllBytes(Path.Combine(_assetsDirectory, FileNames.HOMER_GIF));
        _thumbnailData3 = File.ReadAllBytes(Path.Combine(_assetsDirectory, FileNames.IMAGE_11_90_DEG_HEIC));
        _thumbnailData4 = File.ReadAllBytes(Path.Combine(_assetsDirectory, FileNames.IMAGE_9_PNG));
    }

    [Test]
    public void Set_LoadAndEvict_MaintainsCapacity()
    {
        LruCache<Guid, Dictionary<string, byte[]>> cache = new(3);

        Guid folderId1 = Guid.NewGuid();
        Guid folderId2 = Guid.NewGuid();
        Guid folderId3 = Guid.NewGuid();
        Guid folderId4 = Guid.NewGuid();

        Dictionary<string, byte[]> thumbnails1 = new()
        {
            [FileNames.IMAGE_1_JPG] = _thumbnailData1!,
            [FileNames.HOMER_GIF] = _thumbnailData2!
        };

        Dictionary<string, byte[]> thumbnails2 = new()
        {
            [FileNames.IMAGE_11_90_DEG_HEIC] = _thumbnailData3!
        };

        Dictionary<string, byte[]> thumbnails3 = new()
        {
            [FileNames.IMAGE_9_PNG] = _thumbnailData4!
        };

        Dictionary<string, byte[]> thumbnails4 = new()
        {
            [FileNames.IMAGE_4_JPG] = _thumbnailData1!
        };

        cache.Set(folderId1, thumbnails1);
        cache.Set(folderId2, thumbnails2);
        cache.Set(folderId3, thumbnails3);

        Assert.That(cache.Count, Is.EqualTo(3));

        cache.Set(folderId4, thumbnails4);

        Assert.That(cache.Count, Is.EqualTo(3));
        Assert.That(cache.TryGet(folderId1, out _), Is.False);
        Assert.That(cache.TryGet(folderId2, out _), Is.True);
        Assert.That(cache.TryGet(folderId3, out _), Is.True);
        Assert.That(cache.TryGet(folderId4, out _), Is.True);
    }

    [Test]
    public void Set_AccessPromotes_EvictsCorrectly()
    {
        LruCache<Guid, Dictionary<string, byte[]>> cache = new(2);

        Guid folderId1 = Guid.NewGuid();
        Guid folderId2 = Guid.NewGuid();
        Guid folderId3 = Guid.NewGuid();

        cache.Set(folderId1, new Dictionary<string, byte[]>
        {
            [FileNames.IMAGE_1_JPG] = _thumbnailData1!
        });

        cache.Set(folderId2, new Dictionary<string, byte[]>
        {
            [FileNames.HOMER_GIF] = _thumbnailData2!
        });

        cache.TryGet(folderId1, out _);

        cache.Set(folderId3, new Dictionary<string, byte[]>
        {
            [FileNames.IMAGE_11_90_DEG_HEIC] = _thumbnailData3!
        });

        Assert.That(cache.TryGet(folderId1, out _), Is.True);
        Assert.That(cache.TryGet(folderId2, out _), Is.False);
        Assert.That(cache.TryGet(folderId3, out _), Is.True);
    }

    [Test]
    public void TryGet_ExistingFolder_ReturnsCorrectThumbnailData()
    {
        LruCache<Guid, Dictionary<string, byte[]>> cache = new(5);

        Guid folderId = Guid.NewGuid();

        Dictionary<string, byte[]> thumbnails = new()
        {
            [FileNames.IMAGE_1_JPG] = _thumbnailData1!,
            [FileNames.IMAGE_9_PNG] = _thumbnailData4!
        };

        cache.Set(folderId, thumbnails);

        Assert.That(cache.TryGet(folderId, out Dictionary<string, byte[]> retrieved), Is.True);
        Assert.That(retrieved, Has.Count.EqualTo(2));
        Assert.That(retrieved[FileNames.IMAGE_1_JPG], Has.Length.EqualTo(_thumbnailData1!.Length));
        Assert.That(retrieved[FileNames.IMAGE_9_PNG], Has.Length.EqualTo(_thumbnailData4!.Length));
    }

    [Test]
    public void ConcurrentThumbnailAccess_DoesNotDeadlock()
    {
        LruCache<Guid, Dictionary<string, byte[]>> cache = new(10);
        Guid[] folderIds =
        [
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid()
        ];

        Parallel.For(0, 8, _ =>
        {
            for (int i = 0; i < 500; i++)
            {
                Guid id = folderIds[i % folderIds.Length];

                cache.Set(id, new Dictionary<string, byte[]>
                {
                    [FileNames.IMAGE_1_JPG] = _thumbnailData1!
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
        LruCache<Guid, Dictionary<string, byte[]>> cache = new(100);

        Guid[] ids = [.. Enumerable.Range(0, 10_000).Select(_ => Guid.NewGuid())];

        for (int i = 0; i < 10_000; i++)
        {
            cache.Set(ids[i], new Dictionary<string, byte[]>
            {
                [FileNames.IMAGE_1_JPG] = _thumbnailData1!
            });
        }

        Assert.That(cache.Count, Is.EqualTo(100));

        for (int i = 9_900; i < 10_000; i++)
        {
            Assert.That(cache.TryGet(ids[i], out Dictionary<string, byte[]> data), Is.True);
            Assert.That(data[FileNames.IMAGE_1_JPG], Has.Length.EqualTo(_thumbnailData1!.Length));
        }

        Assert.That(cache.TryGet(ids[0], out _), Is.False);
    }

    [Test]
    public void Remove_ExistingFolder_FreesSlotForNewEntry()
    {
        LruCache<Guid, Dictionary<string, byte[]>> cache = new(2);

        Guid folderId1 = Guid.NewGuid();
        Guid folderId2 = Guid.NewGuid();
        Guid folderId3 = Guid.NewGuid();

        cache.Set(folderId1, new Dictionary<string, byte[]>
        {
            [FileNames.IMAGE_1_JPG] = _thumbnailData1!
        });

        cache.Set(folderId2, new Dictionary<string, byte[]>
        {
            [FileNames.HOMER_GIF] = _thumbnailData2!
        });

        cache.Remove(folderId1);

        cache.Set(folderId3, new Dictionary<string, byte[]>
        {
            [FileNames.IMAGE_11_90_DEG_HEIC] = _thumbnailData3!
        });

        Assert.That(cache.Count, Is.EqualTo(2));
        Assert.That(cache.TryGet(folderId1, out _), Is.False);
        Assert.That(cache.TryGet(folderId2, out _), Is.True);
        Assert.That(cache.TryGet(folderId3, out _), Is.True);
    }
}
