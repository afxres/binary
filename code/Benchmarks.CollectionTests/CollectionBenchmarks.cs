using BenchmarkDotNet.Attributes;
using Mikodev.Binary.Benchmarks.Abstractions;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Benchmarks.CollectionTests
{
    [MemoryDiagnoser]
    public class CollectionBenchmarks
    {
        private byte[] buffer;

        private byte[] encodeBytes;

        private byte[] encodeWithLengthPrefixBytes;

        private byte[] encodeBytesOfPair;

        private byte[] encodeWithLengthPrefixBytesOfPair;

        private HashSet<int> hashSet;

        private LinkedList<int> linkedList;

        private Dictionary<int, int> dictionary;

        private Converter<HashSet<int>> hashSetConverter;

        private Converter<LinkedList<int>> linkedListConverter;

        private Converter<Dictionary<int, int>> dictionaryConverter;

        [Params("constant", "variable")]
        public string Flag;

        [GlobalSetup]
        public void Setup()
        {
            var converter = this.Flag == "constant"
                ? new ConstantNativeConverter<int>()
                : new VariableNativeConverter<int>() as Converter<int>;
            var generator = Generator.CreateDefaultBuilder().AddConverter(converter).Build();

            this.hashSet = new HashSet<int> { 1313 };
            this.linkedList = new LinkedList<int>(new[] { 1313 });
            this.dictionary = new Dictionary<int, int> { [1313] = 1313 };

            this.hashSetConverter = generator.GetConverter<HashSet<int>>();
            this.linkedListConverter = generator.GetConverter<LinkedList<int>>();
            this.dictionaryConverter = generator.GetConverter<Dictionary<int, int>>();

            this.buffer = new byte[65536];
            this.encodeBytes = this.linkedListConverter.Encode(this.linkedList);
            this.encodeWithLengthPrefixBytes = AllocatorHelper.Invoke(this.linkedList, this.linkedListConverter.EncodeWithLengthPrefix);
            this.encodeBytesOfPair = this.dictionaryConverter.Encode(this.dictionary);
            this.encodeWithLengthPrefixBytesOfPair = AllocatorHelper.Invoke(this.dictionary, this.dictionaryConverter.EncodeWithLengthPrefix);
        }

        [Benchmark(Description = "Encode HashSet")]
        public void S01()
        {
            var allocator = new Allocator(this.buffer);
            this.hashSetConverter.Encode(ref allocator, this.hashSet);
        }

        [Benchmark(Description = "Encode LinkedList")]
        public void L01()
        {
            var allocator = new Allocator(this.buffer);
            this.linkedListConverter.Encode(ref allocator, this.linkedList);
        }

        [Benchmark(Description = "Encode Dictionary")]
        public void M01()
        {
            var allocator = new Allocator(this.buffer);
            this.dictionaryConverter.Encode(ref allocator, this.dictionary);
        }

        [Benchmark(Description = "Encode HashSet With Length Prefix")]
        public void S02()
        {
            var allocator = new Allocator(this.buffer);
            this.hashSetConverter.EncodeWithLengthPrefix(ref allocator, this.hashSet);
        }

        [Benchmark(Description = "Encode LinkedList With Length Prefix")]
        public void L02()
        {
            var allocator = new Allocator(this.buffer);
            this.linkedListConverter.EncodeWithLengthPrefix(ref allocator, this.linkedList);
        }

        [Benchmark(Description = "Encode Dictionary With Length Prefix")]
        public void M02()
        {
            var allocator = new Allocator(this.buffer);
            this.dictionaryConverter.EncodeWithLengthPrefix(ref allocator, this.dictionary);
        }

        [Benchmark(Description = "Decode HashSet")]
        public HashSet<int> S03()
        {
            var span = new ReadOnlySpan<byte>(this.encodeBytes);
            return this.hashSetConverter.Decode(in span);
        }

        [Benchmark(Description = "Decode LinkedList")]
        public LinkedList<int> L03()
        {
            var span = new ReadOnlySpan<byte>(this.encodeBytes);
            return this.linkedListConverter.Decode(in span);
        }

        [Benchmark(Description = "Decode Dictionary")]
        public Dictionary<int, int> M03()
        {
            var span = new ReadOnlySpan<byte>(this.encodeBytesOfPair);
            return this.dictionaryConverter.Decode(in span);
        }

        [Benchmark(Description = "Decode HashSet With Length Prefix")]
        public HashSet<int> S04()
        {
            var span = new ReadOnlySpan<byte>(this.encodeWithLengthPrefixBytes);
            return this.hashSetConverter.DecodeWithLengthPrefix(ref span);
        }

        [Benchmark(Description = "Decode LinkedList With Length Prefix")]
        public LinkedList<int> L04()
        {
            var span = new ReadOnlySpan<byte>(this.encodeWithLengthPrefixBytes);
            return this.linkedListConverter.DecodeWithLengthPrefix(ref span);
        }

        [Benchmark(Description = "Decode Dictionary With Length Prefix")]
        public Dictionary<int, int> M04()
        {
            var span = new ReadOnlySpan<byte>(this.encodeWithLengthPrefixBytesOfPair);
            return this.dictionaryConverter.DecodeWithLengthPrefix(ref span);
        }
    }
}
