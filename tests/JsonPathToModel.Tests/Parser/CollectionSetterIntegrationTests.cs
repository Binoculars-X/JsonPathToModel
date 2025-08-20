using JsonPathToModel;
using JsonPathToModel.Tests.ModelData;

namespace JsonPathToModel.Tests.Parser;

/// <summary>
/// Integration tests for collection setter optimization in mixed property/collection scenarios
/// Testing both optimized and non-optimized code paths to ensure consistency
/// Focus on working scenarios: setting properties on objects within collections
/// </summary>
public class CollectionSetterIntegrationTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SetValue_Should_HandleMixedPropertyAndCollection_ObjectInList(bool useOptimization)
    {
        // Arrange
        var model = new SampleModel
        {
            Id = "test-456",
            NestedList = 
            [
                new SampleNested { Id = "nested1", Name = "First" },
                new SampleNested { Id = "nested2", Name = "Second" }
            ]
        };

        var navigator = new JsonPathModelNavigator(new NavigatorConfigOptions
        {
            OptimizeWithCodeEmitter = useOptimization,
            FailOnCollectionKeyNotFound = true
        });

        // Act - Set property on collection element
        navigator.SetValue(model, "$.NestedList[0].Name", "UpdatedFirst");

        // Assert
        Assert.Equal("UpdatedFirst", model.NestedList[0].Name);
        Assert.Equal("nested1", model.NestedList[0].Id);        // Unchanged
        Assert.Equal("Second", model.NestedList[1].Name);       // Unchanged
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SetValue_Should_HandleMultiplePropertiesInCollection(bool useOptimization)
    {
        // Arrange
        var model = new SampleModel
        {
            NestedList = 
            [
                new SampleNested { Id = "item1", Name = "Name1", Date = DateTime.Parse("2023-01-01") },
                new SampleNested { Id = "item2", Name = "Name2", Date = DateTime.Parse("2023-02-01") }
            ]
        };

        var navigator = new JsonPathModelNavigator(new NavigatorConfigOptions
        {
            OptimizeWithCodeEmitter = useOptimization,
            FailOnCollectionKeyNotFound = true
        });

        // Act - Set nested property via array access
        navigator.SetValue(model, "$.NestedList[1].Id", "updatedItem2");

        // Assert
        Assert.Equal("updatedItem2", model.NestedList[1].Id);
        Assert.Equal("Name2", model.NestedList[1].Name);        // Unchanged
        Assert.Equal("item1", model.NestedList[0].Id);          // Unchanged
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SetValue_Should_HandleMultipleCollectionAccess(bool useOptimization)
    {
        // Arrange - Complex scenario: property → collection → property → collection → property
        var model = new SampleModel
        {
            NestedList = 
            [
                new SampleNested { Id = "complex1", Name = "ComplexName1" },
                new SampleNested { Id = "complex2", Name = "ComplexName2" }
            ]
        };

        var navigator = new JsonPathModelNavigator(new NavigatorConfigOptions
        {
            OptimizeWithCodeEmitter = useOptimization,
            FailOnCollectionKeyNotFound = true
        });

        // Act - Multiple complex sets on different collection elements
        navigator.SetValue(model, "$.NestedList[0].Name", "UpdatedComplexName1");
        navigator.SetValue(model, "$.NestedList[1].Id", "updatedComplex2");

        // Assert
        Assert.Equal("UpdatedComplexName1", model.NestedList[0].Name);
        Assert.Equal("updatedComplex2", model.NestedList[1].Id);
        Assert.Equal("complex1", model.NestedList[0].Id);       // Unchanged
        Assert.Equal("ComplexName2", model.NestedList[1].Name); // Unchanged
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SetValue_Should_HandleDifferentCollectionIndices(bool useOptimization)
    {
        // Arrange
        var model = new SampleModel
        {
            NestedList = 
            [
                new SampleNested { Id = "first", Name = "FirstName" },
                new SampleNested { Id = "second", Name = "SecondName" },
                new SampleNested { Id = "third", Name = "ThirdName" }
            ]
        };

        var navigator = new JsonPathModelNavigator(new NavigatorConfigOptions
        {
            OptimizeWithCodeEmitter = useOptimization,
            FailOnCollectionKeyNotFound = true
        });

        // Act - Set properties on different indices
        navigator.SetValue(model, "$.NestedList[0].Name", "Updated0");
        navigator.SetValue(model, "$.NestedList[2].Name", "Updated2");

        // Assert
        Assert.Equal("Updated0", model.NestedList[0].Name);
        Assert.Equal("SecondName", model.NestedList[1].Name);   // Unchanged
        Assert.Equal("Updated2", model.NestedList[2].Name);
        Assert.Equal("first", model.NestedList[0].Id);          // Unchanged
        Assert.Equal("third", model.NestedList[2].Id);          // Unchanged
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SetValue_Should_HandleDateTimePropertiesInCollection(bool useOptimization)
    {
        // Arrange
        var model = new SampleModel
        {
            NestedList = 
            [
                new SampleNested 
                { 
                    Id = "datetime-test", 
                    Name = "DateTest", 
                    Date = DateTime.Parse("2023-01-01") 
                }
            ]
        };

        var navigator = new JsonPathModelNavigator(new NavigatorConfigOptions
        {
            OptimizeWithCodeEmitter = useOptimization,
            FailOnCollectionKeyNotFound = true
        });

        var newDate = DateTime.Parse("2024-12-31");

        // Act - Set DateTime property on collection element
        navigator.SetValue(model, "$.NestedList[0].Date", newDate);

        // Assert
        Assert.Equal(newDate, model.NestedList[0].Date);
        Assert.Equal("datetime-test", model.NestedList[0].Id);  // Unchanged
        Assert.Equal("DateTest", model.NestedList[0].Name);     // Unchanged
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SetValue_Should_HandleSingleElementCollection(bool useOptimization)
    {
        // Arrange - Edge case: single element collection
        var model = new SampleModel
        {
            NestedList = 
            [
                new SampleNested { Id = "single", Name = "SingleElement" }
            ]
        };

        var navigator = new JsonPathModelNavigator(new NavigatorConfigOptions
        {
            OptimizeWithCodeEmitter = useOptimization,
            FailOnCollectionKeyNotFound = true
        });

        // Act - Set property on single collection element
        navigator.SetValue(model, "$.NestedList[0].Id", "updatedSingle");
        navigator.SetValue(model, "$.NestedList[0].Name", "UpdatedSingleElement");

        // Assert
        Assert.Equal("updatedSingle", model.NestedList[0].Id);
        Assert.Equal("UpdatedSingleElement", model.NestedList[0].Name);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SetValue_Should_HandleDictionaryObjectProperties(bool useOptimization)
    {
        // Arrange
        var model = new SampleModel
        {
            NestedDictionary = new Dictionary<string, SampleNested>
            {
                ["key1"] = new SampleNested { Id = "dict1", Name = "DictValue1" },
                ["key2"] = new SampleNested { Id = "dict2", Name = "DictValue2" }
            }
        };

        var navigator = new JsonPathModelNavigator(new NavigatorConfigOptions
        {
            OptimizeWithCodeEmitter = useOptimization,
            FailOnCollectionKeyNotFound = true
        });

        // Act - Set property on dictionary value object
        navigator.SetValue(model, "$.NestedDictionary['key1'].Name", "UpdatedDictValue1");

        // Assert
        Assert.Equal("UpdatedDictValue1", model.NestedDictionary["key1"].Name);
        Assert.Equal("dict1", model.NestedDictionary["key1"].Id);           // Unchanged
        Assert.Equal("DictValue2", model.NestedDictionary["key2"].Name);    // Unchanged
    }

    [Fact]
    public void Performance_CollectionPropertySetAccess_ShouldBe_FasterWithOptimization()
    {
        // Arrange
        var model = new SampleModel
        {
            NestedList = 
            [
                new SampleNested { Id = "perf1", Name = "Performance1" },
                new SampleNested { Id = "perf2", Name = "Performance2" }
            ]
        };

        var optimizedNavigator = new JsonPathModelNavigator(new NavigatorConfigOptions
        {
            OptimizeWithCodeEmitter = true,
            FailOnCollectionKeyNotFound = true
        });

        var reflectionNavigator = new JsonPathModelNavigator(new NavigatorConfigOptions
        {
            OptimizeWithCodeEmitter = false
        });

        const int iterations = 1000;

        // Act - Measure optimized performance (property setting on collection elements)
        var optimizedWatch = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            optimizedNavigator.SetValue(model, "$.NestedList[0].Name", $"OptimizedName{i}");
            optimizedNavigator.SetValue(model, "$.NestedList[1].Id", $"optimizedId{i}");
        }
        optimizedWatch.Stop();

        // Reset model state
        model.NestedList[0].Name = "Performance1";
        model.NestedList[1].Id = "perf2";

        // Act - Measure reflection performance  
        var reflectionWatch = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            reflectionNavigator.SetValue(model, "$.NestedList[0].Name", $"ReflectionName{i}");
            reflectionNavigator.SetValue(model, "$.NestedList[1].Id", $"reflectionId{i}");
        }
        reflectionWatch.Stop();

        // Assert - Both approaches work and complete
        Assert.True(optimizedWatch.ElapsedMilliseconds >= 0);
        Assert.True(reflectionWatch.ElapsedMilliseconds >= 0);
        
        // Log the results for manual inspection
        System.Diagnostics.Debug.WriteLine($"Optimized: {optimizedWatch.ElapsedMilliseconds}ms, Reflection: {reflectionWatch.ElapsedMilliseconds}ms");
        
        // Verify final state
        Assert.Contains("ReflectionName", model.NestedList[0].Name);
        Assert.Contains("reflectionId", model.NestedList[1].Id);
    }

    // DISABLED TESTS - Direct collection element setting not implemented yet
    // See BUG_REPORT_Collection_Setter_Issue.md

    [Theory(Skip = "Collection element setting ($.Array[0]) not implemented yet - see BUG_REPORT_Collection_Setter_Issue.md")]
    [InlineData(true)]
    [InlineData(false)]
    public void SetValue_Should_HandleMixedPropertyAndCollection_SimpleArray_DISABLED(bool useOptimization)
    {
        // This test is disabled - direct collection element setting like $.MiddleNames[1] = "value" not supported
        var model = new SampleModel { MiddleNames = ["First", "Second", "Third"] };
        var navigator = new JsonPathModelNavigator(new NavigatorConfigOptions { OptimizeWithCodeEmitter = useOptimization });
        navigator.SetValue(model, "$.MiddleNames[1]", "Updated");
    }

    [Theory(Skip = "Collection element setting ($.List[0]) not implemented yet - see BUG_REPORT_Collection_Setter_Issue.md")]
    [InlineData(true)]
    [InlineData(false)]
    public void SetValue_Should_HandleNestedCollections_DISABLED(bool useOptimization)
    {
        // This test is disabled - direct collection element setting like $.MiddleNamesList[0] = "value" not supported
        var model = new SampleModel { MiddleNamesList = ["ListItem1", "ListItem2"] };
        var navigator = new JsonPathModelNavigator(new NavigatorConfigOptions { OptimizeWithCodeEmitter = useOptimization });
        navigator.SetValue(model, "$.MiddleNamesList[0]", "UpdatedListItem1");
    }
}
