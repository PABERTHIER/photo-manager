namespace PhotoManager.Domain.UserConfigurationSettings;

public record PathSettings(
    string AssetsDirectory,
    string DatabasePath,
    string ExemptedFolderPath,
    string FirstFrameVideosPath);
