namespace PhotoManager.Domain.UserConfigurationSettings;

public record class PathSettings(
    string AssetsDirectory,
    string BackupPath,
    string ExemptedFolderPath,
    string FirstFrameVideosPath);
