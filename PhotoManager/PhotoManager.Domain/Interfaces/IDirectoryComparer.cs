namespace PhotoManager.Domain.Interfaces;

public interface IDirectoryComparer
{
    string[] GetDeletedFileNames(string[] fileNames, List<Asset> cataloguedAssets);
    string[] GetDeletedFileNames(string[] fileNames, string[] destinationFileNames);
    string[] GetNewFileNames(string[] fileNames, List<Asset> cataloguedAssets);
    string[] GetNewFileNames(string[] sourceFileNames, string[] destinationFileNames);
    (string[], string[]) GetImageAndVideoNames(string[] fileNames);
    string[] GetUpdatedFileNames(string[] fileNames, List<Asset> cataloguedAssets);
}
