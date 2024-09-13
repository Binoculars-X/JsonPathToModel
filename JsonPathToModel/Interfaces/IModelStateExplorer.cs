using System;
using System.Collections.Generic;
using System.Text;

namespace JsonPathToModel;

public interface IModelStateExplorer
{
    ModelStateResult FindModelStateItems(Type modelType, ModelSearchParams ps);
}
