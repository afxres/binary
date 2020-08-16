using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Mikodev.Binary.Benchmarks.GeneratorTests
{
    public class GeneratorBenchmarks
    {
        private IGenerator generator;

        private Dictionary<Type, IConverter> dictionary;

        private ConcurrentDictionary<Type, IConverter> concurrentDictionary;

        [Params(typeof(object), typeof(int), typeof(string))]
        public Type Value;

        [GlobalSetup]
        public void Setup()
        {
            generator = Generator.CreateDefault();
            dictionary = new[] { typeof(object), typeof(int), typeof(string) }.ToDictionary(x => x, generator.GetConverter);
            concurrentDictionary = new ConcurrentDictionary<Type, IConverter>(dictionary);
        }

        [Benchmark(Description = "Get Converter (IGenerator)")]
        public IConverter G01() => generator.GetConverter(Value);

        [Benchmark(Description = "Get Converter (Dictionary)")]
        public IConverter D01() => dictionary[Value];

        [Benchmark(Description = "Get Converter (ConcurrentDictionary)")]
        public IConverter C01() => concurrentDictionary[Value];
    }
}
