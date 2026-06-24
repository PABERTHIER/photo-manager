namespace PhotoManager.Domain.Interfaces;

public interface IAssetConversionService
{
    Task<AssetConversionResult> ConvertAssetsAsync(AssetConversionRequest request,
        AssetConversionProgressCallback callback, CancellationToken token = default);
}
