using System.ComponentModel;
using System.Windows;

namespace PhotoManager.UI.ViewModels;

// TODO: Merge this VM into FindDuplicatedAssetsViewModel since it's just a List<DuplicatedAssetViewModel>
public class DuplicatedSetViewModel : List<DuplicatedAssetViewModel>, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    // TODO: Delete this code since it's the same in BaseViewModel ?
    protected void NotifyPropertyChanged(params string[] propertyNames)
    {
        foreach (string propertyName in propertyNames)
        {
            PropertyChanged?.Invoke(this, new(propertyName));
        }
    }

    // Used in the xaml file to display the first FileName of each sets
    public string FileName => this[0].Asset.FileName;

    public int DuplicatesCount => GetVisibleDuplicates();

    public Visibility Visible => GetVisibleDuplicates() > 1 ? Visibility.Visible : Visibility.Collapsed;

    public void NotifyAssetChanged()
    {
        NotifyPropertyChanged(nameof(DuplicatesCount), nameof(Visible));
    }

    private int GetVisibleDuplicates()
    {
        return this.Count(a => a.Visible == Visibility.Visible);
    }
}
