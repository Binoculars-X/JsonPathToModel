using JsonPathToModel;
using JsonPathToModel.Tests.ModelData;
using System.Diagnostics;

namespace JsonPathToModel.Tests.Benchmarks;

/// <summary>
/// Performance comparison test for getter vs setter operations using value types
/// Minimizes GC impact by using int, decimal, DateTime, and bool properties
/// Compares optimized vs non-optimized performance for both getter and setter operations
/// </summary>
public class GetterVsSetterPerformanceTest
{
    [Fact]
    public void GetterVsSetter_ValueTypes_PerformanceComparison()
    {
        // Arrange - Use value types to minimize GC impact
        const int iterations = 100000;
        
        var model = new SampleModelAllTypes
        {
            NestedList = 
            [
                new SampleModelAllTypesNested 
                { 
                    Int = 42,
                    Decimal = 123.45m,
                    DateTime = DateTime.Parse("2023-01-01"),
                    Double = 99.99
                }
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

        const string intPath = "$.NestedList[0].Int";
        const string decimalPath = "$.NestedList[0].Decimal";
        const string datePath = "$.NestedList[0].DateTime";
        const string doublePath = "$.NestedList[0].Double";

        // Warm up JIT for all operations
        WarmUpOperations(optimizedNavigator, reflectionNavigator, model, intPath, decimalPath, datePath, doublePath);

        // Force garbage collection before measurements
        ForceGarbageCollection();

        // Measure optimized getter performance
        var optimizedGetterResults = MeasureGetterPerformance(optimizedNavigator, model, 
            intPath, decimalPath, datePath, doublePath, iterations);

        ForceGarbageCollection();

        // Measure reflection getter performance  
        var reflectionGetterResults = MeasureGetterPerformance(reflectionNavigator, model,
            intPath, decimalPath, datePath, doublePath, iterations);

        ForceGarbageCollection();

        // Measure optimized setter performance
        var optimizedSetterResults = MeasureSetterPerformance(optimizedNavigator, model,
            intPath, decimalPath, datePath, doublePath, iterations);

        ForceGarbageCollection();

        // Measure reflection setter performance
        var reflectionSetterResults = MeasureSetterPerformance(reflectionNavigator, model,
            intPath, decimalPath, datePath, doublePath, iterations);

        // Calculate performance metrics
        var getterSpeedup = (double)reflectionGetterResults.TotalNanoseconds / optimizedGetterResults.TotalNanoseconds;
        var setterSpeedup = (double)reflectionSetterResults.TotalNanoseconds / optimizedSetterResults.TotalNanoseconds;
        var optimizedRatio = (double)optimizedSetterResults.TotalNanoseconds / optimizedGetterResults.TotalNanoseconds;
        var reflectionRatio = (double)reflectionSetterResults.TotalNanoseconds / reflectionGetterResults.TotalNanoseconds;

        // Output comprehensive results
        Console.WriteLine($"Value Types Performance Analysis ({iterations:N0} iterations each):");
        Console.WriteLine();
        Console.WriteLine("=== GETTER PERFORMANCE ===");
        Console.WriteLine($"Optimized Getter:   {optimizedGetterResults.TotalNanoseconds:F0} ns ({optimizedGetterResults.AvgNsPerCall:F1} ns/call)");
        Console.WriteLine($"Reflection Getter:  {reflectionGetterResults.TotalNanoseconds:F0} ns ({reflectionGetterResults.AvgNsPerCall:F1} ns/call)");
        Console.WriteLine($"Getter Speedup:     {getterSpeedup:F1}x");
        Console.WriteLine();
        Console.WriteLine("=== SETTER PERFORMANCE ===");
        Console.WriteLine($"Optimized Setter:   {optimizedSetterResults.TotalNanoseconds:F0} ns ({optimizedSetterResults.AvgNsPerCall:F1} ns/call)");
        Console.WriteLine($"Reflection Setter:  {reflectionSetterResults.TotalNanoseconds:F0} ns ({reflectionSetterResults.AvgNsPerCall:F1} ns/call)");
        Console.WriteLine($"Setter Speedup:     {setterSpeedup:F1}x");
        Console.WriteLine();
        Console.WriteLine("=== GETTER vs SETTER RATIOS ===");
        Console.WriteLine($"Optimized Setter/Getter:   {optimizedRatio:F1}x");
        Console.WriteLine($"Reflection Setter/Getter:  {reflectionRatio:F1}x");
        Console.WriteLine();

        // Assertions for performance requirements
        Assert.True(getterSpeedup >= 2.0, 
            $"Optimized getter should be at least 2.0x faster, got {getterSpeedup:F1}x");
        
        Assert.True(setterSpeedup >= 2.0, 
            $"Optimized setter should be at least 2.0x faster, got {setterSpeedup:F1}x");

        // Verify that both optimized operations are reasonably fast (< 500ns per call on average)
        Assert.True(optimizedGetterResults.AvgNsPerCall < 500, 
            $"Optimized getter should be < 500ns/call, got {optimizedGetterResults.AvgNsPerCall:F1}ns/call");
        
        Assert.True(optimizedSetterResults.AvgNsPerCall < 500, 
            $"Optimized setter should be < 500ns/call, got {optimizedSetterResults.AvgNsPerCall:F1}ns/call");

        // Setter should typically be slightly slower than getter due to additional validation
        Assert.True(optimizedRatio >= 0.8 && optimizedRatio <= 3.0, 
            $"Optimized setter/getter ratio should be 0.8-3.0x, got {optimizedRatio:F1}x");
    }

    private static void WarmUpOperations(JsonPathModelNavigator optimizedNav, JsonPathModelNavigator reflectionNav, 
        SampleModelAllTypes model, string intPath, string decimalPath, string datePath, string doublePath)
    {
        // Warm up all operation types to ensure JIT compilation
        for (int i = 0; i < 1000; i++)
        {
            // Getter warmup
            optimizedNav.GetValue(model, intPath);
            optimizedNav.GetValue(model, decimalPath);
            reflectionNav.GetValue(model, intPath);
            reflectionNav.GetValue(model, decimalPath);

            // Setter warmup
            optimizedNav.SetValue(model, intPath, i);
            optimizedNav.SetValue(model, decimalPath, 100.50m + i);
            reflectionNav.SetValue(model, intPath, i);
            reflectionNav.SetValue(model, decimalPath, 200.75m + i);
        }
    }

    private static PerformanceResult MeasureGetterPerformance(JsonPathModelNavigator navigator, 
        SampleModelAllTypes model, string intPath, string decimalPath, string datePath, string doublePath, int iterations)
    {
        var stopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < iterations; i++)
        {
            // Mix of different value types to represent realistic usage
            switch (i % 4)
            {
                case 0:
                    navigator.GetValue(model, intPath);
                    break;
                case 1:
                    navigator.GetValue(model, decimalPath);
                    break;
                case 2:
                    navigator.GetValue(model, datePath);
                    break;
                case 3:
                    navigator.GetValue(model, doublePath);
                    break;
            }
        }
        
        stopwatch.Stop();
        return new PerformanceResult(stopwatch, iterations);
    }

    private static PerformanceResult MeasureSetterPerformance(JsonPathModelNavigator navigator,
        SampleModelAllTypes model, string intPath, string decimalPath, string datePath, string doublePath, int iterations)
    {
        var baseDate = DateTime.Parse("2024-01-01");
        var stopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < iterations; i++)
        {
            // Mix of different value types - using rotation to avoid patterns
            switch (i % 4)
            {
                case 0:
                    navigator.SetValue(model, intPath, 1000 + i);
                    break;
                case 1:
                    navigator.SetValue(model, decimalPath, 500.50m + (i * 0.01m));
                    break;
                case 2:
                    navigator.SetValue(model, datePath, baseDate.AddDays(i % 365));
                    break;
                case 3:
                    navigator.SetValue(model, doublePath, 999.99 + (i * 0.001));
                    break;
            }
        }
        
        stopwatch.Stop();
        return new PerformanceResult(stopwatch, iterations);
    }

    private static void ForceGarbageCollection()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        Thread.Sleep(10); // Allow system to stabilize
    }

    private record PerformanceResult(double TotalNanoseconds, double AvgNsPerCall)
    {
        public PerformanceResult(Stopwatch stopwatch, int iterations) : 
            this((stopwatch.ElapsedTicks * 1_000_000_000.0) / Stopwatch.Frequency,
                 ((stopwatch.ElapsedTicks * 1_000_000_000.0) / Stopwatch.Frequency) / iterations)
        {
        }
    }

    [Fact]
    public void GetterVsSetter_DictionaryAccess_PerformanceComparison()
    {
        // Arrange - Test dictionary access with value types
        const int iterations = 50000; // Fewer iterations for dictionary operations
        
        var model = new SampleModelAllTypes
        {
            NestedDictionary = new Dictionary<string, SampleModelAllTypesNested>
            {
                ["valueTypes"] = new SampleModelAllTypesNested 
                { 
                    Int = 999,
                    Decimal = 888.77m,
                    DateTime = DateTime.Parse("2023-12-31"),
                    Double = 777.66
                }
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

        const string dictIntPath = "$.NestedDictionary['valueTypes'].Int";
        const string dictDecimalPath = "$.NestedDictionary['valueTypes'].Decimal";

        // Warm up
        for (int i = 0; i < 500; i++)
        {
            optimizedNavigator.GetValue(model, dictIntPath);
            optimizedNavigator.SetValue(model, dictIntPath, i);
            reflectionNavigator.GetValue(model, dictIntPath);
            reflectionNavigator.SetValue(model, dictIntPath, i);
        }

        ForceGarbageCollection();

        // Measure dictionary getter performance
        var optimizedDictGetterStopwatch = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            optimizedNavigator.GetValue(model, i % 2 == 0 ? dictIntPath : dictDecimalPath);
        }
        optimizedDictGetterStopwatch.Stop();

        ForceGarbageCollection();

        var reflectionDictGetterStopwatch = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            reflectionNavigator.GetValue(model, i % 2 == 0 ? dictIntPath : dictDecimalPath);
        }
        reflectionDictGetterStopwatch.Stop();

        ForceGarbageCollection();

        // Measure dictionary setter performance
        var optimizedDictSetterStopwatch = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            if (i % 2 == 0)
                optimizedNavigator.SetValue(model, dictIntPath, 2000 + i);
            else
                optimizedNavigator.SetValue(model, dictDecimalPath, 1000.10m + i);
        }
        optimizedDictSetterStopwatch.Stop();

        ForceGarbageCollection();

        var reflectionDictSetterStopwatch = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            if (i % 2 == 0)
                reflectionNavigator.SetValue(model, dictIntPath, 3000 + i);
            else
                reflectionNavigator.SetValue(model, dictDecimalPath, 2000.20m + i);
        }
        reflectionDictSetterStopwatch.Stop();

        // Calculate results
        var optimizedDictGetter = new PerformanceResult(optimizedDictGetterStopwatch, iterations);
        var reflectionDictGetter = new PerformanceResult(reflectionDictGetterStopwatch, iterations);
        var optimizedDictSetter = new PerformanceResult(optimizedDictSetterStopwatch, iterations);
        var reflectionDictSetter = new PerformanceResult(reflectionDictSetterStopwatch, iterations);

        var dictGetterSpeedup = reflectionDictGetter.TotalNanoseconds / optimizedDictGetter.TotalNanoseconds;
        var dictSetterSpeedup = reflectionDictSetter.TotalNanoseconds / optimizedDictSetter.TotalNanoseconds;

        // Output results
        Console.WriteLine($"Dictionary Value Types Performance ({iterations:N0} iterations):");
        Console.WriteLine($"Dict Getter - Optimized: {optimizedDictGetter.AvgNsPerCall:F1} ns/call, Reflection: {reflectionDictGetter.AvgNsPerCall:F1} ns/call, Speedup: {dictGetterSpeedup:F1}x");
        Console.WriteLine($"Dict Setter - Optimized: {optimizedDictSetter.AvgNsPerCall:F1} ns/call, Reflection: {reflectionDictSetter.AvgNsPerCall:F1} ns/call, Speedup: {dictSetterSpeedup:F1}x");

        // Assert dictionary performance improvements
        Assert.True(dictGetterSpeedup >= 1.5, $"Dictionary getter speedup should be >= 1.5x, got {dictGetterSpeedup:F1}x");
        Assert.True(dictSetterSpeedup >= 1.5, $"Dictionary setter speedup should be >= 1.5x, got {dictSetterSpeedup:F1}x");
    }
}
