namespace Mikodev.Binary.Benchmarks.AllocatorStringTests;

using BenchmarkDotNet.Attributes;
using System.Diagnostics.CodeAnalysis;
using System.Text;

[MemoryDiagnoser]
public class AllocatorStringBenchmarks
{
    [AllowNull]
    private byte[] buffer;

    [AllowNull]
    private Encoding encoding;

    [AllowNull]
    private AllocatorWriter<string?> writer;

    [AllowNull]
    [Params("", "Hello, world!", "The quick brown fox jumps over the lazy dog.")]
    public string Text;

    [GlobalSetup]
    public void Setup()
    {
        this.buffer = new byte[1024];
        this.encoding = Encoding.UTF8;
        this.writer = new AllocatorWriter<string?>((span, text) => this.encoding.GetBytes(text, span));
    }

    [Benchmark(Description = "Append")]
    public void A01()
    {
        var allocator = new Allocator(this.buffer);
        Allocator.Append(ref allocator, this.Text, this.encoding);
    }

    [Benchmark(Description = "Append With Length Prefix")]
    public void A02()
    {
        var allocator = new Allocator(this.buffer);
        Allocator.AppendWithLengthPrefix(ref allocator, this.Text, this.encoding);
    }

    [Benchmark(Description = "Writer")]
    public void W01()
    {
        var allocator = new Allocator(this.buffer);
        var text = this.Text;
        var encoding = this.encoding;
        var maxLength = encoding.GetMaxByteCount(text.Length);
        Allocator.Append(ref allocator, maxLength, text, this.writer);
    }

    [Benchmark(Description = "Writer With Length Prefix")]
    public void W02()
    {
        var allocator = new Allocator(this.buffer);
        var text = this.Text;
        var encoding = this.encoding;
        var maxLength = encoding.GetMaxByteCount(text.Length);
        Allocator.AppendWithLengthPrefix(ref allocator, maxLength, text, this.writer);
    }
}
