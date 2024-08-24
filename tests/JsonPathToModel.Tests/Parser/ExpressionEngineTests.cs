using JsonPathToModel.Parser;
using JsonPathToModel.Tests.Examples;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonPathToModel.Tests.Parser;

public class ExpressionEngineTests
{
    [Fact]
    public void ExpressionEngine_ForNestedPath_Should_Run_40M_GetValue_In_One_Second()
    {
        var model = SampleClientModelTests.GenerateSampleClient();
        var ee = new ExpressionEngine(new NavigatorConfigOptions { OptimizeWithCodeEmitter = true });
        var expected = model.Person.FirstName;

        for (int i = 0; i < 40*1000000; i++)
        {
            var result = ee
                .ParseJsonPathExpression(model, "$.Person.FirstName")
                .GetValue(model);
        }
    }

    [Fact]
    public void ExpressionEngine_ForNestedPath_StraightEmitter_Should_ReturnValue()
    {
        var model = SampleClientModelTests.GenerateSampleClient();
        var expected = model.Person.FirstName;
        
        var result = new ExpressionEngine(new NavigatorConfigOptions { OptimizeWithCodeEmitter = true })
            .ParseJsonPathExpression(model, "$.Person.FirstName")
            .GetValue(model);

        Assert.Equal(expected, result);
    }
}
