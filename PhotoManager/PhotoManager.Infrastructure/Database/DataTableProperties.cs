namespace PhotoManager.Infrastructure.Database;

public class DataTableProperties
{
    public required string TableName { get; set; }
    public required ColumnProperties[] ColumnProperties { get; set; }
}
