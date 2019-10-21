using Microsoft.FSharp.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Mikodev.Binary.Tests
{
    public class NullableTests
    {
        private readonly IGenerator generator = new GeneratorBuilder()
            .AddDefaultConverterCreators()
            .AddFSharpConverterCreators()
            .Build();

        private byte[] Bytes<T>(Converter<T> converter, T value)
        {
            var allocator = new Allocator();
            converter.Encode(ref allocator, value);
            return allocator.ToArray();
        }

        private byte[] BytesWithMark<T>(Converter<T> converter, T value)
        {
            var allocator = new Allocator();
            converter.EncodeAuto(ref allocator, value);
            return allocator.ToArray();
        }

        [Theory(DisplayName = "Nullable (to bytes & to value)")]
        [InlineData((byte)254)]
        [InlineData(1920)]
        [InlineData(1.4F)]
        [InlineData(3.14)]
        public unsafe void Item<T>(T value) where T : unmanaged
        {
            var converter = generator.GetConverter<T?>();
            Assert.StartsWith("NullableConverter`1", converter.GetType().Name);

            var a = (T?)value;
            var b = (T?)null;

            var ta = Bytes(converter, a);
            var tb = Bytes(converter, b);

            Assert.Equal(sizeof(T) + 1, ta.Length);
            _ = Assert.Single(tb);

            var ra = converter.Decode(ta);
            var rb = converter.Decode(tb);

            Assert.True(ra.HasValue);
            Assert.False(rb.HasValue);
            Assert.Equal(value, ra.Value);
        }

        [Theory(DisplayName = "Nullable (to bytes with mark & to value with mark)")]
        [InlineData((byte)254)]
        [InlineData(1920)]
        [InlineData(1.4F)]
        [InlineData(3.14)]
        public unsafe void ItemWith<T>(T value) where T : unmanaged
        {
            var converter = generator.GetConverter<T?>();
            Assert.StartsWith("NullableConverter`1", converter.GetType().Name);

            var a = (T?)value;
            var b = (T?)null;

            var ta = BytesWithMark(converter, a);
            var tb = BytesWithMark(converter, b);

            Assert.Equal(sizeof(T) + 1, ta.Length);
            _ = Assert.Single(tb);

            var sa = new ReadOnlySpan<byte>(ta);
            var sb = new ReadOnlySpan<byte>(tb);
            var ra = converter.DecodeAuto(ref sa);
            var rb = converter.DecodeAuto(ref sb);

            Assert.True(ra.HasValue);
            Assert.False(rb.HasValue);
            Assert.Equal(value, ra.Value);
        }

        public static IEnumerable<object[]> CollectionData = new object[][]
        {
            new object[] { new byte?[] { 2, 4, null, 8, null } },
            new object[] { new List<float?> { null, 2.71F, null } },
            new object[] { new HashSet<double?> { 3.14, null, 1.41 } }
        };

        internal unsafe void CollectionFunction<TCollection, T>(TCollection collection) where T : unmanaged where TCollection : IEnumerable<T?>
        {
            var buffer = generator.Encode(collection);
            var exceptLength = collection.Count() + collection.Count(x => x != null) * sizeof(T);
            Assert.Equal(exceptLength, buffer.Length);
            var result = generator.Decode<TCollection>(buffer);
            Assert.False(ReferenceEquals(collection, result));
            Assert.Equal(collection, result);
            _ = Assert.IsType<TCollection>(result);
        }

        [Theory(DisplayName = "Nullable Collection")]
        [MemberData(nameof(CollectionData))]
        public unsafe void Collection<TCollection>(TCollection collection)
        {
            var collectionType = collection.GetType();
            var nullableType = collectionType.GetInterfaces()
                .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                .Single()
                .GetGenericArguments()
                .Single();
            var elementType = nullableType.GetGenericArguments().Single();
            var method = GetType()
                .GetMethod(nameof(CollectionFunction), BindingFlags.Instance | BindingFlags.NonPublic)
                .MakeGenericMethod(collectionType, elementType);
            var _ = method.Invoke(this, new object[] { collection });
        }

        public static IEnumerable<object[]> DictionaryData = new object[][]
        {
            new object[] { new Dictionary<int?, double?> { [0] = null, [1] = 1.1, [-2] = 2.2 } },
            new object[] { new Dictionary<float?, long?> { [0] = null, [-3.3F] = 6L, [4.4F] = 8 } },
        };

        [Theory(DisplayName = "Nullable Dictionary")]
        [MemberData(nameof(DictionaryData))]
        public void Dictionary<TKey, TValue>(IDictionary<TKey, TValue> dictionary)
        {
            var buffer = generator.Encode(dictionary);
            Assert.Equal(((1 + 4) * 3) + (1 + ((1 + 8) * 2)), buffer.Length);
            var result = generator.Decode<IDictionary<TKey, TValue>>(buffer);
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

            var buffer = generator.Encode(data);
            Assert.Equal(((1 * 3) + (2 + 3 + 3)) + (((4 + 1) * 3) + (4 + 16 + 0)), buffer.Length);
            var result = generator.Decode(buffer, data);
            Assert.False(ReferenceEquals(data, result));
            Assert.Equal(data, result);
        }

        [Theory(DisplayName = "Nullable Variable Length Type")]
        [InlineData("alpha")]
        [InlineData("echo")]
        public void Variable(string text)
        {
            var source = new (int, string)?((-1, text));
            var buffer = generator.Encode(source);
            Assert.Equal(1 + 4 + Converter.Encoding.GetByteCount(text), buffer.Length);
            var result = generator.Decode<(int, string)?>(buffer);
            Assert.True(result.HasValue);
            Assert.Equal(source, result);
        }

        [Fact(DisplayName = "Nullable variable length collection")]
        public void VariableCollection()
        {
            var source = new (float, string)?[] { (-1.1F, "quick"), null, (2.2F, "fox") };
            var buffer = generator.Encode(source);
            Assert.Equal((1 + 4 + 1 + 5) + (1 + 0) + (1 + 4 + 1 + 3), buffer.Length);
            var result = generator.Decode<(float, string)?[]>(buffer);
            Assert.False(ReferenceEquals(result, source));
            Assert.Equal(source, result);
        }

        public static readonly IEnumerable<object[]> OptionData = new object[][]
        {
            new object[] { 10 },
            new object[] { long.MaxValue },
            new object[] { (-1536, "Inner text") },
            new object[] { ("Value tuple", Guid.NewGuid()) },
        };

        [Theory(DisplayName = "Nullable With F# Option")]
        [MemberData(nameof(OptionData))]
        public void FSharpOptionTest<T>(T data) where T : struct
        {
            var converterItem = generator.GetConverter<T?>();
            var converterOption = generator.GetConverter<FSharpOption<T>>();
            Assert.StartsWith("NullableConverter`1", converterItem.GetType().Name);
            Assert.StartsWith("UnionConverter`1", converterOption.GetType().Name);

            var a = (T?)data;
            var b = (T?)null;
            var m = FSharpOption<T>.Some(data);
            var n = FSharpOption<T>.None;

            Assert.Equal<byte>(Bytes(converterItem, a), Bytes(converterOption, m));
            Assert.Equal<byte>(BytesWithMark(converterItem, a), BytesWithMark(converterOption, m));
            Assert.Equal<byte>(Bytes(converterItem, b), Bytes(converterOption, n));
            Assert.Equal<byte>(BytesWithMark(converterItem, b), BytesWithMark(converterOption, n));
        }

        [Theory(DisplayName = "Invalid Nullable Tag (to value & to value with mark)")]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(255)]
        public void Invalid(byte tag)
        {
            var converter = generator.GetConverter<int?>();
            Assert.StartsWith("NullableConverter`1", converter.GetType().Name);

            var bytes = new byte[] { tag };
            var alpha = Assert.Throws<ArgumentException>(() => converter.Decode(bytes));
            var bravo = Assert.Throws<ArgumentException>(() => { var span = new ReadOnlySpan<byte>(bytes); _ = converter.DecodeAuto(ref span); });
            var message = $"Invalid nullable tag: {tag}, type: System.Nullable`1[System.Int32]";
            Assert.Null(alpha.ParamName);
            Assert.Null(bravo.ParamName);
            Assert.Equal(message, alpha.Message);
            Assert.Equal(message, bravo.Message);
        }
    }
}
