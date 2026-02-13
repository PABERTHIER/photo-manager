# PhotoManager Benchmarks

This project contains performance benchmarks for the PhotoManager solution using BenchmarkDotNet.

## Running Benchmarks

To run the benchmarks, execute the following command from the PhotoManager.Benchmarks directory:

```powershell
# Run all benchmarks
dotnet run -c Release

# Run specific benchmark
dotnet run -c Release --filter *Hashing*

# List available benchmarks
dotnet run -c Release --list
```

**Important:** Always run benchmarks in Release mode for accurate results.

## Understanding Results

BenchmarkDotNet will provide:

- **Mean**: Average execution time
- **Error**: Half of the 99.9% confidence interval
- **StdDev**: Standard deviation
- **Median**: Median execution time
- **Allocated**: Memory allocated per operation

Look for:

1. Lower execution times (Mean)
2. Lower memory allocations (Allocated)
3. Lower standard deviation (more consistent performance)
