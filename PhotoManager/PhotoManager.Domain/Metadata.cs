namespace PhotoManager.Domain;

public readonly struct Metadata
{
    public Flag Corrupted { get; init; }
    public Flag Rotated { get; init; }
}
