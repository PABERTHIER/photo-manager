namespace PhotoManager.Domain;

public class AssetsComparator : IAssetsComparator
{
    public string[] GetNewFileNames(string[] fileNames, List<Asset> cataloguedAssets)
    {
        return GetNewFileNamesList(fileNames, [.. cataloguedAssets.Select(ca => ca.FileName)]);
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
        return [.. cataloguedAssets.Where(IsUpdatedAsset()).Select(ca => ca.FileName)];
    }

    public string[] GetDeletedFileNames(string[] fileNames, List<Asset> cataloguedAssets)
    {
        return [.. cataloguedAssets.Select(ca => ca.FileName).Except(fileNames)];
    }

    public string[] GetDeletedFileNamesToSync(string[] fileNames, string[] destinationFileNames)
    {
        return [.. destinationFileNames.Except(fileNames)];
    }

    private static string[] GetNewFileNamesList(string[] fileNames, string[] destinationFileNames)
    {
        return [.. fileNames.Except(destinationFileNames).Where(IsValidAsset)];
    }

    private static bool IsValidAsset(string assetFileName)
    {
        return ImageHelper.IsImageFile(assetFileName) || VideoHelper.IsVideoFile(assetFileName);
    }

    private static Func<Asset, bool> IsUpdatedAsset()
    {
        return a => a.FileProperties.Creation > a.ThumbnailCreationDateTime || a.FileProperties.Modification > a.ThumbnailCreationDateTime;
    }

    private static (string[], string[]) GetImageAndVideoNamesList(string[] fileNames)
    {
        List<string> imageNames = [];
        List<string> videoNames = [];

        foreach (string fileName in fileNames)
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

        return ([.. imageNames], [.. videoNames]);
    }
}
