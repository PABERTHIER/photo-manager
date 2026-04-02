---
applyTo: "**/Benchmarks/**/*.cs"
---

# Benchmark Code Standards for PhotoManager

## Framework
- BenchmarkDotNet

## File Location
- `PhotoManager/Benchmarks/PhotoManager.Benchmarks/{Category}/{ClassName}{MethodName}Benchmarks.cs`

## Required Attributes
- `[MemoryDiagnoser]` on the benchmark class
- `[Orderer(SummaryOrderPolicy.FastestToSlowest)]` on the benchmark class
- `[RankColumn]` on the benchmark class
- `[Benchmark(Baseline = true)]` on the current implementation method

## Conventions
- Name optimized variants clearly: `Optimized_ApproachName`, `Optimized_Stackalloc`, etc.
- Keep ALL implementations (original + optimizations) in the benchmark file for reference
- After a winner is found and applied, add comment: `// ClassName.MethodName: X% faster`
- Run with: `dotnet run -c Release -- --filter "*BenchmarkClassName"`

## Evaluation Priority
1. Speed (lower Mean/Median is better)
2. Memory (lower Allocated is better, should not increase significantly)
3. Check the Ratio column compared to baseline
