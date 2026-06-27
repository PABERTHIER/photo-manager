namespace PhotoManager.Domain.UserConfigurationSettings;

public record PipelineSettings(
    int ReadConcurrency,
    int ProcessConcurrency,
    int ReadBufferSize,
    int ProcessBufferSize,
    int PersistBufferSize,
    int PersistBatchSize);
