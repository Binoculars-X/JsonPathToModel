using JsonPathToModel.Tests.ModelData;
using System.Collections;
using System.IO;
using System.Xml.Linq;

namespace JsonPathToModel.Tests;

public class GetValueResultTests
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
        Assert.Equal("Pedro", navi.GetValueResult(model, "$.Nested.Name").Value);

        var again = navi.GetValueResult(model, "$.Nested.Name").Value;

        model.Name = null;
        Assert.Null(navi.GetValueResult(model, "$.Name").Value);
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
        Assert.Equal("7", navi.GetValueResult(model, "$.Id").Value);
        Assert.Equal("Gerry", navi.GetValueResult(model, "$.Name").Value);
        Assert.Equal("xyz", navi.GetValueResult(model, "$.NestedList[0].Id").Value);
        Assert.Equal("Pedro", navi.GetValueResult(model, "$.NestedList[0].Name").Value);

        model.Name = null;
        Assert.Null(navi.GetValueResult(model, "$.Name").Value);
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
        Assert.Equal("Pedro", navi.GetValueResult(model, "$.NestedDictionary['key1'].Name").Value);
        Assert.Equal("xyz", navi.GetValueResult(model, "$.NestedDictionary['key1'].Id").Value);

        model.NestedDictionary = null;
        var result = navi.GetValueResult(model, "$.NestedDictionary['key1'].Id");
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
            NestedList = new([new SampleNested { Id = "xyz", Name = "Pedro" }])
        };

        var navi = GetNavigator();

        var path = "$.WrongProperty";
        var result = navi.GetValueResult(model, path);
        Assert.True(result.IsFailed);
        Assert.Contains($"Path '{path}'", result.Errors.Single().Message);
        Assert.Contains($"'WrongProperty' not found", result.Errors.Single().Message);

        path = "$.WrongProperty.WrongSubProperty";
        result = navi.GetValueResult(model, path);
        Assert.True(result.IsFailed);
        Assert.Contains($"Path '{path}'", result.Errors.Single().Message);
        Assert.Contains($"'WrongProperty' not found", result.Errors.Single().Message);

        path = "$.NestedList[0].WrongSubProperty";
        result = navi.GetValueResult(model, path);
        Assert.True(result.IsFailed);
        Assert.Contains($"Path '{path}'", result.Errors.Single().Message);
        Assert.Contains($"'WrongSubProperty' not found", result.Errors.Single().Message);

        path = "$.WrongProperty.NestedList[0].Id";
        result = navi.GetValueResult(model, path);
        Assert.True(result.IsFailed);
        Assert.Contains($"Path '{path}'", result.Errors.Single().Message);
        Assert.Contains($"'WrongProperty' not found", result.Errors.Single().Message);

        path = "$.NestedList[100].Id";
        result = navi.GetValueResult(model, path);
        Assert.True(result.IsFailed);
        Assert.Equal($"Path '{path}': Index was out of range. Must be non-negative and less than the size of the collection. (Parameter 'index')", result.Errors.Single().Message);

        path = "$.NestedList[wrong].Id";
        result = navi.GetValueResult(model, path);
        Assert.True(result.IsFailed);
        Assert.Equal($"Path '{path}': Illigal symbol 'w', ']' is expected", result.Errors.Single().Message);
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

        var path = "$.NestedList[*].Id";
        var result = navi.GetValueResult(model, path);
        Assert.True(result.IsFailed);
        Assert.Equal($"Path '{path}': cannot get/set single value in a wild card collection", result.Errors.Single().Message);
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
        var result = navi.GetValueResult(model, path);
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

        var navi = GetNavigator();

        var path = "$.NestedDictionary['key1'].Id";
        var result = navi.GetValueResult(model, path);
        Assert.False(result.IsFailed);
        Assert.Equal("xyz", result.Value);

        path = "$.NestedDictionary['key1'].Name";
        result = navi.GetValueResult(model, path);
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

        var navi = GetNavigator();

        var path = "$.NestedDictionary[wrong].Id";
        var result = navi.GetValueResult(model, path);
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

        var navi = GetNavigator();

        var path = "$.NestedDictionary['wrong'].Id";
        var result = navi.GetValueResult(model, path);
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }

    [Fact]
    public void CallSiteGetValue_Should_Return()
    {
        var model = new SampleModel
        {
            Id = "7",
            Name = "Gerry",
            NestedList = new([new SampleNested { Id = "xyz", Name = "Pedro" }, new SampleNested { Id = "xzz", Name = "Antuan" }])
        };

        Assert.Equal("7", GetProperty(model, "Id"));
        //Assert.Equal("xyz", GetProperty(model, "Nested[0].Id"));
    }

    public static object GetProperty(object target, string name)
    {
        var site = System.Runtime.CompilerServices.CallSite<Func<System.Runtime.CompilerServices.CallSite, object, object>>
            .Create(Microsoft.CSharp.RuntimeBinder.Binder.GetMember(0, 
                name, 
                target.GetType(), 
                new[] { Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo.Create(0, null) }
                ));
        
        return site.Target(site, target);
    }
}