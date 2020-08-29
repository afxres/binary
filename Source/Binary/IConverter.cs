using System;

namespace Mikodev.Binary
{
    public interface IConverter
    {
        int Length { get; }

        void Encode(ref Allocator allocator, object item);

        void EncodeAuto(ref Allocator allocator, object item);

        void EncodeWithLengthPrefix(ref Allocator allocator, object item);

        byte[] Encode(object item);

        object Decode(in ReadOnlySpan<byte> span);

        object DecodeAuto(ref ReadOnlySpan<byte> span);

        object DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span);

        object Decode(byte[] buffer);
    }
}
