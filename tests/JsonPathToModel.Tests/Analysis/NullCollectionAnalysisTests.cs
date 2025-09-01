using System;
using System.Collections.Generic;
using JsonPathToModel.Exceptions;
using JsonPathToModel.Tests.ModelData;
using Xunit;
using Xunit.Abstractions;

namespace JsonPathToModel.Tests.Analysis;

/// <summary>
/// Comprehensive analysis of null/empty collection scenarios for multi-property paths like "$.List[0].Value"
/// Focuses on unique scenarios not covered in other test suites:
/// - List containing null elements (not just out-of-bounds)
/// - Sigil optimization vs reflection consistency validation
/// - SetValue behavior with null collections
/// </summary>
public class NullCollectionAnalysisTests
{
    private readonly ITestOutputHelper _output;

    public NullCollectionAnalysisTests(ITestOutputHelper output)
    {
        _output = output;
    }

    public class TestModel
    {
        public List<TestItem>? List { get; set; }
    }

    public class TestItem
    {
        public string? Value { get; set; }
    }

    [Fact]
    public void Analyze_ListWithNullElement_Scenario()
    {
        _output.WriteLine("=== Scenario 1: List[0] is null (List contains null element) ===");
        
        var model = new TestModel
        {
            List = new List<TestItem> { null } // List has one null element
        };

        const string path = "$.List[0].Value";

        // With FailOnCollectionKeyNotFound = false (graceful handling)
        var navigatorGraceful = new JsonPathModelNavigator(new NavigatorConfigOptions
        {
            OptimizeWithCodeEmitter = true,
            FailOnCollectionKeyNotFound = false
        });

        try
        {
            var result = navigatorGraceful.GetValue(model, path);
            _output.WriteLine($"Graceful mode result: {(result == null ? "null" : $"\"{result}\"")}");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Graceful mode exception: {ex.GetType().Name}: {ex.Message}");
        }

        // With FailOnCollectionKeyNotFound = true (strict mode)
        var navigatorStrict = new JsonPathModelNavigator(new NavigatorConfigOptions
        {
            OptimizeWithCodeEmitter = true,
            FailOnCollectionKeyNotFound = true
        });

        try
        {
            var result = navigatorStrict.GetValue(model, path);
            _output.WriteLine($"Strict mode result: {(result == null ? "null" : $"\"{result}\"")}");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Strict mode exception: {ex.GetType().Name}: {ex.Message}");
        }

        // Expected: Library gracefully handles null elements by returning null (not throwing exception)
        // This is the correct behavior - the library detects null collection elements and returns null
        var gracefulResult = navigatorGraceful.GetValue(model, path);
        var strictResult = navigatorStrict.GetValue(model, path);
        
        Assert.Null(gracefulResult);
        Assert.Null(strictResult);
    }

    // Note: Empty list scenario testing is already covered in CollectionOptimizationVerificationTests
    // Focus here is on unique null element scenarios

    [Fact]
    public void Analyze_NullList_Scenario()
    {
        _output.WriteLine("=== Scenario 3: List is null ===");
        
        var model = new TestModel
        {
            List = null // Null list
        };

        const string path = "$.List[0].Value";

        // With FailOnCollectionKeyNotFound = false (graceful handling)
        var navigatorGraceful = new JsonPathModelNavigator(new NavigatorConfigOptions
        {
            OptimizeWithCodeEmitter = true,
            FailOnCollectionKeyNotFound = false
        });

        try
        {
            var result = navigatorGraceful.GetValue(model, path);
            _output.WriteLine($"Graceful mode result: {(result == null ? "null" : $"\"{result}\"")}");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Graceful mode exception: {ex.GetType().Name}: {ex.Message}");
        }

        // With FailOnCollectionKeyNotFound = true (strict mode)
        var navigatorStrict = new JsonPathModelNavigator(new NavigatorConfigOptions
        {
            OptimizeWithCodeEmitter = true,
            FailOnCollectionKeyNotFound = true
        });

        try
        {
            var result = navigatorStrict.GetValue(model, path);
            _output.WriteLine($"Strict mode result: {(result == null ? "null" : $"\"{result}\"")}");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Strict mode exception: {ex.GetType().Name}: {ex.Message}");
        }

        // Expected: Should return null when List property itself is null
        var gracefulResult = navigatorGraceful.GetValue(model, path);
        var strictResult = navigatorStrict.GetValue(model, path);
        
        Assert.Null(gracefulResult);
        Assert.Null(strictResult);
    }

    [Fact]
    public void Analyze_ValidScenario_ForComparison()
    {
        _output.WriteLine("=== Scenario 4: Valid scenario (List[0] exists with Value) ===");
        
        var model = new TestModel
        {
            List = new List<TestItem> { new TestItem { Value = "TestValue" } }
        };

        const string path = "$.List[0].Value";

        var navigator = new JsonPathModelNavigator(new NavigatorConfigOptions
        {
            OptimizeWithCodeEmitter = true,
            FailOnCollectionKeyNotFound = false
        });

        var result = navigator.GetValue(model, path);
        _output.WriteLine($"Valid scenario result: \"{result}\"");

        Assert.Equal("TestValue", result);
    }

    [Fact]
    public void Analyze_SigilOptimization_vs_Reflection_NullBehavior()
    {
        _output.WriteLine("=== Sigil Optimization vs Reflection Comparison ===");

        var model = new TestModel { List = null };
        const string path = "$.List[0].Value";

        // Test with Sigil optimization enabled
        var navigatorOptimized = new JsonPathModelNavigator(new NavigatorConfigOptions
        {
            OptimizeWithCodeEmitter = true,
            FailOnCollectionKeyNotFound = false
        });

        // Test with reflection fallback
        var navigatorReflection = new JsonPathModelNavigator(new NavigatorConfigOptions
        {
            OptimizeWithCodeEmitter = false,
            FailOnCollectionKeyNotFound = false
        });

        var optimizedResult = navigatorOptimized.GetValue(model, path);
        var reflectionResult = navigatorReflection.GetValue(model, path);

        _output.WriteLine($"Optimized result: {(optimizedResult == null ? "null" : optimizedResult)}");
        _output.WriteLine($"Reflection result: {(reflectionResult == null ? "null" : reflectionResult)}");

        // Both should have consistent behavior
        Assert.Equal(optimizedResult, reflectionResult);
    }

    [Fact]
    public void Analyze_SetValue_NullScenarios()
    {
        _output.WriteLine("=== SetValue Analysis for Null Scenarios ===");

        // Scenario: Try to set value when List is null
        var model = new TestModel { List = null };
        const string path = "$.List[0].Value";

        var navigator = new JsonPathModelNavigator(new NavigatorConfigOptions
        {
            OptimizeWithCodeEmitter = true,
            FailOnCollectionKeyNotFound = false
        });

        try
        {
            navigator.SetValue(model, path, "NewValue");
            _output.WriteLine("SetValue succeeded on null list");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"SetValue failed: {ex.GetType().Name}: {ex.Message}");
            // Expected to throw since we can't set on null collection
            Assert.True(ex is NullReferenceException or NavigationException);
        }

        // Scenario: Try to set value when List is empty
        model.List = new List<TestItem>();
        
        try
        {
            navigator.SetValue(model, path, "NewValue");
            _output.WriteLine("SetValue succeeded on empty list");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"SetValue failed on empty list: {ex.GetType().Name}: {ex.Message}");
            // Expected: SetValue throws NavigationException for empty collections
            // (Different from GetValue which throws ArgumentOutOfRangeException)
            Assert.IsType<NavigationException>(ex);
        }
    }
}