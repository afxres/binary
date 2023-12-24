namespace Mikodev.Binary.Benchmarks.ArrayTests;

using BenchmarkDotNet.Attributes;
using Mikodev.Binary.Benchmarks.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

[MemoryDiagnoser]
public class ArrayBenchmarks
{
    private byte[]? buffer;

    private byte[]? encodeBytes;

    private byte[]? encodeWithLengthPrefixBytes;

    private int[]? array01;

    private List<int>? list01;

    private Memory<int> memory01;

    private Converter<int[]> arrayConverter = null!;

    private Converter<List<int>> listConverter = null!;

    private Converter<Memory<int>> memoryConverter = null!;

    [Params("constant", "variable", "internal")]
    public string? Flag;

    [GlobalSetup]
    public void Setup()
    {
        static IEnumerable<IConverter> Select(string? flag)
        {
            if (flag is "constant")
                yield return new ConstantNativeConverter<int>();
            else if (flag is "variable")
                yield return new VariableNativeConverter<int>();
        }

        var generator = Generator.CreateDefaultBuilder()
            .AddConverters(Select(this.Flag))
            .Build();

        this.array01 = [1313];
        this.list01 = [1313];
        this.memory01 = new Memory<int>([1313]);

        this.arrayConverter = generator.GetConverter<int[]>();
        this.listConverter = generator.GetConverter<List<int>>();
        this.memoryConverter = generator.GetConverter<Memory<int>>();

        this.buffer = new byte[65536];
        this.encodeBytes = this.arrayConverter.Encode(this.array01);
        this.encodeWithLengthPrefixBytes = Allocator.Invoke(this.array01, this.arrayConverter.EncodeWithLengthPrefix);

        var arrayResult = this.arrayConverter.Decode(this.encodeBytes);
        var listResult = this.listConverter.Decode(this.encodeBytes);
        var memoryResult = this.memoryConverter.Decode(this.encodeBytes);

        // simple tests
        Trace.Assert(arrayResult.Single() is 1313);
        Trace.Assert(listResult.Single() is 1313);
        Trace.Assert(memoryResult.ToArray().Single() is 1313);
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
