using AutoFixture;
using JsonPathToModel.Tests.Helpers;
using JsonPathToModel.Tests.ModelData;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonPathToModel.Tests;
public class SupportedTypesTests
{
    public static SampleModelAllTypes GenerateSampleData()
    {
        var data = new Fixture()
            .Build<SampleModelAllTypes>()
            .Create();

        return data;
    }

    [Fact]
    public void Should_Support_AllPrimitiveTypes()
    {
        var test = (IJsonPathModelNavigator navi) => 
        { 
            var data = GenerateSampleData();

            // Int
            Assert.Equal(data.Nested.Int, navi.GetValue(data, "$.Nested.Int").Value);
            Assert.Equal(data.Nested.IntNullable, navi.GetValue(data, "$.Nested.IntNullable").Value);
            data.Nested.IntNullable = null;
            Assert.Equal(data.Nested.IntNullable, navi.GetValue(data, "$.Nested.IntNullable").Value);

            // String
            Assert.Equal(data.Nested.String, navi.GetValue(data, "$.Nested.String").Value);
            Assert.Equal(data.Nested.StringNullable, navi.GetValue(data, "$.Nested.StringNullable").Value);
            data.Nested.StringNullable = null;
            Assert.Equal(data.Nested.StringNullable, navi.GetValue(data, "$.Nested.StringNullable").Value);

            // DateTime
            Assert.Equal(data.Nested.DateTime, navi.GetValue(data, "$.Nested.DateTime").Value);
            Assert.Equal(data.Nested.DateTimeNullable, navi.GetValue(data, "$.Nested.DateTimeNullable").Value);
            data.Nested.DateTimeNullable = null;
            Assert.Equal(data.Nested.DateTimeNullable, navi.GetValue(data, "$.Nested.DateTimeNullable").Value);

            // Decimal
            Assert.Equal(data.Nested.Decimal, navi.GetValue(data, "$.Nested.Decimal").Value);
            Assert.Equal(data.Nested.DecimalNullable, navi.GetValue(data, "$.Nested.DecimalNullable").Value);
            data.Nested.DecimalNullable = null;
            Assert.Equal(data.Nested.DecimalNullable, navi.GetValue(data, "$.Nested.DecimalNullable").Value);

            // Double
            Assert.Equal(data.Nested.Double, navi.GetValue(data, "$.Nested.Double").Value);
            Assert.Equal(data.Nested.DoubleNullable, navi.GetValue(data, "$.Nested.DoubleNullable").Value);
            data.Nested.DoubleNullable = null;
            Assert.Equal(data.Nested.DoubleNullable, navi.GetValue(data, "$.Nested.DoubleNullable").Value);
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
}
