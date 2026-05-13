namespace PhotoManager.Persistence.Cache;

/// <summary>
/// Doubly-linked-list + dictionary LRU cache.
/// All public operations are O(1) and protected by a single lock.
/// Capacity must be greater than 0 and is fixed for the lifetime of the cache.
/// </summary>
public sealed class LruCache<TKey, TValue> : ILruCache<TKey, TValue> where TKey : notnull
{
    private sealed class Node
    {
        public TKey Key = default!;
        public TValue Value = default!;
        public Node? Prev;
        public Node? Next;
    }

    private readonly Dictionary<TKey, Node> _map;
    private readonly Lock _lock = new();
    private Node? _head; // most-recently-used
    private Node? _tail; // least-recently-used

    public LruCache(int capacity)
    {
        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be greater than zero.");
        }

        Capacity = capacity;
        _map = new Dictionary<TKey, Node>(capacity);
    }

    public int Capacity { get; }

    public int Count
    {
        get
        {
            lock (_lock)
            {
                return _map.Count;
            }
        }
    }

    public bool TryGet(TKey key, out TValue value)
    {
        lock (_lock)
        {
            if (_map.TryGetValue(key, out Node? node))
            {
                MoveToFront(node);
                value = node.Value;
                return true;
            }

            value = default!;
            return false;
        }
    }

    public void Set(TKey key, TValue value)
    {
        lock (_lock)
        {
            if (_map.TryGetValue(key, out Node? existing))
            {
                existing.Value = value;
                MoveToFront(existing);
                return;
            }

            Node node = new() { Key = key, Value = value };
            _map[key] = node;
            AddToFront(node);

            if (_map.Count > Capacity)
            {
                EvictTail();
            }
        }
    }

    public bool Remove(TKey key)
    {
        lock (_lock)
        {
            if (!_map.Remove(key, out Node? node))
            {
                return false;
            }

            Detach(node);
            return true;
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _map.Clear();
            _head = null;
            _tail = null;
        }
    }

    private void AddToFront(Node node)
    {
        node.Prev = null;
        node.Next = _head;

        if (_head is { } existingHead)
        {
            existingHead.Prev = node;
        }

        _head = node;
        _tail ??= node;
    }

    private void MoveToFront(Node node)
    {
        if (ReferenceEquals(node, _head))
        {
            return;
        }

        Detach(node);
        AddToFront(node);
    }

    private void Detach(Node node)
    {
        if (node.Prev is not null)
        {
            node.Prev.Next = node.Next;
        }
        else
        {
            _head = node.Next;
        }

        if (node.Next is not null)
        {
            node.Next.Prev = node.Prev;
        }
        else
        {
            _tail = node.Prev;
        }

        node.Prev = null;
        node.Next = null;
    }

    private void EvictTail()
    {
        if (_tail is null)
        {
            return;
        }

        _map.Remove(_tail.Key);
        Detach(_tail);
    }
}
