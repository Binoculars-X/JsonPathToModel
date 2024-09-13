using System;
using System.Collections.Generic;
using System.Text;

namespace JsonPathToModel;

public record ModelStateResult(string modelTypeName, List<ModelStateItem> Items);

