# JsonPathToModel
Use JSONPath to navigate through .NET in-memory models

**DI registration:**

```
// program
...
    services.AddJsonPathToModel(options => 
                { 
                    options.OptimizeWithCodeEmitter = true;
                });
...

// constructor
public void MyService(IJsonPathModelNavigator navigator)
...
```

**Usage:**

```
var model = new SampleModel
{
    Id = "7",
    Name = "Gerry",
    Nested = new([new SampleNested { Id = "xyz", Name = "Pedro" }])
};

var navi = new JsonPathModelNavigator();

var resultSingle = navi.GetValue(model, "$.Id");
navi.GetValue(model, "$.Nested[0].Name");

var resultValues = navi.SelectValues(model, "$.Nested[*].Id");
navi.SelectValues(model, "$.Nested[*].Name");

navi.SetValue(model, "$.Nested[0].Name", "Abdula");
```

**Model:**

```
public class SampleModel
{
    public string Id { get; set; } 
    public string Name { get; set; }
    public List<SampleNested> Nested { get; set; } = [];
}

public class SampleNested
{
    public string Id { get; set; }
    public string Name { get; set; }
}
```

**Release Notes**

**1.2.0 - 1.2.3:**
- Dramatically improved performance
- Added DI services registration
- Added Sigil Emitter optimizations for GetValue/SetValue simple expressions like '$.Person.FirstName'
- Added Tokenizer for parsing JSONPath expressions
- Refactored exceptions
- -> NETStandard2.1
- ***Breaking changes:*** GetValue now throws exceptions, for Result<> pattern use GetValueResult

**1.0.0 - 1.1.0:**
- Initital import from ProCodersPtyLtd/BlazorForms