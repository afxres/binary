namespace Mikodev.Binary.Benchmarks.IntegrationTests;

using BenchmarkDotNet.Attributes;
using Mikodev.Binary.Attributes;
using Mikodev.Binary.Benchmarks.IntegrationTests.Models;
using System.Collections.Generic;
using System.Diagnostics;
using TypeX1 = System.ValueTuple<int, string, int[], System.ValueTuple<double, System.Collections.Generic.List<string>>>;

[SourceGeneratorContext]
[SourceGeneratorInclude<Type01>]
[SourceGeneratorInclude<TypeX1>]
public partial class IntegrationGeneratorContext { }

[MemoryDiagnoser]
public class IntegrationBenchmarks
{
    private IGenerator generatorJit = null!;

    private IGenerator generatorAot = null!;

    private byte[]? buffer;

    private Type01? value;

    private byte[]? valueBytes;

    private Converter<Type01> valueConverterJit = null!;

    private Converter<Type01> valueConverterAot = null!;

    private TypeX1 tuple;

    private byte[]? tupleBytes;

    private Converter<TypeX1> tupleConverterJit = null!;

    private Converter<TypeX1> tupleConverterAot = null!;

    [GlobalSetup]
    public void Setup()
    {
        this.buffer = new byte[65536];

        this.value = new Type01
        {
            Id = 1024,
            Name = "csharp",
            List = [7, 11, 555, 1313],
            Item = new Type02
            {
                Data = 2.2D,
                Tags = ["one", "two", "three"],
            }
        };
        this.tuple = (1024, "csharp", new[] { 7, 11, 555, 1313 }, (2.2D, new List<string> { "one", "two", "three" }));

        this.generatorJit = Generator.CreateDefault();
        this.valueConverterJit = this.generatorJit.GetConverter<Type01>();
        this.tupleConverterJit = this.generatorJit.GetConverter<TypeX1>();
        this.valueBytes = this.valueConverterJit.Encode(this.value);
        this.tupleBytes = this.tupleConverterJit.Encode(this.tuple);

        this.generatorAot = Generator.CreateAotBuilder().AddConverterCreators(IntegrationGeneratorContext.ConverterCreators.Values).Build();
        this.valueConverterAot = this.generatorAot.GetConverter<Type01>();
        this.tupleConverterAot = this.generatorAot.GetConverter<TypeX1>();

        var valueBytesAot = this.valueConverterAot.Encode(this.value);
        var tupleBytesAot = this.tupleConverterAot.Encode(this.tuple);
        var valueResultJit = this.valueConverterJit.Decode(valueBytesAot);
        var tupleResultJit = this.tupleConverterJit.Decode(tupleBytesAot);
        var valueResultAot = this.valueConverterAot.Decode(this.valueBytes);
        var tupleResultAot = this.tupleConverterAot.Decode(this.tupleBytes);

        // simple tests
        Trace.Assert(valueResultJit.Id is 1024);
        Trace.Assert(tupleResultJit.Item3[1] is 11);
        Trace.Assert(valueResultAot.Item?.Tags?[0] is "one");
        Trace.Assert(tupleResultAot.Item2 is "csharp");
    }

    [Benchmark(Description = "Encode Named Object (jit, generator)")]
    public byte[] B01()
    {
        return this.generatorJit.Encode(this.value);
    }

    [Benchmark(Description = "Encode Named Object (jit)")]
    public void B02()
    {
        var allocator = new Allocator(this.buffer);
        this.valueConverterJit.Encode(ref allocator, this.value);
    }

    [Benchmark(Description = "Encode Tuple Object (jit)")]
    public void B03()
    {
        var allocator = new Allocator(this.buffer);
        this.tupleConverterJit.Encode(ref allocator, this.tuple);
    }

    [Benchmark(Description = "Encode Named Object (aot)")]
    public void A02()
    {
        var allocator = new Allocator(this.buffer);
        this.valueConverterAot.Encode(ref allocator, this.value);
    }

    [Benchmark(Description = "Encode Tuple Object (aot)")]
    public void A03()
    {
        var allocator = new Allocator(this.buffer);
        this.tupleConverterAot.Encode(ref allocator, this.tuple);
    }

    [Benchmark(Description = "Decode Named Object (jit, generator)")]
    public Type01 B04()
    {
        return this.generatorJit.Decode<Type01>(this.valueBytes);
    }

    [Benchmark(Description = "Decode Named Object (jit)")]
    public Type01 B05()
    {
        return this.valueConverterJit.Decode(this.valueBytes);
    }

    [Benchmark(Description = "Decode Tuple Object (jit)")]
    public TypeX1 B06()
    {
        return this.tupleConverterJit.Decode(this.tupleBytes);
    }

    [Benchmark(Description = "Decode Named Object (aot)")]
    public Type01 A05()
    {
        return this.valueConverterAot.Decode(this.valueBytes);
    }

    [Benchmark(Description = "Decode Tuple Object (aot)")]
    public TypeX1 A06()
    {
        return this.tupleConverterAot.Decode(this.tupleBytes);
    }
}
