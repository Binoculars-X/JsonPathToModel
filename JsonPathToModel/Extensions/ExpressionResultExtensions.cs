using FluentResults;
using JsonPathToModel.Exceptions;
using JsonPathToModel.Interfaces;
using JsonPathToModel.Parser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace JsonPathToModel;

internal static class ExpressionResultExtensions
{
    public static List<object?> SelectValues(this ExpressionResult result, object target, NavigatorConfigOptions? options = null)
    {
        if (options == null)
        {
            options = new NavigatorConfigOptions();
        }

        var resultList = new List<object?>();
        var currentValues = new object?[] { target }.ToList();
        var lastToken = result.Tokens.Last();

        // Iterate through tokens resolving value
        for (int i = 1; i < result.Tokens.Count-1; i++)
        {
            var iterationValues = new List<object?>();

            foreach (var currentValue in currentValues)
            {

                var currentResult = ResolveValues(result.Expression, currentValue, result.Tokens[i], options);

                if (currentResult != null && currentResult.Any())
                {
                    iterationValues.AddRange(currentResult);
                }
            }

            currentValues = iterationValues;
        }

        if (!currentValues.Any())
        {
            return resultList;
        }

        // collect final result
        if (lastToken.CollectionDetails != null)
        {
            // assemble all collected items
            foreach (var item in currentValues)
            {
                var collection = GetCollectionProperty(result.Expression, item, lastToken);

                if (collection == null)
                {
                    continue;
                }

                var items = ExtractCollectionItems(result.Expression, collection);
                resultList.AddRange(items);
            }
        }
        else
        {
            var property = currentValues.First()?.GetType().GetProperty(lastToken.Field);

            if (property == null)
            {
                throw new NavigationException($"Path '{result.Expression}': property '{lastToken.Field}' not found");
            }

            // assemble all collected items
            foreach (var item in currentValues)
            {
                var value = property.GetValue(item);
                resultList.Add(value);
            }
        }

        return resultList;
    }

    public static void SetValue(this ExpressionResult result, object target, object val, NavigatorConfigOptions? options = null)
    {
        if (options == null)
        {
            options = new NavigatorConfigOptions();
        }

        // If delegate provided
        if (result.SetDelegateDetails != null)
        {
            try
            {
                var typedValue = TryConvertToTargetType(result.SetDelegateDetails.TargetProperty, val);
                result.SetDelegateDetails.SetDelegate(target, typedValue);
                return;
            }
            catch (NullReferenceException)
            {
                // always throw exception, all nested properties must not be null
                throw;
            }
        }

        CheckForNotSinglePath(result);

        // Iterate through tokens resolving value
        var currentObject = target;

        for (int i = 1; i < result.Tokens.Count-1; i++)
        {
            currentObject = Resolve(result.Expression, currentObject, result.Tokens[i], options);

            if (currentObject == null)
            {
                throw new NavigationException($"Path '{result.Expression}': SetValue cannot set as {result.Tokens[i].Field} is null"); ;
            }
        }

        var property = currentObject.GetType().GetProperty(result.Tokens.Last().Field);

        //if ((property as ICollection) != null)
        //{
        //    throw new NavigationException($"Path '{result.Expression}': SetValue replacing a collection is not supported");
        //}

        if (property == null || currentObject == null || property.SetMethod == null)
        {
            throw new NavigationException($"Path '{result.Expression}': property not found or SetMethod is null");
        }

        if (currentObject.GetType() == typeof(ExpandoObject))
        {
            // ToDo: this should be tested
            (currentObject as ExpandoObject)!.SetValue(property.Name, val);
        }
        else
        {
            property.SetValue(currentObject,TryConvertToTargetType(property, val));
        }
    }

    static object? TryConvertToTargetType(PropertyInfo property, object? val)
    {
        if (val == null || property.PropertyType == val.GetType())
        {
            return val;
        }

        if (property.PropertyType == typeof(int))
        {
            return Convert.ToInt32(val);
        }
        else if (property.PropertyType == typeof(decimal))
        {
            return Convert.ToDecimal(val);
        }
        else if (property.PropertyType == typeof(decimal?))
        {
            return val == null ? (decimal?)null : Convert.ToDecimal(val);
        }
        else if (property.PropertyType == typeof(int?))
        {
            return val == null ? (int?)null : Convert.ToInt32(val);
        }
        else if (property.PropertyType == typeof(DateTime))
        {
            return Convert.ToDateTime(val);
        }
        else if (property.PropertyType == typeof(DateTime?))
        {
            return val == null ? (DateTime?)null : Convert.ToDateTime(val);
        }
            
        return val;
    }

    public static object? GetValue(this ExpressionResult result, object target, NavigatorConfigOptions? options = null)
    {
        if (options == null)
        {
            options = new NavigatorConfigOptions(); 
        }

        // If delegate provided
        if (result.GetDelegate != null)
        {
            try
            {
                return result.GetDelegate(target);
            }
            catch (NullReferenceException)
            {
                // Assuming situation when jsonpath "$.Nested.Name" but target.Nested == null
                // throw if less than 3 segments like "$.Nested"
                if (result.Tokens.Count < 3)
                {
                    throw;
                }
            }
        }

        CheckForNotSinglePath(result);

        // Iterate through tokens resolving value
        var currentObject = target;

        for (int i = 1; i < result.Tokens.Count; i++)
        {
            currentObject = Resolve(result.Expression, currentObject, result.Tokens[i], options);

            if (currentObject == null)
            {
                return null;
            }
        }

        return currentObject;
    }

    private static void CheckForNotSinglePath(ExpressionResult result)
    {
        if (result.Tokens.Take(result.Tokens.Count - 1).Any(t => t.CollectionDetails != null && t.CollectionDetails.SelectAll))
        {
            throw new NavigationException($"Path '{result.Expression}': cannot get/set single value in a wild card collection");
        }
    }

    private static List<object?>? ExtractCollectionItems(string expression, ICollection collection)
    {
        var resultList = new List<object?>();
        var dictObj = collection as IDictionary;
        var listObj = collection as IList;

        if (dictObj != null)
        {
            foreach (var item in dictObj.Values)
            {
                resultList.Add(item);
            }
        }
        else if (listObj != null)
        {
            foreach (var item in listObj)
            {
                resultList.Add(item);
            }
        }
        else
        {
            throw new NavigationException($"Path '{expression}': only IDictionary or IList properties are supported");
        }

        return resultList;
    }

    private static List<object?>? ResolveValues(string expression, object currentObject, TokenInfo token, NavigatorConfigOptions options)
    {
        var emptyResult = new List<object?>();

        if (token.CollectionDetails != null && token.CollectionDetails.SelectAll)
        {
            var collection = GetCollectionProperty(expression, currentObject, token);

            if (collection == null)
            {
                return emptyResult;
            }

            var items = ExtractCollectionItems(expression, collection);
            return items;
        }

        var value = Resolve(expression, currentObject, token, options);

        if (value != null)
        {
            return [value];
        }

        return emptyResult;
    }

    private static object? Resolve(string expression, object currentObject, TokenInfo token, NavigatorConfigOptions options)
    {
        if (token.CollectionDetails != null)
        {
            if (token.CollectionDetails.SelectAll)
            {
                var collection = GetCollectionProperty(expression, currentObject, token);

                if (collection == null)
                {
                    return null;
                }

                return collection;
            }
            else if (token.CollectionDetails.Literal != "")
            {
                var dictionary = GetCollectionProperty(expression, currentObject, token) as IDictionary;

                if (dictionary == null)
                {
                    return null;
                }

                if (!dictionary.Contains(token.CollectionDetails.Literal))
                {
                    if (options.FailOnCollectionKeyNotFound)
                    {
                        throw new NavigationException($"Path '{expression}': dictionary key '{token.CollectionDetails.Literal}' not found");
                    }

                    return null;
                }

                return dictionary[token.CollectionDetails.Literal];
            }

            var list = GetCollectionProperty(expression, currentObject, token) as IList;

            if (list == null)
            {
                return null;
            }

            var index = token.CollectionDetails.Index!.Value;

            if (!options.FailOnCollectionKeyNotFound && index >= list.Count)
            {
                // return null instead of exception if FailOnCollectionKeyNotFound == false
                return null; 
            }

            // ToDo: should we re-throw NavigationException instead of ArgumentOutOfRangeException here?
            return list[index];
        }

        var prop = currentObject.GetType().GetProperty(token.Field);

        if (prop == null)
        {
            throw new NavigationException($"Path '{expression}': property '{token.Field}' not found");
        }

        return prop.GetValue(currentObject);
    }

    private static ICollection GetCollectionProperty(string expression, object currentObject, TokenInfo token)
    {
        var allProperty = currentObject.GetType().GetProperty(token.Field);

        if (allProperty == null)
        {
            throw new NavigationException($"Path '{expression}': property '{token.Field}' not found");
        }

        var propertValue = allProperty.GetValue(currentObject);

        if (propertValue == null)
        {
            return null;
        }

        var collection = propertValue as ICollection;

        if (collection == null)
        {
            throw new NavigationException($"Path '{expression}': only IDictionary or IList collections are supported");
        }

        return collection;
    }
}

