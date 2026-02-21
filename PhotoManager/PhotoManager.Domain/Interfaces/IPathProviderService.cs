namespace PhotoManager.Domain.Interfaces;

public interface IPathProviderService
{
    string ResolveDataDirectory(string storageVersion);
}
