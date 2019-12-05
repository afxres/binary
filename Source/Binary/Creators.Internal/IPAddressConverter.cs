using System;
using System.Net;

namespace Mikodev.Binary.Creators.Internal
{
    internal sealed class IPAddressConverter : Converter<IPAddress>
    {
        public override void Encode(ref Allocator allocator, IPAddress item)
        {
            if (item == null)
                return;
            AllocatorHelper.Append(ref allocator, item.GetAddressBytes());
        }

        public override IPAddress Decode(in ReadOnlySpan<byte> span)
        {
            if (span.IsEmpty)
                return null;
            return new IPAddress(span.ToArray());
        }
    }
}
