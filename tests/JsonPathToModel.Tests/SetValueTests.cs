using JsonPathToModel.Tests.ModelData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonPathToModel.Tests;

public class SetValueTests
{
    [Fact]
    public void SetValue_ShouldChange_SingleValue()
    {
        var model = new SampleModel
        {
            Id = "7",
            Name = "Gerry",
            NestedList = new([new SampleNested { Id = "xyz", Name = "Pedro" }])
        };

        var navi = new JsonPathModelNavigator();

        var result = navi.SetValueResult(model, "$.Id", "new id");
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal("new id", model.Id);

        result = navi.SetValueResult(model, "$.Name", "new name");
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal("new name", model.Name);

        result = navi.SetValueResult(model, "$.NestedList[0].Name", "nested name");
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal("nested name", model.NestedList[0].Name);
    }

    [Fact]
    public void SetValue_ShouldReturnError_ForWrongPath()
    {
        var model = new SampleModel
        {
            Id = "7",
            Name = "Gerry",
            NestedList = new([new SampleNested { Id = "xyz", Name = "Pedro" }])
        };

        var navi = new JsonPathModelNavigator();

        var result = navi.SetValueResult(model, "$.WrongId", "new id");
        Assert.NotNull(result);
        Assert.True(result.IsFailed);
    }

    [Fact]
    public void SetValue_ShouldReturnError_WhenManyPropertiesFound()
    {
        var model = new SampleModel
        {
            Id = "7",
            Name = "Gerry",
            NestedList = new([new SampleNested { Id = "xyz", Name = "Pedro" }, new SampleNested { Id = "xzz", Name = "Lucia" }])
        };

        var navi = new JsonPathModelNavigator();

        var result = navi.SetValueResult(model, "$.NestedList[*].Id", "new id");
        Assert.NotNull(result);
        Assert.True(result.IsFailed);
        Assert.Equal("Path '$.NestedList[*].Id': expected one value but 2 value(s) found", result.Errors.Single().Message);
    }

    [Fact(Skip = "This is may be a useful case to replace a collection")]
    public void SetValue_ShouldReturnError_WhenCollectionIsReplaced()
    {
        var model = new SampleModel
        {
            Id = "7",
            Name = "Gerry",
            NestedList = new([new SampleNested { Id = "xyz", Name = "Pedro" }])
        };

        var navi = new JsonPathModelNavigator();

        var result = navi.SetValueResult(model, "$.Nested[*]", model.NestedList);
        Assert.NotNull(result);
        Assert.True(result.IsFailed);
        Assert.Equal("Path '$.Nested[*]': SetValue replacing a collection is not supported", result.Errors.Single().Message);

        result = navi.SetValueResult(model, "$.Nested[]", model.NestedList);
        Assert.NotNull(result);
        Assert.True(result.IsFailed);
        Assert.Equal("Path '$.Nested[]': SetValue replacing a collection is not supported", result.Errors.Single().Message);
    }

    [Fact]
    public void SetValue_ShouldSetValue_ToExpandoObject()
    {
        //ToDo:
    }

    [Fact]
    public void SetValue_ShouldSetValue_ToAllSupportedDataTypes()
    {
        //ToDo:
    }
}
