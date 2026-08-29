namespace PhotoManager.Infrastructure;

public sealed class ApplicationDataPathProvider : IApplicationDataPathProvider
{
    public string GetPath()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    }
}
