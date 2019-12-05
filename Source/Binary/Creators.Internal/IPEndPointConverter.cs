using System;
using System.Buffers.Binary;
using System.Net;

namespace Mikodev.Binary.Creators.Internal
{
    internal sealed class IPEndPointConverter : Converter<IPEndPoint>
    {
        public override void Encode(ref Allocator allocator, IPEndPoint item)
        {
            if (item == null)
                return;
            PrimitiveHelper.EncodeBufferWithLengthPrefix(ref allocator, item.Address.GetAddressBytes());
            AllocatorHelper.Append(ref allocator, sizeof(ushort), (ushort)item.Port, BinaryPrimitives.WriteUInt16LittleEndian);
        }

        public override IPEndPoint Decode(in ReadOnlySpan<byte> span)
        {
            if (span.IsEmpty)
                return null;
            var body = span;
            var head = PrimitiveHelper.DecodeBufferWithLengthPrefix(ref body);
            var port = BinaryPrimitives.ReadUInt16LittleEndian(body);
            var address = new IPAddress(head.ToArray());
            return new IPEndPoint(address, port);
        }
    }
}
