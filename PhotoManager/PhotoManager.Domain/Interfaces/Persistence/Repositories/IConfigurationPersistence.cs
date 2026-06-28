namespace PhotoManager.Domain.Interfaces.Persistence.Repositories;

/// <summary>
/// Provides key/value configuration storage backed by the SQLite Configuration table.
/// </summary>
public interface IConfigurationPersistence
{
    /// <summary>Returns the stored value for <paramref name="key"/>, or <c>null</c> if not set.</summary>
    string? GetValue(string key);

    /// <summary>Returns every stored key/value pair. Empty when nothing has been persisted yet.</summary>
    IReadOnlyDictionary<string, string> GetAll();

    /// <summary>Inserts or updates the stored value for <paramref name="key"/>.</summary>
    void SetValue(string key, string value);

    /// <summary>Inserts or updates all <paramref name="values"/> atomically in a single transaction.</summary>
    void SetValues(IReadOnlyDictionary<string, string> values);
}
