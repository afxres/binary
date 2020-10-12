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

        private Converter<HashSet<int>> collectionConverter;

        private Converter<Dictionary<int, int>> dictionaryConverter;

        [Params(0, 1 << 4, 1 << 8, 1 << 12, 1 << 16, 1 << 20)]
        public int Count;

        [GlobalSetup]
        public void Setup()
        {
            var source = Enumerable.Range(0, this.Count).ToList();
            var generator = Generator.CreateDefault();
            this.buffer = new byte[1 << 24];
            this.converter = generator.GetConverter<int>();
            this.collection = source.ToHashSet();
            this.dictionary = source.ToDictionary(x => x);
            this.collectionConverter = generator.GetConverter<HashSet<int>>();
            this.dictionaryConverter = generator.GetConverter<Dictionary<int, int>>();
        }

        [Benchmark(Description = "Encode HashSet (foreach interface)")]
        public void S01()
        {
            var allocator = new Allocator(new Span<byte>(this.buffer));
            EnumerationEncoder.EncodeEnumerableForEach(ref allocator, this.converter, this.collection);
        }

        [Benchmark(Description = "Encode HashSet (foreach)")]
        public void S02()
        {
            var allocator = new Allocator(new Span<byte>(this.buffer));
            EnumerationEncoder.EncodeHashSetForEach(ref allocator, this.converter, this.collection);
        }

        [Benchmark(Description = "Encode HashSet (to array then foreach)")]
        public void S03()
        {
            var allocator = new Allocator(new Span<byte>(this.buffer));
            EnumerationEncoder.EncodeCollectionToArrayThenForEach(ref allocator, this.converter, this.collection);
        }

        [Benchmark(Description = "Encode HashSet (converter)")]
        public void S04()
        {
            var allocator = new Allocator(new Span<byte>(this.buffer));
            this.collectionConverter.Encode(ref allocator, this.collection);
        }

        [Benchmark(Description = "Encode Dictionary (foreach interface)")]
        public void D01()
        {
            var allocator = new Allocator(new Span<byte>(this.buffer));
            EnumerationEncoder.EncodeKeyValueEnumerableForEach(ref allocator, this.converter, this.converter, this.dictionary);
        }

        [Benchmark(Description = "Encode Dictionary (foreach)")]
        public void D02()
        {
            var allocator = new Allocator(new Span<byte>(this.buffer));
            EnumerationEncoder.EncodeDictionaryForEach(ref allocator, this.converter, this.converter, this.dictionary);
        }

        [Benchmark(Description = "Encode Dictionary (to array then foreach)")]
        public void D03()
        {
            var allocator = new Allocator(new Span<byte>(this.buffer));
            EnumerationEncoder.EncodeKeyValueCollectionForEach(ref allocator, this.converter, this.converter, this.dictionary);
        }

        [Benchmark(Description = "Encode Dictionary (converter)")]
        public void D04()
        {
            var allocator = new Allocator(new Span<byte>(this.buffer));
            this.dictionaryConverter.Encode(ref allocator, this.dictionary);
        }
    }
}
