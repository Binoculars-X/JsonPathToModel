using JsonPathToModel.Tests.ModelData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonPathToModel.Tests;
public class SetValuesAutoConvertTests
{
    private JsonPathModelNavigator GetNavigator()
    {
        return new JsonPathModelNavigator(new NavigatorConfigOptions { OptimizeWithCodeEmitter = false });
    }

    [Fact]
    public void SetValue_Should_AutoConvert_ToDateTime()
    {
        var data = new SampleModelAllTypes();
        var navi = GetNavigator();
        navi.SetValue(data, "$.Nested.DateTime", "2010-05-01");
        Assert.Equal(data.Nested.DateTime, navi.GetValue(data, "$.Nested.DateTime"));
    }
}
