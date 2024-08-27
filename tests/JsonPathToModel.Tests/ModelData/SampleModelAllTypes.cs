using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonPathToModel.Tests.ModelData;

public class SampleModelAllTypes
{
    public string Id { get; set; } 
    public string? Name { get; set; }
    public SampleModelAllTypesNested? Nested { get; set; } = new();
    public List<SampleModelAllTypesNested>? NestedList { get; set; } = [];
    public Dictionary<string, SampleModelAllTypesNested>? NestedDictionary { get; set; } = [];
}

public class SampleModelAllTypesNested
{
    public int Int { get; set; }
    public int? IntNullable { get; set; }
    public string String { get; set; }
    public string? StringNullable { get; set; }
    public DateTime DateTime { get; set; }
    public DateTime? DateTimeNullable { get; set; }
    // DateOnly - not supported by AutoFixture
    //public DateOnly DateOnly { get; set; }
    //public DateOnly? DateOnlyNullable { get; set; }
    public decimal Decimal { get; set; }
    public decimal? DecimalNullable { get; set; }
    public double Double { get; set; }
    public double? DoubleNullable { get; set; }
    public byte[] Bytes { get; set; }
    public byte[]? BytesNullable { get; set; }
}
