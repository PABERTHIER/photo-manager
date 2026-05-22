using PhotoManager.Domain.Interfaces.Persistence.Repositories;
using PhotoManager.Persistence.Sqlite;

namespace PhotoManager.Persistence.Repositories;

internal sealed class AssetPersistence(ISqliteConnectionFactory connectionFactory) : IAssetPersistence
{
    private const string UPSERT_SQL = """
                                      INSERT INTO Assets (FolderId, FileName, ImageRotation, PixelWidth, PixelHeight,
                                          ThumbnailPixelWidth, ThumbnailPixelHeight, ThumbnailCreationDateTime, Hash,
                                          CorruptedMessage, IsCorrupted, RotatedMessage, IsRotated)
                                      VALUES ($folderId, $fileName, $imageRotation, $pixelWidth, $pixelHeight,
                                          $thumbnailPixelWidth, $thumbnailPixelHeight, $thumbnailCreationDateTime, $hash,
                                          $corruptedMessage, $isCorrupted, $rotatedMessage, $isRotated)
                                      ON CONFLICT(FolderId, FileName) DO UPDATE SET
                                          ImageRotation             = excluded.ImageRotation,
                                          PixelWidth                = excluded.PixelWidth,
                                          PixelHeight               = excluded.PixelHeight,
                                          ThumbnailPixelWidth       = excluded.ThumbnailPixelWidth,
                                          ThumbnailPixelHeight      = excluded.ThumbnailPixelHeight,
                                          ThumbnailCreationDateTime = excluded.ThumbnailCreationDateTime,
                                          Hash                      = excluded.Hash,
                                          CorruptedMessage          = excluded.CorruptedMessage,
                                          IsCorrupted               = excluded.IsCorrupted,
                                          RotatedMessage            = excluded.RotatedMessage,
                                          IsRotated                 = excluded.IsRotated;
                                      """;

    private const string SELECT_COLUMNS = """
                                          SELECT FolderId, FileName, ImageRotation, PixelWidth, PixelHeight,
                                          ThumbnailPixelWidth, ThumbnailPixelHeight, ThumbnailCreationDateTime, Hash,
                                          CorruptedMessage, IsCorrupted, RotatedMessage, IsRotated FROM Assets
                                          """;

    public void Upsert(Asset asset)
    {
        using (SqliteConnection connection = connectionFactory.Open())
        {
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = UPSERT_SQL;
                BindAsset(command, asset);
                command.ExecuteNonQuery();
            }
        }
    }

    public void UpsertMany(IReadOnlyList<Asset> assets)
    {
        if (assets.Count == 0)
        {
            return;
        }

        using (SqliteConnection connection = connectionFactory.Open())
        {
            using (SqliteTransaction transaction = connection.BeginTransaction())
            {
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.Transaction = transaction;
                    command.CommandText = UPSERT_SQL;

                    // Pre-create parameters once and rebind values for each row to avoid allocations.
                    SqliteParameter folderId = command.Parameters.Add("$folderId", SqliteType.Text);
                    SqliteParameter fileName = command.Parameters.Add("$fileName", SqliteType.Text);
                    SqliteParameter imageRotation = command.Parameters.Add("$imageRotation", SqliteType.Integer);
                    SqliteParameter pixelWidth = command.Parameters.Add("$pixelWidth", SqliteType.Integer);
                    SqliteParameter pixelHeight = command.Parameters.Add("$pixelHeight", SqliteType.Integer);
                    SqliteParameter thumbWidth = command.Parameters.Add("$thumbnailPixelWidth", SqliteType.Integer);
                    SqliteParameter thumbHeight = command.Parameters.Add("$thumbnailPixelHeight", SqliteType.Integer);
                    SqliteParameter thumbCreated =
                        command.Parameters.Add("$thumbnailCreationDateTime", SqliteType.Integer);
                    SqliteParameter hash = command.Parameters.Add("$hash", SqliteType.Text);
                    SqliteParameter corruptedMessage = command.Parameters.Add("$corruptedMessage", SqliteType.Text);
                    SqliteParameter isCorrupted = command.Parameters.Add("$isCorrupted", SqliteType.Integer);
                    SqliteParameter rotatedMessage = command.Parameters.Add("$rotatedMessage", SqliteType.Text);
                    SqliteParameter isRotated = command.Parameters.Add("$isRotated", SqliteType.Integer);

                    for (int i = 0; i < assets.Count; i++)
                    {
                        Asset asset = assets[i];
                        folderId.Value = asset.FolderId;
                        fileName.Value = asset.FileName;
                        imageRotation.Value = (int)asset.ImageRotation;
                        pixelWidth.Value = asset.Pixel.Asset.Width;
                        pixelHeight.Value = asset.Pixel.Asset.Height;
                        thumbWidth.Value = asset.Pixel.Thumbnail.Width;
                        thumbHeight.Value = asset.Pixel.Thumbnail.Height;
                        thumbCreated.Value = asset.ThumbnailCreationDateTime.Ticks;
                        hash.Value = asset.Hash;
                        corruptedMessage.Value = (object?)asset.Metadata.Corrupted.Message ?? DBNull.Value;
                        isCorrupted.Value = asset.Metadata.Corrupted.IsTrue ? 1 : 0;
                        rotatedMessage.Value = (object?)asset.Metadata.Rotated.Message ?? DBNull.Value;
                        isRotated.Value = asset.Metadata.Rotated.IsTrue ? 1 : 0;

                        command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
            }
        }
    }

    public bool Delete(Guid folderId, string fileName)
    {
        using (SqliteConnection connection = connectionFactory.Open())
        {
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = "DELETE FROM Assets WHERE FolderId = $folderId AND FileName = $fileName;";
                command.Parameters.AddWithValue("$folderId", folderId);
                command.Parameters.AddWithValue("$fileName", fileName);

                return command.ExecuteNonQuery() > 0;
            }
        }
    }

    public int DeleteByFolderId(Guid folderId)
    {
        using (SqliteConnection connection = connectionFactory.Open())
        {
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = "DELETE FROM Assets WHERE FolderId = $folderId;";
                command.Parameters.AddWithValue("$folderId", folderId);

                return command.ExecuteNonQuery();
            }
        }
    }

    public Asset? Get(Guid folderId, string fileName)
    {
        using (SqliteConnection connection = connectionFactory.Open())
        {
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = SELECT_COLUMNS + " WHERE FolderId = $folderId AND FileName = $fileName LIMIT 1;";
                command.Parameters.AddWithValue("$folderId", folderId);
                command.Parameters.AddWithValue("$fileName", fileName);

                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    return reader.Read() ? Map(reader) : null;
                }
            }
        }
    }

    public IReadOnlyList<Asset> GetByFolderId(Guid folderId)
    {
        using (SqliteConnection connection = connectionFactory.Open())
        {
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = SELECT_COLUMNS + " WHERE FolderId = $folderId;";
                command.Parameters.AddWithValue("$folderId", folderId);

                return ReadAll(command);
            }
        }
    }

    public IReadOnlyList<Asset> GetAll()
    {
        using (SqliteConnection connection = connectionFactory.Open())
        {
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = SELECT_COLUMNS + ";";

                return ReadAll(command);
            }
        }
    }

    public IReadOnlyList<Asset> GetByHash(string hash)
    {
        using (SqliteConnection connection = connectionFactory.Open())
        {
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = SELECT_COLUMNS + " WHERE Hash = $hash;";
                command.Parameters.AddWithValue("$hash", hash);

                return ReadAll(command);
            }
        }
    }

    public int Count()
    {
        using (SqliteConnection connection = connectionFactory.Open())
        {
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT COUNT(*) FROM Assets;";

                return Convert.ToInt32(command.ExecuteScalar());
            }
        }
    }

    private static List<Asset> ReadAll(SqliteCommand cmd)
    {
        List<Asset> result = [];

        using (SqliteDataReader reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                result.Add(Map(reader));
            }

            return result;
        }
    }

    private static void BindAsset(SqliteCommand command, Asset asset)
    {
        command.Parameters.AddWithValue("$folderId", asset.FolderId);
        command.Parameters.AddWithValue("$fileName", asset.FileName);
        command.Parameters.AddWithValue("$imageRotation", (int)asset.ImageRotation);
        command.Parameters.AddWithValue("$pixelWidth", asset.Pixel.Asset.Width);
        command.Parameters.AddWithValue("$pixelHeight", asset.Pixel.Asset.Height);
        command.Parameters.AddWithValue("$thumbnailPixelWidth", asset.Pixel.Thumbnail.Width);
        command.Parameters.AddWithValue("$thumbnailPixelHeight", asset.Pixel.Thumbnail.Height);
        command.Parameters.AddWithValue("$thumbnailCreationDateTime", asset.ThumbnailCreationDateTime.Ticks);
        command.Parameters.AddWithValue("$hash", asset.Hash);
        command.Parameters.AddWithValue("$corruptedMessage", (object?)asset.Metadata.Corrupted.Message ?? DBNull.Value);
        command.Parameters.AddWithValue("$isCorrupted", asset.Metadata.Corrupted.IsTrue ? 1 : 0);
        command.Parameters.AddWithValue("$rotatedMessage", (object?)asset.Metadata.Rotated.Message ?? DBNull.Value);
        command.Parameters.AddWithValue("$isRotated", asset.Metadata.Rotated.IsTrue ? 1 : 0);
    }

    private static Asset Map(SqliteDataReader reader)
    {
        Guid folderId = Guid.Parse(reader.GetString(0));

        return new Asset
        {
            FolderId = folderId,
            Folder = new Folder { Id = Guid.Empty, Path = string.Empty }, // hydrated by repository
            FileName = reader.GetString(1),
            ImageRotation = MapRotationFromDb(reader.GetInt32(2)),
            Pixel = new Pixel
            {
                Asset = new Dimensions
                {
                    Width = reader.GetInt32(3),
                    Height = reader.GetInt32(4)
                },
                Thumbnail = new Dimensions
                {
                    Width = reader.GetInt32(5),
                    Height = reader.GetInt32(6)
                }
            },
            ThumbnailCreationDateTime = new DateTime(reader.GetInt64(7), DateTimeKind.Unspecified),
            Hash = reader.GetString(8),
            Metadata = new Metadata
            {
                Corrupted = new Flag
                {
                    Message = reader.IsDBNull(9) ? null : reader.GetString(9),
                    IsTrue = reader.GetInt32(10) != 0
                },
                Rotated = new Flag
                {
                    Message = reader.IsDBNull(11) ? null : reader.GetString(11),
                    IsTrue = reader.GetInt32(12) != 0
                }
            }
        };
    }

    // TODO: To remove when v3 is out, would need to clean up DB
    // Maps DB integer to ImageRotation, supporting both legacy (0-3) and new (0,90,180,270) formats
    private static ImageRotation MapRotationFromDb(int value)
    {
        return value switch
        {
            0 => ImageRotation.Rotate0,
            1 or 90 => ImageRotation.Rotate90,
            2 or 180 => ImageRotation.Rotate180,
            3 or 270 => ImageRotation.Rotate270,
            _ => ImageRotation.Rotate0
        };
    }
}
