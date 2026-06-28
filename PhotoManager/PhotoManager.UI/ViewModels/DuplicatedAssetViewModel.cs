namespace PhotoManager.UI.ViewModels;

public class DuplicatedAssetViewModel : BaseViewModel
{
    public required Asset Asset { get; init; }

    public string FullPath => Asset.FullPath;

    public string PixelSize => $"{Asset.Pixel.Asset.Width}x{Asset.Pixel.Asset.Height} pixels";

    public long FileSize => Asset.FileProperties.Size;

    public DateTime FileCreation => Asset.FileProperties.Creation;

    public DateTime FileModification => Asset.FileProperties.Modification;

    public bool IsVisible
    {
        get;
        set
        {
            field = value;
            NotifyPropertyChanged(nameof(IsVisible));
            ParentViewModel.NotifyAssetChanged();
        }
    } = true;

    public DuplicatedSetViewModel ParentViewModel { get; init; } = [];
}
