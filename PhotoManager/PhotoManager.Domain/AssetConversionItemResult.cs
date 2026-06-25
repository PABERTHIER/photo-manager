namespace PhotoManager.Domain;

public record AssetConversionItemResult(string SourcePath, string? DestinationPath, bool Success, string Message);
