using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonPathToModel.Tests.ModelData;

public class SampleModel
{
    public string Id { get; set; } 
    public string? Name { get; set; }
    public Gender? Gender { get; set; }
    public string[] MiddleNames { get; set; }
    public List<string> MiddleNamesList { get; set; }
    public SampleNested? Nested { get; set; } = new ();
    public List<SampleNested>? NestedList { get; set; } = [];
    public Dictionary<string, SampleNested>? NestedDictionary { get; set; } = [];
}

public class SampleNested
{
    public string? Id { get; set; }
    public string? Name { get; set; }
}

public enum Gender
{
    NA,
    Male,
    Female
}
