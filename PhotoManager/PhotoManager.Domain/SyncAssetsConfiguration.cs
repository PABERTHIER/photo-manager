namespace PhotoManager.Domain;

public class SyncAssetsConfiguration
{
    public SyncAssetsConfiguration()
    {
        Definitions = [];
    }

    public List<SyncAssetsDirectoriesDefinition> Definitions { get; }

    // TODO: transform to fluent to have syncConfiguration.Validate().Normalize()
    public void Validate() // ISyncAssetsConfiguration Validate();
    {
        Definitions.RemoveAll(d => !d.IsValid());

        // return this;
    }

    public void Normalize() // ISyncAssetsConfiguration Normalize()
    {
        Definitions.ForEach(d => d.Normalize());

        // return this;
    }
}
