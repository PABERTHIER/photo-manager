namespace PhotoManager.Domain.Comparers;

public class LongAssetComparer(bool ascending, Func<Asset, long> longSelector) : IComparer<Asset>
{
    public int Compare(Asset? asset1, Asset? asset2)
    {
        if (asset1 == null || asset2 == null)
        {
            throw new ArgumentNullException(asset1 == null ? nameof(asset1) : nameof(asset2));
        }

        int result = longSelector(asset1).CompareTo(longSelector(asset2));

        if (result == 0)
        {
            StringAssetComparer comparer = new(ascending, asset => asset.FileName);
            return comparer.Compare(asset1, asset2);
        }

        return ascending ? result : -result;
    }
}
