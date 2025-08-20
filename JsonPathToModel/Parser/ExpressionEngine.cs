using Sigil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using JsonPathToModel.Exceptions;
using System.Collections.Concurrent;

namespace JsonPathToModel.Parser;

internal class ExpressionEngine
{
    private static BindingFlags _visibilityAll = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public;
    private readonly NavigatorConfigOptions _options;
    private readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, ExpressionResult>> _expressionCache = [];

    public ExpressionEngine(NavigatorConfigOptions? options = null)
    {
        _options = options ?? new NavigatorConfigOptions();
    }

    public ExpressionResult ParseJsonPathExpression(object target, string path)
    {
        // 1. read all tokens and build initial tree
        var exprResult = ParseExpression(target is Type ? (Type)target : target.GetType(), path);
        return exprResult;
    }
 
    private ExpressionResult ParseExpression(Type type, string expression)
    {
        ConcurrentDictionary<string, ExpressionResult> typeDicitonary;
        ExpressionResult cachedTokenInfo;

        if (_expressionCache.TryGetValue(type, out typeDicitonary))
        {
            if (typeDicitonary.TryGetValue(expression, out cachedTokenInfo))
            {
                return cachedTokenInfo;
            }
        }
        else
        {
            _expressionCache[type] = [];
        }

        var tokenList = new List<TokenInfo>();

        using (TextReader sr = new StringReader(expression))
        {
            var t = new Tokenizer(sr);
            var current = t.Token;

            while (current != Token.EOF)
            {
                tokenList.Add(t.Info);
                t.NextToken();
                current = t.Token;
            }

            if (tokenList.Count < 1 || tokenList[0].Token != Token.Dollar)
            {
                throw new ParserException("Token '$' is expected");
            }
        }

        var result = new ExpressionResult(expression, 
            tokenList, 
            GetStraightEmitterGet(expression, type, tokenList)?.CreateDelegate(),
            GetStraightEmitterSetDetails(expression, type, tokenList));

        _expressionCache[type][expression] = result;
        return result;
    }

    private SetDelegateDetails? GetStraightEmitterSetDetails(string expression, Type modelType, List<TokenInfo> tokens)
    {
        SetDelegateDetails? resultDetails = null;

        if (!_options.OptimizeWithCodeEmitter || !CanOptimizeWithCollections(tokens))
        {
            // only if OptimizeWithCodeEmitter option is enabled and collections can be optimized
            return resultDetails;
        }

        Emit<Action<object, object>> result;
        var currentType = modelType;
        PropertyInfo? propInfo = null;

        result = Emit<Action<object, object>>.NewDynamicMethod();
        result.LoadArgument(0);

        // Navigate to the parent object (all tokens except the last one)
        for (int i = 1; i < tokens.Count - 1; i++)
        {
            var token = tokens[i];

            if (string.IsNullOrEmpty(token.Field))
            {
                throw new NavigationException($"Path '{expression}': token at position {i} has no field name");
            }

            propInfo = currentType.GetProperty(token.Field, _visibilityAll);

            if (propInfo == null)
            {
                var fieldInfo = currentType.GetField(token.Field, _visibilityAll);

                if (fieldInfo == null)
                {
                    throw new NavigationException($"Path '{expression}': property or field '{token.Field}' not found");
                }

                // ToDo: fields not supported
                // https://stackoverflow.com/questions/16073091/is-there-a-way-to-create-a-delegate-to-get-and-set-values-for-a-fieldinfo
                return null;
            }

            result.CastClass(currentType);
            result.Call(propInfo!.GetGetMethod(true)!);

            // Handle collection access if this token has collection details
            if (token.CollectionDetails != null)
            {
                if (!EmitCollectionAccess(result, token, propInfo.PropertyType))
                {
                    return null; // Unsupported collection type, fallback to reflection
                }
                
                // Update current type to collection element type
                currentType = GetCollectionElementType(propInfo.PropertyType);
            }
            else
            {
                // Regular property access
                using (var a = result.DeclareLocal(propInfo.PropertyType))
                {
                    result.StoreLocal(a);
                    result.LoadLocal(a);
                }

                currentType = propInfo.PropertyType;
            }
        }

        // Handle the final token (the property to set)
        var lastToken = tokens.Last();
        result.CastClass(currentType);
        
        propInfo = currentType.GetProperty(lastToken.Field!, _visibilityAll);

        if (propInfo == null)
        {
            var fieldInfo = currentType.GetField(lastToken.Field!, _visibilityAll);

            if (fieldInfo == null)
            {
                throw new NavigationException($"Path '{expression}': property or field '{lastToken.Field}' not found");
            }

            // ToDo: fields not supported
            return null;
        }

        result.LoadArgument(1);

        if (propInfo.PropertyType.IsBoxable())
        {
            result.UnboxAny(propInfo.PropertyType);
        }
        else
        {
            result.CastClass(propInfo.PropertyType);
        }

        result.Call(propInfo!.GetSetMethod(true)!);

        resultDetails = new SetDelegateDetails(propInfo, (Action<object, object?>)result.Return().CreateDelegate());
        return resultDetails;
    }

    private Emit<Func<object, object>>? GetStraightEmitterGet(string expression, Type modelType, List<TokenInfo> tokens)
    {
        if (!_options.OptimizeWithCodeEmitter || !CanOptimizeWithCollections(tokens))
        {
            // only if OptimizeWithCodeEmitter option is enabled and collections can be optimized
            return null;
        }

        Emit<Func<object, object>> result;
        var currentType = modelType;
        result = Emit<Func<object, object>>.NewDynamicMethod();
        result.LoadArgument(0);

        foreach (var token in tokens)
        {
            if (token.Token == Token.Dollar)
            {
                continue;
            }

            var propInfo = currentType.GetProperty(token.Field ?? string.Empty, _visibilityAll);

            if (propInfo == null)
            {
                var fieldInfo = currentType.GetField(token.Field ?? string.Empty, _visibilityAll);

                if (fieldInfo == null)
                {
                    throw new NavigationException($"Path '{expression}': property or field '{token.Field}' not found");
                }

                // ToDo: fields not supported
                return null;
            }
            else
            {
                result.CastClass(currentType);
                result.Call(propInfo.GetGetMethod(true)!);

                // Handle collection access if this token has collection details
                if (token.CollectionDetails != null)
                {
                    if (!EmitCollectionAccess(result, token, propInfo.PropertyType))
                    {
                        return null; // Unsupported collection type, fallback to reflection
                    }
                    
                    // Update current type to collection element type
                    currentType = GetCollectionElementType(propInfo.PropertyType);
                }
                else
                {
                    // Regular property access
                    using (var a = result.DeclareLocal(propInfo.PropertyType))
                    {
                        result.StoreLocal(a);
                        result.LoadLocal(a);
                    }
                    
                    currentType = propInfo.PropertyType;
                }
            }
        }

        if (currentType.IsBoxable())
        {
            result.Box(currentType);
        }

        result.Return();
        return result;
    }

    /// <summary>
    /// Checks if collections in the token list can be optimized with IL emission
    /// Currently supports single-index access only (no wildcards) and only when FailOnCollectionKeyNotFound is true
    /// </summary>
    private bool CanOptimizeWithCollections(List<TokenInfo> tokens)
    {
        // Only optimize when FailOnCollectionKeyNotFound is true, since IL emission
        // cannot gracefully handle out-of-bounds access like reflection can
        if (!_options.FailOnCollectionKeyNotFound)
        {
            return tokens.All(t => t.CollectionDetails == null);
        }

        var collectionsTokens = tokens.Where(t => t.CollectionDetails != null);
        
        // Support single index access and string literal keys, but not wildcards
        return collectionsTokens.All(t => 
            !t.CollectionDetails!.SelectAll && 
            (t.CollectionDetails.Index.HasValue || !string.IsNullOrEmpty(t.CollectionDetails.Literal)));
    }

    /// <summary>
    /// Emits IL code for collection access (arrays, IList, IDictionary)
    /// </summary>
    private bool EmitCollectionAccess(Emit<Func<object, object>> result, TokenInfo token, Type collectionType)
    {
        if (token.CollectionDetails!.Index.HasValue)
        {
            // Array/IList access: collection[index]
            if (collectionType.IsArray)
            {
                // Array access using LoadElement (more efficient)
                result.LoadConstant(token.CollectionDetails.Index.Value);
                result.LoadElement(collectionType.GetElementType()!);
            }
            else if (typeof(IList).IsAssignableFrom(collectionType))
            {
                // IList access using get_Item
                result.CastClass(typeof(IList));
                result.LoadConstant(token.CollectionDetails.Index.Value);
                result.CallVirtual(typeof(IList).GetMethod("get_Item")!);
            }
            else
            {
                return false; // Unsupported collection type
            }
        }
        else if (!string.IsNullOrEmpty(token.CollectionDetails.Literal))
        {
            // Dictionary access: dictionary["key"]
            if (typeof(IDictionary).IsAssignableFrom(collectionType))
            {
                result.CastClass(typeof(IDictionary));
                result.LoadConstant(token.CollectionDetails.Literal);
                result.CallVirtual(typeof(IDictionary).GetMethod("get_Item")!);
            }
            else
            {
                return false; // Unsupported collection type
            }
        }
        else
        {
            return false; // Wildcard access not supported yet
        }

        return true;
    }

    /// <summary>
    /// Emits IL code for collection access for setter operations
    /// </summary>
    private bool EmitCollectionAccess(Emit<Action<object, object>> result, TokenInfo token, Type collectionType)
    {
        if (token.CollectionDetails!.Index.HasValue)
        {
            // Array/IList access: collection[index]
            if (collectionType.IsArray)
            {
                // Array access using LoadElement (more efficient)
                result.LoadConstant(token.CollectionDetails.Index.Value);
                result.LoadElement(collectionType.GetElementType()!);
            }
            else if (typeof(IList).IsAssignableFrom(collectionType))
            {
                // IList access using get_Item
                result.CastClass(typeof(IList));
                result.LoadConstant(token.CollectionDetails.Index.Value);
                result.CallVirtual(typeof(IList).GetMethod("get_Item")!);
            }
            else
            {
                return false; // Unsupported collection type
            }
        }
        else if (!string.IsNullOrEmpty(token.CollectionDetails.Literal))
        {
            // Dictionary access: dictionary["key"]
            if (typeof(IDictionary).IsAssignableFrom(collectionType))
            {
                result.CastClass(typeof(IDictionary));
                result.LoadConstant(token.CollectionDetails.Literal);
                result.CallVirtual(typeof(IDictionary).GetMethod("get_Item")!);
            }
            else
            {
                return false; // Unsupported collection type
            }
        }
        else
        {
            return false; // Wildcard access not supported yet
        }

        return true;
    }

    /// <summary>
    /// Determines the element type of a collection type
    /// </summary>
    private Type GetCollectionElementType(Type collectionType)
    {
        if (collectionType.IsArray)
        {
            return collectionType.GetElementType()!;
        }
        
        if (collectionType.IsGenericType)
        {
            var genericArgs = collectionType.GetGenericArguments();
            
            // IList<T>, ICollection<T>, IEnumerable<T>
            if (typeof(IEnumerable).IsAssignableFrom(collectionType) && genericArgs.Length == 1)
            {
                return genericArgs[0];
            }
            
            // IDictionary<K,V> - return value type
            if (typeof(IDictionary).IsAssignableFrom(collectionType) && genericArgs.Length == 2)
            {
                return genericArgs[1];
            }
        }
        
        // Non-generic collections return object
        return typeof(object);
    }
}

