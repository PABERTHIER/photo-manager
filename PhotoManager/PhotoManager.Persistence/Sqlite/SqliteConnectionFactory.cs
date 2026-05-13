namespace PhotoManager.Persistence.Sqlite;

/// <summary>
/// Default factory. Builds a connection string targeting <see cref="DatabasePath"/>
/// and applies all required PRAGMAs every time a connection is opened
/// (PRAGMAs in SQLite are per-connection except a few that persist).
/// </summary>
public sealed class SqliteConnectionFactory : ISqliteConnectionFactory
{
    private const int BUSY_TIMEOUT_MS = 5000;

    public string DatabasePath { get; private set; } = string.Empty;

    public void Initialize(string databasePath)
    {
        DatabasePath = databasePath;
    }

    public SqliteConnection Open()
    {
        if (string.IsNullOrEmpty(DatabasePath))
        {
            throw new InvalidOperationException(
                $"The Db has not been initialized properly, the directory {DatabasePath} does not exist.");
        }

        SqliteConnectionStringBuilder builder = new()
        {
            DataSource = DatabasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Default,
            Pooling = true,
            ForeignKeys = true,
            DefaultTimeout = 30
        };

        SqliteConnection connection = new(builder.ConnectionString);
        connection.Open();

        using (SqliteCommand command = connection.CreateCommand())
        {
            command.CommandText = $"""
                                   PRAGMA journal_mode=WAL;
                                   PRAGMA synchronous=NORMAL;
                                   PRAGMA busy_timeout={BUSY_TIMEOUT_MS};
                                   PRAGMA temp_store=MEMORY;
                                   PRAGMA foreign_keys=ON;
                                   """;
            command.ExecuteNonQuery();

            return connection;
        }
    }
}
