using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonPathToModel.Tests.Navigation;

public class SelectValuesNestedInNestedTests
{
    [Fact]
    public void SelectValues_Should_ReturnEmptyElement_InNestedInNested()
    {
        var model = NestedInNestedModel.GetSample();
        var navi = new JsonPathModelNavigator(new NavigatorConfigOptions { FailOnCollectionKeyNotFound = false });

        Assert.Empty(navi.SelectValues(model, "$.List[0].Dict['50']"));
    }

    [Fact]
    public void SelectValues_Should_ReturnEmptyElement_InNestedAsteriskInNested()
    {
        var model = NestedInNestedModel.GetSample();
        var navi = new JsonPathModelNavigator(new NavigatorConfigOptions { FailOnCollectionKeyNotFound = false });

        Assert.Empty(navi.SelectValues(model, "$.List[*].Dict['50']"));
    }

    [Fact]
    public void SelectValues_Should_ReturnElement_InNestedInNested()
    {
        var model = NestedInNestedModel.GetSample();
        var navi = new JsonPathModelNavigator(new NavigatorConfigOptions { FailOnCollectionKeyNotFound = false });

        Assert.NotEmpty(navi.SelectValues(model, "$.List[0].Dict['10']"));
        Assert.Equal(model.List[0].Dict["10"].Id, navi.SelectValues(model, "$.List[0].Dict['10'].Id").First());
    }

    [Fact]
    public void SelectValues_Should_ReturnElement_InNestedAsteriskInNested()
    {
        var model = NestedInNestedModel.GetSample();
        var navi = new JsonPathModelNavigator(new NavigatorConfigOptions { FailOnCollectionKeyNotFound = false });

        Assert.NotEmpty(navi.SelectValues(model, "$.List[*].Dict['10']"));
        Assert.Equal(model.List[0].Dict["10"].Id, navi.SelectValues(model, "$.List[0].Dict['10'].Id").First());
    }
}

public class NestedInNestedModel
{
    public List<NestedModel> List { get; set; } = [];

    public static NestedInNestedModel GetSample()
    {
        var x = new NestedInNestedModel();
        var nested = new NestedModel();
        nested.Dict["10"] = new Model { Id = "1", Name = "test1" };
        x.List.Add(nested);
        return x;
    }
}

public class NestedModel
{
    public Dictionary<string, Model> Dict { get; set; } = [];
}

public class Model
{
    public string Id { get; set; }
    public string Name { get; set; }
}
