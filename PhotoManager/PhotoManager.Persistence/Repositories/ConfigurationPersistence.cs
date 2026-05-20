using PhotoManager.Domain.Interfaces.Persistence.Repositories;
using PhotoManager.Persistence.Sqlite;

namespace PhotoManager.Persistence.Repositories;

internal sealed class ConfigurationPersistence(ISqliteConnectionFactory connectionFactory) : IConfigurationPersistence
{
    public string? GetValue(string key)
    {
        using (SqliteConnection connection = connectionFactory.Open())
        {
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT Value FROM Configuration WHERE Key = $key LIMIT 1;";
                command.Parameters.AddWithValue("$key", key);

                object? result = command.ExecuteScalar();

                return result is DBNull or null ? null : (string)result;
            }
        }
    }

    public void SetValue(string key, string value)
    {
        using (SqliteConnection connection = connectionFactory.Open())
        {
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = "INSERT OR REPLACE INTO Configuration (Key, Value) VALUES ($key, $value);";
                command.Parameters.AddWithValue("$key", key);
                command.Parameters.AddWithValue("$value", value);

                command.ExecuteNonQuery();
            }
        }
    }
}
