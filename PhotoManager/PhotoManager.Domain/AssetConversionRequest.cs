namespace PhotoManager.Domain;

public record AssetConversionRequest(IReadOnlyList<string> FilePaths, AssetConversionTargetFormat TargetFormat);
