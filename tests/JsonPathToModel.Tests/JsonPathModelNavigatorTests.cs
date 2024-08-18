using JsonPathToModel.Tests.ModelData;

namespace JsonPathToModel.Tests;

public class JsonPathModelNavigatorTests
{
    [Fact]
    public void GetValue_Test()
    {
        var model = new SampleModel
        {
            Id = "7",
            Name = "Gerry",
            Nested = new([new SampleNested { Id = "xyz", Name = "Pedro" }])
        };

        var navi = new JsonPathModelNavigator();
        Assert.Equal("7", navi.GetValue(model, "$.Id"));
        Assert.Equal("Gerry", navi.GetValue(model, "$.Name"));
        Assert.Equal("xyz", navi.GetValue(model, "$.Nested[0].Id"));
        Assert.Equal("Pedro", navi.GetValue(model, "$.Nested[0].Name"));
    }

    [Fact]
    public void SelectValues_Test()
    {
        var model = new SampleModel
        {
            Id = "7",
            Name = "Gerry",
            Nested = new([new SampleNested { Id = "xyz", Name = "Pedro" }])
        };

        var navi = new JsonPathModelNavigator();
        Assert.Equal("7", navi.SelectValues(model, "$.Id").Single());
        Assert.Equal("Gerry", navi.SelectValues(model, "$.Name").Single());
        Assert.Equal("xyz", navi.SelectValues(model, "$.Nested[*].Id").Single());
        Assert.Equal("Pedro", navi.SelectValues(model, "$.Nested[*].Name").Single());
    }

    [Fact]
    public void ReflectionHelper_Test()
    {
        var list = ReflectionHelper.GetTypeNestedPropertyJsonPaths(typeof(SampleModel));
        Assert.NotNull(list);
        Assert.Equal(4, list.Count);
        Assert.Contains("$.Id", list);
        Assert.Contains("$.Name", list);
        Assert.Contains("$.Nested[*].Id", list);
        Assert.Contains("$.Nested[*].Name", list);
    }
}