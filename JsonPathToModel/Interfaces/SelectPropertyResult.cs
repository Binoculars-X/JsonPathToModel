using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace JsonPathToModel.Interfaces;

public record SelectPropertyResult(object? Target, PropertyInfo? Property)
{
    public object? Value { get; private set; }

    public static SelectPropertyResult FromValue(object value)
    {
        return new SelectPropertyResult(null, null) { Value = value };
    }

    public object? Resolve()
    {
        if (Value != null)
        {
            return Value;
        }

        if (Property == null || Target == null)
        {
            return Target;
        }

        return Property.GetValue(Target);
    }
}
