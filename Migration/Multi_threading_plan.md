# Multi-Threading Plan for PhotoManager

## Executive Summary

This document provides a comprehensive plan for introducing multi-threading into PhotoManager, primarily targeting the `CatalogAssetsService` pipeline. The goal is to achieve **10-20× throughput improvement** for initial cataloging of 100K+ images without introducing race conditions, data corruption, or UI freezes.

**Verdict: Yes, multi-threading should be implemented**, but with a specific architecture (pipeline + bounded parallelism) rather than naive parallel loops.

---

## Current State Analysis

### Single-Threaded Bottleneck

The current `CatalogAssetsService.CatalogAssetsAsync()` processes everything sequentially:

```
For each folder:
    For each file:
        1. Read file bytes from disk     (~5-50ms per file, I/O-bound)
        2. Validate image format          (~1-5ms, CPU-bound)
        3. Extract EXIF orientation       (~0.5-2ms, CPU-bound)
        4. Compute hash                   (~5-200ms, CPU-bound)
        5. Generate thumbnail             (~10-100ms, CPU + memory bound)
        6. Write asset to SQLite          (~1-5ms, I/O-bound)
        7. Write thumbnail to SQLite      (~1-10ms, I/O-bound)
        8. Invoke callback                (~0.1ms, but blocks on UI dispatch)
```

**Total per asset:** ~25-370ms depending on file size and hash algorithm.
**Total for 100K assets:** ~40 minutes (best case) to ~10 hours (worst case, PHash + large files).

### Why Simple Parallel.ForEach Won't Work

1. **WPF BitmapImage requires STA thread** — **DONE**: non-UI layers now use `IImageData`, and the
   Avalonia UI creates `Bitmap` instances only for display.
2. **SQLite writer serialization** — WAL mode allows concurrent reads but still serializes writes. Naive parallelism creates lock contention
3. **Callback ordering** — The UI expects callbacks in a predictable order (folder by folder)
4. **Memory explosion** — Reading 8 files × 20MB simultaneously = 160MB+ of LOH allocations
5. **Shared mutable state** — `_cataloguedAssetsByPath`, `_backupHasSameContent`, `_currentFolderPath`

---

## Recommended Architecture: Staged Pipeline

### Design Overview

```
┌──────────────┐    ┌──────────────────┐    ┌──────────────────┐    ┌──────────────┐
│   Stage 1    │    │     Stage 2      │    │     Stage 3      │    │   Stage 4    │
│   File I/O   │───▶│  Image Process   │───▶│   Persistence    │───▶│   Callback   │
│  (Parallel)  │    │   (Parallel)     │    │  (Single Writer) │    │   (Serial)   │
└──────────────┘    └──────────────────┘    └──────────────────┘    └──────────────┘
  MaxDOP: 4-8         MaxDOP: CPU cores       MaxDOP: 1 (batched)     MaxDOP: 1
  Bounded: 16         Bounded: CPU×2          Bounded: 64             (UI thread)
```

Each stage communicates via bounded `Channel<T>`:
- **Backpressure:** If Stage 2 is slow, Stage 1 blocks (bounded channel full) → prevents memory explosion
- **Isolation:** Each stage has its own concurrency model
- **Cancellation:** CancellationToken propagates through all stages

### Stage Details

#### Stage 1: File Discovery & Reading (I/O-Bound)

**Responsibility:** Enumerate files in folders, read bytes (or prepare file handles) for processing.

**Parallelism:** 4-8 concurrent reads (matches SSD queue depth; HDD should use 1-2)

**Input:** Folder paths from the recursive directory walk.  
**Output:** `Channel<FileReadResult>` containing file path, bytes (or stream), and folder context.

```csharp
record FileReadResult(
    string FilePath,
    string FileName,
    string DirectoryPath,
    Folder Folder,
    byte[] ImageBytes,     // Or Memory<byte> from pool
    bool IsVideo);
```

**Key decisions:**
- Use `ArrayPool<byte>.Shared.Rent()` for buffers to reduce LOH pressure
- For SHA512 (non-PHash), stream directly without full-file allocation
- For PHash, only pass the file path (MagickImage reads the file itself)
- Bounded to `MaxDegreeOfParallelism` items in flight to cap memory usage

#### Stage 2: Image Processing & Hashing (CPU-Bound)

**Responsibility:** Validate image, compute hash, generate thumbnail bytes.

**Parallelism:** `Environment.ProcessorCount` workers (CPU-saturating)

**Input:** `Channel<FileReadResult>` from Stage 1  
**Output:** `Channel<ProcessedAsset>` with all computed data

```csharp
record ProcessedAsset(
    Asset Asset,
    byte[] ThumbnailData,
    string DirectoryPath,
    Folder Folder);
```

**Key decisions:**
- Replace WPF `BitmapImage` thumbnail pipeline with ImageMagick (thread-safe, no STA requirement) — **DONE** (IImageData/ImageRotation introduced; BitmapHelper rewritten with SkiaSharp + MagickImage HEIC fallback, fully thread-safe)
- Remove Windows-specific infrastructure blockers before pipeline work — **DONE** (FFmpeg runtimes, no UWP/OS
  codec dependency, file-lock single-instance, portable path/build tooling)
- Each worker creates its own `MagickImage` instance (ImageMagick is thread-safe at instance level)
- PHash workers can share a thread pool since they're CPU-bound
- Return the `byte[]` buffer to the pool after hash computation

#### Stage 3: Persistence (Write-Serialized, Batched)

**Responsibility:** Write assets and thumbnails to SQLite in batches.

**Parallelism:** Single writer, but batched (50-100 assets per transaction)

**Input:** `Channel<ProcessedAsset>` from Stage 2  
**Output:** `Channel<PersistedAsset>` for callback notification

```csharp
record PersistedAsset(
    Asset Asset,
    string DirectoryPath,
    bool IsNew);  // vs Updated vs Deleted
```

**Key decisions:**
- Use `UpsertMany()` for batched writes (one transaction per batch)
- Use a single long-lived connection for the duration of cataloging
- Batch thumbnails alongside assets in the same transaction
- Commit every 50-100 assets OR every 500ms (whichever comes first)
- Update in-memory data structures (`_assetsByFolderId`) immediately after commit

#### Stage 4: Callback & UI Notification (Single-Threaded)

**Responsibility:** Notify the UI/caller about progress in order.

**Parallelism:** None (serial, on the Task.Run thread or marshaled to UI)

**Input:** `Channel<PersistedAsset>` from Stage 3  
**Output:** `CatalogChangeCallback` invocations

**Key decisions:**
- Callbacks are batched/debounced (e.g., one callback per folder or every 100 assets)
- The heavy `CataloguedAssetsByPath` list is passed as a lazy snapshot, not cloned per callback
- Progress percentage is calculated from `processedCount / totalFilesInFolder`

---

## Detailed Implementation Plan

### Step 1: Extract Pipeline Infrastructure

Create a generic pipeline helper:

```csharp
// PhotoManager.Domain/Pipeline/AssetPipeline.cs
public sealed class AssetPipeline : IDisposable
{
    private readonly Channel<FileReadResult> _readChannel;
    private readonly Channel<ProcessedAsset> _processedChannel;
    private readonly Channel<PersistedAsset> _persistedChannel;
    private readonly CancellationTokenSource _linkedCts;
    
    public AssetPipeline(PipelineOptions options, CancellationToken externalToken)
    {
        _linkedCts = CancellationTokenSource.CreateLinkedTokenSource(externalToken);
        
        _readChannel = Channel.CreateBounded<FileReadResult>(
            new BoundedChannelOptions(options.ReadBufferSize)
            {
                SingleWriter = false,
                SingleReader = false,
                FullMode = BoundedChannelFullMode.Wait
            });
            
        _processedChannel = Channel.CreateBounded<ProcessedAsset>(
            new BoundedChannelOptions(options.ProcessBufferSize)
            {
                SingleWriter = false,
                SingleReader = true,
                FullMode = BoundedChannelFullMode.Wait
            });
            
        _persistedChannel = Channel.CreateBounded<PersistedAsset>(
            new BoundedChannelOptions(options.PersistBufferSize)
            {
                SingleWriter = true,
                SingleReader = true,
                FullMode = BoundedChannelFullMode.Wait
            });
    }
}

public record PipelineOptions(
    int ReadConcurrency = 4,          // I/O parallelism
    int ProcessConcurrency = 0,       // 0 = ProcessorCount
    int ReadBufferSize = 16,          // Max items in read channel
    int ProcessBufferSize = 32,       // Max items in process channel
    int PersistBufferSize = 64,       // Max items in persist channel
    int PersistBatchSize = 50);       // Assets per DB transaction
```

### Step 2: Refactor AssetCreationService for Thread Safety

Current `AssetCreationService` uses:
- `IImageProcessingService` (wraps SkiaSharp BitmapHelper) — **Thread-safe** (SkiaSharp SKBitmap is thread-safe for read operations)
- `IImageMetadataService` (stateless EXIF reading) — **thread-safe**
- `IAssetHashCalculatorService` (stateless) — **thread-safe**
- `IAssetRepository` (ConcurrentDictionary-backed) — **thread-safe for reads**
- `IFileOperationsService` (stateless) — **thread-safe**

**Required changes:**
1. Create `IThreadSafeThumbnailGenerator` interface backed by ImageMagick instead of WPF
2. Make `AssetCreationService` stateless (it already is, just needs the new thumbnail service)
3. Ensure `IUserConfigurationService` returns immutable records (it already does)

```csharp
// New interface — no WPF dependency
public interface IThumbnailGenerator
{
    byte[] GenerateThumbnail(byte[] imageBytes, string filePath, int maxWidth, int maxHeight, 
        Rotation rotation, string format);
    byte[] GenerateThumbnail(string filePath, int maxWidth, int maxHeight, 
        Rotation rotation, string format);
}
```

### Step 3: Implement Parallel File Reader (Stage 1)

```csharp
public sealed class ParallelFileReader
{
    public async Task ReadFilesAsync(
        IEnumerable<(string FileName, string Directory, Folder Folder, bool IsVideo)> files,
        ChannelWriter<FileReadResult> output,
        int maxConcurrency,
        CancellationToken token)
    {
        ParallelOptions options = new()
        {
            MaxDegreeOfParallelism = maxConcurrency,
            CancellationToken = token
        };
        
        await Parallel.ForEachAsync(files, options, async (file, ct) =>
        {
            string fullPath = Path.Combine(file.Directory, file.FileName);
            
            if (!File.Exists(fullPath))
                return;
            
            byte[] bytes = await File.ReadAllBytesAsync(fullPath, ct);
            
            FileReadResult result = new(fullPath, file.FileName, file.Directory, 
                file.Folder, bytes, file.IsVideo);
                
            await output.WriteAsync(result, ct);
        });
        
        output.Complete();
    }
}
```

### Step 4: Implement Parallel Image Processor (Stage 2)

```csharp
public sealed class ParallelImageProcessor
{
    public async Task ProcessAsync(
        ChannelReader<FileReadResult> input,
        ChannelWriter<ProcessedAsset> output,
        int maxConcurrency,
        CancellationToken token)
    {
        // Multiple consumers reading from the same channel
        Task[] workers = new Task[maxConcurrency];
        
        for (int i = 0; i < maxConcurrency; i++)
        {
            workers[i] = Task.Run(async () =>
            {
                await foreach (FileReadResult item in input.ReadAllAsync(token))
                {
                    ProcessedAsset? result = ProcessItem(item);
                    
                    if (result != null)
                    {
                        await output.WriteAsync(result, token);
                    }
                }
            }, token);
        }
        
        await Task.WhenAll(workers);
        output.Complete();
    }
    
    private ProcessedAsset? ProcessItem(FileReadResult item)
    {
        // 1. Validate image
        // 2. Extract EXIF
        // 3. Compute hash (PHash or SHA512)
        // 4. Generate thumbnail via ImageMagick
        // 5. Build Asset object
        // All using thread-safe, non-WPF code
    }
}
```

### Step 5: Implement Batched Writer (Stage 3)

```csharp
public sealed class BatchedPersistenceWriter
{
    private readonly int _batchSize;
    private readonly TimeSpan _maxBatchDelay;
    
    public async Task WriteAsync(
        ChannelReader<ProcessedAsset> input,
        ChannelWriter<PersistedAsset> output,
        CancellationToken token)
    {
        List<ProcessedAsset> batch = new(_batchSize);
        
        await foreach (ProcessedAsset item in input.ReadAllAsync(token))
        {
            batch.Add(item);
            
            if (batch.Count >= _batchSize)
            {
                await FlushBatch(batch, output, token);
                batch.Clear();
            }
        }
        
        // Flush remaining
        if (batch.Count > 0)
        {
            await FlushBatch(batch, output, token);
        }
        
        output.Complete();
    }
    
    private async Task FlushBatch(
        List<ProcessedAsset> batch, 
        ChannelWriter<PersistedAsset> output,
        CancellationToken token)
    {
        // Single transaction for entire batch:
        // 1. UpsertMany(assets)
        // 2. Batch thumbnail upserts
        // 3. Update in-memory state
        // 4. Write results to output channel
        
        IReadOnlyList<Asset> assets = batch.Select(b => b.Asset).ToList();
        _persistenceContext.Assets.UpsertMany(assets);
        
        // Batch thumbnail writes in same connection/transaction
        using SqliteConnection conn = _connectionFactory.Open();
        using SqliteTransaction tx = conn.BeginTransaction();
        // ... batch thumbnail inserts ...
        tx.Commit();
        
        foreach (ProcessedAsset item in batch)
        {
            await output.WriteAsync(new PersistedAsset(item.Asset, item.DirectoryPath, true), token);
        }
    }
}
```

### Step 6: Orchestrate in CatalogAssetsService

```csharp
public async Task CatalogAssetsAsync(CatalogChangeCallback callback, CancellationToken token = default)
{
    PipelineOptions options = new(
        ReadConcurrency: DetermineReadConcurrency(),     // 4 for SSD, 1-2 for HDD
        ProcessConcurrency: Environment.ProcessorCount,
        PersistBatchSize: 50
    );
    
    using AssetPipeline pipeline = new(options, token);
    
    // Launch all stages concurrently
    Task readerTask = _fileReader.ReadFilesAsync(EnumerateNewFiles(), pipeline.ReadWriter, options.ReadConcurrency, token);
    Task processorTask = _imageProcessor.ProcessAsync(pipeline.ReadReader, pipeline.ProcessedWriter, options.ProcessConcurrency, token);
    Task writerTask = _batchWriter.WriteAsync(pipeline.ProcessedReader, pipeline.PersistedWriter, token);
    Task callbackTask = ProcessCallbacks(pipeline.PersistedReader, callback, token);
    
    await Task.WhenAll(readerTask, processorTask, writerTask, callbackTask);
}
```

---

## Thread Safety Analysis

### Shared State in Current Code

| State | Current Access | Thread Safety | Fix |
|-------|---------------|---------------|-----|
| `_cataloguedAssetsByPath` | Mutable List, read+write in CatalogAssetsService | ❌ Not safe | Move to pipeline; each stage owns its data |
| `_backupHasSameContent` | Boolean flag, written in multiple loops | ❌ Not safe | Use `Interlocked` or `volatile` |
| `_currentFolderPath` | String, updated per folder | ❌ Not safe | Pass folder context through the pipeline |
| `_directories` | HashSet, accumulated during run | ❌ Not safe | Use `ConcurrentHashSet` or accumulate in orchestrator |
| `_assetsByFolderId` | ConcurrentDictionary (in AssetRepository) | ✅ Safe | Already thread-safe |
| `_foldersByPath` | ConcurrentDictionary | ✅ Safe | Already thread-safe |
| `_thumbnailCache` | LruCache with internal lock | ✅ Safe | Already thread-safe |
| `_totalAssetCount` | int with `Interlocked` | ✅ Safe | Already thread-safe |

### SQLite Thread Safety

- **WAL mode:** Allows multiple concurrent readers with one writer
- **Connection per thread:** Each stage should use its own connection (already the pattern)
- **Writer serialization:** Stage 3 is single-threaded, so no write contention
- **`busy_timeout`:** Already set to 5000ms as safety net

### WPF Thread Affinity

The biggest constraint: **`BitmapImage` must be created on an STA thread or Frozen before cross-thread access.**

**Solution:** Replace WPF-based thumbnail generation with ImageMagick in the processing pipeline.
The migrated Avalonia UI now converts `IImageData` to `Avalonia.Media.Imaging.Bitmap` only when
display code requests it.

> **Note:** The `IImageData` abstraction has been introduced (Phase 1.1/1.2 of the Cross-Platform
> Migration Plan — **DONE**). All non-UI interfaces now use `IImageData` instead of `BitmapImage`,
> and `ImageRotation` instead of WPF `Rotation`. The UI layer converts `IImageData` to Avalonia
> `Bitmap` at display time only — **DONE**. This unblocks multi-threading in the processing pipeline.

The stored thumbnail data (in SQLite and cache) is raw `byte[]` — already thread-safe.
The UI decode only happens at display time.

### Avalonia UI State Ownership — **DONE**

The migrated UI now avoids direct two-way mutation of performance-sensitive `ApplicationViewModel` state from XAML.
Viewer position, selected assets, refresh status, status text, and move-target history are updated through explicit
methods, with one-way bindings where the UI should only observe state. This keeps the UI side ready for future
background cataloging work without reintroducing broad mutable state from controls.

---

## Concurrency Control Strategy

### Approach: Folder-Level Granularity with Intra-Folder Parallelism

```
Root folders (sequential or parallel):
├── Folder A: [Stage 1 | Stage 2 | Stage 3 | Stage 4] ← Pipeline within folder
├── Folder B: [Stage 1 | Stage 2 | Stage 3 | Stage 4]
└── Folder C: ...
```

**Option A: Pipeline per folder (Recommended)**
- Process one folder at a time through the pipeline
- Within each folder, parallelize file reading and image processing
- Simpler to reason about; callbacks are naturally folder-ordered
- Memory bounded by single folder's files × concurrency

**Option B: Global pipeline across all folders**
- All folders feed into a single pipeline
- Maximum throughput but harder to provide folder-level progress
- Callbacks must be reordered or batched by folder
- Higher memory pressure (files from multiple folders in flight)

**Recommendation:** Start with Option A. It's safer, easier to test, and still provides 10-20× improvement. Option B can be added later if needed.

### Backpressure Strategy

```
If SSD can deliver 2GB/s and each file is 5MB:
  → Stage 1 can produce 400 files/second

If each image takes 100ms to process:
  → Stage 2 with 8 cores consumes 80 files/second

Mismatch! Stage 1 will fill the channel in ~0.2 seconds.
Bounded channel (size 16) blocks Stage 1, limiting memory to:
  16 × 5MB = 80MB in the read buffer (acceptable)
```

The bounded channel naturally throttles the fast producer to match the slow consumer.

---

## Folder Splitting vs. Asset Splitting

### Folder Splitting (Inter-Folder Parallelism)

**Concept:** Process multiple top-level folders simultaneously, each in its own pipeline.

**Pros:**
- Good when you have many folders with few files each
- Natural isolation (no shared per-folder state)
- Easy to implement with `Parallel.ForEach` over folders

**Cons:**
- Doesn't help when one folder has 50K images (common with camera imports)
- Memory multiplied by number of concurrent folders
- SQLite write contention between folder pipelines
- Harder to provide coherent progress feedback

### Asset Splitting (Intra-Folder Parallelism)

**Concept:** Within a single folder, process multiple assets simultaneously through the pipeline.

**Pros:**
- Helps with the common case (large folders from camera dumps)
- Natural fit for the Channel pipeline pattern
- Single writer to SQLite (no contention)
- Bounded memory via channel capacity

**Cons:**
- Need to handle per-folder state carefully
- Callback ordering within a folder becomes non-deterministic

### Hybrid Approach (Recommended)

```
For each root folder path (sequential):
    For each subfolder (sequential — maintains deterministic progress):
        Process all assets in THIS folder via parallel pipeline
        Wait for pipeline to drain
        Move to next subfolder
```

This gives:
- **Intra-folder parallelism** for the CPU-heavy work (hash + thumbnail)
- **Inter-folder sequential ordering** for predictable progress and callbacks
- **Bounded memory** (only one folder's pipeline active at a time)
- **No SQLite contention** (single writer within the pipeline)

---

## Memory Management

### Memory Budget

For 100K images with an 8GB RAM system:

| Component | Current | After Pipeline | Notes |
|-----------|---------|---------------|-------|
| In-memory asset objects | ~100MB | ~100MB | Same (needed for catalog state) |
| Read buffer (Stage 1) | N/A | ~80MB max | 16 slots × 5MB avg |
| Process buffer (Stage 2) | N/A | ~160MB max | 32 slots × 5MB avg |
| Thumbnail cache | ~50MB | ~50MB | Same |
| Peak during processing | ~200MB | ~300MB | Controlled by channel bounds |

### Avoiding LOH Fragmentation

1. **ArrayPool for file buffers:**
   ```csharp
   byte[] buffer = ArrayPool<byte>.Shared.Rent(fileSize);
   try { /* use buffer */ }
   finally { ArrayPool<byte>.Shared.Return(buffer); }
   ```

2. **Pre-sized MemoryStream for thumbnails:**
   ```csharp
   // Thumbnails are typically 5-50KB — avoid growing the buffer
   using MemoryStream ms = new(capacity: 32768);
   ```

3. **Pipeline cleanup:** Each stage returns pooled buffers after processing, before passing results downstream.

---

## Error Handling in Parallel Context

### Per-Item Errors (Don't Kill the Pipeline)

```csharp
// In Stage 2 worker:
try
{
    ProcessedAsset? result = ProcessItem(item);
    if (result != null)
    {
        await output.WriteAsync(result, token);
    }
    else
    {
        // Log and skip corrupted/invalid files
        await errors.WriteAsync(new ProcessingError(item.FilePath, "Invalid image"), token);
    }
}
catch (Exception ex) when (ex is not OperationCanceledException)
{
    _logger.LogError(ex, "Failed to process {FilePath}", item.FilePath);
    // Continue with next item — don't kill the pipeline
}
```

### Fatal Errors (Cancel the Pipeline)

```csharp
// Disk full, database locked permanently, etc.
catch (IOException ex) when (IsDiskFull(ex))
{
    _linkedCts.Cancel(); // Propagates to all stages
    throw;
}
```

### Ordered Error Reporting

Errors are collected in a separate channel and reported to the callback in-order after each batch completes. The UI receives a summary: "5 corrupted images skipped in folder X."

---

## Cancellation Strategy

```csharp
// All stages share a linked cancellation token:
CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(externalToken);

// When user clicks Cancel:
// 1. externalToken is cancelled
// 2. linkedCts propagates to all stages
// 3. Channel.ReadAllAsync() throws OperationCanceledException
// 4. Each stage catches and completes its output channel
// 5. Downstream stages drain and complete
// 6. await Task.WhenAll() returns after all stages finish gracefully
```

**Graceful shutdown:** Stage 3 (persistence) commits any partial batch before exiting, ensuring no data loss.

---

## Testing Strategy

### Unit Tests

1. **Pipeline flow:** Mock all stages, verify items flow through channels correctly
2. **Backpressure:** Verify slow consumer blocks fast producer
3. **Cancellation:** Verify all stages stop promptly on cancellation
4. **Error isolation:** Verify one bad item doesn't kill the pipeline
5. **Batch commit:** Verify partial batch is committed on completion/cancellation

### Integration Tests

1. **Thread safety:** Run pipeline with `ThreadSanitizer` or stress test with many concurrent items
2. **Data integrity:** Verify SQLite state matches expected after parallel catalog
3. **Memory bounds:** Verify peak memory stays within expected limits using `GC.GetTotalMemory()`
4. **Determinism:** Same input produces same catalog state regardless of scheduling order

### Benchmarks

```csharp
[Benchmark]
public async Task CatalogFolder_Sequential() { /* current implementation */ }

[Benchmark]
public async Task CatalogFolder_Pipeline_4Workers() { /* new pipeline, 4 workers */ }

[Benchmark]
public async Task CatalogFolder_Pipeline_8Workers() { /* new pipeline, 8 workers */ }

[Benchmark]
[Arguments(1, 10, 50, 100)]
public async Task CatalogFolder_Pipeline_VaryingBatchSize(int batchSize) { /* parametric */ }
```

---

## Migration Path (Phased Rollout)

### Phase 1: Foundation (No Behavioral Change)

**Goal:** Prepare infrastructure without changing the cataloging flow.

1. Create `IThumbnailGenerator` backed by ImageMagick (replaces WPF pipeline for cataloging) — **DONE**
2. Add `PipelineOptions` configuration to `appsettings.json`
3. Add `Channel<T>` infrastructure classes — **DONE**
4. Add batch asset+thumbnail write path — **DONE** via atomic persistence context batch APIs
5. Unit test all new components in isolation

**Risk:** Zero — new code runs alongside old code, old path still active.

### Phase 2: Single-Folder Pipeline

**Goal:** Replace the inner loop of `CatalogAssets()` with a pipeline for ONE folder.

1. Implement `ParallelFileReader` (Stage 1) — **DONE**
2. Implement `ParallelImageProcessor` (Stage 2) using new `IThumbnailGenerator` — **DONE**
3. Implement `BatchedPersistenceWriter` (Stage 3) — **DONE**
4. Wire stages together in a new `CatalogFolderPipeline` class — **DONE**
5. Replace `CatalogNewAssets()` call with pipeline invocation — **DONE**
6. Keep `CatalogUpdatedAssets()` and `CatalogDeletedAssets()` sequential initially — **DONE**

**Risk:** Medium — new processing path, but isolated to one code path. Feature-flag it:
```json
{ "Pipeline": { "Enabled": true, "ReadConcurrency": 4, "ProcessConcurrency": 0 } }
```

### Phase 3: Full Pipeline Integration

**Goal:** All cataloging operations (new, updated, deleted) flow through the pipeline.

1. Handle updated assets through pipeline (delete old + create new in one batch) — **DONE**
2. Handle deleted assets through batched persistence — **DONE**
3. Replace folder-level callbacks with batched progress reporting
4. Remove old sequential path (keep behind feature flag for rollback)
5. Optimize `ReadCatalog()` startup with parallel file stats

**Risk:** Higher — full behavioral change. Requires thorough integration testing.

### Phase 4: Advanced Optimizations

**Goal:** Squeeze remaining performance.

1. Streaming hash (avoid full-file `byte[]` when not using PHash) — **DONE for standalone file-hash APIs**
   (catalog still reuses loaded image bytes when downstream processing already needs them)
2. Adaptive concurrency (detect SSD vs HDD, adjust `ReadConcurrency`)
3. Memory-mapped file reading for very large files — **DONE for thumbnail storage evaluation** (separate
   thumbnail files were deferred; SQLite BLOB reads were optimized instead)
4. Background pre-fetching (read next folder's files while processing current)
5. Optional inter-folder parallelism for users with many small folders

---

## Configuration

### New appsettings.json Entries

```json
{
  "appsettings": {
    "Pipeline": {
      "Enabled": true,
      "ReadConcurrency": 4,
      "ProcessConcurrency": 0,
      "ReadBufferSize": 16,
      "ProcessBufferSize": 32,
      "PersistBufferSize": 64,
      "PersistBatchSize": 50,
      "MaxBatchDelayMs": 500,
      "UseImageMagickThumbnails": true
    }
  }
}
```

| Setting | Default | Description |
|---------|---------|-------------|
| `Enabled` | `true` | Feature flag for pipeline mode |
| `ReadConcurrency` | `4` | Parallel file reads (4 for SSD, 1-2 for HDD) |
| `ProcessConcurrency` | `0` | Image processing workers (0 = ProcessorCount) |
| `ReadBufferSize` | `16` | Channel capacity for read results |
| `ProcessBufferSize` | `32` | Channel capacity for processed assets |
| `PersistBufferSize` | `64` | Channel capacity for persistence queue |
| `PersistBatchSize` | `50` | Assets per SQLite transaction |
| `MaxBatchDelayMs` | `500` | Max time before flushing partial batch |
| `UseImageMagickThumbnails` | `true` | Use ImageMagick instead of WPF for thumbnails |

---

## Expected Performance Results

### Theoretical Throughput

With the pipeline, the bottleneck becomes the **slowest stage**:

| Stage | Throughput (per worker) | Workers | Aggregate |
|-------|------------------------|---------|-----------|
| File Read (SSD) | 400 files/s | 4 | 1600 files/s |
| Image Process (PHash) | 10 files/s | 8 | 80 files/s |
| Image Process (SHA512) | 100 files/s | 8 | 800 files/s |
| Persistence (batched) | 2000 files/s | 1 | 2000 files/s |

**Bottleneck:** Image processing (Stage 2)
- With SHA512: ~800 files/sec → **100K images in ~2 minutes**
- With PHash: ~80 files/sec → **100K images in ~20 minutes**

**Current sequential:** ~5-10 files/sec → 100K in 3-6 hours

**Improvement:** 10-20× for SHA512, 8-10× for PHash.

### Real-World Expectations (Conservative)

Accounting for OS scheduling, GC pauses, and I/O variability:

| Scenario | Current | Pipeline | Improvement |
|----------|---------|----------|-------------|
| 100K JPEGs (5MB avg, SHA512) | ~5 hours | ~20-30 min | 10-15× |
| 100K JPEGs (5MB avg, PHash) | ~10 hours | ~45-60 min | 10-12× |
| 10K RAW files (25MB avg, SHA512) | ~2 hours | ~15-20 min | 6-8× |
| Incremental (100 new files) | ~30 sec | ~3-5 sec | 6-10× |

---

## Risks and Mitigations

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| Race condition in shared state | Medium | High | Pipeline architecture eliminates shared mutable state |
| Memory explosion from unbounded parallelism | Low | High | Bounded channels with backpressure |
| SQLite corruption from concurrent writes | Low | Critical | Single writer (Stage 3), WAL mode |
| WPF thread affinity violation | Medium | High | Replace WPF thumbnail with ImageMagick for pipeline |
| Callback ordering breaks UI assumptions | Medium | Medium | Folder-sequential processing preserves order |
| ImageMagick thread-safety issues | Low | Medium | Each worker owns its MagickImage instance |
| Harder debugging/reproduction | Medium | Low | Structured logging with correlation IDs per pipeline item |
| Feature flag complexity | Low | Low | Clean separation of old/new paths |

---

## Dependencies

### New NuGet Packages Required

None — all required types are in the BCL:
- `System.Threading.Channels` (in `System.Runtime`)
- `System.Buffers` (ArrayPool, in `System.Runtime`)
- `ImageMagick` (already referenced via `Magick.NET`)

### Affected Projects

| Project | Changes |
|---------|---------|
| `PhotoManager.Domain` | New pipeline interfaces, `PipelineOptions`, refactored `CatalogAssetsService` |
| `PhotoManager.Infrastructure` | New `ImageMagickThumbnailGenerator`, refactored `AssetCreationService` |
| `PhotoManager.Persistence` | Batch thumbnail write, dedicated write connection |
| `PhotoManager.Common` | Streaming hash helper methods |
| `PhotoManager.Tests` | Pipeline unit/integration tests |
| `PhotoManager.Benchmarks` | Pipeline throughput benchmarks |

---

## Summary

Multi-threading is the single most impactful improvement for PhotoManager's cataloging performance. The **staged pipeline architecture** provides:

1. **Safety:** Bounded channels prevent memory explosion; single writer prevents SQLite contention
2. **Throughput:** 10-20× improvement by overlapping I/O with CPU work
3. **Maintainability:** Each stage is independently testable and replaceable
4. **Incremental adoption:** Feature-flagged, phased rollout with fallback to sequential mode
5. **Scalability:** Adding more CPU cores directly improves Stage 2 throughput

The recommended approach is **intra-folder pipeline parallelism with inter-folder sequential ordering**, which gives the best trade-off between performance, complexity, and correctness.
