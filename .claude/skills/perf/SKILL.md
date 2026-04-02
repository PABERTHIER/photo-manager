---
name: perf
description: >
  Performance optimization with benchmarking first.
  Use when asked to optimize, improve speed, reduce allocations, or profile code.
disable-model-invocation: true
argument-hint: <class.method to optimize>
---

You are tasked with optimizing the performance of: $ARGUMENTS

Follow this workflow in order:

1. **Create a BenchmarkDotNet benchmark file**:
   - Path: `PhotoManager/Benchmarks/PhotoManager.Benchmarks/{Category}/{ClassName}{MethodName}Benchmarks.cs`
   - Example: `PhotoManager/Benchmarks/PhotoManager.Benchmarks/Common/HashingHelperCalculateHashBenchmarks.cs`
   - Use attributes: `[MemoryDiagnoser]`, `[Orderer(SummaryOrderPolicy.FastestToSlowest)]`,
     `[RankColumn]`
   - Mark the CURRENT implementation as `[Benchmark(Baseline = true)]`

2. **Keep the original method unchanged** in its source file — do NOT modify it yet.

3. **Add optimized implementations IN THE BENCHMARK FILE ONLY**:
   - Each optimization gets its own `[Benchmark]` method
   - Name them clearly: `Optimized_ApproachName`, `Optimized_Stackalloc`, etc.

4. **Run the benchmark**:

   ```
   dotnet run --project PhotoManager/Benchmarks/PhotoManager.Benchmarks/PhotoManager.Benchmarks.csproj -c Release -- --filter "*BenchmarkClassName"
   ```

5. **Evaluate results** (speed is priority, but allocation shouldn't increase significantly):
   - Speed: Lower Mean/Median is better
   - Memory: Lower Allocated is better
   - Check the Ratio column compared to baseline

6. **Implement the winner** in the original source file:
   - Replace ONLY if there's clear improvement
   - Add comment: `// ClassName.MethodName: X% faster`
   - Check `GlobalUsings.cs` before adding any `using` directives

7. **Verify**: Run `dotnet test --filter "FullyQualifiedName~ClassName" PhotoManager/PhotoManager.slnx`
   to ensure no regressions. Build must produce zero warnings.

8. **Keep all variants** in the benchmark file (original + all optimizations) for future reference.

Return benchmark results summary with percentage improvement.
