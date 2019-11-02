using Microsoft.FSharp.Core;
using Microsoft.FSharp.Reflection;
using System;
using Xunit;

namespace Mikodev.Binary.Tests
{
    public class UnionTests
    {
        [CompilationMapping(SourceConstructFlags.SumType)]
        private class EmptyUnion { }

        [CompilationMapping(SourceConstructFlags.SumType)]
        public sealed class FakeUnion<T>
        {
            private readonly int tag;

            private readonly T item;

            [CompilationMapping(SourceConstructFlags.UnionCase, 0)]
            public static FakeUnion<T> NewOne(T item) => new FakeUnion<T>(item);

            public FakeUnion(T item) => this.item = item;

            public FakeUnion(T item, int tag) : this(item) => this.tag = tag;

            [CompilationMapping(SourceConstructFlags.Field, 0, 0)]
            public T Item => item;

            public int Tag => tag;
        }

        private readonly IGenerator generator = Generator.CreateDefaultBuilder()
            .AddFSharpConverterCreators()
            .Build();

        [Fact(DisplayName = "No Case")]
        public void NoCase()
        {
            var type = typeof(EmptyUnion);
            var flag = FSharpType.IsUnion(type, null);
            var items = FSharpType.GetUnionCases(type, null);
            Assert.True(flag);
            Assert.Empty(items);

            var error = Assert.Throws<ArgumentException>(() => generator.GetConverter<EmptyUnion>());
            var message = $"No available union case found, type: {type}";
            Assert.Equal(message, error.Message);
        }

        [Theory(DisplayName = "Valid Tag (to bytes & to value)")]
        [InlineData("alpha")]
        [InlineData("beta")]
        public void ValidUnionTag(string item)
        {
            var source = new FakeUnion<string>(item);
            var converter = generator.GetConverter(source);
            Assert.Equal(0, source.Tag);
            Assert.StartsWith("UnionConverter`1", converter.GetType().Name);

            var allocator = new Allocator();
            converter.Encode(ref allocator, source);
            var buffer = allocator.AsSpan().ToArray();

            var result = converter.Decode(buffer);
            Assert.False(ReferenceEquals(source, result));
            Assert.Equal(source.Tag, result.Tag);
            Assert.Equal(source.Item, result.Item);
        }

        [Theory(DisplayName = "Valid Tag (to bytes with mark & to value with mark)")]
        [InlineData("quick")]
        [InlineData("fox")]
        public void ValidUnionTagWithMark(string item)
        {
            var source = new FakeUnion<string>(item);
            var converter = generator.GetConverter(source);
            Assert.Equal(0, source.Tag);
            Assert.StartsWith("UnionConverter`1", converter.GetType().Name);

            var allocator = new Allocator();
            converter.EncodeAuto(ref allocator, source);
            var buffer = allocator.AsSpan().ToArray();

            var span = new ReadOnlySpan<byte>(buffer);
            var result = converter.DecodeAuto(ref span);
            Assert.True(span.IsEmpty);
            Assert.False(ReferenceEquals(source, result));
            Assert.Equal(source.Tag, result.Tag);
            Assert.Equal(source.Item, result.Item);
        }

        [Theory(DisplayName = "Invalid Tag (to bytes & to bytes with mark)")]
        [InlineData("some", 1)]
        [InlineData("fake", 2)]
        [InlineData("overflow", 257)]
        public void InvalidUnionTag(string item, int tag)
        {
            var source = new FakeUnion<string>(item, tag);
            var converter = generator.GetConverter(source);
            Assert.StartsWith("UnionConverter`1", converter.GetType().Name);
            var message = $"Invalid union tag '{tag}', type: {source.GetType()}";

            var alpha = Assert.Throws<ArgumentException>(() =>
            {
                var allocator = new Allocator();
                converter.Encode(ref allocator, source);
            });
            var bravo = Assert.Throws<ArgumentException>(() =>
            {
                var allocator = new Allocator();
                converter.EncodeWithLengthPrefix(ref allocator, source);
            });

            Assert.Null(alpha.ParamName);
            Assert.Null(bravo.ParamName);
            Assert.StartsWith(message, alpha.Message);
            Assert.StartsWith(message, bravo.Message);
        }
    }
}
