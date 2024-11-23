namespace PhotoManager.Domain;

public readonly struct Pixel
{
    public Dimensions Asset { get; init; }
    public Dimensions Thumbnail { get; init; }
}
