namespace Mikodev.Binary.Converters;

using Mikodev.Binary.Internal;
using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Net;

internal sealed class IPEndPointConverter : Converter<IPEndPoint?>
{
    private const int MaxLength = 18;

    private static readonly AllocatorWriter<IPEndPoint?> EncodeAction;

    static IPEndPointConverter()
    {
        static int Invoke(Span<byte> span, IPEndPoint? item)
        {
            if (item is null)
                return 0;
            var flag = item.Address.TryWriteBytes(span, out var actual);
            Debug.Assert(flag);
            Debug.Assert(actual is 4 or 16);
            BinaryPrimitives.WriteUInt16LittleEndian(span.Slice(actual, sizeof(ushort)), (ushort)item.Port);
            return actual + sizeof(ushort);
        };
        EncodeAction = Invoke;
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
        var number = BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(offset));
        return new IPEndPoint(header, number);
    }

    public override void Encode(ref Allocator allocator, IPEndPoint? item) => Allocator.Append(ref allocator, MaxLength, item, EncodeAction);

    public override void EncodeAuto(ref Allocator allocator, IPEndPoint? item) => Allocator.AppendWithLengthPrefix(ref allocator, MaxLength, item, EncodeAction);

    public override void EncodeWithLengthPrefix(ref Allocator allocator, IPEndPoint? item) => Allocator.AppendWithLengthPrefix(ref allocator, MaxLength, item, EncodeAction);

    public override IPEndPoint? Decode(in ReadOnlySpan<byte> span) => DecodeInternal(span);

    public override IPEndPoint? DecodeAuto(ref ReadOnlySpan<byte> span) => DecodeInternal(Converter.DecodeWithLengthPrefix(ref span));

    public override IPEndPoint? DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span) => DecodeInternal(Converter.DecodeWithLengthPrefix(ref span));
}
