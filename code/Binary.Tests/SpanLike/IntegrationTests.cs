namespace Mikodev.Binary.Tests.SpanLike;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Xunit;

public class IntegrationTests
{
    private delegate ReadOnlySpan<E> AsSpanDelegate<T, E>(T? item);

    private delegate T InvokeDelegate<T, E>(E[] values, int length);

    private record Adapter<T, E>(AsSpanDelegate<T, E> AsSpan, InvokeDelegate<T, E> Create);

    private static T GetDelegate<T>(Type type, string methodName) where T : Delegate
    {
        var method = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public);
        Assert.NotNull(method);
        var result = Delegate.CreateDelegate(typeof(T), method);
        return (T)result;
    }

    private static Adapter<T, E> GetAdapter<T, E>()
    {
        var types = typeof(IConverter).Assembly.GetTypes();
        var adapterTypeName = (typeof(T).IsArray ? "Array" : typeof(T).Name.Split('`').First()) + "Adapter";
        var adapterTypeDefinition = types.Single(x => x.Namespace is "Mikodev.Binary.Internal.SpanLike.Adapters" && x.Name.StartsWith(adapterTypeName));
        var adapterType = adapterTypeDefinition.MakeGenericType(typeof(E));
        var invoke = GetDelegate<InvokeDelegate<T, E>>(adapterType, "Invoke");
        var memory = GetDelegate<AsSpanDelegate<T, E>>(adapterType, "AsSpan");
        return new Adapter<T, E>(memory, invoke);
    }

    private static void TestAdapterMethods<T, E>(T source, AsSpanDelegate<T, E> invoke)
    {
        var adapter = GetAdapter<T, E>();
        var generator = Generator.CreateDefault();
        var buffer = generator.Encode(source);
        var values = generator.Decode<E[]>(buffer);
        var result = adapter.Create.Invoke(values, values.Length);
        var bufferResult = generator.Encode(result);
        Assert.Equal(buffer, bufferResult);

        var expect = invoke.Invoke(source);
        var actual = adapter.AsSpan.Invoke(source);
        Assert.Equal(expect.ToArray(), actual.ToArray());
        Assert.True(Unsafe.AreSame(ref MemoryMarshal.GetReference(expect), ref MemoryMarshal.GetReference(actual)));
    }

    private static void TestAdapterMethodsWithNullOrEmptyData<T, E>(T? source)
    {
        var adapter = GetAdapter<T, E>();
        var generator = Generator.CreateDefault();
        var result = adapter.Create.Invoke([], 0);
        var bufferResult = generator.Encode(result);
        Assert.Empty(bufferResult);
        Assert.Equal(0, adapter.AsSpan.Invoke(source).Length);
    }

    private static void TestConverterMethods<T, E>(T? source, AsSpanDelegate<T, E> invoke)
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<T>();
        Assert.Equal("Mikodev.Binary.Internal.SpanLike", converter.GetType().Namespace);

        var allocator = new Allocator();
        converter.Encode(ref allocator, source);
        var buffer = allocator.AsSpan();
        var result = converter.Decode(buffer);
        var sourceSpan = invoke.Invoke(source);
        var resultSpan = invoke.Invoke(result);
        Assert.True(sourceSpan.SequenceEqual(resultSpan));

        var allocatorForAutoMethods = new Allocator();
        var allocatorForLengthPrefixMethods = new Allocator();
        converter.EncodeAuto(ref allocatorForAutoMethods, source);
        converter.EncodeWithLengthPrefix(ref allocatorForLengthPrefixMethods, source);
        var bufferForAutoMethods = allocatorForAutoMethods.AsSpan();
        var bufferForLengthPrefixMethods = allocatorForLengthPrefixMethods.AsSpan();
        Assert.True(bufferForAutoMethods.SequenceEqual(bufferForLengthPrefixMethods));

        var resultForAutoMethods = converter.DecodeAuto(ref bufferForAutoMethods);
        var resultForLengthPrefixMethods = converter.DecodeWithLengthPrefix(ref bufferForLengthPrefixMethods);
        var resultForAutoMethodsSpan = invoke.Invoke(resultForAutoMethods);
        var resultForLengthPrefixMethodsSpan = invoke.Invoke(resultForLengthPrefixMethods);
        Assert.True(sourceSpan.SequenceEqual(resultForAutoMethodsSpan));
        Assert.True(sourceSpan.SequenceEqual(resultForLengthPrefixMethodsSpan));
    }

    public static IEnumerable<object?[]> NumberData()
    {
        var sequence = Enumerable.Range(0, 100).ToArray();
        yield return new object[] { sequence, 0, sequence.Length };
        yield return new object[] { sequence, 11, 66 };
        yield return new object[] { sequence, 50, 0 };
    }

    public static IEnumerable<object?[]> TimeSpanData()
    {
        var sequence = Enumerable.Range(0, 100).Select(x => TimeSpan.FromSeconds(x)).ToArray();
        yield return new object[] { sequence, 0, sequence.Length };
        yield return new object[] { sequence, 47, 33 };
        yield return new object[] { sequence, 20, 0 };
    }

    public static IEnumerable<object?[]> StringData()
    {
        var sequence = Enumerable.Range(0, 96).Select(x => new string(Enumerable.Range(32, x).Select(i => (char)i).ToArray())).ToArray();
        yield return new object[] { sequence, 0, sequence.Length };
        yield return new object[] { sequence, 16, 10 };
        yield return new object[] { sequence, 20, 0 };
    }

    public static IEnumerable<object?[]> EmptyData()
    {
        yield return new object[] { Array.Empty<int>() };
        yield return new object[] { Array.Empty<TimeSpan>() };
        yield return new object[] { Array.Empty<string>() };
    }

    [Theory(DisplayName = "Array Test")]
    [MemberData(nameof(NumberData))]
    [MemberData(nameof(TimeSpanData))]
    [MemberData(nameof(StringData))]
    public void ArrayTest<E>(E[] array, int start, int count)
    {
        TestAdapterMethods<E[], E>(array.AsSpan(start, count).ToArray(), x => x);
        TestConverterMethods<E[], E>(array.AsSpan(start, count).ToArray(), x => x);
    }

    [Theory(DisplayName = "Array Null Or Empty Test")]
    [MemberData(nameof(EmptyData))]
    public void ArrayNullOrEmptyTest<E>(E[] array)
    {
        Assert.Empty(array);
        TestAdapterMethodsWithNullOrEmptyData<E[], E>(null);
        TestAdapterMethodsWithNullOrEmptyData<E[], E>([]);
        TestConverterMethods<E[], E>(null, x => x);
        TestConverterMethods<E[], E>([], x => x);

    }

    [Theory(DisplayName = "ArraySegment Test")]
    [MemberData(nameof(NumberData))]
    [MemberData(nameof(TimeSpanData))]
    [MemberData(nameof(StringData))]
    public void ArraySegmentTest<E>(E[] array, int start, int count)
    {
        TestAdapterMethods<ArraySegment<E>, E>(new ArraySegment<E>(array, start, count), x => x);
        TestConverterMethods<ArraySegment<E>, E>(new ArraySegment<E>(array, start, count), x => x);
    }

    [Theory(DisplayName = "ArraySegment Null Or Empty Test")]
    [MemberData(nameof(EmptyData))]
    public void ArraySegmentNullOrEmptyTest<E>(E[] array)
    {
        Assert.Empty(array);
        TestAdapterMethodsWithNullOrEmptyData<ArraySegment<E>, E>(default);
        TestAdapterMethodsWithNullOrEmptyData<ArraySegment<E>, E>(new ArraySegment<E>());
        TestAdapterMethodsWithNullOrEmptyData<ArraySegment<E>, E>(new ArraySegment<E>([]));
        TestAdapterMethodsWithNullOrEmptyData<ArraySegment<E>, E>(new ArraySegment<E>([], 0, 0));
        TestConverterMethods<ArraySegment<E>, E>(default, x => x);
        TestConverterMethods<ArraySegment<E>, E>(new ArraySegment<E>(), x => x);
        TestConverterMethods<ArraySegment<E>, E>(new ArraySegment<E>([]), x => x);
        TestConverterMethods<ArraySegment<E>, E>(new ArraySegment<E>([], 0, 0), x => x);
    }

    [Theory(DisplayName = "ImmutableArray Test")]
    [MemberData(nameof(NumberData))]
    [MemberData(nameof(TimeSpanData))]
    [MemberData(nameof(StringData))]
    public void ImmutableArrayTest<E>(E[] array, int start, int count)
    {
        TestAdapterMethods(ImmutableArray.Create(array, start, count), x => x.AsSpan());
        TestConverterMethods(ImmutableArray.Create(array, start, count), x => x.AsSpan());
    }

    [Theory(DisplayName = "ImmutableArray Null Or Empty Test")]
    [MemberData(nameof(EmptyData))]
    public void ImmutableArrayNullOrEmptyTest<E>(E[] array)
    {
        Assert.Empty(array);
        TestAdapterMethodsWithNullOrEmptyData<ImmutableArray<E>, E>(default);
        TestAdapterMethodsWithNullOrEmptyData<ImmutableArray<E>, E>([]);
        TestConverterMethods<ImmutableArray<E>, E>(default, x => x.AsSpan());
        TestConverterMethods<ImmutableArray<E>, E>([], x => x.AsSpan());
    }

    [Theory(DisplayName = "List Test")]
    [MemberData(nameof(NumberData))]
    [MemberData(nameof(TimeSpanData))]
    [MemberData(nameof(StringData))]
    public void ListTest<E>(E[] array, int start, int count)
    {
        TestConverterMethods<List<E>, E>(new List<E>(array.Skip(start).Take(count)), x => CollectionsMarshal.AsSpan(x));
    }

    [Theory(DisplayName = "List Null Or Empty Test")]
    [MemberData(nameof(EmptyData))]
    public void ListNullOrEmptyTest<E>(E[] array)
    {
        Assert.Empty(array);
        TestConverterMethods<List<E>, E>(null, x => CollectionsMarshal.AsSpan(x));
        TestConverterMethods<List<E>, E>([], x => CollectionsMarshal.AsSpan(x));
        TestConverterMethods<List<E>, E>(new List<E>(capacity: 8), x => CollectionsMarshal.AsSpan(x));
    }

    [Theory(DisplayName = "Memory Test")]
    [MemberData(nameof(NumberData))]
    [MemberData(nameof(TimeSpanData))]
    [MemberData(nameof(StringData))]
    public void MemoryTest<E>(E[] array, int start, int count)
    {
        TestAdapterMethods<Memory<E>, E>(new Memory<E>(array, start, count), x => x.Span);
        TestConverterMethods<Memory<E>, E>(new Memory<E>(array, start, count), x => x.Span);
    }

    [Theory(DisplayName = "ReadOnlyMemory Test")]
    [MemberData(nameof(NumberData))]
    [MemberData(nameof(TimeSpanData))]
    [MemberData(nameof(StringData))]
    public void ReadOnlyMemoryTest<E>(E[] array, int start, int count)
    {
        TestAdapterMethods(new ReadOnlyMemory<E>(array, start, count), x => x.Span);
        TestConverterMethods(new ReadOnlyMemory<E>(array, start, count), x => x.Span);
    }
}
