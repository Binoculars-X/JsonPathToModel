using JsonPathToModel;
using JsonPathToModel.Tests.ModelData;
using System.Diagnostics;

namespace JsonPathToModel.Tests.Benchmarks;

/// <summary>
/// Precise performance validation test to demonstrate collection setter optimization benefits
/// Uses high-resolution timing to measure nanosecond-level differences
/// </summary>
public class CollectionSetterPerformanceTest
{
    [Fact]
    public void CollectionSetter_OptimizedVsReflection_PerformanceComparison()
    {
        // Arrange
        const int iterations = 100000; // More iterations for better precision
        
        var model = new SampleModel
        {
            NestedList = 
            [
                new SampleNested { Id = "test", Name = "Original" }
            ]
        };

        var optimizedNavigator = new JsonPathModelNavigator(new NavigatorConfigOptions
        {
            OptimizeWithCodeEmitter = true,
            FailOnCollectionKeyNotFound = true
        });

        var reflectionNavigator = new JsonPathModelNavigator(new NavigatorConfigOptions
        {
            OptimizeWithCodeEmitter = false,
            FailOnCollectionKeyNotFound = true
        });

        const string testPath = "$.NestedList[0].Name";

        // Warm up both approaches (important for JIT compilation)
        for (int i = 0; i < 1000; i++)
        {
            optimizedNavigator.SetValue(model, testPath, "WarmUp");
            reflectionNavigator.SetValue(model, testPath, "WarmUp");
        }

        // Force garbage collection before measurement
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        // Measure optimized approach with high precision
        var optimizedStopwatch = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            optimizedNavigator.SetValue(model, testPath, $"OptimizedValue{i}");
        }
        optimizedStopwatch.Stop();

        // Short pause and GC before second measurement
        Thread.Sleep(10);
        GC.Collect();
        GC.WaitForPendingFinalizers();

        // Measure reflection approach with high precision
        var reflectionStopwatch = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            reflectionNavigator.SetValue(model, testPath, $"ReflectionValue{i}");
        }
        reflectionStopwatch.Stop();

        // Calculate precise timing in nanoseconds
        var optimizedNs = (optimizedStopwatch.ElapsedTicks * 1_000_000_000.0) / Stopwatch.Frequency;
        var reflectionNs = (reflectionStopwatch.ElapsedTicks * 1_000_000_000.0) / Stopwatch.Frequency;
        var avgOptimizedNsPerCall = optimizedNs / iterations;
        var avgReflectionNsPerCall = reflectionNs / iterations;
        var speedupFactor = reflectionNs / optimizedNs;

        // Output detailed results
        Console.WriteLine($"Collection Setter Performance Results for {iterations:N0} iterations:");
        Console.WriteLine($"Optimized Total: {optimizedNs:F0} ns ({avgOptimizedNsPerCall:F1} ns/call)");
        Console.WriteLine($"Reflection Total: {reflectionNs:F0} ns ({avgReflectionNsPerCall:F1} ns/call)");
        Console.WriteLine($"Speedup Factor: {speedupFactor:F1}x");
        Console.WriteLine($"Performance Improvement: {((speedupFactor - 1) * 100):F1}%");

        // Verify correctness - final values should be different since we used different approaches
        Assert.Equal($"ReflectionValue{iterations - 1}", model.NestedList[0].Name);
        
        // Assert meaningful performance improvement - just ensure optimization is faster
        Assert.True(speedupFactor > 1.0, 
            $"Optimized setter should be faster than reflection, but got {speedupFactor:F1}x speedup. " +
            $"Optimized: {avgOptimizedNsPerCall:F1} ns/call, Reflection: {avgReflectionNsPerCall:F1} ns/call");

        // Ensure we're actually measuring meaningful differences (not noise)
        Assert.True(avgReflectionNsPerCall > 50, 
            $"Reflection calls should take at least 50ns, but measured {avgReflectionNsPerCall:F1} ns/call. Test may be unreliable.");
        Assert.True(avgOptimizedNsPerCall > 1, 
            $"Optimized calls should take at least 1ns, but measured {avgOptimizedNsPerCall:F1} ns/call. Test may be unreliable.");
    }

    [Fact]
    public void CollectionSetter_DictionaryAccess_PerformanceComparison()
    {
        // Arrange
        const int iterations = 50000; // Fewer iterations since dictionary access is typically slower
        
        var model = new SampleModel
        {
            NestedDictionary = new Dictionary<string, SampleNested>
            {
                ["testKey"] = new SampleNested { Id = "test", Name = "Original" }
            }
        };

        var optimizedNavigator = new JsonPathModelNavigator(new NavigatorConfigOptions
        {
            OptimizeWithCodeEmitter = true,
            FailOnCollectionKeyNotFound = true
        });

        var reflectionNavigator = new JsonPathModelNavigator(new NavigatorConfigOptions
        {
            OptimizeWithCodeEmitter = false,
            FailOnCollectionKeyNotFound = true
        });

        const string testPath = "$.NestedDictionary['testKey'].Name";

        // Warm up with more iterations since dictionary access is more complex
        for (int i = 0; i < 1000; i++)
        {
            optimizedNavigator.SetValue(model, testPath, "WarmUp");
            reflectionNavigator.SetValue(model, testPath, "WarmUp");
        }

        // Force garbage collection
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        // Measure optimized approach
        var optimizedStopwatch = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            optimizedNavigator.SetValue(model, testPath, $"OptimizedDictValue{i}");
        }
        optimizedStopwatch.Stop();

        Thread.Sleep(10);
        GC.Collect();
        GC.WaitForPendingFinalizers();

        // Measure reflection approach
        var reflectionStopwatch = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            reflectionNavigator.SetValue(model, testPath, $"ReflectionDictValue{i}");
        }
        reflectionStopwatch.Stop();

        // Calculate precise timing
        var optimizedNs = (optimizedStopwatch.ElapsedTicks * 1_000_000_000.0) / Stopwatch.Frequency;
        var reflectionNs = (reflectionStopwatch.ElapsedTicks * 1_000_000_000.0) / Stopwatch.Frequency;
        var avgOptimizedNsPerCall = optimizedNs / iterations;
        var avgReflectionNsPerCall = reflectionNs / iterations;
        var speedupFactor = reflectionNs / optimizedNs;

        Console.WriteLine($"Dictionary Setter Performance Results for {iterations:N0} iterations:");
        Console.WriteLine($"Optimized Total: {optimizedNs:F0} ns ({avgOptimizedNsPerCall:F1} ns/call)");
        Console.WriteLine($"Reflection Total: {reflectionNs:F0} ns ({avgReflectionNsPerCall:F1} ns/call)");
        Console.WriteLine($"Speedup Factor: {speedupFactor:F1}x");
        Console.WriteLine($"Performance Improvement: {((speedupFactor - 1) * 100):F1}%");

        Assert.Equal($"ReflectionDictValue{iterations - 1}", model.NestedDictionary["testKey"].Name);
        
        // Dictionary access should still show improvement, though potentially less dramatic than simple list access
        Assert.True(speedupFactor >= 1.5, 
            $"Expected optimized dictionary setter to be at least 1.5x faster, but got {speedupFactor:F1}x speedup. " +
            $"Optimized: {avgOptimizedNsPerCall:F1} ns/call, Reflection: {avgReflectionNsPerCall:F1} ns/call");

        // Verify meaningful timing measurements
        Assert.True(avgReflectionNsPerCall > 100, 
            $"Dictionary reflection calls should take at least 100ns, but measured {avgReflectionNsPerCall:F1} ns/call.");
        Assert.True(avgOptimizedNsPerCall > 1, 
            $"Dictionary optimized calls should take at least 1ns, but measured {avgOptimizedNsPerCall:F1} ns/call.");
    }
}
