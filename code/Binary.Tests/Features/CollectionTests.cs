namespace Mikodev.Binary.Tests.Features;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Xunit;

public class CollectionTests
{
    private readonly IGenerator generator;

    private delegate void Encode<T>(ref Allocator allocator, ReadOnlySpan<T> item);

    private static object AssertFieldTypeName(object instance, string fieldName, string typeName)
    {
        var adapterField = Assert.IsAssignableFrom<FieldInfo>(instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic));
        var adapter = Assert.IsAssignableFrom<object>(adapterField.GetValue(instance));
        var adapterType = adapter.GetType();
        Assert.Equal(typeName, adapterType.Name);
        return adapter;
    }

    public CollectionTests()
    {
        var generator = Generator.CreateDefault();
        this.generator = generator;
    }

    [Theory(DisplayName = "Native Type Array Implementation")]
    [InlineData(0)]
    [InlineData(1.0)]
    public void NativeTypeArrayImplementation<T>(T data)
    {
        var converter = this.generator.GetConverter<T[]>();
        Assert.Equal("NativeEndianConverter`1", this.generator.GetConverter<T>().GetType().Name);
        var forward = AssertFieldTypeName(converter, "encoder", "ConstantForwardEncoder`3");
        _ = AssertFieldTypeName(forward, "encoder", "NativeEndianEncoder`1");
        _ = AssertFieldTypeName(converter, "decoder", "NativeEndianDecoder`1");
        var source = Enumerable.Repeat(data, 16).ToArray();
        for (var i = 0; i < source.Length; i++)
        {
            var values = source[0..i];
            var buffer = converter.Encode(values);
            var result = converter.Decode(buffer);
            Assert.Equal(values, result);
        }
    }

    public static IEnumerable<object[]> SimpleObjectData => new List<object[]>
    {
        new object[] { DateOnly.Parse("2001-02-03") },
        new object[] { DateTimeOffset.Parse("2020-02-02T11:22:33+04:00") },
        new object[] { DateTime.Parse("2001-02-03T04:05:06") },
        new object[] { Guid.Parse("f28a5581-c80d-4d66-84cf-790d48e877d1") },
        new object[] { (Rune)'#' },
        new object[] { TimeOnly.Parse("12:34:56") },
        new object[] { TimeSpan.Parse("01:23:45.6789") },
    };

    [Theory(DisplayName = "Common Type Array Implementation")]
    [MemberData(nameof(SimpleObjectData))]
    public void CommonTypeArrayImplementation<T>(T data)
    {
        var converter = this.generator.GetConverter<T[]>();
        Assert.Equal($"{typeof(T).Name}Converter", this.generator.GetConverter<T>().GetType().Name);
        var forward = AssertFieldTypeName(converter, "encoder", "ConstantForwardEncoder`3");
        _ = AssertFieldTypeName(forward, "encoder", "ConstantEncoder`2");
        _ = AssertFieldTypeName(converter, "decoder", "ConstantDecoder`2");
        var source = Enumerable.Repeat(data, 16).ToArray();
        for (var i = 0; i < source.Length; i++)
        {
            var values = source[0..i];
            var buffer = converter.Encode(values);
            var result = converter.Decode(buffer);
            Assert.Equal(values, result);
        }
    }

    [Theory(DisplayName = "Unsafe Adapter Overflow Test")]
    [MemberData(nameof(SimpleObjectData))]
    public void UnsafeAdapterOverflowTest<T>(T data)
    {
        // ignore it
        _ = data;
        var converter = this.generator.GetConverter<T[]>();
        Assert.Equal($"{typeof(T).Name}Converter", this.generator.GetConverter<T>().GetType().Name);
        var forward = AssertFieldTypeName(converter, "encoder", "ConstantForwardEncoder`3");
        var encoder = AssertFieldTypeName(forward, "encoder", "ConstantEncoder`2");
        _ = AssertFieldTypeName(converter, "decoder", "ConstantDecoder`2");
        var functor = (Encode<T>)Delegate.CreateDelegate(typeof(Encode<T>), encoder, "Encode");
        var error = Assert.Throws<OverflowException>(() =>
        {
            // Max capacity is zero
            var allocator = new Allocator(new Span<byte>(), 0);
            // Invalid span, do not dereference!!!
            var invalidSpan = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.NullRef<T>(), 0x4000_0000);
            functor.Invoke(ref allocator, invalidSpan);
        });
        Assert.Equal(new OverflowException().Message, error.Message);
    }
}
