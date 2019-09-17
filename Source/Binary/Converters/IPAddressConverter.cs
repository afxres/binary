using Mikodev.Binary.Abstractions;
using Mikodev.Binary.Internal;
using System;
using System.Net;

namespace Mikodev.Binary.Converters
{
    internal sealed class IPAddressConverter : VariableConverter<IPAddress>
    {
        public override void ToBytes(ref Allocator allocator, IPAddress item)
        {
            if (item == null)
                return;
            allocator.Append(item.GetAddressBytes());
        }

        public override IPAddress ToValue(in ReadOnlySpan<byte> span)
        {
            var byteCount = span.Length;
            if (byteCount == 0)
                return null;
            if (byteCount != 4 && byteCount != 16)
                return ThrowHelper.ThrowInvalidBytes<IPAddress>();
            return new IPAddress(span.ToArray());
        }
    }
}
