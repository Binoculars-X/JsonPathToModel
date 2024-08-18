using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonPathToModel;

public interface IJsonPathModelNavigator
{
    object GetValue(object model, string modelBinding);
    IEnumerable<object> GetItems(object model, string itemsBinding);
    void SetValue(object model, string modelBinding, object val);
    List<object?> SelectValues(object model, string modelBinding);
}
