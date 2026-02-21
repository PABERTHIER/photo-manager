namespace PhotoManager.Domain.Interfaces;

public interface IImageMetadataService
{
    ushort GetExifOrientation(byte[] buffer, ushort defaultExifOrientation, ushort corruptedImageOrientation);
    ushort GetHeicExifOrientation(byte[] buffer, ushort corruptedImageOrientation);
    Rotation GetImageRotation(ushort exifOrientation);
    void UpdateAssetsFileProperties(List<Asset> assets);
    void UpdateAssetFileProperties(Asset asset);
}
