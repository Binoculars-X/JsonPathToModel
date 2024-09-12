using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FluentResults;
using JsonPathToModel.Exceptions;
using JsonPathToModel.Parser;
using JsonPathToModel.Interfaces;

namespace JsonPathToModel;

public class JsonPathModelNavigator : IJsonPathModelNavigator
{
    internal readonly NavigatorConfigOptions _options;
    private readonly ExpressionEngine _expressionEngine;

    public JsonPathModelNavigator() : this(new NavigatorConfigOptions())
    {
    }

    public JsonPathModelNavigator(NavigatorConfigOptions options)
    {
        _options = options;
        _expressionEngine = new ExpressionEngine(options);
    }

    public PropertyInfo? GetPropertyInfo(Type modelType, string path)
    {
        var result = _expressionEngine.ParseJsonPathExpression(modelType, path);
        return result.GetPropertyInfo(modelType);
    }

    public Result<IEnumerable<object>> GetItemsResult(object model, string itemsBinding)
    {
        var result = GetValueResult(model, itemsBinding) as IEnumerable<object>;

        if (result != null)
        {
            return Result.Ok(result);
        }

        return Result.Fail("Not found");
    }
 
    public Result<List<object?>> SelectValuesResult(object model, string path)
    {
        try
        {
            return SelectValues(model, path);
        }
        catch (ParserException pex)
        {
            return Result.Fail($"Path '{path}': {pex.Message}");
        }
        catch (NavigationException nex)
        {
            return Result.Fail($"{nex.Message}");
        }
        catch (Exception e)
        {
            return Result.Fail($"Path '{path}': {e.Message}");
        }
    }

    public List<object?> SelectValues(object model, string path)
    {
        if (path == "" || path == "$")
        {
            return [model];
        }

        var result = _expressionEngine.ParseJsonPathExpression(model, path);
        var values = result.SelectValues(model, _options);
        return values;
    }

    public object? GetValue(object model, string path)
    {
        if (path == "" || path == "$")
        {
            return model;
        }

        var result = _expressionEngine.ParseJsonPathExpression(model, path);
        var value = result.GetValue(model, _options);
        return value;
    }

    public void SetValue(object model, string path, object val)
    {
        var result = _expressionEngine.ParseJsonPathExpression(model, path);
        result.SetValue(model, val, _options);
    }

    public Result<object?> GetValueResult(object model, string path)
    {
        try
        {
            return GetValue(model, path);
        }
        catch (ParserException pex)
        {
            return Result.Fail($"Path '{path}': {pex.Message}");
        }
        catch (NavigationException nex)
        {
            return Result.Fail($"{nex.Message}");
        }
        catch (Exception e)
        {
            return Result.Fail($"Path '{path}': {e.Message}");
        }
    }

    public Result SetValueResult(object model, string path, object val)
    {
        try
        {
            SetValue(model, path, val);
            return Result.Ok();
        }
        catch (ParserException pex)
        {
            return Result.Fail($"Path '{path}': {pex.Message}");
        }
        catch (NavigationException nex)
        {
            return Result.Fail($"{nex.Message}");
        }
        catch (Exception e)
        {
            return Result.Fail($"Path '{path}': {e.Message}");
        }
    }
}
