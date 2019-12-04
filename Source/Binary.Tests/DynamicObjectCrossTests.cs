using System;
using Xunit;

namespace Mikodev.Binary.Tests
{
    public class DynamicObjectCrossTests
    {
        private sealed class EventStringConverter : Converter<string>
        {
            public event Func<string, string> OnDecode;

            public override void Encode(ref Allocator allocator, string item)
            {
                PrimitiveHelper.EncodeString(ref allocator, item.AsSpan());
            }

            public override string Decode(in ReadOnlySpan<byte> span)
            {
                var result = PrimitiveHelper.DecodeString(span);
                result = OnDecode?.Invoke(result) ?? result;
                return result;
            }
        }

        [Fact(DisplayName = "Multiple Tokens With Difference Generators")]
        public void MultipleGenerator()
        {
            var source = new { text = "test" };
            var generator = Generator.CreateDefault();
            var buffer = generator.Encode(source);

            var converter01 = new EventStringConverter();
            var converter02 = new EventStringConverter();
            converter01.OnDecode += text => text + ", 01";
            converter02.OnDecode += text => text + ", 02";
            var generator01 = Generator.CreateDefaultBuilder().AddConverter(converter01).Build();
            var generator02 = Generator.CreateDefaultBuilder().AddConverter(converter02).Build();
            var token01 = new Token(generator01, buffer);
            var token02 = new Token(generator02, buffer);
            var d01 = (dynamic)token01;
            var d02 = (dynamic)token02;
            var text01 = (string)d01.text;
            var text02 = (string)d02.text;
            Assert.Equal("test, 01", text01);
            Assert.Equal("test, 02", text02);
        }
    }
}
