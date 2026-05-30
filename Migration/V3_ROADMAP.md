# V3 Roadmap

V3 focuses on moving the UI to a cross-platform stack, adding multi-threading for faster processing, and optimizing performance across the app.

1. Phase 1 - Remove WPF Types from Non-UI Layers
   - 1.1 Replace Rotation enum everywhere — **DONE**
   - 1.2 Replace BitmapImage in Domain interfaces — **DONE**
   - 1.3 Rewrite BitmapHelper.cs with SkiaSharp — **DONE**
   - 1.4 Rewrite ExifHelper.cs EXIF reading (unify on SkiaSharp) — **DONE**
   - 1.5 Remove System.Drawing usage — **DONE**
   - 1.6 Remove WPF GlobalUsings from non-UI projects — **DONE**
   - 1.7 Update remaining test files — **DONE**

2. Phase 2 - Replace Windows-Specific Infrastructure
   - 2.1 FFmpeg cross-platform distribution — **DONE**
   - 2.2 Remove unused UWP notifications dependency — **DONE**
   - 2.3 HEIC/HEVC handling without Windows codec installation — **DONE**
   - 2.4 Replace single-instance named mutex — **DONE**
   - 2.5 Replace Windows-specific build commands and path assumptions — **DONE**

3. Phase 3 - Migrate the UI Layer (WPF → Avalonia)
   - 3.1 Set up Avalonia project — **DONE**
   - 3.2 Port ViewModels — **DONE**
   - 3.3 Port XAML Views — **DONE**
   - 3.4 Port Converters and Behaviors — **DONE**
   - 3.5 Port image display pipeline — **DONE**
   - 3.6 Apply modern Avalonia Fluent theme and app icon — **DONE**
   - 3.7 Swap solution/tests from WPF UI to Avalonia UI — **DONE**

4. Phase 4 - Multi-Threading Pipeline
   - 4.1 Create IThumbnailGenerator backed by ImageMagick — **DONE**
   - 4.2 Add Channel-based pipeline infrastructure — **DONE**
   - 4.3 Implement staged pipeline (File I/O → Image Process → Persistence → Callback) — **DONE**
   - 4.4 Add batch persistence writes — **DONE**
   - 4.5 Full pipeline integration — **DONE**

5. Phase 5 - Performance Optimization
   - 5.1 Streaming hash computation (avoid full-file byte[]) — **DONE for standalone file-hash APIs**
     and no-bytes hash fallbacks
   - 5.2 Batch SQLite writes — **DONE**
   - 5.3 Per-folder change detection — **DONE**
   - 5.4 Parallelize PHash computation — **DONE**
   - 5.5 Memory-mapped thumbnails or separate thumbnail file — **DONE**
   - 5.6 Parallel startup file-stat refresh — **DONE**
   - 5.7 Exact duplicate detection without ordered PLINQ — benchmarked, not applied due determinism risk
   - 5.8 File-name enumeration allocation cleanup — **DONE**
   - 5.9 SQLite connection-string caching — **DONE**

6. Phase 6 - CI/CD & Packaging
   - 6.1 Cross-platform CI builds
   - 6.2 Platform-specific packaging

7. Phase 7 - Validation & Polish
   - 7.1 Full regression testing
   - 7.2 Performance benchmarking comparison
