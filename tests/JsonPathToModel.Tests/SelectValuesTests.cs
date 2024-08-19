using JsonPathToModel.Tests.ModelData;
using System.IO;

namespace JsonPathToModel.Tests;

public class SelectValuesTests
{
    [Fact]
    public void SelectValues_ShouldReturn_SingleValue()
    {
        var model = new SampleModel
        {
            Id = "7",
            Name = "Gerry",
            Nested = new([new SampleNested { Id = "xyz", Name = "Pedro" }])
        };

        var navi = new JsonPathModelNavigator();
        Assert.Equal("7", navi.SelectValues(model, "$.Id").Value.Single());
        Assert.Equal("Gerry", navi.SelectValues(model, "$.Name").Value.Single());
        Assert.Equal("xyz", navi.SelectValues(model, "$.Nested[*].Id").Value.Single());
        Assert.Equal("Pedro", navi.SelectValues(model, "$.Nested[*].Name").Value.Single());
    }

    [Fact]
    public void SelectValues_Should_ReturnError_WhenPathNotFound()
    {
        var model = new SampleModel
        {
            Id = "7",
            Name = "Gerry",
            Nested = new([new SampleNested { Id = "xyz", Name = "Pedro" }])
        };

        var navi = new JsonPathModelNavigator();

        var path = "$.WrongProperty.Nested[*].Id";
        var result = navi.SelectValues(model, path);
        Assert.True(result.IsFailed);
        Assert.Equal($"Path '{path}': property 'WrongProperty' not found", result.Errors.Single().Message);
    }

    [Fact]
    public void SelectValues_Should_ReturnError_WhenDictionaryWrongKey()
    {
        var model = new SampleModel
        {
            Id = "7",
            Name = "Gerry",
            NestedDictionary = new Dictionary<string, SampleNested>() { { "key1", new SampleNested { Id = "xyz", Name = "Pedro" } } }
        };

        var navi = new JsonPathModelNavigator();

        var path = "$.NestedDictionary[WrongKey].Id";
        var result = navi.SelectValues(model, path);
        Assert.True(result.IsFailed);
        Assert.Equal($"Path '{path}': IDictionary key 'WrongKey' not found", result.Errors.Single().Message);
    }

    [Fact]
    public void SelectValues_Should_ReturnValue_FromDictionaryByKey()
    {
        var model = new SampleModel
        {
            Id = "7",
            Name = "Gerry",
            NestedDictionary = new Dictionary<string, SampleNested>() { { "key1", new SampleNested { Id = "xyz", Name = "Pedro" } } }
        };

        var navi = new JsonPathModelNavigator();

        var path = "$.NestedDictionary[key1].Name";
        var result = navi.SelectValues(model, path);
        Assert.False(result.IsFailed);
        Assert.Equal("Pedro", result.Value.Single());
    }

    [Fact]
    public void SelectValues_Should_ReturnValue_FromWildCardDictionary()
    {
        var model = new SampleModel
        {
            Id = "7",
            Name = "Gerry",
            NestedDictionary = new Dictionary<string, SampleNested>() { { "key1", new SampleNested { Id = "xyz", Name = "Pedro" } } }
        };

        var navi = new JsonPathModelNavigator();

        var path = "$.NestedDictionary[*].Id";
        var result = navi.SelectValues(model, path);
        Assert.False(result.IsFailed);
        Assert.Equal("xyz", result.Value.Single());
    }

    [Fact]
    public void SelectValues_Should_ReturnCollection_FromWildCardDictionary()
    {
        var model = new SampleModel
        {
            Id = "7",
            Name = "Gerry",
            NestedDictionary = new Dictionary<string, SampleNested>() { { "key1", new SampleNested { Id = "xyz", Name = "Pedro" } } }
        };

        var navi = new JsonPathModelNavigator();

        var path = "$.NestedDictionary[*]";
        var result = navi.SelectValues(model, path);
        Assert.False(result.IsFailed);
        Assert.Single(result.Value);
    }

    [Fact]
    public void SelectValues_Should_ReturnCollection_FromWildCardList()
    {
        var model = new SampleModel
        {
            Id = "7",
            Name = "Gerry",
            Nested = new([new SampleNested { Id = "xyz", Name = "Pedro" }])
        };

        var navi = new JsonPathModelNavigator();

        var path = "$.Nested[*]";
        var result = navi.SelectValues(model, path);
        Assert.False(result.IsFailed);
        Assert.Single(result.Value);
    }
}