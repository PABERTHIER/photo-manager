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

    public IReadOnlyDictionary<string, string> GetAll()
    {
        Dictionary<string, string> values = new(StringComparer.Ordinal);

        using (SqliteConnection connection = connectionFactory.Open())
        {
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT Key, Value FROM Configuration;";

                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        values[reader.GetString(0)] = reader.GetString(1);
                    }
                }
            }
        }

        return values;
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

    public void SetValues(IReadOnlyDictionary<string, string> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        if (values.Count == 0)
        {
            return;
        }

        using (SqliteConnection connection = connectionFactory.Open())
        {
            using (SqliteTransaction transaction = connection.BeginTransaction())
            {
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.Transaction = transaction;
                    command.CommandText = "INSERT OR REPLACE INTO Configuration (Key, Value) VALUES ($key, $value);";
                    SqliteParameter keyParameter = command.Parameters.Add("$key", SqliteType.Text);
                    SqliteParameter valueParameter = command.Parameters.Add("$value", SqliteType.Text);

                    foreach (KeyValuePair<string, string> entry in values)
                    {
                        keyParameter.Value = entry.Key;
                        valueParameter.Value = entry.Value;
                        command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
        }
    }
}
