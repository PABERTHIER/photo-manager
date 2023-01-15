using PhotoManager.Common;
using PhotoManager.Domain.Interfaces;

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
                        .Where(ImageHelper.IsImageFile)
                        .ToArray();
    }
}
