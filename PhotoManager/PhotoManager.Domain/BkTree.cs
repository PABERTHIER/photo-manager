using Microsoft.Extensions.Logging;

namespace PhotoManager.Domain;

/// <summary>
/// A Burkhard-Keller tree for nearest-neighbor search under Hamming distance.
/// Provides practically sub-quadratic duplicate detection compared to exhaustive pairwise comparison.
/// </summary>
/// <remarks>This class is not thread-safe.</remarks>
public sealed class BkTree(ILogger logger)
{
    private BkNode? _root;
    private readonly List<List<Asset>> _groups = [];

    // Reused across TryAddToExistingGroups calls to eliminate per-call Stack/List allocations.
    private readonly List<BkNode> _traversalStack = [];

    /// <summary>
    /// Inserts an asset under the given hash key.
    /// If the exact hash already exists in the tree, the asset is appended to that group.
    /// </summary>
    public void Insert(string hash, Asset asset)
    {
        if (_root == null)
        {
            List<Asset> newGroup = [asset];
            _root = new BkNode(hash, newGroup);
            _groups.Add(newGroup);

            return;
        }

        BkNode current = _root;

        // The tree is acyclic by construction: AddChild always allocates a fresh BKNode.
        // Each iteration either returns (distance==0 or no child) or descends one level deeper.
        // Depth is bounded by the number of prior Insert calls, so this always terminates.
        while (true)
        {
            int distance = HashingHelper.CalculateHammingDistance(hash, current.Hash, logger);

            if (distance == 0)
            {
                current.Group.Add(asset);

                return;
            }

            if (current.TryGetChild(distance, out BkNode? child))
            {
                current = child!;
            }
            else
            {
                List<Asset> newGroup = [asset];
                current.AddChild(distance, new BkNode(hash, newGroup));
                _groups.Add(newGroup);

                return;
            }
        }
    }

    /// <summary>
    /// Adds <paramref name="asset"/> to every existing group whose representative hash
    /// is within <paramref name="threshold"/> Hamming distance of <paramref name="hash"/>.
    /// Reuses internal traversal state — do not call concurrently.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the asset was added to at least one existing group;
    /// <see langword="false"/> if no match was found and the caller should call <see cref="Insert"/>.
    /// </returns>
    public bool TryAddToExistingGroups(string hash, Asset asset, int threshold)
    {
        if (_root == null)
        {
            return false;
        }

        bool matched = false;

        _traversalStack.Clear();
        _traversalStack.Add(_root);

        while (_traversalStack.Count > 0)
        {
            BkNode current = _traversalStack[^1];
            _traversalStack.RemoveAt(_traversalStack.Count - 1);

            int distance = HashingHelper.CalculateHammingDistance(hash, current.Hash, logger);

            if (distance <= threshold)
            {
                current.Group.Add(asset);
                matched = true;
            }

            int low = Math.Max(0, distance - threshold);
            int high = distance + threshold;

            current.PushChildrenInRange(low, high, _traversalStack);
        }

        return matched;
    }

    /// <summary>
    /// Returns all asset groups in insertion order.
    /// </summary>
    public IReadOnlyList<List<Asset>> GetAllGroups() => _groups;

    private sealed class BkNode(string hash, List<Asset> group)
    {
        public string Hash { get; } = hash;
        public List<Asset> Group { get; } = group;

        // Lazy: null until the first child is added, saving ~104 bytes per leaf node.
        private Dictionary<int, BkNode>? _children;

        public bool TryGetChild(int distance, out BkNode? child)
        {
            if (_children == null)
            {
                child = null;

                return false;
            }

            return _children.TryGetValue(distance, out child);
        }

        public void AddChild(int distance, BkNode child)
        {
            _children ??= [];
            _children[distance] = child;
        }

        public void PushChildrenInRange(int low, int high, List<BkNode> stack)
        {
            if (_children == null)
            {
                return;
            }

            foreach ((int childDist, BkNode child) in _children)
            {
                if (childDist >= low && childDist <= high)
                {
                    stack.Add(child);
                }
            }
        }
    }
}
