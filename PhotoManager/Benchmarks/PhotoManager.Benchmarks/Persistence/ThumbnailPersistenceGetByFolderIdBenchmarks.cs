using Microsoft.Data.Sqlite;
using PhotoManager.Persistence.Sqlite;

namespace PhotoManager.Benchmarks.Persistence;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
[ShortRunJob]
public class ThumbnailPersistenceGetByFolderIdBenchmarks
{
    private SqlitePersistenceContext _context = null!;
    private SqliteConnectionFactory _factory = null!;
    private Folder _folder = null!;
    private string _tempDirectory = null!;

    [Params(100, 1000)]
    public int ThumbnailCount { get; set; }

    [Params(10_240)]
    public int ThumbnailBytes { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _tempDirectory = Path.Combine(
            Path.GetTempPath(),
            $"{nameof(ThumbnailPersistenceGetByFolderIdBenchmarks)}_{ThumbnailCount}_{ThumbnailBytes}");
        Directory.CreateDirectory(_tempDirectory);

        _factory = new(NullLogger<SqliteConnectionFactory>.Instance);
        SqliteBackupService backupService = new(_factory);
        _context = new(_factory, backupService, NullLogger<SqlitePersistenceContext>.Instance);
        _context.Initialize(_tempDirectory);
        _folder = _context.Folders.Insert($@"C:\Photos\Thumbnails{ThumbnailCount}");

        Dictionary<string, byte[]> thumbnails = new(ThumbnailCount, StringComparer.Ordinal);
        for (int i = 0; i < ThumbnailCount; i++)
        {
            byte[] thumbnail = new byte[ThumbnailBytes];
            thumbnail.AsSpan().Fill((byte)(i % byte.MaxValue));
            thumbnails[$"Image_{i}.jpg"] = thumbnail;
        }

        _context.Thumbnails.ReplaceForFolder(_folder.Id, thumbnails);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _context.Dispose();
        SqliteConnection.ClearAllPools();

        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }

    [Benchmark(Baseline = true)]
    public int Current_StreamCopy()
    {
        Dictionary<string, byte[]> thumbnails = _context.Thumbnails.GetByFolderId(_folder.Id);

        return CountBytes(thumbnails);
    }

    [Benchmark]
    public int Optimized_GetBytesExactBuffer()
    {
        Dictionary<string, byte[]> thumbnails = GetByFolderIdWithGetBytes(_factory, _folder.Id);

        return CountBytes(thumbnails);
    }

    [Benchmark]
    public int Optimized_GetFieldValue()
    {
        Dictionary<string, byte[]> thumbnails = GetByFolderIdWithGetFieldValue(_factory, _folder.Id);

        return CountBytes(thumbnails);
    }

    private static Dictionary<string, byte[]> GetByFolderIdWithGetBytes(SqliteConnectionFactory factory, Guid folderId)
    {
        using (SqliteConnection connection = factory.Open())
        {
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT FileName, Data FROM Thumbnails WHERE FolderId = $folderId;";
                command.Parameters.AddWithValue("$folderId", folderId);

                Dictionary<string, byte[]> result = new(StringComparer.Ordinal);

                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string fileName = reader.GetString(0);
                        long blobLength = reader.GetBytes(1, 0, null, 0, 0);
                        byte[] data = new byte[checked((int)blobLength)];
                        reader.GetBytes(1, 0, data, 0, data.Length);
                        result[fileName] = data;
                    }

                    return result;
                }
            }
        }
    }

    private static Dictionary<string, byte[]> GetByFolderIdWithGetFieldValue(SqliteConnectionFactory factory,
        Guid folderId)
    {
        using (SqliteConnection connection = factory.Open())
        {
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT FileName, Data FROM Thumbnails WHERE FolderId = $folderId;";
                command.Parameters.AddWithValue("$folderId", folderId);

                Dictionary<string, byte[]> result = new(StringComparer.Ordinal);

                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result[reader.GetString(0)] = reader.GetFieldValue<byte[]>(1);
                    }

                    return result;
                }
            }
        }
    }

    private static int CountBytes(Dictionary<string, byte[]> thumbnails)
    {
        int totalBytes = 0;

        foreach (KeyValuePair<string, byte[]> thumbnail in thumbnails)
        {
            totalBytes += thumbnail.Value.Length;
        }

        return totalBytes;
    }
}
