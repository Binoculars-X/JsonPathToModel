using JsonPathToModel.Tests.ModelData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonPathToModel.Tests;

public class ReflectionHelperTests
{
    [Fact]
    public void ReflectionHelper_Test()
    {
        var list = ReflectionHelper.GetTypeNestedPropertyJsonPaths(typeof(SampleModel));
        Assert.NotNull(list);
        Assert.Equal(6, list.Count);
        Assert.Contains("$.Id", list);
        Assert.Contains("$.Name", list);
        Assert.Contains("$.Nested[*].Id", list);
        Assert.Contains("$.Nested[*].Name", list);
        Assert.Contains("$.NestedDictionary[*].Id", list);
        Assert.Contains("$.NestedDictionary[*].Name", list);
    }
}
