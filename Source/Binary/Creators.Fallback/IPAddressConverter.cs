using Mikodev.Binary.Internal;
using System;
using System.Net;
using System.Net.Sockets;

namespace Mikodev.Binary.Creators.Fallback
{
    internal sealed class IPAddressConverter : Converter<IPAddress>
    {
        private static void Append(ref Allocator allocator, IPAddress item)
        {
            if (item is null)
                return;
            var size = item.AddressFamily == AddressFamily.InterNetwork ? 4 : 16;
            Allocator.AppendAction(ref allocator, size, item, SharedHelper.WriteIPAddress);
        }

        private static void AppendWithLengthPrefix(ref Allocator allocator, IPAddress item)
        {
            var size = item is null ? 0 : item.AddressFamily == AddressFamily.InterNetwork ? 4 : 16;
            Allocator.Append(ref allocator, (byte)size);
            if (size == 0)
                return;
            Append(ref allocator, item);
        }

        private static IPAddress Detach(ReadOnlySpan<byte> span)
        {
            if (span.IsEmpty)
                return null;
            return SharedHelper.GetIPAddress(span);
        }

        private static IPAddress DetachWithLengthPrefix(ref ReadOnlySpan<byte> span)
        {
            return Detach(PrimitiveHelper.DecodeBufferWithLengthPrefix(ref span));
        }

        public override void Encode(ref Allocator allocator, IPAddress item) => Append(ref allocator, item);

        public override void EncodeAuto(ref Allocator allocator, IPAddress item) => AppendWithLengthPrefix(ref allocator, item);

        public override void EncodeWithLengthPrefix(ref Allocator allocator, IPAddress item) => AppendWithLengthPrefix(ref allocator, item);

        public override IPAddress Decode(in ReadOnlySpan<byte> span) => Detach(span);

        public override IPAddress DecodeAuto(ref ReadOnlySpan<byte> span) => DetachWithLengthPrefix(ref span);

        public override IPAddress DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span) => DetachWithLengthPrefix(ref span);
    }
}
