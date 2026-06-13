namespace PhotoManager.Domain.Pipeline;

public sealed record CatalogPipelineOptions(
    int ReadConcurrency = 4,
    int ProcessConcurrency = 0,
    int ReadBufferSize = 16,
    int ProcessBufferSize = 32,
    int PersistBufferSize = 64,
    int PersistBatchSize = 50)
{
    public int EffectiveReadConcurrency => Math.Max(1, ReadConcurrency);
    public int EffectiveProcessConcurrency => ProcessConcurrency <= 0
        ? Math.Max(1, Environment.ProcessorCount)
        : ProcessConcurrency;
    public int EffectiveReadBufferSize => Math.Max(1, ReadBufferSize);
    public int EffectiveProcessBufferSize => Math.Max(1, ProcessBufferSize);
    public int EffectivePersistBufferSize => Math.Max(1, PersistBufferSize);
    public int EffectivePersistBatchSize => Math.Max(1, PersistBatchSize);
}
