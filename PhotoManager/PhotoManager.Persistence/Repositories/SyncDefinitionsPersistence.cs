using PhotoManager.Persistence.Sqlite;

namespace PhotoManager.Persistence.Repositories;

internal sealed class SyncDefinitionsPersistence(ISqliteConnectionFactory connectionFactory)
    : ISyncDefinitionsPersistence
{
    public IReadOnlyList<SyncAssetsDirectoriesDefinition> GetAll()
    {
        using (SqliteConnection connection = connectionFactory.Open())
        {
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = """
                                      SELECT SourceDirectory, DestinationDirectory, IncludeSubFolders, DeleteAssetsNotInSource
                                      FROM SyncDefinitions ORDER BY Position;
                                      """;

                List<SyncAssetsDirectoriesDefinition> result = [];

                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new SyncAssetsDirectoriesDefinition
                        {
                            SourceDirectory = reader.GetString(0),
                            DestinationDirectory = reader.GetString(1),
                            IncludeSubFolders = reader.GetInt32(2) != 0,
                            DeleteAssetsNotInSource = reader.GetInt32(3) != 0
                        });
                    }

                    return result;
                }
            }
        }
    }

    public void Replace(IReadOnlyList<SyncAssetsDirectoriesDefinition> definitions)
    {
        using (SqliteConnection connection = connectionFactory.Open())
        {

            using (SqliteTransaction transaction = connection.BeginTransaction())
            {
                using (SqliteCommand delCommand = connection.CreateCommand())
                {
                    delCommand.Transaction = transaction;
                    // TODO: Fix warning "'Delete' statement without 'where' clears all data in the table"
                    delCommand.CommandText = "DELETE FROM SyncDefinitions;";

                    delCommand.ExecuteNonQuery();
                }

                if (definitions.Count > 0)
                {
                    using (SqliteCommand insertCommand = connection.CreateCommand())
                    {
                        insertCommand.Transaction = transaction;
                        insertCommand.CommandText = """
                                                    INSERT INTO SyncDefinitions
                                                        (Position, SourceDirectory, DestinationDirectory, IncludeSubFolders, DeleteAssetsNotInSource)
                                                    VALUES ($position, $source, $destination, $includeSubFolders, $deleteAssetsNotInSource);
                                                    """;

                        SqliteParameter position = insertCommand.Parameters.Add("$position", SqliteType.Integer);
                        SqliteParameter source = insertCommand.Parameters.Add("$source", SqliteType.Text);
                        SqliteParameter destination = insertCommand.Parameters.Add("$destination", SqliteType.Text);
                        SqliteParameter includeSubFolders =
                            insertCommand.Parameters.Add("$includeSubFolders", SqliteType.Integer);
                        SqliteParameter deleteAssetsNotInSource =
                            insertCommand.Parameters.Add("$deleteAssetsNotInSource", SqliteType.Integer);

                        for (int i = 0; i < definitions.Count; i++)
                        {
                            SyncAssetsDirectoriesDefinition def = definitions[i];

                            position.Value = i;
                            source.Value = def.SourceDirectory;
                            destination.Value = def.DestinationDirectory;
                            includeSubFolders.Value = def.IncludeSubFolders ? 1 : 0;
                            deleteAssetsNotInSource.Value = def.DeleteAssetsNotInSource ? 1 : 0;

                            insertCommand.ExecuteNonQuery();
                        }
                    }
                }

                transaction.Commit();
            }
        }
    }
}
