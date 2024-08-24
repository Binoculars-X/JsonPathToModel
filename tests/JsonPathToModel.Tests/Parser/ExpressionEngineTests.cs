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
    public void ExpressionEngine_ForNestedPath_Should_ReturnValue()
    {
        var model = SampleClientModelTests.GenerateSampleClient();
        var expected = model.Person.FirstName;

        var result = new ExpressionEngine()
            .ParseJsonPathExpression(model, "$.Person.FirstName")
            .GetValue(model);

        Assert.Equal(expected, result);
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
