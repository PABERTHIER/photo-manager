using Microsoft.Data.Sqlite;
using PhotoManager.Persistence.Sqlite;

namespace PhotoManager.Benchmarks.Persistence;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
[ShortRunJob]
public class SqlitePersistenceContextUpsertAssetsWithThumbnailsBenchmarks
{
    private const string ASSET_UPSERT_SQL = """
                                           INSERT INTO Assets (FolderId, FileName, ImageRotation, PixelWidth,
                                               PixelHeight, ThumbnailPixelWidth, ThumbnailPixelHeight,
                                               ThumbnailCreationDateTime, Hash, CorruptedMessage, IsCorrupted,
                                               RotatedMessage, IsRotated)
                                           VALUES ($folderId, $fileName, $imageRotation, $pixelWidth, $pixelHeight,
                                               $thumbnailPixelWidth, $thumbnailPixelHeight,
                                               $thumbnailCreationDateTime, $hash, $corruptedMessage, $isCorrupted,
                                               $rotatedMessage, $isRotated)
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

    private const string THUMBNAIL_UPSERT_SQL = """
                                               INSERT INTO Thumbnails (FolderId, FileName, Data)
                                               VALUES ($folderId, $fileName, $data)
                                               ON CONFLICT(FolderId, FileName) DO UPDATE SET Data = excluded.Data;
                                               """;

    private AssetWithThumbnail[] _currentBatch = null!;
    private AssetWithThumbnail[] _optimizedBatch = null!;
    private SqlitePersistenceContext _currentContext = null!;
    private SqliteConnectionFactory _optimizedFactory = null!;
    private SqlitePersistenceContext _optimizedContext = null!;
    private string _tempDirectory = null!;

    [Params(50, 500)]
    public int AssetCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _tempDirectory = Path.Combine(
            Path.GetTempPath(), $"{nameof(SqlitePersistenceContextUpsertAssetsWithThumbnailsBenchmarks)}_{AssetCount}");
        Directory.CreateDirectory(_tempDirectory);

        SqliteConnectionFactory currentFactory = CreateFactory();
        _currentContext = CreateContext(currentFactory);
        _currentContext.Initialize(Path.Combine(_tempDirectory, "Current"));
        Folder currentFolder = _currentContext.Folders.Insert($@"C:\Photos\Current{AssetCount}");
        _currentBatch = CreateBatch(currentFolder, AssetCount);

        _optimizedFactory = CreateFactory();
        _optimizedContext = CreateContext(_optimizedFactory);
        _optimizedContext.Initialize(Path.Combine(_tempDirectory, "Optimized"));
        Folder optimizedFolder = _optimizedContext.Folders.Insert($@"C:\Photos\Optimized{AssetCount}");
        _optimizedBatch = CreateBatch(optimizedFolder, AssetCount);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _currentContext.Dispose();
        _optimizedContext.Dispose();
        SqliteConnection.ClearAllPools();

        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }

    [Benchmark(Baseline = true)]
    public void Current_ClearAndBindEachRow()
    {
        _currentContext.UpsertAssetsWithThumbnails(_currentBatch);
    }

    [Benchmark]
    public void Optimized_PreBoundParameters()
    {
        using (SqliteConnection connection = _optimizedFactory.Open())
        {
            using (SqliteTransaction transaction = connection.BeginTransaction())
            {
                using (SqliteCommand assetCommand = connection.CreateCommand())
                {
                    assetCommand.Transaction = transaction;
                    assetCommand.CommandText = ASSET_UPSERT_SQL;
                    AssetParameters assetParameters = CreateAssetParameters(assetCommand);

                    using (SqliteCommand thumbnailCommand = connection.CreateCommand())
                    {
                        thumbnailCommand.Transaction = transaction;
                        thumbnailCommand.CommandText = THUMBNAIL_UPSERT_SQL;
                        ThumbnailParameters thumbnailParameters = CreateThumbnailParameters(thumbnailCommand);

                        for (int i = 0; i < _optimizedBatch.Length; i++)
                        {
                            AssetWithThumbnail assetWithThumbnail = _optimizedBatch[i];
                            BindAsset(assetParameters, assetWithThumbnail.Asset);
                            assetCommand.ExecuteNonQuery();

                            BindThumbnail(thumbnailParameters, assetWithThumbnail);
                            thumbnailCommand.ExecuteNonQuery();
                        }
                    }
                }

                transaction.Commit();
            }
        }
    }

    private static SqliteConnectionFactory CreateFactory()
    {
        return new(NullLogger<SqliteConnectionFactory>.Instance);
    }

    private static SqlitePersistenceContext CreateContext(SqliteConnectionFactory factory)
    {
        SqliteBackupService backupService = new(factory);
        return new(factory, backupService, NullLogger<SqlitePersistenceContext>.Instance);
    }

    private static AssetWithThumbnail[] CreateBatch(Folder folder, int count)
    {
        AssetWithThumbnail[] batch = new AssetWithThumbnail[count];

        for (int i = 0; i < count; i++)
        {
            Asset asset = AssetBenchmarkBuilder.Create()
                .WithFolder(folder)
                .WithFileName($"Image_{i}.jpg")
                .WithPixels(1920, 1080, 200, 150)
                .WithFileSize(0)
                .WithHash($"hash-{i}")
                .WithThumbnailCreationDateTime(DateTime.UnixEpoch.AddSeconds(i))
                .WithImageRotation(ImageRotation.Rotate0)
                .WithCorrupted(false, null)
                .WithRotated(false, null)
                .Build();

            batch[i] = new(asset, [1, 2, 3, 4, 5, 6, 7, 8]);
        }

        return batch;
    }

    private static AssetParameters CreateAssetParameters(SqliteCommand command)
    {
        return new()
        {
            FolderId = command.Parameters.Add("$folderId", SqliteType.Text),
            FileName = command.Parameters.Add("$fileName", SqliteType.Text),
            ImageRotation = command.Parameters.Add("$imageRotation", SqliteType.Integer),
            PixelWidth = command.Parameters.Add("$pixelWidth", SqliteType.Integer),
            PixelHeight = command.Parameters.Add("$pixelHeight", SqliteType.Integer),
            ThumbnailPixelWidth = command.Parameters.Add("$thumbnailPixelWidth", SqliteType.Integer),
            ThumbnailPixelHeight = command.Parameters.Add("$thumbnailPixelHeight", SqliteType.Integer),
            ThumbnailCreationDateTime = command.Parameters.Add("$thumbnailCreationDateTime", SqliteType.Integer),
            Hash = command.Parameters.Add("$hash", SqliteType.Text),
            CorruptedMessage = command.Parameters.Add("$corruptedMessage", SqliteType.Text),
            IsCorrupted = command.Parameters.Add("$isCorrupted", SqliteType.Integer),
            RotatedMessage = command.Parameters.Add("$rotatedMessage", SqliteType.Text),
            IsRotated = command.Parameters.Add("$isRotated", SqliteType.Integer)
        };
    }

    private static ThumbnailParameters CreateThumbnailParameters(SqliteCommand command)
    {
        return new()
        {
            FolderId = command.Parameters.Add("$folderId", SqliteType.Text),
            FileName = command.Parameters.Add("$fileName", SqliteType.Text),
            Data = command.Parameters.Add("$data", SqliteType.Blob)
        };
    }

    private static void BindAsset(AssetParameters parameters, Asset asset)
    {
        parameters.FolderId.Value = asset.FolderId;
        parameters.FileName.Value = asset.FileName;
        parameters.ImageRotation.Value = (int)asset.ImageRotation;
        parameters.PixelWidth.Value = asset.Pixel.Asset.Width;
        parameters.PixelHeight.Value = asset.Pixel.Asset.Height;
        parameters.ThumbnailPixelWidth.Value = asset.Pixel.Thumbnail.Width;
        parameters.ThumbnailPixelHeight.Value = asset.Pixel.Thumbnail.Height;
        parameters.ThumbnailCreationDateTime.Value = asset.ThumbnailCreationDateTime.Ticks;
        parameters.Hash.Value = asset.Hash;
        parameters.CorruptedMessage.Value = (object?)asset.Metadata.Corrupted.Message ?? DBNull.Value;
        parameters.IsCorrupted.Value = asset.Metadata.Corrupted.IsTrue ? 1 : 0;
        parameters.RotatedMessage.Value = (object?)asset.Metadata.Rotated.Message ?? DBNull.Value;
        parameters.IsRotated.Value = asset.Metadata.Rotated.IsTrue ? 1 : 0;
    }

    private static void BindThumbnail(ThumbnailParameters parameters, AssetWithThumbnail assetWithThumbnail)
    {
        parameters.FolderId.Value = assetWithThumbnail.Asset.FolderId;
        parameters.FileName.Value = assetWithThumbnail.Asset.FileName;
        parameters.Data.Value = assetWithThumbnail.ThumbnailData;
    }

    private sealed class AssetParameters
    {
        public required SqliteParameter FolderId { get; init; }
        public required SqliteParameter FileName { get; init; }
        public required SqliteParameter ImageRotation { get; init; }
        public required SqliteParameter PixelWidth { get; init; }
        public required SqliteParameter PixelHeight { get; init; }
        public required SqliteParameter ThumbnailPixelWidth { get; init; }
        public required SqliteParameter ThumbnailPixelHeight { get; init; }
        public required SqliteParameter ThumbnailCreationDateTime { get; init; }
        public required SqliteParameter Hash { get; init; }
        public required SqliteParameter CorruptedMessage { get; init; }
        public required SqliteParameter IsCorrupted { get; init; }
        public required SqliteParameter RotatedMessage { get; init; }
        public required SqliteParameter IsRotated { get; init; }
    }

    private sealed class ThumbnailParameters
    {
        public required SqliteParameter FolderId { get; init; }
        public required SqliteParameter FileName { get; init; }
        public required SqliteParameter Data { get; init; }
    }
}
