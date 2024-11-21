namespace PhotoManager.Domain.UserConfigurationSettings;

public record PathSettings(
    string AssetsDirectory,
    string BackupPath,
    string ExemptedFolderPath,
    string FirstFrameVideosPath);
