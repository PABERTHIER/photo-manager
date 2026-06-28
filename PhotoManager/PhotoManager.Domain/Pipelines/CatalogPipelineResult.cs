namespace PhotoManager.Domain.Pipelines;

public sealed record CatalogPipelineResult(
    int Sequence,
    string DirectoryPath,
    string FileName,
    bool IsVideo,
    CatalogAssetPipelineOperation Operation,
    Asset? Asset);
