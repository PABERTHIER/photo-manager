using PhotoManager.Domain;

namespace PhotoManager.Benchmarks.Domain;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class FolderIsParentOfBenchmarks
{
    private Folder _parent = null!;
    private Folder _child = null!;
    private Folder _unrelated = null!;

    [GlobalSetup]
    public void Setup()
    {
        _parent = new() { Id = Guid.NewGuid(), Path = @"C:\Users\Photos" };
        _child = new() { Id = Guid.NewGuid(), Path = @"C:\Users\Photos\Holidays" };
        _unrelated = new() { Id = Guid.NewGuid(), Path = @"C:\Documents\Work" };
    }

    // ── IsParentOf ────────────────────────────────────────────────────────────

    [Benchmark(Baseline = true)]
    [Arguments("child")]
    [Arguments("unrelated")]
    public bool IsParentOf_Original(string target)
    {
        Folder folder = target == "child" ? _child : _unrelated;

        // Replicates the original logic, where folder.Parent is called twice
        return !string.IsNullOrWhiteSpace(_parent.Path)
               && !string.IsNullOrWhiteSpace(folder.GetParent()?.Path)
               && string.Compare(_parent.Path, folder.GetParent()?.Path, StringComparison.OrdinalIgnoreCase) == 0;
    }

    [Benchmark]
    [Arguments("child")]
    [Arguments("unrelated")]
    public bool IsParentOf_Optimized(string target)
    {
        Folder folder = target == "child" ? _child : _unrelated;

        if (string.IsNullOrWhiteSpace(_parent.Path))
        {
            return false;
        }

        string? parentPath = GetParentPath_Optimized(folder.Path);

        return !string.IsNullOrWhiteSpace(parentPath)
               && string.Compare(_parent.Path, parentPath, StringComparison.OrdinalIgnoreCase) == 0;
    }

    // ── GetParentPath ─────────────────────────────────────────────────────────

    [Benchmark]
    [Arguments(@"C:\Users\Photos\Holidays")]
    [Arguments(@"C:\Users\Photos")]
    [Arguments(@"C:\")]
    public static string? GetParentPath_Original(string path)
    {
        string[] directoriesPath = path.Split(Path.DirectorySeparatorChar);
        directoriesPath = [.. directoriesPath.SkipLast(1)];

        return directoriesPath.Length > 0 ? Path.Combine(directoriesPath) : null;
    }

    [Benchmark]
    [Arguments(@"C:\Users\Photos\Holidays")]
    [Arguments(@"C:\Users\Photos")]
    [Arguments(@"C:\")]
    public static string? GetParentPath_Optimized_Span(string path)
    {
        ReadOnlySpan<char> pathAsSpan = path.AsSpan().TrimEnd(Path.DirectorySeparatorChar);
        int lastSeparator = pathAsSpan.LastIndexOf(Path.DirectorySeparatorChar);

        return lastSeparator >= 0 ? pathAsSpan[..lastSeparator].ToString() : null;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string? GetParentPath_Optimized(string? path)
    {
        if (path == null)
        {
            return null;
        }

        ReadOnlySpan<char> pathAsSpan = path.AsSpan().TrimEnd(Path.DirectorySeparatorChar);
        int lastSeparator = pathAsSpan.LastIndexOf(Path.DirectorySeparatorChar);

        return lastSeparator >= 0 ? pathAsSpan[..lastSeparator].ToString() : null;
    }
}

// Expose the private Parent property for benchmarking the original behaviour
file static class FolderExtensions
{
    public static Folder? GetParent(this Folder folder)
    {
        string? parentPath = GetParentPath(folder.Path);

        return parentPath != null ? new() { Id = Guid.NewGuid(), Path = parentPath } : null;
    }

    private static string? GetParentPath(string? path)
    {
        string[]? directoriesPath = path?.Split(Path.DirectorySeparatorChar);
        directoriesPath = directoriesPath == null ? null : [.. directoriesPath.SkipLast(1)];

        return directoriesPath?.Length > 0 ? Path.Combine(directoriesPath) : null;
    }
}
