namespace Mikodev.Binary.Converters;

using Mikodev.Binary.Internal;
using System;
using System.Net;

internal sealed class IPAddressConverter : Converter<IPAddress?>
{
    private const int MaxLength = 16;

    private static readonly AllocatorWriter<IPAddress?> EncodeFunction;

    static IPAddressConverter()
    {
        static int Invoke(Span<byte> span, IPAddress? item)
        {
            if (item is null)
                return 0;
            if (item.TryWriteBytes(span, out var actual) is false)
                ThrowHelper.ThrowTryWriteBytesFailed();
            return actual;
        };
        EncodeFunction = Invoke;
    }

    private static IPAddress? DecodeInternal(ReadOnlySpan<byte> span)
    {
        if (span.Length is 0)
            return null;
        return new IPAddress(span);
    }

    public override void Encode(ref Allocator allocator, IPAddress? item) => Allocator.Append(ref allocator, MaxLength, item, EncodeFunction);

    public override void EncodeAuto(ref Allocator allocator, IPAddress? item) => Allocator.AppendWithLengthPrefix(ref allocator, MaxLength, item, EncodeFunction);

    public override void EncodeWithLengthPrefix(ref Allocator allocator, IPAddress? item) => Allocator.AppendWithLengthPrefix(ref allocator, MaxLength, item, EncodeFunction);

    public override IPAddress? Decode(in ReadOnlySpan<byte> span) => DecodeInternal(span);

    public override IPAddress? DecodeAuto(ref ReadOnlySpan<byte> span) => DecodeInternal(Converter.DecodeWithLengthPrefix(ref span));

    public override IPAddress? DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span) => DecodeInternal(Converter.DecodeWithLengthPrefix(ref span));
}
