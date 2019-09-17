using System;

namespace Mikodev.Binary
{
    public interface IConverter
    {
        void ToBytes(ref Allocator allocator, object item);

        void ToBytesWithMark(ref Allocator allocator, object item);

        void ToBytesWithLengthPrefix(ref Allocator allocator, object item);

        object ToValue(in ReadOnlySpan<byte> span);

        object ToValueWithMark(ref ReadOnlySpan<byte> span);

        object ToValueWithLengthPrefix(ref ReadOnlySpan<byte> span);

        byte[] ToBytes(object item);

        object ToValue(byte[] buffer);
    }
}
