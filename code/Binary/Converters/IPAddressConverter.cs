﻿namespace Mikodev.Binary.Converters;

using Mikodev.Binary.Internal;
using System;
using System.Net;

internal sealed class IPAddressConverter : Converter<IPAddress?>
{
    private static void EncodeInternal(ref Allocator allocator, IPAddress? item)
    {
        if (item is null)
            return;
        SharedModule.Encode(ref allocator, item, SharedModule.SizeOf(item));
    }

    private static void EncodeWithLengthPrefixInternal(ref Allocator allocator, IPAddress? item)
    {
        if (item is null)
        {
            Converter.Encode(ref allocator, 0);
        }
        else
        {
            var size = SharedModule.SizeOf(item);
            Converter.Encode(ref allocator, size);
            SharedModule.Encode(ref allocator, item, size);
        }
    }

    private static IPAddress? DecodeInternal(ReadOnlySpan<byte> span)
    {
        if (span.Length is 0)
            return null;
        return new IPAddress(span);
    }

    private static IPAddress? DecodeWithLengthPrefixInternal(ref ReadOnlySpan<byte> span)
    {
        return DecodeInternal(Converter.DecodeWithLengthPrefix(ref span));
    }

    public override void Encode(ref Allocator allocator, IPAddress? item) => EncodeInternal(ref allocator, item);

    public override void EncodeAuto(ref Allocator allocator, IPAddress? item) => EncodeWithLengthPrefixInternal(ref allocator, item);

    public override void EncodeWithLengthPrefix(ref Allocator allocator, IPAddress? item) => EncodeWithLengthPrefixInternal(ref allocator, item);

    public override IPAddress? Decode(in ReadOnlySpan<byte> span) => DecodeInternal(span);

    public override IPAddress? DecodeAuto(ref ReadOnlySpan<byte> span) => DecodeWithLengthPrefixInternal(ref span);

    public override IPAddress? DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span) => DecodeWithLengthPrefixInternal(ref span);
}
