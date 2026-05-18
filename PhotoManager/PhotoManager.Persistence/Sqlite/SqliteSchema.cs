namespace PhotoManager.Persistence.Sqlite;

internal static class SqliteSchema
{
    /// <summary>
    /// Current logical schema version. Stored in <c>PRAGMA user_version</c>
    /// so future migrations can branch on it.
    /// </summary>
    private const int SCHEMA_VERSION = 1;

    private const string CREATE_SCRIPT = """
                                         CREATE TABLE IF NOT EXISTS Folders (
                                             Id   TEXT PRIMARY KEY NOT NULL,
                                             Path TEXT NOT NULL
                                         );
                                         CREATE INDEX IF NOT EXISTS IX_Folders_Path ON Folders(Path);

                                         CREATE TABLE IF NOT EXISTS Assets (
                                             FolderId                  TEXT    NOT NULL,
                                             FileName                  TEXT    NOT NULL,
                                             ImageRotation             INTEGER NOT NULL,
                                             PixelWidth                INTEGER NOT NULL,
                                             PixelHeight               INTEGER NOT NULL,
                                             ThumbnailPixelWidth       INTEGER NOT NULL,
                                             ThumbnailPixelHeight      INTEGER NOT NULL,
                                             ThumbnailCreationDateTime INTEGER NOT NULL,
                                             Hash                      TEXT    NOT NULL,
                                             CorruptedMessage          TEXT    NULL,
                                             IsCorrupted               INTEGER NOT NULL,
                                             RotatedMessage            TEXT    NULL,
                                             IsRotated                 INTEGER NOT NULL,
                                             PRIMARY KEY (FolderId, FileName)
                                         );
                                         CREATE INDEX IF NOT EXISTS IX_Assets_FolderId ON Assets(FolderId);
                                         CREATE INDEX IF NOT EXISTS IX_Assets_Hash     ON Assets(Hash);

                                         CREATE TABLE IF NOT EXISTS Thumbnails (
                                             FolderId TEXT NOT NULL,
                                             FileName TEXT NOT NULL,
                                             Data     BLOB NOT NULL,
                                             PRIMARY KEY (FolderId, FileName)
                                         );
                                         CREATE INDEX IF NOT EXISTS IX_Thumbnails_FolderId ON Thumbnails(FolderId);

                                         CREATE TABLE IF NOT EXISTS RecentPaths (
                                             Position INTEGER PRIMARY KEY,
                                             Path     TEXT    NOT NULL
                                         );

                                         CREATE TABLE IF NOT EXISTS SyncDefinitions (
                                             Position                INTEGER PRIMARY KEY,
                                             SourceDirectory         TEXT    NOT NULL,
                                             DestinationDirectory    TEXT    NOT NULL,
                                             IncludeSubFolders       INTEGER NOT NULL,
                                             DeleteAssetsNotInSource INTEGER NOT NULL
                                         );
                                         """;

    public static void EnsureCreated(SqliteConnection connection)
    {
        using (SqliteCommand command = connection.CreateCommand())
        {
            command.CommandText = CREATE_SCRIPT;
            command.ExecuteNonQuery();

            // Stamp schema version on first creation; never downgrade.
            using (SqliteCommand readVersion = connection.CreateCommand())
            {
                readVersion.CommandText = "PRAGMA user_version;";
                long currentVersion = Convert.ToInt64(readVersion.ExecuteScalar());

                if (currentVersion < SCHEMA_VERSION)
                {
                    using (SqliteCommand setVersion = connection.CreateCommand())
                    {
                        setVersion.CommandText = $"PRAGMA user_version = {SCHEMA_VERSION};";
                        setVersion.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}
