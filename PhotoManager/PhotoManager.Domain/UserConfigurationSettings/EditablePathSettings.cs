namespace PhotoManager.Domain.UserConfigurationSettings;

public record EditablePathSettings(
    string AssetsDirectory,
    string ExemptedFolderPath,
    string FirstFrameVideosFolderName);
