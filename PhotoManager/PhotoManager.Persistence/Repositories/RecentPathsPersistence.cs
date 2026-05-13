using PhotoManager.Persistence.Sqlite;

namespace PhotoManager.Persistence.Repositories;

internal sealed class RecentPathsPersistence(ISqliteConnectionFactory connectionFactory) : IRecentPathsPersistence
{
    public IReadOnlyList<string> GetAll()
    {
        using (SqliteConnection connection = connectionFactory.Open())
        {
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT Path FROM RecentPaths ORDER BY Position;";

                List<string> result = [];

                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(reader.GetString(0));
                    }

                    return result;
                }
            }
        }
    }

    public void Replace(IReadOnlyList<string> paths)
    {
        using (SqliteConnection connection = connectionFactory.Open())
        {
            using (SqliteTransaction transaction = connection.BeginTransaction())
            {
                using (SqliteCommand delCommand = connection.CreateCommand())
                {
                    delCommand.Transaction = transaction;
                    delCommand.CommandText = "DELETE FROM RecentPaths WHERE 1 = 1;";

                    delCommand.ExecuteNonQuery();
                }

                if (paths.Count > 0)
                {
                    using (SqliteCommand insertCommand = connection.CreateCommand())
                    {
                        insertCommand.Transaction = transaction;
                        insertCommand.CommandText =
                            "INSERT INTO RecentPaths (Position, Path) VALUES ($position, $path);";

                        SqliteParameter position = insertCommand.Parameters.Add("$position", SqliteType.Integer);
                        SqliteParameter path = insertCommand.Parameters.Add("$path", SqliteType.Text);

                        for (int i = 0; i < paths.Count; i++)
                        {
                            position.Value = i;
                            path.Value = paths[i];

                            insertCommand.ExecuteNonQuery();
                        }
                    }
                }

                transaction.Commit();
            }
        }
    }
}
