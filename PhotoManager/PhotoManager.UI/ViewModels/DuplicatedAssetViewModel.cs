using PhotoManager.Application;
using PhotoManager.Domain;
using System.Windows;

namespace PhotoManager.UI.ViewModels;

public class DuplicatedAssetViewModel : BaseViewModel
{
    private Asset _asset;
    private Visibility _visible;

    public DuplicatedAssetViewModel(IApplication application) : base(application)
    {
        ParentViewModel = [];
    }

    public Asset Asset
    {
        get => _asset;
        set
        {
            _asset = value;
            NotifyPropertyChanged(nameof(Asset));
        }
    }

    public Visibility Visible
    {
        get => _visible;
        set
        {
            _visible = value;
            NotifyPropertyChanged(nameof(Visible));
            ParentViewModel.NotifyAssetChanged(this);
        }
    }

    public DuplicatedSetViewModel ParentViewModel { get; internal set; }
}
