using BenchmarkDotNet.Attributes;
using Mikodev.Binary.Benchmarks.Abstractions;
using Mikodev.Binary.Benchmarks.CollectionDecodeTests.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mikodev.Binary.Benchmarks.CollectionDecodeTests
{
    [MemoryDiagnoser]
    [GenericTypeArguments(typeof(HashSet<int>))]
    [GenericTypeArguments(typeof(SortedSet<int>))]
    [GenericTypeArguments(typeof(LinkedList<int>))]
    public class CollectionDecodeBenchmarks<T> where T : ICollection<int>
    {
        private Converter<int> converter;

        private Converter<IEnumerable<int>> enumerableConverter;

        private byte[] dataBuffer;

        private Decoder<T> directiveDecoder;

        private Decoder<T> interfaceDecoder;

        private Func<IEnumerable<int>, T> constructor;

        [Params("constant", "variable")]
        public string Flag;

        [GlobalSetup]
        public void Setup()
        {
            converter = this.Flag == "constant"
                ? new ConstantNativeConverter<int>()
                : new VariableNativeConverter<int>();
            var generator = Generator.CreateDefaultBuilder().AddConverter(converter).Build();
            enumerableConverter = generator.GetConverter<IEnumerable<int>>();
            dataBuffer = generator.Encode(Enumerable.Range(0, 1024));
            if (typeof(T) == typeof(HashSet<int>))
                directiveDecoder = (Decoder<T>)(object)ReflectionMethods.GetDecoder<HashSet<int>, int>(converter, a => a.Add(0));
            else if (typeof(T) == typeof(SortedSet<int>))
                directiveDecoder = (Decoder<T>)(object)ReflectionMethods.GetDecoder<SortedSet<int>, int>(converter, a => a.Add(0));
            else if (typeof(T) == typeof(LinkedList<int>))
                directiveDecoder = (Decoder<T>)(object)ReflectionMethods.GetDecoder<LinkedList<int>, int>(converter, a => a.AddLast(0));
            else
                throw new NotSupportedException();
            interfaceDecoder = ReflectionMethods.GetDecoder<T, int>(converter, a => ((ICollection<int>)a).Add(0));
            constructor = ReflectionMethods.GetConstructor<T, int>();
        }

        [Benchmark(Description = "Decode")]
        public T D01()
        {
            return interfaceDecoder.Invoke(new ReadOnlySpan<byte>(dataBuffer));
        }

        [Benchmark(Description = "Decode Via Interface")]
        public T I01()
        {
            return directiveDecoder.Invoke(new ReadOnlySpan<byte>(dataBuffer));
        }

        [Benchmark(Description = "Decode Via Constructor")]
        public T C01()
        {
            return constructor.Invoke(enumerableConverter.Decode(new ReadOnlySpan<byte>(dataBuffer)));
        }
    }
}
