namespace PhotoManager.Domain.UserConfigurationSettings;

public record StorageSettings(ushort BackupsToKeep, ushort ThumbnailsDictionaryEntriesToKeep);
