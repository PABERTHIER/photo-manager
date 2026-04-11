namespace PhotoManager.Domain;

public readonly struct AboutInformation
{
    public required string Product { get; init; }
    public string? Author { get; init; }
    public required string Version { get; init; }
}
