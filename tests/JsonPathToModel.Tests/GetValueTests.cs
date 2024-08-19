using JsonPathToModel.Tests.ModelData;
using System.Collections;
using System.IO;

namespace JsonPathToModel.Tests;

public class GetValueTests
{
    [Fact]
    public void GetValue_ShouldReadValue()
    {
        var model = new SampleModel
        {
            Id = "7",
            Name = "Gerry",
            Nested = new([new SampleNested { Id = "xyz", Name = "Pedro" }])
        };

        var navi = new JsonPathModelNavigator();
        Assert.Equal("7", navi.GetValue(model, "$.Id").Value);
        Assert.Equal("Gerry", navi.GetValue(model, "$.Name").Value);
        Assert.Equal("xyz", navi.GetValue(model, "$.Nested[0].Id").Value);
        Assert.Equal("Pedro", navi.GetValue(model, "$.Nested[0].Name").Value);

        model.Name = null;
        Assert.Null(navi.GetValue(model, "$.Name").Value);
    }

    [Fact]
    public void GetValue_ShouldReturnValue_FromDictionaryByKey()
    {
        var model = new SampleModel
        {
            Id = "7",
            Name = "Gerry",
            NestedDictionary = new Dictionary<string, SampleNested>() { { "key1", new SampleNested { Id = "xyz", Name = "Pedro" } } }
        };

        var navi = new JsonPathModelNavigator();
        Assert.Equal("Pedro", navi.GetValue(model, "$.NestedDictionary[key1].Name").Value);
        Assert.Equal("xyz", navi.GetValue(model, "$.NestedDictionary[key1].Id").Value);

        model.NestedDictionary = null;
        var result = navi.GetValue(model, "$.NestedDictionary[key1].Id");
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }

    [Fact]
    public void GetValue_Should_ReturnError_WhenPathHasErrors()
    {
        var model = new SampleModel
        {
            Id = "7",
            Name = "Gerry",
            Nested = new([new SampleNested { Id = "xyz", Name = "Pedro" }])
        };

        var navi = new JsonPathModelNavigator();

        var path = "$.WrongProperty";
        var result = navi.GetValue(model, path);
        Assert.True(result.IsFailed);
        Assert.Equal($"Path '{path}': property 'WrongProperty' not found", result.Errors.Single().Message);

        path = "$.WrongProperty.WrongSubProperty";
        result = navi.GetValue(model, path);
        Assert.True(result.IsFailed);
        Assert.Equal($"Path '{path}': property 'WrongProperty' not found", result.Errors.Single().Message);

        path = "$.Nested[0].WrongSubProperty";
        result = navi.GetValue(model, path);
        Assert.True(result.IsFailed);
        Assert.Equal($"Path '{path}': property 'WrongSubProperty' not found", result.Errors.Single().Message);

        path = "$.WrongProperty.Nested[0].Id";
        result = navi.GetValue(model, path);
        Assert.True(result.IsFailed);
        Assert.Equal($"Path '{path}': property 'WrongProperty' not found", result.Errors.Single().Message);

        path = "$.Nested[100].Id";
        result = navi.GetValue(model, path);
        Assert.True(result.IsFailed);
        Assert.Equal($"Path '{path}': IList index 100 is out of range", result.Errors.Single().Message);

        path = "$.Nested[wrong].Id";
        result = navi.GetValue(model, path);
        Assert.True(result.IsFailed);
        Assert.Equal($"Path '{path}': IList index 'wrong' is not int", result.Errors.Single().Message);
    }

    [Fact]
    public void GetValue_Should_ReturnError_WhenManyValuesFound()
    {
        var model = new SampleModel
        {
            Id = "7",
            Name = "Gerry",
            Nested = new([new SampleNested { Id = "xyz", Name = "Pedro" }, new SampleNested { Id = "xzz", Name = "Antuan" }])
        };

        var navi = new JsonPathModelNavigator();

        var path = "$.Nested[*].Id";
        var result = navi.GetValue(model, path);
        Assert.True(result.IsFailed);
        Assert.Equal($"Path '{path}': expected one value but 2 value(s) found", result.Errors.Single().Message);
    }

    [Fact]
    public void GetValue_Should_ReturnCollection_FromWildCardDictionary()
    {
        var model = new SampleModel
        {
            Id = "7",
            Name = "Gerry",
            NestedDictionary = new Dictionary<string, SampleNested>() { { "key1", new SampleNested { Id = "xyz", Name = "Pedro" } } }
        };

        var navi = new JsonPathModelNavigator();

        var path = "$.NestedDictionary[*]";
        var result = navi.GetValue(model, path);
        Assert.False(result.IsFailed);
        Assert.Single(result.Value as IEnumerable);
    }

    [Fact]
    public void GetValue_Should_ReturnCollection_FromDictionaryByKey()
    {
        var model = new SampleModel
        {
            Id = "7",
            Name = "Gerry",
            NestedDictionary = new Dictionary<string, SampleNested>() { { "key1", new SampleNested { Id = "xyz", Name = "Pedro" } } }
        };

        var navi = new JsonPathModelNavigator();

        var path = "$.NestedDictionary[key1].Id";
        var result = navi.GetValue(model, path);
        Assert.False(result.IsFailed);
        Assert.Equal("xyz", result.Value);

        path = "$.NestedDictionary[key1].Name";
        result = navi.GetValue(model, path);
        Assert.False(result.IsFailed);
        Assert.Equal("Pedro", result.Value);
    }

    [Fact]
    public void GetValue_Should_ReturnError_WhenDictionaryKeyNotFound()
    {
        var model = new SampleModel
        {
            Id = "7",
            Name = "Gerry",
            NestedDictionary = new Dictionary<string, SampleNested>() { { "key1", new SampleNested { Id = "xyz", Name = "Pedro" } } }
        };

        var navi = new JsonPathModelNavigator();

        var path = "$.NestedDictionary[wrong].Id";
        var result = navi.GetValue(model, path);
        Assert.True(result.IsFailed);
    }

    [Fact]
    public void GetValue_Should_ReturnNull_WhenDictionaryNull()
    {
        var model = new SampleModel
        {
            Id = "7",
            Name = "Gerry",
            NestedDictionary = null
        };

        var navi = new JsonPathModelNavigator();

        var path = "$.NestedDictionary[wrong].Id";
        var result = navi.GetValue(model, path);
        Assert.False(result.IsFailed);
        Assert.Null(result.Value);
    }
}