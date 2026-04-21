---
name: perf
description: >
  Performance optimization for PhotoManager with benchmarking first.
  Use this skill when asked to optimize, improve speed, reduce allocations, or profile code.
---

You are optimizing performance for PhotoManager. Always benchmark before changing any code.

Follow this workflow in order:

1. **Create a BenchmarkDotNet benchmark file**:
   - Path: `PhotoManager/Benchmarks/PhotoManager.Benchmarks/{Category}/{ClassName}{MethodName}Benchmarks.cs`
   - Use `[MemoryDiagnoser]`, `[Orderer(SummaryOrderPolicy.FastestToSlowest)]`, `[RankColumn]`
   - Mark current implementation as `[Benchmark(Baseline = true)]`

2. **Keep the original method unchanged** in its source file — do NOT modify it yet.

3. **Add optimized variants IN THE BENCHMARK FILE ONLY**:
   - Name them clearly: `Optimized_ApproachName`, `Optimized_Stackalloc`, etc.

4. **Run the benchmark**:

   ```
   dotnet run --project PhotoManager/Benchmarks/PhotoManager.Benchmarks/PhotoManager.Benchmarks.csproj -c Release -- --filter "*BenchmarkClassName"
   ```

5. **Evaluate** (speed is priority, allocation shouldn't increase):
   - Lower Mean/Median = faster
   - Check Ratio column vs baseline

6. **Apply winner** to original source only if clearly better.
   Add comment: `// ClassName.MethodName: X% faster`
   Check `GlobalUsings.cs` before adding any `using` directives.

7. **Verify no regressions**:
   `dotnet test --filter "FullyQualifiedName~ClassName" PhotoManager/PhotoManager.slnx`

8. **Keep all variants** in the benchmark file for reference.
