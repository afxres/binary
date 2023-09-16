namespace Mikodev.Binary.Benchmarks.ListDecodeTests;

using BenchmarkDotNet.Attributes;
using Mikodev.Binary.Benchmarks.Abstractions;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

[MemoryDiagnoser]
public partial class ListDecodeBenchmarks
{
    private byte[]? bytes;

    private Converter<int> converter = null!;

    [Params(0, 4, 8, 16, 24, 32)]
    public int Length;

    [GlobalSetup]
    public void Setup()
    {
        var values = Enumerable.Range(0, this.Length).ToList();
        this.converter = new VariableNativeConverter<int>();
        var generator = Generator.CreateDefaultBuilder().AddConverter(this.converter).Build();
        this.bytes = generator.Encode(values);

        var decodeResult = Decode(this.converter, this.bytes);
        var decodeStackBasedResult = DecodeStackBased(this.converter, this.bytes);
        var decodeRecursivelyResult = DecodeRecursively(this.converter, this.bytes);

        // simple tests
        Trace.Assert(this.bytes.Length == this.Length * 5);
        Trace.Assert(values.SequenceEqual(decodeResult));
        Trace.Assert(values.SequenceEqual(decodeStackBasedResult));
        Trace.Assert(values.SequenceEqual(decodeRecursivelyResult));
    }

    [Benchmark(Description = "Decode List")]
    public List<int> M01()
    {
        return Decode(this.converter, this.bytes);
    }

    [Benchmark(Description = "Decode Stack Based")]
    public List<int> M02()
    {
        return DecodeStackBased(this.converter, this.bytes);
    }

    [Benchmark(Description = "Decode List Recursively")]
    public List<int> M03()
    {
        return DecodeRecursively(this.converter, this.bytes);
    }
}
