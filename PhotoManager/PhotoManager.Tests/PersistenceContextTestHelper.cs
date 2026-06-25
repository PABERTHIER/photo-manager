namespace PhotoManager.Tests;

// Mirrors the production composition root (PersistenceServiceCollectionExtensions): builds the SQLite chain and
// initializes the context at the given database directory. Production no longer initializes the context from
// AssetRepository, so tests that bypass DI must supply a ready context themselves.
// The database directory must stay test-controlled (the mocked IPathProviderService) to avoid touching real data.
public static class PersistenceContextTestHelper
{
    public static SqlitePersistenceContext CreateInitializedContext(string databaseDirectory)
    {
        SqliteConnectionFactory connectionFactory = new(new TestLogger<SqliteConnectionFactory>());
        SqliteBackupService backupService = new(connectionFactory);
        SqlitePersistenceContext persistenceContext = new(connectionFactory, backupService,
            new TestLogger<SqlitePersistenceContext>());

        persistenceContext.Initialize(databaseDirectory);

        return persistenceContext;
    }
}
