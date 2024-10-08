﻿using FluentResults;
using JsonPathToModel.Exceptions;
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
    private static BindingFlags _visibilityAll = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public;

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

                var items = ExtractCollectionItems(result.Expression, collection, lastToken, options);
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
                var typedValue = TryConvertToTargetType(result.SetDelegateDetails.TargetProperty.PropertyType, val);
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
        
        var name = result.Tokens.Last().Field;
        var property = currentObject.GetType().GetProperty(name, _visibilityAll);
        
        if (property == null || currentObject == null || property.SetMethod == null)
        {
            var field = currentObject.GetType().GetField(name, _visibilityAll);

            if (field == null)
            {
                throw new NavigationException($"Path '{result.Expression}': property or field '{name}' not found or SetMethod is null");
            }

            field.SetValue(currentObject, TryConvertToTargetType(field.FieldType, val));
            return;
        }

        if (currentObject.GetType() == typeof(ExpandoObject))
        {
            // ToDo: this should be tested
            (currentObject as ExpandoObject)!.SetValue(property.Name, val);
        }
        else
        {
            property.SetValue(currentObject, TryConvertToTargetType(property.PropertyType, val));
        }
    }

    static object? TryConvertToTargetType(Type type, object? val)
    {
        if (val == null || type == val.GetType())
        {
            return val;
        }

        if (type == typeof(int))
        {
            return Convert.ToInt32(val);
        }
        else if (type == typeof(decimal))
        {
            return Convert.ToDecimal(val);
        }
        else if (type == typeof(decimal?))
        {
            return val == null ? (decimal?)null : Convert.ToDecimal(val);
        }
        else if (type == typeof(int?))
        {
            return val == null ? (int?)null : Convert.ToInt32(val);
        }
        else if (type == typeof(DateTime))
        {
            return Convert.ToDateTime(val);
        }
        else if (type == typeof(DateTime?))
        {
            return val == null ? (DateTime?)null : Convert.ToDateTime(val);
        }
        else if (type == typeof(bool))
        {
            return Convert.ToBoolean(val);
        }
        else if (type == typeof(bool?))
        {
            return val == null ? (bool?)null : Convert.ToBoolean(val);
        }
            
        return val;
    }

    public static PropertyInfo? GetPropertyInfo(this ExpressionResult result, Type targetType)
    {
        CheckForNotSinglePath(result);

        var currentType = targetType;
        PropertyInfo? propertyInfo = null;

        for (int i = 1; i < result.Tokens.Count; i++)
        {
            propertyInfo = currentType.GetProperty(result.Tokens[i].Field);
            currentType = propertyInfo.PropertyType;
        }

        return propertyInfo;
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

    private static List<object?>? ExtractCollectionItems(string expression, ICollection collection, TokenInfo token,
        NavigatorConfigOptions? options)
    {
        var resultList = new List<object?>();
        var dictObj = collection as IDictionary;
        var listObj = collection as IList;

        if (dictObj != null)
        {
            if (token.CollectionDetails.Literal != "")
            {
                if (!dictObj.Contains(token.CollectionDetails.Literal))
                {
                    if (options?.FailOnCollectionKeyNotFound == true)
                    {
                        throw new NavigationException($"Path '{expression}': dictionary key '{token.CollectionDetails.Literal}' not found");
                    }

                    //resultList.Add(null);
                }
                else
                {
                    resultList.Add(dictObj[token.CollectionDetails.Literal]);
                }
            }
            else
            {
                foreach (var item in dictObj.Values)
                {
                    resultList.Add(item);
                }
            }
        }
        else if (listObj != null)
        {
            if (token.CollectionDetails.Index != null)
            {
                if (!options.FailOnCollectionKeyNotFound && token.CollectionDetails.Index >= listObj.Count)
                {
                    // return null instead of exception if FailOnCollectionKeyNotFound == false
                    //resultList.Add(null);
                }
                else
                {
                    resultList.Add(listObj[token.CollectionDetails.Index.Value]);
                }
            }
            else
            {
                foreach (var item in listObj)
                {
                    resultList.Add(item);
                }
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

            var items = ExtractCollectionItems(expression, collection, token, options);
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
        
        var prop = currentObject.GetType().GetProperty(token.Field, _visibilityAll);

        if (prop == null)
        {
            var field = currentObject.GetType().GetField(token.Field, _visibilityAll);

            if (field == null)
            {
                throw new NavigationException($"Path '{expression}': property or field '{token.Field}' not found");
            }

            return field.GetValue(currentObject);
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

