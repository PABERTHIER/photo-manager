namespace PhotoManager.Infrastructure.Database;

public class DataTableProperties
{
    public required string TableName { get; init; }
    public required ColumnProperties[] ColumnProperties { get; init; }
}
