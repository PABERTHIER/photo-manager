namespace PhotoManager.UI.Services;

public interface ISingleInstanceService
{
    bool TryAcquire();
}
