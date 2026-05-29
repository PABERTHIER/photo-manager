# Cross-Platform Migration Plan — PhotoManager

> **Goal**: Migrate PhotoManager from a Windows-only WPF application to a cross-platform desktop
> application that runs on **Windows, Linux, and macOS**, while maintaining existing functionality,
> performance, and code quality.

---

## Table of Contents

1. [Current State Analysis](#1-current-state-analysis)
2. [Windows-Specific Dependencies Inventory](#2-windows-specific-dependencies-inventory)
3. [Cross-Platform UI Framework Evaluation](#3-cross-platform-ui-framework-evaluation)
4. [Cross-Platform Imaging Strategy](#4-cross-platform-imaging-strategy)
5. [Migration Phases](#5-migration-phases)
   - [Phase 0 — Preparation & Abstraction](#phase-0--preparation--abstraction)
   - [Phase 1 — Remove WPF Types from Non-UI Layers](#phase-1--remove-wpf-types-from-non-ui-layers)
   - [Phase 2 — Replace Windows-Specific Infrastructure](#phase-2--replace-windows-specific-infrastructure)
   - [Phase 3 — Migrate the UI Layer](#phase-3--migrate-the-ui-layer)
   - [Phase 4 — CI/CD & Packaging](#phase-4--cicd--packaging)
   - [Phase 5 — Validation & Polish](#phase-5--validation--polish)
6. [Risk Register](#6-risk-register)
7. [Decision Log](#7-decision-log)

---

## 1. Current State Analysis

### Architecture (Clean Architecture)

```
PhotoManager.UI (WPF)  →  Application  →  Domain  ←  Infrastructure
       ↓                                                    ↓
   ViewModels                                        ImageProcessing
   Windows/Controls                                  Database (custom)
   Converters                                        AssetRepository
```

### Key Metrics

| Metric | Count |
|--------|-------|
| XAML files (windows + controls + app) | 9 |
| ViewModels | 13 (3 base/utility, 6 feature, 4 enums) |
| Value Converters | 5 |
| Dialog Windows | 4 (+ MainWindow) |
| Test files | ~201 |
| Files with WPF types **outside UI** | ~12 |
| Non-UI WPF type usages | ~90+ references |

### The Core Problem

`Directory.Build.props` sets **globally** for all projects:

```xml
<TargetFramework>net10.0-windows10.0.26100.0</TargetFramework>
<UseWPF>true</UseWPF>
```

This means **every project** (Common, Domain, Infrastructure, Application, Tests, Benchmarks)
compiles as Windows-only with WPF references available — even though only the UI project truly
needs WPF. The WPF type `System.Windows.Media.Imaging` is **globally imported** in Common, Domain,
Infrastructure, and Tests via their `GlobalUsings.cs` files.

---

## 2. Windows-Specific Dependencies Inventory

### 2.1 WPF Types Leaked into Non-UI Layers

These WPF types from `System.Windows.Media.Imaging` are used **outside** the UI project:

| WPF Type | Where Used | Usage Count |
|----------|-----------|-------------|
| **`BitmapImage`** | Common (`BitmapHelper`), Domain (`Asset.ImageData`, `IImageProcessingService`, `IAssetRepository`), Infrastructure (`ImageProcessingService`, `AssetRepository`), Application (`IApplication`, `Application`) | ~30+ |
| **`Rotation`** | Common (`BitmapHelper`, `ExifHelper`), Domain (`IImageProcessingService`), Infrastructure (`ImageProcessingService`), Application (`IApplication`, `Application`) | ~40+ |
| **`BitmapFrame`** | Common (`ExifHelper`, `BitmapHelper`) | 3 |
| **`BitmapMetadata`** | Common (`ExifHelper`) | 1 |
| **`BitmapEncoder`** family | Common (`BitmapHelper`) — `JpegBitmapEncoder`, `PngBitmapEncoder`, `GifBitmapEncoder` | 4 |
| **`BitmapCacheOption`** | Common (`BitmapHelper`) | 7 |
| **`BitmapCreateOptions`** | Common (`BitmapHelper`) | 7 |

#### Files Requiring Changes (Non-UI)

```
PhotoManager.Common/
├── GlobalUsings.cs                  — Remove: global using System.Windows.Media.Imaging
├── BitmapHelper.cs                  — 42 WPF usages (HEAVIEST FILE)
└── ExifHelper.cs                    — 10 WPF usages

PhotoManager.Domain/
├── GlobalUsings.cs                  — Remove: global using System.Windows.Media.Imaging
├── Asset.cs                         — BitmapImage? ImageData property
├── AssetCreationService.cs          — Rotation type usage
└── Interfaces/
    ├── IImageProcessingService.cs   — 13 WPF types in method signatures
    └── IAssetRepository.cs          — BitmapImage in LoadThumbnail return type

PhotoManager.Infrastructure/
├── GlobalUsings.cs                  — Remove: global using System.Windows.Media.Imaging
├── ImageProcessingService.cs        — 12 WPF types (delegates to Common)
└── AssetRepository.cs               — 2 BitmapImage usages

PhotoManager.Application/
├── IApplication.cs                  — 2 methods with BitmapImage/Rotation
└── Application.cs                   — 2 methods with BitmapImage/Rotation
```

### 2.2 Other Windows-Only Dependencies

| Dependency | Location | Issue | Cross-Platform? |
|-----------|----------|-------|----------------|
| **`Magick.NET-Q16-AnyCPU`** v14.11.0 | Common | Image processing (HEIC, hashing, etc.) | ✅ **Already cross-platform** (Windows, Linux, macOS) |
| **`FFMpegCore`** v5.4.0 | Common | Video frame extraction | ✅ **DONE** — versioned `Curiosity.FFmpeg.Runtimes.*` packages copy app-local binaries per RID |
| **`Microsoft.Toolkit.Uwp.Notifications`** v7.1.3 | UI | Toast notifications | ✅ **DONE** — package removed; no active code usage |
| **`System.Drawing`** | Common (`BitmapHelper`) | `Bitmap` class for `LoadBitmapFromPath` + `IsValidGdiPlusImage` | ✅ **DONE** **Limited cross-platform** (System.Drawing.Common requires libgdiplus on Linux) |
| **HEIC/HEVC Codecs** | CI (`build.yml`) | Installed via `winget` + `Add-AppxPackage` | ✅ **DONE** — Windows codec install removed; Magick.NET path verified |
| **`Mutex("PhotoManagerStartup")`** | UI (`App.xaml.cs`) | Single-instance enforcement | ✅ **DONE** — replaced by app-local file lock service |
| **`MessageBox.Show()`** | UI (`App.xaml.cs`) | WPF message dialog | ❌ **WPF-only** |
| **RAR extraction MSBuild task** | Common (`.csproj`) | `FileExtractionTask.dll` extracts FFmpeg from `.rar` files | ✅ **DONE** — `.rar` files and custom extraction DLLs removed |
| **`rmdir /S /Q`** | Tests (`.csproj`) | Post-build cleanup command | ✅ **DONE** — replaced with MSBuild `RemoveDir`; coverage helper moved to `pwsh` |
| **Hardcoded Windows paths** | `appsettings.json` (UI + Tests) | `C:\Users\...`, `E:\Workspace\...` | ✅ **DONE** — portable examples plus `~`/environment-variable expansion |

### 2.3 Build & CI Dependencies

| Item | Current | Issue |
|------|---------|-------|
| Target framework | `net10.0-windows10.0.26100.0` | Must become `net10.0` (or multi-target) |
| `<UseWPF>true</UseWPF>` | `Directory.Build.props` (global) | Must move to UI `.csproj` only |
| CI runner | `windows-latest` only | Must add Linux + macOS matrix |
| HEIC codec install | `winget` + `Add-AppxPackage` | ✅ **DONE** — removed; Magick.NET native HEIC support verified |

### 2.4 Test Impact Assessment

| Category | File Count | Impact Level |
|----------|-----------|-------------|
| Tests directly using `BitmapImage` | ~16 files | 🔴 Must refactor assertions |
| Tests using `Rotation` enum | ~83 files | 🟠 Enum replacement (search-and-replace) |
| Tests asserting `Asset.ImageRotation` | ~86 files | 🟡 Property type change |
| UI/ViewModel tests | ~40 files | 🟡 UI framework change |
| Total test files | ~201 | ~50-70 files need updates |

---

## 3. Cross-Platform UI Framework Evaluation

### 3.1 Candidates

| Framework | Type | Platforms | XAML Compat. | License | Maturity |
|-----------|------|-----------|-------------|---------|----------|
| **Avalonia UI** | Native XAML | Win/Linux/macOS/WASM | ★★★★★ WPF-like | MIT | ★★★★★ Production-ready |
| **Uno Platform** | WinUI XAML | Win/Linux/macOS/iOS/Android/WASM | ★★★☆☆ WinUI dialect | Apache 2.0 | ★★★★☆ Production-ready |
| **.NET MAUI** | Xamarin-successor | Win/macOS/iOS/Android | ★★☆☆☆ Different XAML | MIT | ★★★☆☆ No Linux support |
| **Photino** | WebView wrapper | Win/Linux/macOS | ☆☆☆☆☆ HTML only | Apache 2.0 | ★★☆☆☆ Niche |
| **Blazor Hybrid** | WebView + Razor | Win/macOS (Linux partial) | ☆☆☆☆☆ Razor syntax | MIT | ★★★☆☆ Evolving |

### 3.2 Detailed Comparison for PhotoManager's Needs

#### What PhotoManager Needs

- **Image rendering** with high-quality scaling, blur effects, drop shadows
- **Keyboard shortcuts** (Ctrl+C, Delete, PageUp/Down, F1, arrows)
- **Virtualized lists** for large thumbnail collections
- **TreeView** with lazy-loading for folder navigation
- **Modal dialogs** (4 dialog windows)
- **Dark theme** styling
- **MVVM pattern** with `INotifyPropertyChanged`
- **Menu bar** with nested items
- **Multi-step wizard** UI (SyncAssets, FindDuplicates)
- **Native desktop feel** — not a web app in a wrapper

#### Avalonia UI ✅ Recommended

| Criterion | Score | Notes |
|-----------|-------|-------|
| WPF XAML migration effort | ★★★★★ | Closest XAML dialect to WPF; most constructs translate 1:1 |
| Image rendering quality | ★★★★★ | Skia-based rendering, supports blur, drop shadow, bitmap scaling |
| Keyboard shortcuts | ★★★★★ | `KeyBinding`, `HotKeyManager`, `KeyDown` events |
| Virtualized lists | ★★★★★ | Built-in `VirtualizingStackPanel` |
| TreeView | ★★★★★ | Native `TreeView` with lazy-loading support |
| Dark theme | ★★★★★ | Built-in `FluentTheme` with dark mode, or fully custom |
| MVVM support | ★★★★★ | First-class; CommunityToolkit.Mvvm or ReactiveUI |
| SkiaSharp integration | ★★★★★ | Avalonia uses Skia internally; native `SKBitmap` interop |
| Community & ecosystem | ★★★★☆ | Active, growing; fewer controls than WPF but sufficient |
| Learning curve from WPF | ★★★★★ | Minimal — same concepts, similar API |

**Why Avalonia**: It's the most natural successor to WPF for desktop apps. The XAML dialect is almost
identical, MVVM patterns work the same way, and it provides native rendering on all three desktop
platforms. The project already uses no third-party WPF control libraries, which makes the migration
significantly simpler.

#### Uno Platform — Viable Alternative

| Criterion | Score | Notes |
|-----------|-------|-------|
| XAML migration effort | ★★★☆☆ | WinUI dialect requires XAML adjustments |
| Platform reach | ★★★★★ | Broadest: desktop + mobile + WASM |
| Image rendering | ★★★★☆ | Good, Skia-based on non-Windows |
| Maturity | ★★★★☆ | Strong vendor backing |
| Learning curve from WPF | ★★★☆☆ | WinUI differences require adaptation |

**When to consider Uno**: If the project ever needs mobile (iOS/Android) or web (WASM) targets,
Uno is worth reconsidering. For desktop-only, Avalonia is more natural.

#### Photino / Blazor Hybrid — Not Recommended

These use web technologies (HTML/CSS/JS) rendered in a WebView. They lose the native desktop feel,
lack fine-grained image rendering control (blur effects, drop shadows, bitmap scaling modes), and
make keyboard shortcut handling more complex. Not suitable for an image-intensive desktop application.

#### .NET MAUI — Not Recommended

No Linux support. Less desktop-focused than Avalonia. More complex abstraction layer.
The XAML dialect is different enough from WPF to require significant rework without clear benefits
for this use case.

### 3.3 Recommendation

> **Primary choice: Avalonia UI**
>
> - Closest WPF migration path (lowest effort, highest XAML compatibility)
> - True cross-platform desktop rendering (Windows, Linux, macOS)
> - MIT license, active community, production-proven
> - Native Skia rendering aligns with SkiaSharp for image processing
> - No web technology overhead — genuine native desktop experience
>
> **Fallback: Uno Platform** — only if mobile/WASM targets become a requirement.

---

## 4. Cross-Platform Imaging Strategy

### 4.1 The Problem

The project currently uses WPF's `BitmapImage` as the universal image type across all layers.
This must be replaced with a cross-platform image abstraction.

### 4.2 Image Library Options

| Library | License | Native Deps | HEIC | EXIF | Encoding | Notes |
|---------|---------|-------------|------|------|----------|-------|
| **SkiaSharp** | MIT | Yes (Skia) | Via codec | Via API | JPEG/PNG/WebP | Avalonia's native image type; best integration |
| **Magick.NET** | Apache 2.0 | Yes (ImageMagick) | ✅ Native | ✅ Rich | All formats | **Already used** in the project |
| **ImageSharp** | Split/Commercial | None (pure C#) | Limited | ✅ | Common formats | Commercial license required |

### 4.3 Recommended Approach

**Keep Magick.NET** for image processing (hashing, HEIC, EXIF, format conversion) — it's already
cross-platform and deeply integrated.

**Use SkiaSharp `SKBitmap`** as the cross-platform image carrier type replacing `BitmapImage` — it
integrates natively with Avalonia (which uses Skia internally) and provides image display, rotation,
encoding, and caching.

**Use Avalonia's `Bitmap`** type only in the UI layer for display binding.

### 4.4 Abstraction Layer Design

Create platform-agnostic abstractions in the Domain layer:

```csharp
// PhotoManager.Domain/Imaging/ImageRotation.cs
public enum ImageRotation
{
    Rotate0 = 0,
    Rotate90 = 90,
    Rotate180 = 180,
    Rotate270 = 270
}

// PhotoManager.Domain/Imaging/IImageData.cs
public interface IImageData : IDisposable
{
    int Width { get; }
    int Height { get; }
    ImageRotation Rotation { get; }
    byte[] ToByteArray(ImageFormat format);
    Stream ToStream(ImageFormat format);
}

// PhotoManager.Domain/Imaging/ImageFormat.cs
public enum ImageFormat
{
    Jpeg,
    Png,
    Gif,
    Bmp
}
```

The **Infrastructure** layer implements `IImageData` using SkiaSharp:

```csharp
// PhotoManager.Infrastructure/Imaging/SkiaImageData.cs
public class SkiaImageData : IImageData
{
    private readonly SKBitmap _bitmap;
    // ... implementation wrapping SKBitmap
}
```

The **UI layer** converts `IImageData` → Avalonia `Bitmap` for display only.

---

## 5. Migration Phases

### Phase 0 — Preparation & Abstraction

> **Goal**: Decouple the build system so non-UI projects compile as cross-platform .NET, and
> define the abstraction interfaces that will replace WPF types.

#### 0.1 Split `Directory.Build.props`

**Current** (all projects share one file):
```xml
<TargetFramework>net10.0-windows10.0.26100.0</TargetFramework>
<UseWPF>true</UseWPF>
```

**Target**:
- Root `Directory.Build.props` → `net10.0` (no Windows TFM), remove `<UseWPF>`
- `PhotoManager.UI/Directory.Build.props` (new) → override with `net10.0-windows10.0.26100.0`
  and `<UseWPF>true</UseWPF>` (temporary — will be removed when UI migrates to Avalonia)

This lets Common, Domain, Infrastructure, Application, Tests, and Benchmarks compile as
cross-platform immediately.

#### 0.2 Define Domain Abstractions

Create new files in `PhotoManager.Domain/Imaging/`:

- `ImageRotation.cs` — cross-platform enum replacing `System.Windows.Media.Imaging.Rotation`
- `IImageData.cs` — interface replacing `BitmapImage` in non-UI layer signatures
- `ImageFormat.cs` — enum for encoding target format

#### 0.3 Set Up Feature Branch Strategy

- Create a long-lived `feature/cross-platform` branch
- Each sub-phase gets its own branch merged into it
- Keeps `main` stable throughout the migration

---

### Phase 1 — Remove WPF Types from Non-UI Layers

> **Goal**: Replace all `BitmapImage`, `Rotation`, `BitmapFrame`, `BitmapEncoder`, etc. with
> cross-platform types in Common, Domain, Infrastructure, and Application layers.

#### 1.1 Replace `Rotation` Enum Everywhere **DONE**

This is the simplest, highest-impact change — `Rotation` is used in ~83 test files.

| Layer | File | Change |
|-------|------|--------|
| Common | `ExifHelper.cs` | Return `ImageRotation` instead of `Rotation` |
| Common | `BitmapHelper.cs` | Accept `ImageRotation` parameter, convert internally |
| Domain | `IImageProcessingService.cs` | Replace all `Rotation` parameters with `ImageRotation` |
| Domain | `AssetCreationService.cs` | Use `ImageRotation` |
| Infrastructure | `ImageProcessingService.cs` | Use `ImageRotation` |
| Application | `IApplication.cs` + `Application.cs` | Use `ImageRotation` |
| Tests | ~83 files | Search-and-replace `Rotation.Rotate0` → `ImageRotation.Rotate0`, etc. |

#### 1.2 Replace `BitmapImage` in Domain Interfaces **DONE**

| Interface | Method | Before → After |
|-----------|--------|----------------|
| `IImageProcessingService` | `LoadBitmapThumbnailImage` | Returns `BitmapImage` → `IImageData` |
| `IImageProcessingService` | `LoadBitmapOriginalImage` | Returns `BitmapImage` → `IImageData` |
| `IImageProcessingService` | `LoadBitmapImageFromPath` | Returns `BitmapImage` → `IImageData` |
| `IImageProcessingService` | `LoadBitmapHeic*` (3 methods) | Returns `BitmapImage` → `IImageData` |
| `IImageProcessingService` | `GetJpeg/Png/GifBitmapImage` | Accepts `BitmapImage` → `IImageData` |
| `IAssetRepository` | `LoadThumbnail` | Returns `BitmapImage?` → `IImageData?` |
| `IApplication` | `LoadBitmapImageFromPath` | Returns `BitmapImage` → `IImageData` |
| `IApplication` | `LoadBitmapHeicImageFromPath` | Returns `BitmapImage` → `IImageData` |
| `Asset` | `ImageData` property | Type `BitmapImage?` → `IImageData?` |

#### 1.3 Rewrite `BitmapHelper.cs` (Heaviest File) **DONE**

This is the most complex change. Current `BitmapHelper` creates WPF `BitmapImage` objects with
`StreamSource`, `CacheOption`, `Rotation`, `DecodePixelWidth`, etc.

**Strategy**: Rewrite using **SkiaSharp** (`SKBitmap`, `SKCodec`, `SKImage`):

| Current WPF API | SkiaSharp Replacement |
|-----------------|----------------------|
| `new BitmapImage()` + `StreamSource` | `SKBitmap.Decode(stream)` |
| `BitmapImage.Rotation` | Manual `SKBitmap` rotation via `SKCanvas` transform |
| `BitmapImage.DecodePixelWidth/Height` | `SKBitmap.Resize(new SKImageInfo(w, h))` |
| `BitmapCacheOption.OnLoad` | SkiaSharp loads into memory by default |
| `BitmapCreateOptions.IgnoreColorProfile` | Configure `SKCodecOptions` |
| `JpegBitmapEncoder` | `SKImage.Encode(SKEncodedImageFormat.Jpeg, quality)` |
| `PngBitmapEncoder` | `SKImage.Encode(SKEncodedImageFormat.Png, 100)` |
| `GifBitmapEncoder` | `SKImage.Encode(SKEncodedImageFormat.Gif, 100)` |
| `BitmapFrame.Create(image)` | `SKImage.FromBitmap(bitmap)` |

#### 1.4 Rewrite `ExifHelper.cs` EXIF Reading **DONE**

| Current WPF API | Replacement |
|-----------------|-------------|
| `BitmapFrame.Create(stream)` → `BitmapMetadata` → `GetQuery(exif:274)` | Use **Magick.NET** `MagickImage` EXIF API (already used for HEIC EXIF) or **SkiaSharp** `SKCodec.EncodedOrigin` |

Magick.NET is already used in `ExifHelper.GetHeicExifOrientation()` — unify all EXIF reading
through Magick.NET for consistency:

```csharp
using MagickImage image = new(stream);
ExifProfile? exifProfile = image.GetExifProfile();
ushort orientation = exifProfile?.GetValue(ExifTag.Orientation)?.Value ?? 1;
return MapOrientationToRotation(orientation);
```

#### 1.5 Remove `System.Drawing` Usage **DONE**

`BitmapHelper.LoadBitmapFromPath()` and `IsValidGdiPlusImage()` use `System.Drawing.Bitmap`.

**Replace with**: SkiaSharp equivalents:
- `new Bitmap(stream)` → `SKBitmap.Decode(stream)`
- Validation → check if `SKCodec.Create(stream)` returns non-null

#### 1.6 Remove WPF Global Usings **DONE**

After all replacements are complete:

```diff
- // PhotoManager.Common/GlobalUsings.cs
- global using System.Windows.Media.Imaging;

- // PhotoManager.Domain/GlobalUsings.cs
- global using System.Windows.Media.Imaging;

- // PhotoManager.Infrastructure/GlobalUsings.cs
- global using System.Windows.Media.Imaging;

- // PhotoManager.Tests/GlobalUsings.cs
- global using System.Windows.Media.Imaging;
```

#### 1.7 Update Tests (~50-70 Files) **DONE**

| Test Category | Change Required |
|---------------|----------------|
| `BitmapHelperTests` (5 tests) | Assert on `IImageData` properties instead of `BitmapImage` |
| `ExifHelperTests` (3 tests) | Assert `ImageRotation` instead of `Rotation` |
| `ImageProcessingServiceTests` (~22 tests) | Update return type assertions |
| Rotation enum usage (~83 files) | Find-and-replace enum values |
| `Asset.ImageRotation` assertions (~86 files) | Verify compatible after type change |

---

### Phase 2 — Replace Windows-Specific Infrastructure

> **Goal**: Make all non-UI infrastructure cross-platform.

#### 2.1 FFmpeg Binary Distribution **DONE**

**Current**: Windows-only RAR extraction via custom MSBuild task (`FileExtractionTask.dll`)

**Target**: Replace with cross-platform approach:

- **Option A (Recommended)**: Use a NuGet package that bundles FFmpeg binaries per RID:
  - Remove `.rar` files and `FileExtractionTask.dll`
  - Add conditional `PackageReference` for FFmpeg runtime per OS, or:
  - Simply document that `ffmpeg` must be installed on the system (`apt install ffmpeg`,
    `brew install ffmpeg`, `choco install ffmpeg`)
  - Configure `GlobalFFOptions.Configure(o => o.BinaryFolder = ...)` to check `PATH` first

- **Option B**: Bundle platform-specific FFmpeg binaries as `.zip`/`.tar.gz` in the repo
  with cross-platform extraction (using `System.IO.Compression`)

**Completed:** Option A was implemented with versioned `Curiosity.FFmpeg.Runtimes.*` packages. The build
copies `ffmpeg` and `ffprobe` into `Ffmpeg/Bin`, supports environment/configured overrides, and falls back to
`PATH`. The old `.rar` files and custom extraction DLLs were removed.

#### 2.2 Replace `Microsoft.Toolkit.Uwp.Notifications` **DONE**

**Current**: Windows-only UWP toast notifications (appears to not be actively used in code)

**Target**:
- If not used: simply remove the package reference
- If used: replace with cross-platform notification approach:
  - Avalonia has community notification packages
  - Or use `DesktopNotifications` NuGet package (cross-platform)

**Completed:** The package reference was removed because no production code used UWP toast APIs.

#### 2.3 HEIC/HEVC Codec Handling **DONE**

**Current**: CI installs Windows-specific codec extensions via `winget` and `Add-AppxPackage`

**Target**: Magick.NET already handles HEIC natively on all platforms (it bundles `libheif`).
The Windows codec install in CI was likely needed for the WPF `BitmapFrame.Create()` path
which is being removed. Once HEIC loading goes fully through Magick.NET, the Windows codec
install can be removed from CI.

**Verify**: Run HEIC-related tests after Phase 1 changes to confirm Magick.NET handles
everything without OS codecs.

**Completed:** The CI `winget`/`Add-AppxPackage` codec installation was removed after HEIC/metadata tests
passed through the Magick.NET path.

#### 2.4 Single-Instance Application Mutex **DONE**

**Current**: `new Mutex(true, "PhotoManagerStartup")` in `App.xaml.cs`

**Target**: Named mutexes work on Linux/macOS under .NET but behavior varies. More portable
alternatives:
- File-based lock (`/tmp/photomanager.lock` or equivalent)
- Platform-agnostic single-instance library
- Avalonia has community solutions for this

**Completed:** The named mutex was replaced with `ISingleInstanceService`, backed by an app-local lock file and
disposed through the DI container on application exit.

#### 2.5 Fix Windows-Specific Build Commands **DONE**

| Item | Current | Replacement |
|------|---------|-------------|
| Tests `.csproj` post-build | `rmdir /S /Q` | ✅ MSBuild `RemoveDir` task |
| Local coverage helper | `.bat` script | ✅ `pwsh` script with filtered coverage support |
| `appsettings.json` paths | `C:\Users\...` | ✅ `~`, `%VARIABLE%`, `$VARIABLE`, `${VARIABLE}` expansion |
| Domain path handling | Windows separators only | ✅ Windows and Unix separators accepted for folder/sync paths |

---

### Phase 3 — Migrate the UI Layer

> **Goal**: Replace the WPF UI with Avalonia UI while preserving all functionality.

#### 3.1 Create New Avalonia UI Project — **DONE**

```
PhotoManager.UI.Avalonia/       (new project)
├── App.axaml / App.axaml.cs    — DI container, Serilog setup, single-instance
├── Windows/
│   ├── MainWindow.axaml/.cs
│   ├── FindDuplicatedAssetsWindow.axaml/.cs
│   ├── SyncAssetsWindow.axaml/.cs
│   ├── FolderNavigationWindow.axaml/.cs
│   └── AboutWindow.axaml/.cs
├── Controls/
│   ├── ViewerUserControl.axaml/.cs
│   ├── ThumbnailsUserControl.axaml/.cs
│   └── FolderNavigationControl.axaml/.cs
├── ViewModels/                  — Reuse most code from existing ViewModels
├── Converters/                  — Adapt 5 converters
└── Models/                      — Reuse event delegates/args
```

#### 3.2 XAML Migration (WPF → Avalonia) — **DONE**

Key differences to address:

| WPF | Avalonia | Migration Effort |
|-----|----------|-----------------|
| `.xaml` extension | `.axaml` extension | Rename |
| `xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"` | `xmlns="https://github.com/avaloniaui"` | Find-replace |
| `Visibility.Hidden` / `Visibility.Visible` | `IsVisible` (bool) property | Simplifies code |
| `System.Windows.Visibility` in ViewModels | Replace with `bool` | Simplifies ViewModels |
| `BlurEffect` | Avalonia `BlurEffect` or render-level blur | Similar API |
| `DropShadowEffect` | Avalonia `DropShadowEffect` | Similar API |
| `VirtualizingStackPanel` | Avalonia virtualization support | Similar concept |
| `TreeView` | Avalonia `TreeView` | Similar API |
| `DataGrid` | Avalonia `DataGrid` (community or built-in) | Similar API |
| `Menu` / `MenuItem` | Avalonia `Menu` / `MenuItem` | Nearly identical |
| `Image.Source = BitmapImage` | `Image.Source = Avalonia.Media.Imaging.Bitmap` | Convert `IImageData` → Avalonia `Bitmap` |
| `ControlTemplate` (arrows) | Avalonia `ControlTemplate` | Similar syntax |
| `StaticResource` | Avalonia `StaticResource` or `DynamicResource` | Similar |
| `Dispatcher.InvokeAsync()` | `Dispatcher.UIThread.InvokeAsync()` | Minor rename |
| `MouseLeftButtonDown` | `PointerPressed` | Event rename |
| `KeyDown` | `KeyDown` | Same |
| `MessageBox.Show()` | Avalonia `MessageBoxManager` or custom dialog | Community package |
| `RenderOptions.BitmapScalingMode` | `RenderOptions.BitmapInterpolationMode` | Rename |

#### 3.3 ViewModel Migration — **DONE**

Most ViewModels can be **reused as-is** with minimal changes:

| ViewModel | Changes Needed |
|-----------|----------------|
| `BaseViewModel` | ✅ No WPF dependency — reuse directly |
| `BaseProcessViewModel<TC, TR>` | ✅ No WPF dependency — reuse directly |
| `ApplicationViewModel` | Replace `Visibility` properties with `bool IsXxxVisible` |
| `DuplicatedAssetViewModel` | Replace `Visibility` property with `bool` |
| `SortableObservableCollection` | ✅ Uses standard `ObservableCollection` — reuse |
| Enums (`AppMode`, `ProcessStep`, `SortCriteria`) | ✅ No changes needed |

#### 3.4 Converter Migration — **DONE**

| Converter | Changes |
|-----------|---------|
| `FileNameConverter` | Implement Avalonia `IValueConverter` (same interface name) |
| `FileSizeConverter` | Implement Avalonia `IValueConverter` |
| `PixelSizeConverter` | Implement Avalonia `IValueConverter`, update to use `IImageData` |
| `TernaryConverter` | Implement Avalonia `IMultiValueConverter` |
| `VisibilityConverter` | **Remove** — Avalonia uses `bool` for visibility |

#### 3.5 Image Display Pipeline — **DONE**

```
IImageData (Domain) → SkiaImageData (Infrastructure, wrapping SKBitmap)
     ↓
UI Converter or helper method
     ↓
Avalonia.Media.Imaging.Bitmap (created from SKBitmap data)
     ↓
<Image Source="{Binding CurrentImageBitmap}" />
```

Conversion helper:
```csharp
public static Avalonia.Media.Imaging.Bitmap ToAvaloniaBitmap(this IImageData imageData)
{
    using MemoryStream ms = new();
    // Encode SKBitmap → PNG stream → Avalonia Bitmap
    imageData.ToStream(ImageFormat.Png).CopyTo(ms);
    ms.Position = 0;
    return new Avalonia.Media.Imaging.Bitmap(ms);
}
```

#### 3.6 Theme Migration — **DONE**

Current WPF theme is hardcoded dark colors. Two options:

- **Option A (Quick)**: Port hardcoded colors to Avalonia styles in `App.axaml`
- **Option B (Better)**: Use Avalonia's built-in `FluentTheme` with `Dark` mode and customize
  accent colors to match the current look

#### 3.7 Remove Old WPF UI Project — **DONE**

Completed state:
1. Removed the legacy WPF UI project from `PhotoManager.slnx`.
2. Pointed UI tests to `PhotoManager.UI.Avalonia`.
3. Kept the emitted assembly/root namespace as `PhotoManager.UI` while retaining the project folder
   name for the transition.
4. Removed global `<UseWPF>true</UseWPF>` and moved the solution back to plain `net10.0`.

---

### Phase 4 — CI/CD & Packaging

> **Goal**: Build, test, and package for all three platforms.

#### 4.1 Multi-Platform CI Matrix

```yaml
strategy:
  matrix:
    include:
      - os: windows-latest
        rid: win-x64
      - os: ubuntu-latest
        rid: linux-x64
      - os: macos-latest
        rid: osx-x64
      - os: macos-latest
        rid: osx-arm64
```

#### 4.2 Platform-Specific Packaging

| Platform | Format | Tool |
|----------|--------|------|
| Windows | `.msix` or self-contained `.exe` | `dotnet publish -r win-x64` |
| Linux | `.AppImage`, `.deb`, or `.tar.gz` | Avalonia packaging tools |
| macOS | `.app` bundle / `.dmg` | Avalonia packaging tools |

#### 4.3 FFmpeg Distribution Strategy

| Platform | FFmpeg Source |
|----------|-------------|
| Windows | ✅ **DONE** — app-local runtime package binary, with override/PATH fallback |
| Linux | ✅ **DONE** — app-local runtime package binary, with override/PATH fallback |
| macOS | ✅ **DONE** — app-local runtime package binary, with override/PATH fallback |

#### 4.4 Remove Windows-Only CI Steps

- Remove `winget install` HEIF extensions — **DONE**
- Remove `Add-AppxPackage` HEVC codec — **DONE**
- Remove Windows-specific cleanup scripts — **DONE**
- Add platform-appropriate FFmpeg installation per matrix OS — **DONE** via runtime packages, no CI install needed

---

### Phase 5 — Validation & Polish

> **Goal**: Ensure feature parity, performance, and quality across all platforms.

#### 5.1 Feature Parity Checklist

- [x] Folder navigation startup binding and add/remove refresh — **DONE**
- [x] Thumbnail grid selection and viewer-position binding — **DONE**
- [x] Full-screen image viewer control lookup and image source cleanup — **DONE**
- [ ] Keyboard shortcuts (Ctrl+C, Ctrl+M, Delete, F1, PageUp/Down, arrows)
- [ ] Find Duplicates workflow (multi-step wizard, DataGrid, thumbnail preview)
- [ ] Sync Assets workflow (multi-step wizard, DataGrid, configuration)
- [x] Copy/Move to folder selected-folder persistence — **DONE**
- [ ] About dialog
- [x] Theme mode configuration reader — **DONE**
- [ ] HEIC image support
- [ ] Video thumbnail extraction
- [ ] Single-instance enforcement
- [x] Status bar asset counters after duplicate deletion — **DONE**

#### 5.2 Cross-Platform Testing

- Run full test suite on Windows, Linux, and macOS
- Manual UI testing on all three platforms
- Verify image rendering quality (blur, shadows, scaling)
- Verify HEIC/HEVC handling without Windows codecs
- Verify FFmpeg integration on all platforms
- Performance benchmarks on all platforms

#### 5.3 Documentation Updates

- Update `README.md` with cross-platform build/run instructions
- Update `AGENTS.md` with new project structure
- Update `appsettings.json` examples for each platform
- Add platform-specific installation instructions

---

## 6. Risk Register

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| Avalonia XAML differences cause unexpected layout issues | Medium | Medium | Incremental migration, test each window individually |
| SkiaSharp image quality differs from WPF rendering | Low | Medium | Compare output visually; tune encoding quality parameters |
| HEIC support via Magick.NET differs from Windows codecs | Low | High | Test with all existing HEIC test files early in Phase 1 |
| `SortableObservableCollection` behavior differs | Low | Low | Standard `ObservableCollection` — should work identically |
| Avalonia `DataGrid` lacks WPF `DataGrid` features | Medium | Medium | Evaluate Avalonia `DataGrid` package early; may need custom template |
| FFmpeg runtime package missing for a RID | Low | Medium | Keep override/PATH fallback and fail clearly if no binary is available |
| Performance regression on Linux/macOS | Low | Medium | Run benchmarks on all platforms; SkiaSharp is typically fast |
| ~70 test files need updates → risk of test regression | Medium | High | Phase 1 is test-heavy; run full suite after each sub-step |
| TreeView lazy-loading behaves differently in Avalonia | Low | Medium | Avalonia TreeView supports `HierarchicalDataTemplate` — similar pattern |

---

## 7. Decision Log

| Decision | Chosen | Rationale | Alternatives Considered |
|----------|--------|-----------|------------------------|
| UI Framework | **Avalonia UI** | Closest WPF migration path; native desktop rendering; MIT license; mature ecosystem; built on Skia | Uno Platform (WinUI dialect, more rework), MAUI (no Linux), Photino (web-based, no native feel), Blazor Hybrid (web-based) |
| Image carrier type | **SkiaSharp `SKBitmap`** via `IImageData` abstraction | Integrates natively with Avalonia (Skia-based); cross-platform; MIT license; good performance | ImageSharp (commercial license), raw `byte[]` (too low-level), Magick.NET `MagickImage` (heavier, not display-oriented) |
| Image processing | **Keep Magick.NET** | Already integrated; cross-platform; handles HEIC, perceptual hashing, advanced operations | SkiaSharp alone (lacks advanced features), ImageSharp (license concern) |
| EXIF reading | **Unify on Magick.NET** | Already used for HEIC EXIF; more reliable cross-platform than WPF `BitmapMetadata` | SkiaSharp `SKCodec.EncodedOrigin` (less feature-rich), MetadataExtractor NuGet (another dependency) |
| FFmpeg distribution | **Versioned runtime packages + PATH fallback** | Deterministic frame extraction without user setup; still supports overrides | System-only FFmpeg (version drift), Docker-only (limits desktop use) |
| Rotation enum | **Custom `ImageRotation` in Domain** | Domain should not depend on any UI framework; clean architecture | Keep WPF `Rotation` (blocks cross-platform), use `int` degrees (less type-safe) |
| Notifications | **Removed unused UWP package** | No production code used toast APIs | `DesktopNotifications` package if notifications are reintroduced |
| Catalog thumbnail generation — **DONE** | **`IThumbnailGenerator` backed by Magick.NET** | Keeps catalog image processing cross-platform and independent from Avalonia UI types; enables the multi-threaded catalog pipeline | Reuse Avalonia display types in Domain (cross-layer leak), keep per-file persistence (slower) |
| Migration approach | **New project alongside old** | Allows incremental migration; old UI stays functional until new one is ready | In-place rewrite (risky, breaks `main`), branch-based rewrite (merge conflicts) |

---

## Summary: Execution Order

Key Findings:

- WPF types leak everywhere: BitmapImage and Rotation are globally imported in Common, Domain, Infrastructure, and Tests — ~90+ references outside UI
- Magick.NET and FFMpegCore are cross-platform ✅; the RAR-based FFmpeg bundling blocker is resolved
- ~201 test files, of which ~50-70 will need updates

Recommendation: Avalonia UI
Closest WPF migration path (nearly identical XAML), native Skia rendering, MIT license, true Linux/macOS/Windows support. MAUI lacks Linux; Photino/Blazor are web-based and unsuitable for an image-heavy desktop app.

5-Phase Plan

 1. Phase 0 — Split Directory.Build.props so non-UI projects target net10.0 (not Windows-specific); define ImageRotation/IImageData abstractions in Domain
 2. Phase 1 (largest) — Remove all WPF types from non-UI layers: rewrite BitmapHelper with SkiaSharp, unify EXIF on Magick.NET, replace Rotation enum, update ~70 test files
 3. Phase 2 — Fix Windows-specific infrastructure (FFmpeg, notifications, HEIC, single-instance, build paths) — **DONE**
 4. Phase 3 (most visible) — Build new Avalonia UI project alongside old WPF one, migrate 9 XAML files, reuse ViewModels, then swap
 5. Phase 4-5 — Multi-platform CI matrix, packaging, cross-platform validation

```
Phase 0: Preparation & Abstraction
  ├── 0.1 Split Directory.Build.props (UI-specific vs cross-platform)
  ├── 0.2 Define Domain abstractions (ImageRotation, IImageData, ImageFormat) **DONE**
  └── 0.3 Set up feature branch

Phase 1: Remove WPF from Non-UI Layers (LARGEST PHASE)
  ├── 1.1 Replace Rotation enum everywhere (~83 test files) **DONE**
  ├── 1.2 Replace BitmapImage in Domain interfaces **DONE**
  ├── 1.3 Rewrite BitmapHelper.cs with SkiaSharp **DONE**
  ├── 1.4 Rewrite ExifHelper.cs EXIF reading (unify on Magick.NET)
  ├── 1.5 Remove System.Drawing usage
  ├── 1.6 Remove WPF GlobalUsings from non-UI projects
  └── 1.7 Update ~50-70 test files

Phase 2: Replace Windows-Specific Infrastructure
  ├── 2.1 FFmpeg cross-platform distribution **DONE**
  ├── 2.2 Replace/remove UWP Notifications **DONE**
  ├── 2.3 HEIC codec handling (verify Magick.NET sufficiency) **DONE**
  ├── 2.4 Single-instance Mutex portability **DONE**
  └── 2.5 Fix Windows-specific build commands **DONE**

Phase 3: Migrate UI to Avalonia (MOST VISIBLE PHASE)
  ├── 3.1 Create Avalonia project structure
  ├── 3.2 Migrate XAML (9 files)
  ├── 3.3 Migrate ViewModels (mostly reuse)
  ├── 3.4 Migrate Converters (5 → 4)
  ├── 3.5 Image display pipeline (IImageData → Avalonia Bitmap)
  ├── 3.6 Theme migration (dark theme)
  └── 3.7 Remove old WPF UI project

Phase 4: CI/CD & Packaging
  ├── 4.1 Multi-platform CI matrix
  ├── 4.2 Platform-specific packaging
  ├── 4.3 FFmpeg distribution per platform
  └── 4.4 Remove Windows-only CI steps

Phase 5: Validation & Polish
  ├── 5.1 Feature parity verification
  ├── 5.2 Cross-platform testing
  └── 5.3 Documentation updates
```

> **Estimated scope**: ~100-120 files modified across all phases, with Phase 1 and Phase 3
> being the most labor-intensive. The clean architecture already in place makes this migration
> significantly more manageable than a monolithic WPF application would be.
