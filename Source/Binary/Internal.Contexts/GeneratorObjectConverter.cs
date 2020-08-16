using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Mikodev.Binary.Internal.Contexts
{
    internal sealed class GeneratorObjectConverter : Converter<object>
    {
        private readonly IGenerator generator;

        public GeneratorObjectConverter(IGenerator generator) => this.generator = generator;

        [DebuggerStepThrough, DoesNotReturn]
        private static void ThrowNull() => throw new ArgumentException("Can not get type of null object.");

        [DebuggerStepThrough, DoesNotReturn]
        private static void ThrowEncode() => throw new NotSupportedException($"Can not encode object, type: {typeof(object)}");

        [DebuggerStepThrough, DoesNotReturn]
        private static object ThrowDecode() => throw new NotSupportedException($"Can not decode object, type: {typeof(object)}");

        private IConverter QueryConverter(object item)
        {
            if (item is null)
                ThrowNull();
            var type = item.GetType();
            if (type == typeof(object))
                ThrowEncode();
            return generator.GetConverter(type);
        }

        public override byte[] Encode(object item) => QueryConverter(item).Encode(item);

        public override void Encode(ref Allocator allocator, object item) => QueryConverter(item).Encode(ref allocator, item);

        public override void EncodeAuto(ref Allocator allocator, object item) => QueryConverter(item).EncodeWithLengthPrefix(ref allocator, item);

        public override void EncodeWithLengthPrefix(ref Allocator allocator, object item) => QueryConverter(item).EncodeWithLengthPrefix(ref allocator, item);

        public override object Decode(byte[] buffer) => ThrowDecode();

        public override object Decode(in ReadOnlySpan<byte> span) => ThrowDecode();

        public override object DecodeAuto(ref ReadOnlySpan<byte> span) => ThrowDecode();

        public override object DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span) => ThrowDecode();
    }
}
