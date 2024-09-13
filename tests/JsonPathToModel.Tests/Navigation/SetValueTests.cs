using AutoFixture;
using JsonPathToModel.Exceptions;
using JsonPathToModel.Tests.Helpers;
using JsonPathToModel.Tests.ModelData;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonPathToModel.Tests;

public class SetValueTests
{
    private JsonPathModelNavigator GetNavigator()
    {
        return new JsonPathModelNavigator(new NavigatorConfigOptions { OptimizeWithCodeEmitter = false });
    }

    [Fact]
    public void SetValue_ShouldChange_SingleValue()
    {
        var model = new SampleModel
        {
            Id = "7",
            Name = "Gerry",
            NestedList = new([new SampleNested { Id = "xyz", Name = "Pedro" }])
        };

        var navi = GetNavigator();

        navi.SetValue(model, "$.Id", "new id");
        navi.SetValue(model, "$.Id", "new id");
        Assert.Equal("new id", model.Id);

        navi.SetValue(model, "$.Name", "new name");
        Assert.Equal("new name", model.Name);

        navi.SetValue(model, "$.NestedList[0].Name", "nested name");
        Assert.Equal("nested name", model.NestedList[0].Name);
    }

    [Fact]
    public void SetValue_ShouldSet_ObjectValue()
    {
        var test = (IJsonPathModelNavigator navi) =>
        {
            var model = new SampleModel
            {
                Id = "7",
                Name = "Gerry",
                Nested = null
            };

            Assert.Null(model.Nested);

            navi.SetValue(model, "$.Nested", new SampleNested { Name = "Gonza" });
            Assert.Equal("Gonza", model.Nested.Name);

            navi.SetValue(model, "$.Nested", null);
            Assert.Null(model.Nested);
        };

        // use reflection
        var svc = ConfigHelper.GetConfigurationServices();
        var navi = svc.GetService<IJsonPathModelNavigator>();
        test(navi);

        // use optimisation
        svc = ConfigHelper.GetConfigurationServices(true);
        navi = svc.GetService<IJsonPathModelNavigator>();
        test(navi);
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

        var navi = GetNavigator();

        var pex = Assert.Throws<NavigationException>(() => navi.SetValue(model, "$.WrongId", "new id"));
        Assert.Contains("Path '$.WrongId'", pex.Message);
        Assert.Contains("not found or SetMethod is null", pex.Message);
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

        var navi = GetNavigator();

        var pex = Assert.Throws<NavigationException>(() => navi.SetValue(model, "$.NestedList[*].Id", "new id"));
        Assert.Equal("Path '$.NestedList[*].Id': cannot get/set single value in a wild card collection", pex.Message);
    }

    [Fact]
    public void SetValue_Should_Support_AllPrimitiveTypes()
    {
        var test = (IJsonPathModelNavigator navi) =>
        {
            var data = new SampleModelAllTypes();

            // Int
            navi.SetValue(data, "$.Nested.Int", 10);
            Assert.Equal(data.Nested.Int, navi.GetValue(data, "$.Nested.Int"));

            navi.SetValue(data, "$.Nested.IntNullable", 100);
            Assert.Equal(data.Nested.IntNullable, navi.GetValue(data, "$.Nested.IntNullable"));

            navi.SetValue(data, "$.Nested.IntNullable", null);
            Assert.Equal(data.Nested.IntNullable, navi.GetValue(data, "$.Nested.IntNullable"));

            // String
            navi.SetValue(data, "$.Nested.String", "10");
            Assert.Equal(data.Nested.String, navi.GetValue(data, "$.Nested.String"));

            navi.SetValue(data, "$.Nested.StringNullable", "100");
            Assert.Equal(data.Nested.StringNullable, navi.GetValue(data, "$.Nested.StringNullable"));

            navi.SetValue(data, "$.Nested.StringNullable", null);
            Assert.Equal(data.Nested.StringNullable, navi.GetValue(data, "$.Nested.StringNullable"));

            // DateTime
            navi.SetValue(data, "$.Nested.DateTime", DateTime.Now);
            Assert.Equal(data.Nested.DateTime, navi.GetValue(data, "$.Nested.DateTime"));

            navi.SetValue(data, "$.Nested.DateTimeNullable", DateTime.Now);
            Assert.Equal(data.Nested.DateTimeNullable, navi.GetValue(data, "$.Nested.DateTimeNullable"));

            navi.SetValue(data, "$.Nested.DateTimeNullable", null);
            Assert.Equal(data.Nested.DateTimeNullable, navi.GetValue(data, "$.Nested.DateTimeNullable"));

            // Decimal
            navi.SetValue(data, "$.Nested.Decimal", 10M);
            Assert.Equal(data.Nested.Decimal, navi.GetValue(data, "$.Nested.Decimal"));

            navi.SetValue(data, "$.Nested.DecimalNullable", 100M);
            Assert.Equal(data.Nested.DecimalNullable, navi.GetValue(data, "$.Nested.DecimalNullable"));

            navi.SetValue(data, "$.Nested.DecimalNullable", null);
            Assert.Equal(data.Nested.DecimalNullable, navi.GetValue(data, "$.Nested.DecimalNullable"));

            // Double
            navi.SetValue(data, "$.Nested.Double", 10.00);
            Assert.Equal(data.Nested.Double, navi.GetValue(data, "$.Nested.Double"));

            navi.SetValue(data, "$.Nested.DoubleNullable", 100.00);
            Assert.Equal(data.Nested.DoubleNullable, navi.GetValue(data, "$.Nested.DoubleNullable"));

            navi.SetValue(data, "$.Nested.DoubleNullable", null);
            Assert.Equal(data.Nested.DoubleNullable, navi.GetValue(data, "$.Nested.DoubleNullable"));

            // Bytes
            navi.SetValue(data, "$.Nested.Bytes", Encoding.ASCII.GetBytes("some bytes"));
            Assert.Equal(data.Nested.Bytes, navi.GetValue(data, "$.Nested.Bytes"));

            navi.SetValue(data, "$.Nested.BytesNullable", Encoding.ASCII.GetBytes("some 100 bytes"));
            Assert.Equal(data.Nested.BytesNullable, navi.GetValue(data, "$.Nested.BytesNullable"));

            navi.SetValue(data, "$.Nested.BytesNullable", null);
            Assert.Equal(data.Nested.BytesNullable, navi.GetValue(data, "$.Nested.BytesNullable"));
        };

        // use reflection
        var svc = ConfigHelper.GetConfigurationServices();
        var navi = svc.GetService<IJsonPathModelNavigator>();
        test(navi);

        // use optimisation
        svc = ConfigHelper.GetConfigurationServices(true);
        navi = svc.GetService<IJsonPathModelNavigator>();
        test(navi);
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
