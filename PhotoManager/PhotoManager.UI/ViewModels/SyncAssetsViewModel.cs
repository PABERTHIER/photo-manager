using PhotoManager.Application;
using PhotoManager.Domain;
using System.Collections.ObjectModel;

namespace PhotoManager.UI.ViewModels;

public class SyncAssetsViewModel(IApplication application)
    : BaseProcessViewModel<SyncAssetsConfiguration, SyncAssetsResult>
{
    public override string Description => "This process allows to sync new assets to the catalog. " +
                                          "You can configure one or multiple sync operations by entering a source path and a destination path. " +
                                          "You can specify if the sync operation should also include sub-folders. " +
                                          "There is also the option to delete from the destination path the assets not present in the source path.";

    public ObservableCollection<SyncAssetsDirectoriesDefinition> Definitions
    {
        get;
        set
        {
            field = value;
            NotifyPropertyChanged(nameof(Definitions));
        }
    } = [];

    public void DeleteDefinition(SyncAssetsDirectoriesDefinition definition)
    {
        if (Definitions.Remove(definition))
        {
            NotifyPropertyChanged(nameof(Definitions));
        }
    }

    public void MoveUpDefinition(SyncAssetsDirectoriesDefinition definition)
    {
        Definitions.MoveUp(definition);
    }

    public void MoveDownDefinition(SyncAssetsDirectoriesDefinition definition)
    {
        Definitions.MoveDown(definition);
    }

    public override SyncAssetsConfiguration GetProcessConfiguration() => application.GetSyncAssetsConfiguration();

    public override void SetProcessConfiguration(SyncAssetsConfiguration configuration) =>
        application.SetSyncAssetsConfiguration(configuration);

    public override async Task RunProcessAsync(ProcessStatusChangedCallback callback)
    {
        List<SyncAssetsResult> results = await application.SyncAssetsAsync(callback);
        Results = [.. results];
    }
}
