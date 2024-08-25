using FluentResults;
using JsonPathToModel.Exceptions;
using JsonPathToModel.Parser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Text;

namespace JsonPathToModel;

public static class ExpressionResultExtensions
{
    public static void SetValue(this ExpressionResult result, object target, object val)
    {
        // If delegate provided
        if (result.SetDelegate != null)
        {
            try
            {
                result.SetDelegate(target, val);
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
            currentObject = Resolve(result.Expression, currentObject, result.Tokens[i]);

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
        else if (property.PropertyType == typeof(int))
        {
            property.SetValue(currentObject, Convert.ToInt32(val));
        }
        else if (property.PropertyType == typeof(decimal))
        {
            property.SetValue(currentObject, Convert.ToDecimal(val));
        }
        else if (property.PropertyType == typeof(decimal?))
        {
            property.SetValue(currentObject, val == null ? (decimal?)null : Convert.ToDecimal(val));
        }
        else if (property.PropertyType == typeof(int?))
        {
            property.SetValue(currentObject, val == null ? (int?)null : Convert.ToInt32(val));
        }
        else
        {
            property.SetValue(currentObject, val);
        }
    }

    public static object? GetValue(this ExpressionResult result, object target)
    {
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
            currentObject = Resolve(result.Expression, currentObject, result.Tokens[i]);

            if (currentObject == null)
            {
                return null;
            }
        }

        return currentObject;
    }

    private static void CheckForNotSinglePath(ExpressionResult result)
    {
        if (result.Tokens.Take(result.Tokens.Count - 1).Any(t => t.Collection != null && t.Collection.SelectAll))
        {
            throw new NavigationException($"Path '{result.Expression}': cannot get/set single value in a wild card collection");
        }
    }

    private static object? Resolve(string expression, object currentObject, TokenInfo token)
    {
        if (token.Collection != null)
        {
            if (token.Collection.SelectAll)
            {
                var collection = GetCollectionProperty(expression, currentObject, token);

                if (collection == null)
                {
                    return null;
                }

                return collection;
            }
            else if (token.Collection.Literal != "")
            {
                var dictionary = GetCollectionProperty(expression, currentObject, token) as IDictionary;

                if (dictionary == null)
                {
                    return null;
                }

                if (!dictionary.Contains(token.Collection.Literal))
                {
                    throw new NavigationException($"Path '{expression}': dictionary key '{token.Collection.Literal}' not found");
                }

                return dictionary[token.Collection.Literal];
            }

            var list = GetCollectionProperty(expression, currentObject, token) as IList;

            if (list == null)
            {
                return null;
            }

            return list[token.Collection.Index.Value];
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

