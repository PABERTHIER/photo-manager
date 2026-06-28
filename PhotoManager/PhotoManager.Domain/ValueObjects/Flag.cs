namespace PhotoManager.Domain.ValueObjects;

public readonly struct Flag
{
    public bool IsTrue { get; init; }
    public string? Message { get; init; }
}
