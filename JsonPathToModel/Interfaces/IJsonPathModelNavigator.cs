using FluentResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonPathToModel;

public interface IJsonPathModelNavigator
{
    object? GetValue(object model, string path);
    void SetValue(object model, string path, object? val);
    List<object?> SelectValues(object model, string path);
    Result<object?> GetValueResult(object model, string path);
    Result<IEnumerable<object>> GetItemsResult(object model, string itemsPath);
    Result SetValueResult(object model, string path, object val);
    Result<List<object?>> SelectValuesResult(object model, string path);
}
