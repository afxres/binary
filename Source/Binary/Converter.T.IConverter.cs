using System;

namespace Mikodev.Binary
{
    public abstract partial class Converter<T> : IConverter
    {
        void IConverter.ToBytes(ref Allocator allocator, object item) => ToBytes(ref allocator, (T)item);

        void IConverter.ToBytesWithMark(ref Allocator allocator, object item) => ToBytesWithMark(ref allocator, (T)item);

        void IConverter.ToBytesWithLengthPrefix(ref Allocator allocator, object item) => ToBytesWithLengthPrefix(ref allocator, (T)item);

        object IConverter.ToValue(in ReadOnlySpan<byte> span) => ToValue(in span);

        object IConverter.ToValueWithMark(ref ReadOnlySpan<byte> span) => ToValueWithMark(ref span);

        object IConverter.ToValueWithLengthPrefix(ref ReadOnlySpan<byte> span) => ToValueWithLengthPrefix(ref span);

        byte[] IConverter.ToBytes(object item) => ToBytes((T)item);

        object IConverter.ToValue(byte[] buffer) => ToValue(buffer);
    }
}
