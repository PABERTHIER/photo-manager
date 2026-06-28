namespace PhotoManager.Domain.ValueObjects;

public readonly struct Pixel
{
    public Dimensions Asset { get; init; }
    public Dimensions Thumbnail { get; init; }
}
