namespace Mikodev.Binary.Creators.Isolated.Variables;

using Mikodev.Binary.Features.Contexts;
using Mikodev.Binary.Internal;
using System;
using System.Buffers.Binary;
using System.Net;

internal sealed class IPEndPointConverter : VariableConverter<IPEndPoint?, IPEndPointConverter.Functions>
{
    internal readonly struct Functions : IVariableConverterFunctions<IPEndPoint?>
    {
        public static int Limits(IPEndPoint? item)
        {
            return item is null ? 0 : 18;
        }

        public static int Append(Span<byte> span, IPEndPoint? item)
        {
            if (item is null)
                return 0;
            if (item.Address.TryWriteBytes(span.Slice(0, span.Length - sizeof(ushort)), out var actual) is false)
                ThrowHelper.ThrowTryWriteBytesFailed();
            BinaryPrimitives.WriteUInt16LittleEndian(span.Slice(actual, sizeof(ushort)), (ushort)item.Port);
            return actual + sizeof(ushort);
        }

        public static IPEndPoint? Decode(in ReadOnlySpan<byte> span)
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
    }
}
