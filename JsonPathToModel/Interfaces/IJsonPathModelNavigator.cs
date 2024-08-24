using FluentResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonPathToModel;

public interface IJsonPathModelNavigator
{
    object? GetValue(object model, string modelBinding);
    Result<object?> GetValueResult(object model, string modelBinding);
    Result<IEnumerable<object>> GetItemsResult(object model, string itemsBinding);
    Result SetValueResult(object model, string modelBinding, object val);
    Result<List<object?>> SelectValuesResult(object model, string modelBinding);
}
