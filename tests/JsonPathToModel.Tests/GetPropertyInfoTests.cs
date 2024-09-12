using JsonPathToModel.Tests.ModelData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonPathToModel.Tests;

public class GetPropertyInfoTests
{
    private JsonPathModelNavigator GetNavigator()
    {
        return new JsonPathModelNavigator(new NavigatorConfigOptions { OptimizeWithCodeEmitter = true });
    }

    [Fact]
    public void GetPropertyInfo_Returns_PropertyInfo()
    {
        var model = new SampleModel
        {
            Id = "7",
            Name = "Gerry",
            Nested = new SampleNested { Id = "xyz", Name = "Pedro" }
        };

        var navi = GetNavigator();
        Assert.Equal(typeof(string), navi.GetPropertyInfo(typeof(SampleModel), "$.Id")?.PropertyType);
        Assert.Equal(typeof(string), navi.GetPropertyInfo(typeof(SampleModel), "$.Name")?.PropertyType);
        Assert.Equal(typeof(SampleNested), navi.GetPropertyInfo(typeof(SampleModel), "$.Nested")?.PropertyType);
        Assert.Equal(typeof(string), navi.GetPropertyInfo(typeof(SampleModel), "$.Nested.Id")?.PropertyType);
        Assert.Equal(typeof(DateTime), navi.GetPropertyInfo(typeof(SampleModel), "$.Nested.Date")?.PropertyType);
    }
}
