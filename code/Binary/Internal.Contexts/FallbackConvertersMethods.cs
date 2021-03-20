using Mikodev.Binary.Converters;
using System;
using System.Collections.Generic;
using System.Net;

namespace Mikodev.Binary.Internal.Contexts
{
    internal static class FallbackConvertersMethods
    {
        private static readonly IReadOnlyDictionary<Type, Type> Types = new Dictionary<Type, Type>
        {
            [typeof(Uri)] = typeof(UriConverter),
            [typeof(string)] = typeof(StringConverter),
            [typeof(decimal)] = typeof(DecimalConverter),
            [typeof(DateTime)] = typeof(DateTimeConverter),
            [typeof(TimeSpan)] = typeof(TimeSpanConverter),
            [typeof(IPAddress)] = typeof(IPAddressConverter),
            [typeof(IPEndPoint)] = typeof(IPEndPointConverter),
            [typeof(DateTimeOffset)] = typeof(DateTimeOffsetConverter),
#if NET5_0_OR_GREATER
            [typeof(System.Text.Rune)] = typeof(RuneConverter),
#endif
        };

        internal static IConverter GetConverter(Type type)
        {
            if (Types.TryGetValue(type, out var converterType) is false)
                return null;
            var converter = Activator.CreateInstance(converterType);
            return (IConverter)converter;
        }
    }
}
