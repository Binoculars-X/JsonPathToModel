using JsonPathToModel.Exceptions;
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

        navi.SetValue(model, "$.Id", "new id");
        Assert.Equal("new id", model.Id);

        navi.SetValue(model, "$.Name", "new name");
        Assert.Equal("new name", model.Name);

        navi.SetValue(model, "$.NestedList[0].Name", "nested name");
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

        var pex = Assert.Throws<NavigationException>(() => navi.SetValue(model, "$.WrongId", "new id"));
        Assert.Equal($"Path '$.WrongId': property not found or SetMethod is null", pex.Message);
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

        var pex = Assert.Throws<NavigationException>(() => navi.SetValue(model, "$.NestedList[*].Id", "new id"));
        Assert.Equal("Path '$.NestedList[*].Id': cannot get/set single value in a wild card collection", pex.Message);
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
