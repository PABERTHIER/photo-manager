namespace PhotoManager.Persistence.Cache;

/// <summary>
/// Thread-safe Least-Recently-Used cache with O(1) get / set / evict.
/// </summary>
public interface ILruCache<in TKey, TValue> where TKey : notnull
{
    int Count { get; }
    int Capacity { get; }
    bool TryGet(TKey key, out TValue value);
    void Set(TKey key, TValue value);
    bool Remove(TKey key);
    void Clear();
}
