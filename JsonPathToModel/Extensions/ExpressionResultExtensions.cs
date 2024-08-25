using JsonPathToModel.Exceptions;
using JsonPathToModel.Parser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace JsonPathToModel;

public static class ExpressionResultExtensions
{
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

        if (result.Tokens.Take(result.Tokens.Count-1).Any(t => t.Collection != null && t.Collection.SelectAll))
        {
            throw new NavigationException($"Path '{result.Expression}': cannot get single value from a wild card collection");
        }

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

