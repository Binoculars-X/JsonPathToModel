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

        var result = new List<object?>();

        foreach (var itemResult in selectResult.Value)
        {
            result.Add(itemResult.Resolve());
        }

        return result;
    }

    public Result<object?> GetValue(object model, string modelBinding)
    {
        if (modelBinding == "")
        {
            return model;
        }

        var selectResult = SelectLastPropertiesIterateThroughPath(model, modelBinding);

        if (selectResult.IsFailed)
        {
            return Result.Fail(selectResult.Errors);
        }

        if (selectResult.Value == null || !selectResult.Value.Any())
        {
            return Result.Ok((object?)null);
        }

        if (selectResult.Value.Count == 1)
        {
            return selectResult.Value.Single().Resolve();
        }

        return Result.Fail($"Path '{modelBinding}': expected one value but {selectResult.Value.Count} value(s) found");
    }

    public Result SetValue(object model, string modelBinding, object val)
    {
        //object targetObject;
        //string lastProperty;
        //var prop = GetLastPropertyIterateThroughPath(model, modelBinding, out targetObject, out lastProperty);

        var selectResult = SelectLastPropertiesIterateThroughPath(model, modelBinding);

        if (selectResult.IsFailed)
        {
            return Result.Fail(selectResult.Errors);
        }

        if (selectResult.Value == null || !selectResult.Value.Any())
        {
            return Result.Ok();
        }

        if (selectResult.Value.Count > 1)
        {
            return Result.Fail($"Path '{modelBinding}': expected one value but {selectResult.Value.Count} value(s) found");
        }

        var result = selectResult.Value.Single();
        var targetObject = result.Target;
        var property = result.Property;

        if (property == null || targetObject == null || property.SetMethod == null)
        {
            return Result.Fail($"Path '{modelBinding}': property not found or SetMethod is null");
        }

        if (targetObject.GetType() == typeof(ExpandoObject))
        {
            // ToDo: this should be tested
            (targetObject as ExpandoObject)!.SetValue(property.Name, val);
        }
        else if (property.PropertyType == typeof(int))
        {
            property.SetValue(targetObject, Convert.ToInt32(val));
        }
        else if (property.PropertyType == typeof(decimal))
        {
            property.SetValue(targetObject, Convert.ToDecimal(val));
        }
        else if (property.PropertyType == typeof(decimal?))
        {
            property.SetValue(targetObject, val == null ? (decimal?)null : Convert.ToDecimal(val));
        }
        else if (property.PropertyType == typeof(int?))
        {
            property.SetValue(targetObject, val == null ? (int?)null : Convert.ToInt32(val));
        }
        else
        {
            property.SetValue(targetObject, val);
        }

        return Result.Ok();
    }

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
                        // extracted from model collection is null, continue iteration
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
                        // extracted from model collection is null, continue iteration
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
            var property = item.GetType().GetProperty(lastPropName);

            if (property == null)
            {
                return Result.Fail($"Path '{modelBinding}': property '{lastPropName}' not found");
            }

            var itemResult = new SelectPropertyResult(item, property);
            selectResult.Add(itemResult);
        }

        return selectResult;
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
