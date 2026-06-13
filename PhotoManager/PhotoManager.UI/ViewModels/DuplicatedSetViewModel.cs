using System.ComponentModel;

namespace PhotoManager.UI.ViewModels;

public class DuplicatedSetViewModel : List<DuplicatedAssetViewModel>, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    // Used in the xaml file to display the first FileName of each sets
    public string FileName => Count > 0 ? this[0].Asset.FileName : string.Empty;

    public int DuplicatesCount => GetVisibleDuplicates();

    public bool IsVisible => GetVisibleDuplicates() > 1;

    public void NotifyAssetChanged()
    {
        PropertyChanged?.Invoke(this, new(nameof(DuplicatesCount)));
        PropertyChanged?.Invoke(this, new(nameof(IsVisible)));
    }

    private int GetVisibleDuplicates()
    {
        return this.Count(static asset => asset.IsVisible);
    }
}
