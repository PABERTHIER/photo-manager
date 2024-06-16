namespace PhotoManager.UI.ViewModels.Enums;

public enum SortCriteria
{
    Undefined, // TODO: Remove that one, should be FileName the default one
    FileName,
    FileSize,
    FileCreationDateTime,
    FileModificationDateTime,
    ThumbnailCreationDateTime
}
