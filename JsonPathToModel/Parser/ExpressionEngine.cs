using Sigil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace JsonPathToModel.Parser;

public class ExpressionEngine
{
    private readonly NavigatorConfigOptions _options;
    private readonly Dictionary<Type, Dictionary<string, ExpressionResult>> _expressionCache = [];

    public ExpressionEngine(NavigatorConfigOptions? options = null)
    {
        _options = options ?? new NavigatorConfigOptions();
    }

    public ExpressionResult ParseJsonPathExpression(object target, string path)
    {
        // 1. read all tokens and build initial tree
        var exprResult = ParseExpression(target.GetType(), path);
        return exprResult;
    }
    /*
    //public object GetValue(object target, string path)
    //{
    //    // 1. read all tokens and build initial tree
    //    var exprResult = ParseExpression(target.GetType(), path);

    //    // 2. Iterate through tokens resolving value
    //    var currentObject = target;

    //    for (int i = 1; i < exprResult.Tokens.Count; i++)
    //    {
    //        //var prop = currentObject.GetType().GetProperty(tokenList[i].Field);
    //        //currentObject = prop?.GetValue(currentObject);
    //        currentObject = Resolve(currentObject, exprResult.Tokens[i]);
    //    }

    //    return currentObject;
    //}



    //public static Emit<Func<object, object>> GetJsonPathStraightEmitterGet(Type modelType, string binding)
    //{
    //    Emit<Func<object, object>> result;
    //    var path = binding.Replace("$", "").Split('.').Where(c => !string.IsNullOrWhiteSpace(c)).ToList();

    //    if (binding.Contains("[") || binding.Contains("]"))
    //    {
    //        throw new ArgumentException("Ony straight binding supported: '$.Model.SubModel.SubSubModel.Value'");
    //    }

    //    var currentType = modelType;

    //    result = Emit<Func<object, object>>.NewDynamicMethod(binding.Replace("$", "").Replace(".", ""));
    //    result.LoadArgument(0);

    //    foreach (var prop in path)
    //    {
    //        var propInfo = currentType.GetProperty(prop);

    //        result.CastClass(currentType);
    //        result.Call(propInfo.GetGetMethod(true)!);

    //        using (var a = result.DeclareLocal(propInfo.PropertyType))
    //        {
    //            result.StoreLocal(a);
    //            result.LoadLocal(a);
    //        }

    //        currentType = propInfo.PropertyType;
    //    }

    //    if (currentType.IsBoxable())
    //    {
    //        result.Box(currentType);
    //    }

    //    result.Return();
    //    return result;
    //}

    //private static Emit<Func<object, object>> GetPropertyEmitter(Type modelType, string property, PropertyInfo propertyInfo)
    //{
    //    var result = Emit<Func<object, object>>
    //            .NewDynamicMethod(property)
    //            .LoadArgument(0)
    //            .CastClass(modelType)
    //            .Call(propertyInfo.GetGetMethod(true)!);

    //    if (propertyInfo.PropertyType.IsBoxable())
    //    {
    //        result = result.Box(propertyInfo.PropertyType);
    //    }

    //    result = result.Return();

    //    return result;
    //}

    //private object? Resolve(object currentObject, TokenInfo token)
    //{
    //    var prop = currentObject.GetType().GetProperty(token.Field);
    //    return prop?.GetValue(currentObject);
    //}
    */

    private ExpressionResult ParseExpression(Type type, string expressionText)
    {
        Dictionary<string, ExpressionResult> typeDicitonary;
        ExpressionResult cachedTokenInfo;

        if (_expressionCache.TryGetValue(type, out typeDicitonary))
        {
            if (typeDicitonary.TryGetValue(expressionText, out cachedTokenInfo))
            {
                return cachedTokenInfo;
            }
        }
        else
        {
            _expressionCache[type] = [];
        }

        var tokenList = new List<TokenInfo>();

        using (TextReader sr = new StringReader(expressionText))
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

        var result = new ExpressionResult(tokenList, GetStraightEmitterGet(type, tokenList)?.CreateDelegate());
        _expressionCache[type][expressionText] = result;
        return result;
    }

    private Emit<Func<object, object>>? GetStraightEmitterGet(Type modelType, List<TokenInfo> tokens)
    {
        if (!_options.OptimizeWithCodeEmitter || tokens.Any(t => t.Collection != null))
        {
            // only if OptimizeWithCodeEmitter option is enabled
            // collections not supported yet
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

            var propInfo = currentType.GetProperty(token.Field);

            if (propInfo == null)
            {
                throw new ParserException($"property '{token.Field}' not found");
            }

            result.CastClass(currentType);
            result.Call(propInfo.GetGetMethod(true)!);

            using (var a = result.DeclareLocal(propInfo.PropertyType))
            {
                result.StoreLocal(a);
                result.LoadLocal(a);
            }

            currentType = propInfo.PropertyType;
        }

        if (currentType.IsBoxable())
        {
            result.Box(currentType);
        }

        result.Return();
        return result;
    }
}

