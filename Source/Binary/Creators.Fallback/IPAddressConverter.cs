using System;
using System.Net;

namespace Mikodev.Binary.Creators.Fallback
{
    internal sealed class IPAddressConverter : Converter<IPAddress>
    {
#if NETNEW
        private static readonly AllocatorAction<IPAddress> WriteIPAddress = (span, data) => data.TryWriteBytes(span, out _);
#endif

        private static void Append(ref Allocator allocator, IPAddress item)
        {
            if (item is null)
                return;
#if NETNEW
            var size = item.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ? 4 : 16;
            AllocatorHelper.Append(ref allocator, size, item, WriteIPAddress);
#else
            AllocatorHelper.Append(ref allocator, item.GetAddressBytes());
#endif
        }

        private static void AppendWithLengthPrefix(ref Allocator allocator, IPAddress item)
        {
            var size = item is null ? 0 : item.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ? 4 : 16;
            Allocator.Append(ref allocator, (byte)size);
            if (size == 0)
                return;
            Append(ref allocator, item);
        }

        private static IPAddress Detach(ReadOnlySpan<byte> span)
        {
            if (span.IsEmpty)
                return null;
#if NETNEW
            return new IPAddress(span);
#else
            return new IPAddress(span.ToArray());
#endif
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
