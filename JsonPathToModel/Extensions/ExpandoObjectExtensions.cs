﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonPathToModel;

public static class ExpandoObjectExtensions
{
    public static void SetValue(this ExpandoObject eo, string property, object value)
    {
        (eo as IDictionary<string, object>)[property] = value;
    }

    public static object GetValue(this ExpandoObject eo, string property)
    {
        return (eo as IDictionary<string, object>)[property];
    }

    public static ExpandoObject Clone(this ExpandoObject source)
    {
        var result = new ExpandoObject();
        var dict = result as IDictionary<string, object>;
        var sourceDict = source as IDictionary<string, object>;

        foreach (var key in sourceDict.Keys)
        {
            dict[key] = sourceDict[key];
        }

        return result;
    }

    public static ExpandoObject? CopyTo(this ExpandoObject source, ExpandoObject result)
    {
        if (source == null || result == null)
        {
            return null;
        }

        var dict = result as IDictionary<string, object>;
        dict.Clear();
        var sourceDict = source as IDictionary<string, object>;

        foreach (var key in sourceDict.Keys)
        {
            dict[key] = sourceDict[key];
        }

        return result;
    }

    public static void CopyFrom(this ExpandoObject me, Dictionary<string, object> source)
    {
        foreach (var key in source.Keys)
        {
            me.SetValue(key, source[key]);
        }
    }

    public static Dictionary<string, object> ToDict(this ExpandoObject me)
    {
        var dict = me as IDictionary<string, object>;
        var result = new Dictionary<string, object>();

        foreach (var key in dict.Keys)
        {
            result[key] = dict[key];
        }

        return result;
    }

}

