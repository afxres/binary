namespace Mikodev.Binary.Benchmarks.ObjectTests;

using BenchmarkDotNet.Attributes;
using Mikodev.Binary.Benchmarks.Abstractions;

[MemoryDiagnoser]
public class ObjectBenchmarks
{
    private byte[] buffer;

    private ClassNamedObject<int> class01;

    private byte[] classBytes;

    private Converter<ClassNamedObject<int>> classConverter;

    private ValueNamedObject<int> value01;

    private byte[] valueBytes;

    private Converter<ValueNamedObject<int>> valueConverter;

    [GlobalSetup]
    public void Setup()
    {
        var generator = Generator.CreateDefault();
        this.buffer = new byte[65536];

        this.class01 = new ClassNamedObject<int> { Item1 = 1024 };
        this.value01 = new ValueNamedObject<int> { Item1 = 1024 };

        this.classBytes = generator.Encode(this.class01);
        this.valueBytes = generator.Encode(this.value01);
        this.classConverter = generator.GetConverter<ClassNamedObject<int>>();
        this.valueConverter = generator.GetConverter<ValueNamedObject<int>>();
    }

    [Benchmark(Description = "Encode Class Object")]
    public void C01()
    {
        var allocator = new Allocator(this.buffer);
        this.classConverter.Encode(ref allocator, this.class01);
    }

    [Benchmark(Description = "Encode Value Object")]
    public void V01()
    {
        var allocator = new Allocator(this.buffer);
        this.valueConverter.Encode(ref allocator, this.value01);
    }

    [Benchmark(Description = "Decode Class Object")]
    public ClassNamedObject<int> C02()
    {
        return this.classConverter.Decode(this.classBytes);
    }

    [Benchmark(Description = "Decode Value Object")]
    public ValueNamedObject<int> V02()
    {
        return this.valueConverter.Decode(this.valueBytes);
    }
}
