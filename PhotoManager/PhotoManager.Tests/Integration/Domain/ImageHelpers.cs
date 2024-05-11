namespace PhotoManager.Tests.Integration.Domain;

public static class ImageHelpers
{
    public static void CreateInvalidImage(string validImagePath, string invalidImagePath)
    {
        // Copy the valid image to a new file
        File.Copy(validImagePath, invalidImagePath, overwrite: true);
    
        bool isHeic = invalidImagePath.EndsWith(".heic", StringComparison.OrdinalIgnoreCase);

        // Open the new file in binary mode
        using (FileStream fileStream = new (invalidImagePath, FileMode.Open, FileAccess.ReadWrite))
        {
            if (isHeic)
            {
                // Truncate the file by removing a larger portion for HEIC files
                fileStream.SetLength(fileStream.Length - 1000);
            }
            else
            {
                // Corrupt the file header by changing the second byte
                fileStream.Seek(1, SeekOrigin.Begin);
                fileStream.WriteByte(0x00); // Change 0xD8 to 0x00
            }
        }
    }
}
