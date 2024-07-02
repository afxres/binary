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

    public static IEnumerable<object[]> EnumData()
    {
        yield return new object[] { DayOfWeek.Sunday, "NativeEndianConverter`1.*DayOfWeek" };
        yield return new object[] { ConsoleColor.White, "NativeEndianConverter`1.*ConsoleColor" };
    }

    public static IEnumerable<object[]> KeyValuePairData()
    {
        yield return new object[] { KeyValuePair.Create(1, "2"), "KeyValuePairConverter`2.*Int32.*String" };
        yield return new object[] { KeyValuePair.Create("3", 4), "KeyValuePairConverter`2.*String.*Int32" };
    }

    public static IEnumerable<object[]> VariableBoundArrayData()
    {
        yield return new object[] { new int[1, 1] { { 1 } }, @"VariableBoundArrayConverter`2.*Int32\[,\].*Int32" };
        yield return new object[] { new string[1, 1, 1] { { { "2" } } }, @"VariableBoundArrayConverter`2.*String\[,,\].*String" };
    }

    public static IEnumerable<object[]> ArrayData()
    {
        yield return new object[] { new[] { 1 }, @"ArrayBasedNativeEndianConverter`3.*Int32\[\]" };
        yield return new object[] { new[] { "2" }, @"ArrayBasedConverter`3.*String\[\]" };
    }

    public static IEnumerable<object[]> ArraySegmentData()
    {
        yield return new object[] { new ArraySegment<int>([1]), "ArrayBasedNativeEndianConverter`3.*ArraySegment`1.*Int32" };
        yield return new object[] { new ArraySegment<string>(["2"]), "ArrayBasedConverter`3.*ArraySegment`1.*String" };
    }

    public static IEnumerable<object[]> MemoryData()
    {
        yield return new object[] { new Memory<int>([3]), "ArrayBasedNativeEndianConverter`3.*Memory`1.*Int32" };
        yield return new object[] { new Memory<string>(["4"]), "ArrayBasedConverter`3.*Memory`1.*String" };
    }

    public static IEnumerable<object[]> ReadOnlyMemoryData()
    {
        yield return new object[] { new ReadOnlyMemory<int>([5]), "ArrayBasedNativeEndianConverter`3.*ReadOnlyMemory`1.*Int32" };
        yield return new object[] { new ReadOnlyMemory<string>(["6"]), "ArrayBasedConverter`3.*ReadOnlyMemory`1.*String" };
    }

    public static IEnumerable<object[]> ReadOnlySequenceData()
    {
        yield return new object[] { new ReadOnlySequence<int>([1]), "ReadOnlySequenceConverter`1.*Int32" };
        yield return new object[] { new ReadOnlySequence<string>(["2"]), "ReadOnlySequenceConverter`1.*String" };
    }

    public static IEnumerable<object[]> PriorityQueueData()
    {
        yield return new object[] { new PriorityQueue<int, long>([(1, 2L)]), "PriorityQueueConverter`2.*Int32.*Int64" };
        yield return new object[] { new PriorityQueue<string, int>([("3", 4)]), "PriorityQueueConverter`2.*String.*Int32" };
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
        yield return new object?[] { Box<int?>(null), "NullableConverter`1.*Int32" };
        yield return new object?[] { Box<int?>(1024), "NullableConverter`1.*Int32" };
        yield return new object?[] { Box<ConsoleKey?>(null), "NullableConverter`1.*ConsoleKey" };
        yield return new object?[] { Box<ConsoleKey?>(ConsoleKey.Enter), "NullableConverter`1.*ConsoleKey" };
    }

    [Theory(DisplayName = "Get Converter With Boxed Data Test")]
    [MemberData(nameof(NullableData))]
    public void GetConverterBoxedTest<T>(StrongBox<T> data, string pattern)
    {
        GetConverterTest(data.Value, pattern);
    }
}
