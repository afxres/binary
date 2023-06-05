namespace Mikodev.Binary.Benchmarks.GeneratorTests;

using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

public class GeneratorBenchmarks
{
    private IGenerator generator = null!;

    private Dictionary<Type, IConverter> dictionary = null!;

    private ConcurrentDictionary<Type, IConverter> concurrentDictionary = null!;

    [Params(typeof(object), typeof(int), typeof(string))]
    public Type Value = null!;

    [GlobalSetup]
    public void Setup()
    {
        this.generator = Generator.CreateDefault();
        this.dictionary = new[] { typeof(object), typeof(int), typeof(string) }.ToDictionary(x => x, this.generator.GetConverter);
        this.concurrentDictionary = new ConcurrentDictionary<Type, IConverter>(this.dictionary);
    }

    [Benchmark(Description = "Get Converter (IGenerator)")]
    public IConverter G01() => this.generator.GetConverter(this.Value);

    [Benchmark(Description = "Get Converter (Dictionary)")]
    public IConverter D01() => this.dictionary[this.Value];

    [Benchmark(Description = "Get Converter (ConcurrentDictionary)")]
    public IConverter C01() => this.concurrentDictionary[this.Value];
}
