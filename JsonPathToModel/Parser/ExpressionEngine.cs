using Sigil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using JsonPathToModel.Exceptions;
using JsonPathToModel.Interfaces;

namespace JsonPathToModel.Parser;

internal class ExpressionEngine
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
        var exprResult = ParseExpression(target is Type ? (Type)target : target.GetType(), path);
        return exprResult;
    }
 
    private ExpressionResult ParseExpression(Type type, string expression)
    {
        Dictionary<string, ExpressionResult> typeDicitonary;
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

        if (!_options.OptimizeWithCodeEmitter || tokens.Any(t => t.CollectionDetails != null))
        {
            // only if OptimizeWithCodeEmitter option is enabled
            // collections not supported yet
            return resultDetails;
        }

        Emit<Action<object, object>> result;
        var currentType = modelType;
        PropertyInfo propInfo = null!;

        result = Emit<Action<object, object>>.NewDynamicMethod();
        result.LoadArgument(0);

        for (int i = 1; i < tokens.Count - 1; i++)
        {
            var token = tokens[i];

            propInfo = currentType.GetProperty(token.Field);

            if (propInfo == null)
            {
                throw new NavigationException($"Path '{expression}': property '{token.Field}' not found");
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

        result.CastClass(currentType);
        propInfo = currentType.GetProperty(tokens.Last().Field!);

        result.LoadArgument(1);

        if (propInfo.PropertyType.IsBoxable())
        {
            result.UnboxAny(propInfo.PropertyType);
        }
        else
        {
            result.CastClass(propInfo.PropertyType);
        }

        result.Call(propInfo.GetSetMethod(true)!);

        resultDetails = new SetDelegateDetails(propInfo, result.Return().CreateDelegate());
        return resultDetails;
    }

    private Emit<Func<object, object>>? GetStraightEmitterGet(string expression, Type modelType, List<TokenInfo> tokens)
    {
        if (!_options.OptimizeWithCodeEmitter || tokens.Any(t => t.CollectionDetails != null))
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
                throw new NavigationException($"Path '{expression}': property '{token.Field}' not found");
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

