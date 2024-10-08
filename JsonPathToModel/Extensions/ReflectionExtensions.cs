﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JsonPathToModel;

public static class ReflectionExtensions
{
    public static bool IsPrimitive(this PropertyInfo property)
    {
        return property.PropertyType.IsPrimitive();
    }

    public static bool IsPrimitive(this Type type)
    {
        var t = type;

        var result = t.IsPrimitive || t == typeof(decimal) || t == typeof(decimal?) || t == typeof(string) || t == typeof(DateTime)
            || t == typeof(DateTime?) || t == typeof(DateOnly) || t == typeof(DateOnly?) || t == typeof(byte[])
            || t == typeof(int?) || t == typeof(float?) || t == typeof(double?) || t == typeof(long?)
            || t.IsEnum || Nullable.GetUnderlyingType(t)?.IsEnum == true;

        return result;
    }

    public static bool IsBoxable(this Type t)
    {
        return t.IsPrimitive || t == typeof(decimal) || t == typeof(DateTime) || t == typeof(DateTime?)
            || t == typeof(DateOnly) || t == typeof(DateOnly?) || t.IsEnum || Nullable.GetUnderlyingType(t)?.IsEnum == true
            || Nullable.GetUnderlyingType(t) != null;
    }
}
