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
            new() { ColumnName = "ImageRotation" },
            new() { ColumnName = "PixelWidth" },
            new() { ColumnName = "PixelHeight" },
            new() { ColumnName = "ThumbnailPixelWidth" },
            new() { ColumnName = "ThumbnailPixelHeight" },
            new() { ColumnName = "ThumbnailCreationDateTime" },
            new() { ColumnName = "Hash" },
            new() { ColumnName = "CorruptedMessage" },
            new() { ColumnName = "IsCorrupted" },
            new() { ColumnName = "RotatedMessage" },
            new() { ColumnName = "IsRotated" }
        ];
    }

    public static Asset ReadFunc(string[] values)
    {
        string[] formats = ["M/dd/yyyy HH:mm:ss"];

        if (!DateTime.TryParseExact(values[7], formats, CultureInfo.InvariantCulture, DateTimeStyles.None,
                out DateTime thumbnailCreationDateTime))
        {
            thumbnailCreationDateTime = DateTime.Now;
        }

        return new()
        {
            FolderId = new(values[0]),
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = values[1],
            ImageRotation = (Rotation)Enum.Parse(typeof(Rotation), values[2]),
            Pixel = new()
            {
                Asset = new() { Width = int.Parse(values[3]), Height = int.Parse(values[4]) },
                Thumbnail = new() { Width = int.Parse(values[5]), Height = int.Parse(values[6]) }
            },
            ThumbnailCreationDateTime = thumbnailCreationDateTime,
            Hash = values[8],
            Metadata = new()
            {
                Corrupted = new() { IsTrue = bool.Parse(values[10]), Message = values[9] },
                Rotated = new() { IsTrue = bool.Parse(values[12]), Message = values[11] }
            }
        };
    }

    public static object WriteFunc(Asset a, int i)
    {
        return i switch
        {
            0 => a.FolderId,
            1 => a.FileName,
            2 => a.ImageRotation,
            3 => a.Pixel.Asset.Width,
            4 => a.Pixel.Asset.Height,
            5 => a.Pixel.Thumbnail.Width,
            6 => a.Pixel.Thumbnail.Height,
            7 => a.ThumbnailCreationDateTime.ToString("M/dd/yyyy HH:mm:ss"),
            8 => a.Hash,
            9 => a.Metadata.Corrupted.Message!,
            10 => a.Metadata.Corrupted.IsTrue,
            11 => a.Metadata.Rotated.Message!,
            12 => a.Metadata.Rotated.IsTrue,
            _ => throw new ArgumentOutOfRangeException(nameof(i))
        };
    }
}
