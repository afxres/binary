namespace Mikodev.Binary.Converters;

using Mikodev.Binary.Features.Contexts;
using Mikodev.Binary.Internal;
using System;
using System.Net;

internal sealed class IPAddressConverter : VariableWriterEncodeConverter<IPAddress?, IPAddressConverter.Functions>
{
    internal struct Functions : IVariableWriterEncodeConverterFunctions<IPAddress?>
    {
        private const int MaxLength = 16;

        public static int GetMaxLength(IPAddress? item)
        {
            return MaxLength;
        }

        public static int Encode(Span<byte> span, IPAddress? item)
        {
            if (item is null)
                return 0;
            if (item.TryWriteBytes(span, out var actual) is false)
                ThrowHelper.ThrowTryWriteBytesFailed();
            return actual;
        }

        public static IPAddress? Decode(in ReadOnlySpan<byte> span)
        {
            if (span.Length is 0)
                return null;
            return new IPAddress(span);
        }
    }
}
