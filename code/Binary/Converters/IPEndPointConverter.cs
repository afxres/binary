namespace Mikodev.Binary.Converters;

using Mikodev.Binary.Features.Contexts;
using Mikodev.Binary.Internal;
using System;
using System.Buffers.Binary;
using System.Net;

internal sealed class IPEndPointConverter : VariablePrefixEncodeConverter<IPEndPoint?, IPEndPointConverter.Functions>
{
    private const int MaxLength = 18;

    private static readonly AllocatorWriter<IPEndPoint?> EncodeFunction;

    static IPEndPointConverter()
    {
        static int Invoke(Span<byte> span, IPEndPoint? item)
        {
            if (item is null)
                return 0;
            if (item.Address.TryWriteBytes(span.Slice(0, span.Length - sizeof(ushort)), out var actual) is false)
                ThrowHelper.ThrowTryWriteBytesFailed();
            BinaryPrimitives.WriteUInt16LittleEndian(span.Slice(actual, sizeof(ushort)), (ushort)item.Port);
            return actual + sizeof(ushort);
        };
        EncodeFunction = Invoke;
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

    internal struct Functions : IVariablePrefixEncodeConverterFunctions<IPEndPoint?>
    {
        public static IPEndPoint? Decode(in ReadOnlySpan<byte> span)
        {
            return DecodeInternal(span);
        }

        public static void Encode(ref Allocator allocator, IPEndPoint? item)
        {
            Allocator.Append(ref allocator, MaxLength, item, EncodeFunction);
        }

        public static void EncodeWithLengthPrefix(ref Allocator allocator, IPEndPoint? item)
        {
            Allocator.AppendWithLengthPrefix(ref allocator, MaxLength, item, EncodeFunction);
        }
    }
}
