namespace PhotoManager.Tests;

public static class AssetTestExtensions
{
    extension(Asset asset)
    {
        public Asset WithFolder(Folder folder)
        {
            return new()
            {
                FolderId = folder.Id,
                Folder = folder,
                FileName = asset.FileName,
                FileProperties = asset.FileProperties,
                Pixel = asset.Pixel,
                ImageRotation = asset.ImageRotation,
                Hash = asset.Hash,
                ImageData = asset.ImageData,
                ThumbnailCreationDateTime = asset.ThumbnailCreationDateTime,
                Metadata = asset.Metadata
            };
        }

        public Asset WithHash(string hash)
        {
            return new()
            {
                FolderId = asset.FolderId,
                Folder = asset.Folder,
                FileName = asset.FileName,
                FileProperties = asset.FileProperties,
                Pixel = asset.Pixel,
                ImageRotation = asset.ImageRotation,
                Hash = hash,
                ImageData = asset.ImageData,
                ThumbnailCreationDateTime = asset.ThumbnailCreationDateTime,
                Metadata = asset.Metadata
            };
        }
    }
}
