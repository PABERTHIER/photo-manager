using Microsoft.Data.Sqlite;

namespace PhotoManager.Benchmarks.Persistence;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class SqliteConnectionFactoryConnectionStringBenchmarks
{
    private string _databasePath = null!;
    private string _cachedConnectionString = null!;

    [GlobalSetup]
    public void Setup()
    {
        _databasePath = Path.Combine(Path.GetTempPath(),
            "SqliteConnectionFactoryBenchmark_" + Guid.NewGuid().ToString("N"),
            "benchmark.db");
        _cachedConnectionString = BuildConnectionString(_databasePath);
    }

    [Benchmark(Baseline = true)]
    public string CreateConnectionWithBuilderPerOpen()
    {
        using (SqliteConnection connection = new(BuildConnectionString(_databasePath)))
        {
            return connection.ConnectionString;
        }
    }

    [Benchmark]
    public string CreateConnectionWithCachedConnectionString()
    {
        using (SqliteConnection connection = new(_cachedConnectionString))
        {
            return connection.ConnectionString;
        }
    }

    private static string BuildConnectionString(string databasePath)
    {
        SqliteConnectionStringBuilder builder = new()
        {
            DataSource = databasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Default,
            Pooling = true,
            ForeignKeys = true,
            DefaultTimeout = 30
        };

        return builder.ConnectionString;
    }
}
