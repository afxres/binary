namespace Mikodev.Binary.Benchmarks.LengthPrefixEncodeTests;

using BenchmarkDotNet.Attributes;
using Mikodev.Binary.Benchmarks.LengthPrefixEncodeTests.Models;
using System;

public class LengthPrefixEncodeBenchmarks
{
    private byte[] buffer;

    private Converter<int> converter;

    [Params(0, 4, 8, 12, 16, 32)]
    public int Count;

    [GlobalSetup]
    public void Setup()
    {
        this.buffer = new byte[1024];
        this.converter = new EmptyBytesConverter();
    }

    [Benchmark(Description = "Encode (reuse buffer)")]
    public void E01()
    {
        var allocator = new Allocator(new Span<byte>(this.buffer));
        this.converter.Encode(ref allocator, this.Count);
    }

    [Benchmark(Description = "Encode With Length Prefix (reuse buffer)")]
    public void E02()
    {
        var allocator = new Allocator(new Span<byte>(this.buffer));
        this.converter.EncodeWithLengthPrefix(ref allocator, this.Count);
    }
}
