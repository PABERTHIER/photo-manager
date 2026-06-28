using System.Threading.Channels;

namespace PhotoManager.Domain.Pipelines;

public sealed class CatalogFolderPipeline(IFileOperationsService fileOperationsService,
    IAssetCreationService assetCreationService, IAssetRepository assetRepository)
{
    public async Task<int> CatalogAsync(string directory, IReadOnlyList<string> fileNames, bool isAssetVideo,
        int maxSuccessfulAssets, CatalogAssetPipelineOperation operation, CatalogPipelineOptions options,
        Func<CatalogPipelineResult, CancellationToken, ValueTask<bool>> onResultAsync, CancellationToken token)
    {
        if (maxSuccessfulAssets <= 0 || fileNames.Count == 0)
        {
            return 0;
        }

        int queuedCount = fileNames.Count;
        CatalogPipelineFileItem[] items = new CatalogPipelineFileItem[queuedCount];

        for (int i = 0; i < queuedCount; i++)
        {
            items[i] = new(i, directory, fileNames[i], isAssetVideo, operation);
        }

        Channel<CatalogPipelineFileReadResult> readChannel = Channel.CreateBounded<CatalogPipelineFileReadResult>(
            new BoundedChannelOptions(options.EffectiveReadBufferSize)
            {
                SingleWriter = false,
                SingleReader = false,
                FullMode = BoundedChannelFullMode.Wait
            });
        Channel<CatalogPipelineProcessedAsset> processedChannel =
            Channel.CreateBounded<CatalogPipelineProcessedAsset>(
                new BoundedChannelOptions(options.EffectiveProcessBufferSize)
                {
                    SingleWriter = false,
                    SingleReader = true,
                    FullMode = BoundedChannelFullMode.Wait
                });
        Channel<CatalogPipelineResult> persistedChannel = Channel.CreateBounded<CatalogPipelineResult>(
            new BoundedChannelOptions(options.EffectivePersistBufferSize)
            {
                SingleWriter = true,
                SingleReader = true,
                FullMode = BoundedChannelFullMode.Wait
            });

        using CancellationTokenSource faultCancellation = new();
        using CancellationTokenSource limitCancellation = new();
        using CancellationTokenSource pipelineCancellation =
            CancellationTokenSource.CreateLinkedTokenSource(token, faultCancellation.Token, limitCancellation.Token);
        CancellationToken pipelineToken = pipelineCancellation.Token;

        Task readerTask = ReadFilesAsync(items, readChannel.Writer, options, pipelineToken);
        Task processorTask = ProcessAssetsAsync(readChannel.Reader, processedChannel.Writer, options, pipelineToken);
        Task writerTask = PersistAssetsAsync(processedChannel.Reader, persistedChannel.Writer, options,
            maxSuccessfulAssets, limitCancellation, pipelineToken, faultCancellation.Token);
        Task<int> callbackTask = ProcessCallbacksAsync(persistedChannel.Reader, onResultAsync, token);
        Task[] faultMonitors =
        [
            CancelOnFailureAsync(readerTask, faultCancellation, token, limitCancellation.Token, false),
            CancelOnFailureAsync(processorTask, faultCancellation, token, limitCancellation.Token, false),
            CancelOnFailureAsync(writerTask, faultCancellation, token, limitCancellation.Token, false),
            CancelOnFailureAsync(callbackTask, faultCancellation, token, limitCancellation.Token, true)
        ];

        try
        {
            await Task.WhenAll(readerTask, processorTask, writerTask, callbackTask).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (limitCancellation.IsCancellationRequested
                                                && !token.IsCancellationRequested
                                                && !faultCancellation.IsCancellationRequested)
        {
            // The success limit is a normal stop condition; upstream stages were canceled to avoid extra work.
        }
        catch
        {
            await faultCancellation.CancelAsync();
            throw;
        }
        finally
        {
            await Task.WhenAll(faultMonitors).ConfigureAwait(false);
        }

        return await callbackTask.ConfigureAwait(false);
    }

    private static async Task CancelOnFailureAsync(Task task, CancellationTokenSource cancellationTokenSource,
        CancellationToken externalToken, CancellationToken limitToken, bool cancelOnCancellation)
    {
        try
        {
            await task.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            bool expectedCancellation = ex is OperationCanceledException
                && (externalToken.IsCancellationRequested || limitToken.IsCancellationRequested);

            if (!expectedCancellation || cancelOnCancellation)
            {
                await cancellationTokenSource.CancelAsync();
            }
        }
    }

    private async Task ReadFilesAsync(IReadOnlyList<CatalogPipelineFileItem> items,
        ChannelWriter<CatalogPipelineFileReadResult> output, CatalogPipelineOptions options, CancellationToken token)
    {
        try
        {
            ParallelOptions parallelOptions = new()
            {
                MaxDegreeOfParallelism = options.EffectiveReadConcurrency,
                CancellationToken = token
            };

            await Parallel.ForEachAsync(items, parallelOptions, async (item, ct) =>
            {
                try
                {
                    string fullPath = Path.Combine(item.DirectoryPath, item.FileName);
                    byte[] imageBytes = fileOperationsService.GetFileBytes(fullPath);
                    await output.WriteAsync(new(item, imageBytes, null), ct).ConfigureAwait(false);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    await output.WriteAsync(new(item, null, ex), ct).ConfigureAwait(false);
                }
            }).ConfigureAwait(false);

            output.TryComplete();
        }
        catch (Exception ex)
        {
            output.TryComplete(ex);
            throw;
        }
    }

    private async Task ProcessAssetsAsync(ChannelReader<CatalogPipelineFileReadResult> input,
        ChannelWriter<CatalogPipelineProcessedAsset> output, CatalogPipelineOptions options, CancellationToken token)
    {
        Task[] workers = new Task[options.EffectiveProcessConcurrency];

        for (int i = 0; i < workers.Length; i++)
        {
            workers[i] = Task.Run(async () =>
            {
                await foreach (CatalogPipelineFileReadResult item in input.ReadAllAsync(token).ConfigureAwait(false))
                {
                    CatalogPipelineProcessedAsset processedAsset = ProcessAsset(item);
                    await output.WriteAsync(processedAsset, token).ConfigureAwait(false);
                }
            }, token);
        }

        try
        {
            await Task.WhenAll(workers).ConfigureAwait(false);
            output.TryComplete();
        }
        catch (Exception ex)
        {
            output.TryComplete(ex);
            throw;
        }
    }

    private CatalogPipelineProcessedAsset ProcessAsset(CatalogPipelineFileReadResult item)
    {
        if (item.Exception != null || item.ImageBytes == null)
        {
            return new(item.Item, null);
        }

        AssetWithThumbnail? assetWithThumbnail = assetCreationService.CreateAssetWithThumbnail(
            item.Item.DirectoryPath,
            item.Item.FileName,
            item.ImageBytes,
            item.Item.IsVideo,
            skipCatalogCheck: true);

        return new(item.Item, assetWithThumbnail);
    }

    private async Task PersistAssetsAsync(ChannelReader<CatalogPipelineProcessedAsset> input,
        ChannelWriter<CatalogPipelineResult> output, CatalogPipelineOptions options, int maxSuccessfulAssets,
        CancellationTokenSource limitCancellation, CancellationToken token, CancellationToken faultToken)
    {
        List<CatalogPipelineProcessedAsset> batch = new(options.EffectivePersistBatchSize);
        SortedDictionary<int, CatalogPipelineProcessedAsset> pendingAssets = [];
        int nextSequence = 0;
        int successCount = 0;

        try
        {
            await foreach (CatalogPipelineProcessedAsset item in input.ReadAllAsync(token).ConfigureAwait(false))
            {
                pendingAssets[item.Item.Sequence] = item;

                while (pendingAssets.TryGetValue(nextSequence, out CatalogPipelineProcessedAsset? pendingAsset))
                {
                    pendingAssets.Remove(nextSequence);
                    nextSequence++;

                    if (pendingAsset.AssetWithThumbnail == null)
                    {
                        await output.WriteAsync(ToResult(pendingAsset, null), token).ConfigureAwait(false);
                        continue;
                    }

                    batch.Add(pendingAsset);
                    successCount++;

                    if (batch.Count >= options.EffectivePersistBatchSize)
                    {
                        await FlushBatchAsync(batch, output, faultToken).ConfigureAwait(false);
                        batch.Clear();
                    }

                    if (successCount >= maxSuccessfulAssets)
                    {
                        if (batch.Count > 0)
                        {
                            await FlushBatchAsync(batch, output, faultToken).ConfigureAwait(false);
                            batch.Clear();
                        }

                        await limitCancellation.CancelAsync();
                        output.TryComplete();
                        return;
                    }
                }
            }

            if (batch.Count > 0)
            {
                await FlushBatchAsync(batch, output, faultToken).ConfigureAwait(false);
            }

            output.TryComplete();
        }
        catch (Exception ex)
        {
            output.TryComplete(ex);
            throw;
        }
    }

    private async Task FlushBatchAsync(List<CatalogPipelineProcessedAsset> batch,
        ChannelWriter<CatalogPipelineResult> output, CancellationToken faultToken)
    {
        AssetWithThumbnail[] assetsWithThumbnails = new AssetWithThumbnail[batch.Count];

        for (int i = 0; i < batch.Count; i++)
        {
            assetsWithThumbnails[i] = batch[i].AssetWithThumbnail!;
        }

        assetRepository.AddAssets(assetsWithThumbnails);

        for (int i = 0; i < batch.Count; i++)
        {
            CatalogPipelineProcessedAsset item = batch[i];
            await output.WriteAsync(ToResult(item, item.AssetWithThumbnail!.Asset), faultToken)
                .ConfigureAwait(false);
        }
    }

    private static async Task<int> ProcessCallbacksAsync(ChannelReader<CatalogPipelineResult> input,
        Func<CatalogPipelineResult, CancellationToken, ValueTask<bool>> onResultAsync, CancellationToken token)
    {
        SortedDictionary<int, CatalogPipelineResult> pendingResults = [];
        int nextSequence = 0;
        int successCount = 0;

        bool cancellationRequested = false;

        try
        {
            await foreach (CatalogPipelineResult item in input.ReadAllAsync(token).ConfigureAwait(false))
            {
                pendingResults[item.Sequence] = item;

                while (pendingResults.TryGetValue(nextSequence, out CatalogPipelineResult? pendingResult))
                {
                    pendingResults.Remove(nextSequence);

                    if (await onResultAsync(pendingResult, token).ConfigureAwait(false))
                    {
                        successCount++;
                    }

                    if (token.IsCancellationRequested)
                    {
                        throw new OperationCanceledException(token);
                    }

                    nextSequence++;
                }
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            cancellationRequested = true;
        }

        if (cancellationRequested)
        {
            throw new OperationCanceledException(token);
        }

        return successCount;
    }

    private static CatalogPipelineResult ToResult(CatalogPipelineProcessedAsset item, Asset? asset)
    {
        return new(item.Item.Sequence, item.Item.DirectoryPath, item.Item.FileName, item.Item.IsVideo,
            item.Item.Operation, asset);
    }

    private sealed record CatalogPipelineFileItem(int Sequence, string DirectoryPath, string FileName, bool IsVideo,
        CatalogAssetPipelineOperation Operation);

    private sealed record CatalogPipelineFileReadResult(CatalogPipelineFileItem Item, byte[]? ImageBytes,
        Exception? Exception);

    private sealed record CatalogPipelineProcessedAsset(CatalogPipelineFileItem Item,
        AssetWithThumbnail? AssetWithThumbnail);
}
