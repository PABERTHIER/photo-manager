namespace PhotoManager.Domain;

public class SyncAssetsResult
{
    public required string SourceDirectory { get; init; }
    public required string DestinationDirectory { get; init; }
    public int SyncedImages { get; set; }
    public string? Message { get; set; }
}
