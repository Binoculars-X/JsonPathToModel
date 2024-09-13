using FluentResults;
using JsonPathToModel.Exceptions;
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
            NestedList = new([new SampleNested { Id = "xyz", Name = "Pedro" }])
        };

        var navi = new JsonPathModelNavigator();
        Assert.Equal("xyz", navi.SelectValues(model, "$.NestedList[*].Id").Single());
        Assert.Equal("Pedro", navi.SelectValues(model, "$.NestedList[*].Name").Single());
        Assert.Equal("7", navi.SelectValues(model, "$.Id").Single());
        Assert.Equal("Gerry", navi.SelectValues(model, "$.Name").Single());
    }

    [Fact]
    public void SelectValues_Should_ReturnError_WhenPathNotFound()
    {
        var model = new SampleModel
        {
            Id = "7",
            Name = "Gerry",
            NestedList = new([new SampleNested { Id = "xyz", Name = "Pedro" }])
        };

        var navi = new JsonPathModelNavigator();

        var path = "$.WrongProperty.Nested[*].Id";
        var pex = Assert.Throws<NavigationException>(() => navi.SelectValues(model, path));
        Assert.Contains($"Path '{path}'", pex.Message);
        Assert.Contains($"'WrongProperty' not found", pex.Message);
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

        var path = "$.NestedDictionary['WrongKey'].Id";
        var pex = Assert.Throws<NavigationException>(() => navi.SelectValues(model, path));
        Assert.Equal($"Path '{path}': dictionary key 'WrongKey' not found", pex.Message);
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

        var path = "$.NestedDictionary['key1'].Name";
        var result = navi.SelectValues(model, path);
        Assert.Equal("Pedro", result.Single());
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
        Assert.Equal("xyz", result.Single());
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
        Assert.Single(result);

        var item = result.Single() as SampleNested;
        Assert.NotNull(item);
        Assert.Equal("xyz", item.Id);
        Assert.Equal("Pedro", item.Name);
    }

    [Fact]
    public void SelectValues_Should_ReturnCollection_FromWildCardList()
    {
        var model = new SampleModel
        {
            Id = "7",
            Name = "Gerry",
            NestedList = new([new SampleNested { Id = "xyz", Name = "Pedro" }])
        };

        var navi = new JsonPathModelNavigator();

        var path = "$.NestedList[*]";
        var result = navi.SelectValues(model, path);
        Assert.Single(result);

        var item = result.Single() as SampleNested;
        Assert.NotNull(item);
        Assert.Equal("xyz", item.Id);
        Assert.Equal("Pedro", item.Name);
    }
}