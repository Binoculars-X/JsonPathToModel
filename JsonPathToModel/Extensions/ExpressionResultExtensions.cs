using JsonPathToModel.Parser;
using System;
using System.Collections;
using System.Collections.Generic;
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
            throw new ParserException("cannot get single value from a wild card collection");
        }

        //throw new NotImplementedException();

        // Iterate through tokens resolving value
        var currentObject = target;

        for (int i = 1; i < result.Tokens.Count; i++)
        {
            currentObject = Resolve(currentObject, result.Tokens[i]);

            if (currentObject == null)
            {
                return null;
            }
        }

        return currentObject;
    }

    private static object? Resolve(object currentObject, TokenInfo token)
    {
        if (token.Collection != null)
        {
            if (token.Collection.SelectAll)
            {
                var collection = GetCollectionProperty(currentObject, token);

                if (collection == null)
                {
                    return null;
                }

                return collection;
            }
            else if (token.Collection.Literal != "")
            {
                var dictionary = GetCollectionProperty(currentObject, token) as IDictionary;

                if (dictionary == null)
                {
                    return null;
                }

                return dictionary[token.Collection.Literal];
            }

            // array or list
            //var listProperty = currentObject.GetType().GetProperty(token.Field);

            //if (listProperty == null)
            //{
            //    throw new ParserException($"property '{token.Field}' not found");
            //}

            //var list = listProperty.GetValue(currentObject) as IList;
            var list = GetCollectionProperty(currentObject, token) as IList;

            if (list == null)
            {
                return null;
            }

            return list[token.Collection.Index.Value];
        }

        var prop = currentObject.GetType().GetProperty(token.Field);

        if (prop == null)
        {
            throw new ParserException($"property '{token.Field}' not found");
        }

        return prop.GetValue(currentObject);
    }

    private static ICollection GetCollectionProperty(object currentObject, TokenInfo token)
    {
        var allProperty = currentObject.GetType().GetProperty(token.Field);

        if (allProperty == null)
        {
            throw new ParserException($"property '{token.Field}' not found");
        }

        var propertValue = allProperty.GetValue(currentObject);

        if (propertValue == null)
        {
            return null;
        }

        var collection = propertValue as ICollection;

        if (collection != null)
        {
            //if (collection.Count > 1)
            //{
            //    throw new ParserException($"expected one value but {collection.Count} value(s) found");
            //}

            return collection;
        }
        else
        {
            throw new ParserException($"only IDictionary or IList collections are supported");
        }
    }
}

