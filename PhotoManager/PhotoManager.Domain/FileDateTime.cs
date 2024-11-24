namespace PhotoManager.Domain;

public readonly struct FileDateTime
{
    public DateTime Creation { get; init; }
    public DateTime Modification { get; init; }
}
