namespace PhotoManager.Domain.Comparers;

public class DateTimeAssetComparer(bool ascending, Func<Asset, DateTime> dateTimeSelector) : IComparer<Asset>
{
    public int Compare(Asset? asset1, Asset? asset2)
    {
        if (asset1 == null || asset2 == null)
        {
            throw new ArgumentNullException(asset1 == null ? nameof(asset1) : nameof(asset2));
        }

        long ticks1 = dateTimeSelector(asset1).Ticks / TimeSpan.TicksPerSecond;
        long ticks2 = dateTimeSelector(asset2).Ticks / TimeSpan.TicksPerSecond;
        int result = ticks1.CompareTo(ticks2);

        if (result == 0)
        {
            result = string.CompareOrdinal(asset1.FileName, asset2.FileName);
        }

        return ascending ? result : -result;
    }
}
