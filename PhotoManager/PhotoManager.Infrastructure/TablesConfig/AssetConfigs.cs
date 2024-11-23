using System.Globalization;

namespace PhotoManager.Infrastructure.TablesConfig;

public static class AssetConfigs
{
    public static ColumnProperties[] ConfigureDataTable()
    {
        return
        [
            new() { ColumnName = "FolderId" },
            new() { ColumnName = "FileName" },
            new() { ColumnName = "FileSize" },
            new() { ColumnName = "ImageRotation" },
            new() { ColumnName = "PixelWidth" },
            new() { ColumnName = "PixelHeight" },
            new() { ColumnName = "ThumbnailPixelWidth" },
            new() { ColumnName = "ThumbnailPixelHeight" },
            new() { ColumnName = "ThumbnailCreationDateTime" },
            new() { ColumnName = "Hash" },
            new() { ColumnName = "AssetCorruptedMessage" },
            new() { ColumnName = "IsAssetCorrupted" },
            new() { ColumnName = "AssetRotatedMessage" },
            new() { ColumnName = "IsAssetRotated" }
        ];
    }

    public static Asset ReadFunc(string[] values)
    {
        string[] formats = ["M/dd/yyyy HH:mm:ss"];

        if (!DateTime.TryParseExact(values[8], formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime thumbnailCreationDateTime))
        {
            thumbnailCreationDateTime = DateTime.Now;
        }

        return new Asset
        {
            FolderId = new Guid(values[0]),
            Folder = new() { Path = "" }, // Initialised later
            FileName = values[1],
            FileSize = long.Parse(values[2]),
            ImageRotation = (Rotation)Enum.Parse(typeof(Rotation), values[3]),
            Pixel = new()
            {
                Asset = new() { Width = int.Parse(values[4]), Height = int.Parse(values[5]) },
                Thumbnail = new() { Width = int.Parse(values[6]), Height = int.Parse(values[7]) }
            },
            ThumbnailCreationDateTime = thumbnailCreationDateTime,
            Hash = values[9],
            AssetCorruptedMessage = values[10],
            IsAssetCorrupted = bool.Parse(values[11]),
            AssetRotatedMessage = values[12],
            IsAssetRotated = bool.Parse(values[13])
        };
    }

    public static object WriteFunc(Asset a, int i)
    {
        return i switch
        {
            0 => a.FolderId,
            1 => a.FileName,
            2 => a.FileSize,
            3 => a.ImageRotation,
            4 => a.Pixel.Asset.Width,
            5 => a.Pixel.Asset.Height,
            6 => a.Pixel.Thumbnail.Width,
            7 => a.Pixel.Thumbnail.Height,
            8 => a.ThumbnailCreationDateTime.ToString("M/dd/yyyy HH:mm:ss"),
            9 => a.Hash,
            10 => a.AssetCorruptedMessage!,
            11 => a.IsAssetCorrupted,
            12 => a.AssetRotatedMessage!,
            13 => a.IsAssetRotated,
            _ => throw new ArgumentOutOfRangeException(nameof(i))
        };
    }
}
