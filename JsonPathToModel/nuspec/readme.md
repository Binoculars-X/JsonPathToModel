# JsonPathToModel
Use JsonPath to navigate through .NET in memory models

Usage:

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
Model:
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
