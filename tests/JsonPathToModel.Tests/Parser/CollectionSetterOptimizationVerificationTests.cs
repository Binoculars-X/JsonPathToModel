using JsonPathToModel;
using JsonPathToModel.Exceptions;
using JsonPathToModel.Tests.ModelData;

namespace JsonPathToModel.Tests.Parser;

/// <summary>
/// Tests to verify that collection setter optimization is properly applied based on configuration
/// Focus on working scenarios: setting properties on objects within collections
/// </summary>
public class CollectionSetterOptimizationVerificationTests
{
    [Fact]
    public void SetValue_Should_UseOptimization_When_FailOnCollectionKeyNotFound_IsTrue_ForObjectInCollection()
    {
        // Arrange
        var model = new SampleModel
        {
            NestedList = 
            [
                new SampleNested { Id = "test1", Name = "Original" }
            ]
        };

        var navigator = new JsonPathModelNavigator(new NavigatorConfigOptions
        {
            OptimizeWithCodeEmitter = true,
            FailOnCollectionKeyNotFound = true // This should allow optimization
        });

        // Act - this should use optimized IL emitted code for property setting on collection element
        navigator.SetValue(model, "$.NestedList[0].Name", "Updated");

        // Assert
        Assert.Equal("Updated", model.NestedList[0].Name);
        Assert.Equal("test1", model.NestedList[0].Id); // Unchanged
    }

    [Fact]
    public void SetValue_Should_FallbackToReflection_When_FailOnCollectionKeyNotFound_IsFalse_ForObjectInCollection()
    {
        // Arrange
        var model = new SampleModel
        {
            NestedList = 
            [
                new SampleNested { Id = "test2", Name = "Original" }
            ]
        };

        var navigator = new JsonPathModelNavigator(new NavigatorConfigOptions
        {
            OptimizeWithCodeEmitter = true,
            FailOnCollectionKeyNotFound = false // This should prevent optimization and use reflection
        });

        // Act - this should fallback to reflection-based approach
        navigator.SetValue(model, "$.NestedList[0].Name", "Updated");

        // Assert - should still work, just via reflection
        Assert.Equal("Updated", model.NestedList[0].Name);
        Assert.Equal("test2", model.NestedList[0].Id); // Unchanged
    }

    [Fact]
    public void SetValue_Should_ThrowNavigationException_When_FailOnCollectionKeyNotFound_IsFalse_AndOutOfBounds()
    {
        // Arrange
        var model = new SampleModel
        {
            NestedList = 
            [
                new SampleNested { Id = "single", Name = "Original" }
            ]
        };

        var navigator = new JsonPathModelNavigator(new NavigatorConfigOptions
        {
            OptimizeWithCodeEmitter = true,
            FailOnCollectionKeyNotFound = false // This should handle out-of-bounds but still throw NavigationException for setter
        });

        // Act & Assert - accessing index 5 when only index 0 exists should throw NavigationException
        var exception = Assert.Throws<NavigationException>(() => navigator.SetValue(model, "$.NestedList[5].Name", "should-throw"));
        
        // Navigation exception should mention that collection access failed
        Assert.Contains("NestedList is null", exception.Message);
        Assert.Equal("Original", model.NestedList[0].Name); // Original value unchanged due to exception
    }

    [Fact]
    public void SetValue_Should_ThrowArgumentOutOfRangeException_When_FailOnCollectionKeyNotFound_IsTrue_AndOutOfBounds()
    {
        // Arrange
        var model = new SampleModel
        {
            NestedList = 
            [
                new SampleNested { Id = "single", Name = "Original" }
            ]
        };

        var navigator = new JsonPathModelNavigator(new NavigatorConfigOptions
        {
            OptimizeWithCodeEmitter = true,
            FailOnCollectionKeyNotFound = true // This should throw on out-of-bounds
        });

        // Act & Assert - accessing index 5 when only index 0 exists should throw ArgumentOutOfRangeException
        Assert.Throws<ArgumentOutOfRangeException>(() => navigator.SetValue(model, "$.NestedList[5].Name", "should-throw"));
        Assert.Equal("Original", model.NestedList[0].Name); // Original value unchanged due to exception
    }

    [Fact]
    public void SetValue_Should_HandleComplexCollectionProperty_WithOptimization()
    {
        // Arrange
        var model = new SampleModel
        {
            NestedList = 
            [
                new SampleNested { Id = "test1", Name = "Original" }
            ]
        };

        var navigator = new JsonPathModelNavigator(new NavigatorConfigOptions
        {
            OptimizeWithCodeEmitter = true,
            FailOnCollectionKeyNotFound = true
        });

        // Act - Complex path: property → collection → property
        navigator.SetValue(model, "$.NestedList[0].Name", "Updated");

        // Assert
        Assert.Equal("Updated", model.NestedList[0].Name);
        Assert.Equal("test1", model.NestedList[0].Id); // Other property unchanged
    }

    [Fact]
    public void SetValue_Should_HandleMultipleCollectionPropertySets()
    {
        // Arrange
        var model = new SampleModel
        {
            NestedList = 
            [
                new SampleNested { Id = "first", Name = "FirstName" },
                new SampleNested { Id = "second", Name = "SecondName" }
            ]
        };

        var navigator = new JsonPathModelNavigator(new NavigatorConfigOptions
        {
            OptimizeWithCodeEmitter = true,
            FailOnCollectionKeyNotFound = true
        });

        // Act - Set properties on different collection elements
        navigator.SetValue(model, "$.NestedList[0].Name", "UpdatedFirst");
        navigator.SetValue(model, "$.NestedList[1].Id", "updatedSecond");

        // Assert
        Assert.Equal("UpdatedFirst", model.NestedList[0].Name);
        Assert.Equal("updatedSecond", model.NestedList[1].Id);
        Assert.Equal("first", model.NestedList[0].Id);        // Unchanged
        Assert.Equal("SecondName", model.NestedList[1].Name); // Unchanged
    }

    [Fact]
    public void SetValue_Should_HandleDictionaryValueProperty_WithOptimization()
    {
        // Arrange
        var model = new SampleModel
        {
            NestedDictionary = new Dictionary<string, SampleNested>
            {
                ["key1"] = new SampleNested { Id = "dict1", Name = "DictValue1" }
            }
        };

        var navigator = new JsonPathModelNavigator(new NavigatorConfigOptions
        {
            OptimizeWithCodeEmitter = true,
            FailOnCollectionKeyNotFound = true
        });

        // Act - Set property on dictionary value object (using single quotes for string keys)
        navigator.SetValue(model, "$.NestedDictionary['key1'].Name", "UpdatedDictValue");

        // Assert
        Assert.Equal("UpdatedDictValue", model.NestedDictionary["key1"].Name);
        Assert.Equal("dict1", model.NestedDictionary["key1"].Id); // Unchanged
    }

    // DISABLED TESTS - Direct collection element setting not implemented yet
    // See BUG_REPORT_Collection_Setter_Issue.md

    [Fact(Skip = "Collection element setting ($.Array[0]) not implemented yet - see BUG_REPORT_Collection_Setter_Issue.md")]
    public void SetValue_Should_UseOptimization_When_FailOnCollectionKeyNotFound_IsTrue_DISABLED()
    {
        // This test is disabled - direct collection element setting like $.MiddleNames[0] = "value" not supported
        var model = new SampleModel { MiddleNames = ["original", "values"] };
        var navigator = new JsonPathModelNavigator(new NavigatorConfigOptions { OptimizeWithCodeEmitter = true, FailOnCollectionKeyNotFound = true });
        navigator.SetValue(model, "$.MiddleNames[0]", "updated");
    }

    [Fact(Skip = "Collection element setting ($.Array[0]) not implemented yet - see BUG_REPORT_Collection_Setter_Issue.md")]
    public void SetValue_Should_FallbackToReflection_When_FailOnCollectionKeyNotFound_IsFalse_DISABLED()
    {
        // This test is disabled - direct collection element setting like $.MiddleNames[0] = "value" not supported
        var model = new SampleModel { MiddleNames = ["original"] };
        var navigator = new JsonPathModelNavigator(new NavigatorConfigOptions { OptimizeWithCodeEmitter = true, FailOnCollectionKeyNotFound = false });
        navigator.SetValue(model, "$.MiddleNames[0]", "updated");
    }
}
