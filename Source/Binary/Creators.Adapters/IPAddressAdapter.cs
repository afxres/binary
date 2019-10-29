using System;
using System.Net;

namespace Mikodev.Binary.Creators.Adapters
{
    internal sealed class IPAddressAdapter : Adapter<IPAddress, ReadOnlyMemory<byte>>
    {
        public override ReadOnlyMemory<byte> Of(IPAddress item) => item?.GetAddressBytes();

        public override IPAddress To(ReadOnlyMemory<byte> item) => item.IsEmpty ? null : new IPAddress(item.ToArray());
    }
}
