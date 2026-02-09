namespace PhotoManager.Domain.Comparers;

public class DateTimeAssetComparer(bool ascending, Func<Asset, DateTime> dateTimeSelector) : IComparer<Asset>
{
    public int Compare(Asset? asset1, Asset? asset2)
    {
        LongAssetComparer comparer = new(ascending, asset => dateTimeSelector(asset).Ticks / TimeSpan.TicksPerSecond);

        return comparer.Compare(asset1, asset2);
    }
}
