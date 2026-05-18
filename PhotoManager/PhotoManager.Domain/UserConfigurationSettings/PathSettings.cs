namespace PhotoManager.Domain.UserConfigurationSettings;

public record PathSettings(
    string AssetsDirectory,
    string ExemptedFolderPath,
    string FirstFrameVideosPath);
