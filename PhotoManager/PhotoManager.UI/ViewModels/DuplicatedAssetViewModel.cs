namespace PhotoManager.UI.ViewModels;

public class DuplicatedAssetViewModel : BaseViewModel
{
    public required Asset Asset { get; init; }

    public string FullPath => Asset.FullPath;

    public string PixelSize => $"{Asset.Pixel.Asset.Width}x{Asset.Pixel.Asset.Height} pixels";

    public long FileSize => Asset.FileProperties.Size;

    public DateTime FileCreation => Asset.FileProperties.Creation;

    public DateTime FileModification => Asset.FileProperties.Modification;

    // The setter raises only this asset's own IsVisible change. It deliberately does NOT notify the parent set:
    // a set's DuplicatesCount/IsVisible are aggregates recomputed via DuplicatedSetViewModel.NotifyAssetChanged,
    // which the owning bulk operation (FindDuplicatedAssetsViewModel.CollapseAssets) raises once per affected set.
    // Notifying the parent here instead fired two LINQ Count scans per collapsed asset, i.e. O(deleted) redundant
    // set notifications on a large "delete all but this".
    public bool IsVisible
    {
        get;
        set
        {
            field = value;
            NotifyPropertyChanged(nameof(IsVisible));
        }
    } = true;

    public DuplicatedSetViewModel ParentViewModel { get; init; } = [];
}
