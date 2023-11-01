namespace PhotoManager.Domain;

public class DirectoryComparer : IDirectoryComparer
{
    private readonly IStorageService _storageService;

    public DirectoryComparer(IStorageService storageService)
    {
        _storageService = storageService;
    }

    public string[] GetNewFileNames(string[] fileNames, List<Asset> cataloguedAssets)
    {
        return GetNewFileNamesList(fileNames, cataloguedAssets.Select(ca => ca.FileName).ToArray());
    }

    public string[] GetNewFileNamesToSync(string[] sourceFileNames, string[] destinationFileNames)
    {
        return GetNewFileNamesList(sourceFileNames, destinationFileNames);
    }

    public (string[], string[]) GetImageAndVideoNames(string[] fileNames)
    {
        return GetImageAndVideoNamesList(fileNames);
    }

    public string[] GetUpdatedFileNames(List<Asset> cataloguedAssets)
    {
        foreach (Asset asset in cataloguedAssets)
        {
            _storageService.LoadFileInformation(asset);
        }

        return cataloguedAssets.Where(IsUpdatedAsset()).Select(ca => ca.FileName).ToArray();
    }

    public string[] GetDeletedFileNames(string[] fileNames, List<Asset> cataloguedAssets)
    {
        return cataloguedAssets.Select(ca => ca.FileName).Except(fileNames).ToArray();
    }

    public string[] GetDeletedFileNamesToSync(string[] fileNames, string[] destinationFileNames)
    {
        return destinationFileNames.Except(fileNames).ToArray();
    }

    private static string[] GetNewFileNamesList(string[] fileNames, string[] destinationFileNames)
    {
        return fileNames.Except(destinationFileNames).Where(IsValidAsset).ToArray();
    }

    private static bool IsValidAsset(string assetFileName)
    {
        return ImageHelper.IsImageFile(assetFileName) || VideoHelper.IsVideoFile(assetFileName);
    }

    private static Func<Asset, bool> IsUpdatedAsset()
    {
        return a => a.FileCreationDateTime > a.ThumbnailCreationDateTime || a.FileModificationDateTime > a.ThumbnailCreationDateTime;
    }

    private static (string[], string[]) GetImageAndVideoNamesList(string[] fileNames)
    {
        List<string> imageNames = new();
        List<string> videoNames = new();

        foreach (var fileName in fileNames)
        {
            if (ImageHelper.IsImageFile(fileName))
            {
                imageNames.Add(fileName);
            }

            else if (VideoHelper.IsVideoFile(fileName))
            {
                videoNames.Add(fileName);
            }
        }

        return (imageNames.ToArray(), videoNames.ToArray());
    }
}
