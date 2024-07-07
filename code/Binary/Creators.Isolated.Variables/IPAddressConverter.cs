namespace Mikodev.Binary.Creators.Isolated.Variables;

using Mikodev.Binary.Features.Contexts;
using Mikodev.Binary.Internal;
using System;
using System.Net;

internal sealed class IPAddressConverter : VariableConverter<IPAddress?, IPAddressConverter.Functions>
{
    internal readonly struct Functions : IVariableConverterFunctions<IPAddress?>
    {
        public static int Limits(IPAddress? item)
        {
            return item is null ? 0 : 16;
        }

        public static int Append(Span<byte> span, IPAddress? item)
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
