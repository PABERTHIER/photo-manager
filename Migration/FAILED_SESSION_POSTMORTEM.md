# Failed Session Post-Mortem: Phase 1.3 Attempt (2026-05-22)

## Summary

A previous coding session attempted to complete Phase 1 items 1.3–1.7 (removing WPF types from
non-UI layers). The session ran for 5+ hours and ultimately self-terminated. The resulting code
changes introduce **severe performance regressions** that increase test execution time from ~10
minutes to ~40 minutes.

---

## Root Cause: Wrong Library Choice + Excessive Transcoding

### What the plan said

> Use **SkiaSharp** (`SKBitmap`, `SKCodec`, `SKImage`) as the replacement for WPF BitmapImage.
> SkiaSharp is lightweight, fast, and integrates natively with Avalonia (the target UI framework).

### What was done instead

The session chose **Magick.NET** (`MagickImage`) as the replacement, reasoning that "it's already
a dependency." This fundamentally misunderstands the performance profile:

| Operation | WPF BitmapImage | SkiaSharp SKBitmap | MagickImage |
|-----------|----------------|-------------------|-------------|
| Decode JPEG to memory | ~2–5ms | ~2–5ms | ~10–30ms |
| In-memory rotation | ~0.1ms (metadata) | ~1ms (pixel copy) | ~5–20ms |
| Encode to JPEG | ~3–8ms | ~3–8ms | ~10–30ms |
| Pixel access (1 pixel) | ~0.001ms | ~0.001ms | ~5–15ms (requires GetPixels()) |
| Memory overhead | Low (decoded bitmap) | Low (decoded bitmap) | High (full ImageMagick pipeline) |

**Magick.NET is excellent for complex image processing** (format conversion, HEIC, PHash, filters)
but it is **NOT a replacement for a lightweight bitmap carrier**. It's an overkill image processing
engine being used as a simple image container.

---

## Specific Performance Issues Identified

### 1. `MagickImageData` stores images as PNG byte arrays (CATASTROPHIC)

```csharp
public sealed class MagickImageData(...) : IImageData
{
    private const ImageEncodingFormat InternalFormat = ImageEncodingFormat.Png;
    private readonly byte[] _imageData = NormalizeImageData(imageData); // Re-encodes to PNG!
}
```

**Problem**: Every image loaded is encoded to PNG and stored as raw bytes. Every subsequent
operation (display, encode to JPEG, pixel access) must **decode the PNG again**. This turns
every operation into: `Original → Decode → PNG encode → store → PNG decode → operation`.

**Impact**: For a thumbnail that was JPEG (2KB), it's now stored as PNG (5–17KB) internally,
then re-decoded for any use. The encode+decode cycle adds ~20–50ms per image operation.

### 2. `NormalizeImageData` re-encodes non-PNG data on construction

```csharp
private static byte[] NormalizeImageData(byte[] imageData)
{
    if (IsPng(imageData)) return (byte[])imageData.Clone();
    using (MagickImage image = new(imageData))
    {
        return image.ToByteArray(MagickFormat.Png); // Full decode + re-encode!
    }
}
```

**Problem**: If any image data is passed that isn't already PNG (e.g., JPEG thumbnail bytes from
the database), it gets fully decoded and re-encoded. This happens at construction time for every
single MagickImageData instance.

### 3. `GetPixelBrightness` decodes entire image for 1 pixel

```csharp
public static float GetPixelBrightness(MagickImageData imageData, int x, int y)
{
    using (MagickImage image = new(imageData.ToByteArray(ImageEncodingFormat.Png)))
    {
        using (IPixelCollection<ushort> pixels = image.GetPixels())
        {
            IPixel<ushort>? pixel = pixels.GetPixel(x, y);
            // ... brightness calculation
        }
    }
}
```

**Problem**: This method is called in tests to verify pixel data. Each call:
1. Clones the PNG byte array
2. Decodes the full PNG into MagickImage
3. Gets the pixel collection
4. Reads one pixel
5. Disposes everything

For DHash computation or test assertions that check multiple pixels, this is devastating.

### 4. `CalculateDHash` double-decodes images

```csharp
MagickImageData? imageData = BitmapHelper.LoadBitmapFromPath(filePath!);
// LoadBitmapFromPath: Open file → MagickImage → encode to PNG → store in MagickImageData

using (MagickImage image = new(imageData.ToByteArray(ImageEncodingFormat.Png)))
// ToByteArray: Clone PNG array → create ANOTHER MagickImage from those PNG bytes
```

**Problem**: The file is loaded into MagickImage, encoded to PNG bytes, stored, then those PNG
bytes are decoded AGAIN into a new MagickImage. Two full decode cycles instead of one.

### 5. Thumbnail byte sizes increased significantly

| Image | Before (WPF JPEG) | After (MagickImage PNG→JPEG) | Increase |
|-------|-------------------|------------------------------|----------|
| IMAGE_1_JPG | 2,534 bytes | 1,644 bytes | -35% (smaller but different quality) |
| IMAGE_9_PNG | 15,026 bytes | 4,496 bytes | -70% (too aggressive compression?) |
| _1336_ORIGINAL_JPG | 10,753 bytes | 17,609 bytes | +64% |
| _1336_BOTTOM_LEFT_PART_JPG | 13,128 bytes | 24,975 bytes | +90% |
| IMAGE_11_90_DEG_HEIC | 5,921 bytes | 14,755 bytes | +149% |

The inconsistency shows that the encoding pipeline is not equivalent. Some images are smaller
(different compression settings), others are much larger (PNG internal format leaking, or
MagickImage using different JPEG quality settings).

### 6. Test execution time 4× slower

The cumulative effect: every test that creates/loads/asserts on an image now goes through
MagickImage encode/decode cycles instead of lightweight WPF bitmap operations. With ~500+ image
tests, each adding 20–100ms of overhead, total test time increases from ~10 min to ~40 min.

---

## Why SkiaSharp Is The Correct Choice

1. **Lightweight bitmap carrier**: `SKBitmap` holds decoded pixel data in memory — no re-encoding
2. **Fast pixel access**: Direct pointer to pixel buffer, O(1) per pixel
3. **Fast encode/decode**: Uses Skia's optimized JPEG/PNG codecs
4. **Avalonia integration**: Avalonia uses Skia internally — `SKBitmap` converts to `Avalonia.Bitmap` with zero-copy
5. **Thread-safe**: No STA thread requirement (unlike WPF BitmapImage)
6. **Small memory footprint**: Only stores decoded pixels, not a full ImageMagick context

**Magick.NET should continue to be used for**:
- HEIC format support (decoding HEIC files)
- PHash computation (which already uses MagickImage)
- Complex image processing (color profiles, filters)
- EXIF reading (via `GetExifProfile()`)

**SkiaSharp should be used for**:
- Image carrier type (`IImageData` implementation)
- Thumbnail generation (decode → resize → encode)
- Image display pipeline (load → rotate → display)
- Pixel access (DHash, brightness checks)
- JPEG/PNG/GIF encoding for storage

---

## Lessons Learned / Rules for Future Work

### 1. Never use MagickImage as a general-purpose image container
MagickImage is a heavyweight processing engine. Use it for specific tasks (HEIC decode, PHash,
EXIF), then immediately convert to a lightweight representation (SKBitmap or byte[]).

### 2. Avoid internal PNG storage
Storing images as PNG byte arrays forces encode+decode on every access. Instead, keep decoded
pixel data (SKBitmap) in memory and encode only when writing to disk/database.

### 3. Measure before and after every change
Run the relevant tests before AND after making changes. If a test file that took 2 seconds now
takes 10 seconds, something is wrong. The 4× regression was not caught because tests were only
run after ALL changes were made.

### 4. Follow the migration plan's library choice
The CROSS_PLATFORM_MIGRATION_PLAN.md explicitly states:
> "Use SkiaSharp `SKBitmap` as the cross-platform image carrier type replacing `BitmapImage`"

This was not a suggestion — it was the result of careful analysis of performance characteristics.

### 5. Don't change image byte sizes without understanding why
If thumbnail sizes change significantly, it means the encoding pipeline is different. This must
be investigated — different sizes mean different quality or format, which affects:
- Database storage size
- Network transfer (if syncing)
- Visual quality for users
- Hash values (breaking duplicate detection)

### 6. Avoid double-decode patterns
If you need pixel access from an IImageData, the implementation should provide direct access to
the decoded pixel buffer, not require re-decoding from a compressed format.

---

## Correct Approach for Phase 1.3

1. **Add SkiaSharp NuGet package** to PhotoManager.Common
2. **Create `SkiaImageData : IImageData`** that wraps `SKBitmap` (decoded pixels in memory)
3. **Rewrite `BitmapHelper.cs`** to use `SKBitmap.Decode()`, `SKCanvas` rotation, `SKBitmap.Resize()`
4. **Keep Magick.NET** for HEIC loading — but convert `MagickImage` → `SKBitmap` immediately after decode
5. **For DHash**: Use `SKBitmap` pixel access (direct, no re-encoding)
6. **For encoding** (GetJpegBitmapImage etc.): Use `SKImage.Encode()` from the SKBitmap
7. **Validate**: Thumbnail byte sizes should be similar to WPF output (same JPEG quality)
8. **Measure**: Run targeted tests before and after — ensure no >2× time increase per test class
