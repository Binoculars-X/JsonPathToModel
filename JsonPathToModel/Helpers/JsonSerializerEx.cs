using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace JsonPathToModel.Helpers;

public static class JsonSerializerEx
{
    internal static ConcurrentDictionary<string, Type> _types = [];

    internal static MethodInfo _deserializeMethod = typeof(JsonSerializer).GetMethods()
        .Where(x => x.Name == "Deserialize" && x.ReturnType?.Name == "TValue")
        .Where(x => x.GetParameters()?.Count() == 2)
        .First(x => x.GetParameters().ElementAt(0).Name == "json" && x.GetParameters().ElementAt(1).Name == "options");

    public static object? Deserialize(string json, string typeName)
    {
        var type = FindType(typeName);
        MethodInfo generic = _deserializeMethod.MakeGenericMethod(type);
        var obj = generic.Invoke(null, [json, null]);
        return obj;
    }

    private static Type FindType(string typeName)
    {   
        Type type;

        if (_types.TryGetValue(typeName, out type))
        {
            return type;
        }

        type = Type.GetType(typeName);

        type = type ?? AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes())
            .FirstOrDefault(t => t.FullName == typeName);

        _types[typeName] = type;
        return type;
    }
}
