namespace PhotoManager.Domain.Models;

public record AssetConversionRequest(IReadOnlyList<string> FilePaths, AssetConversionTargetFormat TargetFormat);
