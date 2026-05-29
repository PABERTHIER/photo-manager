# Performance Bottleneck Analysis

## Executive Summary

This document identifies the remaining performance bottlenecks in PhotoManager after the migration from the binary database to SQLite. While the persistence layer has been modernized, the application still has significant throughput issues when dealing with 100K+ images due to sequential processing patterns, excessive I/O syscalls, memory pressure from full-file reads, and per-operation database overhead.

---

## Critical Bottlenecks (High Impact)

### 1. Sequential Asset Processing in CatalogAssetsService — **DONE**

Image create/update cataloging now uses the bounded channel pipeline with batched persistence; video extraction
remains on the existing sequential path.

**Location:** `PhotoManager.Domain/CatalogAssetsService.cs` (lines 380-425)

**Problem:**
The `CatalogAssets` method processes every file **one at a time**, in a single thread. For each new image, the full pipeline executes sequentially:
1. Read entire file into `byte[]`
2. Validate image format
3. Extract EXIF orientation
4. Compute hash (SHA512 or PHash)
5. Generate thumbnail (decode → resize → encode)
6. Write asset record to SQLite
7. Write thumbnail BLOB to SQLite

With 100,000 images averaging 5MB each, this means ~500GB of sequential I/O with no overlap between CPU work (hashing, thumbnail generation) and disk I/O (file reads, DB writes).

**Impact:** For a 100K-image library, the initial catalog could take **10+ hours** instead of ~1 hour with proper parallelism.

**Fix:**
- Introduce a producer-consumer pipeline using `Channel<T>` with bounded capacity
- Parallelize file reading (I/O-bound) separately from image processing (CPU-bound) and DB writes (I/O-bound)
- Batch SQLite writes using the existing `UpsertMany()` method
- See the Multi-Threading Plan document for detailed implementation

---

### 2. Full-File Read into Memory (byte[]) for Every Asset

**Location:** `PhotoManager.Infrastructure/FileOperationsService.cs` (line 51) + `PhotoManager.Domain/AssetCreationService.cs` (line 47)

**Problem:**
```csharp
byte[] imageBytes = fileOperationsService.GetFileBytes(imagePath);
// Which calls: File.ReadAllBytes(filePath)
```

Every image file (JPEG 5-20MB, RAW 25-80MB) is loaded **entirely** into a `byte[]` on the Large Object Heap (LOH). This causes:
- **LOH fragmentation** leading to Gen2 GC collections
- **Peak memory** proportional to `batchSize × average_file_size` if parallelized naively
- **Double memory** when the hash is computed (the array must survive until thumbnail + hash are done)

**Impact:** With RAW images, a single batch of 100 assets could consume 2-8GB of RAM temporarily. GC pauses become frequent and long.

**Fix:**
1. **For hash calculation (SHA512/MD5):** Use streaming hash with `FileStream` and `IncrementalHash`:
   ```csharp
   using FileStream fs = new(path, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, FileOptions.SequentialScan);
   byte[] hash = SHA512.HashData(fs);
   ```
   This eliminates the need to load the entire file for non-PHash scenarios.

2. **For thumbnail generation:** Read only what WPF/MagickImage needs. `BitmapImage` can work directly from a `FileStream` (via `UriSource` or `StreamSource`), and MagickImage supports streaming reads.

3. **For PHash:** Already uses file path (ImageMagick reads the file itself), so `imageBytes` is only needed for the thumbnail pipeline. Split the pipeline so PHash and thumbnail generation share a single file read.

4. **Memory pooling:** Use `ArrayPool<byte>.Shared` for the small buffer needed for EXIF header reading (~64KB), and `MemoryMappedFile` for large files when multiple operations need the same data.

---

### 3. SQLite Per-Operation Connection & PRAGMA Overhead — **DONE** for catalog asset/thumbnail writes

**Location:** `PhotoManager.Persistence/Sqlite/SqliteConnectionFactory.cs` (lines 21-57)

**Problem:**
Every single database operation opens a new connection and executes 5 PRAGMAs:
```sql
PRAGMA journal_mode=WAL;
PRAGMA synchronous=NORMAL;
PRAGMA busy_timeout=5000;
PRAGMA temp_store=MEMORY;
PRAGMA foreign_keys=ON;
```

In `AddAssetCore()`, adding one asset performs:
1. `Assets.Upsert(asset)` → Open + PRAGMAs + INSERT + Close
2. `Thumbnails.Upsert(...)` → Open + PRAGMAs + INSERT + Close

That's **2 connection opens + 10 PRAGMA executions + 2 round trips** per asset. With 100K assets, that's 200K connection opens and 1M PRAGMA statements.

**Impact:** Benchmarks show PRAGMA execution adds ~0.5ms per connection, so 200K connections = ~100 seconds of pure overhead. Each connection also requires SQLite to acquire and release WAL locks.

**Fix:**
1. **Remove persistent PRAGMAs from per-connection logic:**
   - `journal_mode=WAL` only needs to be set once (it persists in the DB file)
   - Move it to `SqliteSchema.EnsureCreated()` or check it once at initialization

2. **Reduce PRAGMAs to only session-level ones:**
   ```csharp
   // These are actually per-connection:
   PRAGMA synchronous=NORMAL;
   PRAGMA busy_timeout=5000;
   PRAGMA temp_store=MEMORY;
   PRAGMA foreign_keys=ON;
   ```
   But with connection pooling enabled (`Pooling = true`), Microsoft.Data.Sqlite reuses connections. PRAGMAs may already be set on a reused connection. Check before executing.

3. **Batch asset writes:** Accumulate 50-100 assets, then write them all in a single transaction using the existing `UpsertMany()`:
   ```csharp
   // Instead of:
   foreach (asset in batch) { _persistence.Assets.Upsert(asset); }
   
   // Use:
   _persistence.Assets.UpsertMany(batchAssets);
   ```
   This reduces 100 connection opens → 1, and wraps everything in one transaction (one WAL sync).

4. **Create a dedicated write connection** that stays open for the duration of cataloging, avoiding pool churn during the hot path.

---

### 4. Startup Full-Catalog Load with Per-Asset File Stats

**Location:** `PhotoManager.Infrastructure/AssetRepository.cs` (lines 488-540)

**Problem:**
`ReadCatalog()` at startup:
1. Loads ALL assets from SQLite into memory (`GetAll()`)
2. For each asset, calls `_imageMetadataService.UpdateAssetFileProperties(asset)` which does:
   ```csharp
   FileInfo info = new(asset.FullPath);
   asset.FileProperties = new() { Size = info.Length, Creation = info.CreationTime, Modification = info.LastWriteTime };
   ```

With 100K assets, that's **100K `FileInfo` syscalls** at startup, each of which involves:
- Path normalization
- Security check
- Filesystem metadata read (NTFS MFT lookup)

**Impact:** On an SSD, each `FileInfo` takes ~0.1-0.5ms. For 100K assets: **10-50 seconds just for file stats at startup**. On HDD or network drives, this becomes minutes.

**Fix:**
1. **Lazy file property loading:** Don't stat files at startup. Instead, load file properties on-demand when a folder is actually accessed (opened in the UI or cataloged).

2. **Parallel file stats with bounded parallelism:** If eager loading is required, use `Parallel.ForEach` with `MaxDegreeOfParallelism = Environment.ProcessorCount`:
   ```csharp
   Parallel.ForEach(assets, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, asset =>
   {
       _imageMetadataService.UpdateAssetFileProperties(asset);
   });
   ```

3. **Store file properties in SQLite:** Persist `Size`, `CreationTime`, and `LastWriteTime` in the Assets table. Only re-stat files during catalog sync (when you need to detect changes). This completely eliminates startup stats for unchanged libraries.

4. **Batch directory enumeration:** Instead of stating individual files, use `Directory.GetFiles()` + single `FindFirstFile`/`FindNextFile` per directory (which the OS optimizes internally). Compare modification times from directory enumeration vs. stored timestamps.

---

### 5. CatalogChangeCallback List Cloning

**Location:** `PhotoManager.Domain/CatalogAssetsService.cs` (multiple sites)

**Problem:**
Every callback invocation clones the entire folder's asset list:
```csharp
callback(new()
{
    CataloguedAssetsByPath = [.. _cataloguedAssetsByPath], // Full list clone every time!
    ...
});
```

For a folder with 1,000 images, processing 500 new assets means cloning a growing list 500 times:
- Clone 1: 1 item
- Clone 2: 2 items
- ...
- Clone 500: 500 items
- Total: ~125,250 Asset references copied

**Impact:** O(n²) allocations per folder. For a folder with 10,000 assets, this creates ~50M reference copies and ~50,000 array allocations.

**Fix:**
1. **Pass a read-only snapshot only when the UI actually needs it:**
   ```csharp
   // Instead of cloning on every callback, pass a lazy accessor:
   CataloguedAssetsByPath = _cataloguedAssetsByPath.AsReadOnly()
   ```
   Or better, don't include the list in every callback—only in `AssetCreated`/`AssetDeleted` events where the UI needs to refresh.

2. **Use an immutable collection** (`ImmutableArray<Asset>`) that shares structure. Adding one element creates a new array but reuses most of the underlying storage.

3. **Debounce UI updates:** The UI doesn't need to re-render after every single asset. Batch callbacks: send a batch summary every N assets or every T milliseconds.

---

## Significant Bottlenecks (Medium Impact)

### 6. Per-Folder UpdateAssetsFileProperties During Cataloging

**Location:** `PhotoManager.Domain/CatalogAssetsService.cs` (line 280)

**Problem:**
```csharp
_imageMetadataService.UpdateAssetsFileProperties(_cataloguedAssetsByPath);
```
This is called for every folder visited, stating ALL already-cataloged assets in that folder to detect modifications (file creation/modification times). This means re-stating files that were just cataloged in the same run.

**Impact:** With 100 folders × 1,000 assets/folder = 100K unnecessary stats on top of the initial catalog work.

**Fix:**
1. **Use directory change timestamp:** Before stating individual files, check if the directory's `LastWriteTime` has changed since the last catalog run. If not, skip the per-file stats entirely.

2. **Store `LastWriteTime` in DB per folder:**
   ```sql
   ALTER TABLE Folders ADD COLUMN LastCatalogedAt INTEGER;
   ```
   Compare `Directory.GetLastWriteTime(folderPath)` vs. stored timestamp. If unchanged, skip update detection for that folder.

3. **Use FileSystemWatcher for incremental detection:** Register a watcher on the root folder(s) and only re-stat files that actually changed between catalog runs.

---

### 7. PHash Calculation (6× Slower than SHA512)

**Location:** `PhotoManager.Common/HashingHelper.cs` (lines 31-58)

**Problem:**
The code comment states: "Performances are decreased by 6 times with CalculatePHash."
PHash involves:
1. Loading the entire image into MagickImage (ImageMagick decode)
2. Resizing to 32×32
3. Converting to grayscale
4. Computing perceptual hash

This is CPU-intensive and cannot be streamed. Each PHash takes ~50-200ms depending on image size.

**Impact:** With PHash enabled, 100K images × 100ms = **~2.8 hours** just for hash computation.

**Fix:**
1. **Parallelize PHash computation:** PHash is CPU-bound with no shared state. Use `Parallel.ForEach` or a dedicated `Channel<T>` pipeline stage with `ProcessorCount` workers.

2. **Skip PHash for unchanged files:** Store the hash in DB and only recalculate when `LastWriteTime` changes.

3. **Use native SIMD-optimized PHash:** Consider replacing the MagickImage-based PHash with a purpose-built DCT-based implementation using `System.Numerics.Vector<T>` or hardware intrinsics. The resize + grayscale + DCT can be done without a full image decode.

4. **Hybrid approach:** Compute SHA512 first (fast, streaming), store it. Compute PHash lazily/in background for duplicate detection only. This decouples cataloging speed from duplicate detection accuracy.

---

### 8. GetCataloguedAssets() Full Materialization + Sort — **DONE**

**Location:** `PhotoManager.Infrastructure/AssetRepository.cs` (lines 260-273)

**Problem:**
```csharp
public Asset[] GetCataloguedAssets()
{
    Asset[] assets = [.. _assetsByFolderId.Values.SelectMany(static inner => inner.Values)];
    // TODO: Why do we need this sort ?
    Array.Sort(assets, ...);
    return assets;
}
```

This materializes ALL assets from the in-memory dictionaries into a single array, then sorts it. With 100K assets:
- 100K object references materialized via LINQ `SelectMany`
- O(n log n) sort = ~1.7M comparisons with two string comparisons each

**Impact:** ~200-500ms per call. This is called by `FindDuplicatedAssetsService.GetDuplicatedAssets()`, which may be invoked from the UI.

**Fix:**
1. **Remove the sort if unnecessary** (the TODO comment suggests uncertainty about its purpose). Duplicate detection doesn't require sorted input. If the UI needs sorted display, sort only the subset being displayed.

2. **If sort is needed, maintain a sorted structure:** Keep a parallel sorted data structure (e.g., `SortedList<string, Asset>`) updated on add/remove, avoiding the full-sort cost.

3. **Return an `IReadOnlyCollection<Asset>` view** that doesn't copy. Since `ConcurrentDictionary.Values` already returns a snapshot, flatten without the intermediate array.

---

### 9. Thumbnail Cache Too Small (Default: 5 Folders)

**Location:** `PhotoManager.Infrastructure/AssetRepository.cs` (line 69) + `appsettings.json` (ThumbnailsDictionaryEntriesToKeep: 5)

**Problem:**
The `LruCache<Guid, ConcurrentDictionary<string, byte[]>>` holds thumbnails for only 5 folders. When the user navigates between folders (or during cataloging), the cache is evicted and thumbnails must be re-read from SQLite BLOBs.

Each cache miss triggers:
```csharp
Dictionary<string, byte[]> savedThumbnails = _persistenceContext.Thumbnails.GetByFolderId(folder.Id);
```
This reads ALL thumbnails for that folder (potentially hundreds of BLOBs totaling 10-50MB).

**Impact:** Navigating back and forth between 6+ folders causes repeated SQLite BLOB reads. Each re-read of a folder with 1,000 thumbnails (avg 10KB each) = 10MB of I/O per cache miss.

**Fix:**
1. **Increase the default cache size:** For a machine with 8GB+ RAM, 50-100 folder entries is reasonable. Each folder's thumbnails might be 5-50MB, so 100 folders = 500MB-5GB. Make it configurable based on available memory.

2. **Individual thumbnail caching:** Instead of caching entire folders, cache individual thumbnails with a total byte budget:
   ```csharp
   LruCache<(Guid folderId, string fileName), byte[]> _thumbnailCache;
   // With a max total size of 500MB
   ```

3. **Memory-mapped file for thumbnail storage:** Store thumbnails in a separate file (not SQLite BLOB) using memory-mapped I/O. The OS handles caching via the page cache, eliminating the need for application-level LRU.

4. **Lazy thumbnail loading:** Only load thumbnails when the UI actually displays them (virtual scrolling). Currently `GetAssetsByPath` eagerly loads all thumbnails for the folder.

---

### 10. FindDuplicatedAssetsService - Full File Existence Check

**Location:** `PhotoManager.Domain/FindDuplicatedAssetsService.cs` (lines 27-31, 48-53)

**Problem:**
```csharp
List<Asset> validAssets = assets
    .AsParallel()
    .AsOrdered()
    .Where(asset => fileOperationsService.FileExists(asset.FullPath))
    .ToList();
```

This checks `File.Exists()` for EVERY asset in the catalog before duplicate detection. With 100K assets on an SSD, that's 100K syscalls (~10-50 seconds). On a network drive, this could take minutes.

**Impact:** The duplicate detection feature becomes unusable for large libraries. Users might wait 1-2 minutes before any results appear.

**Fix:**
1. **Trust the catalog:** If the file was recently cataloged (within the current session), skip the existence check. Use a `lastVerifiedAt` timestamp per asset.

2. **Batch existence checks per directory:** Use `Directory.GetFiles()` per folder and intersect with known assets—one syscall per folder instead of per file.

3. **Remove `.AsOrdered()`:** The results don't need to maintain insertion order. `AsOrdered()` forces PLINQ to synchronize partition results, reducing parallelism benefit by 30-50%.

4. **Filter in SQL:** If files have been deleted, they would have been detected during catalog runs. Trust the in-memory state unless explicitly refreshing.

---

## Minor Bottlenecks (Lower Impact, Easy Wins)

### 11. GetFileNames Allocates Unnecessarily

**Location:** `PhotoManager.Infrastructure/FileOperationsService.cs` (line 46-48)

**Problem:**
```csharp
public string[] GetFileNames(string directory)
{
    string[] files = Directory.GetFiles(directory);
    return [.. files.Select(f => Path.GetFileName(f))];
}
```

This allocates TWO arrays: one from `Directory.GetFiles()` (full paths), and one from the LINQ projection (file names only). For a folder with 5,000 files, that's 10,000 string allocations.

**Fix:**
Use `DirectoryInfo.EnumerateFiles()` with just the name:
```csharp
public string[] GetFileNames(string directory)
{
    return [.. new DirectoryInfo(directory).EnumerateFiles().Select(fi => fi.Name)];
}
```
Or better, use `Directory.GetFiles()` and extract names in-place:
```csharp
public string[] GetFileNames(string directory)
{
    string[] files = Directory.GetFiles(directory);
    for (int i = 0; i < files.Length; i++)
    {
        files[i] = Path.GetFileName(files[i]);
    }
    return files;
}
```
This eliminates the second array allocation entirely.

---

### 12. ExifHelper Double Image Validation

**Location:** `PhotoManager.Domain/AssetCreationService.cs`

**Problem:**
The image is validated (`IsValidGdiPlusImage` / `IsValidHeic`) and THEN the full bytes are processed for thumbnail. The validation itself decodes partial image headers but the subsequent `LoadBitmapThumbnailImage` reads the bytes again from scratch.

**Fix:**
Combine validation with the first decode pass. WPF's `BitmapImage` will throw `NotSupportedException` for invalid images anyway—wrap the thumbnail creation in a try/catch instead of pre-validating.

---

### 13. Reactive Subject Overhead for Single-Subscriber Pattern — **DONE**

**Location:** `PhotoManager.Infrastructure/AssetRepository.cs` (lines 37-40, 253, 563)

**Problem:**
`Subject<Unit>` + `IObservable<Unit>` machinery for a simple event that has exactly one subscriber (`CatalogAssetsService`). Every `OnNext` call:
1. Acquires the Subject's internal lock
2. Iterates subscribers
3. Invokes the callback
4. The callback is suppressed during cataloging anyway (`_suppressReactiveUpdates`)

**Fix:**
Replace with a simple `Action` callback or `event EventHandler` pattern. Rx adds allocations and virtual dispatch for what is essentially a synchronous notification.

---

### 14. BitmapImage for Thumbnails Stored as byte[] Twice — **DONE**

**Location:** `PhotoManager.Domain/AssetCreationService.cs` (lines 92-93, etc.)

**Problem:**
The thumbnail pipeline:
1. Loads original image bytes into `BitmapImage` (WPF decode)
2. Creates a thumbnail `BitmapImage` (resize via `DecodePixelWidth/Height`)
3. Re-encodes the `BitmapImage` back to `byte[]` (JPEG/PNG encode)
4. Stores the `byte[]` in SQLite

The WPF decode → re-encode cycle is expensive and loses quality (JPEG re-compression). It also pins the thread to STA (WPF requirement).

**Fix:**
Use ImageMagick (already a dependency) for all thumbnail operations:
```csharp
using MagickImage image = new(imageBytes);
image.Resize(width, height);
byte[] thumbnailBytes = image.ToByteArray(MagickFormat.Jpeg);
```
Benefits:
- No WPF STA thread requirement
- Single decode → resize → encode pipeline
- Better JPEG quality control
- Thread-safe (each MagickImage instance is independent)
- HEIC, PNG, GIF all handled uniformly
- Phase 3 also removed the UI-side `BitmapImageData` display bridge; the Avalonia UI now creates
  `Avalonia.Media.Imaging.Bitmap` only when an image is displayed.

---

### 15. Connection String Rebuilt on Every Open()

**Location:** `PhotoManager.Persistence/Sqlite/SqliteConnectionFactory.cs` (lines 31-39)

**Problem:**
A new `SqliteConnectionStringBuilder` is created on every `Open()` call. While cheap, with 200K+ calls during a full catalog run, the allocations add up.

**Fix:**
Build the connection string once during `Initialize()` and cache it:
```csharp
private string _connectionString = string.Empty;

public void Initialize(string databasePath)
{
    DatabasePath = databasePath;
    _connectionString = new SqliteConnectionStringBuilder
    {
        DataSource = databasePath,
        Mode = SqliteOpenMode.ReadWriteCreate,
        Cache = SqliteCacheMode.Default,
        Pooling = true,
        ForeignKeys = true,
        DefaultTimeout = 30
    }.ConnectionString;
}
```

---

### 16. Avalonia UI Asset Removal Collection Churn — **DONE**

**Location:** `PhotoManager.UI.Avalonia/ViewModels/ApplicationViewModel.cs`

**Problem:**
The migrated UI removed deleted assets with repeated `ObservableCollection.Remove()` calls. For large selections,
that performs one linear search and one collection notification per removed asset.

**Fix:**
`RemoveAssets()` now builds a `HashSet<Asset>`, creates the remaining asset collection in one pass, disposes image
data for removed assets, and raises the observable collection update once.

---

## Performance Improvement Priority Matrix

| # | Bottleneck | Impact | Effort | Priority |
|---|-----------|--------|--------|----------|
| 1 | Sequential asset processing | Critical | High | P0 — **DONE** |
| 2 | Full-file read into byte[] | Critical | Medium | P0 |
| 3 | Per-operation SQLite connections | High | Medium | P1 — **DONE for catalog writes** |
| 4 | Startup file stats (100K) | High | Low | P1 |
| 5 | Callback list cloning (O(n²)) | High | Low | P1 |
| 6 | Per-folder file stats during catalog | Medium | Low | P2 |
| 7 | PHash computation (6× slower) | High (if enabled) | Medium | P2 |
| 8 | GetCataloguedAssets() sort | Medium | Low | P2 — **DONE** |
| 9 | Thumbnail cache too small | Medium | Low | P2 |
| 10 | Duplicate detection file checks | Medium | Low | P2 |
| 11 | GetFileNames double allocation | Low | Low | P3 |
| 12 | Double image validation | Low | Medium | P3 |
| 13 | Reactive Subject overhead | Low | Low | P3 — **DONE** |
| 14 | WPF thumbnail pipeline | Medium | High | P3 — **DONE** |
| 15 | Connection string rebuild | Low | Low | P3 |
| 16 | Avalonia UI asset-removal collection churn | Medium | Low | P2 — **DONE** |

---

## Estimated Performance Gains

| Scenario (100K images) | Current (Est.) | After Fixes (Est.) | Improvement |
|------------------------|---------------|-------------------|-------------|
| Initial full catalog | 8-12 hours | 30-60 minutes | 10-20× |
| Incremental catalog (100 changes) | 30-60 seconds | 2-5 seconds | 10-15× |
| Startup (ReadCatalog) | 30-60 seconds | 1-3 seconds | 15-30× |
| Duplicate detection | 2-5 minutes | 5-15 seconds | 10-20× |
| Folder navigation (UI) | 1-3 seconds | <200ms | 5-15× |

---

## Recommended Implementation Order

### Phase 1: Quick Wins (P1 low-effort items)
1. Fix callback list cloning (replace with read-only view)
2. Cache connection string in factory
3. Lazy file property loading at startup (defer to folder access)
4. Increase thumbnail cache size to 50+ entries
5. Remove unnecessary sort in `GetCataloguedAssets()` (investigate the TODO) — **DONE**

### Phase 2: Batching & I/O Optimization
1. Batch asset writes (accumulate, use `UpsertMany` + batch thumbnail writes) — **DONE**
2. Eliminate redundant PRAGMAs (set WAL once, cache other PRAGMAs on pooled connections)
3. Streaming hash computation (avoid full-file `byte[]` for SHA512/MD5)
4. Per-folder change detection via directory timestamp

### Phase 3: Parallelism (See Multi-Threading Plan)
1. Implement pipeline architecture in CatalogAssetsService — **DONE**
2. Parallelize PHash computation
3. Parallelize startup file stats (if not deferred)
4. Parallelize duplicate detection without `.AsOrdered()`

### Phase 4: Architecture Evolution
1. Replace WPF thumbnail pipeline with ImageMagick-only pipeline — **DONE** (IImageData/ImageRotation
   replace WPF types in all interfaces; BitmapHelper rewritten with SkiaSharp + MagickImage HEIC
   fallback; Avalonia UI display now uses `Bitmap` instead of WPF `BitmapImage`)
2. Store file properties in SQLite (eliminate startup stats entirely)
3. Consider memory-mapped thumbnails or separate thumbnail file
4. Implement FileSystemWatcher for real-time change detection
5. Remove Windows-specific infrastructure blockers — **DONE** (FFmpeg runtimes, file-lock single-instance,
   portable build/coverage cleanup, and path expansion)
