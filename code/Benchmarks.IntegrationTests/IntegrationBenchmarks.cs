namespace Mikodev.Binary.Benchmarks.IntegrationTests;

using BenchmarkDotNet.Attributes;
using Mikodev.Binary.Benchmarks.IntegrationTests.Models;
using System.Collections.Generic;
using TypeX1 = System.ValueTuple<int, string, int[], System.ValueTuple<double, System.Collections.Generic.List<string>>>;

[MemoryDiagnoser]
public class IntegrationBenchmarks
{
    private IGenerator generator;

    private byte[] buffer;

    private Type01 value;

    private byte[] valueBytes;

    private Converter<Type01> valueConverter;

    private TypeX1 tuple;

    private byte[] tupleBytes;

    private Converter<TypeX1> tupleConverter;

    [GlobalSetup]
    public void Setup()
    {
        this.generator = Generator.CreateDefault();
        this.buffer = new byte[65536];

        this.value = new Type01
        {
            Id = 1024,
            Name = "csharp",
            List = new[] { 7, 11, 555, 1313 },
            Item = new Type02
            {
                Data = 2.2D,
                Tags = new List<string> { "one", "two", "three" },
            }
        };
        this.tuple = (1024, "csharp", new[] { 7, 11, 555, 1313 }, (2.2D, new List<string> { "one", "two", "three" }));

        this.valueBytes = this.generator.Encode(this.value);
        this.tupleBytes = this.generator.Encode(this.tuple);
        this.valueConverter = this.generator.GetConverter<Type01>();
        this.tupleConverter = this.generator.GetConverter<TypeX1>();
    }

    [Benchmark(Description = "Encode Named Object (use generator)")]
    public byte[] B01()
    {
        return this.generator.Encode(this.value);
    }

    [Benchmark(Description = "Encode Named Object (use converter, reuse buffer)")]
    public void B02()
    {
        var allocator = new Allocator(this.buffer);
        this.valueConverter.Encode(ref allocator, this.value);
    }

    [Benchmark(Description = "Encode Tuple Object (use converter, reuse buffer)")]
    public void B03()
    {
        var allocator = new Allocator(this.buffer);
        this.tupleConverter.Encode(ref allocator, this.tuple);
    }

    [Benchmark(Description = "Decode Named Object (use generator)")]
    public Type01 B04()
    {
        return this.generator.Decode<Type01>(this.valueBytes);
    }

    [Benchmark(Description = "Decode Named Object (use converter)")]
    public Type01 B05()
    {
        return this.valueConverter.Decode(this.valueBytes);
    }

    [Benchmark(Description = "Decode Tuple Object (use converter)")]
    public TypeX1 B06()
    {
        return this.tupleConverter.Decode(this.tupleBytes);
    }
}
