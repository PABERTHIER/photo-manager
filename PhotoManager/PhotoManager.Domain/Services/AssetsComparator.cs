using System.Runtime.InteropServices;

namespace PhotoManager.Domain.Services;

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

        return CopyToArray(result);
    }

    public string[] GetNewFileNamesToSync(string[] sourceFileNames, string[] destinationFileNames)
    {
        HashSet<string> destinationSet = CreateHashSet(destinationFileNames);
        List<string> result = new(sourceFileNames.Length);
        HashSet<string> resultSet = [];

        for (int i = 0; i < sourceFileNames.Length; i++)
        {
            string fileName = sourceFileNames[i];

            if (!destinationSet.Contains(fileName) && resultSet.Add(fileName) && IsValidAsset(fileName))
            {
                result.Add(fileName);
            }
        }

        return CopyToArray(result);
    }

    public (string[], string[]) GetImageAndVideoNames(string[] fileNames)
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

        return (CopyToArray(imageNames), CopyToArray(videoNames));
    }

    public string[] GetUpdatedFileNames(List<Asset> cataloguedAssets)
    {
        List<string> result = new(cataloguedAssets.Count);

        foreach (Asset cataloguedAsset in CollectionsMarshal.AsSpan(cataloguedAssets))
        {
            if (IsUpdatedAsset(cataloguedAsset))
            {
                result.Add(cataloguedAsset.FileName);
            }
        }

        return CopyToArray(result);
    }

    public string[] GetDeletedFileNames(string[] fileNames, List<Asset> cataloguedAssets)
    {
        HashSet<string> fileNameSet = CreateHashSet(fileNames);
        List<string> result = new(cataloguedAssets.Count);

        foreach (Asset cataloguedAsset in CollectionsMarshal.AsSpan(cataloguedAssets))
        {
            if (!fileNameSet.Contains(cataloguedAsset.FileName))
            {
                result.Add(cataloguedAsset.FileName);
            }
        }

        return CopyToArray(result);
    }

    public string[] GetDeletedFileNamesToSync(string[] fileNames, string[] destinationFileNames)
    {
        HashSet<string> fileNameSet = CreateHashSet(fileNames);
        List<string> result = new(destinationFileNames.Length);
        HashSet<string> resultSet = [];

        for (int i = 0; i < destinationFileNames.Length; i++)
        {
            string destinationFileName = destinationFileNames[i];

            if (!fileNameSet.Contains(destinationFileName) && resultSet.Add(destinationFileName))
            {
                result.Add(destinationFileName);
            }
        }

        return CopyToArray(result);
    }

    private static bool IsValidAsset(string assetFileName)
    {
        return ImageHelper.IsImageFile(assetFileName) || VideoHelper.IsVideoFile(assetFileName);
    }

    private static bool IsUpdatedAsset(Asset asset)
    {
        return asset.FileProperties.Creation > asset.ThumbnailCreationDateTime
               || asset.FileProperties.Modification > asset.ThumbnailCreationDateTime;
    }

    private static string[] CopyToArray(List<string> values)
    {
        string[] result = new string[values.Count];
        values.CopyTo(result);
        return result;
    }

    private static HashSet<string> CreateHashSet(string[] values)
    {
        HashSet<string> result = new(values.Length);

        for (int i = 0; i < values.Length; i++)
        {
            result.Add(values[i]);
        }

        return result;
    }
}
