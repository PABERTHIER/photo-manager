namespace PhotoManager.Infrastructure.Database;

public class ColumnProperties
{
    public required string ColumnName { get; set; }
    public bool EscapeText { get; set; }
}
