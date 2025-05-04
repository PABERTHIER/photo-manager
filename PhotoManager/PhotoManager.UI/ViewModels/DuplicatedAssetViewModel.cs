using PhotoManager.Application;
using PhotoManager.Domain;
using System.Windows;

namespace PhotoManager.UI.ViewModels;

public class DuplicatedAssetViewModel(IApplication application) : BaseViewModel(application)
{
    private Visibility _visible;

    public required Asset Asset { get; init; }

    public Visibility Visible
    {
        get => _visible;
        set
        {
            _visible = value;
            NotifyPropertyChanged(nameof(Visible));
            ParentViewModel.NotifyAssetChanged();
        }
    }

    public DuplicatedSetViewModel ParentViewModel { get; init; } = [];
}
