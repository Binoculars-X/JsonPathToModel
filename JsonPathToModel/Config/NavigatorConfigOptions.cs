using System;
using System.Collections.Generic;
using System.Text;

namespace JsonPathToModel;

public class NavigatorConfigOptions
{
    public bool OptimizeWithCodeEmitter { get; set; }
    public bool FailOnCollectionKeyNotFound { get; set; } = true;
}
