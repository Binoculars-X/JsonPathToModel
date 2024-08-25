# JsonPathToModel
Use JsonPath to navigate through .NET in-memory models

**DI registration:**

```
// program
...
    services.AddJsonPathToModel(options => 
                { 
                    options.OptimizeWithCodeEmitter = true;
                });
...

// use in constructor
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

var result = navi.SetValue(model, "$.Nested[0].Name", "Abdula");
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

**Supported expressions**

*Linear path to get single value:*

```$.Id```
is equivalent to
```model.Id```

```$.Person.PrimaryContact.Email.Value```
is equivalent to
```model.Person.PrimaryContact.Email.Value```

*Get collection item value:*

```$.Roles[1]```
get Role object is equivalent to
```model.Roles[1]```

```$.Roles[1].Name```
get Name property of Role object is equivalent to
```model.Roles[1].Name```

*Get collection all item values*

```$.Roles[*]``` or ```$.Roles[]```
is equivalent to
```model.Roles```

```$.Roles[*].Name``` or ```$.Roles[].Name```
get Name properties of all Roles, is equivalent to
```model.Roles.Select(r => r.Name).ToList()```

*Supported collections*

The following collections are supported:
- Array
- List
- Dictionary

**Setting Values**

Examples:

```
var result1 = navi.SetValue(model, "$.IntId", 123);
var result2 = navi.SetValue(model, "$.Nested[1].Id", "456");
var result3 = navi.SetValue(model, "$.Person.PrimaryContact.Email", new Email("a@a.com"));
var result4 = navi.SetValue(model, "$.Person.PrimaryContact.Email.Value", "a@a.com");
```
If you like what I do and it is helpful, you can give me a cup of coffee :)
[![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://www.paypal.com/donate?hosted_button_id=Q7XEPGTBQFWNG)

**Release Notes**

**1.2.0:**
- Dramatically improved performance
- Added DI services registration
- Added Sigil Emitter optimizations for GetValue simple expressions like '$.Person.FirstName'
- Added Tokenizer for parsing JSONPath expressions
- Refactored exceptions
- ***Breaking changes:*** GetValue now throws exceptions, for Result<> pattern use GetValueResult

**1.0.0 - 1.1.0:**
- Initital import from ProCodersPtyLtd/BlazorForms