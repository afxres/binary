using System;
using System.Collections.Generic;
using System.Net;

namespace Mikodev.Binary.Creators.Fallback
{
    internal sealed class FallbackConverterCreator : IConverterCreator
    {
        private static readonly IReadOnlyDictionary<Type, Type> dictionary = new Dictionary<Type, Type>
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

        public Converter GetConverter(IGeneratorContext context, Type type)
        {
            if (!dictionary.TryGetValue(type, out var converterType))
                return null;
            var converter = Activator.CreateInstance(converterType);
            return (Converter)converter;
        }
    }
}
