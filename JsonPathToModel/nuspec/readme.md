﻿# JsonPathToModel
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

**1.6.0+**
- added HackingExtensions: WithHack, StealValue, StealString

**1.5.0+**
- added ModelStateExplorer to create JsonPath for type properties and fields
- added support of fields
- added support of nonpublics
- added ModelSnapshot to extract model state into a serializable form

**1.3.0 - 1.3.2**
- fixed few bugs
- extended ReflectionHelper to support JsonIgnore

**1.2.6**
- fixed a few bugs in tokenizer

**1.2.5**
- fixed bug in auto-convert for SetValue, it works now when OptimizeWithCodeEmitter = true too

**1.2.0 - 1.2.4:**
- Dramatically improved performance
- Added DI services registration
- Added Sigil Emitter optimizations for GetValue/SetValue simple expressions like '$.Person.FirstName'
- Added Tokenizer for parsing JSONPath expressions
- Added DateTime auto-convert for SetValue, it works only when OptimizeWithCodeEmitter = false
- Refactored exceptions
- -> NETStandard2.1
- ***Breaking changes:*** GetValue now throws exceptions, for Result<> pattern use GetValueResult

**1.0.0 - 1.1.0:**
- Initital import from ProCodersPtyLtd/BlazorForms