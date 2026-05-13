namespace PhotoManager.Persistence.Sqlite;

/// <summary>
/// Opens (and configures) a brand-new <see cref="SqliteConnection"/> per call.
/// SQLite connections are NOT safe to share across threads in
/// <c>Microsoft.Data.Sqlite</c>; the cheap and correct pattern is open/close
/// per logical operation. SQLite's own connection pool keeps overhead low.
/// </summary>
internal interface ISqliteConnectionFactory
{
    SqliteConnection Open();
}
