namespace PhotoManager.Domain;

public readonly struct FileProperties
{
    public long Size { get; init; }
    public DateTime Creation { get; init; }
    public DateTime Modification { get; init; }
}
