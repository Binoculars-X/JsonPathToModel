using JsonPathToModel;
using JsonPathToModel.Tests.ModelData;

namespace JsonPathToModel.Tests.Parser;

/// <summary>
/// Tests to verify collection setter optimization works correctly with different data types
/// Tests string, int, decimal, DateTime, bool, nullable types, and enums
/// </summary>
public class CollectionSetterDataTypeTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SetValue_Should_HandleIntegerProperties(bool useOptimization)
    {
        // Arrange
        var model = new SampleModelAllTypes
        {
            NestedList = 
            [
                new SampleModelAllTypesNested { Int = 100, IntNullable = 200, String = "Test" }
            ]
        };

        var navigator = new JsonPathModelNavigator(new NavigatorConfigOptions
        {
            OptimizeWithCodeEmitter = useOptimization,
            FailOnCollectionKeyNotFound = true
        });

        // Act - Set int and nullable int properties
        navigator.SetValue(model, "$.NestedList[0].Int", 999);
        navigator.SetValue(model, "$.NestedList[0].IntNullable", 888);

        // Assert
        Assert.Equal(999, model.NestedList[0].Int);
        Assert.Equal(888, model.NestedList[0].IntNullable);
        Assert.Equal("Test", model.NestedList[0].String); // Unchanged
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SetValue_Should_HandleDecimalProperties(bool useOptimization)
    {
        // Arrange
        var model = new SampleModelAllTypes
        {
            NestedList = 
            [
                new SampleModelAllTypesNested 
                { 
                    Decimal = 123.45m, 
                    DecimalNullable = 678.90m,
                    String = "DecimalTest"
                }
            ]
        };

        var navigator = new JsonPathModelNavigator(new NavigatorConfigOptions
        {
            OptimizeWithCodeEmitter = useOptimization,
            FailOnCollectionKeyNotFound = true
        });

        // Act - Set decimal and nullable decimal properties
        navigator.SetValue(model, "$.NestedList[0].Decimal", 999.99m);
        navigator.SetValue(model, "$.NestedList[0].DecimalNullable", 111.11m);

        // Assert
        Assert.Equal(999.99m, model.NestedList[0].Decimal);
        Assert.Equal(111.11m, model.NestedList[0].DecimalNullable);
        Assert.Equal("DecimalTest", model.NestedList[0].String); // Unchanged
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SetValue_Should_HandleBooleanProperties(bool useOptimization)
    {
        // Arrange
        var model = new SampleModelAllTypes
        {
            Bool = false,
            BoolNullable = null,
            NestedList = 
            [
                new SampleModelAllTypesNested { String = "BoolTest" }
            ]
        };

        var navigator = new JsonPathModelNavigator(new NavigatorConfigOptions
        {
            OptimizeWithCodeEmitter = useOptimization,
            FailOnCollectionKeyNotFound = true
        });

        // Act - Set bool properties on root object (not in collection for this test)
        navigator.SetValue(model, "$.Bool", true);
        navigator.SetValue(model, "$.BoolNullable", false);

        // Assert
        Assert.True(model.Bool);
        Assert.False(model.BoolNullable);
        Assert.Equal("BoolTest", model.NestedList[0].String); // Collection unchanged
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SetValue_Should_HandleDoubleProperties(bool useOptimization)
    {
        // Arrange
        var model = new SampleModelAllTypes
        {
            NestedList = 
            [
                new SampleModelAllTypesNested 
                { 
                    Double = 123.456,
                    DoubleNullable = 789.012,
                    String = "DoubleTest"
                }
            ]
        };

        var navigator = new JsonPathModelNavigator(new NavigatorConfigOptions
        {
            OptimizeWithCodeEmitter = useOptimization,
            FailOnCollectionKeyNotFound = true
        });

        // Act - Set double and nullable double properties
        navigator.SetValue(model, "$.NestedList[0].Double", 999.999);
        navigator.SetValue(model, "$.NestedList[0].DoubleNullable", 111.111);

        // Assert
        Assert.Equal(999.999, model.NestedList[0].Double);
        Assert.Equal(111.111, model.NestedList[0].DoubleNullable);
        Assert.Equal("DoubleTest", model.NestedList[0].String); // Unchanged
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SetValue_Should_HandleNullableDateTimeProperties(bool useOptimization)
    {
        // Arrange
        var model = new SampleModelAllTypes
        {
            NestedList = 
            [
                new SampleModelAllTypesNested 
                { 
                    DateTime = DateTime.Parse("2023-01-01"),
                    DateTimeNullable = DateTime.Parse("2023-02-01"),
                    String = "DateTimeTest"
                }
            ]
        };

        var navigator = new JsonPathModelNavigator(new NavigatorConfigOptions
        {
            OptimizeWithCodeEmitter = useOptimization,
            FailOnCollectionKeyNotFound = true
        });

        var newDateTime = DateTime.Parse("2024-12-31T23:59:59");
        var newNullableDateTime = DateTime.Parse("2024-06-15T12:30:45");

        // Act - Set DateTime and nullable DateTime properties
        navigator.SetValue(model, "$.NestedList[0].DateTime", newDateTime);
        navigator.SetValue(model, "$.NestedList[0].DateTimeNullable", newNullableDateTime);

        // Assert
        Assert.Equal(newDateTime, model.NestedList[0].DateTime);
        Assert.Equal(newNullableDateTime, model.NestedList[0].DateTimeNullable);
        Assert.Equal("DateTimeTest", model.NestedList[0].String); // Unchanged
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SetValue_Should_HandleNullableTypesWithNull(bool useOptimization)
    {
        // Arrange
        var model = new SampleModelAllTypes
        {
            NestedList = 
            [
                new SampleModelAllTypesNested 
                { 
                    IntNullable = 123,
                    DecimalNullable = 456.78m,
                    DateTimeNullable = DateTime.Now,
                    DoubleNullable = 999.999,
                    String = "NullTest"
                }
            ]
        };

        var navigator = new JsonPathModelNavigator(new NavigatorConfigOptions
        {
            OptimizeWithCodeEmitter = useOptimization,
            FailOnCollectionKeyNotFound = true
        });

        // Act - Set nullable properties to null
        navigator.SetValue(model, "$.NestedList[0].IntNullable", null);
        navigator.SetValue(model, "$.NestedList[0].DecimalNullable", null);
        navigator.SetValue(model, "$.NestedList[0].DateTimeNullable", null);
        navigator.SetValue(model, "$.NestedList[0].DoubleNullable", null);

        // Assert - All should be null now
        Assert.Null(model.NestedList[0].IntNullable);
        Assert.Null(model.NestedList[0].DecimalNullable);
        Assert.Null(model.NestedList[0].DateTimeNullable);
        Assert.Null(model.NestedList[0].DoubleNullable);
        Assert.Equal("NullTest", model.NestedList[0].String); // Unchanged
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SetValue_Should_HandleEnumProperties(bool useOptimization)
    {
        // Arrange
        var model = new SampleModel
        {
            Gender = Gender.Male, // Set initial enum value
            NestedList = 
            [
                new SampleNested { Id = "enum-test", Name = "EnumTest" }
            ]
        };

        var navigator = new JsonPathModelNavigator(new NavigatorConfigOptions
        {
            OptimizeWithCodeEmitter = useOptimization,
            FailOnCollectionKeyNotFound = true
        });

        // Act - Set enum property
        navigator.SetValue(model, "$.Gender", Gender.Female);

        // Assert
        Assert.Equal(Gender.Female, model.Gender);
        Assert.Equal("enum-test", model.NestedList[0].Id); // Collection unchanged
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SetValue_Should_HandleStringNullableProperties(bool useOptimization)
    {
        // Arrange
        var model = new SampleModelAllTypes
        {
            NestedList = 
            [
                new SampleModelAllTypesNested 
                { 
                    String = "NotNull",
                    StringNullable = "AlsoNotNull"
                }
            ]
        };

        var navigator = new JsonPathModelNavigator(new NavigatorConfigOptions
        {
            OptimizeWithCodeEmitter = useOptimization,
            FailOnCollectionKeyNotFound = true
        });

        // Act - Set nullable string to null and regular string to new value
        navigator.SetValue(model, "$.NestedList[0].StringNullable", null);
        navigator.SetValue(model, "$.NestedList[0].String", "UpdatedNotNull");

        // Assert
        Assert.Null(model.NestedList[0].StringNullable);
        Assert.Equal("UpdatedNotNull", model.NestedList[0].String);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SetValue_Should_HandleByteArrayProperties(bool useOptimization)
    {
        // Arrange
        var originalBytes = new byte[] { 1, 2, 3, 4, 5 };
        var model = new SampleModelAllTypes
        {
            NestedList = 
            [
                new SampleModelAllTypesNested 
                { 
                    Bytes = originalBytes,
                    BytesNullable = new byte[] { 10, 20, 30 },
                    String = "ByteTest"
                }
            ]
        };

        var navigator = new JsonPathModelNavigator(new NavigatorConfigOptions
        {
            OptimizeWithCodeEmitter = useOptimization,
            FailOnCollectionKeyNotFound = true
        });

        var newBytes = new byte[] { 99, 88, 77, 66 };

        // Act - Set byte array properties
        navigator.SetValue(model, "$.NestedList[0].Bytes", newBytes);
        navigator.SetValue(model, "$.NestedList[0].BytesNullable", null);

        // Assert
        Assert.Equal(newBytes, model.NestedList[0].Bytes);
        Assert.Null(model.NestedList[0].BytesNullable);
        Assert.Equal("ByteTest", model.NestedList[0].String); // Unchanged
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SetValue_Should_HandleDictionaryWithDifferentDataTypes(bool useOptimization)
    {
        // Arrange
        var model = new SampleModelAllTypes
        {
            NestedDictionary = new Dictionary<string, SampleModelAllTypesNested>
            {
                ["dataTypes"] = new SampleModelAllTypesNested 
                { 
                    Int = 100,
                    Decimal = 200.50m,
                    DateTime = DateTime.Parse("2023-01-01"),
                    String = "DictDataTypes"
                }
            }
        };

        var navigator = new JsonPathModelNavigator(new NavigatorConfigOptions
        {
            OptimizeWithCodeEmitter = useOptimization,
            FailOnCollectionKeyNotFound = true
        });

        // Act - Set different data types via dictionary access
        navigator.SetValue(model, "$.NestedDictionary['dataTypes'].Int", 999);
        navigator.SetValue(model, "$.NestedDictionary['dataTypes'].Decimal", 123.45m);
        
        var newDate = DateTime.Parse("2024-12-31");
        navigator.SetValue(model, "$.NestedDictionary['dataTypes'].DateTime", newDate);

        // Assert
        Assert.Equal(999, model.NestedDictionary["dataTypes"].Int);
        Assert.Equal(123.45m, model.NestedDictionary["dataTypes"].Decimal);
        Assert.Equal(newDate, model.NestedDictionary["dataTypes"].DateTime);
        Assert.Equal("DictDataTypes", model.NestedDictionary["dataTypes"].String); // Unchanged
    }
}
