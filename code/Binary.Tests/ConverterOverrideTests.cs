using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace Mikodev.Binary.Tests
{
    public class ConverterOverrideTests
    {
        private Encode<T> TestEncoder<T>(Converter<T> converter, string encoderName)
        {
            var type = typeof(Converter<T>);
            var encoderField = type.GetField("encoder", BindingFlags.Instance | BindingFlags.NonPublic);
            var encoder = encoderField.GetValue(converter);
            var encoderType = encoder.GetType();
            Assert.Equal(encoderName, encoderType.Name);
            var methodInfo = encoderType.GetMethod("EncodeAuto", BindingFlags.Instance | BindingFlags.Public);
            return (Encode<T>)Delegate.CreateDelegate(typeof(Encode<T>), encoder, methodInfo);
        }

        private Decode<T> TestDecoder<T>(Converter<T> converter, string decoderName)
        {
            var type = typeof(Converter<T>);
            var decoderField = type.GetField("decoder", BindingFlags.Instance | BindingFlags.NonPublic);
            var decoder = decoderField.GetValue(converter);
            var decoderType = decoder.GetType();
            Assert.Equal(decoderName, decoderType.Name);
            var methodInfo = decoderType.GetMethod("DecodeAuto", BindingFlags.Instance | BindingFlags.Public);
            return (Decode<T>)Delegate.CreateDelegate(typeof(Decode<T>), decoder, methodInfo);
        }

        private class FakeConverter<T> : Converter<T>
        {
            public FakeConverter(int length) : base(length) { }

            public override void Encode(ref Allocator allocator, T item) => throw new NotSupportedException();

            public override T Decode(in ReadOnlySpan<byte> span) => throw new NotSupportedException();
        }

        [Theory(DisplayName = "Converter")]
        [InlineData(0, "VariableEncoder`1")]
        [InlineData(4, "ConstantEncoder`1")]
        public void Encoder(int length, string encoder)
        {
            _ = TestEncoder(new FakeConverter<int>(length), encoder);
        }

        private class FakeConverterOverrideEncodeWithLengthPrefixMethod<T> : Converter<T>
        {
            public FakeConverterOverrideEncodeWithLengthPrefixMethod(int length) : base(length) { }

            public override void Encode(ref Allocator allocator, T item) => throw new NotSupportedException();

            public override T Decode(in ReadOnlySpan<byte> span) => throw new NotSupportedException();

            public override void EncodeWithLengthPrefix(ref Allocator allocator, T item) => throw new NotSupportedException();
        }

        [Theory(DisplayName = "Converter Overridden 'EncodeWithLengthPrefix'")]
        [InlineData(0, "VariableOverriddenEncoder`1")]
        [InlineData(4, "ConstantEncoder`1")]
        public void OverrideEncodeWithLengthPrefixMethod(int length, string encoder)
        {
            _ = TestEncoder(new FakeConverterOverrideEncodeWithLengthPrefixMethod<int>(length), encoder);
        }

        [Theory(DisplayName = "Converter")]
        [InlineData(0, "VariableDecoder`1")]
        [InlineData(4, "ConstantDecoder`1")]
        public void Decoder(int length, string decoder)
        {
            _ = TestDecoder(new FakeConverter<int>(length), decoder);
        }

        private class FakeConverterOverrideDecodeWithLengthPrefixMethod<T> : Converter<T>
        {
            public FakeConverterOverrideDecodeWithLengthPrefixMethod(int length) : base(length) { }

            public override void Encode(ref Allocator allocator, T item) => throw new NotSupportedException();

            public override T Decode(in ReadOnlySpan<byte> span) => throw new NotSupportedException();

            public override T DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span) => throw new NotSupportedException();
        }

        [Theory(DisplayName = "Converter Overridden 'DecodeWithLengthPrefix'")]
        [InlineData(0, "VariableOverriddenDecoder`1")]
        [InlineData(4, "ConstantDecoder`1")]
        public void OverrideDecodeWithLengthPrefixMethod(int length, string decoder)
        {
            _ = TestDecoder(new FakeConverterOverrideDecodeWithLengthPrefixMethod<int>(length), decoder);
        }

        private class StringConverterOverrideBothLengthPrefixMethods : Converter<string>
        {
            public List<string> CallList { get; } = new List<string>();

            public override void Encode(ref Allocator allocator, string item) => throw new NotSupportedException();

            public override string Decode(in ReadOnlySpan<byte> span) => throw new NotSupportedException();

            public override void EncodeWithLengthPrefix(ref Allocator allocator, string item)
            {
                var span = item.AsSpan();
                PrimitiveHelper.EncodeStringWithLengthPrefix(ref allocator, span);
                CallList.Add($"EncodeWithLengthPrefix {item}");
            }

            public override string DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span)
            {
                var result = PrimitiveHelper.DecodeStringWithLengthPrefix(ref span);
                CallList.Add($"DecodeWithLengthPrefix {result}");
                return result;
            }
        }

        private delegate void Encode<in T>(ref Allocator allocator, T item);

        private delegate T Decode<out T>(ref ReadOnlySpan<byte> span);

        [Theory(DisplayName = "String Converter Overridden '{*}WithLengthPrefix'")]
        [InlineData("alpha")]
        [InlineData("Hello, 世界!")]
        public void StringConverterOverrideLengthPrefixMethods(string item)
        {
            var converter = new StringConverterOverrideBothLengthPrefixMethods();
            var encoder = TestEncoder(converter, "VariableOverriddenEncoder`1");
            var decoder = TestDecoder(converter, "VariableOverriddenDecoder`1");
            Assert.Empty(converter.CallList);
            var allocator = new Allocator();
            encoder.Invoke(ref allocator, item);
            var span = new ReadOnlySpan<byte>(allocator.ToArray());
            var result = decoder.Invoke(ref span);
            Assert.Equal(0, span.Length);
            Assert.Equal(item, result);
            var list = new[] { $"EncodeWithLengthPrefix {item}", $"DecodeWithLengthPrefix {item}" };
            Assert.Equal(list, converter.CallList);
        }
    }
}
