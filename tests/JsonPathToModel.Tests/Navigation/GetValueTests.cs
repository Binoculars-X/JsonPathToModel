using FluentResults;
using JsonPathToModel.Exceptions;
using JsonPathToModel.Tests.ModelData;
using System.Collections;
using System.IO;
using System.Xml.Linq;

namespace JsonPathToModel.Tests;

public class GetValueTests
{
    private JsonPathModelNavigator GetNavigator()
    {
        return new JsonPathModelNavigator(new NavigatorConfigOptions { OptimizeWithCodeEmitter = true });
    }

    [Fact]
    public void GetValue_ShouldRead_NestedValue()
    {
        var model = new SampleModel
        {
            Id = "7",
            Name = "Gerry",
            Nested = new SampleNested { Id = "xyz", Name = "Pedro" }
        };

        var navi = GetNavigator();
        Assert.Equal("Pedro", navi.GetValue(model, "$.Nested.Name"));

        model.Name = null;
        Assert.Null(navi.GetValue(model, "$.Name"));
    }

    [Fact]
    public void GetValue_ShouldReadValue()
    {
        var model = new SampleModel
        {
            Id = "7",
            Name = "Gerry",
            NestedList = new([new SampleNested { Id = "xyz", Name = "Pedro" }])
        };

        var navi = GetNavigator();
        Assert.Equal("7", navi.GetValue(model, "$.Id"));
        Assert.Equal("Gerry", navi.GetValue(model, "$.Name"));
        Assert.Equal("xyz", navi.GetValue(model, "$.NestedList[0].Id"));
        Assert.Equal("Pedro", navi.GetValue(model, "$.NestedList[0].Name"));

        model.Name = null;
        Assert.Null(navi.GetValue(model, "$.Name"));
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

        var navi = GetNavigator();
        Assert.Equal("Pedro", navi.GetValue(model, "$.NestedDictionary['key1'].Name"));
        Assert.Equal("xyz", navi.GetValue(model, "$.NestedDictionary['key1'].Id"));

        model.NestedDictionary = null;
        var result = navi.GetValue(model, "$.NestedDictionary['key1'].Id");
        Assert.Null(result);
    }

    [Fact]
    public void GetValue_ShouldNotFail_When_FailOnCollectionKeyNotFound_Disabled()
    {
        var model = new SampleModel
        {
            Id = "7",
            Name = "Gerry",
            NestedDictionary = new Dictionary<string, SampleNested>() { { "key1", new SampleNested { Id = "xyz", Name = "Pedro" } } }
        };

        var navi = new JsonPathModelNavigator(new NavigatorConfigOptions
        {
            OptimizeWithCodeEmitter = true,
            FailOnCollectionKeyNotFound = false
        });

        Assert.Null(navi.GetValue(model, "$.NestedDictionary['wrong'].Name"));
    }

    [Fact]
    public void GetValue_Should_ReturnError_WhenPathHasErrors()
    {
        var model = new SampleModel
        {
            Id = "7",
            Name = "Gerry",
            NestedList = new([new SampleNested { Id = "xyz", Name = "Pedro" }])
        };

        var navi = GetNavigator();

        Assert.Throws<NavigationException>(() => navi.GetValue(model, "$.WrongProperty"));
        Assert.Throws<NavigationException>(() => navi.GetValue(model, "$.WrongProperty.WrongSubProperty"));
        Assert.Throws<NavigationException>(() => navi.GetValue(model, "$.NestedList[0].WrongSubProperty"));
        Assert.Throws<NavigationException>(() => navi.GetValue(model, "$.WrongProperty.NestedList[0].Id"));
        Assert.Throws<ArgumentOutOfRangeException>(() => navi.GetValue(model, "$.NestedList[100].Id"));
        Assert.Throws<ParserException>(() => navi.GetValue(model, "$.NestedList[wrong].Id"));
    }

    [Fact]
    public void GetValue_Should_ReturnError_WhenManyValuesFound()
    {
        var model = new SampleModel
        {
            Id = "7",
            Name = "Gerry",
            NestedList = new([new SampleNested { Id = "xyz", Name = "Pedro" }, new SampleNested { Id = "xzz", Name = "Antuan" }])
        };

        var navi = GetNavigator();

        var pex = Assert.Throws<NavigationException>(() => navi.GetValue(model, "$.NestedList[*].Id"));
        Assert.Equal($"Path '$.NestedList[*].Id': cannot get/set single value in a wild card collection", pex.Message);
    }

    [Fact]
    public void GetValue_Should_ReturnError_When_WrongIndexSupplied()
    {
        var model = new SampleModel
        {
            Id = "7",
            Name = "Gerry",
            NestedList = new([new SampleNested { Id = "xyz", Name = "Pedro" }, new SampleNested { Id = "xzz", Name = "Antuan" }])
        };

        var navi = GetNavigator();

        var pex = Assert.Throws<ArgumentOutOfRangeException>(() => navi.GetValue(model, "$.NestedList[2].Id"));
        //Assert.Equal($"Path '$.NestedList[2].Id': cannot get/set single value in a wild card collection", pex.Message);
    }

    [Fact]
    public void GetValue_Should_NotReturnError_When_WrongIndexSupplied_FailOnCollectionKeyNotFound_Disabled()
    {
        var model = new SampleModel
        {
            Id = "7",
            Name = "Gerry",
            NestedList = new([new SampleNested { Id = "xyz", Name = "Pedro" }, new SampleNested { Id = "xzz", Name = "Antuan" }])
        };

        var navi = new JsonPathModelNavigator(new NavigatorConfigOptions
        {
            OptimizeWithCodeEmitter = true,
            FailOnCollectionKeyNotFound = false
        });

        var value = navi.GetValue(model, "$.NestedList[2].Id");
        Assert.Null(value);
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

        var navi = GetNavigator();

        var path = "$.NestedDictionary[*]";
        var result = navi.GetValue(model, path);
        Assert.Single(result as IEnumerable);
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

        var navi = GetNavigator();

        var path = "$.NestedDictionary['key1'].Id";
        var result = navi.GetValue(model, path);
        Assert.Equal("xyz", result);

        path = "$.NestedDictionary['key1'].Name";
        result = navi.GetValue(model, path);
        Assert.Equal("Pedro", result);
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

        var navi = new JsonPathModelNavigator(new NavigatorConfigOptions { 
            OptimizeWithCodeEmitter = true, 
            FailOnCollectionKeyNotFound = true 
        });

        var pex = Assert.Throws<NavigationException>(() => navi.GetValue(model, "$.NestedDictionary['wrong'].Id"));
        Assert.Equal("Path '$.NestedDictionary['wrong'].Id': dictionary key 'wrong' not found", pex.Message);
    }

    [Fact]
    public void GetValue_Should_ReturnNull_WhenDictionaryIndexSupplied()
    {
        var model = new SampleModel
        {
            Id = "7",
            Name = "Gerry",
            NestedDictionary = new Dictionary<string, SampleNested>() { { "key1", new SampleNested { Id = "xyz", Name = "Pedro" } } }
        };

        var navi = GetNavigator();

        var value = navi.GetValue(model, "$.NestedDictionary[0].Id");
        Assert.Null(value);
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

        var navi = GetNavigator();

        var path = "$.NestedDictionary['wrong'].Id";
        var result = navi.GetValue(model, path);
        Assert.Null(result);
    }
}