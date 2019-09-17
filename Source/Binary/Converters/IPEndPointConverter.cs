using Mikodev.Binary.Abstractions;
using Mikodev.Binary.Internal;
using System;
using System.Net;
using System.Runtime.InteropServices;
using uint16 = System.UInt16;

namespace Mikodev.Binary.Converters
{
    internal sealed class IPEndPointConverter : VariableConverter<IPEndPoint>
    {
        public override void ToBytes(ref Allocator allocator, IPEndPoint item)
        {
            if (item == null)
                return;
            var port = (uint16)item.Port;
            var address = item.Address.GetAddressBytes();
            var addressCount = address.Length;
            ref var target = ref allocator.AllocateReference(addressCount + sizeof(uint16));
            ref var source = ref address[0];
            Memory.Copy(ref target, ref source, addressCount);
            Endian<uint16>.Set(ref Memory.Add(ref target, addressCount), port);
        }

        public override IPEndPoint ToValue(in ReadOnlySpan<byte> span)
        {
            var byteCount = span.Length;
            if (byteCount == 0)
                return null;
            var addressCount = byteCount - sizeof(uint16);
            if (addressCount != 4 && addressCount != 16)
                return ThrowHelper.ThrowInvalidBytes<IPEndPoint>();
            var address = new byte[addressCount];
            ref var source = ref MemoryMarshal.GetReference(span);
            ref var target = ref address[0];
            Memory.Copy(ref target, ref source, addressCount);
            var port = Endian<uint16>.Get(ref Memory.Add(ref source, addressCount));
            return new IPEndPoint(new IPAddress(address), port);
        }
    }
}
