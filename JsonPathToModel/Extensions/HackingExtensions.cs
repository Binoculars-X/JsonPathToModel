using System;
using System.Collections.Generic;
using System.Text;

namespace JsonPathToModel;

public static class HackingExtensions
{
    private static readonly IJsonPathModelNavigator _navi = new JsonPathModelNavigator();
    /// <summary>
    /// Replaces public/nonpublic field/property with the provided value 
    /// </summary>
    /// <param name="target"></param>
    /// <param name="jsonPath"></param>
    /// <param name="value"></param>
    public static void WithHack(this object target,  string jsonPath, object value)
    {
        var mapping = FormatAndVerifyJsonPath(jsonPath);
        _navi.SetValue(target, mapping, value);
    }

    public static object? StealValue(this object target, string jsonPath)
    {
        var mapping = FormatAndVerifyJsonPath(jsonPath);
        var result = _navi.GetValue(target, mapping);
        return result;
    }

    public static string? StealString(this object target, string jsonPath)
    {
        var mapping = FormatAndVerifyJsonPath(jsonPath);
        var result = _navi.GetValue(target, mapping);
        return Convert.ToString(result);
    }

    private static string FormatAndVerifyJsonPath(string jsonPath)
    {
        var result = jsonPath;

        if (!result.StartsWith("$."))
        {
            result = "$." + result;
        }

        return result;
    }
}
