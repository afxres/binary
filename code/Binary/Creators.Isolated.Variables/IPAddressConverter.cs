namespace Mikodev.Binary.Creators.Isolated.Variables;

using Mikodev.Binary;
using Mikodev.Binary.Features.Contexts;
using Mikodev.Binary.Internal;
using System;
using System.Net;

internal sealed class IPAddressConverter : VariableConverter<IPAddress?, IPAddressConverter.Functions>
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

    internal readonly struct Functions : IVariableConverterFunctions<IPAddress?>
    {
        public static IPAddress? Decode(in ReadOnlySpan<byte> span)
        {
            return DecodeInternal(span);
        }

        public static void Encode(ref Allocator allocator, IPAddress? item)
        {
            Allocator.Append(ref allocator, MaxLength, item, EncodeFunction);
        }

        public static void EncodeWithLengthPrefix(ref Allocator allocator, IPAddress? item)
        {
            Allocator.AppendWithLengthPrefix(ref allocator, MaxLength, item, EncodeFunction);
        }
    }
}
