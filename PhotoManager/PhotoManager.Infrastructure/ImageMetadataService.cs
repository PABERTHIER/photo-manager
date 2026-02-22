namespace PhotoManager.Infrastructure;

public class ImageMetadataService(IFileOperationsService fileOperationsService) : IImageMetadataService
{
    public ushort GetExifOrientation(byte[] buffer, ushort defaultExifOrientation, ushort corruptedImageOrientation)
    {
        return ExifHelper.GetExifOrientation(buffer, defaultExifOrientation, corruptedImageOrientation);
    }

    public ushort GetHeicExifOrientation(byte[] buffer, ushort corruptedImageOrientation)
    {
        return ExifHelper.GetHeicExifOrientation(buffer, corruptedImageOrientation);
    }

    public Rotation GetImageRotation(ushort exifOrientation)
    {
        return ExifHelper.GetImageRotation(exifOrientation);
    }

    public void UpdateAssetsFileProperties(List<Asset> assets)
    {
        for (int i = 0; i < assets.Count; i++)
        {
            UpdateAssetFileProperties(assets[i]);
        }
    }

    public void UpdateAssetFileProperties(Asset asset)
    {
        if (!fileOperationsService.FileExists(asset.FullPath))
        {
            return;
        }

        FileInfo info = new(asset.FullPath);

        asset.FileProperties = new()
        {
            Size = info.Length,
            Creation = info.CreationTime,
            Modification = info.LastWriteTime
        };
    }
}
