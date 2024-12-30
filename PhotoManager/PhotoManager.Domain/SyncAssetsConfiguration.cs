namespace PhotoManager.Domain;

public class SyncAssetsConfiguration
{
    public List<SyncAssetsDirectoriesDefinition> Definitions { get; } = [];

    public SyncAssetsConfiguration Validate()
    {
        Definitions.RemoveAll(d => !d.IsValid());

        return this;
    }

    public void Normalize()
    {
        Definitions.ForEach(d => d.Normalize());
    }
}
