namespace PhotoManager.Domain.Interfaces.Persistence.Repositories;

/// <summary>
/// Provides key/value configuration storage backed by the SQLite Configuration table.
/// </summary>
public interface IConfigurationPersistence
{
    /// <summary>Returns the stored value for <paramref name="key"/>, or <c>null</c> if not set.</summary>
    string? GetValue(string key);

    /// <summary>Inserts or updates the stored value for <paramref name="key"/>.</summary>
    void SetValue(string key, string value);
}
