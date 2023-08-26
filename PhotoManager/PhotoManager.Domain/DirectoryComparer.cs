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

    public string[] GetNewFileNames(string[] sourceFileNames, string[] destinationFileNames)
    {
        return GetNewFileNamesList(sourceFileNames, destinationFileNames);
    }

    public (string[], string[]) GetImageAndVideoNames(string[] fileNames)
    {
        return GetImageAndVideoNamesList(fileNames);
    }

    public string[] GetUpdatedFileNames(string[] fileNames, List<Asset> cataloguedAssets)
    {
        foreach (Asset asset in cataloguedAssets)
        {
            _storageService.GetFileInformation(asset);
        }

        return cataloguedAssets
            .Where(ca => ca.FileCreationDateTime > ca.ThumbnailCreationDateTime ||
                ca.FileModificationDateTime > ca.ThumbnailCreationDateTime)
            .Select(ca => ca.FileName)
            .ToArray();
    }

    public string[] GetDeletedFileNames(string[] fileNames, List<Asset> cataloguedAssets)
    {
        return cataloguedAssets.Select(ca => ca.FileName).Except(fileNames).ToArray();
    }

    public string[] GetDeletedFileNames(string[] fileNames, string[] destinationFileNames)
    {
        return destinationFileNames.Except(fileNames).ToArray();
    }

    private static string[] GetNewFileNamesList(string[] fileNames, string[] destinationFileNames)
    {
        return fileNames.Except(destinationFileNames)
                        .Where(fileName => ImageHelper.IsImageFile(fileName) || VideoHelper.IsVideoFile(fileName))
                        .ToArray();
    }

    private static (string[], string[]) GetImageAndVideoNamesList(string[] fileNames)
    {
        var imageNames = new List<string>();
        var videoNames = new List<string>();

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
