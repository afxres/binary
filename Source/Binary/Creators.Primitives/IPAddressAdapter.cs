using System;
using System.Net;

namespace Mikodev.Binary.Creators.Primitives
{
    internal sealed class IPAddressAdapter : Adapter<IPAddress, ReadOnlyMemory<byte>>
    {
        public override ReadOnlyMemory<byte> OfValue(IPAddress item) => item?.GetAddressBytes();

        public override IPAddress ToValue(ReadOnlyMemory<byte> item) => item.IsEmpty ? null : new IPAddress(item.ToArray());
    }
}
