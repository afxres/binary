namespace Mikodev.Binary.Internal.Metadata;

using System;
using System.Reflection;

internal delegate T DecodeDelegate<out T>(ref ReadOnlySpan<byte> span);

internal delegate T DecodePassSpanDelegate<out T>(ReadOnlySpan<byte> span);

internal delegate T DecodeReadOnlyDelegate<out T>(in ReadOnlySpan<byte> span);

internal interface IConverterMetadata
{
    Type GetGenericArgument();

    MethodInfo GetMethod(string name);
}
