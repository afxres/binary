﻿using Mikodev.Binary.Internal;
using System;
using System.Net;

namespace Mikodev.Binary.Converters
{
    internal sealed class IPEndPointConverter : Converter<IPEndPoint>
    {
        private static void EncodeInternal(ref Allocator allocator, IPEndPoint item)
        {
            if (item is null)
                return;
            SharedHelper.EncodeIPAddress(ref allocator, item.Address);
            MemoryHelper.EncodeLittleEndian(ref allocator, (short)(ushort)item.Port);
        }

        private static void EncodeWithLengthPrefixInternal(ref Allocator allocator, IPEndPoint item)
        {
            var size = item is null ? 0 : SharedHelper.SizeOfIPAddress(item.Address) + sizeof(ushort);
            PrimitiveHelper.EncodeNumber(ref allocator, size);
            EncodeInternal(ref allocator, item);
        }

        private static IPEndPoint DecodeInternal(ReadOnlySpan<byte> span)
        {
            if (span.IsEmpty)
                return null;
            var size = span.Length - sizeof(ushort);
            var data = new IPAddress(span.Slice(0, size));
            var port = MemoryHelper.DecodeLittleEndian<short>(span.Slice(size));
            return new IPEndPoint(data, (ushort)port);
        }

        private static IPEndPoint DecodeWithLengthPrefixInternal(ref ReadOnlySpan<byte> span)
        {
            return DecodeInternal(PrimitiveHelper.DecodeBufferWithLengthPrefix(ref span));
        }

        public override void Encode(ref Allocator allocator, IPEndPoint item) => EncodeInternal(ref allocator, item);

        public override void EncodeAuto(ref Allocator allocator, IPEndPoint item) => EncodeWithLengthPrefixInternal(ref allocator, item);

        public override void EncodeWithLengthPrefix(ref Allocator allocator, IPEndPoint item) => EncodeWithLengthPrefixInternal(ref allocator, item);

        public override IPEndPoint Decode(in ReadOnlySpan<byte> span) => DecodeInternal(span);

        public override IPEndPoint DecodeAuto(ref ReadOnlySpan<byte> span) => DecodeWithLengthPrefixInternal(ref span);

        public override IPEndPoint DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span) => DecodeWithLengthPrefixInternal(ref span);
    }
}