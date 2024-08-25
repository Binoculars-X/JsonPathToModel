using AutoFixture;
using JsonPathToModel.Tests.ModelData;

namespace JsonPathToModel.Tests.Examples;

public class SampleClientModelTests
{
    public static SampleClientModel GenerateSampleClient(string id = "1")
    {
        var client = new Fixture()
            .Build<SampleClientModel>()
            .With(p => p.Id, id)
            .Create();

        return client;
    }

    [Fact]
    public void GetValue_ShouldReturn_ForLongPath()
    {
        var navi = new JsonPathModelNavigator();
        var model = GenerateSampleClient();
        var expected = model.Person.PrimaryContact.Email.Value;

        var result = navi.GetValueResult(model, "$.Person.PrimaryContact.Email.Value");
        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value);
    }

    [Fact]
    public void GetValue_ShouldReturn_Array()
    {
        var navi = new JsonPathModelNavigator();
        var model = GenerateSampleClient();

        // property notation
        var result = navi.GetValueResult(model, "$.Roles");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value as Role[]);

        // explicit array notation
        result = navi.GetValueResult(model, "$.Roles[]");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value as Role[]);

        // explicit all items notation
        result = navi.GetValueResult(model, "$.Roles[*]");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value as Role[]);
    }

    [Fact]
    public void GetValue_ShouldReturn_ArrayItem()
    {
        var navi = new JsonPathModelNavigator();
        var model = GenerateSampleClient();
        var result = navi.GetValueResult(model, "$.Roles[1]");

        Assert.True(result.IsSuccess);
        Assert.Equal(model.Roles[1], result.Value as Role);
    }

    [Fact]
    public void GetValue_ShouldReturn_ArrayValue()
    {
        var navi = new JsonPathModelNavigator();
        var model = GenerateSampleClient();
        var result = navi.GetValueResult(model, "$.Roles[1].Name");

        Assert.True(result.IsSuccess);
        Assert.Equal(model.Roles[1].Name, result.Value as string);
    }

    [Fact]
    public void SelectValues_ShouldReturn_ArrayValues()
    {
        var navi = new JsonPathModelNavigator();
        var model = GenerateSampleClient();
        var result = navi.SelectValuesResult(model, "$.Roles[*].Name");

        Assert.True(result.IsSuccess);

        Assert.Collection(result.Value,
            e => Assert.Equal(model.Roles[0].Name, e as string),
            e => Assert.Equal(model.Roles[1].Name, e as string),
            e => Assert.Equal(model.Roles[2].Name, e as string));
    }
}
