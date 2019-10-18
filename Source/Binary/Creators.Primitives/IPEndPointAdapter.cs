using Mikodev.Binary.Internal;
using System;
using System.Net;

namespace Mikodev.Binary.Creators.Primitives
{
    internal sealed class IPEndPointAdapter : Adapter<IPEndPoint, ReadOnlyMemory<byte>>
    {
        private readonly Converter<(IPAddress, ushort)> converter;

        public IPEndPointAdapter(Converter<(IPAddress, ushort)> converter) => this.converter = converter;

        public override ReadOnlyMemory<byte> Of(IPEndPoint item)
        {
            if (item == null)
                return default;
            return converter.Encode((item.Address, (ushort)item.Port));
        }

        public override IPEndPoint To(ReadOnlyMemory<byte> item)
        {
            if (item.IsEmpty)
                return null;
            var (address, port) = converter.Decode(item.Span);
            if (address == null)
                return ThrowHelper.ThrowNotEnoughBytes<IPEndPoint>();
            return new IPEndPoint(address, port);
        }
    }
}
