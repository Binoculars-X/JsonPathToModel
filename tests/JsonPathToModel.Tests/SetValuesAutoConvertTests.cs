using JsonPathToModel.Tests.Helpers;
using JsonPathToModel.Tests.ModelData;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonPathToModel.Tests;
public class SetValuesAutoConvertTests
{
    //private JsonPathModelNavigator GetNavigator()
    //{
    //    return new JsonPathModelNavigator(new NavigatorConfigOptions { OptimizeWithCodeEmitter = false });
    //}

    [Fact]
    public void SetValue_Should_AutoConvert_FromString()
    {
        // ToDo: add more types converting from string
        var test = (IJsonPathModelNavigator navi) =>
        {
            var data = new SampleModelAllTypes();
            navi.SetValue(data, "$.Nested.DateTime", "2010-05-01");
            Assert.Equal(data.Nested.DateTime, navi.GetValue(data, "$.Nested.DateTime"));

            navi.SetValue(data, "$.Nested.DateTimeNullable", "2015-05-01");
            Assert.Equal(data.Nested.DateTimeNullable, navi.GetValue(data, "$.Nested.DateTimeNullable"));

            navi.SetValue(data, "$.Nested.DateTimeNullable", (string?)null);
            Assert.Equal(data.Nested.DateTimeNullable, navi.GetValue(data, "$.Nested.DateTimeNullable"));
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
