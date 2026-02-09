namespace PhotoManager.Tests;

public static class ImageHelper
{
    public static void CreateInvalidImage(string validImagePath, string invalidImagePath)
    {
        // Copy the valid image to a new file
        File.Copy(validImagePath, invalidImagePath, overwrite: true);

        bool isHeic = invalidImagePath.EndsWith(".heic", StringComparison.OrdinalIgnoreCase);

        // Open the new file in binary mode
        using (FileStream fileStream = new(invalidImagePath, FileMode.Open, FileAccess.ReadWrite))
        {
            if (isHeic)
            {
                // Corrupt the HEIC file header
                // HEIC files start with 'ftyp' box at offset 4
                // Corrupting this critical header makes the file unreadable
                fileStream.Seek(4, SeekOrigin.Begin);
                fileStream.WriteByte(0x00);
                fileStream.WriteByte(0x00);
                fileStream.WriteByte(0x00);
            }
            else
            {
                // Corrupt the file header by changing the second byte
                fileStream.Seek(1, SeekOrigin.Begin);
            }

            fileStream.WriteByte(0x00); // Change 0xD8 to 0x00
        }
    }
}
