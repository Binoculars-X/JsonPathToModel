using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JsonPathToModel;

public static class ReflectionHelper
{
    public static List<string> GetTypeNestedPropertyJsonPaths(Type target)
    {
        var result = new List<string>();

        var currentProperties = new List<Context>();
        var start = new Context("$", target);
        currentProperties.Add(start);

        while (currentProperties.Any())
        {
            var current = currentProperties[0];
            currentProperties.RemoveAt(0);

            foreach (var prop in current.Properties)
            {
                if (prop.IsPrimitive())
                {
                    result.Add($"{current.Path}.{prop.Name}");
                }
                else if (prop.PropertyType.IsArray)
                {
                    var path = $"{current.Path}.{prop.Name}[*]";
                    var type = prop.PropertyType.GetElementType();

                    if (type?.IsPrimitive() == true)
                    {
                        result.Add(path);
                    }
                    else
                    {
                        currentProperties.Add(new Context(path, type!));
                    }
                }
                else if (typeof(IList).IsAssignableFrom(prop.PropertyType))
                {
                    var path = $"{current.Path}.{prop.Name}[*]";
                    var type = prop.PropertyType.GenericTypeArguments[0];
                    currentProperties.Add(new Context(path, type));
                }
                else if (typeof(IDictionary).IsAssignableFrom(prop.PropertyType))
                {
                    var path = $"{current.Path}.{prop.Name}[*]";
                    var type = prop.PropertyType.GenericTypeArguments[1];
                    currentProperties.Add(new Context(path, type));
                }
                else
                {
                    // Another complex type
                    currentProperties.Add(new Context($"{current.Path}.{prop.Name}", prop.PropertyType));
                }
            }
        }

        return result;
    }

    private record Context(string Path, Type Type, List<PropertyInfo> Properties)
    {
        public Context(string path, Type type) : this(path, type, [.. type.GetProperties()])
        {
        }
    }
}

