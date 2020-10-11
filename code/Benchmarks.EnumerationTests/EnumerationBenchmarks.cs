using BenchmarkDotNet.Attributes;
using Mikodev.Binary.Benchmarks.EnumerationTests.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mikodev.Binary.Benchmarks.EnumerationTests
{
    [MemoryDiagnoser]
    public class EnumerationBenchmarks
    {
        private byte[] buffer;

        private Converter<int> converter;

        private HashSet<int> collection;

        private Dictionary<int, int> dictionary;

        [Params(0, 1 << 4, 1 << 8, 1 << 12, 1 << 16, 1 << 20)]
        public int Count;

        [GlobalSetup]
        public void Setup()
        {
            var source = Enumerable.Range(0, Count).ToList();
            var generator = Generator.CreateDefault();
            buffer = new byte[1 << 24];
            converter = generator.GetConverter<int>();
            collection = source.ToHashSet();
            dictionary = source.ToDictionary(x => x);
        }

        [Benchmark(Description = "Encode HashSet (foreach interface)")]
        public void S01()
        {
            var allocator = new Allocator(new Span<byte>(buffer));
            EnumerationEncoder.EncodeEnumerableForEach(ref allocator, converter, collection);
        }

        [Benchmark(Description = "Encode HashSet (foreach)")]
        public void S02()
        {
            var allocator = new Allocator(new Span<byte>(buffer));
            EnumerationEncoder.EncodeHashSetForEach(ref allocator, converter, collection);
        }

        [Benchmark(Description = "Encode HashSet (to array then foreach)")]
        public void S03()
        {
            var allocator = new Allocator(new Span<byte>(buffer));
            EnumerationEncoder.EncodeCollectionToArrayThenForEach(ref allocator, converter, collection);
        }

        [Benchmark(Description = "Encode Dictionary (foreach interface)")]
        public void D01()
        {
            var allocator = new Allocator(new Span<byte>(buffer));
            EnumerationEncoder.EncodeKeyValueEnumerableForEach(ref allocator, converter, converter, dictionary);
        }

        [Benchmark(Description = "Encode Dictionary (foreach)")]
        public void D02()
        {
            var allocator = new Allocator(new Span<byte>(buffer));
            EnumerationEncoder.EncodeDictionaryForEach(ref allocator, converter, converter, dictionary);
        }

        [Benchmark(Description = "Encode Dictionary (to array then foreach)")]
        public void D03()
        {
            var allocator = new Allocator(new Span<byte>(buffer));
            EnumerationEncoder.EncodeKeyValueCollectionForEach(ref allocator, converter, converter, dictionary);
        }
    }
}
