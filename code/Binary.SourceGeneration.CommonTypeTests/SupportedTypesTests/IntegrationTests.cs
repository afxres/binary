namespace Mikodev.Binary.SourceGeneration.CommonTypeTests.SupportedTypesTests;

using Mikodev.Binary.Attributes;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Xunit;

[SourceGeneratorContext]
[SourceGeneratorInclude<DayOfWeek>]
[SourceGeneratorInclude<ConsoleColor>]
[SourceGeneratorInclude<int?>]
[SourceGeneratorInclude<ConsoleKey?>]
[SourceGeneratorInclude<KeyValuePair<int, string>>]
[SourceGeneratorInclude<KeyValuePair<string, int>>]
[SourceGeneratorInclude<int[]>]
[SourceGeneratorInclude<string[]>]
[SourceGeneratorInclude<int[,]>]
[SourceGeneratorInclude<string[,,]>]
[SourceGeneratorInclude<ArraySegment<int>>]
[SourceGeneratorInclude<ArraySegment<string>>]
[SourceGeneratorInclude<Memory<int>>]
[SourceGeneratorInclude<Memory<string>>]
[SourceGeneratorInclude<ReadOnlyMemory<int>>]
[SourceGeneratorInclude<ReadOnlyMemory<string>>]
[SourceGeneratorInclude<ReadOnlySequence<int>>]
[SourceGeneratorInclude<ReadOnlySequence<string>>]
[SourceGeneratorInclude<PriorityQueue<int, long>>]
[SourceGeneratorInclude<PriorityQueue<string, int>>]
public partial class IntegrationGeneratorContext { }

public class IntegrationTests
{
    private static StrongBox<T> Box<T>(T data) => new StrongBox<T>(data);

    private static readonly string BimodalArrayBasedStringConverterName = BitConverter.IsLittleEndian ? "ArrayBasedNativeEndianConverter`3" : "ArrayBasedConverter`3";

    public static IEnumerable<object[]> EnumData()
    {
        yield return [DayOfWeek.Sunday, "LittleEndianConverter`1.*DayOfWeek"];
        yield return [ConsoleColor.White, "LittleEndianConverter`1.*ConsoleColor"];
    }

    public static IEnumerable<object[]> KeyValuePairData()
    {
        yield return [KeyValuePair.Create(1, "2"), "KeyValuePairConverter`2.*Int32.*String"];
        yield return [KeyValuePair.Create("3", 4), "KeyValuePairConverter`2.*String.*Int32"];
    }

    public static IEnumerable<object[]> VariableBoundArrayData()
    {
        yield return [new int[1, 1] { { 1 } }, @"VariableBoundArrayConverter`2.*Int32\[,\].*Int32"];
        yield return [new string[1, 1, 1] { { { "2" } } }, @"VariableBoundArrayConverter`2.*String\[,,\].*String"];
    }

    public static IEnumerable<object[]> ArrayData()
    {
        yield return [new[] { 1 }, BimodalArrayBasedStringConverterName + @".*Int32\[\]"];
        yield return [new[] { "2" }, @"ArrayBasedConverter`3.*String\[\]"];
    }

    public static IEnumerable<object[]> ArraySegmentData()
    {
        yield return [new ArraySegment<int>([1]), BimodalArrayBasedStringConverterName + ".*ArraySegment`1.*Int32"];
        yield return [new ArraySegment<string>(["2"]), "ArrayBasedConverter`3.*ArraySegment`1.*String"];
    }

    public static IEnumerable<object[]> MemoryData()
    {
        yield return [new Memory<int>([3]), BimodalArrayBasedStringConverterName + ".*Memory`1.*Int32"];
        yield return [new Memory<string>(["4"]), "ArrayBasedConverter`3.*Memory`1.*String"];
    }

    public static IEnumerable<object[]> ReadOnlyMemoryData()
    {
        yield return [new ReadOnlyMemory<int>([5]), BimodalArrayBasedStringConverterName + ".*ReadOnlyMemory`1.*Int32"];
        yield return [new ReadOnlyMemory<string>(["6"]), "ArrayBasedConverter`3.*ReadOnlyMemory`1.*String"];
    }

    public static IEnumerable<object[]> ReadOnlySequenceData()
    {
        yield return [new ReadOnlySequence<int>([1]), "ReadOnlySequenceConverter`1.*Int32"];
        yield return [new ReadOnlySequence<string>(["2"]), "ReadOnlySequenceConverter`1.*String"];
    }

    public static IEnumerable<object[]> PriorityQueueData()
    {
        yield return [new PriorityQueue<int, long>([(1, 2L)]), "PriorityQueueConverter`2.*Int32.*Int64"];
        yield return [new PriorityQueue<string, int>([("3", 4)]), "PriorityQueueConverter`2.*String.*Int32"];
    }

    [Theory(DisplayName = "Get Converter Test")]
    [MemberData(nameof(EnumData))]
    [MemberData(nameof(KeyValuePairData))]
    [MemberData(nameof(VariableBoundArrayData))]
    [MemberData(nameof(ArrayData))]
    [MemberData(nameof(ArraySegmentData))]
    [MemberData(nameof(MemoryData))]
    [MemberData(nameof(ReadOnlyMemoryData))]
    [MemberData(nameof(ReadOnlySequenceData))]
    [MemberData(nameof(PriorityQueueData))]
    public void GetConverterTest<T>(T? data, string pattern)
    {
        var generator = Generator.CreateAotBuilder()
            .AddConverterCreators(IntegrationGeneratorContext.ConverterCreators.Values)
            .Build();
        var converter = generator.GetConverter<T>();
        var converterType = converter.GetType();
        Assert.True(converterType.Assembly == typeof(IConverter).Assembly);
        Assert.Matches(pattern, converterType.FullName);

        // simple encode decode test
        var buffer = converter.Encode(data);
        var result = converter.Decode(buffer);
        _ = result;
    }

    public static IEnumerable<object?[]> NullableData()
    {
        yield return [Box<int?>(null), "NullableConverter`1.*Int32"];
        yield return [Box<int?>(1024), "NullableConverter`1.*Int32"];
        yield return [Box<ConsoleKey?>(null), "NullableConverter`1.*ConsoleKey"];
        yield return [Box<ConsoleKey?>(ConsoleKey.Enter), "NullableConverter`1.*ConsoleKey"];
    }

    [Theory(DisplayName = "Get Converter With Boxed Data Test")]
    [MemberData(nameof(NullableData))]
    public void GetConverterBoxedTest<T>(StrongBox<T> data, string pattern)
    {
        GetConverterTest(data.Value, pattern);
    }
}
