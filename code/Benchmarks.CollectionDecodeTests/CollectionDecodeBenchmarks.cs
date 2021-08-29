namespace Mikodev.Binary.Benchmarks.CollectionDecodeTests;

using BenchmarkDotNet.Attributes;
using Mikodev.Binary.Benchmarks.Abstractions;
using Mikodev.Binary.Benchmarks.CollectionDecodeTests.Models;
using System;
using System.Collections.Generic;
using System.Linq;

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
        this.converter = this.Flag == "constant"
            ? new ConstantNativeConverter<int>()
            : new VariableNativeConverter<int>();
        var generator = Generator.CreateDefaultBuilder().AddConverter(this.converter).Build();
        this.enumerableConverter = generator.GetConverter<IEnumerable<int>>();
        this.dataBuffer = generator.Encode(Enumerable.Range(0, 1024));
        if (typeof(T) == typeof(HashSet<int>))
            this.directiveDecoder = (Decoder<T>)(object)ReflectionMethods.GetDecoder<HashSet<int>, int>(this.converter, a => a.Add(0));
        else if (typeof(T) == typeof(SortedSet<int>))
            this.directiveDecoder = (Decoder<T>)(object)ReflectionMethods.GetDecoder<SortedSet<int>, int>(this.converter, a => a.Add(0));
        else if (typeof(T) == typeof(LinkedList<int>))
            this.directiveDecoder = (Decoder<T>)(object)ReflectionMethods.GetDecoder<LinkedList<int>, int>(this.converter, a => a.AddLast(0));
        else
            throw new NotSupportedException();
        this.interfaceDecoder = ReflectionMethods.GetDecoder<T, int>(this.converter, a => ((ICollection<int>)a).Add(0));
        this.constructor = ReflectionMethods.GetConstructor<T, int>();
    }

    [Benchmark(Description = "Decode Via Add Method")]
    public T D01()
    {
        return this.directiveDecoder.Invoke(new ReadOnlySpan<byte>(this.dataBuffer));
    }

    [Benchmark(Description = "Decode Via Interface Add Method")]
    public T I01()
    {
        return this.interfaceDecoder.Invoke(new ReadOnlySpan<byte>(this.dataBuffer));
    }

    [Benchmark(Description = "Decode Via Constructor")]
    public T C01()
    {
        return this.constructor.Invoke(this.enumerableConverter.Decode(new ReadOnlySpan<byte>(this.dataBuffer)));
    }
}
