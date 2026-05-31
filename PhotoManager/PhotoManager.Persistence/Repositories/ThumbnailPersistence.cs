using PhotoManager.Domain.Interfaces.Persistence.Repositories;
using PhotoManager.Persistence.Sqlite;

namespace PhotoManager.Persistence.Repositories;

internal sealed class ThumbnailPersistence(ISqliteConnectionFactory connectionFactory) : IThumbnailPersistence
{
    internal const string UPSERT_SQL = """
                                       INSERT INTO Thumbnails (FolderId, FileName, Data)
                                       VALUES ($folderId, $fileName, $data)
                                       ON CONFLICT(FolderId, FileName) DO UPDATE SET Data = excluded.Data;
                                       """;

    public Dictionary<string, byte[]> GetByFolderId(Guid folderId)
    {
        using (SqliteConnection connection = connectionFactory.Open())
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
                        result[fileName] = reader.GetFieldValue<byte[]>(1);
                    }

                    return result;
                }
            }
        }
    }

    public void ReplaceForFolder(Guid folderId, IReadOnlyDictionary<string, byte[]> thumbnails)
    {
        using (SqliteConnection connection = connectionFactory.Open())
        {

            using (SqliteTransaction transaction = connection.BeginTransaction())
            {
                using (SqliteCommand delCommand = connection.CreateCommand())
                {
                    delCommand.Transaction = transaction;
                    delCommand.CommandText = "DELETE FROM Thumbnails WHERE FolderId = $folderId;";
                    delCommand.Parameters.AddWithValue("$folderId", folderId);
                    delCommand.ExecuteNonQuery();
                }

                if (thumbnails.Count > 0)
                {
                    using (SqliteCommand insertCommand = connection.CreateCommand())
                    {
                        insertCommand.Transaction = transaction;
                        insertCommand.CommandText =
                            "INSERT INTO Thumbnails (FolderId, FileName, Data) VALUES ($folderId, $fileName, $data);";

                        SqliteParameter folderIdParameter = insertCommand.Parameters.Add("$folderId", SqliteType.Text);
                        SqliteParameter fileName = insertCommand.Parameters.Add("$fileName", SqliteType.Text);
                        SqliteParameter data = insertCommand.Parameters.Add("$data", SqliteType.Blob);

                        folderIdParameter.Value = folderId;

                        foreach (KeyValuePair<string, byte[]> kvp in thumbnails)
                        {
                            fileName.Value = kvp.Key;
                            data.Value = kvp.Value;
                            insertCommand.ExecuteNonQuery();
                        }
                    }
                }

                transaction.Commit();
            }
        }
    }

    public void Upsert(Guid folderId, string fileName, byte[] data)
    {
        using (SqliteConnection connection = connectionFactory.Open())
        {
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = UPSERT_SQL;
                BindUpsert(command, folderId, fileName, data);

                command.ExecuteNonQuery();
            }
        }
    }

    public bool Delete(Guid folderId, string fileName)
    {
        using (SqliteConnection connection = connectionFactory.Open())
        {
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = "DELETE FROM Thumbnails WHERE FolderId = $folderId AND FileName = $fileName;";
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
                command.CommandText = "DELETE FROM Thumbnails WHERE FolderId = $folderId;";
                command.Parameters.AddWithValue("$folderId", folderId);

                return command.ExecuteNonQuery();
            }
        }
    }

    public bool ExistsForFolder(Guid folderId)
    {
        using (SqliteConnection connection = connectionFactory.Open())
        {
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT 1 FROM Thumbnails WHERE FolderId = $folderId LIMIT 1;";
                command.Parameters.AddWithValue("$folderId", folderId);

                return command.ExecuteScalar() != null;
            }
        }
    }

    internal static void BindUpsert(SqliteCommand command, Guid folderId, string fileName, byte[] data)
    {
        command.Parameters.AddWithValue("$folderId", folderId);
        command.Parameters.AddWithValue("$fileName", fileName);
        command.Parameters.AddWithValue("$data", data);
    }
}
