namespace Mikodev.Binary.Tests.Creators;

using Microsoft.FSharp.Core;
using Mikodev.Binary.Tests.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit;

public class NullableConverterTests
{
    private readonly IGenerator generator = Generator.CreateDefaultBuilder()
        .AddFSharpConverterCreators()
        .Build();

    private static byte[] Encode<T>(Converter<T> converter, T value)
    {
        var allocator = new Allocator();
        converter.Encode(ref allocator, value);
        return allocator.AsSpan().ToArray();
    }

    private static byte[] EncodeAuto<T>(Converter<T> converter, T value)
    {
        var allocator = new Allocator();
        converter.EncodeAuto(ref allocator, value);
        return allocator.AsSpan().ToArray();
    }

    [Theory(DisplayName = "Nullable (encode & decode)")]
    [InlineData((byte)254)]
    [InlineData(1920)]
    [InlineData(1.4F)]
    [InlineData(3.14)]
    public unsafe void Item<T>(T value) where T : unmanaged
    {
        var converter = this.generator.GetConverter<T?>();
        Assert.StartsWith("NullableConverter`1", converter.GetType().Name);

        var a = (T?)value;
        var b = (T?)null;

        var ta = Encode(converter, a);
        var tb = Encode(converter, b);

        Assert.Equal(sizeof(T) + 1, ta.Length);
        _ = Assert.Single(tb);

        var ra = converter.Decode(ta);
        var rb = converter.Decode(tb);

        Assert.True(ra.HasValue);
        Assert.False(rb.HasValue);
        Assert.Equal(value, ra);
    }

    [Theory(DisplayName = "Nullable (encode auto & decode auto)")]
    [InlineData((byte)254)]
    [InlineData(1920)]
    [InlineData(1.4F)]
    [InlineData(3.14)]
    public unsafe void ItemAuto<T>(T value) where T : unmanaged
    {
        var converter = this.generator.GetConverter<T?>();
        Assert.StartsWith("NullableConverter`1", converter.GetType().Name);

        var a = (T?)value;
        var b = (T?)null;

        var ta = EncodeAuto(converter, a);
        var tb = EncodeAuto(converter, b);

        Assert.Equal(sizeof(T) + 1, ta.Length);
        _ = Assert.Single(tb);

        var sa = new ReadOnlySpan<byte>(ta);
        var sb = new ReadOnlySpan<byte>(tb);
        var ra = converter.DecodeAuto(ref sa);
        var rb = converter.DecodeAuto(ref sb);

        Assert.True(ra.HasValue);
        Assert.False(rb.HasValue);
        Assert.Equal(value, ra);
    }

    public static readonly IEnumerable<object[]> CollectionData =
    [
        [new byte?[] { 2, 4, null, 8, null }],
        [new List<float?> { null, 2.71F, null }],
        [new HashSet<double?> { 3.14, null, 1.41 }]
    ];

    internal unsafe void CollectionFunction<TCollection, T>(TCollection collection) where T : unmanaged where TCollection : IEnumerable<T?>
    {
        var buffer = this.generator.Encode(collection);
        var exceptLength = collection.Count() + collection.Count(x => x != null) * sizeof(T);
        Assert.Equal(exceptLength, buffer.Length);
        var result = this.generator.Decode<TCollection>(buffer);
        Assert.False(ReferenceEquals(collection, result));
        Assert.Equal(collection, result);
        _ = Assert.IsType<TCollection>(result);
    }

    [Theory(DisplayName = "Nullable Collection")]
    [MemberData(nameof(CollectionData))]
    public void Collection<TCollection>(TCollection collection) where TCollection : notnull
    {
        var collectionType = collection.GetType();
        var nullableType = collectionType
            .GetInterfaces()
            .Single(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            .GetGenericArguments()
            .Single();
        var elementType = nullableType.GetGenericArguments().Single();
        var method = GetType()
            .GetMethodNotNull(nameof(CollectionFunction), BindingFlags.Instance | BindingFlags.NonPublic)
            .MakeGenericMethod(collectionType, elementType);
        _ = method.Invoke(this, [collection]);
    }

    public static readonly IEnumerable<object[]> DictionaryData =
    [
        [new Dictionary<int, double?> { [0] = null, [1] = 1.1, [-2] = 2.2 }],
        [new Dictionary<float, long?> { [0] = null, [-3.3F] = 6L, [4.4F] = 8 }],
    ];

    [Theory(DisplayName = "Nullable Dictionary")]
    [MemberData(nameof(DictionaryData))]
    public void Dictionary<TKey, TValue>(IDictionary<TKey, TValue> dictionary) where TKey : notnull
    {
        var buffer = this.generator.Encode(dictionary);
        Assert.Equal((4 * 3) + (1 + ((1 + 8) * 2)), buffer.Length);
        var result = this.generator.Decode<IDictionary<TKey, TValue>>(buffer);
        Assert.False(ReferenceEquals(dictionary, result));
        Assert.Equal(dictionary, result);
        _ = Assert.IsType<Dictionary<TKey, TValue>>(result);
    }

    [Fact(DisplayName = "Nullable Property")]
    public void Property()
    {
        var data = new
        {
            id = (int?)97531,
            one = (Guid?)Guid.NewGuid(),
            two = (decimal?)null
        };

        var buffer = this.generator.Encode(data);
        Assert.Equal(((1 * 3) + (2 + 3 + 3)) + (((1 + 1) * 2 + (1 + 4)) + (4 + 16 + 0)), buffer.Length);
        var result = this.generator.Decode(buffer, data);
        Assert.False(ReferenceEquals(data, result));
        Assert.Equal(data, result);
    }

    [Theory(DisplayName = "Nullable Variable Length Type (encode & decode)")]
    [InlineData("alpha")]
    [InlineData("echo")]
    public void Variable(string text)
    {
        var source = new (int, string)?((-1, text));
        var buffer = this.generator.Encode(source);
        Assert.Equal(1 + 4 + Encoding.UTF8.GetByteCount(text), buffer.Length);
        var result = this.generator.Decode<(int, string)?>(buffer);
        Assert.True(result.HasValue);
        Assert.Equal(source, result);
    }

    [Theory(DisplayName = "Nullable Variable Length Type (encode auto & decode auto)")]
    [InlineData("charlie")]
    [InlineData("delta")]
    public void VariableAuto(string text)
    {
        var source = new (int, string)?((-1, text));
        var converter = this.generator.GetConverter(source);
        var buffer = EncodeAuto(converter, source);
        Assert.Equal(1 + 4 + 1 + Encoding.UTF8.GetByteCount(text), buffer.Length);
        var span = new ReadOnlySpan<byte>(buffer);
        var result = converter.DecodeAuto(ref span);
        Assert.True(result.HasValue);
        Assert.Equal(source, result);
        Assert.Equal(0, span.Length);
    }

    [Fact(DisplayName = "Nullable Variable Length Collection")]
    public void VariableCollection()
    {
        var source = new (float, string)?[] { (-1.1F, "quick"), null, (2.2F, "fox") };
        var buffer = this.generator.Encode(source);
        Assert.Equal((1 + 4 + 1 + 5) + (1 + 0) + (1 + 4 + 1 + 3), buffer.Length);
        var result = this.generator.Decode<(float, string)?[]>(buffer);
        Assert.False(ReferenceEquals(result, source));
        Assert.Equal(source, result);
    }

    public static readonly IEnumerable<object[]> OptionData =
    [
        [10],
        [long.MaxValue],
        [(-1536, "Inner text")],
        [("Value tuple", Guid.NewGuid())],
    ];

    [Theory(DisplayName = "Nullable With F# Option")]
    [MemberData(nameof(OptionData))]
    public void FSharpOptionTest<T>(T data) where T : struct
    {
        var converterItem = this.generator.GetConverter<T?>();
        var converterOption = this.generator.GetConverter<FSharpOption<T>>();
        Assert.StartsWith("NullableConverter`1", converterItem.GetType().Name);
        Assert.StartsWith("UnionConverter`1", converterOption.GetType().Name);

        var a = (T?)data;
        var b = (T?)null;
        var m = FSharpOption<T>.Some(data);
        var n = FSharpOption<T>.None;

        Assert.Equal(Encode(converterItem, a), Encode(converterOption, m));
        Assert.Equal(EncodeAuto(converterItem, a), EncodeAuto(converterOption, m));
        Assert.Equal(Encode(converterItem, b), Encode(converterOption, n));
        Assert.Equal(EncodeAuto(converterItem, b), EncodeAuto(converterOption, n));
    }

    [Theory(DisplayName = "Invalid Nullable Tag (decode & decode auto)")]
    [InlineData(2, new byte[] { 0x02 })]
    [InlineData(3, new byte[] { 0x03 })]
    [InlineData(127, new byte[] { 0x80, 0x00, 0x00, 0x7F })]
    [InlineData(2333, new byte[] { 0x80, 0x00, 0x09, 0x1D })]
    public void Invalid(int tag, byte[] buffer)
    {
        var converter = this.generator.GetConverter<int?>();
        Assert.StartsWith("NullableConverter`1", converter.GetType().Name);

        var alpha = Assert.Throws<ArgumentException>(() => converter.Decode(buffer));
        var bravo = Assert.Throws<ArgumentException>(() =>
        {
            var span = new ReadOnlySpan<byte>(buffer);
            _ = converter.DecodeAuto(ref span);
        });
        var message = $"Invalid nullable tag '{tag}', type: System.Nullable`1[System.Int32]";
        Assert.Null(alpha.ParamName);
        Assert.Null(bravo.ParamName);
        Assert.Equal(message, alpha.Message);
        Assert.Equal(message, bravo.Message);
    }

    [Theory(DisplayName = "Valid Nullable Tag (decode & decode auto)")]
    [InlineData(false, 0, new byte[] { 0x00 })]
    [InlineData(true, (byte)0x5E, new byte[] { 0x01, 0x5E })]
    [InlineData(false, 0, new byte[] { 0x80, 0x00, 0x00, 0x00 })]
    [InlineData(true, (byte)0x7F, new byte[] { 0x80, 0x00, 0x00, 0x01, 0x7F })]
    public void ValidTag<T>(bool hasValue, T value, byte[] buffer) where T : struct
    {
        var converter = this.generator.GetConverter<T?>();
        var a = new ReadOnlySpan<byte>(buffer);
        var b = new ReadOnlySpan<byte>(buffer);
        var m = converter.Decode(in a);
        var n = converter.DecodeAuto(ref b);
        Assert.Equal(buffer.Length, a.Length);
        Assert.Equal(0, b.Length);
        Assert.Equal(hasValue, m.HasValue);
        Assert.Equal(hasValue, n.HasValue);
        Assert.Equal(value, m.GetValueOrDefault());
        Assert.Equal(value, m.GetValueOrDefault());
    }
}
