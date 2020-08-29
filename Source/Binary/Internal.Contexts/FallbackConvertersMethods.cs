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
        };

        internal static IConverter GetConverter(Type type)
        {
            if (!Types.TryGetValue(type, out var converterType))
                return null;
            var converter = Activator.CreateInstance(converterType);
            return (IConverter)converter;
        }
    }
}
