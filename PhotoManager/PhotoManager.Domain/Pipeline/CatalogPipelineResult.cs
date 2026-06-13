namespace PhotoManager.Domain.Pipeline;

public sealed record CatalogPipelineResult(
    int Sequence,
    string DirectoryPath,
    string FileName,
    bool IsVideo,
    CatalogAssetPipelineOperation Operation,
    Asset? Asset);
