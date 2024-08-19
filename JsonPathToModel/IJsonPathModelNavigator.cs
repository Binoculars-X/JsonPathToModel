using FluentResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonPathToModel;

public interface IJsonPathModelNavigator
{
    Result<object> GetValue(object model, string modelBinding);
    Result<IEnumerable<object>> GetItems(object model, string itemsBinding);
    Result SetValue(object model, string modelBinding, object val);
    Result<List<object?>> SelectValues(object model, string modelBinding);
}
