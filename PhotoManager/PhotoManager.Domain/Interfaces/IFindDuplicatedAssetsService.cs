namespace PhotoManager.Domain.Interfaces;

public interface IFindDuplicatedAssetsService
{
    Asset[][] GetDuplicatedAssets();
}
