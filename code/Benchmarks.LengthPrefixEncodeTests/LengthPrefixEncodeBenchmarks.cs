namespace Mikodev.Binary.Benchmarks.LengthPrefixEncodeTests;

using BenchmarkDotNet.Attributes;
using Mikodev.Binary.Benchmarks.LengthPrefixEncodeTests.Models;
using System;

public class LengthPrefixEncodeBenchmarks
{
    private byte[]? buffer;

    private Converter<int> constantConverter = null!;

    private Converter<int> variableConverter = null!;

    [GlobalSetup]
    public void Setup()
    {
        this.buffer = new byte[1024];
        this.constantConverter = new EmptyBytesConverter(4);
        this.variableConverter = new EmptyBytesConverter(0);
    }

    [Benchmark(Description = "Encode (constant)")]
    public void C01()
    {
        var allocator = new Allocator(new Span<byte>(this.buffer));
        this.constantConverter.Encode(ref allocator, this.constantConverter.Length);
    }

    [Benchmark(Description = "Encode With Length Prefix (constant)")]
    public void C02()
    {
        var allocator = new Allocator(new Span<byte>(this.buffer));
        this.constantConverter.EncodeWithLengthPrefix(ref allocator, this.constantConverter.Length);
    }

    [Benchmark(Description = "Encode (variable)")]
    [Arguments(0)]
    [Arguments(1)]
    [Arguments(16)]
    [Arguments(128)]
    public void V01(int length)
    {
        var allocator = new Allocator(new Span<byte>(this.buffer));
        this.variableConverter.Encode(ref allocator, length);
    }

    [Benchmark(Description = "Encode With Length Prefix (variable)")]
    [Arguments(0)]
    [Arguments(1)]
    [Arguments(16)]
    [Arguments(128)]
    public void V02(int length)
    {
        var allocator = new Allocator(new Span<byte>(this.buffer));
        this.variableConverter.EncodeWithLengthPrefix(ref allocator, length);
    }
}
