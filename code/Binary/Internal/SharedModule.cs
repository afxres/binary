namespace Mikodev.Binary.Internal;

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

internal static class SharedModule
{
    internal static readonly Encoding Encoding = Encoding.UTF8;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int GetMaxByteCount(ReadOnlySpan<char> span, Encoding encoding)
    {
        Debug.Assert(encoding is not null);
        var length = span.Length;
        if (length is 0)
            return 0;
        const int Limits = 32;
        if ((uint)length <= Limits && ReferenceEquals(encoding, Encoding))
            return (length + 1) * 3;
        return encoding.GetByteCount(span);
    }
}
