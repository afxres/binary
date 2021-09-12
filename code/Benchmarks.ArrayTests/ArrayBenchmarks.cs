namespace Mikodev.Binary.Benchmarks.ArrayTests;

using BenchmarkDotNet.Attributes;
using Mikodev.Binary.Benchmarks.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

[MemoryDiagnoser]
public class ArrayBenchmarks
{
    private byte[]? buffer;

    private byte[]? encodeBytes;

    private byte[]? encodeWithLengthPrefixBytes;

    private int[]? array01;

    private List<int>? list01;

    private Memory<int> memory01;

    [AllowNull]
    private Converter<int[]> arrayConverter;

    [AllowNull]
    private Converter<List<int>> listConverter;

    [AllowNull]
    private Converter<Memory<int>> memoryConverter;

    [Params("constant", "variable")]
    public string? Flag;

    [GlobalSetup]
    public void Setup()
    {
        var converter = this.Flag == "constant"
            ? new ConstantNativeConverter<int>()
            : new VariableNativeConverter<int>() as Converter<int>;
        var generator = Generator.CreateDefaultBuilder().AddConverter(converter).Build();

        this.array01 = new[] { 1313 };
        this.list01 = new List<int> { 1313 };
        this.memory01 = new Memory<int>(new int[] { 1313 });

        this.arrayConverter = generator.GetConverter<int[]>();
        this.listConverter = generator.GetConverter<List<int>>();
        this.memoryConverter = generator.GetConverter<Memory<int>>();

        this.buffer = new byte[65536];
        this.encodeBytes = this.arrayConverter.Encode(this.array01);
        this.encodeWithLengthPrefixBytes = Allocator.Invoke(this.array01, this.arrayConverter.EncodeWithLengthPrefix);
    }

    [Benchmark(Description = "Encode Array")]
    public void A01()
    {
        var allocator = new Allocator(this.buffer);
        this.arrayConverter.Encode(ref allocator, this.array01);
    }

    [Benchmark(Description = "Encode List")]
    public void L01()
    {
        var allocator = new Allocator(this.buffer);
        this.listConverter.Encode(ref allocator, this.list01);
    }

    [Benchmark(Description = "Encode Memory")]
    public void M01()
    {
        var allocator = new Allocator(this.buffer);
        this.memoryConverter.Encode(ref allocator, this.memory01);
    }

    [Benchmark(Description = "Encode Array With Length Prefix")]
    public void A02()
    {
        var allocator = new Allocator(this.buffer);
        this.arrayConverter.EncodeWithLengthPrefix(ref allocator, this.array01);
    }

    [Benchmark(Description = "Encode List With Length Prefix")]
    public void L02()
    {
        var allocator = new Allocator(this.buffer);
        this.listConverter.EncodeWithLengthPrefix(ref allocator, this.list01);
    }

    [Benchmark(Description = "Encode Memory With Length Prefix")]
    public void M02()
    {
        var allocator = new Allocator(this.buffer);
        this.memoryConverter.EncodeWithLengthPrefix(ref allocator, this.memory01);
    }

    [Benchmark(Description = "Decode Array")]
    public int[] A03()
    {
        var span = new ReadOnlySpan<byte>(this.encodeBytes);
        return this.arrayConverter.Decode(in span);
    }

    [Benchmark(Description = "Decode List")]
    public List<int> L03()
    {
        var span = new ReadOnlySpan<byte>(this.encodeBytes);
        return this.listConverter.Decode(in span);
    }

    [Benchmark(Description = "Decode Memory")]
    public Memory<int> M03()
    {
        var span = new ReadOnlySpan<byte>(this.encodeBytes);
        return this.memoryConverter.Decode(in span);
    }

    [Benchmark(Description = "Decode Array With Length Prefix")]
    public int[] A04()
    {
        var span = new ReadOnlySpan<byte>(this.encodeWithLengthPrefixBytes);
        return this.arrayConverter.DecodeWithLengthPrefix(ref span);
    }

    [Benchmark(Description = "Decode List With Length Prefix")]
    public List<int> L04()
    {
        var span = new ReadOnlySpan<byte>(this.encodeWithLengthPrefixBytes);
        return this.listConverter.DecodeWithLengthPrefix(ref span);
    }

    [Benchmark(Description = "Decode Memory With Length Prefix")]
    public Memory<int> M04()
    {
        var span = new ReadOnlySpan<byte>(this.encodeWithLengthPrefixBytes);
        return this.memoryConverter.DecodeWithLengthPrefix(ref span);
    }
}
