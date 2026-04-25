using System.Runtime.InteropServices;

namespace PhotoManager.Domain;

public class AssetsComparator : IAssetsComparator
{
    public string[] GetNewFileNames(string[] fileNames, List<Asset> cataloguedAssets)
    {
        HashSet<string> cataloguedSet = new(cataloguedAssets.Count);

        foreach (Asset cataloguedAsset in CollectionsMarshal.AsSpan(cataloguedAssets))
        {
            cataloguedSet.Add(cataloguedAsset.FileName);
        }

        List<string> result = new(fileNames.Length);

        foreach (string fileName in fileNames)
        {
            if (!cataloguedSet.Contains(fileName) && IsValidAsset(fileName))
            {
                result.Add(fileName);
            }
        }

        return [.. result];
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
        HashSet<string> fileNameSet = [.. fileNames];
        List<string> result = new(cataloguedAssets.Count);

        foreach (Asset cataloguedAsset in CollectionsMarshal.AsSpan(cataloguedAssets))
        {
            if (!fileNameSet.Contains(cataloguedAsset.FileName))
            {
                result.Add(cataloguedAsset.FileName);
            }
        }

        return [.. result];
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
        return a => a.FileProperties.Creation > a.ThumbnailCreationDateTime
                    || a.FileProperties.Modification > a.ThumbnailCreationDateTime;
    }

    private static (string[], string[]) GetImageAndVideoNamesList(string[] fileNames)
    {
        List<string> imageNames = new(fileNames.Length);
        List<string> videoNames = new(fileNames.Length / 4);

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
