using PhotoManager.Domain.Interfaces.Persistence.Repositories;
using PhotoManager.Persistence.Sqlite;

namespace PhotoManager.Persistence.Repositories;

internal sealed class FolderPersistence(ISqliteConnectionFactory connectionFactory) : IFolderPersistence
{
    public Folder Insert(string path)
    {
        Guid folderId = Guid.NewGuid();
        Folder folder = new() { Id = folderId, Path = path };

        using (SqliteConnection connection = connectionFactory.Open())
        {
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = "INSERT INTO Folders (Id, Path) VALUES ($id, $path);";
                command.Parameters.AddWithValue("$id", folderId);
                command.Parameters.AddWithValue("$path", path);

                command.ExecuteNonQuery();

                return folder;
            }
        }
    }

    public bool Delete(Guid folderId)
    {
        using (SqliteConnection connection = connectionFactory.Open())
        {
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = "DELETE FROM Folders WHERE Id = $id;";
                command.Parameters.AddWithValue("$id", folderId);

                return command.ExecuteNonQuery() > 0;
            }
        }
    }

    public Folder? GetById(Guid folderId)
    {
        using (SqliteConnection connection = connectionFactory.Open())
        {
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT Id, Path FROM Folders WHERE Id = $id LIMIT 1;";
                command.Parameters.AddWithValue("$id", folderId);

                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    return reader.Read() ? Map(reader) : null;
                }
            }
        }
    }

    public Folder? GetByPath(string path)
    {
        using (SqliteConnection connection = connectionFactory.Open())
        {
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT Id, Path FROM Folders WHERE Path = $path LIMIT 1;";
                command.Parameters.AddWithValue("$path", path);

                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    return reader.Read() ? Map(reader) : null;
                }
            }
        }
    }

    public IReadOnlyList<Folder> GetAll()
    {
        using (SqliteConnection connection = connectionFactory.Open())
        {
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT Id, Path FROM Folders ORDER BY rowid;";

                List<Folder> result = [];

                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(Map(reader));
                    }

                    return result;
                }
            }
        }
    }

    public int Count()
    {
        using (SqliteConnection connection = connectionFactory.Open())
        {
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT COUNT(*) FROM Folders;";

                return Convert.ToInt32(command.ExecuteScalar());
            }
        }
    }

    private static Folder Map(SqliteDataReader reader) => new()
    {
        Id = Guid.Parse(reader.GetString(0)),
        Path = reader.GetString(1)
    };
}
