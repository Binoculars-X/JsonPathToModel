using JsonPathToModel.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonPathToModel.Tests.Parser;

public class ExpressionEngineTests
{
    [Fact]
    public void Test1()
    {
        var navi = new ExpressionEngine();
        var model = SampleClientModelTests.GenerateSampleClient();
        var expected = model.Person.FirstName;

        var result = navi.GetValue(model, "$.Person.FirstName");
        navi.GetValue(model, "$.Person.FirstName");
        Assert.Equal(expected, result);
    }

    [Fact]
    public void NestedPath_StraightEmitter_Test1()
    {
        var model = SampleClientModelTests.GenerateSampleClient();
        var expected = model.Person.FirstName;
        var func = ExpressionEngine.GetJsonPathStraightEmitterGet(model.GetType(), "$.Person.FirstName");

        var result = func.CreateDelegate()(model);
        Assert.Equal(expected, result);
    }
}
