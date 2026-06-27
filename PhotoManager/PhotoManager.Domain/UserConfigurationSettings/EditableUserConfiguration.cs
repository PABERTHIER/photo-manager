namespace PhotoManager.Domain.UserConfigurationSettings;

public record EditableUserConfiguration(
    AssetSettings AssetSettings,
    HashSettings HashSettings,
    EditablePathSettings PathSettings,
    PipelineSettings PipelineSettings,
    StorageSettings StorageSettings,
    UiSettings UiSettings);
