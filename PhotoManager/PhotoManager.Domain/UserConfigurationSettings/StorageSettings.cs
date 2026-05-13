namespace PhotoManager.Domain.UserConfigurationSettings;

public record StorageSettings(
    ushort BackupsToKeep,
    string StorageVersion,
    ushort ThumbnailsDictionaryEntriesToKeep);
