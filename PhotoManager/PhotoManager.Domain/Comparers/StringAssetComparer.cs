namespace PhotoManager.Domain.Comparers;

public class StringAssetComparer(bool ascending, Func<Asset, string> stringSelector) : IComparer<Asset>
{
    public int Compare(Asset? asset1, Asset? asset2)
    {
        if (asset1 == null || asset2 == null)
        {
            throw new ArgumentNullException(asset1 == null ? nameof(asset1) : nameof(asset2));
        }

        int result = string.Compare(stringSelector(asset1), stringSelector(asset2), StringComparison.Ordinal);

        return ascending ? result : -result;
    }
}
