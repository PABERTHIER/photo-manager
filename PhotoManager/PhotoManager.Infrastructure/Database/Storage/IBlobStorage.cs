namespace PhotoManager.Infrastructure.Database.Storage;

public interface IBlobStorage
{
    Dictionary<string, byte[]>? ReadFromBinaryFile(string binaryFilePath);
    void WriteToBinaryFile(Dictionary<string, byte[]> data, string binaryFilePath);
}
