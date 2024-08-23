using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BenchmarkConsoleApp;

[MemoryDiagnoser]
public class JsonPathToModelBenchmarks
{
    // straight c# code return _model.Person.FirstName
    // 2.8ns for straight
    // 3.1ns for nested
    [Benchmark]
    public string DotNetCodeGetValue() => JsonPathToModelUsage.DotNetCodeGetValue();

    // 1st version of JsonPath navigation using basic string.Split parser
    // 514ns for straight
    // 817ns for nested
    [Benchmark]
    public string JpathV1GetValue() => JsonPathToModelUsage.JpathV1GetValue();

    // 2nd version using Tokenizer, caching Tokenizer results, caching segment emitters model.Person, Person.FirstName, etc.
    // 74ns for nested (cached)
    // 225ns for nested (not cached)
    // 64ns with nested cached emitter - segment emitters don't improve too much
    [Benchmark]
    public string JpathV2GetValue() => JsonPathToModelUsage.JpathV2GetValue();

    // Full code emitter of expression _model.Person.FirstName
    // 6.5ns nested
    [Benchmark]
    public string GetJsonPathStraightEmitterGet() => JsonPathToModelUsage.GetJsonPathStraightEmitterGet();
}
