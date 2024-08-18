using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JsonPathToModel;

public class JsonPathModelNavigator : IJsonPathModelNavigator
{
    public IEnumerable<object> GetItems(object model, string itemsBinding)
    {
        var result = GetValue(model, itemsBinding);
        return result as IEnumerable<object>;
    }

    public List<object?> SelectValues(object model, string modelBinding)
    {
        if (modelBinding == "")
        {
            return [model];
        }

        var selectResult = SelectLastPropertiesIterateThroughPath(model, modelBinding);

        var result = new List<object?>();

        foreach (var itemResult in selectResult)
        {
            result.Add(itemResult.Resolve());
        }

        return result;
    }

    private static List<SelectPropertyResult> SelectLastPropertiesIterateThroughPath(object model, string modelBinding)
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
                if (propName.EndsWith("[*]"))
                {
                    var cleanPropName = propName.Substring(0, propName.Length - 3);
                    var prop = currentValue.GetType().GetProperty(cleanPropName);
                    var list = prop?.GetValue(currentValue) as IList;

                    if (list == null)
                    {
                        // ToDo: implement scenario when json path = "$.Customer.Email[*].Value" and Email[*] is null
                        //throw new JsonPathException("Wildcard collection property is null");
                        continue;
                    }

                    foreach (var item in list)
                    {
                        iterationValues.Add(item);
                    }
                }
                else if (propName.Contains('[') && propName.Contains(']'))
                {
                    var propSplit = propName.Split('[');
                    var dictPropName = propSplit[0];
                    var dictKey = propSplit[1].Split(']')[0];
                    dictKey = dictKey.Replace("'", "").Replace("\"", "");

                    var prop = currentValue.GetType().GetProperty(dictPropName);
                    var dict = prop?.GetValue(currentValue);

                    var item = dict switch
                    {
                        IDictionary dictObj => dictObj[dictKey],
                        IList listObj => listObj[
                            int.TryParse(dictKey, out var listIdx)
                                ? listIdx
                                : throw new IndexOutOfRangeException($"Unable to use {dictKey} as an index")],
                        _ => throw new NotImplementedException("Only IDictionary or IList properties are supported")
                    };

                    iterationValues.Add(item);
                }
                else
                {
                    var prop = currentValue.GetType().GetProperty(propName);
                    var item = prop?.GetValue(currentValue);
                    iterationValues.Add(item);
                }

                //if (currentValue == null)
                //{
                //    // ToDo: implement scenario when middle property is null but nested value expected
                //    // for example: json path = "$.Customer.PersonName.GivenName" and PersonName is null
                //    //throw new NotImplementedException();
                //    //return [null];
                //    continue;
                //}
            }

            currentValues = iterationValues;
        }



        //target = currentObject;

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

        //if (currentObject is IList)
        //{

        //}

        //var lastProp = currentObject.GetType().GetProperty(lastPropName);
        //return lastProp;

        return selectResult;
    }


    public object GetValue(object model, string modelBinding)
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

        if (property == null)
        {
            return null;
        }

        var result = property.GetValue(targetObject);
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

        if (prop == null || targetObject == null || prop.SetMethod == null)
        {
            return;
        }

        if (prop.PropertyType == typeof(int))
        {
            prop.SetValue(targetObject, Convert.ToInt32(val));
        }
        else if (prop.PropertyType == typeof(decimal))
        {
            prop.SetValue(targetObject, Convert.ToDecimal(val));
        }
        else if (prop.PropertyType == typeof(decimal?))
        {
            prop.SetValue(targetObject, val == null ? (decimal?)null : Convert.ToDecimal(val));
        }
        else if (prop.PropertyType == typeof(int?))
        {
            prop.SetValue(targetObject, val == null ? (int?)null : Convert.ToInt32(val));
        }
        else
        {
            prop.SetValue(targetObject, val);
        }
    }

    private static PropertyInfo GetLastPropertyIterateThroughPath(object model, string modelBinding, out object target, out string lastPropName)
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
                var dict = prop?.GetValue(currentObject);

                currentObject = dict switch
                {
                    IDictionary dictObj => dictObj[dictKey],
                    IList listObj => listObj[
                        int.TryParse(dictKey, out var listIdx)
                            ? listIdx
                            : throw new IndexOutOfRangeException($"Unable to use {dictKey} as an index")],
                    _ => throw new NotImplementedException("Only IDictionary or IList properties are supported")
                };
            }
            else
            {
                var prop = currentObject.GetType().GetProperty(propName);
                currentObject = prop?.GetValue(currentObject);
            }

            if (currentObject == null)
            {
                return null;
            }
        }

        target = currentObject;

        if (lastPropName.EndsWith("[*]"))
        {
            lastPropName = lastPropName.Substring(0, lastPropName.Length - 3);
        }

        var lastProp = currentObject.GetType().GetProperty(lastPropName);
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

public class JsonPathException : Exception
{
    public JsonPathException() { }

    public JsonPathException(string message) : base(message) { }
}

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
