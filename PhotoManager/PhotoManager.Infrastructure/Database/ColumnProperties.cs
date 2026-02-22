namespace PhotoManager.Infrastructure.Database;

public readonly struct ColumnProperties
{
    public required string ColumnName { get; init; }
    public bool EscapeText { get; init; }
}
