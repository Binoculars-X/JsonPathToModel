using JsonPathToModel;
using JsonPathToModel.Tests.ModelData;

namespace JsonPathToModel.Tests.Parser;

/// <summary>
/// Tests to verify that collection optimization is properly applied based on configuration
/// </summary>
public class CollectionOptimizationVerificationTests
{
    [Fact]
    public void GetValue_Should_UseOptimization_When_FailOnCollectionKeyNotFound_IsTrue()
    {
        // Arrange
        var model = new SampleModel
        {
            Name = "Test",
            MiddleNames = ["John", "William"]
        };

        var navigator = new JsonPathModelNavigator(new NavigatorConfigOptions
        {
            OptimizeWithCodeEmitter = true,
            FailOnCollectionKeyNotFound = true // This should allow optimization
        });

        // Act - this should use optimized IL emitted code
        var result = navigator.GetValue(model, "$.MiddleNames[0]");

        // Assert
        Assert.Equal("John", result);
    }

    [Fact]
    public void GetValue_Should_FallbackToReflection_When_FailOnCollectionKeyNotFound_IsFalse()
    {
        // Arrange
        var model = new SampleModel
        {
            Name = "Test",
            MiddleNames = ["John"]
        };

        var navigator = new JsonPathModelNavigator(new NavigatorConfigOptions
        {
            OptimizeWithCodeEmitter = true,
            FailOnCollectionKeyNotFound = false // This should prevent optimization and use reflection
        });

        // Act - this should fallback to reflection-based approach
        var result = navigator.GetValue(model, "$.MiddleNames[0]");

        // Assert - should still work, just via reflection
        Assert.Equal("John", result);
    }

    [Fact]
    public void GetValue_Should_HandleOutOfBounds_Gracefully_When_FailOnCollectionKeyNotFound_IsFalse()
    {
        // Arrange
        var model = new SampleModel
        {
            Name = "Test",
            MiddleNames = ["John"]
        };

        var navigator = new JsonPathModelNavigator(new NavigatorConfigOptions
        {
            OptimizeWithCodeEmitter = true,
            FailOnCollectionKeyNotFound = false // This should handle out-of-bounds gracefully
        });

        // Act - accessing index 5 when only index 0 exists
        var result = navigator.GetValue(model, "$.MiddleNames[5]");

        // Assert - should return null instead of throwing exception
        Assert.Null(result);
    }

    [Fact]
    public void GetValue_Should_ThrowException_When_FailOnCollectionKeyNotFound_IsTrue_AndOutOfBounds()
    {
        // Arrange
        var model = new SampleModel
        {
            Name = "Test", 
            MiddleNames = ["John"]
        };

        var navigator = new JsonPathModelNavigator(new NavigatorConfigOptions
        {
            OptimizeWithCodeEmitter = true,
            FailOnCollectionKeyNotFound = true // This should throw on out-of-bounds
        });

        // Act & Assert - accessing index 5 when only index 0 exists should throw
        Assert.Throws<IndexOutOfRangeException>(() => navigator.GetValue(model, "$.MiddleNames[5]"));
    }
}
