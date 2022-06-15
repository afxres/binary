namespace Mikodev.Binary.Benchmarks.ConverterTests;

using BenchmarkDotNet.Attributes;
using Mikodev.Binary.Benchmarks.Abstractions;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

[MemoryDiagnoser]
public class ConverterBenchmarks
{
    private byte[]? buffer;

    private int number;

    private byte[]? encodeBytes;

    private byte[]? encodeAutoBytes;

    private byte[]? encodeWithLengthPrefixBytes;

    [AllowNull]
    private Converter<int> converter;

    [Params("constant", "variable", "fallback", "features")]
    public string? Flag;

    [GlobalSetup]
    public void Setup()
    {
        this.buffer = new byte[65536];
        this.number = 31415926;
        this.converter = this.Flag switch
        {
            "constant" => new ConstantNativeConverter<int>(),
            "variable" => new VariableNativeConverter<int>(),
            "features" => Generator.CreateDefault().GetConverter<int>(),
            "fallback" => (Converter<int>)Activator.CreateInstance(typeof(IConverter).Assembly.GetTypes().Single(x => x.Name is "NativeEndianConverter`1").MakeGenericType(typeof(int)))!,
            _ => throw new NotSupportedException(),
        };
        this.encodeBytes = this.converter.Encode(this.number);
        this.encodeAutoBytes = Allocator.Invoke(this.number, this.converter.EncodeAuto);
        this.encodeWithLengthPrefixBytes = Allocator.Invoke(this.number, this.converter.EncodeWithLengthPrefix);
    }

    [Benchmark(Description = "Encode")]
    public byte[] C02()
    {
        return this.converter.Encode(this.number);
    }

    [Benchmark(Description = "Encode (reuse buffer)")]
    public void C01()
    {
        var allocator = new Allocator(this.buffer);
        this.converter.Encode(ref allocator, this.number);
    }

    [Benchmark(Description = "Encode Auto (reuse buffer)")]
    public void C05()
    {
        var allocator = new Allocator(this.buffer);
        this.converter.EncodeAuto(ref allocator, this.number);
    }

    [Benchmark(Description = "Encode With Length Prefix (reuse buffer)")]
    public void C06()
    {
        var allocator = new Allocator(this.buffer);
        this.converter.EncodeWithLengthPrefix(ref allocator, this.number);
    }

    [Benchmark(Description = "Decode (by bytes)")]
    public int C04()
    {
        return this.converter.Decode(this.encodeBytes);
    }

    [Benchmark(Description = "Decode (by span)")]
    public int C03()
    {
        return this.converter.Decode(new ReadOnlySpan<byte>(this.encodeBytes));
    }

    [Benchmark(Description = "Decode Auto (by span)")]
    public int C07()
    {
        var span = new ReadOnlySpan<byte>(this.encodeAutoBytes);
        return this.converter.DecodeAuto(ref span);
    }

    [Benchmark(Description = "Decode With Length Prefix (by span)")]
    public int C08()
    {
        var span = new ReadOnlySpan<byte>(this.encodeWithLengthPrefixBytes);
        return this.converter.DecodeWithLengthPrefix(ref span);
    }
}
