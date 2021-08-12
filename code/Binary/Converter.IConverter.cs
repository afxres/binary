namespace Mikodev.Binary;

using System;

public abstract partial class Converter<T> : IConverter
{
    void IConverter.Encode(ref Allocator allocator, object item) => Encode(ref allocator, (T)item);

    void IConverter.EncodeAuto(ref Allocator allocator, object item) => EncodeAuto(ref allocator, (T)item);

    void IConverter.EncodeWithLengthPrefix(ref Allocator allocator, object item) => EncodeWithLengthPrefix(ref allocator, (T)item);

    byte[] IConverter.Encode(object item) => Encode((T)item);

    object IConverter.Decode(in ReadOnlySpan<byte> span) => Decode(in span);

    object IConverter.DecodeAuto(ref ReadOnlySpan<byte> span) => DecodeAuto(ref span);

    object IConverter.DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span) => DecodeWithLengthPrefix(ref span);

    object IConverter.Decode(byte[] buffer) => Decode(buffer);
}
