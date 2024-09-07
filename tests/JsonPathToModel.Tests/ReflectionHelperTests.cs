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
        Assert.True(list.Count > 8);
        Assert.Contains("$.Id", list);
        Assert.Contains("$.Name", list);
        Assert.Contains("$.NestedList[*].Id", list);
        Assert.Contains("$.NestedList[*].Name", list);
        Assert.Contains("$.NestedDictionary[*].Id", list);
        Assert.Contains("$.NestedDictionary[*].Name", list);
    }

    [Fact]
    public void ReflectionHelper_Returns_Enums()
    {
        var list = ReflectionHelper.GetTypeNestedPropertyJsonPaths(typeof(SampleModel));
        Assert.NotNull(list);
        Assert.Contains("$.Gender", list);
    }

    [Fact]
    public void ReflectionHelper_Returns_Arrays()
    {
        var list = ReflectionHelper.GetTypeNestedPropertyJsonPaths(typeof(SampleModel));
        Assert.NotNull(list);
        Assert.Contains("$.MiddleNames[*]", list);
        Assert.Contains("$.MiddleNamesList[*]", list);
        Assert.Contains("$.NestedList[*]", list);
        Assert.Contains("$.NestedDictionary[*]", list);
    }

    [Fact]
    public void ReflectionHelper_Ignores_MarkedJsonIgnore()
    {
        var list = ReflectionHelper.GetTypeNestedPropertyJsonPaths(typeof(SampleModel));
        Assert.NotNull(list);
        Assert.DoesNotContain("$.NestedIgnored.Id", list);
        Assert.DoesNotContain("$.NestedIgnored.Name", list);
    }

    [Fact]
    public void ReflectionHelper_Ignores_NullableHasValue()
    {
        var list = ReflectionHelper.GetTypeNestedPropertyJsonPaths(typeof(SampleModel));
        Assert.NotNull(list);
        Assert.DoesNotContain("$.NullableDateTime.Value", list);
        Assert.DoesNotContain("$.NullableDateTime.HasValue", list);
        Assert.DoesNotContain("$.NullableInt.HasValue", list);
        Assert.DoesNotContain("$.NullableInt.Value", list);
    }

    [Fact]
    public void ReflectionHelper_Ignores_CharsAndLength()
    {
        var list = ReflectionHelper.GetTypeNestedPropertyJsonPaths(typeof(SampleModel));
        Assert.NotNull(list);
        Assert.False(list.Any(x => x.EndsWith(".Chars") || x.EndsWith(".Length")));
    }
}
