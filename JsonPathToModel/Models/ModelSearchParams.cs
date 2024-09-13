using System;
using System.Collections.Generic;
using System.Text;

namespace JsonPathToModel;

public class ModelSearchParams
{
    public ModelSearchParams()
    {
    }

    public static ModelSearchParams FullModelState()
    {
        return new ModelSearchParams()
        {
            Fields = true,
            Properties = true,
            Public = true,
            NonPublic = true,
            Nested = false,
        };
    }

    public bool Fields { get; set; } = true;
    public bool Properties { get; set; } = true;
    public bool Public { get; set; } = true;
    public bool NonPublic { get; set; }
    public bool Nested { get; set; } = false;
    public string? ExcludeStartsWith { get; set; }
}
