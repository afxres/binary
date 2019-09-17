using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Mikodev.Binary.Internal
{
    internal static class Define
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int GetItemCount(int byteCount, int definition)
        {
            Debug.Assert(byteCount > 0);
            Debug.Assert(definition > 0);
            var quotient = Math.DivRem(byteCount, definition, out var remainder);
            if (remainder != 0)
                ThrowHelper.ThrowNotEnoughBytes();
            return quotient;
        }

        internal static int GetConverterLength(params Converter[] values)
        {
            Debug.Assert(values.Any());
            Debug.Assert(values.All(x => x != null && x.Length >= 0));
            var length = values.All(x => x.Length > 0) ? values.Sum(x => (long)x.Length) : 0;
            Debug.Assert(length >= 0);
            return checked((int)(uint)length);
        }

        internal static MethodInfo GetToBytesMethodInfo(Type type, bool withMark)
        {
            var converterType = typeof(Converter<>).MakeGenericType(type);
            var types = new[] { typeof(Allocator).MakeByRefType(), type };
            var method = !withMark
                ? converterType.GetMethod(nameof(IConverter.ToBytes), types)
                : converterType.GetMethod(nameof(IConverter.ToBytesWithMark), types);
            Debug.Assert(method != null);
            return method;
        }

        internal static MethodInfo GetToValueMethodInfo(Type type, bool withMark)
        {
            var converterType = typeof(Converter<>).MakeGenericType(type);
            var types = new[] { typeof(ReadOnlySpan<byte>).MakeByRefType() };
            var method = !withMark
                ? converterType.GetMethod(nameof(IConverter.ToValue), types)
                : converterType.GetMethod(nameof(IConverter.ToValueWithMark), types);
            Debug.Assert(method != null);
            return method;
        }
    }
}
