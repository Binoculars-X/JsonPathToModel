using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FluentResults;

namespace JsonPathToModel;

public class JsonPathModelNavigator : IJsonPathModelNavigator
{
    public Result<IEnumerable<object>> GetItems(object model, string itemsBinding)
    {
        var result = GetValue(model, itemsBinding) as IEnumerable<object>;

        if (result != null)
        {
            return Result.Ok(result);
        }

        return Result.Fail("Not found");
    }

    public Result<int> GetValue()
    {
        var x = new Result<int>();
        x = 10;
        x = Result.Fail("");
        int f = 33;
        return f;
        return x;
    }

    public Result<List<object?>> SelectValues(object model, string modelBinding)
    {
        if (modelBinding == "")
        {
            return Result.Ok(new[] { model }.ToList())!;
        }

        var selectResult = SelectLastPropertiesIterateThroughPath(model, modelBinding);

        if (selectResult.IsFailed)
        {
            return Result.Fail(selectResult.Errors);
        }

        if (selectResult == null)
        {
            return Result.Fail($"Path '{modelBinding}' not found");
        }

        var result = new List<object?>();

        foreach (var itemResult in selectResult.Value)
        {
            result.Add(itemResult.Resolve());
        }

        return result;
    }

    // ToDo: cover incorrect binding scenarios with exceptions
    private static Result<List<SelectPropertyResult>?> SelectLastPropertiesIterateThroughPath(object model, string modelBinding)
    {
        var selectResult = new List<SelectPropertyResult>();
        var path = modelBinding.Replace("$", "").Split('.').Where(c => !string.IsNullOrWhiteSpace(c)).ToList();
        var currentValues = new object[] { model }.ToList();
        var lastPropName = path.Last();
        path.RemoveAt(path.Count - 1);

        foreach (var propName in path)
        {
            var iterationValues = new List<object>();

            foreach (var currentValue in currentValues)
            {
                // all values from list or dictionary
                if (propName.EndsWith("[*]"))
                {
                    var cleanPropName = propName.Substring(0, propName.Length - 3);
                    var prop = currentValue.GetType().GetProperty(cleanPropName);

                    if (prop == null)
                    {
                        return Result.Fail($"Path '{modelBinding}': property '{cleanPropName}' not found");
                    }

                    var list = prop?.GetValue(currentValue);

                    if (list == null)
                    {
                        continue;
                    }

                    var dictObj = list as IDictionary;
                    var listObj = list as IList;

                    if (dictObj != null)
                    {
                        foreach (var item in dictObj.Values)
                        {
                            iterationValues.Add(item);
                        }
                    }
                    else if (listObj != null)
                    {
                        foreach (var item in listObj)
                        {
                            iterationValues.Add(item);
                        }
                    }
                    else
                    {
                        // error
                        return Result.Fail($"Path '{modelBinding}': only IDictionary or IList properties are supported");
                    }
                }
                else if (propName.Contains('[') && propName.Contains(']'))
                {
                    var propSplit = propName.Split('[');
                    var dictPropName = propSplit[0];
                    var dictKey = propSplit[1].Split(']')[0];
                    dictKey = dictKey.Replace("'", "").Replace("\"", "");

                    var prop = currentValue.GetType().GetProperty(dictPropName);

                    if (prop == null)
                    {
                        return Result.Fail($"Path '{modelBinding}': property '{dictPropName}' not found");
                    }

                    var dict = prop?.GetValue(currentValue);

                    if (dict == null)
                    {
                        continue;
                    }

                    var dictObj = dict as IDictionary;
                    var listObj = dict as IList;

                    if (dictObj != null)
                    {
                        if (!dictObj.Contains(dictKey))
                        {
                            // error
                            return Result.Fail($"Path '{modelBinding}': IDictionary key '{dictKey}' not found");
                        }

                        iterationValues.Add(dictObj[dictKey]);
                    }
                    else if (listObj != null)
                    {
                        if (!int.TryParse(dictKey, out var listIdx))
                        {
                            // error
                            return Result.Fail($"Path '{modelBinding}': IList index '{dictKey}' is not int");
                        }

                        if (listIdx >= listObj.Count)
                        {
                            // error
                            return Result.Fail($"Path '{modelBinding}': IList index {dictKey} is out of range");
                        }

                        iterationValues.Add(listObj[listIdx]);
                    }
                    else
                    {
                        // error
                        return Result.Fail($"Path '{modelBinding}': only IDictionary or IList properties are supported");
                    }
                }
                else
                {
                    var prop = currentValue.GetType().GetProperty(propName);

                    if (prop == null)
                    {
                        // ToDo: return error
                        return Result.Fail($"Path '{modelBinding}': property '{propName}' not found");
                    }

                    var item = prop?.GetValue(currentValue);
                    iterationValues.Add(item);
                }
            }

            currentValues = iterationValues;
        }

        if (lastPropName.EndsWith("[*]"))
        {
            lastPropName = lastPropName.Substring(0, lastPropName.Length - 3);
        }

        // assemble all collected items to result
        foreach (var item in currentValues)
        {
            var itemResult = new SelectPropertyResult(item, item.GetType().GetProperty(lastPropName));
            selectResult.Add(itemResult);
        }

        return selectResult;
    }

    public Result<object?> GetValue(object model, string modelBinding)
    {
        if (modelBinding == "")
        {
            return model;
        }

        object targetObject;
        string lastProperty;
        var property = GetLastPropertyIterateThroughPath(model, modelBinding, out targetObject, out lastProperty);

        if (targetObject != null && targetObject.GetType() == typeof(ExpandoObject))
        {
            return (targetObject as ExpandoObject).GetValue(lastProperty);
        }

        if (property.IsFailed)
        {
            return Result.Fail(property.Errors);
        }

        if (property == null)
        {
            return Result.Ok((object?)null);
        }

        var result = property.Value?.GetValue(targetObject);
        return result;
    }

    public void SetValue(object model, string modelBinding, object val)
    {
        object targetObject;
        string lastProperty;
        var prop = GetLastPropertyIterateThroughPath(model, modelBinding, out targetObject, out lastProperty);

        if (targetObject.GetType() == typeof(ExpandoObject))
        {
            (targetObject as ExpandoObject).SetValue(lastProperty, val);
            return;
        }

        if (prop == null || targetObject == null || prop.Value.SetMethod == null)
        {
            return;
        }

        if (prop.Value.PropertyType == typeof(int))
        {
            prop.Value.SetValue(targetObject, Convert.ToInt32(val));
        }
        else if (prop.Value.PropertyType == typeof(decimal))
        {
            prop.Value.SetValue(targetObject, Convert.ToDecimal(val));
        }
        else if (prop.Value.PropertyType == typeof(decimal?))
        {
            prop.Value.SetValue(targetObject, val == null ? (decimal?)null : Convert.ToDecimal(val));
        }
        else if (prop.Value.PropertyType == typeof(int?))
        {
            prop.Value.SetValue(targetObject, val == null ? (int?)null : Convert.ToInt32(val));
        }
        else
        {
            prop.Value.SetValue(targetObject, val);
        }
    }

    private static Result<PropertyInfo?> GetLastPropertyIterateThroughPath(object model, string modelBinding, out object target, out string lastPropName)
    {
        target = null;
        var path = modelBinding.Replace("$", "").Split('.').Where(c => !string.IsNullOrWhiteSpace(c)).ToList();
        var currentObject = model;
        lastPropName = path.Last();
        path.RemoveAt(path.Count - 1);

        foreach (var propName in path)
        {
            if (propName.EndsWith("[*]"))
            {
                var cleanPropName = propName.Substring(0, propName.Length - 3);
                var prop = currentObject.GetType().GetProperty(cleanPropName);
                var list = prop?.GetValue(currentObject);
                currentObject = list;
            }
            else if (propName.Contains('[') && propName.Contains(']'))
            {
                var propSplit = propName.Split('[');
                var dictPropName = propSplit[0];
                var dictKey = propSplit[1].Split(']')[0];
                dictKey = dictKey.Replace("'", "").Replace("\"", "");

                var prop = currentObject.GetType().GetProperty(dictPropName);

                if (prop == null)
                {
                    return Result.Fail($"Path '{modelBinding}': property '{dictPropName}' not found");
                }

                var dict = prop?.GetValue(currentObject);

                if (dict == null)
                {
                    return Result.Ok((PropertyInfo?)null);
                }

                var dictObj = dict as IDictionary;
                var listObj = dict as IList;

                if (dictObj != null)
                {
                    if (!dictObj.Contains(dictKey))
                    {
                        // error
                        return Result.Fail($"Path '{modelBinding}': IDictionary key not found");
                    }

                    currentObject = dictObj[dictKey];
                }
                else if (listObj != null)
                {
                    if (!int.TryParse(dictKey, out var listIdx))
                    {
                        // error
                        return Result.Fail($"Path '{modelBinding}': IList index is not int");
                    }

                    if (listIdx >= listObj.Count)
                    {
                        // error
                        return Result.Fail($"Path '{modelBinding}': IList index is out of range");
                    }

                    currentObject = listObj[listIdx];
                }
                else
                {
                    // error
                    return Result.Fail($"Path '{modelBinding}': only IDictionary or IList properties are supported");
                }
            }
            else
            {
                var prop = currentObject.GetType().GetProperty(propName);

                if (prop == null)
                {
                    // error
                    return Result.Fail($"Path '{modelBinding}' not found");
                }

                currentObject = prop.GetValue(currentObject);
            }

            if (currentObject == null)
            {
                return Result.Ok((PropertyInfo?)null);
            }
        }

        target = currentObject;

        if (lastPropName.EndsWith("[*]"))
        {
            lastPropName = lastPropName.Substring(0, lastPropName.Length - 3);
        }

        var lastProp = currentObject.GetType().GetProperty(lastPropName);

        if (lastProp == null)
        {
            return Result.Fail($"Path '{modelBinding}' not found");
        }

        return lastProp;
    }
}

public record SelectPropertyResult(object Target, PropertyInfo Property)
{
    public object? Resolve()
    {
        if (Property == null || Target == null)
        {
            return Target;
        }

        return Property.GetValue(Target);
    }
}

//public class JsonPathException : Exception
//{
//    public JsonPathException() { }

//    public JsonPathException(string message) : base(message) { }
//}

public static class ExpandoObjectExtensions
{
    public static void SetValue(this ExpandoObject eo, string property, object value)
    {
        (eo as IDictionary<string, object>)[property] = value;
    }

    public static object GetValue(this ExpandoObject eo, string property)
    {
        return (eo as IDictionary<string, object>)[property];
    }

    public static ExpandoObject Clone(this ExpandoObject source)
    {
        var result = new ExpandoObject();
        var dict = result as IDictionary<string, object>;
        var sourceDict = source as IDictionary<string, object>;

        foreach (var key in sourceDict.Keys)
        {
            dict[key] = sourceDict[key];
        }

        return result;
    }

    public static ExpandoObject CopyTo(this ExpandoObject source, ExpandoObject result)
    {
        if (source == null || result == null)
        {
            return null;
        }

        var dict = result as IDictionary<string, object>;
        dict.Clear();
        var sourceDict = source as IDictionary<string, object>;

        foreach (var key in sourceDict.Keys)
        {
            dict[key] = sourceDict[key];
        }

        return result;
    }
    public static void CopyFrom(this ExpandoObject me, Dictionary<string, object> source)
    {
        foreach (var key in source.Keys)
        {
            me.SetValue(key, source[key]);
        }
    }

    public static Dictionary<string, object> ToDict(this ExpandoObject me)
    {
        var dict = me as IDictionary<string, object>;
        var result = new Dictionary<string, object>();

        foreach (var key in dict.Keys)
        {
            result[key] = dict[key];
        }

        return result;
    }

}
