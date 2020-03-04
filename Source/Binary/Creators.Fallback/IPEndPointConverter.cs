using Mikodev.Binary.Internal;
using System;
using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;

namespace Mikodev.Binary.Creators.Fallback
{
    internal sealed class IPEndPointConverter : Converter<IPEndPoint>
    {
        private static void Append(ref Allocator allocator, IPEndPoint item)
        {
            if (item is null)
                return;
            var size = item.AddressFamily == AddressFamily.InterNetwork ? 4 : 16;
            Allocator.Append(ref allocator, (byte)size);
            Allocator.AppendAction(ref allocator, size, item.Address, SharedHelper.WriteIPAddress);
            Allocator.AppendLittleEndian(ref allocator, (ushort)item.Port);
        }

        private static void AppendWithLengthPrefix(ref Allocator allocator, IPEndPoint item)
        {
            var size = item is null ? 0 : (item.AddressFamily == AddressFamily.InterNetwork ? 4 : 16) + sizeof(ushort) + 1;
            Allocator.Append(ref allocator, (byte)size);
            Append(ref allocator, item);
        }

        private static IPEndPoint Detach(ReadOnlySpan<byte> span)
        {
            if (span.IsEmpty)
                return null;
            var body = span;
            var head = PrimitiveHelper.DecodeBufferWithLengthPrefix(ref body);
            var port = BinaryPrimitives.ReadUInt16LittleEndian(body);
            var address = SharedHelper.GetIPAddress(head);
            return new IPEndPoint(address, port);
        }

        private static IPEndPoint DetachWithLengthPrefix(ref ReadOnlySpan<byte> span)
        {
            return Detach(PrimitiveHelper.DecodeBufferWithLengthPrefix(ref span));
        }

        public override void Encode(ref Allocator allocator, IPEndPoint item) => Append(ref allocator, item);

        public override void EncodeAuto(ref Allocator allocator, IPEndPoint item) => AppendWithLengthPrefix(ref allocator, item);

        public override void EncodeWithLengthPrefix(ref Allocator allocator, IPEndPoint item) => AppendWithLengthPrefix(ref allocator, item);

        public override IPEndPoint Decode(in ReadOnlySpan<byte> span) => Detach(span);

        public override IPEndPoint DecodeAuto(ref ReadOnlySpan<byte> span) => DetachWithLengthPrefix(ref span);

        public override IPEndPoint DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span) => DetachWithLengthPrefix(ref span);
    }
}
