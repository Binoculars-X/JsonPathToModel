﻿using BenchmarkDotNet.Attributes;
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
    // 2.8 ns for straight
    // 3.0 ns for nested
    [Benchmark]
    public string DotNetCodeGetValue() => JsonPathToModelUsage.DotNetCodeGetValue();

    // 1st version of JsonPath navigation using basic string.Split parser
    // 514 ns for straight
    // 817 ns for nested
    //[Benchmark]
    //public string JpathV1GetValue() => JsonPathToModelUsage.JpathV1GetValue();

    // 2nd version using Tokenizer, caching Tokenizer results, caching segment emitters model.Person, Person.FirstName, etc.
    // 77.8 ns for nested (cached)
    // 225 ns for nested (not cached)
    // 64 ns with nested cached emitter - segment emitters don't improve too much
    //[Benchmark]
    //public string JpathV2GetValue() => JsonPathToModelUsage.JpathV2GetValue();

    // Synthetic full code emitter of expression _model.Person.FirstName
    // 6.5 ns nested
    //[Benchmark]
    //public string GetJsonPathStraightEmitterGet() => JsonPathToModelUsage.GetJsonPathStraightEmitterGet();

    [Benchmark]
    public string JpathLatestGetValue() => JsonPathToModelUsage.JpathLatestGetValue();
}