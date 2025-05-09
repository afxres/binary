namespace Mikodev.Binary.Benchmarks.ListDecodeTests;

using BenchmarkDotNet.Attributes;
using Mikodev.Binary.Benchmarks.Abstractions;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

[MemoryDiagnoser]
public partial class ListDecodeBenchmarks
{
    private const int FallbackCapacity = 8;

    private byte[]? bytes;

    private Converter<int> converter = null!;

    private Converter<int[]> converterArray = null!;

    private Converter<List<int>> converterList = null!;

    [Params(0, 1, 16, 32, 64, 128)]
    public int Length;

    [GlobalSetup]
    public void Setup()
    {
        var values = Enumerable.Range(0, this.Length).ToList();
        this.converter = new VariableNativeConverter<int>();
        this.converterList = Generator.GetListConverter(this.converter);
        this.converterArray = Generator.GetArrayConverter(this.converter);
        this.bytes = this.converterList.Encode(values);

        var decodeKnowCapacityResult = Decode(this.converter, this.bytes, this.Length);
        var decodeUnknownCapacityResult = Decode(this.converter, this.bytes, FallbackCapacity);
        var decodeRecursivelyResult = DecodeRecursively(this.converter, this.bytes);
        var decodeViaConverterResult = this.converterList.Decode(this.bytes);
        var decodeViaArrayConverterResult = this.converterArray.Decode(this.bytes);

        // simple tests
        Trace.Assert(this.bytes.Length == this.Length * 5);
        Trace.Assert(values.SequenceEqual(decodeKnowCapacityResult));
        Trace.Assert(values.SequenceEqual(decodeUnknownCapacityResult));
        Trace.Assert(values.SequenceEqual(decodeRecursivelyResult));
        Trace.Assert(values.SequenceEqual(decodeViaConverterResult));
        Trace.Assert(values.SequenceEqual(decodeViaArrayConverterResult));
    }

    [Benchmark(Description = "Decode List Known Capacity")]
    public List<int> M00()
    {
        return Decode(this.converter, this.bytes, this.Length);
    }

    [Benchmark(Description = "Decode List Unknown Capacity")]
    public List<int> M01()
    {
        return Decode(this.converter, this.bytes, FallbackCapacity);
    }

    [Benchmark(Description = "Decode List Recursively")]
    public List<int> M03()
    {
        return DecodeRecursively(this.converter, this.bytes);
    }

    [Benchmark(Description = "Decode List Current Implementation")]
    public List<int> M04()
    {
        return this.converterList.Decode(this.bytes);
    }

    [Benchmark(Description = "Decode Array Current Implementation")]
    public int[] M05()
    {
        return this.converterArray.Decode(this.bytes);
    }
}
