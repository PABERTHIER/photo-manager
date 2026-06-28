namespace PhotoManager.Domain.ValueObjects;

public readonly struct Metadata
{
    public Flag Corrupted { get; init; }
    public Flag Rotated { get; init; }
}
