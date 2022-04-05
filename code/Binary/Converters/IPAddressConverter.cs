namespace Mikodev.Binary.Converters;

using System;
using System.Diagnostics;
using System.Net;

internal sealed class IPAddressConverter : Converter<IPAddress?>
{
    private const int MaxLength = 16;

    private static readonly AllocatorSpanAction<IPAddress?> EncodeAction;

    static IPAddressConverter()
    {
        static int Invoke(Span<byte> span, IPAddress? item)
        {
            if (item is null)
                return 0;
            var flag = item.TryWriteBytes(span, out var actual);
            Debug.Assert(flag);
            Debug.Assert(actual is 4 or 16);
            return actual;
        };
        EncodeAction = Invoke;
    }

    private static IPAddress? DecodeInternal(ReadOnlySpan<byte> span)
    {
        if (span.Length is 0)
            return null;
        return new IPAddress(span);
    }

    public override void Encode(ref Allocator allocator, IPAddress? item) => Allocator.Append(ref allocator, MaxLength, item, EncodeAction);

    public override void EncodeAuto(ref Allocator allocator, IPAddress? item) => Allocator.AppendWithLengthPrefix(ref allocator, MaxLength, item, EncodeAction);

    public override void EncodeWithLengthPrefix(ref Allocator allocator, IPAddress? item) => Allocator.AppendWithLengthPrefix(ref allocator, MaxLength, item, EncodeAction);

    public override IPAddress? Decode(in ReadOnlySpan<byte> span) => DecodeInternal(span);

    public override IPAddress? DecodeAuto(ref ReadOnlySpan<byte> span) => DecodeInternal(Converter.DecodeWithLengthPrefix(ref span));

    public override IPAddress? DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span) => DecodeInternal(Converter.DecodeWithLengthPrefix(ref span));
}
