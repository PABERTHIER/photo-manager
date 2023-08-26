using System.IO;

namespace PhotoManager.Infrastructure.Database.Storage;

public class BlobStorage : IBlobStorage
{
    // Key is imageName (string), value is the binary file -> image data (byte[])
    public Dictionary<string, byte[]>? ReadFromBinaryFile(string binaryFilePath)
    {
        Dictionary<string, byte[]>? result = null;

        if (File.Exists(binaryFilePath))
        {
            using (FileStream fileStream = new(binaryFilePath, FileMode.Open))
            {
                using (BinaryReader reader = new(fileStream))
                {
                    int itemCount = reader.ReadInt32();
                    result = new Dictionary<string, byte[]>(itemCount);

                    for (int i = 0; i < itemCount; i++)
                    {
                        string key = reader.ReadString();
                        int valueLength = reader.ReadInt32();
                        byte[] value = reader.ReadBytes(valueLength);

                        result[key] = value;
                    }
                }
            }
        }

        return result;
    }

    public void WriteToBinaryFile(Dictionary<string, byte[]> data, string binaryFilePath)
    {
        using (FileStream fileStream = new(binaryFilePath, FileMode.Create))
        {
            using (BinaryWriter writer = new(fileStream))
            {
                writer.Write(data.Count);

                foreach (var kvp in data)
                {
                    writer.Write(kvp.Key);
                    writer.Write(kvp.Value.Length);
                    writer.Write(kvp.Value);
                }
            }
        }
    }
}
