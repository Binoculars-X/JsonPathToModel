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
    public void GetValueResult_Should_Support_AllPrimitiveTypes()
    {
        var test = (IJsonPathModelNavigator navi) => 
        { 
            var data = GenerateSampleData();

            // Int
            Assert.Equal(data.Nested.Int, navi.GetValueResult(data, "$.Nested.Int").Value);
            Assert.Equal(data.Nested.IntNullable, navi.GetValueResult(data, "$.Nested.IntNullable").Value);
            data.Nested.IntNullable = null;
            Assert.Equal(data.Nested.IntNullable, navi.GetValueResult(data, "$.Nested.IntNullable").Value);

            // String
            Assert.Equal(data.Nested.String, navi.GetValueResult(data, "$.Nested.String").Value);
            Assert.Equal(data.Nested.StringNullable, navi.GetValueResult(data, "$.Nested.StringNullable").Value);
            data.Nested.StringNullable = null;
            Assert.Equal(data.Nested.StringNullable, navi.GetValueResult(data, "$.Nested.StringNullable").Value);

            // DateTime
            Assert.Equal(data.Nested.DateTime, navi.GetValueResult(data, "$.Nested.DateTime").Value);
            Assert.Equal(data.Nested.DateTimeNullable, navi.GetValueResult(data, "$.Nested.DateTimeNullable").Value);
            data.Nested.DateTimeNullable = null;
            Assert.Equal(data.Nested.DateTimeNullable, navi.GetValueResult(data, "$.Nested.DateTimeNullable").Value);

            // Decimal
            Assert.Equal(data.Nested.Decimal, navi.GetValueResult(data, "$.Nested.Decimal").Value);
            Assert.Equal(data.Nested.DecimalNullable, navi.GetValueResult(data, "$.Nested.DecimalNullable").Value);
            data.Nested.DecimalNullable = null;
            Assert.Equal(data.Nested.DecimalNullable, navi.GetValueResult(data, "$.Nested.DecimalNullable").Value);

            // Double
            Assert.Equal(data.Nested.Double, navi.GetValueResult(data, "$.Nested.Double").Value);
            Assert.Equal(data.Nested.DoubleNullable, navi.GetValueResult(data, "$.Nested.DoubleNullable").Value);
            data.Nested.DoubleNullable = null;
            Assert.Equal(data.Nested.DoubleNullable, navi.GetValueResult(data, "$.Nested.DoubleNullable").Value);

            // Bytes - not supported by AutoFixture
            Assert.Equal(data.Nested.Bytes, navi.GetValueResult(data, "$.Nested.Bytes").Value);
            Assert.Equal(data.Nested.BytesNullable, navi.GetValueResult(data, "$.Nested.BytesNullable").Value);
            data.Nested.BytesNullable = null;
            Assert.Equal(data.Nested.BytesNullable, navi.GetValueResult(data, "$.Nested.BytesNullable").Value);
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
    public void Should_Support_DateOnlyTypes()
    {
        var test = (IJsonPathModelNavigator navi) =>
        {
            var data = new HostModel();
            data.Nested.DateOnly =  DateOnly.FromDateTime(DateTime.Now);
            data.Nested.DateOnlyNullable =  DateOnly.FromDateTime(DateTime.Now);

            // DateOnly - not supported by AutoFixture
            Assert.Equal(data.Nested.DateOnly, navi.GetValueResult(data, "$.Nested.DateOnly").Value);
            Assert.Equal(data.Nested.DateOnlyNullable, navi.GetValueResult(data, "$.Nested.DateOnlyNullable").Value);
            data.Nested.DateOnlyNullable = null;
            Assert.Equal(data.Nested.DateOnlyNullable, navi.GetValueResult(data, "$.Nested.DateOnlyNullable").Value);
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

    public class HostModel
    {
        public DateOnlyDataModel Nested { get; set; } = new();
    }

    public class DateOnlyDataModel
    {
        public DateOnly DateOnly { get; set; }
        public DateOnly? DateOnlyNullable { get; set; }
    }
}
