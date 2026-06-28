namespace PhotoManager.Domain.Models;

public record AssetConversionItemResult(string SourcePath, string? DestinationPath, bool Success, string Message);
