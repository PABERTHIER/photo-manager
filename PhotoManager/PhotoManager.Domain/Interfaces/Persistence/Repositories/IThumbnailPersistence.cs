namespace PhotoManager.Domain.Interfaces.Persistence.Repositories;

public interface IThumbnailPersistence
{
    /// <summary>Returns all thumbnails for the given folder, keyed by file name.</summary>
    Dictionary<string, byte[]> GetByFolderId(Guid folderId);

    /// <summary>Replaces the entire thumbnail set for a folder.</summary>
    void ReplaceForFolder(Guid folderId, IReadOnlyDictionary<string, byte[]> thumbnails);

    /// <summary>Inserts or updates a single thumbnail.</summary>
    void Upsert(Guid folderId, string fileName, byte[] data);

    /// <summary>Deletes a single thumbnail.</summary>
    bool Delete(Guid folderId, string fileName);

    /// <summary>Deletes all thumbnails for a folder.</summary>
    int DeleteByFolderId(Guid folderId);

    /// <summary>Returns true if any thumbnail exists for that folder.</summary>
    bool ExistsForFolder(Guid folderId);
}
