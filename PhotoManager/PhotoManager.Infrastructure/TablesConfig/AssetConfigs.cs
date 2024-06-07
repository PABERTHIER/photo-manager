using System.Globalization;

namespace PhotoManager.Infrastructure.TablesConfig;

public static class AssetConfigs
{
    public static ColumnProperties[] ConfigureDataTable()
    {
        return new ColumnProperties[]
        {
            new ColumnProperties { ColumnName = "FolderId" },
            new ColumnProperties { ColumnName = "FileName" },
            new ColumnProperties { ColumnName = "FileSize" },
            new ColumnProperties { ColumnName = "ImageRotation" },
            new ColumnProperties { ColumnName = "PixelWidth" },
            new ColumnProperties { ColumnName = "PixelHeight" },
            new ColumnProperties { ColumnName = "ThumbnailPixelWidth" },
            new ColumnProperties { ColumnName = "ThumbnailPixelHeight" },
            new ColumnProperties { ColumnName = "ThumbnailCreationDateTime" },
            new ColumnProperties { ColumnName = "Hash" },
            new ColumnProperties { ColumnName = "AssetCorruptedMessage" },
            new ColumnProperties { ColumnName = "IsAssetCorrupted" },
            new ColumnProperties { ColumnName = "AssetRotatedMessage" },
            new ColumnProperties { ColumnName = "IsAssetRotated" }
        };
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
            FileName = values[1],
            FileSize = long.Parse(values[2]),
            ImageRotation = (Rotation)Enum.Parse(typeof(Rotation), values[3]),
            PixelWidth = int.Parse(values[4]),
            PixelHeight = int.Parse(values[5]),
            ThumbnailPixelWidth = int.Parse(values[6]),
            ThumbnailPixelHeight = int.Parse(values[7]),
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
            4 => a.PixelWidth,
            5 => a.PixelHeight,
            6 => a.ThumbnailPixelWidth,
            7 => a.ThumbnailPixelHeight,
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
