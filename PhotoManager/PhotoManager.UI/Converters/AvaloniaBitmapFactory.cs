namespace PhotoManager.UI.Converters;

internal static class AvaloniaBitmapFactory
{
    public static Bitmap? Create(IImageData imageData, ImageEncodingFormat format)
    {
        byte[] bytes = imageData.ToByteArray(format);

        if (bytes.Length == 0)
        {
            return null;
        }

        using (MemoryStream stream = new(bytes))
        {
            return new(stream);
        }
    }
}
