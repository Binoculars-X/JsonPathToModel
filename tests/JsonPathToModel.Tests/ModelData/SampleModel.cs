using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace JsonPathToModel.Tests.ModelData;

public class SampleModel
{
    public List<string> MiddleNamesList { get; set; }
    public int? NullableInt { get; set; } 
    public DateTime? NullableDateTime { get; set; } 
    public string Id { get; set; } 
    public string? Name { get; set; }
    public Gender? Gender { get; set; }
    public string[] MiddleNames { get; set; }
    public SampleNested? Nested { get; set; } = new ();
    public List<SampleNested>? NestedList { get; set; } = [];
    public Dictionary<string, SampleNested>? NestedDictionary { get; set; } = [];

    [JsonIgnore]
    public SampleNested? NestedIgnored { get; set; } = new();
}

public class SampleNested
{
    public string? Id { get; set; }
    public string Name { get; set; }
    public DateTime Date { get; set; }
}

public enum Gender
{
    NA,
    Male,
    Female
}
