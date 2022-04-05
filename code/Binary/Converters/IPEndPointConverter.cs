namespace Mikodev.Binary.Converters;

using Mikodev.Binary.Internal;
using System;
using System.Net;

internal sealed class IPEndPointConverter : Converter<IPEndPoint?>
{
    private static void EncodeInternal(ref Allocator allocator, IPEndPoint? item)
    {
        if (item is null)
            return;
        var address = item.Address;
        SharedModule.Encode(ref allocator, address, SharedModule.SizeOf(address), item.Port);
    }

    private static void EncodeWithLengthPrefixInternal(ref Allocator allocator, IPEndPoint? item)
    {
        if (item is null)
        {
            Converter.Encode(ref allocator, 0);
        }
        else
        {
            var address = item.Address;
            var addressSize = SharedModule.SizeOf(address);
            Converter.Encode(ref allocator, addressSize + sizeof(ushort));
            SharedModule.Encode(ref allocator, address, addressSize, item.Port);
        }
    }

    private static IPEndPoint? DecodeInternal(ReadOnlySpan<byte> span)
    {
        var limits = span.Length;
        if (limits is 0)
            return null;
        var offset = limits - sizeof(ushort);
        if (offset < 0)
            ThrowHelper.ThrowNotEnoughBytes();
        var header = new IPAddress(span.Slice(0, offset));
        var number = LittleEndian.Decode<short>(span.Slice(offset));
        return new IPEndPoint(header, (ushort)number);
    }

    private static IPEndPoint? DecodeWithLengthPrefixInternal(ref ReadOnlySpan<byte> span)
    {
        return DecodeInternal(Converter.DecodeWithLengthPrefix(ref span));
    }

    public override void Encode(ref Allocator allocator, IPEndPoint? item) => EncodeInternal(ref allocator, item);

    public override void EncodeAuto(ref Allocator allocator, IPEndPoint? item) => EncodeWithLengthPrefixInternal(ref allocator, item);

    public override void EncodeWithLengthPrefix(ref Allocator allocator, IPEndPoint? item) => EncodeWithLengthPrefixInternal(ref allocator, item);

    public override IPEndPoint? Decode(in ReadOnlySpan<byte> span) => DecodeInternal(span);

    public override IPEndPoint? DecodeAuto(ref ReadOnlySpan<byte> span) => DecodeWithLengthPrefixInternal(ref span);

    public override IPEndPoint? DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span) => DecodeWithLengthPrefixInternal(ref span);
}
