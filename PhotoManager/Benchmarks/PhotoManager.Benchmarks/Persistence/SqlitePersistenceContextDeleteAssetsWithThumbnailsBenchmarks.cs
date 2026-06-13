using Microsoft.Data.Sqlite;
using PhotoManager.Persistence.Sqlite;

namespace PhotoManager.Benchmarks.Persistence;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
[ShortRunJob]
[InvocationCount(1)]
public class SqlitePersistenceContextDeleteAssetsWithThumbnailsBenchmarks
{
    private const string THUMBNAIL_DELETE_SQL = """
                                                DELETE FROM Thumbnails
                                                WHERE FolderId = $folderId AND FileName = $fileName;
                                                """;

    private const string ASSET_DELETE_SQL = """
                                            DELETE FROM Assets
                                            WHERE FolderId = $folderId AND FileName = $fileName;
                                            """;

    private AssetWithThumbnail[] _currentBatch = null!;
    private AssetWithThumbnail[] _optimizedBatch = null!;
    private string[] _currentFileNames = null!;
    private string[] _optimizedFileNames = null!;
    private SqliteConnectionFactory _currentFactory = null!;
    private SqliteConnectionFactory _optimizedFactory = null!;
    private SqlitePersistenceContext _currentContext = null!;
    private SqlitePersistenceContext _optimizedContext = null!;
    private Folder _currentFolder = null!;
    private Folder _optimizedFolder = null!;
    private string _tempDirectory = null!;

    [Params(50, 500)]
    public int AssetCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _tempDirectory = Path.Combine(
            Path.GetTempPath(), $"{nameof(SqlitePersistenceContextDeleteAssetsWithThumbnailsBenchmarks)}_{AssetCount}");
        Directory.CreateDirectory(_tempDirectory);

        _currentFactory = CreateFactory();
        _currentContext = CreateContext(_currentFactory);
        _currentContext.Initialize(Path.Combine(_tempDirectory, "Current"));
        _currentFolder = _currentContext.Folders.Insert($@"C:\Photos\Current{AssetCount}");
        _currentBatch = CreateBatch(_currentFolder, AssetCount);
        _currentFileNames = CreateFileNames(_currentBatch);

        _optimizedFactory = CreateFactory();
        _optimizedContext = CreateContext(_optimizedFactory);
        _optimizedContext.Initialize(Path.Combine(_tempDirectory, "Optimized"));
        _optimizedFolder = _optimizedContext.Folders.Insert($@"C:\Photos\Optimized{AssetCount}");
        _optimizedBatch = CreateBatch(_optimizedFolder, AssetCount);
        _optimizedFileNames = CreateFileNames(_optimizedBatch);
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

    [IterationSetup(Target = nameof(Original_ClearAndBindEachRow))]
    public void SetupOriginalIteration()
    {
        _currentContext.UpsertAssetsWithThumbnails(_currentBatch);
    }

    [IterationSetup(Target = nameof(Optimized_PreBoundParameters))]
    public void SetupOptimizedIteration()
    {
        _optimizedContext.UpsertAssetsWithThumbnails(_optimizedBatch);
    }

    [Benchmark(Baseline = true)]
    public void Original_ClearAndBindEachRow()
    {
        using (SqliteConnection connection = _currentFactory.Open())
        {
            using (SqliteTransaction transaction = connection.BeginTransaction())
            {
                using (SqliteCommand thumbnailCommand = connection.CreateCommand())
                {
                    thumbnailCommand.Transaction = transaction;
                    thumbnailCommand.CommandText = THUMBNAIL_DELETE_SQL;

                    using (SqliteCommand assetCommand = connection.CreateCommand())
                    {
                        assetCommand.Transaction = transaction;
                        assetCommand.CommandText = ASSET_DELETE_SQL;

                        for (int i = 0; i < _currentFileNames.Length; i++)
                        {
                            string fileName = _currentFileNames[i];

                            thumbnailCommand.Parameters.Clear();
                            thumbnailCommand.Parameters.AddWithValue("$folderId", _currentFolder.Id);
                            thumbnailCommand.Parameters.AddWithValue("$fileName", fileName);
                            thumbnailCommand.ExecuteNonQuery();

                            assetCommand.Parameters.Clear();
                            assetCommand.Parameters.AddWithValue("$folderId", _currentFolder.Id);
                            assetCommand.Parameters.AddWithValue("$fileName", fileName);
                            assetCommand.ExecuteNonQuery();
                        }
                    }
                }

                transaction.Commit();
            }
        }
    }

    [Benchmark]
    public void Optimized_PreBoundParameters()
    {
        using (SqliteConnection connection = _optimizedFactory.Open())
        {
            using (SqliteTransaction transaction = connection.BeginTransaction())
            {
                using (SqliteCommand thumbnailCommand = connection.CreateCommand())
                {
                    thumbnailCommand.Transaction = transaction;
                    thumbnailCommand.CommandText = THUMBNAIL_DELETE_SQL;
                    DeleteParameters thumbnailParameters = CreateDeleteParameters(thumbnailCommand);
                    thumbnailParameters.FolderId.Value = _optimizedFolder.Id;

                    using (SqliteCommand assetCommand = connection.CreateCommand())
                    {
                        assetCommand.Transaction = transaction;
                        assetCommand.CommandText = ASSET_DELETE_SQL;
                        DeleteParameters assetParameters = CreateDeleteParameters(assetCommand);
                        assetParameters.FolderId.Value = _optimizedFolder.Id;

                        for (int i = 0; i < _optimizedFileNames.Length; i++)
                        {
                            string fileName = _optimizedFileNames[i];

                            thumbnailParameters.FileName.Value = fileName;
                            thumbnailCommand.ExecuteNonQuery();

                            assetParameters.FileName.Value = fileName;
                            assetCommand.ExecuteNonQuery();
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
            Asset asset = new()
            {
                FolderId = folder.Id,
                Folder = folder,
                FileName = $"Image_{i}.jpg",
                Pixel = new()
                {
                    Asset = new() { Width = 1920, Height = 1080 },
                    Thumbnail = new() { Width = 200, Height = 150 }
                },
                ImageRotation = ImageRotation.Rotate0,
                Hash = $"hash-{i}",
                ThumbnailCreationDateTime = DateTime.UnixEpoch.AddSeconds(i),
                Metadata = new()
            };

            batch[i] = new(asset, [1, 2, 3, 4, 5, 6, 7, 8]);
        }

        return batch;
    }

    private static string[] CreateFileNames(AssetWithThumbnail[] batch)
    {
        string[] fileNames = new string[batch.Length];

        for (int i = 0; i < batch.Length; i++)
        {
            fileNames[i] = batch[i].Asset.FileName;
        }

        return fileNames;
    }

    private static DeleteParameters CreateDeleteParameters(SqliteCommand command)
    {
        return new()
        {
            FolderId = command.Parameters.Add("$folderId", SqliteType.Text),
            FileName = command.Parameters.Add("$fileName", SqliteType.Text)
        };
    }

    private sealed class DeleteParameters
    {
        public required SqliteParameter FolderId { get; init; }
        public required SqliteParameter FileName { get; init; }
    }
}
