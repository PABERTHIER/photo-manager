---
description: "Performance optimization with benchmarking first"
---

Optimize the performance of: ${input:target:Class.Method to optimize}

Follow this workflow:

1. **Create benchmark file** at `PhotoManager/Benchmarks/PhotoManager.Benchmarks/{Category}/{ClassName}{MethodName}Benchmarks.cs`.
   Use `[MemoryDiagnoser]`, `[Orderer(SummaryOrderPolicy.FastestToSlowest)]`, `[RankColumn]`.
   Mark current implementation as `[Benchmark(Baseline = true)]`.
2. **Keep original method unchanged** in source file.
3. **Add optimized variants** in the benchmark file only, named `Optimized_ApproachName`.
4. **Run benchmark**: `dotnet run --project PhotoManager/Benchmarks/PhotoManager.Benchmarks/PhotoManager.Benchmarks.csproj -c Release -- --filter "*BenchmarkClassName"`.
5. **Evaluate**: Speed is priority (lower Mean), allocation shouldn't increase significantly.
6. **Apply winner** to original source only if clearly better. Add comment: `// ClassName.MethodName: X% faster`.
7. **Verify**: `dotnet test --filter "FullyQualifiedName~ClassName" PhotoManager/PhotoManager.slnx` — no regressions.
8. **Keep all variants** in benchmark file for reference.
