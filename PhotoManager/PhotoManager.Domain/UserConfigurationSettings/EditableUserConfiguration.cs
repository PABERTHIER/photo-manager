namespace PhotoManager.Domain.UserConfigurationSettings;

public record EditableUserConfiguration(
    AssetSettings AssetSettings,
    HashSettings HashSettings,
    EditablePathSettings PathSettings,
    StorageSettings StorageSettings,
    UiSettings UiSettings);
