using Sigil;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace JsonPathToModel.Parser;

public class ExpressionEngine
{
    private readonly Dictionary<Type, Dictionary<string, Func<object, object>>> _delegateNestedCache = [];
    private readonly Dictionary<PropertyInfo, Func<object, object>> _delegatePropertiesCache = [];
    private readonly Dictionary<string, List<TokenInfo>> _expressionCache = [];
    //private static readonly Dictionary<string, Func<object, object>> _delegateCache = [];

    public ExpressionEngine()
    {
    }

    public object GetValue(object target, string path)
    {
        // 1. read all tokens and build initial tree
        var tokenList = ParseExpression(path);

        // 2. Iterate through tokens resolving value
        var currentObject = target;

        for (int i = 1; i < tokenList.Count; i++)
        {
            //var prop = currentObject.GetType().GetProperty(tokenList[i].Field);
            //currentObject = prop?.GetValue(currentObject);
            currentObject = Resolve(currentObject, tokenList[i]);
        }

        return currentObject;
    }

    public object GetValueNestedDictionary(object model, string property)
    {
        var modelType = model.GetType();
        Dictionary<string, Func<object, object>> properties;
        Func<object, object> emitter;

        if (!_delegateNestedCache.TryGetValue(modelType, out properties))
        {
            properties = new Dictionary<string, Func<object, object>>();
            _delegateNestedCache[modelType] = properties;
        }

        if (!properties.TryGetValue(property, out emitter))
        {
            var propertyInfo = model.GetType().GetProperty(property);

            if (propertyInfo == null)
            {
                return null;
            }

            emitter = GetPropertyEmitter(modelType, property, propertyInfo).CreateDelegate();
            properties[property] = emitter;
        }

        var value = emitter(model);
        return value;
    }

    public static Emit<Func<object, object>> GetJsonPathStraightEmitterGet(Type modelType, string binding)
    {
        Emit<Func<object, object>> result;
        var path = binding.Replace("$", "").Split('.').Where(c => !string.IsNullOrWhiteSpace(c)).ToList();

        if (binding.Contains("[") || binding.Contains("]"))
        {
            throw new ArgumentException("Ony straight binding supported: '$.Model.SubModel.SubSubModel.Value'");
        }

        var currentType = modelType;

        result = Emit<Func<object, object>>.NewDynamicMethod(binding.Replace("$", "").Replace(".", ""));
        result.LoadArgument(0);

        foreach (var prop in path)
        {
            var propInfo = currentType.GetProperty(prop);

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

    /*
    public object GetValuePropertyDictionary(object model, string property)
    {
        var modelType = model.GetType();
        Func<object, object> emitter;
        var propertyInfo = modelType.GetProperty(property);

        if (propertyInfo == null)
        {
            return null;
        }

        if (!_delegatePropertiesCache.TryGetValue(propertyInfo, out emitter))
        {
            emitter = GetPropertyEmitter(modelType, property, propertyInfo).CreateDelegate();
            _delegatePropertiesCache[propertyInfo] = emitter;
        }

        var value = emitter(model);
        return value;
    }
    */

    private static Emit<Func<object, object>> GetPropertyEmitter(Type modelType, string property, PropertyInfo propertyInfo)
    {
        var result = Emit<Func<object, object>>
                .NewDynamicMethod(property)
                .LoadArgument(0)
                .CastClass(modelType)
                .Call(propertyInfo.GetGetMethod(true)!);

        if (propertyInfo.PropertyType.IsBoxable())
        {
            result = result.Box(propertyInfo.PropertyType);
        }

        result = result.Return();

        return result;
    }
    private object? ResolveV3(object currentObject, TokenInfo token)
    {
        return GetValueNestedDictionary(currentObject, token.Field);
    }

    private object? Resolve(object currentObject, TokenInfo token)
    {
        var prop = currentObject.GetType().GetProperty(token.Field);
        return prop?.GetValue(currentObject);
    }

    private List<TokenInfo> ParseExpression(string text)
    {
        if (_expressionCache.ContainsKey(text))
        {
            return _expressionCache[text];
        }
        else
        {
            var tokenList = new List<TokenInfo>();

            using (TextReader sr = new StringReader(text))
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

            _expressionCache[text] = tokenList;
            return tokenList;
        }
    }
}

public class Expr
{ }

//class ObjectResolver : IObjectResolver
//{
//    public string[] GetOperators()
//    {
//        throw new NotImplementedException();
//    }

//    public Dictionary<int, string[]> GetPriorityOperators()
//    {
//        throw new NotImplementedException();
//    }

//    public string[] GetQueryFields()
//    {
//        throw new NotImplementedException();
//    }

//    public string[] GetQueryParams()
//    {
//        throw new NotImplementedException();
//    }
//}
