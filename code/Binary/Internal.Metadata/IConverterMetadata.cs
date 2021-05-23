using System;
using System.Reflection;

namespace Mikodev.Binary.Internal.Metadata
{
    internal delegate void EncodeDelegate<in T>(ref Allocator allocator, T item);

    internal delegate T DecodeDelegate<out T>(ref ReadOnlySpan<byte> span);

    internal delegate T DecodePassSpanDelegate<out T>(ReadOnlySpan<byte> span);

    internal delegate T DecodeReadOnlyDelegate<out T>(in ReadOnlySpan<byte> span);

    internal interface IConverterMetadata
    {
        Type GetGenericArgument();

        MethodInfo GetMethod(string methodName);
    }
}
