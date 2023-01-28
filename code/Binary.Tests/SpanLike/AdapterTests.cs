namespace Mikodev.Binary.Tests.SpanLike;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Xunit;

public class AdapterTests
{
    private delegate int Length<T>(T? item);

    private delegate ReadOnlySpan<E> AsSpanDelegate<T, E>(T? item);

    private delegate void Encode<T, E>(ref Allocator allocator, T? item, Converter<E> converter);

    private record Adapter<T, E>(AsSpanDelegate<T, E> AsSpan, Encode<T, E> Encode, Length<T> Length);

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
        var length = GetDelegate<Length<T>>(adapterType, "Length");
        var encode = GetDelegate<Encode<T, E>>(adapterType, "Encode");
        var invoke = GetDelegate<AsSpanDelegate<T, E>>(adapterType, "AsSpan");
        return new Adapter<T, E>(invoke, encode, length);
    }

    private static void TestAllMethod<T, E>(T source, int length, AsSpanDelegate<T, E> invoke)
    {
        var adapter = GetAdapter<T, E>();
        Assert.Equal(length, adapter.Length.Invoke(source));

        var generator = Generator.CreateDefault();
        var allocator = new Allocator();
        adapter.Encode.Invoke(ref allocator, source, generator.GetConverter<E>());
        var buffer = allocator.ToArray();
        var result = generator.Decode<T>(buffer);
        Assert.Equal(invoke.Invoke(source).ToArray(), invoke.Invoke(result).ToArray());

        if (RuntimeHelpers.IsReferenceOrContainsReferences<E>() is true)
            return;
        var expect = invoke.Invoke(source);
        var actual = adapter.AsSpan.Invoke(source);
        Assert.Equal(length, actual.Length);
        Assert.Equal(expect.ToArray(), actual.ToArray());
        Assert.True(Unsafe.AreSame(ref MemoryMarshal.GetReference(expect), ref MemoryMarshal.GetReference(actual)));
    }

    private static void TestAllMethodNullOrEmptyData<T, E>(T? source)
    {
        var adapter = GetAdapter<T, E>();
        Assert.Equal(0, adapter.Length.Invoke(source));

        var allocator = new Allocator();
        Assert.Equal(0, allocator.Length);
        // converter is null here, because it should not be invoked
        var converter = (Converter<E>)null!;
        adapter.Encode.Invoke(ref allocator, source, converter);
        Assert.Equal(0, allocator.Length);

        if (RuntimeHelpers.IsReferenceOrContainsReferences<E>() is true)
            return;
        Assert.Equal(0, adapter.AsSpan.Invoke(source).Length);
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

    [Theory(DisplayName = "Array Adapter Test")]
    [MemberData(nameof(NumberData))]
    [MemberData(nameof(TimeSpanData))]
    [MemberData(nameof(StringData))]
    public void ArrayAdapterTest<E>(E[] array, int start, int count)
    {
        TestAllMethod<E[], E>(array.AsSpan().Slice(start, count).ToArray(), count, x => x);
    }

    [Theory(DisplayName = "Array Adapter Null Or Empty Test")]
    [MemberData(nameof(EmptyData))]
    public void ArrayAdapterNullOrEmptyTest<E>(E[] array)
    {
        Assert.Empty(array);
        var test = TestAllMethodNullOrEmptyData<E[], E>;
        test.Invoke(null);
        test.Invoke(Array.Empty<E>());
    }

    [Theory(DisplayName = "ArraySegment Adapter Test")]
    [MemberData(nameof(NumberData))]
    [MemberData(nameof(TimeSpanData))]
    [MemberData(nameof(StringData))]
    public void ArraySegmentAdapterTest<E>(E[] array, int start, int count)
    {
        TestAllMethod<ArraySegment<E>, E>(new ArraySegment<E>(array, start, count), count, x => x);
    }

    [Theory(DisplayName = "ArraySegment Adapter Null Or Empty Test")]
    [MemberData(nameof(EmptyData))]
    public void ArraySegmentAdapterNullOrEmptyTest<E>(E[] array)
    {
        Assert.Empty(array);
        var test = TestAllMethodNullOrEmptyData<ArraySegment<E>, E>;
        test.Invoke(default);
        test.Invoke(new ArraySegment<E>());
        test.Invoke(new ArraySegment<E>(Array.Empty<E>()));
        test.Invoke(new ArraySegment<E>(Array.Empty<E>(), 0, 0));
    }

    [Theory(DisplayName = "ImmutableArray Adapter Test")]
    [MemberData(nameof(NumberData))]
    [MemberData(nameof(TimeSpanData))]
    [MemberData(nameof(StringData))]
    public void ImmutableArrayAdapterTest<E>(E[] array, int start, int count)
    {
        TestAllMethod(ImmutableArray.Create(array, start, count), count, x => x.AsSpan());
    }

    [Theory(DisplayName = "ImmutableArray Adapter Null Or Empty Test")]
    [MemberData(nameof(EmptyData))]
    public void ImmutableArrayAdapterNullOrEmptyTest<E>(E[] array)
    {
        Assert.Empty(array);
        var test = TestAllMethodNullOrEmptyData<ImmutableArray<E>, E>;
        test.Invoke(default);
        test.Invoke(new ImmutableArray<E>());
        test.Invoke(ImmutableArray<E>.Empty);
        test.Invoke(ImmutableArray.Create<E>());
        test.Invoke(ImmutableArray.Create(Array.Empty<E>()));
        test.Invoke(ImmutableArray.CreateRange(Array.Empty<E>()));
    }

    [Theory(DisplayName = "List Adapter Test")]
    [MemberData(nameof(NumberData))]
    [MemberData(nameof(TimeSpanData))]
    [MemberData(nameof(StringData))]
    public void ListAdapterTest<E>(E[] array, int start, int count)
    {
        TestAllMethod<List<E>, E>(new List<E>(array.Skip(start).Take(count)), count, x => CollectionsMarshal.AsSpan(x));
    }

    [Theory(DisplayName = "List Adapter Null Or Empty Test")]
    [MemberData(nameof(EmptyData))]
    public void ListAdapterNullOrEmptyTest<E>(E[] array)
    {
        Assert.Empty(array);
        var test = TestAllMethodNullOrEmptyData<List<E>, E>;
        test.Invoke(null);
        test.Invoke(new List<E>());
        test.Invoke(new List<E>(capacity: 8));
    }

    [Theory(DisplayName = "Memory Adapter Test")]
    [MemberData(nameof(NumberData))]
    [MemberData(nameof(TimeSpanData))]
    [MemberData(nameof(StringData))]
    public void MemoryAdapterTest<E>(E[] array, int start, int count)
    {
        TestAllMethod<Memory<E>, E>(new Memory<E>(array, start, count), count, x => x.Span);
    }

    [Theory(DisplayName = "ReadOnlyMemory Adapter Test")]
    [MemberData(nameof(NumberData))]
    [MemberData(nameof(TimeSpanData))]
    [MemberData(nameof(StringData))]
    public void ReadOnlyMemoryAdapterTest<E>(E[] array, int start, int count)
    {
        TestAllMethod(new ReadOnlyMemory<E>(array, start, count), count, x => x.Span);
    }
}
