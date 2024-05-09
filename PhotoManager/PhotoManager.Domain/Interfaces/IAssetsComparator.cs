namespace PhotoManager.Domain.Interfaces;

public interface IAssetsComparator
{
    string[] GetNewFileNames(string[] fileNames, List<Asset> cataloguedAssets);
    string[] GetNewFileNamesToSync(string[] sourceFileNames, string[] destinationFileNames);
    (string[], string[]) GetImageAndVideoNames(string[] fileNames);
    string[] GetUpdatedFileNames(List<Asset> cataloguedAssets);
    string[] GetDeletedFileNames(string[] fileNames, List<Asset> cataloguedAssets);
    string[] GetDeletedFileNamesToSync(string[] fileNames, string[] destinationFileNames);
}
