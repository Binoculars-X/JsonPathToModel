using JsonPathToModel.Tests.ModelData;
using JsonPathToModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JsonPathToModel.Tests;
using JsonPathToModel.Parser;
using Sigil;

namespace BenchmarkConsoleApp;

public class JsonPathToModelUsage
{
    private static readonly SampleClientModel _model;

    static ExpressionEngine _exprEng = new();
    static JsonPathModelNavigator _navi = new();
    //static JsonPathNavigatorFastEmitted _fast = new JsonPathNavigatorFastEmitted();
    //static FastReflectionPrototype _fastReflection = new FastReflectionPrototype();
    static Func<object, object> _emitter;

    static JsonPathToModelUsage()
    {
        _model = SampleClientModelTests.GenerateSampleClient();
        _model.BusinessId = "Java";
        _model.Person.FirstName = "Abba";
        //_netsedEmitter = FastReflectionPrototype.GetNestedEmitter(_model, "$.Nested.Value").CreateDelegate();

        var func = ExpressionEngine.GetJsonPathStraightEmitterGet(_model.GetType(), "$.Person.FirstName");
        _emitter = func.CreateDelegate();
    }

    public static string DotNetCodeGetValue()
    {
        //return _model.BusinessId;
        return _model.Person.FirstName;
    }
    
    public static string JpathV1GetValue()
    {
        //return _navi.GetValue(_model, "$.BusinessId").ToString();
        return _navi.GetValue(_model, "$.Person.FirstName").ToString();
    }

    public static string JpathV2GetValue()
    {
        //return _exprEng.GetValue(_model, "$.BusinessId").ToString();
        return _exprEng.GetValue(_model, "$.Person.FirstName").ToString();
    }

    static Dictionary<Type, Func<object, object>> _dummy = [];

    public static string GetJsonPathStraightEmitterGet()
    {
        //return _exprEng.GetValue(_model, "$.BusinessId").ToString();

        _dummy.TryGetValue(_model.GetType(), out _);

        return _emitter(_model).ToString();
    }

    
}
