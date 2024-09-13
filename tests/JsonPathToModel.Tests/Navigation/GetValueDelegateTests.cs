using JsonPathToModel.Tests.ModelData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonPathToModel.Tests;
public class GetValueDelegateTests
{
    private JsonPathModelNavigator GetNavigator()
    {
        return new JsonPathModelNavigator(new NavigatorConfigOptions { OptimizeWithCodeEmitter = true });
    }

    [Fact]
    public void GetValue_ShouldRead_WhenNestedValueNull()
    {
        var model = new SampleModel
        {
            Id = "7",
            Name = "Gerry",
            Nested = null
        };

        var navi = GetNavigator();
        Assert.Null(navi.GetValue(model, "$.Nested.Name"));
    }
}
