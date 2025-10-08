namespace Mikodev.Binary.Tests.Creators;

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using Xunit;

public class ReadOnlySequenceConverterTests
{
    [Fact(DisplayName = "Converter Type Name And Length")]
    public void BasicTest()
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<ReadOnlySequence<int>>();
        Assert.Equal(0, converter.Length);
        Assert.Equal("ReadOnlySequenceConverter`1", converter.GetType().Name);
    }

    public static IEnumerable<object[]> EmptySequenceData()
    {
        yield return [ReadOnlySequence<int>.Empty];
        yield return [ReadOnlySequence<string>.Empty];
    }

    [Theory(DisplayName = "Empty Or Default Test")]
    [MemberData(nameof(EmptySequenceData))]
    public void EmptyOrDefaultTest<E>(ReadOnlySequence<E> source)
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<ReadOnlySequence<E>>();
        Assert.True(source.IsEmpty);
        var bufferDefault = converter.Encode(default);
        Assert.Empty(bufferDefault);
        var bufferEmpty = converter.Encode(source);
        Assert.Empty(bufferEmpty);
        var result = converter.Decode(Array.Empty<byte>());
        Assert.True(result.IsEmpty);
        Assert.True(result.IsSingleSegment);
        Assert.Equal(0, result.Length);
    }

    public static IEnumerable<object[]> SingleSegmentData()
    {
        var alpha = Enumerable.Range(0, 100).ToArray();
        var bravo = Enumerable.Range(0, 100).Select(x => x.ToString()).ToArray();
        yield return [new ReadOnlySequence<int>(alpha)];
        yield return [new ReadOnlySequence<int>(alpha, 33, 34)];
        yield return [new ReadOnlySequence<int>(alpha, 66, 0)];
        yield return [new ReadOnlySequence<string>(bravo)];
        yield return [new ReadOnlySequence<string>(bravo, 43, 21)];
        yield return [new ReadOnlySequence<string>(bravo, 55, 0)];
    }

    [Theory(DisplayName = "Single Segment Test")]
    [MemberData(nameof(SingleSegmentData))]
    public void SingleSegmentTest<E>(ReadOnlySequence<E> source)
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<ReadOnlySequence<E>>();
        Assert.True(source.IsSingleSegment);
        var buffer = converter.Encode(source);
        var result = converter.Decode(buffer);
        Assert.True(result.IsSingleSegment);
        Assert.Equal(source.Length, result.Length);
        Assert.Equal(source.FirstSpan.ToArray(), result.ToArray());
    }

    private sealed class MemorySegment<E> : ReadOnlySequenceSegment<E>
    {
        public MemorySegment(ReadOnlyMemory<E> memory)
        {
            Memory = memory;
        }

        public MemorySegment<E> Append(ReadOnlyMemory<E> memory)
        {
            var result = new MemorySegment<E>(memory) { RunningIndex = RunningIndex + Memory.Length };
            Next = result;
            return result;
        }
    }

    public static IEnumerable<object[]> MultipleSegmentData()
    {
        static Memory<E> Invoke<E>(Func<int, E> func, int total, int start, int count)
        {
            return new Memory<E>(Enumerable.Range(0, total).Select(func).ToArray(), start, count);
        }

        var alphaStart = new MemorySegment<int>(Invoke(x => x, 40, 2, 30));
        var alphaEnd = alphaStart
            .Append(Invoke(x => x, 100, 0, 100))
            .Append(Invoke(x => x, 50, 25, 0))
            .Append(Invoke(x => x, 100, 45, 50));
        yield return [new ReadOnlySequence<int>(alphaStart, 0, alphaEnd, alphaEnd.Memory.Length)];
        yield return [new ReadOnlySequence<int>(alphaStart, 11, alphaEnd, 25)];

        var bravoStart = new MemorySegment<string>(Invoke(x => x.ToString(), 80, 12, 66));
        var bravoEnd = bravoStart
            .Append(Invoke(x => x.ToString(), 44, 0, 44))
            .Append(Invoke(x => x.ToString(), 30, 16, 0))
            .Append(Invoke(x => x.ToString(), 100, 24, 70));
        yield return [new ReadOnlySequence<string>(bravoStart, 0, bravoEnd, bravoEnd.Memory.Length)];
        yield return [new ReadOnlySequence<string>(bravoStart, 22, bravoEnd, 54)];
    }

    [Theory(DisplayName = "Multiple Segment Test")]
    [MemberData(nameof(MultipleSegmentData))]
    public void MultipleSegmentTest<E>(ReadOnlySequence<E> source)
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<ReadOnlySequence<E>>();
        Assert.False(source.IsSingleSegment);
        var buffer = converter.Encode(source);
        var result = converter.Decode(buffer);
        Assert.True(result.IsSingleSegment);
        Assert.Equal(source.Length, result.Length);
        Assert.Equal(source.ToArray(), result.ToArray());
    }
}
