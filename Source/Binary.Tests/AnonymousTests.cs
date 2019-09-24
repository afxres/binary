using System;
using Xunit;

namespace Mikodev.Binary.Tests
{
    public class AnonymousTests
    {
        private readonly Generator generator = new Generator();

        [Fact(DisplayName = "Get Converter For Anonymous Type")]
        public void GetConverter()
        {
            var a = new { name = "Tom", age = 20 };
            var converter = generator.GetConverter(a);
            Assert.NotNull(converter);
            Assert.Equal(0, converter.Length);
        }

        [Fact(DisplayName = "Get Converter For Empty Anonymous Type")]
        public void Empty()
        {
            var a = new { };
            var error = Assert.Throws<ArgumentException>(() => generator.GetConverter(a));
            Assert.Contains("No available property found", error.Message);
        }

        [Fact(DisplayName = "To Bytes Then To Value")]
        public void Convert()
        {
            var a = new { id = "some", data = new { name = "Bob" } };
            var bytes = generator.ToBytes(a);
            Assert.NotEmpty(bytes);
            var value = generator.ToValue(bytes, a);
            Assert.False(ReferenceEquals(a, value));
            Assert.Equal(a, value);
        }

        [Fact(DisplayName = "Value From Empty Bytes")]
        public void ValueFromEmptyBytes()
        {
            var bytes = Array.Empty<byte>();
            var value = generator.ToValue(bytes, new { key = default(string) });
            Assert.Null(value);
        }

        [Fact(DisplayName = "Null Value To Bytes")]
        public void NullValueToBytes()
        {
            static T DefaultOf<T>(T _) => default;

            var a = DefaultOf(new { key = default(string), value = default(int) });
            Assert.Null(a);
            var bytes = generator.ToBytes(a);
            Assert.Empty(bytes);
        }

        [Fact(DisplayName = "Token As Value")]
        public void TokenAs()
        {
            var a = new { guid = Guid.NewGuid(), inner = new { name = "Pro C# ...", price = 51.2 } };
            var bytes = generator.ToBytes(a);
            var token = generator.AsToken(bytes);
            var value = token.As(a);
            Assert.False(ReferenceEquals(a, value));
            Assert.Equal(a, value);

            var inner = token["inner"].As(a.inner);
            Assert.False(ReferenceEquals(a.inner, inner));
            Assert.Equal(a.inner, inner);
        }
    }
}
