using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Xunit;

namespace Mikodev.Binary.Tests
{
    public class ConverterOverrideTests
    {
        private void TestEncode<T>(Converter<T> converter, string option)
        {
            var type = typeof(Converter<T>);
            var encodeField = type.GetField("encode", BindingFlags.Instance | BindingFlags.NonPublic);
            var encode = encodeField.GetValue(converter);
            Assert.Equal(option, encode.ToString());
        }

        private void TestDecode<T>(Converter<T> converter, string option)
        {
            var type = typeof(Converter<T>);
            var decodeField = type.GetField("decode", BindingFlags.Instance | BindingFlags.NonPublic);
            var decode = decodeField.GetValue(converter);
            Assert.Equal(option, decode.ToString());
        }

        private class FakeConverter<T> : Converter<T>
        {
            public FakeConverter(int length) : base(length) { }

            public override void Encode(ref Allocator allocator, T item) => throw new NotSupportedException();

            public override T Decode(in ReadOnlySpan<byte> span) => throw new NotSupportedException();
        }

        [Theory(DisplayName = "Encode Option")]
        [InlineData(0, "Variable")]
        [InlineData(4, "Constant")]
        public void Encode(int length, string option)
        {
            TestEncode(new FakeConverter<int>(length), option);
        }

        private class FakeConverterOverrideEncodeWithLengthPrefixMethod<T> : Converter<T>
        {
            public FakeConverterOverrideEncodeWithLengthPrefixMethod(int length) : base(length) { }

            public override void Encode(ref Allocator allocator, T item) => throw new NotSupportedException();

            public override T Decode(in ReadOnlySpan<byte> span) => throw new NotSupportedException();

            public override void EncodeWithLengthPrefix(ref Allocator allocator, T item) => throw new NotSupportedException();
        }

        [Theory(DisplayName = "Override 'EncodeWithLengthPrefix'")]
        [InlineData(0, "VariableOverride")]
        [InlineData(4, "Constant")]
        public void OverrideEncodeWithLengthPrefixMethod(int length, string option)
        {
            TestEncode(new FakeConverterOverrideEncodeWithLengthPrefixMethod<int>(length), option);
        }

        [Theory(DisplayName = "Decode Option")]
        [InlineData(0, "Variable")]
        [InlineData(4, "Constant")]
        public void Decode(int length, string option)
        {
            TestDecode(new FakeConverter<int>(length), option);
        }

        private class FakeConverterOverrideDecodeWithLengthPrefixMethod<T> : Converter<T>
        {
            public FakeConverterOverrideDecodeWithLengthPrefixMethod(int length) : base(length) { }

            public override void Encode(ref Allocator allocator, T item) => throw new NotSupportedException();

            public override T Decode(in ReadOnlySpan<byte> span) => throw new NotSupportedException();

            public override T DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span) => throw new NotSupportedException();
        }

        [Theory(DisplayName = "Override 'DecodeWithLengthPrefix'")]
        [InlineData(0, "VariableOverride")]
        [InlineData(4, "Constant")]
        public void OverrideDecodeWithLengthPrefixMethod(int length, string option)
        {
            TestDecode(new FakeConverterOverrideDecodeWithLengthPrefixMethod<int>(length), option);
        }

        private class StringConverterOverrideBothLengthPrefixMethods : Converter<string>
        {
            public List<string> CallList { get; } = new List<string>();

            public override void Encode(ref Allocator allocator, string item) => throw new NotSupportedException();

            public override string Decode(in ReadOnlySpan<byte> span) => throw new NotSupportedException();

            public override void EncodeWithLengthPrefix(ref Allocator allocator, string item)
            {
                var span = item.AsSpan();
                Allocator.AppendWithLengthPrefix(ref allocator, span, Encoding.UTF8);
                CallList.Add($"EncodeWithLengthPrefix {item}");
            }

            public override string DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span)
            {
                var result = Encoding.UTF8.GetString(Converter.DecodeWithLengthPrefix(ref span));
                CallList.Add($"DecodeWithLengthPrefix {result}");
                return result;
            }
        }

        [Theory(DisplayName = "String Converter Override '{*}WithLengthPrefix'")]
        [InlineData("alpha")]
        [InlineData("Hello, 世界!")]
        public void StringConverterOverrideLengthPrefixMethods(string item)
        {
            var converter = new StringConverterOverrideBothLengthPrefixMethods();
            TestEncode(converter, "VariableOverride");
            TestDecode(converter, "VariableOverride");
            Assert.Empty(converter.CallList);
            var allocator = new Allocator();
            converter.EncodeAuto(ref allocator, item);
            var span = new ReadOnlySpan<byte>(allocator.ToArray());
            var result = converter.DecodeAuto(ref span);
            Assert.Equal(0, span.Length);
            Assert.Equal(item, result);
            var list = new[] { $"EncodeWithLengthPrefix {item}", $"DecodeWithLengthPrefix {item}" };
            Assert.Equal(list, converter.CallList);
        }
    }
}
