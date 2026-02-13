using PhotoManager.Domain;
using System.Windows;

namespace PhotoManager.UI.ViewModels;

public class DuplicatedAssetViewModel : BaseViewModel
{
    public required Asset Asset { get; init; }

    public Visibility Visible
    {
        get;
        set
        {
            field = value;
            NotifyPropertyChanged(nameof(Visible));
            ParentViewModel.NotifyAssetChanged();
        }
    }

    public DuplicatedSetViewModel ParentViewModel { get; init; } = [];
}
