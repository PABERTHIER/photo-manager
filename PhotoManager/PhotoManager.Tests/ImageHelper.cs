using SkiaSharp;

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

    public static bool IsValidImage(string filePath)
    {
        try
        {
            using (SKCodec? codec = SKCodec.Create(filePath))
            {
                return codec != null;
            }
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static void AssertValidImageOfFormat(string filePath, SKEncodedImageFormat expectedFormat)
    {
        Assert.That(File.Exists(filePath), Is.True);

        using (SKCodec? codec = SKCodec.Create(filePath))
        {
            Assert.That(codec, Is.Not.Null);
            Assert.That(codec.EncodedFormat, Is.EqualTo(expectedFormat));
        }
    }

    public static void AssertBufferHasExpectedSignature(byte[] imageBuffer, ImageEncodingFormat targetFormat)
    {
        switch (targetFormat)
        {
            case ImageEncodingFormat.Jpeg:
                // JPEG SOI marker (FF D8) followed by the start of the next marker (FF).
                Assert.That(imageBuffer, Has.Length.GreaterThanOrEqualTo(3));
                Assert.That(imageBuffer[0], Is.EqualTo((byte)0xFF));
                Assert.That(imageBuffer[1], Is.EqualTo((byte)0xD8));
                Assert.That(imageBuffer[2], Is.EqualTo((byte)0xFF));
                break;

            case ImageEncodingFormat.Png:
                // Full 8-byte PNG signature.
                Assert.That(imageBuffer, Has.Length.GreaterThanOrEqualTo(8));
                Assert.That(imageBuffer[0], Is.EqualTo((byte)0x89));
                Assert.That(imageBuffer[1], Is.EqualTo((byte)0x50));
                Assert.That(imageBuffer[2], Is.EqualTo((byte)0x4E));
                Assert.That(imageBuffer[3], Is.EqualTo((byte)0x47));
                Assert.That(imageBuffer[4], Is.EqualTo((byte)0x0D));
                Assert.That(imageBuffer[5], Is.EqualTo((byte)0x0A));
                Assert.That(imageBuffer[6], Is.EqualTo((byte)0x1A));
                Assert.That(imageBuffer[7], Is.EqualTo((byte)0x0A));
                break;

            default:
                Assert.Fail($"Unexpected target format: {targetFormat}");
                break;
        }
    }
}
